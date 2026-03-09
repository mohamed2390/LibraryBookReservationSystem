using System.ComponentModel.DataAnnotations;

namespace LibraryMemberSystem.Core.Entities;

public class Member
{
   public Guid Id { get; set; } = Guid.NewGuid();
   [Required] public string Username { get; set; } = string.Empty;
   [Required] public string Email { get; set; } = string.Empty;
   [Required] public string PasswordHash { get; set; } = string.Empty;
   [Required] public string Role { get; set; } = "Member"; // Admin or Member
}