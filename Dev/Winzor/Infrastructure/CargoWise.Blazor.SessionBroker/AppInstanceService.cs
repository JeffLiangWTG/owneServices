//#define CAPTURE_BLAZOR_OUTPUT
// uncomment above (and WTG.DevTools.Common AssemblyReference in csproj) and the server will capture stdout/stderr from the launched blazor app, and upload to DAT artifact repository
// see WI00399117 for task to remove dep and capture log output in test logs

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using CargoWiseNext.Infrastructure.Authentication;
using CargoWiseNext.Infrastructure.Instance.Management;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Configuration;
#if CAPTURE_BLAZOR_OUTPUT
using WTG.DevTools.Common;
#endif

namespace CargoWise.Blazor.SessionBroker
{
	public class AppInstanceService
	{
		public AppInstanceService(IProxyConfigProvider reverseProxyConfigProvider, ILogger<AppInstanceService> logger, AppServerProcess appServerProcess, ISecureSecretGenerator secureSecretGenerator, SessionSecretStore sessionSecretStore, IOptions<CargoWiseOptions> cargoWiseOptions)
		{
			this.reverseProxyConfigProvider = reverseProxyConfigProvider as InMemoryConfigProvider;
			this.logger = logger;
			this.appServerProcess = appServerProcess;
			this.secureSecretGenerator = secureSecretGenerator;
			this.sessionSecretStore = sessionSecretStore;
			this.cargoWiseOptions = cargoWiseOptions;
		}

		readonly InMemoryConfigProvider reverseProxyConfigProvider;
		readonly ILogger<AppInstanceService> logger;
		readonly AppServerProcess appServerProcess;
		readonly ISecureSecretGenerator secureSecretGenerator;
		readonly SessionSecretStore sessionSecretStore;
		readonly IOptions<CargoWiseOptions> cargoWiseOptions;

		public string CreateWithStmAccessToken(string databaseConfigHeader, string clientToken, string clientIdentifier)
		{
			logger.LogDebug($"CargoWise.Winzor.AppServer instance creating");
			var versionBrokerProcessCorrelationId = cargoWiseOptions.Value.VersionBrokerProcessCorrelationId;
			var sessionBrokerProcessCorrelationId = cargoWiseOptions.Value.SessionBrokerProcessCorrelationId.Value;
			var hostname = cargoWiseOptions.Value.Hostname;
			AppServerProcess appServer;
			var sessionToken = secureSecretGenerator.Generate();

			try
			{
				appServer = appServerProcess.CreateWithStmAccessToken(databaseConfigHeader, clientToken, sessionToken, cargoWiseOptions.Value.AppServerPathOverride ?? BuildFileSystem.AppServerBin.PublishExePath, versionBrokerProcessCorrelationId, sessionBrokerProcessCorrelationId, clientIdentifier, hostname: hostname);
			}
			catch (Exception ex)
			{
				logger.LogError($"CargoWise.Winzor.AppServer instance creation failed: {ex}");
				throw;
			}

			return AddNodeToCluster(appServer, sessionToken, appServer.UniqueId, appServer.ProcessId, appServer.Address);
		}

		public string CreateWithOIDCAuthCookie(
			string databaseConfigHeader,
			CargoWiseAuthCookie cargoWiseAuthCookie,
			string clientIdentifier,
			string persist = null,
			bool isNetCore = false)
		{
			logger.LogDebug($"CargoWise.Winzor.AppServer instance creating");
			var versionBrokerProcessCorrelationId = cargoWiseOptions.Value.VersionBrokerProcessCorrelationId;
			var sessionBrokerProcessCorrelationId = cargoWiseOptions.Value.SessionBrokerProcessCorrelationId.Value;
			var hostname = cargoWiseOptions.Value.Hostname;
			AppServerProcess appServer;
			var sessionToken = secureSecretGenerator.Generate();

			try
			{
				appServer = appServerProcess.CreateWithOIDCAuthCookie(
					databaseConfigHeader: databaseConfigHeader,
					sessionToken: sessionToken,
					appServerBinExePath: cargoWiseOptions.Value.AppServerPathOverride ?? BuildFileSystem.AppServerBin.PublishExePath,
					cargoWiseAuthCookie: cargoWiseAuthCookie,
					versionBrokerProcessCorrelationId: versionBrokerProcessCorrelationId,
					sessionBrokerProcessCorrelationId: sessionBrokerProcessCorrelationId,
					clientIdentifier: clientIdentifier,
					persist: persist,
					hostname: hostname,
					isNetCore: isNetCore);
			}
			catch (Exception ex)
			{
				logger.LogError($"CargoWise.Winzor.AppServer instance creation failed: {ex}");
				throw;
			}

			return AddNodeToCluster(appServer, sessionToken, appServer.UniqueId, appServer.ProcessId, appServer.Address);
		}

		string AddNodeToCluster(AppServerProcess appServer, string sessionToken, string uniqueId, int processId, string address)
		{
			if (!string.IsNullOrWhiteSpace(uniqueId))
			{
				logger.LogInformation($"CargoWise.Winzor.AppServer instance created, adding to cluster (uniqueId: {uniqueId}, processId: {processId})");
				var appServerJobObject = new Job();
				appServerJobObject.AddProcess(appServer.Process);
				var existingClusterId = reverseProxyConfigProvider.GetConfig().Clusters.Single().ClusterId;
				reverseProxyConfigProvider.AddNodeToCluster(existingClusterId, uniqueId, address, processId);
				sessionSecretStore[uniqueId] = sessionToken;
				appServer.Process.Exited += (_, _) => RemoveOnExit(appServer, existingClusterId, appServerJobObject);
				return appServer.UniqueId;
			}

			logger.LogError($"CargoWise.Winzor.AppServer instance created without UniqueId");
			return null;
		}

		void RemoveOnExit(AppServerProcess appServer, string existingClusterId, Job appServerJobObject)
		{
			logger.LogInformation($"CargoWise.Winzor.AppServer instance exited, removing from cluster (uniqueId: {appServer.UniqueId}, processId: {appServer.ProcessId})");
			reverseProxyConfigProvider.RemoveNodeFromCluster(existingClusterId, appServer.UniqueId);
			sessionSecretStore.Remove(appServer.UniqueId, out _);
			appServerJobObject.Dispose();
		}
	}
}
