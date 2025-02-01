using System.ComponentModel.DataAnnotations;

namespace SkladisteRobe.Models
{
    public class Materijal
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Obavezan naziv materijala")]
        public string Naziv { get; set; }

        [Required(ErrorMessage = "Obavezna količina")]
        public int Kolicina { get; set; }
    }
}