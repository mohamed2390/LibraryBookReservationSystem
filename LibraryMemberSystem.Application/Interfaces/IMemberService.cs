using LibraryMemberSystem.Application.DTOs;

namespace LibraryMemberSystem.Application.Interfaces;

public interface IMemberService
{
   Task<string> RegisterAsync(RegisterDto dto);
   Task<AuthResponseDto?> LoginAsync(LoginDto dto);
   Task<IEnumerable<MemberDto>> GetAllMembersAsync();
}