import { fetchApi } from '../api/apiClient.js';
import { authStore } from '../auth/authStore.js';


if (!authStore.isAuthenticated()) window.location.href = '/login.html';

const user = authStore.getUser();
if(user) {
    document.getElementById('userName').textContent = `${user.nombre} (${user.rol})`;
    
    // Ocultar opciones según el rol
    if (user.rol !== 'Administrador') {
        document.getElementById('navReportes').style.display = 'none'; // <-- FALTABA ESTA LÍNEA
        document.getElementById('navUsuarios').style.display = 'none';
    }
}

document.getElementById('btnLogout').addEventListener('click', () => {
    authStore.logout();
    window.location.href = '/login.html';
});

let pedidoSeleccionado = null;

async function cargarPedidos() {
    try {
        const pedidos = await fetchApi('/facturas/pendientes');
        renderizarPedidos(pedidos);
    } catch (error) {
        document.getElementById('pedidosPendientes').innerHTML = `<p class="error">${error.message}</p>`;
    }
}

function renderizarPedidos(pedidos) {
    const container = document.getElementById('pedidosPendientes');
    container.innerHTML = '';

    if (pedidos.length === 0) {
        container.innerHTML = '<p style="color: #7f8c8d; font-size: 1.1rem;">No hay pedidos pendientes por cobrar en este momento.</p>';
        return;
    }

    pedidos.forEach(p => {
        const card = document.createElement('div');
        card.className = 'pedido-card';
        card.innerHTML = `
            <h3>
                Mesa ${p.numeroMesa}
                <span class="badge-estado">${p.estado}</span>
            </h3>
            <div class="info-row">
                <span>Pedido #${p.idPedido}</span>
            </div>
            <div class="info-row" style="margin-top: 15px;">
                <span>Total:</span>
                <strong>$${p.total.toLocaleString('es-CO')}</strong>
            </div>
        `;
        card.onclick = () => seleccionarPedido(p);
        container.appendChild(card);
    });
}

function seleccionarPedido(pedido) {
    pedidoSeleccionado = pedido;
    document.getElementById('panelCobro').classList.remove('hidden');
    document.getElementById('cobrarIdPedido').textContent = `#${pedido.idPedido}`;
    document.getElementById('cobrarTotal').textContent = `$${pedido.total.toLocaleString('es-CO')}`;

    const detalleDiv = document.getElementById('detalleCobro');
    detalleDiv.innerHTML = pedido.detalles.map(d => `
        <div class="detalle-item">
            <span>${d.cantidad}x ${d.nombre}</span>
            <span>$${d.subtotal.toLocaleString('es-CO')}</span>
        </div>
    `).join('');
}

document.getElementById('btnFacturar').addEventListener('click', async () => {
    if (!pedidoSeleccionado) return;

    const btn = document.getElementById('btnFacturar');
    btn.disabled = true;
    btn.textContent = 'Procesando...';

    const metodoPago = parseInt(document.getElementById('metodoPago').value);

    try {
        await fetchApi('/facturas', {
            method: 'POST',
            body: JSON.stringify({
                idPedido: pedidoSeleccionado.idPedido,
                metodoPago: metodoPago
            })
        });

        alert("Factura generada y mesa liberada exitosamente.");
        document.getElementById('panelCobro').classList.add('hidden');
        pedidoSeleccionado = null;
        cargarPedidos(); // Recargar la lista
        
    } catch (error) {
        alert("Error: " + error.message);
    } finally {
        btn.disabled = false;
        btn.textContent = 'Generar Factura y Liberar Mesa';
    }
});

cargarPedidos();