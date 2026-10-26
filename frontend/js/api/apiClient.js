import { authStore } from '../auth/authStore.js';

const API_URL = '/api';

export async function fetchApi(endpoint, options = {}) {
    const headers = {
        'Content-Type': 'application/json',
        ...options.headers,
    };

    const token = authStore.getToken();
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }

    const config = {
        ...options,
        headers,
    };

    const response = await fetch(`${API_URL}${endpoint}`, config);

    if (response.status === 401 || response.status === 403) {
        authStore.logout();
        window.location.href = '/login.html';
        throw new Error('Sesión expirada o no autorizada');
    }

    if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.mensaje || 'Error en la petición');
    }
    if (response.status === 204) return null;
    return response.json();
}