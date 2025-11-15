using System.ComponentModel.DataAnnotations;

namespace Projeto22025.Models
{
    public class Fornecedor
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Razão Social")]
        public string Razaosocial { get; set; } = string.Empty;

        // A MUDANÇA IMPORTANTE É O 'string?' (permitir nulo)
        [Display(Name = "CNPJ")]
        public string? Cnpj { get; set; }
    }
}