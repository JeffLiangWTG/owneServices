using System;
using System.IO;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web;
using CargoWise.RefDbRepo.Staging.ProcessorRunner;
using CargoWise.RefDbRepo.Staging.Schedulers.Common;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Quartz.Logging;
using Topshelf;

namespace CargoWise.RefDbRepo.Staging.NewSchedulers
{
	public class Program
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "dependency injection")]
		static void Main(string[] args)
		{
			Environment.SetEnvironmentVariable("BASEDIR", AppDomain.CurrentDomain.BaseDirectory);
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
			var logWrapper = new LogWrapper("NewStagingProcessor");
			var logHelper = new LogHelper(logWrapper, "NewScheduler");

			var builder = WebApplication.CreateBuilder(args);
			builder.Services.AddWtgStatusHealthChecks(logWrapper, "Quartz", ConfigurationProvider.QuartzDefaultConnectionString);
			builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
			builder.Services.AddAuthorization();
			builder.Services.AddHttpContextAccessor();
			builder.Services.AddScoped<IFormCollectionService, FormCollectionService>();
			builder.WebHost.UseKestrel(option => option.AddServerHeader = false);
			CorsPolicyConfig.Register(builder);

			var app = builder.Build();
			app.UseStaticFiles();
			app.UseStaticFiles(new StaticFileOptions
			{
				FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
				RequestPath = "/auth"
			});
			app.UseCors();
			app.UseAuthentication();
			app.UseAuthorization();
			app.UseWtgHealthChecksForQuartz();
			using (var stagingRepo = new StagingRepository(ConfigurationProvider.StagingConnectionString))
			using (var accessTokenProvider = new AccessTokenProvider(ConfigurationProvider.TenantId, ConfigurationProvider.ClientId, ConfigurationProvider.ServiceId, ConfigurationProvider.PrivateKeyFileName, ConfigurationProvider.CertificateFileName, ConfigurationProvider.RefreshAdvanceInMinutes))
			{
				app.CheckTriggerScheduler(CreateSchedulerValidator(stagingRepo));
				app.UseCustomAuthorization(CreateAuthorizationHelper(stagingRepo, accessTokenProvider));
				app.MapGet("/auth/quartz", Request);
				app.MapPost("/auth/quartz", Request);

				logHelper.LogInfo("Start NewSchedulers Program");
				LogProvider.SetCurrentLogProvider(new QuartzLogProvider(logHelper));
#if DEBUG
				if (UnitTestDetector.IsRunningTests.Value)
				{
					var server = QuartzServerFactory.CreateServer(logHelper, app);
					server.Initialize().Wait();
					server.StartScheduler().Wait();
					return;
				}
#endif
				try
				{
					HostFactory.Run(x =>
					{
						x.RunAsLocalSystem();
						x.SetDescription(ServerConfiguration.ServiceDescription);
						x.SetDisplayName(ServerConfiguration.ServiceDisplayName);
						x.SetServiceName(ServerConfiguration.ServiceName);
						x.Service(factory =>
						{
							var server = QuartzServerFactory.CreateServer(logHelper, app);
							server.Initialize().Wait();
							return server;
						});
					});
				}
				catch (Exception ex)
				{
					logHelper.LogError("Error happens when starting NewSchedulers Program.", ex);
					throw;
				}
			}
		}

		static IAuthorizationHelper CreateAuthorizationHelper(IStagingRepository stagingRepo, IAccessTokenProvider accessTokenProvider)
		{
			var safeRepo = new SafeRepository(new Uri(ConfigurationProvider.SafeUpdateServiceUri), accessTokenProvider: accessTokenProvider);
			return new AuthorizationHelper(safeRepo, stagingRepo, ConfigurationProvider.CheckAuthorization);
		}

		static ISchedulerValidator CreateSchedulerValidator(IStagingRepository stagingRepo)
		{
			return new SchedulerValidator(stagingRepo);
		}

		[Authorize]
		static Task Request(HttpContext context)
		{
			return Task.CompletedTask;
		}
	}
}
