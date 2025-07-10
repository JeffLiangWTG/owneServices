using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CargoWise;
using CargoWise.Data;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Utilities.Environment;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NetCore.WebInfrastructure;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace Enterprise.Services.ServiceHost.NetCore
{
	class Program
	{
		[ModuleInitializer]
		public static void InitializeModule()
		{
			NetCoreAssemblyResolver.Setup();
		}

		public static void Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.WriteTo.Console()
				.CreateLogger();

			var builder = WebApplication.CreateBuilder(args);

			builder.Services.AddTransient<IAccountingBillingService, AccountingBillingService>();
			builder.Services.AddTransient<IAccountingPayablesService, AccountingPayablesService>();
			builder.Services.AddTransient<IAccountingRatingService, AccountingRatingService>();
			builder.Services.AddTransient<IAPReconciliationService, APReconciliationService>();

			builder.Services.AddHttpContextAccessor();
			builder.Services.AddLogging();
			builder.Services.AddMemoryCache();
			builder.Services.AddDistributedMemoryCache();
			builder.Services.AddSession();
			builder.Services
				.AddControllers(options =>
				{
					//Remove the special case formatter for strings as they are causing a difference in behaviour
					//https://learn.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-9.0#special-case-formatters-2
					options.OutputFormatters.RemoveType<StringOutputFormatter>();
				})
				.AddNewtonsoftJson(options =>
				{
					// Ensure PascalCase Naming (First Letter Capitalized)
					options.SerializerSettings.ContractResolver = new DefaultContractResolver
					{
						NamingStrategy = new DefaultNamingStrategy()
					};
					options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
				})
				.AddJsonOptions(o =>
				{
					o.JsonSerializerOptions.IncludeFields = true;
				});

			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddCargoWiseServiceFactory();
			EnterpriseApplicationConfiguration.ConfigureObjectFactory();
			InitialiseWebService();
			builder.Services.AddSingleton<IWebConfigurationProvider, WebConfigurationProvider>();

			builder.Services.AddSwaggerGen(c =>
			{
				c.AddSecurityDefinition((NoResString)"Bearer", //Name the security scheme
					new OpenApiSecurityScheme
					{
						Description = (NoResString)"Using Glow-Auth cookie from the request header to verify if a valid session exists.",
						Type = SecuritySchemeType.Http,
						Scheme = (NoResString)"bearer" //The name of the HTTP Authorization scheme to be used in the Authorization header. In this case "bearer".
					});

				c.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Id = (NoResString)"Bearer",
								Type = ReferenceType.SecurityScheme
							}
						},new List<string>()
					}
				});
			});

			var app = builder.Build();

			var configurationProvider = app.Services.GetRequiredService<IWebConfigurationProvider>();
			var dbConfigFromRegistry = configurationProvider.TryGetWebDbConfigFromIISRegistry(out var config);

			var serverName = dbConfigFromRegistry ? config.ServerName : builder.Configuration["ServerName"];
			var databaseName = dbConfigFromRegistry ? config.DatabaseName : builder.Configuration["DatabaseName"];

			Log.Information($"serverName : {serverName}");
			Log.Information($"databaseName : {databaseName}");
			InitializeDatabase(serverName, databaseName);

			app.UseSwagger();
			app.UseSwaggerUI();

			app.UseAuthorization();
			app.UseAuthentication();

			app.MapControllers();

			app.Run();
		}

		static void InitialiseWebService()
		{
			try
			{
				Globals.IsUserInteractive = false;
				WebInitialiser.Initialise(enableErrorReport: false);
			}
			catch (Exception)
			{
				throw;
			}
		}

		static void InitializeDatabase(string serverName, string databaseName)
		{
			// if already set for unit tests, don't change it
			if (Db.DatabaseNameIsInitialized && Db.ServerNameIsInitialized)
			{
				return;
			}

			Db.InitializeDatabaseDetails(
				Db.GetMachineNameIfLocal(serverName),
				databaseName,
				CargoWise.DataProtection.ApplicationType.Web);

			using (Db.DisposableActionForDbConnection())
			{
				Db.Connection.EnsureIsOpen();
			}
		}
	}
}
