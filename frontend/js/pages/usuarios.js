import { fetchApi } from '../api/apiClient.js';
import { authStore } from '../auth/authStore.js';

if (!authStore.isAuthenticated()) window.location.href = '/login.html';
const user = authStore.getUser();
if(user.rol !== 'Administrador') window.location.href = '/mesas.html';

document.getElementById('userName').textContent = user.nombre;
document.getElementById('btnLogout').addEventListener('click', () => { authStore.logout(); window.location.href = '/login.html'; });

const tbody = document.getElementById('usuariosTbody');
const modal = document.getElementById('modalUsuario');
const form = document.getElementById('formUsuario');
const errorMsg = document.getElementById('errorMsg');

// Cargar usuarios
async function cargarUsuarios() {
    try {
        const usuarios = await fetchApi('/usuarios');
        tbody.innerHTML = usuarios.map(u => `
            <tr>
                <td>${u.nombre}</td>
                <td>${u.tipoDocumento} ${u.numeroDocumento}</td>
                <td>${u.correo}</td>
                <td>${u.rol}</td>
                <td><span class="badge ${u.activo ? 'activo' : 'inactivo'}">${u.activo ? 'Activo' : 'Inactivo'}</span></td>
                <td>
                    <button class="btn-sm ${u.activo ? 'btn-danger' : 'btn-primary'}" onclick="cambiarEstado(${u.idUsuario})">
                        ${u.activo ? 'Inactivar' : 'Activar'}
                    </button>
                </td>
            </tr>
        `).join('');
    } catch (error) {
        tbody.innerHTML = `<tr><td colspan="6" class="error">${error.message}</td></tr>`;
    }
}

// Abrir Modal
document.getElementById('btnNuevoUsuario').onclick = () => {
    form.reset();
    errorMsg.classList.add('hidden');
    modal.classList.remove('hidden');
};
document.getElementById('closeModal').onclick = () => modal.classList.add('hidden');

// Guardar Usuario
form.addEventListener('submit', async (e) => {
    e.preventDefault();
    const btn = form.querySelector('button[type="submit"]');
    btn.disabled = true;

    const payload = {
        nombre: document.getElementById('nombre').value,
        tipoDocumento: document.getElementById('tipoDocumento').value,
        numeroDocumento: document.getElementById('numeroDocumento').value,
        correo: document.getElementById('correo').value,
        contrasena: document.getElementById('contrasena').value,
        idRol: parseInt(document.getElementById('idRol').value)
    };

    try {
        await fetchApi('/usuarios', { method: 'POST', body: JSON.stringify(payload) });
        alert("Usuario creado exitosamente.");
        modal.classList.add('hidden');
        cargarUsuarios();
    } catch (error) {
        errorMsg.textContent = error.message;
        errorMsg.classList.remove('hidden');
    } finally {
        btn.disabled = false;
    }
});

// Cambiar Estado
window.cambiarEstado = async (id) => {
    if(!confirm("¿Cambiar el estado de este usuario?")) return;
    try {
        await fetchApi(`/usuarios/${id}/estado`, { method: 'PATCH' });
        cargarUsuarios();
    } catch (error) { alert(error.message); }
};

cargarUsuarios();