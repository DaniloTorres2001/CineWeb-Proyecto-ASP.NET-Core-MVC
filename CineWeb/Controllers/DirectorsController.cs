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
    public class DirectorsController : Controller
    {
        private readonly AppDbContext _context;

        public DirectorsController(AppDbContext context)
        {
            _context = context;
        }
        // /Directors/Peliculas/5?q=matrix
        public async Task<IActionResult> Peliculas(int id, string? q)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var director = await _context.Directores
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);
            if (director == null) return NotFound();

            IQueryable<Pelicula> pelis = _context.Peliculas
                .AsNoTracking()
                .Where(p => p.DirectorId == id);

            if (!string.IsNullOrWhiteSpace(q))
            {
                pelis = pelis.Where(p => p.Titulo.Contains(q));
            }
            pelis = pelis.Include(p => p.Genero);

            var lista = await pelis
                .OrderByDescending(p => p.FechaEstreno)
                .ToListAsync();

            ViewBag.DirectorNombre = director.Nombre;
            ViewBag.DirectorId = director.Id;
            ViewData["q"] = q;

            return View(lista);
        }



        // GET: Directors
        public async Task<IActionResult> Index(string q)
        {
            var query = _context.Directores.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(d => d.Nombre.Contains(q));

            var counts = await _context.Peliculas
                .AsNoTracking()
                .GroupBy(p => p.DirectorId)
                .Select(g => new { DirectorId = g.Key, Cnt = g.Count() })
                .ToDictionaryAsync(x => x.DirectorId, x => x.Cnt);

            ViewBag.PeliculasPorDirector = counts;
            ViewData["q"] = q;

            var lista = await query.OrderBy(d => d.Nombre).ToListAsync();
            return View(lista);
        }

        // GET: Directors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id == null) return NotFound();

            var director = await _context.Directores
                .AsNoTracking()
                .Include(d => d.Peliculas) // colección de Películas del director
                .FirstOrDefaultAsync(m => m.Id == id);

            if (director == null) return NotFound();

            return View(director);
        }


        // GET: Directors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Directors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Nacionalidad,FechaNacimiento")] Director director)
        {
            if (ModelState.IsValid)
            {
                _context.Add(director);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(director);
        }

        // GET: Directors/Edit/5
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

            var director = await _context.Directores.FindAsync(id);
            if (director == null)
            {
                return NotFound();
            }
            return View(director);
        }

        // POST: Directors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Nacionalidad,FechaNacimiento")] Director director)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != director.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(director);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DirectorExists(director.Id))
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
            return View(director);
        }

        // GET: Directors/Delete/5
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

            var director = await _context.Directores
                .FirstOrDefaultAsync(m => m.Id == id);
            if (director == null)
            {
                return NotFound();
            }

            return View(director);
        }

        // POST: Directors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var director = await _context.Directores.FindAsync(id);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (director != null)
            {
                _context.Directores.Remove(director);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DirectorExists(int id)
        {
            return _context.Directores.Any(e => e.Id == id);
        }
    }
}
