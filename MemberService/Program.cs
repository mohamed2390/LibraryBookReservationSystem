using FluentValidation;
using FluentValidation.AspNetCore;
using MemberService.Application.Interfaces;
using MemberService.Application.Services;
using MemberService.Application.Validators;
using MemberService.Domain.Interfaces;
using MemberService.Infrastructure.Persistence;
using MemberService.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// === FLUENT VALIDATION AUTO SETUP ===
builder.Services.AddFluentValidationAutoValidation();                    // ← This enables automatic 400 BadRequest
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>(); // discovers both validators

builder.Services.AddDbContext<MemberDbContext>(options =>
    options.UseInMemoryDatabase("MemberDb"));

builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IMemberService, MemberService.Application.Services.MemberService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher, SimplePasswordHasher>();

// JWT Authentication
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
       options.TokenValidationParameters = new TokenValidationParameters
       {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = builder.Configuration["Jwt:Issuer"],
          ValidAudience = builder.Configuration["Jwt:Audience"],
          IssuerSigningKey = new SymmetricSecurityKey(key)
       };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
   var db = scope.ServiceProvider.GetRequiredService<MemberDbContext>();
   var haser = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
   if (!await db.Members.AnyAsync())
   {
      db.Members.Add(new MemberService.Domain.Entities.Member
      {
         Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
         Username = "admin",
         Email = "admin@library.com",
         PasswordHash = haser.Hash("admin123"),
         Role = "Admin"
      });
      await db.SaveChangesAsync();
   }
}

if (app.Environment.IsDevelopment())
{
   app.UseSwagger();
   app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();