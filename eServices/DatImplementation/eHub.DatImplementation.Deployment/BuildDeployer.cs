using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dat.Integration;
using Dat.Integration.Deployment;
using Microsoft.Build.Evaluation;

namespace eHub.DatImplementation.Deployment
{
	public class BuildDeployer : IBuildDeployer
	{
		#region Member variables

		const string DeployProjFileName = "Deploy.proj";
		const string NuGetRegexPattern = @"^BATNuGet=.*\.nupkg$";

		readonly ITaskLogger taskLogger;
		readonly INugetInfo nugetInfo;

		public static string[] ErrorOutputLines = new[] {
			@"^BUILD FAILED\.$",
			@"^.*: ERROR :.*$",
			@"^.*: ERROR MSB\d+:.*$",
			@"^\s+[1-9]\d*\s+ERROR\(s\)$"
		};

		public const string ManualDeploymentString = "BAT=Backup deployment binaries for manual deployment";

		#endregion

		#region Constructor

		public BuildDeployer(ITaskLogger taskLogger) : this(taskLogger, NugetInfo.Instance) { }

		public BuildDeployer(ITaskLogger taskLogger, INugetInfo nugetInfo)
		{
			this.taskLogger = taskLogger;
			this.nugetInfo = nugetInfo;
			lazySecureStorage = new Lazy<SecureStorage>(CreateSecureStorage);
		}

		SecureStorage CreateSecureStorage()
		{
			return new SecureStorage();
		}
		readonly Lazy<SecureStorage> lazySecureStorage;

		#endregion

		#region Methods

		public void AutoDeployLatestBuild(string buildConfiguration, string deploymentConfiguration, string sourcePath, string binPath)
		{
			Deploy(buildConfiguration, deploymentConfiguration, sourcePath, binPath);
		}

		public void AutoDeployTestedShelf(string buildConfiguration, string deploymentConfiguration, string sourcePath, string binPath, TaskInfo taskInfo)
		{
			throw new NotImplementedException();
		}

		public void DeployOnDemand(string buildConfiguration, string deploymentConfiguration, string sourcePath, string binPath)
		{
			if (deploymentConfiguration.Trim() != ManualDeploymentString)
			{
				Deploy(buildConfiguration, deploymentConfiguration, sourcePath, binPath);
			}
		}

		public virtual void Deploy(string buildConfiguration, string deploymentConfiguration, string sourcePath, string binPath, [CallerMemberName] string callerName = "")
		{
			using (var task = taskLogger.RecordTask(callerName))
			{
				taskLogger.RecordInfo($"buildConfiguration: {buildConfiguration}, deploymentConfiguration: {deploymentConfiguration}, sourcePath: {sourcePath}, binPath: {binPath}");

				foreach (var kv in new Dictionary<string, string> { { "SOURCEPATH", sourcePath }, { "BINPATH", binPath } })
				{
					Environment.SetEnvironmentVariable(kv.Key, kv.Value);
				}

				var config = eHubDeploymentConfig.Parse(ResolveEnvionmentalVariables(deploymentConfiguration));

				if (Regex.IsMatch(deploymentConfiguration, NuGetRegexPattern, RegexOptions.IgnoreCase))
				{
					NugetDeploy(config, deploymentConfiguration);
				}
				else
				{
					MsBuildDeploy(config, buildConfiguration, deploymentConfiguration, sourcePath, binPath);
				}
			}
		}

