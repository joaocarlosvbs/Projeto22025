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
                    Total = g.Sum(m => m.Quantidade)
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

        // --- RELATÓRIO 3: Média de Saída por Produto ---
        public async Task<List<Relatorio>> GetMediaSaidaProdutoAsync()
        {
            return await _context.Movimentacoes
                .Where(m => m.Tipo == "Saída" && m.Produto != null)
                .GroupBy(m => m.Produto.Nome)
                .Select(g => new Relatorio
                {
                    Grupo = g.Key,
                    Total = g.Average(m => m.Quantidade)
                })
                .OrderByDescending(r => r.Total)
                .ToListAsync();
        }

        // --- RELATÓRIO 4: Extrato por Data ---
        public async Task<List<Movimentacao>> GetMovimentacoesPorDataAsync(DateTime dataInicio, DateTime dataFim)
        {
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

        // --- RELATÓRIO 5: Estoque total por categoria ---
        public async Task<List<Relatorio>> GetEstoqueValoradoAsync()
        {
            var produtos = await _context.Produtos
                .Where(p => p.EstoqueAtual > 0)
                .Include(p => p.Categoria)
                .AsNoTracking()
                .ToListAsync();

            var relatorio = produtos
                .Where(p => p.Categoria != null)
                .GroupBy(p => p.Categoria.Nome)
                .Select(g => new Relatorio
                {
                    Grupo = g.Key,
                    Total = (double)g.Sum(p => p.EstoqueAtual * p.Preco)
                })
                .OrderByDescending(r => r.Total)
                .ToList();

            return relatorio;
        }
    }
}