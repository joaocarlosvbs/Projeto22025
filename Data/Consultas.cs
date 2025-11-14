// Em Data/Consultas.cs
using Microsoft.EntityFrameworkCore;
using Projeto22025.Models;

namespace Projeto22025.Data
{
    public class Consultas
    {
        private readonly ApplicationDbContext _context;

        public Consultas(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- ADICIONE ESTE MÉTODO ---
        public async Task<List<Relatorio>> GetConsumoPorSetorAsync()
        {
            return await _context.Movimentacoes
                .Where(m => m.Tipo == "Saída" && m.SetorId != null)
                .GroupBy(m => m.Setor.Nome) // Agrupa por Nome do Setor
                .Select(g => new Relatorio // Usa seu model "Relatorio.cs"
                {
                    Grupo = g.Key, // O "Grupo" é o Nome do Setor
                    Total = g.Sum(m => m.Quantidade * -1) // O "Total" é a soma (invertida)
                })
                .OrderByDescending(r => r.Total)
                .ToListAsync();
        }
    }
}