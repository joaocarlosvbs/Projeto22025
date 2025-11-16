using Projeto22025.Models;

namespace Projeto22025.Models
{
    public class Pivot
    {
        public string GrupoEixo1 { get; set; } = string.Empty; // Ex: Nome do Setor
        public string GrupoEixo2 { get; set; } = string.Empty; // Ex: Nome da Categoria
        public int Total { get; set; }
    }
}
