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
// 2. SWAGGER CONFIGURATION WITH JWT SUPPORT
// ========================================

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Digital Library API",
        Version = "v1",
        Description = "API quản lý thư viện số với hệ thống phân quyền động",
        Contact = new OpenApiContact
        {
            Name = "Digital Library Team",
            Email = "support@digitallibrary.com"
        }
    });

    // Thêm định nghĩa Security cho JWT Bearer Token
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = @"JWT Authorization header using the Bearer scheme.
                      
Enter 'Bearer' [space] and then your token in the text input below.

Example: 'Bearer 12345abcdef'",
    });

    // Yêu cầu JWT cho tất cả operations
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});

// ========================================
// 3. DATABASE CONFIGURATION
// ========================================

builder.Services.AddDbContext<DigitalLibraryContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DigitalLibraryContext")));

// ========================================
// 4. IDENTITY CONFIGURATION
// ========================================

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<DigitalLibraryContext>()
.AddDefaultTokenProviders();

// ========================================
// 5. CUSTOM SERVICES
// ========================================

builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// ========================================
// 6. JWT AUTHENTICATION CONFIGURATION
// ========================================

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "YourSecretKeyHere_AtLeast32Characters!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "DigitalLibraryAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "DigitalLibraryClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero // Remove delay of token when expire
    };

    // Thêm support cho Swagger Authorization
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Hỗ trợ token từ query string (useful cho Swagger)
            var accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// ========================================
// 7. CORS CONFIGURATION
// ========================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// ========================================
// 8. BUILD APPLICATION
// ========================================

var app = builder.Build();

// ========================================
// 9. CONFIGURE HTTP REQUEST PIPELINE
// ========================================

// Swagger available in all environments for easier testing
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Digital Library API v1");
    options.RoutePrefix = string.Empty; // Swagger at root URL
    options.DocumentTitle = "Digital Library API";
    options.DefaultModelsExpandDepth(-1); // Hide schemas section by default
});

app.UseHttpsRedirection();

// Serve static files from uploads folder
app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ========================================
// 10. INITIALIZE DATABASE WITH ADMIN USER
// ========================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DigitalLibraryContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        // Ensure database is created
        context.Database.Migrate();

        // Create default admin user if not exists
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
                Console.WriteLine("═══════════════════════════════════════════════════════");
                Console.WriteLine("✅ DEFAULT ADMIN USER CREATED");
                Console.WriteLine($"📧 Email: {adminEmail}");
                Console.WriteLine($"🔑 Password: Admin@123");
                Console.WriteLine("⚠️  Please change password after first login!");
                Console.WriteLine("═══════════════════════════════════════════════════════");
            }
            else
            {
                Console.WriteLine("❌ Failed to create admin user:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"   - {error.Description}");
                }
            }
        }
        else
        {
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("ℹ️  Admin user already exists");
            Console.WriteLine($"📧 Email: {adminEmail}");
            Console.WriteLine("═══════════════════════════════════════════════════════");
        }

        // Display application info
        Console.WriteLine("");
        Console.WriteLine("🚀 DIGITAL LIBRARY API STARTED");
        Console.WriteLine($"📍 Swagger UI: https://localhost:{builder.Configuration["Https:Port"] ?? "7000"}");
        Console.WriteLine($"🔐 Authentication: JWT Bearer Token");
        Console.WriteLine($"💾 Database: {context.Database.GetConnectionString()}");
        Console.WriteLine("");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ An error occurred while initializing the database: {ex.Message}");
        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
    }
}

app.Run();

/*
 * ═══════════════════════════════════════════════════════════════════════════
 * DIGITAL LIBRARY API - CONFIGURATION NOTES
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 * 1. SWAGGER AUTHORIZATION:
 *    - Click "Authorize" button in Swagger UI
 *    - Enter: Bearer {your-token}
 *    - Example: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
 *    - All subsequent requests will include the token
 * 
 * 2. ASP.NET IDENTITY TABLES:
 *    - AspNetUsers: User information
 *    - AspNetRoles: Roles (Admin, Librarian, Teacher, Student)
 *    - AspNetUserRoles: User-Role mapping
 *    - AspNetRoleClaims: Built-in role claims
 *    - RolePermissions: Custom permission table (dynamic permissions)
 * 
 * 3. DEFAULT CREDENTIALS:
 *    Email: admin@digitallibrary.com
 *    Password: Admin@123
 *    ⚠️ Change password immediately after first login!
 * 
 * 4. PERMISSION SYSTEM:
 *    - Permissions stored in RolePermissions table
 *    - Admin can enable/disable any permission for any role
 *    - Changes take effect immediately (no restart needed)
 *    - Use [RequirePermission("Type", "Action")] on endpoints
 * 
 * 5. GETTING JWT TOKEN:
 *    POST /api/Auth/login
 *    {
 *      "email": "admin@digitallibrary.com",
 *      "password": "Admin@123"
 *    }
 *    
 *    Response will include "token" field - copy this token
 * 
 * 6. USING TOKEN IN SWAGGER:
 *    - Click "Authorize" button (top right)
 *    - Enter: Bearer {paste-your-token-here}
 *    - Click "Authorize"
 *    - Click "Close"
 *    - Now all requests will be authenticated!
 * 
 * 7. MIGRATION COMMANDS:
 *    dotnet ef migrations add InitialIdentity
 *    dotnet ef database update
 * 
 * 8. TESTING WORKFLOW:
 *    a) Navigate to Swagger UI (root URL)
 *    b) Expand POST /api/Auth/login
 *    c) Click "Try it out"
 *    d) Enter credentials
 *    e) Execute
 *    f) Copy the token from response
 *    g) Click "Authorize" button
 *    h) Enter "Bearer {token}"
 *    i) Test protected endpoints!
 * 
 * 9. CORS:
 *    - Currently allows all origins (AllowAll policy)
 *    - Restrict in production to specific domains
 * 
 * 10. FILE UPLOADS:
 *     - Files saved to /uploads directory
 *     - Static file serving enabled
 *     - Access via /uploads/{filename}
 * 
 * ═══════════════════════════════════════════════════════════════════════════
 */