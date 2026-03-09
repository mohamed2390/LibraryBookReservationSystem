using LibraryMemberSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryMemberSystem.Infrastructure.Persistence;

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