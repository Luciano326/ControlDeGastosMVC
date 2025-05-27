# 💸 Sistema de Control de Gastos

Aplicación web desarrollada con ASP.NET Core MVC y SQL Server para registrar, consultar y gestionar gastos personales. Cuenta con soporte multiusuario, control de roles, recuperación de contraseña, filtros avanzados y generación de reportes.

---

## 🚀 Funcionalidades principales

- Registro de gastos con descripción, categoría, fecha y monto
- Filtro por mes, año, detalle o combinación de estos
- Generación de reportes en PDF y Excel
- Autenticación con recuperación de contraseña por correo electrónico
- Gestión de usuarios: roles "Administrador" y "Usuario"
- Interfaz intuitiva basada en Bootstrap

---

## 🛠️ Tecnologías utilizadas

- ASP.NET Core 8 MVC
- SQL Server
- Entity Framework Core
- Bootstrap 5
- Razor
- iTextSharp (PDF)
- ClosedXML (Excel)
- MailKit (correo electrónico)

---

## 📸 Capturas

### 🔐 Login
![login](https://github.com/user-attachments/assets/2e34ac52-74d2-4161-a2a6-c4cb2c94299c)  
Formulario de acceso con opciones para registro y recuperación de clave

### 📝 Registro de usuario
![register](https://github.com/user-attachments/assets/503622ff-0205-457c-887c-6689afca83f1)  
Alta de nuevos usuarios

### 🏠 Inicio
![home](https://github.com/user-attachments/assets/2a9492fe-cd8e-4648-9613-0466c54782c3)  
Panel principal con resumen de actividades

### 💰 Gestión de gastos
![gastos](https://github.com/user-attachments/assets/4f6a7624-af1e-4711-9a1e-21cb0ae1c80d)  
CRUD completo con filtros dinámicos

### 📈 Estadísticas
![estadisticas](https://github.com/user-attachments/assets/96cbc057-16ed-46cc-a313-4a8ced64f1d0)  
Gráficos y análisis mensual

### 👤 Perfil de usuario
![perfil](https://github.com/user-attachments/assets/ac1de840-43e3-480d-a28a-35880e4618fb)  
Edición de datos personales y cambio de contraseña

### 📊 Reporte PDF mensual
![reportepdf](https://github.com/user-attachments/assets/2adf4ee0-b311-4752-acb3-90b066fc4093)  
Exportación de reportes en PDF con filtros aplicados

### 👥 Gestión de usuarios (admin)
![control](https://github.com/user-attachments/assets/d691cfe5-c397-4f3c-820a-a5181dfcdb06)  
Asignación de roles y control de accesos

---

## 🧠 Estructura del proyecto


Solución "ControlDeGastosMVC"

│

ControlDeGastosMVC.API/

│

├── Context/

├── Controllers/

├── Interfaces/

├── Models/

├── Services/

├── ViewsModels/

├── Views/

├── wwwroot/

├── .gitignore

├── appsettings.json

└── Program.cs


---

## 📦 Cómo ejecutar

1. Clonar el repositorio
2. Configurar la cadena de conexión en `appsettings.json`
3. Ejecutar las migraciones de base de datos (EF Core)
4. Ejecutar el proyecto desde Visual Studio

---

## 📩 Contacto

**Luciano Raspo**  
📍 San Cristóbal, Santa Fe, Argentina  
📧 [lucianoraspo04@outlook.com]  
🌐 [Tu portfolio web]  
🔗 [LinkedIn](https://www.linkedin.com/in/lucianoraspo)
