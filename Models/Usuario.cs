// Em Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Projeto22025.Models
{
    public class Usuario : IdentityUser
    {
        [Required]
        [StringLength(11)]
        public string CPF { get; set; } = string.Empty;
    }
}