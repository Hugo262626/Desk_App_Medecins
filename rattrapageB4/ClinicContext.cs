using Microsoft.EntityFrameworkCore;
using rattrapageB4.Models;

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
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = "Server=localhost;Database=clinic;User=root;Password=mysql;";
                optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            }
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

            base.OnModelCreating(modelBuilder);
        }
    }
}
