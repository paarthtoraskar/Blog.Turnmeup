using AspNet.Security.OpenIdConnect.Primitives;
using Blog.Turnmeup.DAL.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;

namespace Blog.Turnmeup.API
{
    public partial class Startup
    {
        private void ConfigureServicesAuth(IServiceCollection services)
        {
            services.AddDbContext<IdentityDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("AuthServer"));
                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
            });


            // Register the OpenIddict services.
            services.AddOpenIddict().AddCore(options =>
            {
                // Register the Entity Framework stores.
                options.UseEntityFrameworkCore().UseDbContext<IdentityDbContext>();

                //// Register the ASP.NET Core MVC binder used by OpenIddict.
                //// Note: if you don't call this method, you won't be able to
                //// bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                //options.AddMvcBinders();

                //// Enable the token endpoint (required to use the password flow).
                //options.EnableTokenEndpoint("/connect/token");

                //// Allow client applications to use the grant_type=password flow.
                //options.AllowPasswordFlow();

                //// During development, you can disable the HTTPS requirement.
                //options.DisableHttpsRequirement();
            }).AddServer(options =>
            {
                // AddMvcBinders() is now UseMvc().
                options.UseMvc();

                options.EnableAuthorizationEndpoint("/connect/authorize")
                    .EnableLogoutEndpoint("/connect/logout")
                    .EnableTokenEndpoint("/connect/token")
                    .EnableUserinfoEndpoint("/api/userinfo");

                options.AllowAuthorizationCodeFlow()
                    .AllowPasswordFlow()
                    .AllowRefreshTokenFlow();

                options.RegisterScopes(OpenIdConnectConstants.Scopes.Email,
                    OpenIdConnectConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles);

                // This API was removed as client identification is now
                // required by default. You can remove or comment this line.
                //
                // options.RequireClientIdentification();

                options.EnableRequestCaching();

                // This API was removed as scope validation is now enforced
                // by default. You can safely remove or comment this line.
                //
                // options.EnableScopeValidation();

                options.DisableHttpsRequirement();
            });

            // TODO: Inject custom validation 
            // services.AddTransient<IPasswordValidator<AppUser>, CustomPasswordValidator>();
            // services.AddTransient<IUserValidator<AppUser>, CustomUserValidator>();

            // Register the Identity services.
            services.AddIdentity<AppUser, IdentityRole>(opts =>
            {
                opts.Password.RequiredLength = 6;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireDigit = false;
            })
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

            // Configure Identity to use the same JWT claims as OpenIddict instead
            // of the legacy WS-Federation claims it uses by default (ClaimTypes),
            // which saves you from doing the mapping in your authorization controller.
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });
        }

        private void ConfigureAuth(IApplicationBuilder app)
        {
            app.UseIdentity();
            app.UseAuthentication();
        }
    }
}