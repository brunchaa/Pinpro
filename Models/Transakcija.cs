using System;
using System.ComponentModel.DataAnnotations;

namespace SkladisteRobe.Models
{
    public class Transakcija
    {
        public int Id { get; set; }

        [Required]
        public int MaterijalId { get; set; }

        // Navigation property to Materijal
        public Materijal Materijal { get; set; }

        [Required(ErrorMessage = "Obavezna količina")]
        public int Kolicina { get; set; }

        [Required]
        public DateTime Datum { get; set; }

        // Should contain values such as "Ulaz" (receipt) or "Izlaz" (issuance)
        [Required]
        public string Tip { get; set; }

        [Required]
        public int KorisnikId { get; set; }

        // Navigation property to Korisnik (the user who performed the transaction)
        public Korisnik Korisnik { get; set; }
    }
}