		void MsBuildDeploy(eHubDeploymentConfig config, string buildConfiguration, string deploymentConfiguration, string sourcePath, string binPath)
		{
			var commandLineProps = config.SettingsByPrefix(eHubDeploymentConfig.Prefix.Property);
			config.AddKeyValue("p:Configuration", buildConfiguration);

			var projectFilePath = string.Empty;
			var cleanUpWorkingDirectory = false;
			if (config.Settings.ContainsKey(eHubDeploymentConfig.Keys.BAT))
			{
				cleanUpWorkingDirectory = true;
				projectFilePath = ExpandBatPackage(config.Settings[eHubDeploymentConfig.Keys.BAT]);
			}
			else if (config.Settings.ContainsKey(eHubDeploymentConfig.Keys.Project))
			{
				projectFilePath = config.Settings[eHubDeploymentConfig.Keys.Project];
			}
			else
			{
				throw new NotSupportedException($"No supported project found from the DeploymentConfiguration: {deploymentConfiguration}");
			}

			if (!File.Exists(projectFilePath))
			{
				throw new FileNotFoundException(projectFilePath);
			}

			const ProjectLoadSettings loadSettings = ProjectLoadSettings.IgnoreMissingImports;
			var msbuildProject = new Project(projectFilePath, config.SettingsByPrefix(eHubDeploymentConfig.Prefix.Property), "Current", new ProjectCollection(), loadSettings);

			var targets = new List<string>(config.SettingsByPrefix(eHubDeploymentConfig.Prefix.Target).Keys);
			if (!targets.Any())
			{
				targets.Add(eHubDeploymentConfig.Target.DatDeploy);
			}

			var workingDirectory = Path.GetDirectoryName(projectFilePath);
			var exeFilePath = QuoteQuote(Path.Combine(msbuildProject.GetProperty("MSBuildBinPath").EvaluatedValue, "msbuild.exe"));
			var propertyArgs = ConstructArgs("/p:", CommandLineProps(commandLineProps).Concat(SourceBinPath(sourcePath, binPath).Concat(DecryptPasswords(msbuildProject))));
			var targetArgs = ConstructArgs("/t:", targets);

			var args = new[] { QuoteQuote(projectFilePath), propertyArgs, targetArgs };
			var processTask = Task.Run(async () => await StartProcessAsync(taskLogger, workingDirectory, exeFilePath, args));

			if (cleanUpWorkingDirectory)
			{
				processTask.ContinueWith((t) =>
				{
					Directory.Delete(workingDirectory, true);
				});
			}

			if (0 != processTask.Result)
			{
				throw new Exception($"Build failed: {deploymentConfiguration}");
			}
		}

