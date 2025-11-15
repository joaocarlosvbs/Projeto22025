using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using Projeto22025.Models;
using System.ComponentModel.DataAnnotations; // <-- Adicionado

namespace Projeto22025.Models
{
    public class Movimentacao
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }

        // --- MUDANÇA AQUI ---
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Data da Movimentação")]
        public DateTime Data { get; set; }
        // --- FIM DA MUDANÇA ---

        [Required(ErrorMessage = "A quantidade é obrigatória.")]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que 0.")]
        public int Quantidade { get; set; }

        public string Tipo { get; set; } = string.Empty;

        [Display(Name = "Fornecedor")]
        public int? FornecedorId { get; set; }

        [Display(Name = "Setor")]
        public int? SetorId { get; set; }

        public string UsuarioId { get; set; } = string.Empty;

        // Propriedades de Navegação
        public Produto? Produto { get; set; }
        public Fornecedor? Fornecedor { get; set; }
        public Setor? Setor { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }
    }
}