using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using CargoWise.Definitions;
using CargoWiseNext.Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WTG.Logging.Extensions;

namespace CargoWise.Blazor.Common
{
	public class AppServerProcess
	{
		readonly ILogger<AppServerProcess> logger;

		public AppServerProcess(IConfiguration config, ILogger<AppServerProcess> logger)
		{
			this.config = config;
			this.logger = logger;
		}

		readonly IConfiguration config;

		public string Address { get; private set; }
		public string UniqueId { get; private set; }
		public int ProcessId => Process.Id;
		public Process Process { get; private set; }

		public AppServerProcess CreateWithStmAccessToken(
			string databaseConfigHeader,
			string clientToken,
			string sessionToken,
			string appServerBinExePath,
			Guid? versionBrokerProcessCorrelationId,
			Guid sessionBrokerProcessCorrelationId,
			string clientIdentifier,
			string hostname = null)
		{
			var authOptions = new CargoWiseAuthOptions()
			{
				ClientToken = clientToken,
				SessionToken = sessionToken,
			};

			return InitializingAppServerProcess(
				cargoWiseAuthOptions: authOptions,
				databaseConfigHeader: databaseConfigHeader,
				appServerBinExePath: appServerBinExePath,
				versionBrokerProcessCorrelationId: versionBrokerProcessCorrelationId,
				sessionBrokerProcessCorrelationId: sessionBrokerProcessCorrelationId,
				clientIdentifier: clientIdentifier,
				hostname: hostname);
		}

