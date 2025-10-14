using Microsoft.EntityFrameworkCore;
using CineWeb.Models;

namespace CineWeb.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Pelicula> Peliculas { get; set; }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<Director> Directores { get; set; }
        public DbSet<Actor> Actores { get; set; }
        public DbSet<PeliculaActor> PeliculasActores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<PeliculaActor>().HasKey(pa => new { pa.PeliculaId, pa.ActorId });
        }
    }
}
