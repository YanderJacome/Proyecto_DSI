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

// Exportar a PDF usando el CSS de impresión
document.getElementById('btnExportar').addEventListener('click', () => {
    window.print();
});

// --- LÓGICA MENSUAL (HU-10) ---
const mesSelect = document.getElementById('mesSelect');
const anioSelect = document.getElementById('anioSelect');
const kpiMensual = document.getElementById('kpiMensual');
const fechaActual = new Date();

// 1. Llenar los selectores de Mes y Año
const meses = ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"];
meses.forEach((m, i) => {
    const opt = document.createElement('option');
    opt.value = i + 1;
    opt.textContent = m;
    if (i === fechaActual.getMonth()) opt.selected = true; // Selecciona el mes actual por defecto
    mesSelect.appendChild(opt);
});

// Llenar años (del actual hacia atrás)
for (let i = fechaActual.getFullYear(); i >= 2024; i--) {
    const opt = document.createElement('option');
    opt.value = i;
    opt.textContent = i;
    anioSelect.appendChild(opt);
}

// 2. Función para consultar el backend
async function cargarReporteMensual() {
    const mes = mesSelect.value;
    const anio = anioSelect.value;
    
    try {
        kpiMensual.innerHTML = '<p>Cargando...</p>';
        
        // Hacemos la petición al endpoint que acabamos de crear en el backend
        const data = await fetchApi(`/reportes/ventas-mensuales?mes=${mes}&anio=${anio}`);
        
        // Lógica de colores para la variación
        const colorVariacion = data.variacionPorcentual >= 0 ? '#27ae60' : '#e74c3c';
        const iconoVariacion = data.variacionPorcentual >= 0 ? '↑' : '↓';

        kpiMensual.innerHTML = `
            <div class="kpi-card">
                <h3>Total Ingresos (${data.periodo})</h3>
                <div class="valor">$${data.totalMes.toLocaleString('es-CO')}</div>
            </div>
            <div class="kpi-card">
                <h3>Mes Anterior</h3>
                <div class="valor" style="color: #7f8c8d;">$${data.totalMesAnterior.toLocaleString('es-CO')}</div>
            </div>
            <div class="kpi-card" style="border-top-color: ${colorVariacion};">
                <h3>Variación vs Anterior</h3>
                <div class="valor" style="color: ${colorVariacion};">
                    ${iconoVariacion} ${Math.abs(data.variacionPorcentual)}%
                </div>
            </div>
        `;
    } catch (error) {
        kpiMensual.innerHTML = `<p class="error">Error al cargar resumen: ${error.message}</p>`;
    }
}

// 3. Escuchar cuando el usuario cambie de mes o año
mesSelect.addEventListener('change', cargarReporteMensual);
anioSelect.addEventListener('change', cargarReporteMensual);

// 4. Hacer la carga inicial apenas se abra la página
cargarReporteMensual();