		public AppServerProcess CreateWithOIDCAuthCookie(
			string databaseConfigHeader,
			string sessionToken,
			string appServerBinExePath,
			CargoWiseAuthCookie cargoWiseAuthCookie,
			Guid? versionBrokerProcessCorrelationId,
			Guid sessionBrokerProcessCorrelationId,
			string clientIdentifier,
			string persist = null,
			string hostname = null,
			bool isNetCore = false)
		{
			var authOptions = new CargoWiseAuthOptions
			{
				IdentityToken = cargoWiseAuthCookie.IdentityToken,
				SessionToken = sessionToken,
			};

			return InitializingAppServerProcess(
				cargoWiseAuthOptions: authOptions,
				databaseConfigHeader: databaseConfigHeader,
				appServerBinExePath: appServerBinExePath,
				versionBrokerProcessCorrelationId: versionBrokerProcessCorrelationId,
				sessionBrokerProcessCorrelationId: sessionBrokerProcessCorrelationId,
				clientIdentifier: clientIdentifier,
				persist: persist,
				hostname: hostname,
				isNetCore: isNetCore);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Uses ProcessStartInfo")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		AppServerProcess InitializingAppServerProcess(
			CargoWiseAuthOptions cargoWiseAuthOptions,
			string databaseConfigHeader,
			string appServerBinExePath,
			Guid? versionBrokerProcessCorrelationId,
			Guid sessionBrokerProcessCorrelationId,
			string clientIdentifier,
			string persist = null,
			string hostname = null,
			bool isNetCore = false)
		{
			if (cargoWiseAuthOptions.SessionToken is null)
			{
				logger.Enrich()
					.WithAlert(LogEventCategory.Session, LogEventOutcome.Failure)
					.LogWarning("No session token provided on initialization");
				throw new ArgumentNullException(nameof(cargoWiseAuthOptions), "Session token cannot be null.");
			}

			logger.LogDebug("Initiating app server process");
			var appServer = new AppServerProcess(config, logger);
			var binPath = Path.GetFullPath(Path.Combine(Assembly.GetExecutingAssembly().Location, "..", ".."));
			var appServerExePath = Path.Combine(Path.Combine(binPath, appServerBinExePath));
#pragma warning disable EDI011 // CW1 Temp Path Rule
			var tempFilePath = config["BlazorAppProcInfoDirectory"] ?? Path.GetTempPath();
#pragma warning restore EDI011 // CW1 Temp Path Rule
			var eventName = Guid.NewGuid().ToString();
			var argumentsDictionary = appServer.GetCommandLineArguments(databaseConfigHeader, tempFilePath, eventName, persist, hostname: hostname);
			var arguments = config["AdditionalArguments"] + " " + string.Join(" ", argumentsDictionary.Select(kvp => $"{kvp.Key} {kvp.Value}"));
			var psi = new ProcessStartInfo
			{
				Arguments = arguments,
				FileName = appServerExePath,
				WorkingDirectory = Path.GetDirectoryName(appServerExePath),
				RedirectStandardOutput = false,
				RedirectStandardError = false,
				RedirectStandardInput = true
			};

			logger.LogDebug("Setting environment variables");
			if (versionBrokerProcessCorrelationId != null)
			{
				psi.EnvironmentVariables["CargoWiseOptions:VersionBrokerProcessCorrelationId"] =
					versionBrokerProcessCorrelationId.ToString();
			}
			psi.EnvironmentVariables["CargoWiseOptions:SessionBrokerProcessCorrelationId"] = sessionBrokerProcessCorrelationId.ToString();
			psi.EnvironmentVariables["CargoWiseOptions:ClientIdentifier"] = clientIdentifier;
			if (isNetCore)
			{
				psi.EnvironmentVariables["WINZOR_USE_NETCORE_LIBS"] = true.ToString();
			}
			SetEnvironmentVariables(psi.EnvironmentVariables);
			appServer.Process = new Process()
			{
				StartInfo = psi
			};

			using var ewh = new EventWaitHandle(false, EventResetMode.ManualReset, eventName);

			Exception procFileCheckException = null;

			// don't wait for timeout if appserver exits before setting the wait handle itself
			EventHandler procStartFailedHandler = (s, e) =>
			{
				try
				{
					// NOTE: Race condition exists. When the calling thread is between WaitOne() and the removal of this handler from the Exited EventHandler,
					// the CargoWise.Winzor.AppServer process can exit such that this handler starts on a second thread. The first thread can
					// subsequently dispose the EventWaitHandle object before the handler has finished
					// NOTE: Exceptions may go unobserved and crash the process
					if (s is Process process)
					{
						logger.LogWarning($"AppServer exited unexpectedly! {process.ExitCode}");
					}

					ewh.Set();
				}
				catch (ObjectDisposedException ode)
				{
					logger.LogError($"Object disposed exception {ode}");
				}
			};

			EventHandler procExitedHandler = (s, _) =>
			{
				if (s is Process process)
				{
					logger.LogInformation($"Application exited with code {process.ExitCode}");
					Thread.Sleep(5000);
					if (process.ExitCode == ExitCodes.DatabaseUpgraded)
					{
						logger.LogInformation("Application exited due to database upgrade!");
						Environment.Exit(ExitCodes.DatabaseUpgraded);
					}
					else if (process.ExitCode == ExitCodes.VersionUpgraded)
					{
						logger.LogInformation("Application exited due to version upgrade!");
						Environment.Exit(ExitCodes.VersionUpgraded);
					}
				}
			};

			appServer.Process.EnableRaisingEvents = true;
			appServer.Process.Exited += procStartFailedHandler;
			appServer.Process.Exited += procExitedHandler;

			try
			{
				logger.LogDebug($"Start {appServerExePath}...");
				appServer.Process.Start();

				appServer.Process.StandardInput.WriteLine(JsonConvert.SerializeObject(new { CargoWiseAuthOptions = cargoWiseAuthOptions }));
				appServer.Process.StandardInput.Close();
				logger.LogDebug($"Process started {appServerExePath}");

				var timeoutSeconds = config.GetValue("AppServerProcessTimeoutSeconds", 600);
				logger.LogInformation("Waiting for launch signal");

				if (ewh.WaitOne(timeoutSeconds * 1000))
				{
					logger.LogInformation($"CargoWise.Winzor.AppServer started (StartNotifier) (ProcessId: {appServer.Process.Id})");
					try
					{
						var path = Path.Combine(tempFilePath, appServer.Process.Id.ToString(CultureInfo.InvariantCulture) + ".json");
						using var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read | FileShare.Write | FileShare.Delete);
						var root = JsonDocument.Parse(stream).RootElement;
						appServer.Address = root.GetProperty(nameof(Address)).GetString();
						appServer.UniqueId = root.GetProperty(nameof(UniqueId)).GetString();
						logger.LogInformation($"CargoWise.Winzor.AppServer started (ProcessId: {appServer.ProcessId})");
						return appServer;
					}
					catch (Exception e)
					{
						logger.LogError(e, $"Error handling CargoWise.Winzor.AppServer post-start");
						procFileCheckException = e;
					}
				}
				else
				{
					logger.LogError($"Timeout waiting for CargoWise.Winzor.AppServer startup");
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, $"Process start failed");
				// failure to start process, take opportunity to log and rethrow for caller
				throw;
			}
			finally
			{
				appServer.Process.Exited -= procStartFailedHandler;
			}

