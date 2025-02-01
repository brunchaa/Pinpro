using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SkladisteRobe.Models
{
    public class BulkTransactionItemViewModel
    {
        [Required(ErrorMessage = "Naziv je obavezan")]
        // This regex ensures the name contains at least one letter.
        [RegularExpression(@"^(?=.*\p{L})[\p{L}\p{N}\s,.\-]+$",
            ErrorMessage = "Naziv mora sadržavati barem jedno slovo i može sadržavati samo slova, brojeve, razmake, zareze, točke i crtice.")]
        public string Naziv { get; set; }

        [Required(ErrorMessage = "Količina je obavezna")]
        [Range(1, int.MaxValue, ErrorMessage = "Količina mora biti veća od 0")]
        public int Kolicina { get; set; }

        [Required(ErrorMessage = "Mjerna jedinica je obavezna")]
        public MjernaJedinica Jedinica { get; set; }
    }

    public class BulkTransactionViewModel
    {
        public List<BulkTransactionItemViewModel> Items { get; set; } = new List<BulkTransactionItemViewModel>();
    }
}