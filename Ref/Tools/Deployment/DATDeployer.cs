using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using CargoWise.RefDbRepo.Common.Argument;
using Dat.Integration;
using Dat.Integration.Deployment;
using WTG.DevTools.Common;

namespace CargoWise.RefDbRepo.Deployment
{
	public class DATDeployer : IBuildDeployer, IBuildDeployer2
	{
		public DATDeployer(ITaskLogger taskLogger)
		{
			logger = taskLogger;
		}

		TestRig _testRig;
		readonly ITaskLogger logger;

		public void AutoDeployLatestBuild(string buildConfiguration, string deploymentConfiguration, string sourcePath, string binPath)
		{
			throw new NotImplementedException();
		}

		public void AutoDeployTestedShelf(string buildConfiguration, string deploymentConfiguration, string sourcePath, string binPath, TaskInfo taskInfo)
		{
			var options = new TestedShelfDeploymentOptions(taskInfo);
			if (!string.IsNullOrEmpty(options.WebSiteName))
			{
				Log("---------------- TestRig ready for creation ----------------");
				Log($"Name: {options.WebSiteName}");
				Log($"RefDbRepoSafe backup: {options.RefDbRepoSafeRestoreFromBackup}");
				Log($"RefDbRepoStaging backup: {options.RefDbRepoStagingRestoreFromBackup}");
				_testRig = new TestRig(options);
				Log("---------------- TestRig restoring db from backup ----------------");
				Log($"Restoring on DB Server: {options.SqlServer}");
				_testRig.CreateDbAndRestoreFromBackupIfNeeded();

				//create delivery service website
				Log("Creating service websites");
				RunPowerShellScript(binPath, "CreateServiceWebSites.ps1", ps =>
				{
					ps.AddParameter("ComputerName", options.WebServer);
					ps.AddParameter("DeliveryServiceName", options.DeliveryServiceDomain);
					ps.AddParameter("UpdateServiceName", options.UpdateServiceDomain);
					ps.AddParameter("UpdateServicePhysicalFolder", options.UpdateServiceFilesLocalPath);
					ps.AddParameter("DeliveryServicePhysicalFolder", options.DeliveryServiceFilesLocalPath);
					if (!string.IsNullOrEmpty(options.DeployUsername) && !string.IsNullOrEmpty(options.DeployPassword))
					{
						ps.AddParameter("UserName", options.DeployUsername);
						ps.AddParameter("Password", options.DeployPassword);
					}
				});
				Log("Finished creating websites");
			}
			if (!string.IsNullOrEmpty(options.WebSiteName) || !string.IsNullOrEmpty(options.RefDataRepo))
			{
				Log("Starting test rig deployment");
				var env = options.RefDataRepo.ToUpper(CultureInfo.InvariantCulture);
				switch (env)
				{
					case "TEST":
					case "ALPHA":
					case "BETA":
					case "GAMMA":
					case "DELTA":
					case "EPSILON":
					case "ZETA":
					case "ETA":
					case "THETA":
						DeployOnDemand(buildConfiguration, env, sourcePath, binPath);
						break;
				}
				return;
			}
			// throwing this exception will inform DAT that the deployment is cancelled and it will not show the rocket symbol.
			throw new DeploymentCancelledException("There is no information about TestRig. Cancel the deployment");
		}

		public void DeployOnDemand(string buildConfiguration, string deploymentConfiguration, string sourcePath, string binPath)
		{
			deploymentConfiguration = deploymentConfiguration.ToUpperInvariant();
			var msbuildLocation = VisualStudioHelpers.GetMSBuildPath(VisualStudioVersion.VisualStudio2022, Architecture.x64);
			var msdeploy = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "iis", "Microsoft Web Deploy V3", "msdeploy.exe");
			var parameterCreationStrategies = ParameterCreationStrategyFactory.GetStrategies(deploymentConfiguration);
			foreach(var parameterCreationStrategy in parameterCreationStrategies)
			{
				var parameters = parameterCreationStrategy.CreateParameters(_testRig);

				parameters.Add(("sourcePath", sourcePath));
				parameters.Add(("binPath", binPath));
				parameters.Add(("msbuild", msbuildLocation));
				parameters.Add(("publishProfile", deploymentConfiguration));
				parameters.Add(("msdeploy", msdeploy));

				var connectionStringJsonConfigFileFullPath = Path.Combine(sourcePath, connectionStringJsonConfigFilePath);
				var connectionStringJsonConfigDestinationFoldersPath = connectionStringJsonConfigDestinationFolders.Select(x => Path.Combine(binPath, x))
					.Append(Path.GetDirectoryName(connectionStringJsonConfigFileFullPath));
				parameters.Add(("connectionStringJsonConfigFilePath", connectionStringJsonConfigFileFullPath));
				parameters.Add(("connectionStringJsonConfigDestinationFolders", connectionStringJsonConfigDestinationFoldersPath.ToArray()));
				Deploy(binPath, parameters);
			}
		}

