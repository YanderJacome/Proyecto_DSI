import { fetchApi } from '../api/apiClient.js';
import { authStore } from '../auth/authStore.js';

if (!authStore.isAuthenticated()) window.location.href = '/login.html';

const urlParams = new URLSearchParams(window.location.search);
const mesaId = parseInt(urlParams.get('mesaId'));
const pedidoId = parseInt(urlParams.get('pedidoId')); // NUEVO

if (!mesaId) {
    alert('No se seleccionó una mesa válida.');
    window.location.href = '/mesas.html';
}

document.getElementById('mesaInfo').textContent = `Mesa N° ${mesaId} ${pedidoId ? '- Modificando Pedido #' + pedidoId : ''}`;
if(pedidoId) {
    document.querySelector('h1').textContent = 'Modificar Pedido';
    document.getElementById('btnConfirmarPedido').textContent = 'Guardar Cambios';
}

let carrito = [];
let productosMenu = [];

const productosGrid = document.getElementById('productosGrid');
const categoriasTabs = document.getElementById('categoriasTabs');
const carritoItemsDiv = document.getElementById('carritoItems');
const totalPedidoH2 = document.getElementById('totalPedido');
const btnConfirmar = document.getElementById('btnConfirmarPedido');
const errorMsg = document.getElementById('mensajeError');

document.getElementById('btnVolver').addEventListener('click', () => window.location.href = '/mesas.html');
document.getElementById('btnLogout').addEventListener('click', () => { authStore.logout(); window.location.href = '/login.html'; });

async function init() {
    try {
        productosMenu = await fetchApi('/productos');
        renderizarCategorias();
        renderizarProductos('Todos');

        // Si es modificación, cargar el pedido existente en el carrito
        if (pedidoId) {
            const pedido = await fetchApi(`/pedidos/${pedidoId}`);
            pedido.detalles.forEach(d => {
                const prod = productosMenu.find(p => p.idProducto === d.idProducto);
                if(prod) {
                    carrito.push({
                        idProducto: d.idProducto,
                        nombre: prod.nombre,
                        precio: d.precioUnitario,
                        cantidad: d.cantidad,
                        observaciones: d.observaciones || ""
                    });
                }
            });
            actualizarCarritoUI();
        }
    } catch (error) {
        productosGrid.innerHTML = `<p class="error">Error al cargar el menú: ${error.message}</p>`;
    }
}

// ... (Las funciones renderizarCategorias, renderizarProductos, agregarAlCarrito, actualizarCarritoUI y modificarCantidad quedan EXACTAMENTE IGUAL) ...
// --- RENDERIZADO DEL MENÚ ---

function renderizarCategorias() {
    // Obtener categorías únicas
    const categorias = ['Todos', ...new Set(productosMenu.map(p => p.categoria))];
    
    categoriasTabs.innerHTML = '';
    categorias.forEach((cat, index) => {
        const btn = document.createElement('button');
        btn.className = `tab-btn ${index === 0 ? 'active' : ''}`;
        btn.textContent = cat;
        btn.onclick = () => {
            document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            renderizarProductos(cat);
        };
        categoriasTabs.appendChild(btn);
    });
}

function renderizarProductos(categoriaFiltro) {
    productosGrid.innerHTML = '';
    const filtrados = categoriaFiltro === 'Todos' 
        ? productosMenu 
        : productosMenu.filter(p => p.categoria === categoriaFiltro);

    filtrados.forEach(prod => {
        const card = document.createElement('div');
        card.className = 'producto-card';
        card.innerHTML = `
            <div>
                <div class="prod-nombre">${prod.nombre}</div>
                <div class="prod-precio">$${prod.precio.toLocaleString('es-CO')}</div>
            </div>
            <button class="btn-agregar">+ Agregar</button>
        `;
        card.querySelector('.btn-agregar').onclick = () => agregarAlCarrito(prod);
        productosGrid.appendChild(card);
    });
}

