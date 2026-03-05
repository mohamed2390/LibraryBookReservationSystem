using MemberService.Application.Interfaces;

namespace MemberService.Application.Services
{
   public class SimplePasswordHasher : IPasswordHasher
   {
      public string Hash(string password)
      {
         using var sha = System.Security.Cryptography.SHA256.Create();
         var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
         return Convert.ToBase64String(bytes);
      }

      public bool Verify(string password, string hash) => Hash(password) == hash;
   }
}