		void Deploy(string binPath, List<(string paramName, object paramValue)> parameters)
		{
			Argument.NotNull(parameters, nameof(parameters));
			Argument.NotNullOrEmpty(binPath, nameof(binPath));

			RunPowerShellScript(binPath, "Deploy.ps1", ps =>
			{
				foreach (var (paramName, paramValue) in parameters)
				{
					ps.AddParameter(paramName, paramValue);
				}
			});
		}

		void Log(string message)
		{
			logger?.RecordInfo($"{DateTime.Now}: {message}");
		}

		[Obsolete("Use the IBuildDeployer2.TeardownTestedShelf overload specifying sourcePath and binPath")]
		public void TeardownTestedShelf(string buildConfiguration, string deploymentConfiguration, TaskInfo taskInfo)
		{
		}

		public void TeardownTestedShelf(string buildConfiguration, string deploymentConfiguration, string sourcePath, string binPath, TaskInfo taskInfo)
		{
			var options = new TestedShelfDeploymentOptions(taskInfo);
			//teardown must only be executed when we are creating new websites. Not in the default UA0 deployment.
			if (!string.IsNullOrEmpty(options.WebSiteName))
			{
				Log("Deleting websites");
				RunPowerShellScript(binPath, "DeleteServiceWebSites.ps1", ps =>
				{
					ps.AddParameter("ComputerName", options.WebServer);
					ps.AddParameter("UpdateServiceName", options.UpdateServiceDomain);
					ps.AddParameter("DeliveryServiceName", options.DeliveryServiceDomain);
					if (!string.IsNullOrEmpty(options.DeployUsername) && !string.IsNullOrEmpty(options.DeployPassword))
					{
						ps.AddParameter("UserName", options.DeployUsername);
						ps.AddParameter("Password", options.DeployPassword);
					}
				});

				Log("Deleting TestRig files");
				if (Directory.Exists(options.DeliveryServiceFilesSharedPath))
				{
					Directory.Delete(options.DeliveryServiceFilesSharedPath, true);
				}
				if (Directory.Exists(options.UpdateServiceFilesSharedPath))
				{
					Directory.Delete(options.UpdateServiceFilesSharedPath, true);
				}

				Log("Dropping databases");
				_testRig = new TestRig(options);
				_testRig.DropDatabases();
			}
		}

		void RunPowerShellScript(string binPath, string fileName, Action<PowerShell> setParameter)
		{
			var errorBuilder = new PowerShellScriptErrorBuilder();
			var psScriptCreator = new PowerShellScriptCreator(binPath, fileName, errorBuilder, logger);
			using (var pspInstance = new PowerShellProcessInstance(new Version(5, 1), null, null, true))
			using (var runSpace = RunspaceFactory.CreateOutOfProcessRunspace(null, pspInstance))
			{
				runSpace.Open();
				using (var powerShell = psScriptCreator.Create(runSpace))
				{
					setParameter(powerShell);
					psScriptCreator.Invoke(powerShell);
				}
			}
		}

		readonly string connectionStringJsonConfigFilePath = @"Common\Infrastructure\Utils\ConnectionStrings.config.json";
		readonly string[] connectionStringJsonConfigDestinationFolders = new[] { @"Server\net8.0\", @"Staging\net8.0\", @"StagingService\net8.0\", @"UniversalXMLProducers\net8.0\" };
	}
}
