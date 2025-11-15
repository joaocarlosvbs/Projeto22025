using Bogus; // A biblioteca de dados falsos
using Microsoft.AspNetCore.Identity;
using Projeto22025.Models;
using Microsoft.EntityFrameworkCore; // Para o .Any()

namespace Projeto22025.Data
{
    public static class DbInitializer
    {
        // Este é o método principal que será chamado pelo Program.cs
        public static async Task SeedDatabase(IApplicationBuilder app)
        {
            // Usamos um 'scope' para pegar os serviços (DbContext, UserManager)
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Garante que o banco de dados e as migrações estejam aplicados
                await context.Database.MigrateAsync();

                // --- PASSO 1: A "TRAVA" ---
                // Verifica se já existem produtos. Se sim, não faz NADA.
                if (context.Produtos.Any())
                {
                    return; // O banco já foi populado
                }

                // --- PASSO 2: CRIAR ROLES (se não existirem) ---
                if (!await roleManager.RoleExistsAsync("Almoxarife"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Almoxarife"));
                }
                if (!await roleManager.RoleExistsAsync("Servidor"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Servidor"));
                }

                // --- PASSO 3: CRIAR DADOS BÁSICOS (FKs) ---

                // Criar 1 Fornecedor (para Entradas)
                var fornecedor = new Fornecedor { Razaosocial = "Fornecedor Principal" };
                await context.Fornecedores.AddAsync(fornecedor);

                // Criar 10 Setores (para Saídas)
                var setorFaker = new Faker<Setor>("pt_BR")
                    .RuleFor(s => s.Nome, f => f.Commerce.Department(1))
                    .RuleFor(s => s.Responsavel, f => f.Name.FullName());
                var setores = setorFaker.Generate(10);
                await context.Setores.AddRangeAsync(setores);

                // Criar 5 Categorias
                var categoriaFaker = new Faker<Categoria>("pt_BR")
                    .RuleFor(c => c.Nome, f => f.Commerce.Categories(1)[0]);
                var categorias = categoriaFaker.Generate(5);
                await context.Categorias.AddRangeAsync(categorias);

                // Salva os dados básicos para que eles peguem IDs
                await context.SaveChangesAsync();

                // --- PASSO 4: CRIAR USUÁRIO E PRODUTOS ---

                // Criar 1 Usuário "Almoxarife" (para ser o dono das movimentações)
                var almoxarife = await userManager.FindByNameAsync("almoxarife");
                if (almoxarife == null)
                {
                    almoxarife = new Usuario
                    {
                        UserName = "almoxarife",
                        Email = "almoxarife@estoque.com",
                        CPF = "12345678901", // (Lembre-se de usar um CPF válido para testes)
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(almoxarife, "Password123!");
                    await userManager.AddToRoleAsync(almoxarife, "Almoxarife");
                }

                // Criar 100 Produtos
                var produtoFaker = new Faker<Produto>("pt_BR")
                    .RuleFor(p => p.Nome, f => f.Commerce.ProductName())
                    .RuleFor(p => p.Descricao, f => f.Commerce.ProductDescription())
                    .RuleFor(p => p.EstoqueAtual, f => f.Random.Number(100, 500))
                    .RuleFor(p => p.CategoriaId, f => f.PickRandom(categorias).Id);
                var produtos = produtoFaker.Generate(100);
                await context.Produtos.AddRangeAsync(produtos);
                await context.SaveChangesAsync();

                // --- PASSO 5: CRIAR DADOS MASSIVOS (500 Movimentações) ---
                var movimentacaoFaker = new Faker<Movimentacao>("pt_BR")
                    .RuleFor(m => m.ProdutoId, f => f.PickRandom(produtos).Id)
                    .RuleFor(m => m.Data, f => f.Date.Past(1)) // Data aleatória no último ano
                    .RuleFor(m => m.Tipo, f => f.PickRandom(new[] { "Entrada", "Saída" }))
                    .RuleFor(m => m.Quantidade, f => f.Random.Number(1, 50))
                    .RuleFor(m => m.UsuarioId, almoxarife.Id)
                    // Lógica condicional: Se for "Saída", pega um Setor.
                    .RuleFor(m => m.SetorId, (f, m) => m.Tipo == "Saída" ? f.PickRandom(setores).Id : (int?)null)
                    // Se for "Entrada", pega o Fornecedor.
                    .RuleFor(m => m.FornecedorId, (f, m) => m.Tipo == "Entrada" ? fornecedor.Id : (int?)null);

                var movimentacoes = movimentacaoFaker.Generate(500);
                await context.Movimentacoes.AddRangeAsync(movimentacoes);

                // Salva tudo no banco
                await context.SaveChangesAsync();
            }
        }
    }
}