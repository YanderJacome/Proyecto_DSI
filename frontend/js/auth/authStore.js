const TOKEN_KEY = 'restaurante_token';
const USER_KEY = 'restaurante_user';

export const authStore = {
    saveSession: (token, user) => {
        sessionStorage.setItem(TOKEN_KEY, token);
        sessionStorage.setItem(USER_KEY, JSON.stringify(user));
    },
    getToken: () => sessionStorage.getItem(TOKEN_KEY),
    getUser: () => {
        const user = sessionStorage.getItem(USER_KEY);
        return user ? JSON.parse(user) : null;
    },
    logout: () => {
        sessionStorage.removeItem(TOKEN_KEY);
        sessionStorage.removeItem(USER_KEY);
    },
    isAuthenticated: () => !!sessionStorage.getItem(TOKEN_KEY)
};