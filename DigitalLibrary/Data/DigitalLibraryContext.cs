using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DigitalLibrary.Models;

namespace DigitalLibrary.Data
{
    public class DigitalLibraryContext : DbContext
    {
        public DigitalLibraryContext (DbContextOptions<DigitalLibraryContext> options)
            : base(options)
        {
        }

        public DbSet<DigitalLibrary.Models.Role> Roles { get; set; } = default!;
        public DbSet<DigitalLibrary.Models.User> Users{ get; set; } = default!;
        public DbSet<DigitalLibrary.Models.Category> Categories{ get; set; } = default!;
        public DbSet<DigitalLibrary.Models.Document> Documents{ get; set; } = default!;
        public DbSet<DigitalLibrary.Models.FavDoc> FavDocs{ get; set; } = default!;
        public DbSet<DigitalLibrary.Models.ViewLog> ViewLogs{ get; set; } = default!;
        public DbSet<DigitalLibrary.Models.DownloadLog> DownloadLogs{ get; set; } = default!;
        public DbSet<DigitalLibrary.Models.Review>Reviews{ get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Document>()
                .HasIndex(d => d.Title);

            modelBuilder.Entity<Document>()
                .HasIndex(d => d.CategoryId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

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

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "Librarian" },
                new Role { RoleId = 3, RoleName = "Student" },
                new Role { RoleId = 4, RoleName = "Teacher" }
            );

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
