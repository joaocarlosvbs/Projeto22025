using System.ComponentModel.DataAnnotations;

namespace Projeto22025.Models
{
    public class Fornecedor
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string ? Razaosocial { get; set; }
        [Required]
        public string ? Cnpj { get; set; }
    }
}
