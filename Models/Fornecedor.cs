using System.ComponentModel.DataAnnotations;

namespace Projeto22025.Models
{
    public class Fornecedor
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Razão Social")]
        public string Razaosocial { get; set; } = string.Empty;

        [Required]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "O CNPJ deve ter 14 dígitos.")]
        [Display(Name = "CNPJ")]
        public string Cnpj { get; set; } = string.Empty;
    }
}