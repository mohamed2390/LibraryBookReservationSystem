using Microsoft.EntityFrameworkCore;
using MemberService.Domain.Entities;

namespace MemberService.Infrastructure.Persistence;

public class MemberDbContext : DbContext
{
   public DbSet<Member> Members => Set<Member>();

   public MemberDbContext(DbContextOptions<MemberDbContext> options) : base(options) { }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      modelBuilder.Entity<Member>(entity =>
      {
         entity.HasKey(e => e.Id);
         entity.HasIndex(e => e.Username).IsUnique();
         entity.HasIndex(e => e.Email).IsUnique();
      });
   }
}