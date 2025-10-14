using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CineWeb.Models
{
    public class Actor
    {
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        public string? Biografia { get; set; }
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }

        [ValidateNever]
        public ICollection<PeliculaActor>? Peliculas { get; set; }
    }
}
