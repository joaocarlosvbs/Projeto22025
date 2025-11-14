using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Projeto22025.Data;
using Projeto22025.Models; // <-- Para ApplicationUser

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar Connection String
var connectionString = builder.Configuration.GetConnectionString("conexao") ?? throw new InvalidOperationException("Connection string 'conexao' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 2. Configurar Identity Customizado
builder.Services.AddDefaultIdentity<Usuario>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 3. Configurar Servi�os
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<Consultas>(); // Registra sua classe de consulta
//Rotativa.AspNetCore.RotativaConfiguration.Setup(builder.Environment.WebRootPath, "Rotativa"); // Registra o PDF

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // <-- Adicionado (Se j� n�o estava)
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();