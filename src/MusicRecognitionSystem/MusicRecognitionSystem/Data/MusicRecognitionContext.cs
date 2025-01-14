using Microsoft.EntityFrameworkCore;
using MusicRecognitionSystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicRecognitionSystem.Data
{
    internal class MusicRecognitionContext : DbContext
    {
        public DbSet<Song> Songs { get; set; }
        public DbSet<Hash> Hashes { get; set; }
        public DbSet<SongHash> SongHashes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=MusicRecognition;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SongHash>()
                .HasKey(sh => new { sh.songID, sh.hashID, sh.timestamp });

            modelBuilder.Entity<SongHash>()
                .HasOne(sh => sh.song)
                .WithMany(s => s.songHashes)
                .HasForeignKey(sh => sh.songID);

            modelBuilder.Entity<SongHash>()
                .HasOne(sh => sh.hash)
                .WithMany(h => h.songHashes)
                .HasForeignKey(sh => sh.hashID);
        }
    }
}