		void NugetDeploy(eHubDeploymentConfig config, string deploymentConfiguration)
		{
			var concurrentDict = new ConcurrentDictionary<string, (int exitCode, string output)>();
			var nupkgPath = config.Settings[eHubDeploymentConfig.Keys.BATNuGet];
			var files = Directory.GetFiles(Path.GetDirectoryName(nupkgPath), Path.GetFileName(nupkgPath));

			if (files.Length==0)
			{
				throw new FileNotFoundException($@"No deployment files could be found. Please check the following ""DeploymentConfiguration"" attribute in Build.xml against the path in the downloadable build or the local build: ""{deploymentConfiguration}""");
			}

			Parallel.ForEach(files, file =>
			{
				var exitCode = 0;
				var stringBuilder = new StringBuilder();
				using (var nugetPush = new Process())
				{
					nugetPush.StartInfo.FileName = "dotnet";
					nugetPush.StartInfo.Arguments = $@"nuget push ""{file}"" --api-key {nugetInfo.ApiKey} --source {nugetInfo.PackageSource} --skip-duplicate";
					nugetPush.StartInfo.UseShellExecute = false;
					nugetPush.StartInfo.RedirectStandardOutput = true;

					nugetPush.OutputDataReceived += (sender, e) =>
					{
						if (!string.IsNullOrEmpty(e.Data))
						{
							stringBuilder.AppendLine(e.Data);
						}
					};

					nugetPush.Start();
					nugetPush.BeginOutputReadLine();
					if (nugetPush.WaitForExit((int)TimeSpan.FromMinutes(5).TotalMilliseconds))
					{
						nugetPush.WaitForExit();
					}
					exitCode = nugetPush.ExitCode;
				}

				var output = stringBuilder.ToString();
				taskLogger.RecordInfo(output);
				concurrentDict.TryAdd(file, (exitCode, output));
			});

			var error = concurrentDict.Where(pair => pair.Value.exitCode != 0 && !pair.Value.output.Contains("already exists at feed"));
			if (error.Any())
			{
				throw new Exception($@"""nuget push"" command failed...
{error.Aggregate(new StringBuilder(), (sb, pair) => sb.AppendFormat(@"Exit Code {0} returned from {1}.
", pair.Value.exitCode, Path.GetFileName(pair.Key)))}For more details, please check the output.");
			}
		}

		public string ExpandBatPackage(string packageFilePath)
		{
			if (!File.Exists(packageFilePath))
			{
				throw new FileNotFoundException(packageFilePath);
			}

			var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			ZipFile.ExtractToDirectory(packageFilePath, tempDir);
			return Path.Combine(tempDir, DeployProjFileName);
		}

		public static string ResolveEnvionmentalVariables(string text)
		{
			var output = text;
			var regex = new Regex(@"\$\((?<variable>.*?)\)");
			var match = regex.Match(output);
			while (match.Success)
			{
				var variable = match.Groups["variable"].Value;
				output = output.Replace($"$({variable})", Environment.GetEnvironmentVariable(variable));
				match = match.NextMatch();
			}

			return output;
		}

		public void TeardownTestedShelf(string buildConfiguration, string deploymentConfiguration, TaskInfo taskInfo)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Helpers

		static string QuoteQuote(string text)
		{
			return $"\"{text}\"";
		}

		string DecryptPassword(string encryptedPassword)
		{
			try
			{
				return lazySecureStorage.Value.Decrypt(encryptedPassword);
			}
			catch
			{
				return encryptedPassword;
			}
		}

		static string ConstructArgs(string type, IEnumerable<string> args)
		{
			return type + string.Join(";", args);
		}

		static List<string> SourceBinPath(string sourcePath, string binPath)
		{
			var args = new List<string>
			{
				$"{eHubDeploymentConfig.Keys.SourcePath}={QuoteQuote(sourcePath)}",
				$"{eHubDeploymentConfig.Keys.BinPath}={QuoteQuote(binPath)}"
			};

			return args;
		}

		static List<string> CommandLineProps(Dictionary<string, string> props)
		{
			var args = new List<string>();

			foreach (var prop in props)
			{
				args.Add($"{prop.Key}={QuoteQuote(prop.Value)}");
			}

			return args;
		}

		List<string> DecryptPasswords(Project msbuildProject)
		{
			var passwordProperties = new[] { "_Password", "Password", "AccountPassword" };

			return (
				from passswordKey
				in passwordProperties
				let encryptedPassword = msbuildProject.GetProperty(passswordKey)
				where encryptedPassword != null
				select $"{passswordKey}={QuoteQuote(DecryptPassword(encryptedPassword.EvaluatedValue))}"
			).ToList();
		}

		static async Task<int> StartProcessAsync(ITaskLogger logger, string workingDirectory, string exeFilePath, string[] args)
		{
			var exitCode = -1;

			try
			{
				using (var process = new Process())
				{
					var arguments = string.Join(" ", args);
					logger.RecordInfo($"Executing: {exeFilePath} {arguments}");

					var startInfo = new ProcessStartInfo
					{
						WorkingDirectory = workingDirectory,
						CreateNoWindow = true,
						FileName = exeFilePath,
						Arguments = arguments,
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						UseShellExecute = false,
						Verb = "runas"
					};

					process.StartInfo = startInfo;
					process.Start();

					var outputErrors = await LogStreamReaderAsync(logger, process.StandardOutput);
					var errors = await LogStreamReaderAsync(logger, process.StandardError, true);
					errors.AddRange(outputErrors);

					process.WaitForExit();
					exitCode = process.ExitCode;

					if (errors.Any())
					{
						throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
					}

					return exitCode;
				}
			}
			catch (Exception ex)
			{
				logger.RecordInfo($"Error: {ex.Message}");
			}

			logger.RecordInfo($"exitCode: {exitCode}");
			return exitCode;
		}

		static async Task<List<string>> LogStreamReaderAsync(ITaskLogger logger, StreamReader streamReader, bool isStandardErrorStream = false)
		{
			var errors = new List<string>();
			var line = string.Empty;
			while ((line = await streamReader.ReadLineAsync()) != null)
			{
				if (!string.IsNullOrWhiteSpace(line))
				{
					logger.RecordInfo(line);

					if (isStandardErrorStream ||
						ErrorOutputLines.Any(pattern => Regex.Match(line, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase).Success))
					{
						errors.Add(line);
					}
				}
			}

			return errors;
		}

		#endregion
	}
}
