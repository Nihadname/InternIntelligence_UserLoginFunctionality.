using AutoMapper;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using FluentValidation;
using FluentValidation.AspNetCore;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using UserAuthFunctionality.Application.Profiles;
using UserAuthFunctionality.Application.Settings;
using UserAuthFunctionality.Application.Validators.AuthValidators;
using UserAuthFunctionality.Core.Entities;
using UserAuthFunctionality.DataAccess.Data;
using UserAuthFunctionality.Application.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserAuthFunctionality.Application.Interfaces;
using UserAuthFunctionality.Application.Implementations;
using Microsoft.AspNetCore.Mvc;
using UserAuthFunctionality.Core.Entities.Common;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Reflection;

namespace UserAuthFunctionality.Api
{
    public static class ServiceRegistration
    {
        public static void Register(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
           options.UseSqlServer(configuration.GetConnectionString("AppConnectionString"))
       );
            services.AddFluentValidationAutoValidation()
           .AddFluentValidationClientsideAdapters()
           .AddValidatorsFromAssemblyContaining<RegisterValidator>();

            services.AddFluentValidationRulesToSwagger();
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SmartEccommerceApi",
                    Version = "v1"
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
            });
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.User.RequireUniqueEmail = true;
            }).AddDefaultTokenProviders().AddEntityFrameworkStores<ApplicationDbContext>();
           services.AddSingleton<IMapper>(provider =>
            {
                var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

              //  using var scope = scopeFactory.CreateScope();
                //var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                //var photoOrVideoService = scope.ServiceProvider.GetRequiredService<IPhotoOrVideoService>();

                var mapperConfig = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile(new MapperProfile());
                });

                return mapperConfig.CreateMapper();
            });
            services.Configure<CloudinarySettings>(
    configuration.GetSection("CloudinarySettings"));
            services.AddSingleton(provider =>
            {
                var settings = provider.GetRequiredService<IOptions<CloudinarySettings>>().Value;

                Console.WriteLine($"Initializing Cloudinary with: {settings.CloudName}, {settings.ApiKey}, {settings.ApiSecret}");

                var account = new CloudinaryDotNet.Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
                var cloudinary = new Cloudinary(account);
                try
                {
                    var result = cloudinary.ListResources(new ListResourcesParams { MaxResults = 1 });
                    if (result.Error != null)
                    {
                        throw new CustomException(400, $"Cloudinary Account Error: {result.Error.Message}");
                    }
                }
                catch (Exception ex)
                {
                    throw new CustomException(400, ex.Message);
                }

                return cloudinary;
            });
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
       .AddJwtBearer(options =>
       {
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateLifetime = true,
               ValidateIssuerSigningKey = true,
               ValidIssuer = configuration["Jwt:Issuer"],
               ValidAudience = configuration["Jwt:Audience"],
               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"])),
               ClockSkew = TimeSpan.Zero
           };
       });
            services.AddControllersWithViews()
           .ConfigureApiBehaviorOptions(opt =>
           {
               opt.InvalidModelStateResponseFactory = context =>
               {
                   var errorsValidation = context.ModelState
                       .Where(e => e.Value?.Errors.Count > 0)
                       .ToDictionary(
                           x => x.Key,
                           x => x.Value.Errors.First().ErrorMessage
                       );
                   string errors = string.Empty;
                   foreach (KeyValuePair<string, string> keyValues in errorsValidation)
                   {
                       errors += keyValues.Key + " : " + keyValues.Value + ", ";
                   }

                   var response=Result<string>.Success(errors);
                   //var response = new
                   //{
                   //    message = "Validation errors occurred.",
                   //    errors
                   //};

                   return new BadRequestObjectResult(response);
               };
           });
          services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddFixedWindowLimiter("fixed", options =>
                {
                    options.PermitLimit = 10;
                    options.Window = TimeSpan.FromSeconds(10);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 5;
                });
            });
           services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.Load("UserAuthFunctionality.Application")));

            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<IAuthService, AuthService>();  
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IEmailService, EmailService>();
        }

    }
}
