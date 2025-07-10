using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.ErrorReporting;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web;
using CargoWise.RefDbRepo.Common.Web.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public class Program
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "dependency injection")]
		static void Main(string[] args)
		{
			var argumentConfig = GetArgumentsConfig(args);
			Environment.SetEnvironmentVariable("BASEDIR", argumentConfig.BaseDir);
			var safeConnectionString = argumentConfig.SafeDbConnectionString;
			var logWrapper = new LogWrapper("UpdateServiceException");

			var builder = WebApplication.CreateBuilder(args);
			builder.Services.AddSystemWebAdapters();
			builder.Services.AddWtgStatusHealthChecks(logWrapper, "UpdateService", safeConnectionString);

			builder.Services.AddScoped<ISystemVersionContext, SystemVersionContext>()
				.AddScoped<SystemVersionInterceptor>()
				.AddScoped<IReferenceDataRepository>(sp =>
				{
					var interceptor = sp.GetRequiredService<SystemVersionInterceptor>();
					return new ReferenceDataRepository(false, safeConnectionString, interceptor);
				})
				.AddScoped<IAuthorizationHelper, SafeAuthorizationHelper>()
				.AddSingleton<ILogWrapper>(logWrapper)
				.AddSingleton(new ErrorReportingClientWrapper())
				.AddScoped<IClientLicenseInformationProvider, ClientLicenseInformationProvider>()
				.AddScoped<ITokenValidationHelper>(sp => new OIDCTokenValidationHelper(ConfigurationProvider.OIDCAuthority, ConfigurationProvider.OIDCAudience, ConfigurationProvider.DisableAuthentication))
				.AddScoped<ITokenValidationHelper>(sp => new S2STrustTokenValidationHelper(ConfigurationProvider.S2SAuthority, ConfigurationProvider.S2SAudience, ConfigurationProvider.DisableAuthentication));

			builder.Services.AddControllers(options =>
				{
					options.Filters.Add<TrackDurationAttribute>();
				})
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.PropertyNamingPolicy = null;
				})
				.AddOData(option =>
				{
					option.Count().Filter().OrderBy().Expand().Select().SetMaxTop(null);
					option.AddRouteComponents("odata", ModelConfig.GetEdmModel(), services =>
					{
						services.AddSingleton<IODataDeserializerProvider>(sp => new CustomDeserializerProvider(sp))
						.AddSingleton<IODataSerializerProvider>(sp => new CustomSerializerProvider(sp))
						.AddSingleton<ODataQuerySettings>(sp => new ODataQuerySettings() { TimeZone = TimeZoneInfo.Utc })
						.AddSingleton<ODataBatchHandler>(sp => new ODataBatchHandlerSingleTransaction(() => new ReferenceDataRepository(true, safeConnectionString)))
						.AddSingleton<ODataMessageReaderSettings>(sp => new ODataMessageReaderSettings()
						{
							MessageQuotas = new ODataMessageQuotas() { MaxOperationsPerChangeset = 65000, MaxPartsPerBatch = 65000, MaxReceivedMessageSize = int.MaxValue }
						});
					});
					option.TimeZone = TimeZoneInfo.Utc;
				});

			//must add it after AddControllers to get the default model validator
			var validatorServiceDescriptor = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(IObjectModelValidator));
			builder.Services.AddSingleton<IObjectModelValidator>(sp => CustomBodyModelValidator.GetCustomModelValidator(sp, validatorServiceDescriptor));

			builder.Services.Configure<MemoryUsageHostedServiceOptions>(ConfigurationProvider.MemoryUsageLogging);
			builder.Services.AddHostedService<MemoryUsageHostedService>();

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
			builder.Services.AddSwaggerGen();
			CorsPolicyConfig.Register(builder);
			builder.Services.AddAuthentication(options =>
			{
				options.AddScheme<TokenAuthenticationHandler>(AuthType.TokenAuth, AuthType.TokenAuth);
			});

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
				app.UseCors();
			}

			app.UseApiExceptionHandler();
			app.UseAuthentication();
			app.UseODataBatching(); // Must be before UseRouting
			app.UseRouting();
			app.UseCors();

			app.UseAuthorization(); // Must be after UseRouting
			app.UseApiExceptionHandler();
			app.UseSystemWebAdapters();
			app.MapControllers();
			app.UseWtgStatusHealthChecks();
			app.Run();
		}

		static ArgumentConfig GetArgumentsConfig(string[] args)
		{
			var argumentConfig = new ArgumentConfig();
			if (args == null || args.Length == 0)
			{
				return argumentConfig;
			}
			var index = Array.IndexOf(args, "--entityConnectionString");
			if (index >= 0 && index < args.Length - 1)
			{
				argumentConfig.SafeDbConnectionString = args[index + 1];
			}
			index = Array.IndexOf(args, "--baseDir");
			if (index >= 0 && index < args.Length - 1)
			{
				argumentConfig.BaseDir = args[index + 1];
			}
			return argumentConfig;
		}

	}

	class ArgumentConfig
	{
		public string BaseDir { get; set; } = AppDomain.CurrentDomain.BaseDirectory;
		public string SafeDbConnectionString { get; set; } = ConfigurationProvider.SafeConnectionString;
	}
}
