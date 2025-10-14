using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CineWeb.Models
{
    public class Genero
    {
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }

        [ValidateNever]
        public ICollection<Pelicula>? Peliculas { get; set; }
    }
}
