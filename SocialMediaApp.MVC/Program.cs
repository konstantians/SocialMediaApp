using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Authentication;
using SocialMediaApp.AuthenticationLibrary;
using SocialMediaApp.DataAccessLibrary;
using SocialMediaApp.DataAccessLibrary.Repositories;
using SocialMediaApp.EmailServiceLibrary;
using SocialMediaApp.MVC.Hubs;
using SocialMediaApp.SharedModels;

namespace SocialMediaApp.MVC;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        IConfiguration configuration = builder.Configuration;

        // Add services to the container.
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Production)
        {
            var options = new ApplicationInsightsServiceOptions { ConnectionString = configuration["ApplicationInsights:ConnectionString"] };
            builder.Services.AddApplicationInsightsTelemetry(options: options);
        }

        builder.Services.AddControllersWithViews();
        builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultData"))
            );
        builder.Services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultAuthentication"))
        );

        
        builder.Services.AddIdentity<AppUser, IdentityRole>()
        .AddEntityFrameworkStores<AppIdentityDbContext>()
        .AddDefaultTokenProviders();

        builder.Services.Configure<IdentityOptions>(options =>
        {
            //these are all the password options, should be enough
            options.Password.RequiredLength = 10;
            options.Password.RequiredUniqueChars = 3;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;

            //these are all the lockout options. Maybe the only thing that is missing is a mechanism to 
            //increase lockout time in case of multiple lock outs
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

            options.SignIn.RequireConfirmedEmail = true;
        });

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.Cookie.IsEssential = true; // Ensure that the cookie is considered essential
            options.Cookie.HttpOnly = true; // Make the cookie accessible only through HTTP (not JavaScript) for security reasons
        });

        builder.Services.AddAuthentication().AddGoogle(options =>
        {
            options.ClientId = configuration.GetValue<string>("Authentication:Google:ClientId")!;
            options.ClientSecret = configuration.GetValue<string>("Authentication:Google:ClientSecret")!;
        }).AddMicrosoftAccount(options =>
        {
            options.ClientId = configuration.GetValue<string>("Authentication:Microsoft:ClientId")!;
            options.ClientSecret = configuration.GetValue<string>("Authentication:Microsoft:ClientSecret")!;
        }).AddTwitter(options =>
        {
            options.ConsumerKey = configuration.GetValue<string>("Authentication:Twitter:ClientId")!;
            options.ConsumerSecret = configuration.GetValue<string>("Authentication:Twitter:ClientSecret")!;
        });

        builder.Services.AddScoped<IAuthenticationProcedures, AuthenticationProcedures>();
        builder.Services.AddScoped<IPostDataAccess, PostDataAccess>();
        builder.Services.AddScoped<IMessageDataAccess, MessageDataAccess>();
        builder.Services.AddScoped<IChatDataAccess, ChatDataAccess>();
        builder.Services.AddScoped<INotificationDataAccess, NotificationDataAccess>();
        builder.Services.AddSingleton<IEmailService, EmailService>();

        builder.Services.AddSignalR();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapHub<PostVoteHub>("/PostVoteHub");
        app.MapHub<NotificationHub>("/NotificationHub");
        app.Run();
    }
}