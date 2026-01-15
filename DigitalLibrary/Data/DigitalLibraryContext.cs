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
    }
}
