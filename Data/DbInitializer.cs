using Bogus;
using Microsoft.AspNetCore.Identity;
using Projeto22025.Models;
using Microsoft.EntityFrameworkCore;
using System; // Para o Math.Round

namespace Projeto22025.Data
{
    public static class DbInitializer
    {
        public static async Task SeedDatabase(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                await context.Database.MigrateAsync();

                if (await context.Produtos.AnyAsync())
                {
                    return;
                }

                if (!await roleManager.RoleExistsAsync("Almoxarife"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Almoxarife"));
                }
                if (!await roleManager.RoleExistsAsync("Servidor"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Servidor"));
                }

                var fornecedor = new Fornecedor { Razaosocial = "Fornecedor Principal", Cnpj = "11222333000144" };
                await context.Fornecedores.AddAsync(fornecedor);

                var setorFaker = new Faker<Setor>("pt_BR")
                    .RuleFor(s => s.Nome, f => f.Commerce.Department(1))
                    .RuleFor(s => s.Responsavel, f => f.Name.FullName());
                var setores = setorFaker.Generate(10);
                await context.Setores.AddRangeAsync(setores);

                var categoriaFaker = new Faker<Categoria>("pt_BR")
                    .RuleFor(c => c.Nome, f => f.Commerce.Categories(1)[0]);
                var categorias = categoriaFaker.Generate(5);
                await context.Categorias.AddRangeAsync(categorias);

                await context.SaveChangesAsync();

                var almoxarife = await userManager.FindByNameAsync("almoxarife");
                if (almoxarife == null)
                {
                    almoxarife = new Usuario
                    {
                        UserName = "almoxarife",
                        Email = "almoxarife@estoque.com",
                        CPF = "12345678901",
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(almoxarife, "Password123!");
                    await userManager.AddToRoleAsync(almoxarife, "Almoxarife");
                }

                var produtoFaker = new Faker<Produto>("pt_BR")
                    .RuleFor(p => p.Nome, f => f.Commerce.ProductName())
                    .RuleFor(p => p.Descricao, f => f.Commerce.ProductDescription())
                    .RuleFor(p => p.EstoqueAtual, f => f.Random.Number(100, 500))

                    .RuleFor(p => p.Preco, f => Math.Round(f.Random.Decimal(1.00m, 1000.00m), 2))

                    .RuleFor(p => p.CategoriaId, f => f.PickRandom(categorias).Id);

                var produtos = produtoFaker.Generate(100);
                await context.Produtos.AddRangeAsync(produtos);
                await context.SaveChangesAsync();

                var movimentacaoFaker = new Faker<Movimentacao>("pt_BR")
                    .RuleFor(m => m.ProdutoId, f => f.PickRandom(produtos).Id)
                    .RuleFor(m => m.Data, f => f.Date.Past(1))
                    .RuleFor(m => m.Tipo, f => f.PickRandom(new[] { "Entrada", "Saída" }))
                    .RuleFor(m => m.Quantidade, f => f.Random.Number(1, 50))
                    .RuleFor(m => m.UsuarioId, almoxarife.Id)
                    .RuleFor(m => m.SetorId, (f, m) => m.Tipo == "Saída" ? f.PickRandom(setores).Id : (int?)null)
                    .RuleFor(m => m.FornecedorId, (f, m) => m.Tipo == "Entrada" ? fornecedor.Id : (int?)null);

                var movimentacoes = movimentacaoFaker.Generate(500);
                await context.Movimentacoes.AddRangeAsync(movimentacoes);

                await context.SaveChangesAsync();
            }
        }
    }
}