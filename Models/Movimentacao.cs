using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using Projeto22025.Models; // <-- Adicionado

namespace Projeto22025.Models
{
    public class Movimentacao
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public DateTime Data { get; set; }
        public int Quantidade { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public int? FornecedorId { get; set; }
        public int? SetorId { get; set; }
        public string UsuarioId { get; set; } = string.Empty;

        // Propriedades de Navegação
        public Produto? Produto { get; set; }
        public Fornecedor? Fornecedor { get; set; }
        public Setor? Setor { get; set; }

        [ForeignKey("UsuarioId")]
        // 4. MUDANÇA PRINCIPAL AQUI
        public Usuario? Usuario { get; set; } // <-- Aponta para seu usuário customizado
    }
}