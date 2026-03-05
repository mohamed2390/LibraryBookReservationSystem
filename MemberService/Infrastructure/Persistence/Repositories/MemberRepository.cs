using Microsoft.EntityFrameworkCore;
using MemberService.Domain.Entities;
using MemberService.Domain.Interfaces;
using MemberService.Infrastructure.Persistence;

namespace MemberService.Infrastructure.Persistence.Repositories;

public class MemberRepository : IMemberRepository
{
   private readonly MemberDbContext _context;

   public MemberRepository(MemberDbContext context) => _context = context;

   public async Task<Member?> GetByUsernameAsync(string username)
       => await _context.Members.FirstOrDefaultAsync(m => m.Username == username);

   public async Task<IEnumerable<Member?>> GetAllAsync()
       => await _context.Members.ToListAsync();

   public async Task<Member?> GetByIdAsync(Guid id)
       => await _context.Members.FindAsync(id);

   public async Task AddAsync(Member member)
   {
      await _context.Members.AddAsync(member);
      await _context.SaveChangesAsync();
   }

   public async Task<bool> ExistsAsync(string username)
       => await _context.Members.AnyAsync(m => m.Username == username);
}