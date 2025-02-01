using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SkladisteRobe.Models
{
    public class BulkTransactionItemViewModel
    {
        [Required(ErrorMessage = "Naziv je obavezan")]
        // Regular expression explanation:
        // ^               : start of string
        // (?=.*\p{L})     : at least one Unicode letter must be present
        // [\p{L}\p{N}\s,.\-]+ : one or more of any Unicode letter, number, whitespace, comma, period, or dash
        // $               : end of string
        [RegularExpression(@"^(?=.*\p{L})[\p{L}\p{N}\s,.\-]+$",
            ErrorMessage = "Naziv mora sadržavati barem jedno slovo i može sadržavati samo slova, brojeve, razmake, zareze, točke i crtice.")]
        public string Naziv { get; set; }

        [Required(ErrorMessage = "Količina je obavezna")]
        [Range(1, int.MaxValue, ErrorMessage = "Količina mora biti veća od 0")]
        public int Kolicina { get; set; }
    }

    public class BulkTransactionViewModel
    {
        public List<BulkTransactionItemViewModel> Items { get; set; } = new List<BulkTransactionItemViewModel>();
    }
}