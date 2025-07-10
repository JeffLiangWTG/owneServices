using System;
using System.Collections.Specialized;
using System.IO;
using CargoWise.Blazor.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace CargoWise.Blazor.Testing.Common
{
	public class TestAppServerProcess : AppServerProcess
	{
		readonly string logPath;

		public TestAppServerProcess(IConfiguration config, string logPath) : base(config, NullLogger<AppServerProcess>.Instance)
		{
			this.logPath = logPath;
		}

		public static TestAppServerProcess FromTestContext(IConfiguration config)
		{
			return new TestAppServerProcess(config, WithAttachedAppServerLogsAttribute.LogPath);
		}

		protected override void SetEnvironmentVariables(StringDictionary environment)
		{
			environment["Serilog__Using__1"] = "Serilog.Sinks.File";
			environment["Serilog__WriteTo__1__Name"] = "File";
			var file = Path.Combine(logPath, $"AppServer_{DateTimeOffset.Now:yyyyMMdd-HHmmss.fff}_" + Path.GetRandomFileName() + ".log");
			environment["Serilog__WriteTo__1__Args__path"] = file;
			environment["Serilog__WriteTo__1__Args__outputTemplate"] = "[{Timestamp:HH:mm:ss} {Level:u3} (PID: {ProcessId})] {Message:lj}{NewLine}{Exception}";
		}
	}
}