// --- LÓGICA DEL CARRITO ---

function agregarAlCarrito(producto) {
    const itemExistente = carrito.find(i => i.idProducto === producto.idProducto);
    
    if (itemExistente) {
        itemExistente.cantidad++;
    } else {
        carrito.push({
            idProducto: producto.idProducto,
            nombre: producto.nombre,
            precio: producto.precio,
            cantidad: 1,
            observaciones: ""
        });
    }
    actualizarCarritoUI();
}

function actualizarCarritoUI() {
    carritoItemsDiv.innerHTML = '';
    
    if (carrito.length === 0) {
        carritoItemsDiv.innerHTML = '<p class="empty-cart">No hay productos agregados.</p>';
        totalPedidoH2.textContent = '$0';
        btnConfirmar.disabled = true;
        return;
    }

    let total = 0;

    carrito.forEach((item, index) => {
        total += (item.precio * item.cantidad);

        const div = document.createElement('div');
        div.className = 'carrito-item';
        div.innerHTML = `
            <div class="carrito-item-header">
                <span>${item.nombre}</span>
                <span>$${(item.precio * item.cantidad).toLocaleString('es-CO')}</span>
            </div>
            <div class="carrito-item-actions">
                <button class="btn-qty" data-action="restar">-</button>
                <span>${item.cantidad}</span>
                <button class="btn-qty" data-action="sumar">+</button>
                <button class="btn-remove">Eliminar</button>
            </div>
            <input type="text" class="obs-input" placeholder="Observaciones (ej. sin cebolla)" value="${item.observaciones}">
        `;

        // Eventos de cantidad
        div.querySelector('[data-action="restar"]').onclick = () => modificarCantidad(index, -1);
        div.querySelector('[data-action="sumar"]').onclick = () => modificarCantidad(index, 1);
        
        // Evento de eliminar
        div.querySelector('.btn-remove').onclick = () => {
            carrito.splice(index, 1);
            actualizarCarritoUI();
        };

        // Evento de observaciones
        div.querySelector('.obs-input').onchange = (e) => {
            carrito[index].observaciones = e.target.value;
        };

        carritoItemsDiv.appendChild(div);
    });

    totalPedidoH2.textContent = `$${total.toLocaleString('es-CO')}`;
    btnConfirmar.disabled = false;
}

function modificarCantidad(index, delta) {
    carrito[index].cantidad += delta;
    if (carrito[index].cantidad <= 0) {
        carrito.splice(index, 1);
    }
    actualizarCarritoUI();
}

// --- CONFIRMAR PEDIDO ---

btnConfirmar.addEventListener('click', async () => {
    if (carrito.length === 0) return;

    btnConfirmar.disabled = true;
    btnConfirmar.textContent = 'Procesando...';
    errorMsg.classList.add('hidden');

    const payload = {
        items: carrito.map(i => ({
            idProducto: i.idProducto,
            cantidad: i.cantidad,
            observaciones: i.observaciones || null
        }))
    };

    try {
        if (pedidoId) {
            // Es una modificación (PUT)
            const response = await fetchApi(`/pedidos/${pedidoId}`, {
                method: 'PUT',
                body: JSON.stringify(payload)
            });
            alert(`¡Pedido modificado! Nuevo total: $${response.total.toLocaleString('es-CO')}`);
        } else {
            // Es un pedido nuevo (POST)
            payload.idMesa = mesaId;
            const response = await fetchApi('/pedidos', {
                method: 'POST',
                body: JSON.stringify(payload)
            });
            alert(`¡Pedido #${response.idPedido} registrado! Total: $${response.total.toLocaleString('es-CO')}`);
        }
        window.location.href = '/mesas.html';

    } catch (error) {
        errorMsg.textContent = error.message;
        errorMsg.classList.remove('hidden');
        btnConfirmar.disabled = false;
        btnConfirmar.textContent = pedidoId ? 'Guardar Cambios' : 'Confirmar Pedido';
    }
});

init();