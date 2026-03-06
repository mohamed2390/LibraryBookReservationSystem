using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MemberService.Domain.Entities;
using MemberService.Infrastructure.Persistence;
using MemberService.Infrastructure.Persistence.Repositories;
using Xunit;

namespace MemberService.Tests.Infrastructure.Persistence.Repositories;

public class MemberRepositoryTests : IDisposable
{
   private readonly MemberDbContext _context;
   private readonly MemberRepository _sut;

   public MemberRepositoryTests()
   {
      var options = new DbContextOptionsBuilder<MemberDbContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .Options;

      _context = new MemberDbContext(options);
      _sut = new MemberRepository(_context);
   }

   [Fact]
   public async Task AddAsync_ShouldSaveMember()
   {
      var member = new Member { Username = "test", Email = "test@test.com", PasswordHash = "hash", Role = "Member" };

      await _sut.AddAsync(member);

      var saved = await _context.Members.FirstOrDefaultAsync(m => m.Username == "test");
      saved.Should().NotBeNull();
      saved!.Username.Should().Be("test");
   }

   [Fact]
   public async Task ExistsAsync_ShouldReturnTrue_WhenUserExists()
   {
      _context.Members.Add(new Member { Username = "existing" });
      await _context.SaveChangesAsync();

      var exists = await _sut.ExistsAsync("existing");
      exists.Should().BeTrue();
   }
   [Fact]
   public async Task ExistsByEmailAsync_ShouldReturnTrue_WhenEmailExists()
   {
      _context.Members.Add(new Member { Email = "duplicate@test.com" });
      await _context.SaveChangesAsync();

      var exists = await _sut.ExistsByEmailAsync("duplicate@test.com");
      exists.Should().BeTrue();
   }
   public void Dispose() => _context.Dispose();
}