using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using DigitalLibrary.Data;
using DigitalLibrary.Models;
using DigitalLibrary.Services;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// 1. ADD SERVICES TO THE CONTAINER
// ========================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ========================================
// SWAGGER + JWT AUTH
// ========================================

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DigitalLibrary API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT theo dạng: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ========================================
// 2. DATABASE CONFIGURATION
// ========================================

builder.Services.AddDbContext<DigitalLibraryContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DigitalLibraryContext")
    )
);

// ========================================
// 3. IDENTITY CONFIGURATION
// ========================================

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    options.User.RequireUniqueEmail = true;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<DigitalLibraryContext>()
.AddDefaultTokenProviders();

// ========================================
// 4. CUSTOM SERVICES
// ========================================

builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// ========================================
// 5. JWT AUTHENTICATION CONFIGURATION
// ========================================

var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? "YourSecretKeyHere_AtLeast32Characters!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? "DigitalLibraryAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? "DigitalLibraryClient";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)
            )
        };
    });

builder.Services.AddAuthorization();

// ========================================
// 6. CORS CONFIGURATION
// ========================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ========================================
// 7. BUILD APPLICATION
// ========================================

var app = builder.Build();

// ========================================
// 8. CONFIGURE HTTP REQUEST PIPELINE
// ========================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ========================================
// 9. INITIALIZE DATABASE WITH ADMIN USER
// ========================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<DigitalLibraryContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        context.Database.Migrate();

        var adminEmail = "admin@digitallibrary.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine($"Admin created: {adminEmail} / Admin@123");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"DB init error: {ex.Message}");
    }
}

app.Run();
