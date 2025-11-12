using System.ComponentModel.DataAnnotations;

namespace Projeto22025.Models
{
    public class Setor
    {
        [Required]
        public int Id { get; set; }
        [Required] 
        public string ? Nome { get; set; }
        [Required]
        public string ? Responsavel { get; set; }
        
    }
}
