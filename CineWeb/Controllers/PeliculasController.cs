using CineWeb.Data;
using CineWeb.Models;
using CineWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CineWeb.Controllers
{
    public class PeliculasController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] _extPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly string[] _mimePermitidos = new[] { "image/jpeg", "image/png", "image/webp" };
        private const string CarpetaRelativa = "imagenes/peliculas"; // bajo wwwroot/

        public PeliculasController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Peliculas
        public async Task<IActionResult> Index(
            string? q,
            int[]? generos,
            int[]? actores,
            int[]? directores,
            int page = 1,
            int pageSize = 12)
        {
            var baseQuery = _context.Peliculas
                .Include(p => p.Genero)
                .Include(p => p.Director)
                .Include(p => p.Actores).ThenInclude(pa => pa.Actor)
                .AsQueryable();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Filtros
            if (!string.IsNullOrWhiteSpace(q))
            {
                var txt = q.Trim();
                baseQuery = baseQuery.Where(p => EF.Functions.Like(p.Titulo, $"%{txt}%"));
            }
            if (generos is { Length: > 0 })
                baseQuery = baseQuery.Where(p => generos.Contains(p.GeneroId));

            if (directores is { Length: > 0 })
                baseQuery = baseQuery.Where(p => directores.Contains(p.DirectorId));

            if (actores is { Length: > 0 })
                baseQuery = baseQuery.Where(p => p.Actores.Any(pa => actores.Contains(pa.ActorId)));

            // Orden simple (como en tu ejemplo, por Título asc)
            baseQuery = baseQuery.OrderBy(p => p.Titulo);

            // Conteo total
            var total = await baseQuery.CountAsync();

            // Normalizar paginación
            if (pageSize <= 0) pageSize = 12;
            var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            if (page <= 0) page = 1;
            if (page > totalPages) page = totalPages;

            // Página
            var peliculas = await baseQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Filtros VM (catálogos y seleccionados)
            var filtrosVM = new PeliculasFiltrosVM
            {
                Q = q,
                Generos = await _context.Generos.OrderBy(g => g.Nombre).ToListAsync(),
                Actores = await _context.Actores.OrderBy(a => a.Nombre).ToListAsync(),
                Directores = await _context.Directores.OrderBy(d => d.Nombre).ToListAsync(),

                SelGeneros = new HashSet<int>(generos ?? Array.Empty<int>()),
                SelActores = new HashSet<int>(actores ?? Array.Empty<int>()),
                SelDirectores = new HashSet<int>(directores ?? Array.Empty<int>()),

                Page = page,
                PageSize = pageSize,
                Total = total,
                TotalPages = totalPages,
                QueryBase = BuildQueryBase(q, generos, actores, directores, pageSize)
            };

            ViewBag.Filtros = filtrosVM;

            // También dejamos ViewData, por si prefieres el patrón de tu ejemplo
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentFilter"] = q ?? "";

            return View(peliculas);
        }

        // Arma ?q=...&generos=1&generos=2&actores=...&directores=...&pageSize=12 (sin el parámetro page)
        private static string BuildQueryBase(string? q, int[]? generos, int[]? actores, int[]? directores, int pageSize)
        {
            var sb = new StringBuilder();

            void Append(string key, string? value)
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                if (sb.Length > 0) sb.Append('&');
                sb.Append(key).Append('=').Append(Uri.EscapeDataString(value));
            }
            void AppendArr(string key, int[]? arr)
            {
                if (arr is not { Length: > 0 }) return;
                foreach (var v in arr)
                {
                    if (sb.Length > 0) sb.Append('&');
                    sb.Append(key).Append('=').Append(v);
                }
            }

            Append("q", q?.Trim());
            AppendArr("generos", generos);
            AppendArr("actores", actores);
            AppendArr("directores", directores);
            Append("pageSize", pageSize.ToString());

            return sb.ToString();
        }

        // Construye ?q=...&generos=1&generos=2&actores=5&directores=9&sort=...&pageSize=...
        private static string BuildQueryBase(string? q, int[]? generos, int[]? actores, int[]? directores, string sort, int pageSize)
        {
            var sb = new StringBuilder();

            void Append(string key, string? value)
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                if (sb.Length > 0) sb.Append('&');
                sb.Append(key).Append('=').Append(Uri.EscapeDataString(value));
            }
            void AppendArr(string key, int[]? arr)
            {
                if (arr is not { Length: > 0 }) return;
                foreach (var v in arr)
                {
                    if (sb.Length > 0) sb.Append('&');
                    sb.Append(key).Append('=').Append(v);
                }
            }

            Append("q", q?.Trim());
            AppendArr("generos", generos);
            AppendArr("actores", actores);
            AppendArr("directores", directores);
            Append("sort", string.IsNullOrWhiteSpace(sort) ? "titulo_asc" : sort);
            Append("pageSize", pageSize.ToString());

            return sb.ToString();
        }
    
    

        // GET: Peliculas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id == null) return NotFound();

            var pelicula = await _context.Peliculas
                .Include(p => p.Genero)
                .Include(p => p.Director)
                .Include(p => p.Actores).ThenInclude(pa => pa.Actor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pelicula == null) return NotFound();

            return View(pelicula);
        }

        // GET: Peliculas/Create
        public IActionResult Create()
        {
            CargarCombos();
            return View();
        }

        // POST: Peliculas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pelicula pelicula, int[] actorIds)
        {
            if (!ModelState.IsValid)
            {
                CargarCombos(pelicula.GeneroId, pelicula.DirectorId, actorIds);
                return View(pelicula);
            }

            // Guardar imagen si existe
            if (pelicula.ImagenArchivo != null && pelicula.ImagenArchivo.Length > 0)
            {
                var (ok, rutaRel, error) = await SaveImageAsync(pelicula.ImagenArchivo);
                if (!ok)
                {
                    ModelState.AddModelError(nameof(pelicula.ImagenArchivo), error ?? "Archivo inválido");
                    CargarCombos(pelicula.GeneroId, pelicula.DirectorId, actorIds);
                    return View(pelicula);
                }
                pelicula.ImagenRuta = "/" + rutaRel.Replace("\\", "/");
            }

            _context.Add(pelicula);
            await _context.SaveChangesAsync();

            // Guardar actores seleccionados
            if (actorIds != null && actorIds.Length > 0)
            {
                foreach (var aid in actorIds)
                    _context.PeliculasActores.Add(new PeliculaActor { PeliculaId = pelicula.Id, ActorId = aid });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Peliculas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id == null) return NotFound();

            var pelicula = await _context.Peliculas
                .Include(p => p.Actores)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pelicula == null) return NotFound();

            var seleccionados = pelicula.Actores?.Select(a => a.ActorId).ToArray() ?? Array.Empty<int>();
            CargarCombos(pelicula.GeneroId, pelicula.DirectorId, seleccionados);

            return View(pelicula);
        }

        // POST: Peliculas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Pelicula pelicula, int[] actorIds)
        {
            if (id != pelicula.Id) return NotFound();

            var peliculaDB = await _context.Peliculas.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (peliculaDB == null) return NotFound();

            if (!ModelState.IsValid)
            {
                CargarCombos(pelicula.GeneroId, pelicula.DirectorId, actorIds);
                return View(pelicula);
            }

            try
            {
                // Reemplazo de imagen si viene nueva
                if (pelicula.ImagenArchivo != null && pelicula.ImagenArchivo.Length > 0)
                {
                    var (ok, rutaRel, error) = await SaveImageAsync(pelicula.ImagenArchivo);
                    if (!ok)
                    {
                        ModelState.AddModelError(nameof(pelicula.ImagenArchivo), error ?? "Archivo inválido");
                        CargarCombos(pelicula.GeneroId, pelicula.DirectorId, actorIds);
                        return View(pelicula);
                    }

                    // eliminar imagen anterior si existía
                    if (!string.IsNullOrEmpty(peliculaDB.ImagenRuta))
                        DeleteImageIfExists(peliculaDB.ImagenRuta);

                    pelicula.ImagenRuta = "/" + rutaRel.Replace("\\", "/");
                }
                else
                {
                    // mantener ruta existente
                    pelicula.ImagenRuta = peliculaDB.ImagenRuta;
                }

                _context.Update(pelicula);
                await _context.SaveChangesAsync();

                // Sincronizar actores (remover todos y volver a insertar seleccionados)
                var actuales = _context.PeliculasActores.Where(pa => pa.PeliculaId == id);
                _context.PeliculasActores.RemoveRange(actuales);

                if (actorIds != null && actorIds.Length > 0)
                {
                    foreach (var aid in actorIds)
                        _context.PeliculasActores.Add(new PeliculaActor { PeliculaId = id, ActorId = aid });
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PeliculaExists(pelicula.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Peliculas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id == null) return NotFound();

            var pelicula = await _context.Peliculas
                .Include(p => p.Genero)
                .Include(p => p.Director)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pelicula == null) return NotFound();

            return View(pelicula);
        }

        // POST: Peliculas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var pelicula = await _context.Peliculas.FindAsync(id);
            if (pelicula != null)
            {
                // borrar vínculos
                var enlaces = _context.PeliculasActores.Where(pa => pa.PeliculaId == id);
                _context.PeliculasActores.RemoveRange(enlaces);

                // eliminar imagen física
                if (!string.IsNullOrEmpty(pelicula.ImagenRuta))
                    DeleteImageIfExists(pelicula.ImagenRuta);

                _context.Peliculas.Remove(pelicula);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ----------------- Helpers -----------------

        private void CargarCombos(int? generoId = null, int? directorId = null, int[]? actorIds = null)
        {
            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nombre", generoId);
            ViewData["DirectorId"] = new SelectList(_context.Directores, "Id", "Nombre", directorId);
            ViewBag.ActorIds = new MultiSelectList(_context.Actores, "Id", "Nombre", actorIds ?? Array.Empty<int>());
        }

        private bool PeliculaExists(int id) => _context.Peliculas.Any(e => e.Id == id);

        private async Task<(bool ok, string? rutaRelativa, string? error)> SaveImageAsync(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_extPermitidas.Contains(ext))
                return (false, null, $"Extensión no permitida. Usa: {string.Join(", ", _extPermitidas)}");

            // Checar MIME real
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(file.FileName, out var contentType))
                contentType = file.ContentType; // fallback

            if (string.IsNullOrEmpty(contentType) || !_mimePermitidos.Contains(contentType.ToLowerInvariant()))
                return (false, null, "Tipo de contenido no permitido.");

            // Crear carpeta si no existe
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var carpetaAbs = Path.Combine(webRoot, CarpetaRelativa);
            Directory.CreateDirectory(carpetaAbs);

            // Nombre único
            var nombre = $"{Guid.NewGuid():N}{ext}";
            var rutaAbs = Path.Combine(carpetaAbs, nombre);

            using (var stream = new FileStream(rutaAbs, FileMode.Create))
                await file.CopyToAsync(stream);

            var rutaRel = Path.Combine(CarpetaRelativa, nombre); // sin slash inicial
            return (true, rutaRel, null);
        }

        private void DeleteImageIfExists(string rutaPublica)
        {
            // rutaPublica ej: /imagenes/peliculas/abc.webp
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var relativa = rutaPublica.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
            var rutaAbs = Path.Combine(webRoot, relativa);

            if (System.IO.File.Exists(rutaAbs))
                System.IO.File.Delete(rutaAbs);
        }
    }
}
