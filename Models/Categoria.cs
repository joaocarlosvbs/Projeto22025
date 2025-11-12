using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Projeto22025.Models
{
    public class Categoria
    {
        [Required]
        public int Id { get; set; }
        [Required] 
        public string ? Nome { get; set; }
    }
}
