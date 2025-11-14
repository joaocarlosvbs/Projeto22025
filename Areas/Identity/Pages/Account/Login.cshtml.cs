// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Projeto22025.Models; // <-- Garanta que seu 'Usuario' (ApplicationUser) está aqui
using Projeto22025.Data; // <-- Importa o DbContext
using Microsoft.EntityFrameworkCore; // <-- Importa o FirstOrDefaultAsync

namespace Projeto22025.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<Usuario> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly ApplicationDbContext _context; // <-- 1. Adicionar o DbContext
        private readonly UserManager<Usuario> _userManager; // <-- 2. Adicionar o UserManager

        public LoginModel(SignInManager<Usuario> signInManager,
                          ILogger<LoginModel> logger,
                          ApplicationDbContext context, // <-- 3. Injetar
                          UserManager<Usuario> userManager) // <-- 4. Injetar
        {
            _signInManager = signInManager;
            _logger = logger;
            _context = context; // <-- 5. Atribuir
            _userManager = userManager; // <-- 6. Atribuir
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        // --- MUDANÇA 7: O InputModel agora pede 'UsuarioOuCpf' ---
        public class InputModel
        {
            [Required(ErrorMessage = "O campo 'Usuário ou CPF' é obrigatório.")]
            [Display(Name = "Usuário ou CPF")]
            public string UsuarioOuCpf { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Senha")]
            public string Password { get; set; }

            [Display(Name = "Lembrar de mim?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        // --- MUDANÇA 8: A Lógica de Post (Login) ---
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                string loginInput = Input.UsuarioOuCpf;
                string userNameToTry; // O UserName que tentaremos logar

                // Verifica se o input parece ser um CPF (11 dígitos e só números)
                bool isCpf = loginInput.All(char.IsDigit) && loginInput.Length == 11;

                if (isCpf)
                {
                    // Se for CPF, busca o usuário pelo CPF no banco
                    var userByCpf = await _context.Users.FirstOrDefaultAsync(u => u.CPF == loginInput);

                    if (userByCpf != null)
                    {
                        // Se achou, pegamos o UserName dele para o login
                        userNameToTry = userByCpf.UserName;
                    }
                    else
                    {
                        // Se digitou um CPF que não existe, falha
                        ModelState.AddModelError(string.Empty, "Usuário, CPF ou Senha inválidos.");
                        return Page();
                    }
                }
                else
                {
                    // Se não for CPF, assume que é o Nome de Usuário
                    userNameToTry = loginInput;
                }

                // Tenta fazer o login com o userNameToTry e a senha
                var result = await _signInManager.PasswordSignInAsync(
                    userNameToTry,
                    Input.Password,
                    Input.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Usuário logado.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Conta de usuário bloqueada.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Usuário, CPF ou Senha inválidos.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}