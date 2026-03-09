using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using LibraryBookReservationSystem.Application.Services;
using LibraryBookReservationSystem.Application.Validators;
using LibraryBookReservationSystem.Application.ExceptionHandling;
using LibraryBookReservationSystem.Infrastructure.Persistence;
using LibraryBookReservationSystem.Core.Interfaces;
using LibraryBookReservationSystem.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using LibraryBookReservationSystem.Application.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// === SWAGGER WITH JWT AUTHORIZATION ===
builder.Services.AddSwaggerGen(c =>
{
   c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
   {
      Title = "BookService API",
      Version = "v1"
   });

   // Add JWT Bearer definition
   c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
   {
      Description = "Enter JWT token in the format: Bearer {your token here}",
      Name = "Authorization",
      In = Microsoft.OpenApi.Models.ParameterLocation.Header,
      Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
      Scheme = "Bearer"
   });

   // Apply security globally to all endpoints
   c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// === FLUENT VALIDATION AUTO SETUP ===
builder.Services.AddFluentValidationAutoValidation();           // Auto 400 responses
builder.Services.AddValidatorsFromAssemblyContaining<RequestBookDtoValidator>(); // Discovers both validators


builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseInMemoryDatabase("BookDb"));

builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookService, BookService>();


// JWT (same key as MemberService)
// === JWT AUTHENTICATION WITH GLOBAL 401 & 403 HANDLING ===
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
          IssuerSigningKey = new SymmetricSecurityKey(key),
          ClockSkew = TimeSpan.Zero
       };

       // === GLOBAL ERROR HANDLING ===
       options.Events = new JwtBearerEvents
       {
          // 401 Unauthorized (missing/invalid/expired token)
          OnChallenge = async context =>
          {
             context.HandleResponse(); // Prevent default redirect

             context.Response.ContentType = "application/problem+json";
             context.Response.StatusCode = StatusCodes.Status401Unauthorized;

             var problem = new ProblemDetails
             {
                Title = "Unauthorized",
                Detail = "Authentication token is missing, invalid, or expired. Please login again.",
                Status = StatusCodes.Status401Unauthorized,
                Instance = context.HttpContext.Request.Path
             };

             await context.Response.WriteAsJsonAsync(problem);
          },

          // 403 Forbidden (valid token but insufficient role - e.g. Member calling Admin endpoint)
          OnForbidden = async context =>
          {
             context.Response.ContentType = "application/problem+json";
             context.Response.StatusCode = StatusCodes.Status403Forbidden;

             var problem = new ProblemDetails
             {
                Title = "Forbidden",
                Detail = "You do not have permission to access this resource. Admin role is required.",
                Status = StatusCodes.Status403Forbidden,
                Instance = context.HttpContext.Request.Path
             };

             await context.Response.WriteAsJsonAsync(problem);
          }
       };
    });

builder.Services.AddAuthorization();

// === GLOBAL EXCEPTION HANDLER ===
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();   // Enables rich ProblemDetails responses

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
   var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
   db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
   app.UseSwagger();
   app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();