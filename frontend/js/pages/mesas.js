import { fetchApi } from '../api/apiClient.js';
import { authStore } from '../auth/authStore.js';

if (!authStore.isAuthenticated()) {
    window.location.href = '/login.html';
}

const user = authStore.getUser();
if (user) {
    document.getElementById('userName').textContent = `${user.nombre} (${user.rol})`;
    
    // Control de visibilidad de menús según el rol
    if (user.rol !== 'Administrador' && user.rol !== 'Cajero') {
        document.getElementById('navFacturacion').style.display = 'none';
    }
    if (user.rol !== 'Administrador') {
        document.getElementById('navReportes').style.display = 'none';
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
        
        card.addEventListener('click', () => {
            if (mesa.estado === 'Disponible') {
                window.location.href = `/pedidos.html?mesaId=${mesa.idMesa}`;
            } else {
                alert(`La mesa ${mesa.numeroMesa} está ${mesa.estado}.`);
            }
        });

        mesasGrid.appendChild(card);
    });
}

cargarMesas();