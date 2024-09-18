using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MedService.Models;

public partial class DbMedServiceContext : DbContext
{
    public DbMedServiceContext()
    {
    }

    public DbMedServiceContext(DbContextOptions<DbMedServiceContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Availability> Availabilities { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<Specialization> Specializations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server= DESKTOP-DHK8L7H\\SQLEXPRESS; Database=MedService; Trusted_Connection=True;  Trust Server Certificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.Property(e => e.AdminEmail).HasMaxLength(50);
            entity.Property(e => e.AdminName).HasMaxLength(50);
            entity.Property(e => e.AdminPassword).HasMaxLength(50);
        });

        modelBuilder.Entity<Availability>(entity =>
        {
            entity.HasKey(e => e.AvailabilityId);

            entity.Property(e => e.Day)
                  .IsRequired();

            entity.Property(e => e.Date)
                  .IsRequired();


            entity.Property(e => e.IsAvailable)
                  .IsRequired();

            entity.HasMany(e => e.DoctorAvailabilities)
                  .WithOne(da => da.Availability)
                  .HasForeignKey(da => da.AvailabilityId);
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId);

            entity.Property(e => e.PatientId)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.PatientName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.PatientEmail)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.PatientPassword)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.DateOfBirth)
                  .IsRequired(false); 

            entity.Property(e => e.PatientSex)
                  .IsRequired(false);  

            entity.Property(e => e.MedicalCardFilePath)
                  .HasMaxLength(255);

            entity.HasMany(d => d.DoctorAvailabilities)
                  .WithOne(da => da.Patient)
                  .HasForeignKey(da => da.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.DoctorId);

            entity.Property(e => e.DoctorId)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.DoctorName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.DoctorEmail)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.DoctorPassword)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.DoctorPhoto)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.HasOne(d => d.Specialization)
                  .WithMany(s => s.Doctors)
                  .HasForeignKey(d => d.SpecializationId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(d => d.DoctorAvailabilities)
                  .WithOne(da => da.Doctor)
                  .HasForeignKey(da => da.DoctorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DoctorAvailability>(entity =>
        {
            entity.HasKey(da => new { da.DoctorId, da.AvailabilityId });

            entity.HasOne(da => da.Doctor)
                  .WithMany(d => d.DoctorAvailabilities)
                  .HasForeignKey(da => da.DoctorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(da => da.Patient)
                  .WithMany(d => d.DoctorAvailabilities)
                  .HasForeignKey(da => da.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(da => da.Availability)
                  .WithMany(a => a.DoctorAvailabilities)
                  .HasForeignKey(da => da.AvailabilityId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Specialization>(entity =>
        {
            entity.Property(e => e.SpecializationName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
