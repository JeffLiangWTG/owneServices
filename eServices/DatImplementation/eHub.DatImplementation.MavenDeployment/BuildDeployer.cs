using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dat.Integration;
using Dat.Integration.Deployment;

namespace eHub.DatImplementation.MavenDeployment
{
	public class BuildDeployer : IBuildDeployer
	{
		readonly ITaskLogger taskLogger;

		readonly Lazy<SecureStorage> secureStorage;

		readonly static string[] ErrorOutputLines = new[]
		{
			@"^BUILD FAILED\.$",
			@"^.*: ERROR :.*$",
			@"^.*: ERROR MSB\d+:.*$",
			@"^\s+[1-9]\d*\s+ERROR\(s\)$"
		};

		public BuildDeployer(ITaskLogger taskLogger)
		{
			this.taskLogger = taskLogger;
			secureStorage = new Lazy<SecureStorage>();
		}

		public void DeployOnDemand(string _, string deploymentConfiguration, string sourcePath, string binPath)
		{
			using (var task = taskLogger.RecordTask($"DeployOnDemand: {deploymentConfiguration}"))
			{
				taskLogger.RecordInfo(
					$"DeployOnDemand: deploymentConfiguration: {deploymentConfiguration}, sourcePath: {sourcePath}, binPath: {binPath}");

				foreach (var kv in new Dictionary<string, string> { { "SOURCEPATH", sourcePath }, { "BINPATH", binPath } })
				{
					Environment.SetEnvironmentVariable(kv.Key, kv.Value);
				}
				var config = MavenDeploymentConfig.Parse(ResolveEnvironmentVariables(deploymentConfiguration));

				var batName = config.SettingsByPrefix("BAT");
				if (string.IsNullOrEmpty(batName))
				{
					throw new NotSupportedException($"No BAT found from the DeploymentConfiguration: {deploymentConfiguration}");
				}
				var workingDirectory = ExpandBatPackage(batName);
				var pomFilePath = $@"{workingDirectory}\pom.xml";
				if (!File.Exists(pomFilePath))
				{
					throw new FileNotFoundException($"No pom file found for BAT {batName} in path {pomFilePath}");
				}

				var pomXDocument = XDocument.Load(pomFilePath);
				var profileName = config.SettingsByPrefix("Profile");
				if (!string.IsNullOrEmpty(profileName) && !PomFileParser.CheckPomFileForProfileName(pomXDocument, profileName))
				{
					throw new NotSupportedException($"No profile information found within the project pom.xml for profile: {profileName}");
				}
				var password = PomFileParser.GetPasswordFromPomXDocument(pomXDocument, profileName);
				if (string.IsNullOrEmpty(password))
				{
					throw new ArgumentException($"Cannot find password in pom.xml");
				}
				password = TryDecryptPassword(password);

				var commandName = config.SettingsByPrefix("Command");
				if (string.IsNullOrEmpty(commandName))
				{
					throw new NotSupportedException($"No Command found from the DeploymentConfiguration: {deploymentConfiguration}");
				}

				var exeFilePath = "cmd.exe";
				var args = new List<string> { "/c ", commandName, $"-Dtomcat_password={password}" };
				if (!string.IsNullOrEmpty(profileName))
				{
					args.Add($"-P {profileName}");
				}

				var processTask = Task.Run(async () =>
					await StartProcessAsync(taskLogger, workingDirectory, exeFilePath, args));
				if (processTask.Result != 0)
				{
					throw new Exception($"Build failed: {deploymentConfiguration}");
				}
			}
		}

		string TryDecryptPassword(string encryptedPassword)
		{
			try
			{
				return secureStorage.Value.Decrypt(encryptedPassword);
			}
			catch
			{
				return encryptedPassword;
			}
		}

		static string ExpandBatPackage(string packageFilePath)
		{
			if (!File.Exists(packageFilePath))
			{
				throw new FileNotFoundException(packageFilePath);
			}

			var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			ZipFile.ExtractToDirectory(packageFilePath, tempDir);
			return tempDir;
		}

		static async Task<int> StartProcessAsync(ITaskLogger logger, string workingDirectory, string exeFilePath, List<string> args)
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

		static string ResolveEnvironmentVariables(string text)
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

		public void AutoDeployLatestBuild(string buildConfiguration, string deploymentConfiguration, string sourcePath, string binPath)
		{
			throw new NotImplementedException();
		}

		public void AutoDeployTestedShelf(string buildConfiguration, string deploymentConfiguration, string sourcePath, string binPath, TaskInfo taskInfo)
		{
			throw new NotImplementedException();
		}

		public void TeardownTestedShelf(string buildConfiguration, string deploymentConfiguration, TaskInfo taskInfo)
		{
			throw new NotImplementedException();
		}
	}
}
