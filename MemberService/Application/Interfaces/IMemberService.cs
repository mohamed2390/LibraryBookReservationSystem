using MemberService.Application.DTOs;

namespace MemberService.Application.Interfaces;

public interface IMemberService
{
   Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
   Task<AuthResponseDto?> LoginAsync(LoginDto dto);
   Task<IEnumerable<MemberDto>> GetAllMembersAsync();
}