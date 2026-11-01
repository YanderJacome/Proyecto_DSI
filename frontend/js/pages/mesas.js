import { fetchApi } from '../api/apiClient.js';
import { authStore } from '../auth/authStore.js';

if (!authStore.isAuthenticated()) {
    window.location.href = '/login.html';
}

const user = authStore.getUser();
if (user) {
    document.getElementById('userName').textContent = `${user.nombre} (${user.rol})`;
    
    if (user.rol !== 'Administrador' && user.rol !== 'Cajero') {
        document.getElementById('navFacturacion').style.display = 'none';
    }
    if (user.rol !== 'Administrador') {
        document.getElementById('navReportes').style.display = 'none';
        document.getElementById('navUsuarios').style.display = 'none'; // <-- NUEVA LÍNEA
    }
}
const mesasGrid = document.getElementById('mesasGrid');

async function cargarMesas() {
    try {
        const mesas = await fetchApi('/mesas');
        renderizarMesas(mesas);
    } catch (error) {
        mesasGrid.innerHTML = `<p class="error">Error al cargar mesas: ${error.message}</p>`;
    }
}

function renderizarMesas(mesas) {
    mesasGrid.innerHTML = '';
    
    if (mesas.length === 0) {
        mesasGrid.innerHTML = '<p>No hay mesas registradas.</p>';
        return;
    }

    mesas.forEach(mesa => {
        const estadoLower = mesa.estado.toLowerCase();
        
        const card = document.createElement('div');
        card.className = `mesa-card ${estadoLower}`;
        card.innerHTML = `
            <div class="mesa-numero">${mesa.numeroMesa}</div>
            <div class="mesa-estado">${mesa.estado}</div>
        `;
        
                card.addEventListener('click', async () => {
            if (mesa.estado === 'Disponible') {
                window.location.href = `/pedidos.html?mesaId=${mesa.idMesa}`;
            } else if (mesa.estado === 'Ocupada') {
                await abrirModalMesa(mesa);
            } else {
                alert(`La mesa ${mesa.numeroMesa} está ${mesa.estado}.`);
            }
        });
        mesasGrid.appendChild(card);
    });
}

cargarMesas();
// --- LÓGICA DEL MODAL ---
const modal = document.getElementById('modalMesa');
const closeModal = document.getElementById('closeModal');
const modalTitulo = document.getElementById('modalTitulo');
const modalPedidosList = document.getElementById('modalPedidosList');

closeModal.onclick = () => modal.classList.add('hidden');
window.onclick = (event) => { if (event.target === modal) modal.classList.add('hidden'); };

async function abrirModalMesa(mesa) {
    modal.classList.remove('hidden');
    modalTitulo.textContent = `Mesa ${mesa.numeroMesa} - Pedidos Activos`;
    modalPedidosList.innerHTML = '<p>Cargando pedidos...</p>';

    try {
        const pedidos = await fetchApi(`/pedidos/mesa/${mesa.idMesa}`);
        
        if (pedidos.length === 0) {
            modalPedidosList.innerHTML = '<p>No hay pedidos activos.</p>';
            return;
        }

                modalPedidosList.innerHTML = pedidos.map(p => `
            <div class="modal-pedido-card">
                <h4>Pedido #${p.idPedido} - Estado: ${p.estado}</h4>
                <ul>
                    ${p.detalles.map(d => `<li>${d.cantidad}x ${d.nombre}</li>`).join('')}
                </ul>
                <p><strong>Total: $${p.total.toLocaleString('es-CO')}</strong></p>
                ${p.estado !== 'Facturado' ? `
                    <div style="display: flex; gap: 10px; margin-top: 10px;">
                        <button class="btn-cancelar-pedido" style="background-color: #3498db; margin-top:0;" onclick="window.location.href='/pedidos.html?mesaId=${p.idMesa}&pedidoId=${p.idPedido}'">Modificar Pedido</button>
                        <button class="btn-cancelar-pedido" style="margin-top:0;" onclick="cancelarPedido(${p.idPedido})">Cancelar Pedido</button>
                    </div>
                ` : ''}
            </div>
        `).join('');

    } catch (error) {
        modalPedidosList.innerHTML = `<p class="error">Error: ${error.message}</p>`;
    }
}

window.cancelarPedido = async (idPedido) => {
    if (!confirm("¿Está seguro de cancelar este pedido? La acción no se puede deshacer.")) return;

    try {
        await fetchApi(`/pedidos/${idPedido}/cancelar`, { method: 'POST' });
        alert("Pedido cancelado. La mesa puede haber sido liberada.");
        modal.classList.add('hidden');
        cargarMesas(); // Recargar el mapa de mesas
    } catch (error) {
        alert("Error al cancelar: " + error.message);
    }
};