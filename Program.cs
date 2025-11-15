using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Projeto22025.Data;
using Projeto22025.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. VOLTAMOS A USAR A CONNECTION STRING "conexao"
var connectionString = builder.Configuration.GetConnectionString("conexao") ?? throw new InvalidOperationException("Connection string 'conexao' not found.");

// 2. MUDAMOS DE UseSqlite DE VOLTA PARA UseSqlServer
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 3. Configurar Identity Customizado
builder.Services.AddDefaultIdentity<Usuario>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 4. Configurar Serviços
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<Consultas>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();

    // O "DbInitializer" (semeador) funciona com SQL Server também
    await DbInitializer.SeedDatabase(app);
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();