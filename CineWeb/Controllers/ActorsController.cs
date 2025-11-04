using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CineWeb.Data;
using CineWeb.Models;

namespace CineWeb.Controllers
{
    public class ActorsController : Controller
    {
        private readonly AppDbContext _context;

        public ActorsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Actors
        // ?q=texto  -> busca por nombre
        public async Task<IActionResult> Index(string q)
        {
            var query = _context.Actores.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(a => a.Nombre.Contains(q));

            // Diccionario { ActorId -> #Películas } desde la tabla intermedia
            var counts = await _context.Set<PeliculaActor>()
                .AsNoTracking()
                .GroupBy(pa => pa.ActorId)
                .Select(g => new { ActorId = g.Key, Cnt = g.Count() })
                .ToDictionaryAsync(x => x.ActorId, x => x.Cnt);

            ViewBag.PeliculasPorActor = counts;

            var lista = await query
                .OrderBy(a => a.Nombre)
                .ToListAsync();

            ViewData["q"] = q;
            return View(lista);
        }


        // GET: Actors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actores
                .AsNoTracking()
                .Include(a => a.Peliculas)
                    .ThenInclude(pa => pa.Pelicula)
                        .ThenInclude(p => p.Director)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (actor == null) return NotFound();

            return View(actor);
        }

        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Biografia,FechaNacimiento")] Actor actor)
        {
            if (!ModelState.IsValid) return View(actor);

            _context.Add(actor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Actors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actores.FindAsync(id);
            if (actor == null) return NotFound();

            return View(actor);
        }

        // POST: Actors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Biografia,FechaNacimiento")] Actor actor)
        {
            if (id != actor.Id) return NotFound();
            if (!ModelState.IsValid) return View(actor);

            try
            {
                _context.Update(actor);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ActorExists(actor.Id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Actors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actores
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (actor == null) return NotFound();

            return View(actor);
        }

        // POST: Actors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actores.FindAsync(id);
            if (actor != null)
            {
                _context.Actores.Remove(actor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actores.Any(e => e.Id == id);
        }
    }
}
