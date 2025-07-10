using System;
using System.Text.Json.Serialization;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Common.Web.ExceptionHandler;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace CargoWise.RefDbRepo.Staging.NewService
{
	public class Program
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "dependency injection")]
		static void Main(string[] args)
		{
			var argumentConfig = GetArgumentsConfig(args);

			Environment.SetEnvironmentVariable("BASEDIR", AppDomain.CurrentDomain.BaseDirectory);
			var builder = WebApplication.CreateBuilder(args);
			builder.Services.AddSystemWebAdapters();

			var accessTokenProvider = new AccessTokenProvider(ConfigurationProvider.TenantId, ConfigurationProvider.ClientId, ConfigurationProvider.ServiceId, ConfigurationProvider.PrivateKeyFileName, ConfigurationProvider.CertificateFileName, ConfigurationProvider.RefreshAdvanceInMinutes);
			var safeRepository = new SafeRepository(new Uri(argumentConfig.SafeUpdateServiceUri), accessTokenProvider: accessTokenProvider);
			builder.Services.AddSingleton<ILogWrapper>(new LogWrapper("StagingServiceException"))
				.AddScoped<IAuthorizationHelper>(sp => new StagingAuthorizationHelper(safeRepository))
				.AddScoped<IStagingRepository>(sp => new StagingRepository(argumentConfig.StagingDbConnectionString))
				.AddScoped<IEntityRecognition, EntityRecognition>()
				.AddScoped<IEntityMatcher, EntityMatcher>()
				.AddScoped<ITokenValidationHelper>(sp => new OIDCTokenValidationHelper(ConfigurationProvider.OIDCAuthority, ConfigurationProvider.OIDCAudience))
				.AddScoped<IRefDbRepoCrypto>(sp => new RefDbRepoCrypto(ConfigurationProvider.CipherPublicKeyName, ConfigurationProvider.CipherPrivateKeyName, ConfigurationProvider.AesKeyName));

			builder.Services.Configure<MemoryUsageHostedServiceOptions>(ConfigurationProvider.MemoryUsageLogging);
			builder.Services.AddHostedService<MemoryUsageHostedService>();

			builder.Services.AddControllersWithViews();
			builder.Services.AddControllers(
				options =>
				{
					options.Filters.Add<TrackDurationAttribute>();
				}
				)
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
					options.JsonSerializerOptions.PropertyNamingPolicy = null;
					options.JsonSerializerOptions.WriteIndented = true;
				})
				.AddOData(option =>
				{
					option.Count().Filter().OrderBy().Expand().Select().SetMaxTop(null);
					option.AddRouteComponents("odata", GetEdmModel(), services =>
					{
						services.AddSingleton<ODataBatchHandler>(sp => new ODataBatchHandlerSingleTransaction(() => new StagingRepository(ConfigurationProvider.StagingConnectionString)));
						services.AddSingleton(sp => new ODataQuerySettings() { TimeZone = TimeZoneInfo.Utc });
						services.AddSingleton<ODataMessageReaderSettings>(sp => new ODataMessageReaderSettings()
						{ MessageQuotas = new ODataMessageQuotas() { MaxOperationsPerChangeset = 65000, MaxPartsPerBatch = 65000, MaxReceivedMessageSize = int.MaxValue } });
					});
					option.TimeZone = TimeZoneInfo.Utc;
				});
			CorsPolicyConfig.Register(builder);
			builder.Services.Configure<KestrelServerOptions>(options =>
			{
				options.AllowSynchronousIO = true;
			});
			builder.Services.Configure<IISServerOptions>(options =>
			{
				options.AllowSynchronousIO = true;
			});
			builder.Services.AddAuthentication(options =>
			{
				options.AddScheme<TokenAuthenticationHandler>(AuthType.TokenAuth, AuthType.TokenAuth);
			});

			var app = builder.Build();

			app.UseCors();
			app.UseCommonApiExceptionHandler();
			app.UseODataBatching();
			app.UseRouting();
			app.UseCors();
			app.UseAuthentication();
			app.UseAuthorization();
			app.UseCommonApiExceptionHandler();
			app.UseSystemWebAdapters();
			app.MapControllerRoute(
				name: "Default",
				pattern: "{controller}/{action}/{id?}",
				defaults: new { action = "Index" });
			app.Map("/{resource}.axd/{*pathInfo}", delegate
			{ });

			app.MapDefaultControllerRoute();
			app.Run();
		}

		static IEdmModel GetEdmModel()
		{
			var builder = new ODataConventionModelBuilder();
			ModelConfig.RegisterModels(builder);
			return builder.GetEdmModel();
		}

		static ArgumentConfig GetArgumentsConfig(string[] args)
		{
			var argumentConfig = new ArgumentConfig();
			if (args == null || args.Length == 0)
			{
				return argumentConfig;
			}

			// override StagingDbConnectionString if --stagingDbConnectionString is provided
			var stagingDbConnStringIndex = Array.IndexOf(args, "--stagingDbConnectionString");
			if (stagingDbConnStringIndex >= 0 && stagingDbConnStringIndex < args.Length - 1)
			{
				argumentConfig.StagingDbConnectionString = args[stagingDbConnStringIndex + 1];
			}

			// override SafeUpdateServiceUri if --safeUpdateServiceUri is provided
			var safeUpdateServiceUriIndex = Array.IndexOf(args, "--safeUpdateServiceUri");
			if (safeUpdateServiceUriIndex >= 0 && safeUpdateServiceUriIndex < args.Length - 1)
			{
				argumentConfig.SafeUpdateServiceUri = args[safeUpdateServiceUriIndex + 1];
			}

			return argumentConfig;
		}
	}

	internal class ArgumentConfig
	{
		public string StagingDbConnectionString { get; set; } = ConfigurationProvider.StagingConnectionString;
		public string SafeUpdateServiceUri { get; set; } = ConfigurationProvider.SafeUpdateServiceUri;
	}
}
