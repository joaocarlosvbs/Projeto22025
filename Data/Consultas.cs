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

        // --- RELATÓRIO 1: Consumo por Setor ---
        public async Task<List<Relatorio>> GetConsumoPorSetorAsync()
        {
            return await _context.Movimentacoes
                .Where(m => m.Tipo == "Saída" && m.SetorId != null && m.Setor != null)
                .GroupBy(m => m.Setor.Nome)
                .Select(g => new Relatorio
                {
                    Grupo = g.Key,
                    Total = g.Sum(m => m.Quantidade) // O 'Sum' (int) cabe no 'double'
                })
                .OrderByDescending(r => r.Total)
                .ToListAsync();
        }

        // --- RELATÓRIO 2: Pivot Setor/Categoria ---
        public async Task<List<Pivot>> GetPivotSetorCategoriaAsync()
        {
            return await _context.Movimentacoes
                .Where(m => m.Tipo == "Saída" && m.SetorId != null && m.Produto != null && m.Produto.Categoria != null)
                .Include(m => m.Setor)
                .Include(m => m.Produto.Categoria)
                .GroupBy(m => new {
                    SetorNome = m.Setor.Nome,
                    CategoriaNome = m.Produto.Categoria.Nome
                })
                .Select(g => new Pivot
                {
                    GrupoEixo1 = g.Key.SetorNome,
                    GrupoEixo2 = g.Key.CategoriaNome,
                    Total = g.Sum(m => m.Quantidade)
                })
                .OrderBy(r => r.GrupoEixo1)
                .ThenBy(r => r.GrupoEixo2)
                .ToListAsync();
        }

        // --- RELATÓRIO 3 (NOVO): Média de Saída por Produto ---
        public async Task<List<Relatorio>> GetMediaSaidaProdutoAsync()
        {
            return await _context.Movimentacoes
                .Where(m => m.Tipo == "Saída" && m.Produto != null)
                .GroupBy(m => m.Produto.Nome) // Agrupa por Produto
                .Select(g => new Relatorio // <-- REUTILIZA o "pacote" Relatorio.cs
                {
                    Grupo = g.Key,
                    // Calcula a Média (Average), que é um 'double'
                    Total = g.Average(m => m.Quantidade)
                })
                .OrderByDescending(r => r.Total)
                .ToListAsync();
        }

        // --- RELATÓRIO 4 (NOVO): Extrato por Data ---
        // Não usa "pacote". Retorna a lista do Model original.
        public async Task<List<Movimentacao>> GetMovimentacoesPorDataAsync(DateTime dataInicio, DateTime dataFim)
        {
            // Adiciona 1 dia ao dataFim para incluir o dia inteiro
            dataFim = dataFim.AddDays(1);

            return await _context.Movimentacoes
                .Where(m => m.Data >= dataInicio && m.Data < dataFim)
                .Include(m => m.Produto)
                .Include(m => m.Setor)
                .Include(m => m.Fornecedor)
                .Include(m => m.Usuario)
                .OrderByDescending(m => m.Data)
                .ToListAsync();
        }
    }
}