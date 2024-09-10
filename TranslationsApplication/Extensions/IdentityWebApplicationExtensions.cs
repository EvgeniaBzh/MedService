using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using MedService.Data.Identity;
using MedService.Models;
using System.Numerics;

namespace MedService.Extensions;

public static class IdentityWebApplicationExtensions
{
    private record UserInfo(string Username, string Password, string Role)
    {
        public UserInfo() : this(string.Empty, string.Empty, string.Empty)
        {
        }
    }
    public static async Task AddUserIfNotExistsAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        DbMedServiceContext agencyContext,
        string userName,
        string password,
        string role)
    {
        var applicationUser = await userManager.FindByEmailAsync(userName);
        if (applicationUser is null)
        {
            applicationUser = new ApplicationUser
            {
                UserName = userName,
                Email = userName
            };
            var result = await userManager.CreateAsync(applicationUser, password);
            if (result.Succeeded)
            {
                logger.LogInformation("{username} user added", userName);
                await userManager.AddToRoleAsync(applicationUser, role);
                logger.LogInformation("{username} assigned to role {roleName}", userName, role);

                if (role.Equals("patient", StringComparison.OrdinalIgnoreCase))
                {
                    var patient = new Patient
                    {
                        PatientId = applicationUser.Id,
                        PatientName = applicationUser.UserName,
                        PatientEmail = applicationUser.Email,
                        PatientPassword = password
                    };
                    await agencyContext.Patients.AddAsync(patient);
                    await agencyContext.SaveChangesAsync();
                    logger.LogInformation("Patient {username} added to Patients table", userName);
                }

                else if (role.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    var admin = new Admin
                    {
                        AdminId = applicationUser.Id,
                        AdminName = applicationUser.UserName,
                        AdminEmail = applicationUser.Email,
                        AdminPassword = password
                    };
                    await agencyContext.Admins.AddAsync(admin);
                    await agencyContext.SaveChangesAsync();
                    logger.LogInformation("Admin {username} added to Admins table", userName);
                }

                else if (role.Equals("doctor", StringComparison.OrdinalIgnoreCase))
                {
                    var doctor = new Doctor
                    {
                        DoctorId = applicationUser.Id,
                        DoctorName = applicationUser.UserName,
                        DoctorEmail = applicationUser.Email,
                        DoctorPassword = password
                    };
                    await agencyContext.Doctors.AddAsync(doctor);
                    await agencyContext.SaveChangesAsync();
                    logger.LogInformation("Doctor {username} added to Doctors table", userName);
                }
            }
            else
            {
                logger.LogWarning("Failed to create user {username}: {errors}", userName, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogInformation("User {username} is already in database", userName);
        }
    }
    public static async Task InitializeRolesAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var roleName in RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole { Name = roleName };
                await roleManager.CreateAsync(role);
            }
        }
    }
    public static async Task InitializeUsersAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var agencyContext = serviceProvider.GetRequiredService<DbMedServiceContext>();
        var usersConfiguration = app.Configuration.GetSection("IdentityDefaults:Users").Get<UserInfo[]>();
        if (usersConfiguration is not null)
        {
            foreach (var userInfo in usersConfiguration)
            {
                await AddUserIfNotExistsAsync(userManager, app.Logger, agencyContext, userInfo.Username, userInfo.Password, userInfo.Role);
            }
        }
    }
}