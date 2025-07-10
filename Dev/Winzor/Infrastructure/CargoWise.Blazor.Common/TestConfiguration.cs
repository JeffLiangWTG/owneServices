using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace CargoWise.Blazor.Common
{
	// The blazor app (CargoWise.Winzor.AppServer.exe) uses the .NET Core configuration abstraction to load config from files,
	// command line args, environment variables, etc - which is then loaded into the ASP.NET Core dependency injection system.
	// Many of the tests rely on using this configuration, but can't create an IConfiguration object the same way because they
	// don't do app startup.  Here is an implementation that mirrors what ASP.NET Core does as closely as possible.
	// Command line arguments are omitted because tests aren't run with command line arguments.
	public class TestConfiguration : IConfiguration
	{
		public TestConfiguration()
		{
			wrappedConfig = new ConfigurationBuilder()
				.AddBlazorCommon()
				.Build();
		}

		readonly IConfiguration wrappedConfig;

		public string this[string key] { get => wrappedConfig[key]; set => wrappedConfig[key] = value; }

		public IEnumerable<IConfigurationSection> GetChildren() => wrappedConfig.GetChildren();

		public IChangeToken GetReloadToken() => wrappedConfig.GetReloadToken();

		public IConfigurationSection GetSection(string key) => wrappedConfig.GetSection(key);
	}
}
