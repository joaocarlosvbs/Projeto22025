using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Projeto22025.Models; // <-- Garante que ele enxergue TODOS os models

namespace Projeto22025.Data
{
    // 1. Mudamos para IdentityDbContext<ApplicationUser>
    public class ApplicationDbContext : IdentityDbContext<Usuario>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // 2. Nossos DbSets
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<Setor> Setores { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Movimentacao> Movimentacoes { get; set; } // <-- Agora vai enxergar

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 3. Seed Roles
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "a1-role-id", Name = "Almoxarife", NormalizedName = "ALMOXARIFE" },
                new IdentityRole { Id = "s1-role-id", Name = "Servidor", NormalizedName = "SERVIDOR" }
            );

            // 4. CPF Único (vinculado ao ApplicationUser)
            builder.Entity<Usuario>()
                .HasIndex(u => u.CPF)
                .IsUnique();
        }
    }
}