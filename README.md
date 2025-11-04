# 🎬 CineWeb – Proyecto ASP.NET Core MVC

Aplicación web desarrollada en **ASP.NET Core MVC** con **Entity Framework Core**, que permite gestionar información de **películas**, **géneros**, **directores** y **actores**, con relaciones entre entidades y CRUD completo para cada modelo.

---

## 🚀 Tecnologías utilizadas
- **ASP.NET Core MVC (.NET 8 / .NET 7)**
- **Entity Framework Core**
- **SQL Server LocalDB**
- **Razor Views**
- **Bootstrap 5**
- **C# 11**
- **Visual Studio 2022**

---

## 🧩 Modelos implementados

| Modelo | Campos principales | Relaciones |
|--------|--------------------|-------------|
| **Pelicula** | Título, Sinopsis, Duración, FechaEstreno, ImagenRuta | FK a **Genero** y **Director**, relación M:N con **Actor** |
| **Genero** | Nombre, Descripción | 1:N con Películas |
| **Director** | Nombre, Nacionalidad, FechaNacimiento | 1:N con Películas |
| **Actor** | Nombre, Biografía, FechaNacimiento | M:N con Películas (a través de `PeliculaActor`) |

Relación adicional:
- Una **Película** puede tener **múltiples Actores**.
- Un **Actor** puede participar en **múltiples Películas**.

---

## 🧠 Funcionalidades

✅ CRUD completo para:
- Películas  
- Géneros  
- Directores  
- Actores  

✅ Asociación y navegación entre entidades:
- Ver todas las películas de un género.  
- Ver todas las películas dirigidas por un director.  
- Ver todas las películas donde participa un actor.  
- Desde el detalle de una película, navegar al género, director y actores vinculados.

✅ Soporte de imágenes (opcional):
- Las películas pueden incluir una imagen almacenada en `/wwwroot/images/`.

✅ Filtros y búsqueda avanzada:
- Búsqueda por título (q).
- Filtros por género, actor y director (checkboxes).
- Filtros aplicados sin recargar toda la página.
- Contadores de películas en cada módulo (géneros, actores, directores).

✅ Subida y visualización de imágenes:
- En la creación y edición de películas se puede cargar una imagen (IFormFile).
- Las imágenes se almacenan en /wwwroot/imagenes/.
- Si se reemplaza la imagen, la anterior se elimina automáticamente del servidor.

✅ Paginación
- Paginación funcional en la vista de películas (y extensible a otros módulos).

✅ Extensibilidad
Código preparado para futuras implementaciones como:
- Calificaciones y comentarios de usuarios.
- Autenticación y roles.
- Paginación global.

---

## ⚙️ Requisitos técnicos cumplidos
- Patrón **MVC** implementado.
- **Entity Framework Core** con migraciones (`Add-Migration` / `Update-Database`).
- **Inyección de dependencias** del contexto `AppDbContext`.
- **Base de datos LocalDB**.
- Rutas limpias y controladores organizados por entidad.
- **Barra de navegación** con acceso directo a todos los módulos.

---

## 🧰 Configuración y ejecución

### 1️⃣ Clonar el repositorio
```bash
git clone https://github.com/tuusuario/CineWeb.git
```

### 2️⃣ Abrir en Visual Studio
- Abrir el archivo de solución `.sln`
- Verificar el archivo `appsettings.json`:
  ```json
  {
    "ConnectionStrings": {
      "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CineWeb;Trusted_Connection=True;MultipleActiveResultSets=true"
    }
  }
  ```

### 3️⃣ Aplicar migraciones
En la **Consola del Administrador de Paquetes**:
```powershell
Add-Migration Inicial
Update-Database
```

Esto crea la base de datos `CineWeb` en tu LocalDB.

### 4️⃣ Ejecutar la aplicación
Presiona **Ctrl + F5** o ejecuta el proyecto.

La aplicación se abrirá en tu navegador:
```
http://localhost:xxxx/
```

---

## 🖼️ Estructura del proyecto

```
CineWeb/
│
├── Controllers/
│   ├── PeliculasController.cs
│   ├── GeneroesController.cs
│   ├── DirectoresController.cs
│   └── ActoresController.cs
│
├── Models/
│   ├── Pelicula.cs
│   ├── Genero.cs
│   ├── Director.cs
│   ├── Actor.cs
│   └── PeliculaActor.cs
│
├── Views/
│   ├── Peliculas/
│   ├── Generoes/
│   ├── Directores/
│   ├── Actores/
│   └── Shared/
│
├── Data/
│   └── AppDbContext.cs
│
├── wwwroot/
│   └── images/
│
└── appsettings.json
```

---

## 🧑‍💻 Autores
Proyecto desarrollado por **Danilo Torres Vera**  

---

## 🏁 Estado del proyecto
**Versión:** 1.0  
**Entrega:** CRUD + Navegación + Base de datos inicializada  
**Pendiente:** Validación y subida de imágenes opcional (implementación parcial lista)

---
