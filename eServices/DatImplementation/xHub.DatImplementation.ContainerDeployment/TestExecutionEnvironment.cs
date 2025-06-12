using System.IO.Compression;
using System.Linq;
using Dat.Integration;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Locator;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;

namespace xHub.DatImplementation.ContainerDeployment
{
	public class TestExecutionEnvironment : ITestExecutionEnvironment
	{
		private readonly LoadBalancer loadBalancer;
		private readonly ILogger logger;
		private bool shouldExecute = true;

		public TestExecutionEnvironment()
		{
			if (MSBuildLocator.CanRegister)
			{
				MSBuildLocator.RegisterDefaults();
			}
			GetServerConfiguration(out var serverSettings, out logger);
			loadBalancer = new LoadBalancer(serverSettings);
		}

		private static void GetServerConfiguration(out ServerSettings serverSettings, out ILogger logger)
		{
			var config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile(Constants.AppSettingsFileName, optional: false, reloadOnChange: true)
				.Build();
			serverSettings = new ServerSettings();
			config
				.GetSection("ServerSettings")
				.Bind(serverSettings);
			logger = new LoggerConfiguration().ReadFrom.Configuration(config).CreateLogger();
		}

		private static IEnumerable<PackageFileInfo> GetRequiredPackages(TestExecutionEnvironmentContext context, string binPath, IntegrationTestEnvironments environments)
		{
			var packageFiles = environments.Environments
					.SelectMany(env => env.Deployments, (env, deployment) => new PackageFileInfo
					{
						ContainerName = env.Name,
						PackageName = deployment.Name,
						PackageFileName = deployment.FileName
					})
					.ToList();
			foreach (var zipFile in Directory.GetFiles(binPath, "*.zip", SearchOption.AllDirectories))
			{
				foreach (var package in packageFiles)
				{
					if (package.PackageFileName == Path.GetFileName(zipFile))
					{
						package.PackageFilePath = zipFile;
					}
				}
			}
			if (packageFiles.Any(package => string.IsNullOrEmpty(package.PackageFilePath)))
			{
				throw new InvalidOperationException("Missing deployment packages.");
			}
			return packageFiles;
		}

		private static void SaveDeploymentEnvironments(string binPath, IntegrationTestEnvironments environments)
		{
			try
			{
				var filePath = Path.Combine(binPath, Constants.IntegrationTestEnvironmentsFileName);
				File.WriteAllText(filePath, JsonConvert.SerializeObject(environments));
			}
			catch
			{
				throw new InvalidOperationException($"{Constants.IntegrationTestEnvironmentsFileName} cannot be saved in the BinPath");
			}
		}

		private IntegrationTestEnvironments GetDeploymentEnvironments(string sourcePath)
		{
			var config = new IntegrationTestEnvironments();
			new ConfigurationBuilder()
				.SetBasePath(sourcePath)
				.AddJsonFile(Constants.IntegrationTestEnvironmentsFileName, optional: true, reloadOnChange: true)
				.Build()
				.Bind(config);
			if (config.Environments.Count == 0)
			{
				shouldExecute = false;
			}
			return config;
		}

		private void RunMSBuild(string projectFile)
		{
			using var projectCollection = new ProjectCollection();
			var buildParameters = new BuildParameters(projectCollection);

			var buildRequest = new BuildRequestData(projectFile, new Dictionary<string, string?>(), null, new[] { "bat:build" }, null);
			var buildResult = BuildManager.DefaultBuildManager.Build(buildParameters, buildRequest);

			if (buildResult.OverallResult == BuildResultCode.Success)
			{
				logger.Information("Build Succeeded");
			}
			else
			{
				logger.Information("Build Failed");
				throw new InvalidOperationException($"Build for project {projectFile} Failed");
			}
		}

		private static string DetectProjectType(string projectFile)
		{
			using var projectCollection = new ProjectCollection();
			var project = projectCollection.LoadProject(projectFile);
			return project.GetPropertyValue("ProjectType");
		}

