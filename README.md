# Sistema de Gestión Integral para Restaurante

Proyecto desarrollado para el curso de **Diseño de Sistemas de Información** del **Instituto Tecnológico Metropolitano (ITM)**.

##  Integrantes
*   Yander Samir Jacome Barbosa
*   Jose Miguel Meneses Munera
*   Santiago Rodríguez Muñoz

##  Descripción
Aplicación web full-stack para la gestión de un restaurante. Permite administrar mesas, registrar pedidos, gestionar la preparación en cocina en tiempo real (SignalR), facturar y generar reportes de ventas.

##  Tecnologías Utilizadas
*   **Backend:** C# / ASP.NET Core Web API (.NET 10), Entity Framework Core, SignalR, JWT.
*   **Frontend:** HTML5, CSS3, JavaScript Vanilla (ES Modules).
*   **Base de Datos:** Microsoft SQL Server.

## 🏗️ Patrones de Diseño y Arquitectura
*   **State:** Gestión del ciclo de vida de los pedidos.
*   **Facade:** Coordinación del registro y modificación de pedidos.
*   **Observer:** Notificaciones en tiempo real hacia la cocina (vía SignalR).
*   **SOLID:** Código estructurado bajo los principios de responsabilidad única, inversión de dependencias, etc.

##  Requisitos Previos
*   [.NET SDK 10.0](https://dotnet.microsoft.com/download) (o superior).
*   [SQL Server](https://www.microsoft.com/es-es/sql-server/sql-server-downloads) (LocalDB o Express).
*   Visual Studio Code o Visual Studio 2022.

## ⚙️ Instalación y Ejecución
1. Clonar el repositorio.
2. Abrir una terminal en la carpeta `backend/Restaurante.Api`.
3. Ejecutar las migraciones de la base de datos:
   ```bash
   dotnet ef database update
   ```
  4.  Iniciar el servidor backend (esto también sirve el frontend):
```bash
    dotnet run
```
    
  5.  Abrir en el navegador: `http://localhost:5267/login.html` (o el puerto que indique la consola).


## Credenciales de Prueba
| Rol | Correo | Contraseña |
|---|---|---|
| Administrador | admin@restaurante.com | Admin123! |
| Mesero | mesero@restaurante.com | Mesero123! |
| Cajero | cajero@restaurante.com | Cajero123! |
| Cocina | cocina@restaurante.com | Cocina123! |