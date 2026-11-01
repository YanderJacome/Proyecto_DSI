import { fetchApi } from '../api/apiClient.js';
import { authStore } from '../auth/authStore.js';

if (!authStore.isAuthenticated()) window.location.href = '/login.html';

const user = authStore.getUser();
if(user) {
    if (user.rol !== 'Administrador') {
        alert("No tienes permisos para ver esta página.");
        window.location.href = '/login.html';
    }
    document.getElementById('userName').textContent = `${user.nombre} (${user.rol})`;
}

document.getElementById('btnLogout').addEventListener('click', () => {
    authStore.logout();
    window.location.href = '/login.html';
});

const fechaInput = document.getElementById('fechaReporte');
const kpiContainer = document.getElementById('kpiContainer');
const metodosPagoDiv = document.getElementById('metodosPago');

// Poner la fecha de hoy por defecto (formato YYYY-MM-DD)
const hoy = new Date().toISOString().split('T')[0];
fechaInput.value = hoy;

async function cargarReporte(fecha) {
    try {
        kpiContainer.innerHTML = '<p>Cargando...</p>';
        
        const data = await fetchApi(`/reportes/ventas-diarias?fecha=${fecha}`);
        
        kpiContainer.innerHTML = `
            <div class="kpi-card ventas">
                <h3>Total Ventas</h3>
                <div class="valor">$${data.totalVentas.toLocaleString('es-CO')}</div>
            </div>
            <div class="kpi-card">
                <h3>Pedidos Facturados</h3>
                <div class="valor">${data.cantidadPedidos}</div>
            </div>
            <div class="kpi-card ticket">
                <h3>Ticket Promedio</h3>
                <div class="valor">$${data.ticketPromedio.toLocaleString('es-CO')}</div>
            </div>
        `;

        metodosPagoDiv.innerHTML = `
            <div class="metodo-item">
                <h4>Efectivo</h4>
                <div class="valor-metodo">$${data.ventasPorMetodo.efectivo.toLocaleString('es-CO')}</div>
            </div>
            <div class="metodo-item">
                <h4>Tarjeta</h4>
                <div class="valor-metodo">$${data.ventasPorMetodo.tarjeta.toLocaleString('es-CO')}</div>
            </div>
            <div class="metodo-item">
                <h4>Transferencia</h4>
                <div class="valor-metodo">$${data.ventasPorMetodo.transferencia.toLocaleString('es-CO')}</div>
            </div>
        `;

    } catch (error) {
        kpiContainer.innerHTML = `<p class="error">Error al cargar reporte: ${error.message}</p>`;
    }
}

document.getElementById('btnConsultar').addEventListener('click', () => {
    cargarReporte(fechaInput.value);
});

// Carga inicial
cargarReporte(hoy);
document.getElementById('btnExportar').addEventListener('click', () => {
    // Oculta los botones de la pantalla temporalmente para que no salgan en el PDF
    const botones = document.querySelectorAll('button, input, .nav-link');
    botones.forEach(b => b.style.display = 'none');
    
    // Abre el diálogo de impresión (donde el usuario elige "Guardar como PDF")
    window.print();
    
    // Restaura los botones después de imprimir
    setTimeout(() => {
        botones.forEach(b => b.style.display = '');
    }, 1000);
});