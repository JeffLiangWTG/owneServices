using System.Linq;
using CargoWise.Blazor.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace CargoWise.Blazor.AppServer.Test
{
	public class StartupTests
	{
		[Test]
		public void ForwardedHeadersOptionsAreConfigured()
		{
			var configuration = new ConfigurationBuilder().Build();
			var sc = new ServiceCollection();
			new Startup(configuration, null).ConfigureServices(sc);
			var configureOptions = sc.Where(sd => sd.ServiceType == typeof(IConfigureOptions<ForwardedHeadersOptions>)).Select(sd => sd.ImplementationInstance).OfType<IConfigureOptions<ForwardedHeadersOptions>>();
			var options = new ForwardedHeadersOptions();
			foreach (var configureDelegate in configureOptions)
			{
				configureDelegate.Configure(options);
			}
			Assert.That(options.ForwardLimit, Is.Null);
			Assert.That(options.KnownNetworks.Last().PrefixLength, Is.EqualTo(0));
			Assert.That(options.KnownNetworks.Last().Prefix.ToString(), Is.EqualTo("0.0.0.0"));
		}

		public static IHostBuilder CreateTestHostBuilder() =>
			Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configurationBuilder =>
				{
					configurationBuilder.AddBlazorCommon();
				})
				.ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
	}
}
