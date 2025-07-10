using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CargoWise.Blazor.Client.Integration;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.SessionBroker.StaticFiles;
using CargoWise.Blazor.Testing.Common;
using CargoWiseNext.Infrastructure.Installations;
using CargoWiseNext.Infrastructure.Installations.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Serilog;

namespace CargoWise.Blazor.SessionBroker.Test
{
	public class CustomWebApplicationFactory<TStartup> : AddPreActionApplicaionFactory<TStartup> where TStartup : class
	{
		const string MsixVersion = "0.0.0.0";

		internal const string DefaultUserAgentHeaderValue = RequestHeaders.ClientAppUserAgentPrefix + "/" + MsixVersion;

		public bool AddDefaultUserAgentHeader { get; set; } = true;

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.ConfigureServices(services =>
			{
				var existingAppServerProcessDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(AppServerProcess));
				services.Remove(existingAppServerProcessDescriptor);
				services.Configure<CargoWiseOptions>(c =>
				{
					c.VersionBrokerProcessCorrelationId ??= Guid.NewGuid();
				});

				services.AddTransient<AppServerProcess, TestAppServerProcess>(serviceProvider =>
				{
					var config = serviceProvider.GetRequiredService<IConfiguration>();
					var logPath = WithAttachedAppServerLogsAttribute.LogPath;
					return new TestAppServerProcess(config, logPath);
				});

				var mockAppInstallerHandler = new Mock<IAppInstallerHandler>();
				mockAppInstallerHandler.Setup(h => h.GetAppInstallerInfo()).Returns(new AppInstallerInfo(Constants.MsixClientPackageName, "WiseTech", new Version(MsixVersion)));
				services.AddSingleton(mockAppInstallerHandler.Object);
				services.AddSingleton<IInstallerPaths>(InstallerPathsForTest.Instance);
				services.AddSingleton(UrlHandlerProviderMock.GetObject());
			});
			base.ConfigureWebHost(builder);
		}

		protected override void ConfigureClient(HttpClient client)
		{
			if (AddDefaultUserAgentHeader)
			{
				client.DefaultRequestHeaders.Add(RequestHeaders.ClientAppUserAgent, DefaultUserAgentHeaderValue);
			}

			base.ConfigureClient(client);
		}
	}
	public class AddPreActionApplicaionFactory<T> : WebApplicationFactory<T> where T : class
	{
		public readonly List<Action<IWebHostBuilder>> PreBuildActions = new List<Action<IWebHostBuilder>>();
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			foreach (var action in PreBuildActions)
			{
				action(builder);
			}
			base.ConfigureWebHost(builder);
		}
		protected override IHostBuilder CreateHostBuilder()
		{
			// we don't want tests writing to disk or Kafka so this effectively suppresses logging
			return base.CreateHostBuilder().AddScopedEnvironmentVariablesInDevelopment().UseSerilog((_, __) => { });
		}
	}
}
