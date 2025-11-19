using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CineWeb.Data;
using CineWeb.Models;

namespace CineWeb.Controllers
{
    public class GeneroesController : Controller
    {
        private readonly AppDbContext _context;

        public GeneroesController(AppDbContext context)
        {
            _context = context;
        }
        // GET: Generoes      
        public async Task<IActionResult> Index(string q)
        {
            var query = _context.Generos.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(g => g.Nombre.Contains(q));

            var counts = await _context.Peliculas
                .AsNoTracking()
                .GroupBy(p => p.GeneroId)
                .Select(g => new { GeneroId = g.Key, Cnt = g.Count() })
                .ToDictionaryAsync(x => x.GeneroId, x => x.Cnt);

            ViewBag.PeliculasPorGenero = counts;

            var lista = await query
                .OrderBy(g => g.Nombre)
                .ToListAsync();

            return View(lista);
        }


        // GET: Generoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id == null)
            {
                return NotFound();
            }

            var genero = await _context.Generos
                .Include(g => g.Peliculas) // Incluir las películas relacionadas
                    .ThenInclude(p => p.Director) // Incluir el director de cada película
                .FirstOrDefaultAsync(m => m.Id == id);
            if (genero == null)
            {
                return NotFound();
            }

            return View(genero);
        }

        public async Task<IActionResult> Peliculas(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var genero = await _context.Generos.FindAsync(id);
            if (genero == null) return NotFound();

            var pelis = await _context.Peliculas
                .Where(p => p.GeneroId == id)
                .Include(p => p.Director)
                .ToListAsync();

            ViewBag.GeneroNombre = genero.Nombre;
            ViewBag.GeneroId = genero.Id;
            return View(pelis);
        }

        // GET: Generoes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Generoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Descripcion")] Genero genero)
        {
            if (ModelState.IsValid)
            {
                _context.Add(genero);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(genero);
        }

        // GET: Generoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id == null)
            {
                return NotFound();
            }

            var genero = await _context.Generos.FindAsync(id);
            if (genero == null)
            {
                return NotFound();
            }
            return View(genero);
        }

        // POST: Generoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion")] Genero genero)
        {

            if (id != genero.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(genero);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GeneroExists(genero.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(genero);
        }

        // GET: Generoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id == null)
            {
                return NotFound();
            }

            var genero = await _context.Generos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (genero == null)
            {
                return NotFound();
            }

            return View(genero);
        }

        // POST: Generoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var genero = await _context.Generos.FindAsync(id);
            if (genero != null)
            {
                _context.Generos.Remove(genero);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GeneroExists(int id)
        {
            return _context.Generos.Any(e => e.Id == id);
        }
    }
}
