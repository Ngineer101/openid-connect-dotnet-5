using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NWBlog.OpenIdConnect.Demo.Identity;
using OpenIddict.Abstractions;
using System;

namespace NWBlog.OpenIdConnect.Demo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "NWBlog.OpenIdConnect.Demo", Version = "v1" });
            });

            services.AddDbContext<DefaultDbContext>(options =>
            {
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnectionString"));
                options.UseOpenIddict();
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
                // configure more options if necessary...
            });

            // OpenId Connect server configuration
            services.AddOpenIddict()
                .AddCore(options => options.UseEntityFrameworkCore().UseDbContext<DefaultDbContext>())
                .AddServer(options =>
                {
                    // Enable the required endpoints
                    options.SetTokenEndpointUris("/connect/token");
                    options.SetUserinfoEndpointUris("/connect/userinfo");

                    options.AllowPasswordFlow();
                    options.AllowRefreshTokenFlow();
                    // Add all auth flows that you want to support
                    // Supported flows are:
                    //      - Authorization code flow
                    //      - Client credentials flow
                    //      - Device code flow
                    //      - Implicit flow
                    //      - Password flow
                    //      - Refresh token flow

                    // Custom auth flows are also supported
                    options.AllowCustomFlow("custom_flow_name");

                    // Using reference tokens means that the actual access and refresh tokens are stored in the database
                    // and a token referencing the actual tokens (in the db) is used in the request header.
                    // The actual tokens are not made public.
                    options.UseReferenceAccessTokens();
                    options.UseReferenceRefreshTokens();

                    // Register your scopes
                    // Scopes are a list of identifiers used to specify what access privileges are requested.
                    options.RegisterScopes(OpenIddictConstants.Permissions.Scopes.Email,
                                                OpenIddictConstants.Permissions.Scopes.Profile,
                                                OpenIddictConstants.Permissions.Scopes.Roles);

                    // Set the lifetime of your tokens
                    options.SetAccessTokenLifetime(TimeSpan.FromMinutes(30));
                    options.SetRefreshTokenLifetime(TimeSpan.FromDays(7));

                    // Register signing and encryption details
                    options.AddDevelopmentEncryptionCertificate()
                                    .AddDevelopmentSigningCertificate();

                    // Register ASP.NET Core host and configure options
                    options.UseAspNetCore().EnableTokenEndpointPassthrough();
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = OpenIddictConstants.Schemes.Bearer;
                options.DefaultChallengeScheme = OpenIddictConstants.Schemes.Bearer;
            });

            services.AddIdentity<User, Role>()
                .AddSignInManager()
                .AddUserStore<UserStore>()
                .AddRoleStore<RoleStore>()
                .AddUserManager<UserManager<User>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NWBlog.OpenIdConnect.Demo v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication(); // add this line
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
