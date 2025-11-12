using Microsoft.AspNetCore.Identity;
using Projeto22025.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Movimentacao
{
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public Produto ? Produto { get; set; }
    public DateTime Data { get; set; }
    public int Quantidade { get; set; } // Ex: +100 (Entrada), -5 (Saída)
    public string ? Tipo { get; set; } // "Entrada" ou "Saída"

    // Relacionamentos para saber de onde veio e para onde foi
    public int? FornecedorId { get; set; } // Nulo se for Saída
    public Fornecedor? Fornecedor { get; set; }

    public int? SetorId { get; set; } // Nulo se for Entrada
    public Setor? Setor { get; set; }

    // Quem autorizou a movimentação (via Identity)
    [Required] 
    public string ? UsuarioId { get; set; }
    [ForeignKey("UsuarioId")] 
    public IdentityUser ? Usuario { get; set; }
}