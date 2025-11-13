// Topo do arquivo
using Microsoft.AspNetCore.Identity; // Para IdentityRole
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Projeto22025.Models; // <-- IMPORTANTE

namespace Projeto22025.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<Setor> Setores { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Movimentacao> Movimentacoes { get; set; }

        // Isso cria as "Roles" (Funções) de usuário no banco
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "a1-role-id", Name = "Almoxarife", NormalizedName = "ALMOXARIFE" },
                new IdentityRole { Id = "s1-role-id", Name = "Servidor", NormalizedName = "SERVIDOR" }
            );
        }
    }
}