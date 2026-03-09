using LibraryMemberSystem.Core.Entities;

namespace LibraryMemberSystem.Core.Interfaces;

public interface IMemberRepository
{
   Task<Member?> GetByUsernameAsync(string username);
   Task<IEnumerable<Member?>> GetAllAsync();
   Task<Member?> GetByIdAsync(Guid id);
   Task AddAsync(Member member);
   Task<bool> ExistsAsync(string username);
   Task<bool> ExistsByEmailAsync(string email);
}