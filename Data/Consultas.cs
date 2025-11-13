using Microsoft.EntityFrameworkCore;
using Projeto22025.Models;

namespace Projeto22025.Data
{
    // Nossa classe de lógica de consulta
    public class Consultas
    {
        private readonly ApplicationDbContext _context;

        public Consultas(ApplicationDbContext context)
        {
            _context = context;
        }

        // Deixaremos em branco por enquanto
        // Aqui entrarão os métodos, ex: public async Task<List<Relatorio>> GetConsumoPorSetorAsync()
    }
}