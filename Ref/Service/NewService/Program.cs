using System;
using System.Collections.Generic;
using AspNetCore.CacheOutput.InMemory.Extensions;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.NewService.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace CargoWise.RefDbRepo.NewService
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1102:Make class static", Justification = "<Pending>")]
	public class Program
	{
		static void Main(string[] args)
		{
			Environment.SetEnvironmentVariable("BASEDIR", AppDomain.CurrentDomain.BaseDirectory);
			var connectionString = ApplicationConfig.ZZReadOnlyDbServerEntities;
			using var logWrapper = new LogWrapper("deliveryService");

			var builder = WebApplication.CreateBuilder(args);
			builder.Services.AddWtgStatusHealthChecks(logWrapper, "DeliveryService", connectionString);
			builder.Services.AddSystemWebAdapters();
			builder.Services.AddControllers(options =>
				{
					options.Filters.Add<TrackDurationAttribute>();
				})
				.AddNewtonsoftJson(options =>
				{
					options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
				})
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.WriteIndented = true;
				});

			builder.Services.AddInMemoryCacheOutput();
			builder.Services.Configure<KestrelServerOptions>(options =>
			{
				options.AllowSynchronousIO = true;
			});
			builder.Services.Configure<IISServerOptions>(options =>
			{
				options.AllowSynchronousIO = true;
			});

			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddHttpContextAccessor();

			DependencyInjectionConfig.Register(builder.Services, logWrapper);

			if (builder.Environment.IsDevelopment())
			{
				builder.Services.AddSwaggerGen(options =>
				{
					options.AddSecurityDefinition("basic", new OpenApiSecurityScheme
					{
						Name = "Authorization",
						Type = SecuritySchemeType.Http,
						Scheme = "Basic",
						In = ParameterLocation.Header,
						Description = "Basic Authorization header using the Bearer scheme."
					});
					options.AddSecurityRequirement(new OpenApiSecurityRequirement
					{
						{
							new OpenApiSecurityScheme
							{
								Reference = new OpenApiReference
								{
									Type = ReferenceType.SecurityScheme,
									Id = "basic"
									}
							},
							new string[] {}
						}
					});
				});
			}

			builder.Services.Configure<MemoryUsageHostedServiceOptions>(ApplicationConfig.MemoryUsageLogging);
			builder.Services.AddHostedService<MemoryUsageHostedService>();

			builder.Services.AddAuthentication(options =>
			{
				options.AddScheme<BasicAuthenticationHandler>(AuthType.BasicAuth, AuthType.BasicAuth);
				options.AddScheme<TokenAuthenticationHandler>(AuthType.TokenAuth, AuthType.TokenAuth);
			});
			CorsPolicyConfig.Register(builder);

			var app = builder.Build();
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI(config =>
				{
					//configuration to improve swagger performance
					//as seen here https://github.com/swagger-api/swagger-ui/issues/3832
					config.ConfigObject.AdditionalItems["syntaxHighlight"] = new Dictionary<string, object>
					{
						["activated"] = false
					};
				});
			}

			app.UseCors();
			app.UseAuthentication();
			app.UseAuthorization();
			app.UseApiExceptionHandler();
			app.UseApiLog();
			app.UseSystemWebAdapters();
			app.MapControllers();
			app.UseWtgStatusHealthChecks();

			app.Run();
		}
	}
}
