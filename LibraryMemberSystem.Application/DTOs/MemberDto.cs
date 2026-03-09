namespace LibraryMemberSystem.Application.DTOs;

public class MemberDto
{
   public Guid Id { get; set; } = Guid.Empty;
   public string Username { get; set; } = string.Empty;
   public string Email { get; set; } = string.Empty;
   public string Role { get; set; } = string.Empty;
}