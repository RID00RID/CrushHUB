using CrushHUB.Domain.Entities;
using CrushHUB.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CrushHUB.Domain;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<Crash> Crashes => Set<Crash>();

    public DbSet<UserReport> UserReports => Set<UserReport>();

    public DbSet<GameUser> GameUsers => Set<GameUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Project>(project =>
        {
            project.Property(p => p.Name).IsRequired().HasMaxLength(100);
            project.Property(p => p.Platform).IsRequired().HasMaxLength(50);
            project.Property(p => p.ApiKey).IsRequired().HasMaxLength(64);
            project.HasIndex(p => p.ApiKey).IsUnique();
        });

        modelBuilder.Entity<GameUser>(user =>
        {
            user.Property(u => u.SystemId).IsRequired().HasMaxLength(100);
            user.Property(u => u.OsName).HasMaxLength(200);
            user.Property(u => u.OsVersion).HasMaxLength(100);
            user.Property(u => u.Cpu).HasMaxLength(200);
            user.Property(u => u.Gpu).HasMaxLength(200);
            user.HasIndex(u => new { u.ProjectId, u.SystemId }).IsUnique();

            user.HasOne(u => u.Project)
                .WithMany()
                .HasForeignKey(u => u.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Crash>(crash =>
        {
            crash.Property(c => c.Title).IsRequired().HasMaxLength(300);
            crash.Property(c => c.Version).HasMaxLength(50);
            crash.Property(c => c.Platform).HasMaxLength(50);
            crash.HasIndex(c => new { c.ProjectId, c.OccurredAt });

            crash.HasOne(c => c.Project)
                .WithMany()
                .HasForeignKey(c => c.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Машину удаляем отдельно: каскад от проекта уже уносит и краши, и пользователей.
            crash.HasOne(c => c.GameUser)
                .WithMany()
                .HasForeignKey(c => c.GameUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<UserReport>(report =>
        {
            report.Property(r => r.Category).IsRequired().HasMaxLength(100);
            report.Property(r => r.Description).IsRequired();
            report.Property(r => r.ScreenshotPath).HasMaxLength(300);
            report.HasIndex(r => new { r.ProjectId, r.CreatedAt });

            report.HasOne(r => r.Project)
                .WithMany()
                .HasForeignKey(r => r.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            report.HasOne(r => r.GameUser)
                .WithMany()
                .HasForeignKey(r => r.GameUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<AppUser>(user =>
        {
            user.Property(u => u.DisplayName).HasMaxLength(100);
            user.Property(u => u.Bio).HasMaxLength(1000);
        });

        string adminName = "admin";
        string roleAdminId = "161B548E-0A90-43E0-A76E-0F34C60955B0";
        string roleMemberId = "2F5F5C1E-3A65-4F0C-9E5F-9F2A0B6C4D11";
        string userAdminId = "4B00D67B-169D-459D-8BE0-5A1F9575F247";

        modelBuilder.Entity<IdentityRole>().HasData(new IdentityRole()
        {
            Id = roleAdminId,
            Name = RoleNames.Admin,
            NormalizedName = RoleNames.Admin.ToUpper(),
            // Штампы и хеш пароля заданы константами: иначе каждая новая миграция
            // тащила бы за собой UpdateData со свежесгенерированными значениями.
            ConcurrencyStamp = "521d70ab-4374-4e03-b939-535e4086198e",
        });

        modelBuilder.Entity<IdentityRole>().HasData(new IdentityRole()
        {
            Id = roleMemberId,
            Name = RoleNames.Member,
            NormalizedName = RoleNames.Member.ToUpper(),
            ConcurrencyStamp = "8c1de6f0-5f2b-4b8a-9a4c-1d0f6b7e2a33",
        });

        modelBuilder.Entity<AppUser>().HasData(new AppUser()
        {
            Id = userAdminId,
            UserName = adminName,
            NormalizedUserName = adminName.ToUpper(),
            DisplayName = "Администратор",
            Email = "admin@admin.com",
            NormalizedEmail = "ADMIN@ADMIN.COM",
            EmailConfirmed = true,
            PasswordHash = "AQAAAAIAAYagAAAAEH9/P9sjnqQYmtdLt4WYqMyiUKm+/CJ1l0+xpKP+nZQkWEEA13l064nLv25vllnFXQ==",
            ConcurrencyStamp = "f46a6c1e-c201-4b8c-bca7-e39627f4e8ad",
            SecurityStamp = String.Empty,
            PhoneNumberConfirmed = true,
        });

        modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>()
        {
            RoleId = roleAdminId,
            UserId = userAdminId
        });

    }
}
