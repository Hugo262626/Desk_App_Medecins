using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using rattrapageB4.Models;
using System.IO;

namespace rattrapageB4
{
    public class ClinicContext : DbContext
    {
        public DbSet<Speciality> Specialities { get; set; } = null!;
        public DbSet<Doctor> Doctors { get; set; } = null!;
        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured) return;

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            var connectionString = config.GetConnectionString("ClinicDb");
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indices utiles (performance agenda + conflits)
            modelBuilder.Entity<Appointment>()
                .HasIndex(a => new { a.DoctorId, a.StartAt });

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => new { a.PatientId, a.StartAt });
            modelBuilder.Entity<Speciality>()
                .HasIndex(s => s.Name)
                .IsUnique();

            modelBuilder.Entity<Doctor>()
                .HasIndex(d => new { d.LastName, d.FirstName })
                .IsUnique();


            base.OnModelCreating(modelBuilder);
        }
    }
}
