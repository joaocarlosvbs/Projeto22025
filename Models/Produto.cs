using Projeto22025.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Para o [NotMapped]

public class Produto
{
    [Required]
    public int Id { get; set; }

    [Required] 
    public string ? Nome { get; set; }
    [Required]
    public string ? Descricao { get; set; }
    [Required]
    public int EstoqueAtual { get; set; }

    // Relacionamento
    public int CategoriaId { get; set; }
    public Categoria ? Categoria { get; set; }

    public string? ImagemUrl { get; set; } // O '?' permite ser nulo


    [NotMapped] // Não salvar este campo no banco
    [Display(Name = "Imagem do Produto")]
    public IFormFile? ImagemUpload { get; set; }
}