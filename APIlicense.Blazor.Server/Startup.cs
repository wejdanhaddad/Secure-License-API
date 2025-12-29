using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.Persistent.Base;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.Circuits;
using DevExpress.ExpressApp.Xpo;
using APIlicense.Blazor.Server.Services;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.OData;
using DevExpress.ExpressApp.WebApi.Services;
using APIlicense.WebApi.JWT;
using DevExpress.ExpressApp.Security.Authentication;
using DevExpress.ExpressApp.Security.Authentication.ClientServer;
using DevExpress.ExpressApp.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using LicenceAPIv2.Controllers; // Make sure this is here

namespace APIlicense.Blazor.Server;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(typeof(Microsoft.AspNetCore.SignalR.HubConnectionHandler<>), typeof(ProxyHubConnectionHandler<>));
        services.AddScoped<ITenantProvider, StaticTenantProvider>();

        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddHttpContextAccessor();
        services.AddScoped<IAuthenticationTokenProvider, JwtTokenProviderService>();
        services.AddScoped<CircuitHandler, CircuitHandlerProxy>();

        services.AddXaf(Configuration, builder =>
        {
            builder.UseApplication<APIlicenseBlazorApplication>();

            builder.ObjectSpaceProviders
                .AddSecuredXpo((serviceProvider, options) => {
                    string connectionString = serviceProvider.GetRequiredService<IConnectionStringProvider>().GetConnectionString();
                    options.ConnectionString = connectionString;
                    options.ThreadSafe = true;
                    options.UseSharedDataStoreProvider = true;
                })
                .AddNonPersistent();

            builder.Security
                .UseIntegratedMode(options => {
                    options.Lockout.Enabled = true;
                    options.RoleType = typeof(PermissionPolicyRole);
                    options.UserType = typeof(APIlicense.Module.BusinessObjects.ApplicationUser);
                    options.UserLoginInfoType = typeof(APIlicense.Module.BusinessObjects.ApplicationUserLoginInfo);
                    options.UseXpoPermissionsCaching();
                    options.Events.OnSecurityStrategyCreated += securityStrategy => {
                        ((SecurityStrategy)securityStrategy).PermissionsReloadMode = PermissionsReloadMode.NoCache;
                    };
                })
                .AddPasswordAuthentication(options => {
                    options.IsSupportChangePassword = true;
                });

            builder.AddXafWebApi(webApiBuilder =>
            {
                webApiBuilder.ConfigureOptions(options =>
                {
                    options.BusinessObject<LicenseTest>();
                    options.BusinessObject<APIlicense.Blazor.Server.Models.xaf_license>();
                     // Ensure this is present
                });
                webApiBuilder.AddXpoServices();
            });

            builder.Modules
                .AddConditionalAppearance()
                .AddValidation(options => {
                    options.AllowValidationDetailsAccess = false;
                })
                .Add<APIlicense.Module.APIlicenseModule>()
                .Add<APIlicenseBlazorModule>();

            builder.AddMultiTenancy()
                .WithHostDatabaseConnectionString(Configuration.GetConnectionString("ConnectionString"))
#if EASYTEST
                .WithHostDatabaseConnectionString(Configuration.GetConnectionString("EasyTestConnectionString"))
#endif
                .WithMultiTenancyModelDifferenceStore(options => {
#if !RELEASE
                    options.UseTenantSpecificModel = false;
#endif
                })
                .WithTenantResolver<TenantByEmailResolver>();
        });

        var authentication = services.AddAuthentication(options => {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        });
        authentication.AddCookie(options => {
            options.LoginPath = "/LoginPage";
        });
        authentication.AddJwtBearer(options => {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Authentication:Jwt:IssuerSigningKey"]))
            };
        });
        services.AddAuthorization(options => {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(
                    JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireXafAuthentication()
                .Build();
        });

        services
            .AddControllers()
            .AddOData((options, serviceProvider) => {
                options
                    .AddRouteComponents("api/odata", new EdmModelBuilder(serviceProvider).GetEdmModel())
                    .EnableQueryFeatures(100);
            });

        services.AddSwaggerGen(c => {
            c.EnableAnnotations();
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "APIlicense API",
                Version = "v1",
                Description = @"Use AddXafWebApi(options) in the APIlicense.Blazor.Server\Startup.cs file to make Business Objects available in the Web API."
            });
            c.AddSecurityDefinition("JWT", new OpenApiSecurityScheme()
            {
                Type = SecuritySchemeType.Http,
                Name = "Bearer",
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
                {
                    new OpenApiSecurityScheme() {
                        Reference = new OpenApiReference() {
                            Type = ReferenceType.SecurityScheme,
                            Id = "JWT"
                        }
                    },
                    new string[0]
                },
            });
        });

        services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o => {
            o.JsonSerializerOptions.PropertyNamingPolicy = null;
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "APIlicense WebApi v1");
            });
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseRequestLocalization();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseXaf();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapXafEndpoints();
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
            endpoints.MapControllers();
        });
    }
}