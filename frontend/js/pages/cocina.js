import { fetchApi } from '../api/apiClient.js';
import { authStore } from '../auth/authStore.js';

if (!authStore.isAuthenticated()) { window.location.href = '/login.html'; }

const user = authStore.getUser();
document.getElementById('userName').textContent = `${user.nombre} (${user.rol})`;
document.getElementById('btnLogout').addEventListener('click', () => { authStore.logout(); window.location.href = '/login.html'; });

// Variables de estado
let pedidos = [];
let connection;

const contenedores = {
    'EnviadoACocina': document.getElementById('col-enviado'),
    'EnPreparacion': document.getElementById('col-preparacion'),
    'ListoParaServir': document.getElementById('col-listo')
};

// 1. Cargar pedidos iniciales
async function cargarPedidos() {
    try {
        pedidos = await fetchApi('/cocina/pedidos');
        renderizarTablero();
    } catch (error) {
        console.error("Error al cargar pedidos", error);
    }
}

// 2. Configurar SignalR (Tiempo Real)
async function conectarSignalR() {
    // El token debe ir en la URL o headers para hubs autenticados
    const token = authStore.getToken();
    connection = new signalR.HubConnectionBuilder()
        .withUrl(`/hubs/pedidos?access_token=${token}`) // ASP.NET Core lee el token de aquí
        .withAutomaticReconnect()
        .build();

    // Escuchar eventos del backend
    connection.on("NuevoPedidoRecibido", (idPedido) => {
        console.log("¡Nuevo pedido!", idPedido);
        cargarPedidos(); // Recargar la lista completa (solución rápida)
    });

    connection.on("PedidoActualizado", (idPedido, nuevoEstado) => {
        console.log("Pedido actualizado", idPedido, nuevoEstado);
        cargarPedidos(); 
    });

    try {
        await connection.start();
        console.log("Conectado a SignalR");
        // Unirse al grupo de cocina
        await connection.invoke("UnirseAGrupo", "Cocina");
    } catch (err) {
        console.error("Error de conexión SignalR: ", err);
        setTimeout(conectarSignalR, 5000); // Reintentar
    }
}

// 3. Renderizar Interfaz
function renderizarTablero() {
    // Limpiar columnas
    Object.values(contenedores).forEach(c => c.innerHTML = '');

    pedidos.forEach(pedido => {
        const card = document.createElement('div');
        const estadoClass = pedido.estado === 'EnPreparacion' ? 'en-preparacion' : 
                            pedido.estado === 'ListoParaServir' ? 'listo' : '';
        
        card.className = `pedido-card ${estadoClass}`;
        
        let itemsHtml = pedido.detalles.map(d => `
            <div class="pedido-item">
                <strong>${d.cantidad}x</strong> ${d.producto}
                ${d.observaciones ? `<div class="pedido-obs">Nota: ${d.observaciones}</div>` : ''}
            </div>
        `).join('');

        let btnHtml = '';
        if (pedido.estado === 'EnviadoACocina') {
            btnHtml = `<button class="btn-estado btn-preparar" onclick="cambiarEstado(${pedido.idPedido})">Iniciar Preparación</button>`;
        } else if (pedido.estado === 'EnPreparacion') {
            btnHtml = `<button class="btn-estado btn-listo" onclick="cambiarEstado(${pedido.idPedido})">Marcar Listo</button>`;
        }

        card.innerHTML = `
            <div class="pedido-header">
                <span>Mesa ${pedido.numeroMesa}</span>
                <span>#${pedido.idPedido}</span>
            </div>
            <div class="pedido-items">${itemsHtml}</div>
            ${btnHtml}
        `;

        if (contenedores[pedido.estado]) {
            contenedores[pedido.estado].appendChild(card);
        }
    });
}

// 4. Cambiar Estado (HU-08)
window.cambiarEstado = async (id) => {
    try {
        await fetchApi(`/cocina/${id}/estado`, { method: 'PATCH' });
        // La actualización visual llegará por SignalR, no hace falta recargar manualmente
    } catch (error) {
        alert("Error: " + error.message);
    }
};

// Inicializar
cargarPedidos();
conectarSignalR();