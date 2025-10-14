using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CineWeb.Models
{
    public class Pelicula
    {
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; }
        public string? Sinopsis { get; set; }
        public int Duracion { get; set; }

        [Display(Name = "Fecha de Estreno")]
        [DataType(DataType.Date)]
        public DateTime? FechaEstreno { get; set; }

        [Display(Name = "Imagen")]
        public string? ImagenRuta { get; set; }
        [NotMapped]
        public IFormFile? ImagenArchivo { get; set; }

        // FKs
        [Display(Name = "Género")]
        public int GeneroId { get; set; }
        [ForeignKey("GeneroId")]
        [ValidateNever]
        public Genero? Genero { get; set; }

        [Display(Name = "Director")]
        public int DirectorId { get; set; }
        [ForeignKey("DirectorId")]
        [ValidateNever]
        public Director? Director { get; set; }

        [ValidateNever]
        public ICollection<PeliculaActor>? Actores { get; set; }
    }
}
