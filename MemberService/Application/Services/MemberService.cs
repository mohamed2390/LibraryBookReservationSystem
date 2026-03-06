using MemberService.Application.DTOs;
using MemberService.Application.Interfaces;
using MemberService.Domain.Entities;
using MemberService.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace MemberService.Application.Services;

public class MemberService : IMemberService
{
   private readonly IMemberRepository _repository;
   private readonly ITokenService _tokenService;
   private readonly IPasswordHasher _passwordHasher;
   private readonly ILogger<MemberService> _logger;

   public MemberService(IMemberRepository repository, ITokenService tokenService, IPasswordHasher passwordHasher, ILogger<MemberService> logger)
   {
      _repository = repository;
      _tokenService = tokenService;
      _passwordHasher = passwordHasher;
      _logger = logger;
   }

   public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
   {
      if (await _repository.ExistsAsync(dto.Username))
         throw new InvalidOperationException("Username already exists");

      if (await _repository.ExistsByEmailAsync(dto.Email))
         throw new InvalidOperationException("Email already exists");

      var member = new Member
      {
         Id = Guid.NewGuid(),
         Username = dto.Username,
         Email = dto.Email,
         PasswordHash = _passwordHasher.Hash(dto.Password),
         Role = dto.Role ?? "Member"
      };

      await _repository.AddAsync(member);
      _logger.LogInformation("User registered: {Username}", dto.Username);

      var token = _tokenService.GenerateToken(member.Id, member.Username, member.Role);
      return new AuthResponseDto { Token = token, Role = member.Role };
   }

   public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
   {
      var member = await _repository.GetByUsernameAsync(dto.Username);
      if (member == null || !_passwordHasher.Verify(dto.Password,member.PasswordHash))
         return null;

      var token = _tokenService.GenerateToken(member.Id, member.Username, member.Role);
      return new AuthResponseDto { Token = token, Role = member.Role };
   }

   public async Task<IEnumerable<MemberDto>> GetAllMembersAsync()
   {
      var members = await _repository.GetAllAsync() ?? Enumerable.Empty<Member>();
      return members.Select(m => new MemberDto     
      {
         Id = m.Id,
         Username = m.Username,
         Email = m.Email,
         Role = m.Role
      });
   }
}