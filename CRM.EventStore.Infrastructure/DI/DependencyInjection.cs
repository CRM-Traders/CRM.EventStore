using System.Security.Cryptography;
using CRM.EventStore.Application.Common.Synchronizer;
using CRM.EventStore.Domain.Common.Options.Auth;
using CRM.EventStore.Infrastructure.Services.Synchronizer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CRM.EventStore.Infrastructure.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingeltonServices();
        services.AddScopedServices();

        services.AddCompression();
        services.ConfigureCors();

        services.AddOptions(configuration);

        services.AddAsymmetricAuthentication(configuration);

        return services;
    }

    private static void ConfigureCors(this IServiceCollection services)
    {
        // TODO Restrict In Future Base On Origins Options
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
    }

    private static void AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>()!;
        services.AddSingleton(jwtOptions);
    }

    private static void AddSingeltonServices(this IServiceCollection services)
    {
    }

    private static void AddScopedServices(this IServiceCollection services)
    {
        services.AddScoped<IPermissionSynchronizer, PermissionSynchronizer>();
    }

    private static void AddCompression(this IServiceCollection services)
    {
        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = System.IO.Compression.CompressionLevel.Fastest;
        });

        services.AddResponseCompression(options =>
        {
            options.Providers.Add<GzipCompressionProvider>();
            options.EnableForHttps = true;
        });
    }

    private static void AddAsymmetricAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>()!;

        byte[] publicKeyBytes = Convert.FromBase64String(jwtOptions.PublicKey);

        RSA rsaPublicKey = RSA.Create();
        rsaPublicKey.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

        var issuerSigningKey = new RsaSecurityKey(rsaPublicKey);


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
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = issuerSigningKey,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
    }
}