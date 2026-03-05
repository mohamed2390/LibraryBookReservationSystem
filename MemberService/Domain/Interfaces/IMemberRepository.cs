using MemberService.Domain.Entities;

namespace MemberService.Domain.Interfaces;

public interface IMemberRepository
{
   Task<Member?> GetByUsernameAsync(string username);
   Task<IEnumerable<Member?>> GetAllAsync();
   Task<Member?> GetByIdAsync(Guid id);
   Task AddAsync(Member member);
   Task<bool> ExistsAsync(string username);
}