// ViewModels/PeliculasFiltrosVM.cs
using System.Collections.Generic;
using CineWeb.Models;

namespace CineWeb.ViewModels
{
    public class PeliculasFiltrosVM
    {
        public string? Q { get; set; }

        public List<Genero> Generos { get; set; } = new();
        public List<Actor> Actores { get; set; } = new();
        public List<Director> Directores { get; set; } = new();

        public HashSet<int> SelGeneros { get; set; } = new();
        public HashSet<int> SelActores { get; set; } = new();
        public HashSet<int> SelDirectores { get; set; } = new();

        // Paginación
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int Total { get; set; }
        public int TotalPages { get; set; }

        // Para armar los links de paginación preservando filtros
        public string QueryBase { get; set; } = "";
    }
}
