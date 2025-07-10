using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.Data.SqlProxy.Interface;
using CargoWise.Data.SqlProxyServer.Server;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace CargoWise.Data.SqlProxyServer;

[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer")]
class Program
{
	[SuppressMessage("CargoWiseOne", "CW1106:DebugMessages")]
	public static async Task Main(string[]? args)
	{
		try
		{
			if (args == null || args.Length < 3)
			{
				await Console.Error.WriteLineAsync(
					"Usage: CargoWise.Data.SqlProxyServer.exe <port> <serverName> <databaseName>");
				return;
			}

			var port = args[0];
			var serverName = args[1];
			var databaseName = args[2];
			var cts = new CancellationTokenSource();

			ApplicationContext.Initialize(serverName, databaseName);
			RegisterCommandLineHandler(cts);

			// create a mutex to ensure only one instance of the service is running for the same server and database
			var glowLoaderServiceMutex = GlobalMutex.GetSqlProxyServiceMutex(serverName, databaseName);
			var mutexTask = glowLoaderServiceMutex.AcquireMutexAsync(cts.Token);

			ConfigureLogging(serverName, databaseName);

			// process monitoring tasks to exit when all client processes are closed
			var parentProcessMonitorTask = SqlProxyServerListener.StartParentProcessMonitorAsync(cts);

			// start the server
			using var server = await SqlProxyApi.StartServerOverHttpAsync(new Core.SqlProxy(), int.Parse(port));
			Console.WriteLine(FormattableString.Invariant($"{port} {serverName} {databaseName}"));

			var listenerTask =
				SqlProxyServerListener.StartClientConnectionsListenerAsync(serverName, databaseName, port, cts.Token);

			Environment.SetEnvironmentVariable("SqlProxy_ServerName", $"{SqlProxyNamingConvention.NormalizedInstanceName(serverName)}", EnvironmentVariableTarget.Process);
			Environment.SetEnvironmentVariable("SqlProxy_DatabaseName", $"{databaseName}", EnvironmentVariableTarget.Process);
			Environment.SetEnvironmentVariable("SqlProxy_Port", port, EnvironmentVariableTarget.Process);

			await Task.WhenAll(mutexTask, parentProcessMonitorTask, listenerTask);
		}
		catch (OperationCanceledException)
		{
			// ignored
		}
		catch (Exception exception)
		{
			LogManager.GetCurrentClassLogger()?.Error(exception);
			Console.Error.WriteLine(exception);
		}
	}

	static void RegisterCommandLineHandler(CancellationTokenSource cts)
	{
		// Handle Ctrl-C and shutdown signals
		Console.CancelKeyPress += (_, eventArgs) =>
		{
			cts.Cancel();
			eventArgs.Cancel = true;
		};

		AppDomain.CurrentDomain.ProcessExit += (_, __) =>
		{
			cts.Cancel();
		};
	}

	[SuppressMessage("CargoWiseOne", "CW1061:Do not use System.DateTime.UtcNow Rule")]
	static void ConfigureLogging(string serverName, string databaseName)
	{
		var logDirectory = CommonProgramData.GetCargoWiseDirectory("GlowServer", serverName, databaseName);
		if (!Directory.Exists(logDirectory))
		{
			Directory.CreateDirectory(logDirectory);
		}

		var config = new LoggingConfiguration();
		var fileTarget = new FileTarget("file")
		{
			FileName = $"{logDirectory}/GlowServer-{Process.GetCurrentProcess().Id}-{DateTime.UtcNow:yy-MM-dd}.log",
			Layout = "${longdate} [${level}] ${message}",
			ArchiveFileName = $"{logDirectory}/archives/GlowServer.{{#}}.log",
			ArchiveEvery = FileArchivePeriod.Day,
			ArchiveNumbering = ArchiveNumberingMode.Rolling,
			MaxArchiveFiles = 7,
		};

		config.AddTarget(fileTarget);

		var rule = new LoggingRule("*", LogLevel.Info, fileTarget);
		config.LoggingRules.Add(rule);

		LogManager.Configuration = config;
	}

	static Program()
	{
		// setup assembly resolver as we are using CW assemblies
		NetCoreAssemblyResolver.Setup();
	}
}