		private void RecreatePackages(string deploymentDirectory, IEnumerable<PackageFileInfo> packages)
		{
			if (!Directory.Exists(deploymentDirectory))
			{
				Directory.CreateDirectory(deploymentDirectory);
			}
			foreach (var package in packages)
			{
				using var zip = ZipFile.OpenRead(package.PackageFilePath);
				var targetPackageDirectory = Path.Combine(deploymentDirectory, package.PackageName);
				if (Directory.Exists(targetPackageDirectory))
				{
					Directory.Delete(targetPackageDirectory, true);
				}
				Directory.CreateDirectory(targetPackageDirectory);
				zip.ExtractToDirectory(targetPackageDirectory);
				var projectFile = Path.Combine(targetPackageDirectory, "Deploy.proj");
				var projectType = DetectProjectType(projectFile);
				if (projectType == "xT")
				{
					RunMSBuild(projectFile);
				}
				ZipFile.CreateFromDirectory(targetPackageDirectory, Path.Combine(deploymentDirectory, package.PackageFileName));
			}
		}

		public void Deploy(TestExecutionEnvironmentContext context)
		{
			if (!shouldExecute)
			{
				return;
			}
			logger.Information($"DeployOnDemand-{Guid.NewGuid()}");
			if (context == null)
			{
				throw new InvalidOperationException("TestExecutionEnvironmentContext is null: DAT Issue");
			}
			logger.Information("Deployment started.");
			var config = GetDeploymentEnvironments(context.BinPath);
			var requiredPackages = GetRequiredPackages(context, context.BinPath, config);
			var deploymentDirectory = Path.Combine(context.BinPath, Constants.TempDeploymentDirectoryName);
			RecreatePackages(deploymentDirectory, requiredPackages);
			try
			{
				logger.Information($"Start Deployments");
				foreach (var env in config.Environments)
				{
					logger.Information($"Start to install each Container");
					var targetFolderName = Guid.NewGuid().ToString("N");
					var excludedServers = new List<Server>();
					var isSuccessfullyDeployed = false;
					do
					{
						var (errorMessage, selectedServer) = loadBalancer.SelectRandomServerInstance(excludedServers);
						if (!string.IsNullOrEmpty(errorMessage))
						{
							throw new Exception(errorMessage);
						}

						try
						{
							var deploymentPackages = requiredPackages.Where(package => package.ContainerName == env.Name)
								.Select(package => Path.Combine(deploymentDirectory, package.PackageFileName))
								.ToList();
							var networkcopy = new NetworkCopy(logger);
							networkcopy.CopyFiles(deploymentPackages, selectedServer, targetFolderName);
							var deploymentServiceDeployments = new List<Deployment>();
							var deploymentRequest = new DeploymentRequest(env.Deployments,env.ServerInfo.DynamicPorts ,targetFolderName);
							logger.Information($"deployment request is - {JsonConvert.SerializeObject(deploymentRequest)}");
							var deploymentService = new DeploymentServiceAPICall(logger);
							(isSuccessfullyDeployed, var serverInfo) = deploymentService.SendDeploymentRequestAsync(deploymentRequest, selectedServer).GetAwaiter().GetResult();

							if (isSuccessfullyDeployed)
							{
								env.ServerInfo = serverInfo;
							}
							else
							{
								excludedServers.Add(selectedServer);
							}
						}
						catch (Exception ex)
						{
							logger.Information($"Warning on copy file or deployment - {ex.Message ?? "unknown Error"}");
							excludedServers.Add(selectedServer);
						}
					} while (!isSuccessfullyDeployed);
				}
				SaveDeploymentEnvironments(context.BinPath, config);
			}
			catch (Exception ex)
			{
				logger.Information($"Unknown Exception - {ex.Message ?? "Unknown"}");
				throw;
			}
			logger.Information("Deployment completed successfully.");
			Directory.Delete(deploymentDirectory, true);
		}


		public void Teardown(TestExecutionEnvironmentContext context)
		{
		}
	}
}

