using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DigitalLibrary.Models;

namespace DigitalLibrary.Data
{
    public class DigitalLibraryContext : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        int,
        IdentityUserClaim<int>,
        IdentityUserRole<int>,
        IdentityUserLogin<int>,
        IdentityRoleClaim<int>,
        IdentityUserToken<int>>
    {
        public DigitalLibraryContext(DbContextOptions<DigitalLibraryContext> options)
            : base(options)
        {
        }

        // DbSets cho các entity tùy chỉnh
        public DbSet<Category> Categories { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<FavDoc> FavDocs { get; set; }
        public DbSet<ViewLog> ViewLogs { get; set; }
        public DbSet<DownloadLog> DownloadLogs { get; set; }
        public DbSet<RoleClaim> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Đặt tên bảng cho Identity tables
            modelBuilder.Entity<ApplicationUser>(b =>
            {
                b.ToTable("AspNetUsers");
            });

            modelBuilder.Entity<ApplicationRole>(b =>
            {
                b.ToTable("AspNetRoles");
            });

            modelBuilder.Entity<IdentityUserClaim<int>>(b =>
            {
                b.ToTable("AspNetUserClaims");
            });

            modelBuilder.Entity<IdentityUserLogin<int>>(b =>
            {
                b.ToTable("AspNetUserLogins");
            });

            modelBuilder.Entity<IdentityUserRole<int>>(b =>
            {
                b.ToTable("AspNetUserRoles");
            });

            modelBuilder.Entity<IdentityRoleClaim<int>>(b =>
            {
                b.ToTable("AspNetRoleClaims");
            });

            modelBuilder.Entity<IdentityUserToken<int>>(b =>
            {
                b.ToTable("AspNetUserTokens");
            });

            // Cấu hình bảng RoleClaim tùy chỉnh
            modelBuilder.Entity<RoleClaim>(b =>
            {
                b.ToTable("RolePermissions");
                b.HasIndex(rc => new { rc.RoleId, rc.ClaimType, rc.ClaimValue }).IsUnique();
            });

            // Cấu hình indexes
            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Document>()
                .HasIndex(d => d.Title);

            modelBuilder.Entity<Document>()
                .HasIndex(d => d.CategoryId);

            // Cấu hình relationships
            modelBuilder.Entity<Document>()
                .HasOne(d => d.Category)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Document)
                .WithMany(d => d.Reviews)
                .HasForeignKey(r => r.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FavDoc>()
                .HasOne(f => f.User)
                .WithMany(u => u.FavDocs)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FavDoc>()
                .HasOne(f => f.Document)
                .WithMany(d => d.FavDocs)
                .HasForeignKey(f => f.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ViewLog>()
                .HasOne(v => v.User)
                .WithMany(u => u.ViewLogs)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ViewLog>()
                .HasOne(v => v.Document)
                .WithMany(d => d.ViewLogs)
                .HasForeignKey(v => v.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DownloadLog>()
                .HasOne(d => d.User)
                .WithMany(u => u.DownloadLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DownloadLog>()
                .HasOne(d => d.Document)
                .WithMany(doc => doc.DownloadLogs)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RoleClaim>()
                .HasOne(rc => rc.Role)
                .WithMany(r => r.RoleClaims)
                .HasForeignKey(rc => rc.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed dữ liệu
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Roles
            var adminRole = new ApplicationRole
            {
                Id = 1,
                Name = "Admin",
                NormalizedName = "ADMIN",
                Description = "Quản trị viên hệ thống",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            var librarianRole = new ApplicationRole
            {
                Id = 2,
                Name = "Librarian",
                NormalizedName = "LIBRARIAN",
                Description = "Thủ thư",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            var teacherRole = new ApplicationRole
            {
                Id = 3,
                Name = "Teacher",
                NormalizedName = "TEACHER",
                Description = "Giáo viên",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            var studentRole = new ApplicationRole
            {
                Id = 4,
                Name = "Student",
                NormalizedName = "STUDENT",
                Description = "Sinh viên",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            modelBuilder.Entity<ApplicationRole>().HasData(
                adminRole, librarianRole, teacherRole, studentRole
            );

            // Seed Default Permissions cho mỗi Role
            var permissions = new List<RoleClaim>();
            int permissionId = 1;

            // Admin - Full permissions
            var adminPermissions = new[]
            {
                // User Management
                ("User", "View"), ("User", "Create"), ("User", "Edit"), ("User", "Delete"),
                // Document Management
                ("Document", "View"), ("Document", "Create"), ("Document", "Edit"), ("Document", "Delete"), ("Document", "Download"), ("Document", "Upload"),
                // Category Management
                ("Category", "View"), ("Category", "Create"), ("Category", "Edit"), ("Category", "Delete"),
                // Review Management
                ("Review", "View"), ("Review", "Create"), ("Review", "Edit"), ("Review", "Delete"), ("Review", "Moderate"),
                // Dashboard
                ("Dashboard", "View"), ("Dashboard", "Export"),
                // System
                ("System", "Configure"), ("System", "Backup")
            };

            foreach (var (claimType, claimValue) in adminPermissions)
            {
                permissions.Add(new RoleClaim
                {
                    RoleClaimId = permissionId++,
                    RoleId = 1,
                    ClaimType = claimType,
                    ClaimValue = claimValue,
                    IsGranted = true
                });
            }

            // Librarian - Document và Content Management
            var librarianPermissions = new[]
            {
                ("Document", "View"), ("Document", "Create"), ("Document", "Edit"), ("Document", "Delete"), ("Document", "Download"), ("Document", "Upload"),
                ("Category", "View"), ("Category", "Create"), ("Category", "Edit"), ("Category", "Delete"),
                ("Review", "View"), ("Review", "Moderate"),
                ("Dashboard", "View")
            };

            foreach (var (claimType, claimValue) in librarianPermissions)
            {
                permissions.Add(new RoleClaim
                {
                    RoleClaimId = permissionId++,
                    RoleId = 2,
                    ClaimType = claimType,
                    ClaimValue = claimValue,
                    IsGranted = true
                });
            }

            // Teacher - Có thể tải và xem
            var teacherPermissions = new[]
            {
                ("Document", "View"), ("Document", "Download"), ("Document", "Upload"),
                ("Review", "View"), ("Review", "Create"), ("Review", "Edit"), ("Review", "Delete")
            };

            foreach (var (claimType, claimValue) in teacherPermissions)
            {
                permissions.Add(new RoleClaim
                {
                    RoleClaimId = permissionId++,
                    RoleId = 3,
                    ClaimType = claimType,
                    ClaimValue = claimValue,
                    IsGranted = true
                });
            }

            // Student - Chỉ xem và tải
            var studentPermissions = new[]
            {
                ("Document", "View"), ("Document", "Download"),
                ("Review", "View"), ("Review", "Create"), ("Review", "Edit"), ("Review", "Delete")
            };

            foreach (var (claimType, claimValue) in studentPermissions)
            {
                permissions.Add(new RoleClaim
                {
                    RoleClaimId = permissionId++,
                    RoleId = 4,
                    ClaimType = claimType,
                    ClaimValue = claimValue,
                    IsGranted = true
                });
            }

            modelBuilder.Entity<RoleClaim>().HasData(permissions);

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Công nghệ thông tin" },
                new Category { CategoryId = 2, CategoryName = "Khoa học tự nhiên" },
                new Category { CategoryId = 3, CategoryName = "Khoa học xã hội" },
                new Category { CategoryId = 4, CategoryName = "Văn học" },
                new Category { CategoryId = 5, CategoryName = "Kinh tế" },
                new Category { CategoryId = 6, CategoryName = "Y học" }
            );
        }
    }
}