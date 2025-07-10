using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.Testing.Common;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace CargoWise.Blazor.HostedAppRuntime.Test
{
	public class DatabaseConfigTests
	{
		[Test]
		public void TestSettingDatabaseViaHeader()
		{
			var config = new TestConfiguration();
			var appServer = TestAppServerProcess.FromTestContext(config);
			var args = appServer.GetCommandLineArguments("example.wtg.zone Odyssey", Path.GetTempPath(), null);
			Assert.That(args["--CargoWiseOptions:DbServerName"], Is.EqualTo("example.wtg.zone"));
			Assert.That(args["--CargoWiseOptions:DatabaseName"], Is.EqualTo("Odyssey"));
		}

		[Test]
		public void TestHeaderOverridesConfig()
		{
			var config = new ConfigurationBuilder()
				.AddInMemoryCollection(new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("CargoWiseOptions:DbServerName", "server_from_config"),
					new KeyValuePair<string, string>("CargoWiseOptions:DatabaseName", "db_from_config"),
				})
				.Build();
			var appServer = TestAppServerProcess.FromTestContext(config);
			var args = appServer.GetCommandLineArguments("server_from_header db_from_header", Path.GetTempPath(), null);
			Assert.That(args["--CargoWiseOptions:DbServerName"], Is.EqualTo("server_from_header"));
			Assert.That(args["--CargoWiseOptions:DatabaseName"], Is.EqualTo("db_from_header"));
		}

		[Test]
		public void TestSettingDatabaseFromConfig()
		{
			var config = new ConfigurationBuilder()
				.AddInMemoryCollection(new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("CargoWiseOptions:DbServerName", "server_from_config"),
					new KeyValuePair<string, string>("CargoWiseOptions:DatabaseName", "db_from_config"),
				})
				.Build();
			var appServer = TestAppServerProcess.FromTestContext(config);
			var args = appServer.GetCommandLineArguments(null, Path.GetTempPath(), null);
			Assert.That(args["--CargoWiseOptions:DbServerName"], Is.EqualTo("server_from_config"));
			Assert.That(args["--CargoWiseOptions:DatabaseName"], Is.EqualTo("db_from_config"));
		}

		[Test]
		public void TestNoHeaderOrConfigUsesConfigDefaults()
		{
			var config = new ConfigurationBuilder().Build();
			var appServer = TestAppServerProcess.FromTestContext(config);
			var args = appServer.GetCommandLineArguments(null, Path.GetTempPath(), null);
			Assert.That(args["--CargoWiseOptions:DbServerName"], Is.EqualTo(Environment.MachineName));
			Assert.That(args["--CargoWiseOptions:DatabaseName"], Is.EqualTo("Odyssey"));
		}

		[Test]
		public void TestInvalidHeaderFails()
		{
			var config = new TestConfiguration();
			var appServer = TestAppServerProcess.FromTestContext(config);
			Assert.Throws<ConfigurationErrorsException>(() => appServer.GetCommandLineArguments("invalid_header", Path.GetTempPath(), null)); // header value must contain space between server and db
		}
	}
}
