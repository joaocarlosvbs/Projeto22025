using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto22025.Models
{
    public class Produto
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        [Display(Name = "Estoque Atual")]
        public int EstoqueAtual { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório.")]
        [Display(Name = "Preço Unitário R$")]
        [Column(TypeName = "decimal(18,2)")]
        [DataType(DataType.Currency)]
        public decimal Preco { get; set; }

        public string? ImagemUrl { get; set; }

        [Display(Name = "Categoria")]
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        [NotMapped]
        [Display(Name = "Imagem")]
        public IFormFile? ImagemUpload { get; set; }
    }
}