using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CineWeb.Models
{
    public class Director
    {
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        public string? Nacionalidad { get; set; }
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }

        [ValidateNever]
        public ICollection<Pelicula>? Peliculas { get; set; }
    }
}
