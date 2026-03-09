using LibraryMemberSystem.Application.DTOs;
using LibraryMemberSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMemeberSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
   private readonly IMemberService _memberService;

   public MembersController(IMemberService memberService)
   {
      _memberService = memberService;
   }

   [HttpPost("register")]
   public async Task<IActionResult> Register([FromBody] RegisterDto dto)
   {
      try
      {
         var response = await _memberService.RegisterAsync(dto);
         return Ok(response);
      }
      catch (InvalidOperationException ex)
      {
         return Conflict(new ProblemDetails
         {
            Title = "Conflict occurred, Bad Request",
            Detail = ex.Message,
            Status = StatusCodes.Status409Conflict,
         });
      }
   }

   [HttpPost("login")]
   public async Task<IActionResult> Login([FromBody] LoginDto dto)
   {
      var response = await _memberService.LoginAsync(dto);
      return response != null ? Ok(response) : Unauthorized("Invalid credentials");
   }

   
   [HttpGet]
   public async Task<IActionResult> GetAllMembers()
   {
      var members = await _memberService.GetAllMembersAsync();
      return Ok(members);
   }
}