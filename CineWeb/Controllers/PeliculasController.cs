using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CineWeb.Data;
using CineWeb.Models;

namespace CineWeb.Controllers
{
    public class PeliculasController : Controller
    {
        private readonly AppDbContext _context;

        public PeliculasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Peliculas
        public async Task<IActionResult> Index()
        {
            var q = _context.Peliculas
                .Include(p => p.Genero)
                .Include(p => p.Director)
                .Include(p => p.Actores).ThenInclude(pa => pa.Actor);

            return View(await q.ToListAsync());
        }

        // GET: Peliculas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
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
            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nombre");
            ViewData["DirectorId"] = new SelectList(_context.Directores, "Id", "Nombre");
            ViewBag.ActorIds = new MultiSelectList(_context.Actores, "Id", "Nombre");
            return View();
        }

        // POST: Peliculas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Titulo,Sinopsis,Duracion,FechaEstreno,ImagenRuta,GeneroId,DirectorId")] Pelicula pelicula, int[] actorIds)
        {
            if (!ModelState.IsValid)
            {
                ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nombre", pelicula.GeneroId);
                ViewData["DirectorId"] = new SelectList(_context.Directores, "Id", "Nombre", pelicula.DirectorId);
                ViewBag.ActorIds = new MultiSelectList(_context.Actores, "Id", "Nombre", actorIds);
                return View(pelicula);
            }

            _context.Add(pelicula);
            await _context.SaveChangesAsync();

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
            if (id == null) return NotFound();

            var pelicula = await _context.Peliculas
                .Include(p => p.Actores)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pelicula == null) return NotFound();

            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nombre", pelicula.GeneroId);
            ViewData["DirectorId"] = new SelectList(_context.Directores, "Id", "Nombre", pelicula.DirectorId);

            var seleccionados = pelicula.Actores?.Select(a => a.ActorId).ToArray() ?? Array.Empty<int>();
            ViewBag.ActorIds = new MultiSelectList(_context.Actores, "Id", "Nombre", seleccionados);

            return View(pelicula);
        }

        // POST: Peliculas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Sinopsis,Duracion,FechaEstreno,ImagenRuta,GeneroId,DirectorId")] Pelicula pelicula, int[] actorIds)
        {
            if (id != pelicula.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nombre", pelicula.GeneroId);
                ViewData["DirectorId"] = new SelectList(_context.Directores, "Id", "Nombre", pelicula.DirectorId);
                ViewBag.ActorIds = new MultiSelectList(_context.Actores, "Id", "Nombre", actorIds);
                return View(pelicula);
            }

            try
            {
                _context.Update(pelicula);
                await _context.SaveChangesAsync();

                // Sincronizar relación M:N: eliminar actuales e insertar seleccionados
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
            var pelicula = await _context.Peliculas.FindAsync(id);
            if (pelicula != null)
            {
                // borro vínculos M:N primero para evitar residuos
                var enlaces = _context.PeliculasActores.Where(pa => pa.PeliculaId == id);
                _context.PeliculasActores.RemoveRange(enlaces);

                _context.Peliculas.Remove(pelicula);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PeliculaExists(int id)
        {
            return _context.Peliculas.Any(e => e.Id == id);
        }
    }
}