			// if we failed to get info about the new process, kill it so we don't leak the system resources
			logger.LogInformation("Killing app server");
			appServer.Process.Kill();
			appServer.Process.Dispose();

			throw new ProcessStartException($"Failure during CargoWise.Winzor.AppServer startup ({(procFileCheckException?.Message ?? "no proc file exception")})", procFileCheckException);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1058:DoNotUseDebuggerIsAttached", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected virtual void SetEnvironmentVariables(StringDictionary environment)
		{
			// further overridden for testing to set bespoke logging configuration

			if (Debugger.IsAttached)
			{
				environment["CARGOWISE_APPSERVER_LAUNCHDEBUGGERONSTART"] = "true";
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public Dictionary<string, string> GetCommandLineArguments(string databaseHeader, string tempPath, string eventName, string persist = null, string hostname = null)
		{
			var result = new Dictionary<string, string>()
			{
				["--urls"] = "http://127.0.0.1:0",
				["--webroot"] = ".\\wwwroot",
				["--BlazorAppProcInfoDirectory"] = FormatDirectory(tempPath),
				["--SignalEventWhenStarted"] = eventName,
				["--CargoWiseOptions:ReadConfigFromStdIn"] = true.ToString(),
			};

			if (!string.IsNullOrEmpty(hostname))
			{
				result["--CargoWiseOptions:Hostname"] = hostname;
			}

			if (!string.IsNullOrEmpty(databaseHeader))
			{
				// header value should be "servername databasename", so there should be exactly one space
				var parts = databaseHeader.Split(' ');
				if (parts.Length != 2)
				{
					throw new ConfigurationErrorsException("The database header is not as expected");
				}

				result["--CargoWiseOptions:DbServerName"] = parts[0];
				result["--CargoWiseOptions:DatabaseName"] = parts[1];
			}
			else
			{
				var cwOptions = ConfigurationBinder.Get<CargoWiseOptions>(config.GetSection("CargoWiseOptions")) ?? new CargoWiseOptions();
				result["--CargoWiseOptions:DbServerName"] = cwOptions.DbServerName;
				result["--CargoWiseOptions:DatabaseName"] = cwOptions.DatabaseName;
			}

			if (!string.IsNullOrEmpty(persist))
			{
				result["--CargoWiseOptions:Persist"] = persist;
			}

			return result;
		}

		static string FormatDirectory(string path)
		{
			// if the path ends in '\"', it causes weird problems with command line argument parsing because it escapes the closing quote.
			// Solution is to make sure that the path does not end in \ before doing anything
			path = path.TrimEnd(Path.DirectorySeparatorChar);

			if (path.Contains(' ', StringComparison.OrdinalIgnoreCase))
			{
				path = "\"" + path + "\"";
			}

			return path;
		}
	}
}
