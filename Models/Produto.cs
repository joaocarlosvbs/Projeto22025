using System.ComponentModel.DataAnnotations; // <-- Adicione este
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto22025.Models
{
    public class Produto
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        [Display(Name = "Descrição")] // <-- Nome amigável
        public string? Descricao { get; set; }

        [Display(Name = "Estoque Atual")] // <-- Nome amigável
        public int EstoqueAtual { get; set; }

        public string? ImagemUrl { get; set; }

        [Display(Name = "Categoria")] // <-- Nome amigável
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        [NotMapped]
        [Display(Name = "Imagem")] // <-- Nome amigável
        public IFormFile? ImagemUpload { get; set; }
    }
}