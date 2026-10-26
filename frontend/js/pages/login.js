import { authStore } from '../auth/authStore.js';

const form = document.getElementById('loginForm');
const errorMsg = document.getElementById('errorMessage');
const btnLogin = document.getElementById('btnLogin');

if (authStore.isAuthenticated()) {
    window.location.href = '/mesas.html';
}

form.addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const correo = document.getElementById('correo').value;
    const contrasena = document.getElementById('contrasena').value;

    btnLogin.disabled = true;
    btnLogin.textContent = 'Ingresando...';
    errorMsg.classList.add('hidden');

    try {
        const response = await fetch('/api/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ correo, contrasena })
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.mensaje || 'Error al iniciar sesión');
        }

        authStore.saveSession(data.token, { nombre: data.nombre, rol: data.rol });
        
        if (data.rol === 'Cocina') window.location.href = '/cocina.html';
        else if (data.rol === 'Cajero') window.location.href = '/facturacion.html';
        else window.location.href = '/mesas.html';

    } catch (error) {
        errorMsg.textContent = error.message;
        errorMsg.classList.remove('hidden');
    } finally {
        btnLogin.disabled = false;
        btnLogin.textContent = 'Ingresar';
    }
});