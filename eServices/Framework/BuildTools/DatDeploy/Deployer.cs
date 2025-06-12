using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Dat.Integration;
using Dat.Integration.Deployment;
using Microsoft.Extensions.Logging;

namespace eServices.BuildTools.DatDeploy
{
	public class Deployer : IBuildDeployer
	{
		private readonly ILogger logger;
		private static readonly string assemblyLoadPath;

		static Deployer()
		{
			assemblyLoadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DatTool");
			if (Directory.Exists(assemblyLoadPath))
				AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
		}

		private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var assemblyPath = Path.Combine(assemblyLoadPath, args.Name.Remove(args.Name.IndexOf(',')) + ".dll");
			if (File.Exists(assemblyPath))
			{
				var assembly = Assembly.LoadFrom(assemblyPath);
				if (assembly.FullName == args.Name)
					return assembly;
			}
			return null;
		}

		public Deployer() : this(LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<Deployer>()) { }

		public Deployer(ITaskLogger taskLogger) : this(new DeployTaskLogger(taskLogger)) { }

		public Deployer(ILogger logger) => this.logger = logger;

		public void DeployOnDemand(string buildConfiguration, string deploymentConfiguration, string sourcePath, string binPath)
			=> Deploy(deploymentConfiguration, sourcePath, binPath);

		public void AutoDeployLatestBuild(string buildConfiguration, string deploymentConfiguration, string sourcePath, string binPath)
			=> Deploy(deploymentConfiguration, sourcePath, binPath, new DeployOptions { MissingEnvironmentErrorAction = ErrorAction.Stop });

		internal virtual void Deploy(string deploymentConfiguration, string sourcePath = "", string binPath = "", DeployOptions deployOptions = default)
		{
			if (string.IsNullOrEmpty(deploymentConfiguration))
				throw new ArgumentException($"'{nameof(deploymentConfiguration)}' cannot be null or empty.", nameof(deploymentConfiguration));

			logger.LogInformation($"Starting deploy for '{deploymentConfiguration}'.");
			var settings = new Dictionary<string, string>();
			var secretsData = new Dictionary<string, string>();
			var steps = new LinkedList<(int Id, string Description, string Command)>();

			XNamespace ns = "http://eservices.buildtools/datdeploy";
			var xn = new XmlNamespaceManager(new NameTable());
			xn.AddNamespace("ns", ns.ToString());
			XElement deployXml;

			try
			{
				settings["DAT_DEPLOYMENT_CONFIGURATION"] = deploymentConfiguration;
				settings["DAT_DEPLOYMENT_NAME"] = Regex.Replace(deploymentConfiguration, @"[^A-Za-z0-9]+", "_");
				settings["DAT_SOURCE_PATH"] = Path.GetFullPath(sourcePath);
				settings["DAT_BIN_PATH"] = Path.GetFullPath(binPath);
				settings["TIMESTAMP"] = DateTime.UtcNow.ToString("yyyyMMdd.HHmmssZ");

				using (var propsFileStream = OpenBuildToolsPropsFile(sourcePath))
				{
					if (propsFileStream != null)
					{
						var propsXml = XElement.Load(propsFileStream);
						foreach (var prop in propsXml.XPathSelectElements("//*[local-name()='PropertyGroup']/*"))
						{
							var name = Regex.Replace(prop.Name.LocalName, @"([^\p{Lu}])([\p{Lu}])", "$1_$2").ToUpperInvariant();
							settings[name] = prop.Value;
						}
					}
				}
				var buildEnvironment = settings.TryGetValue("BUILD_TOOLS_ENVIRONMENT", out string buildEnvironmentValue) ? buildEnvironmentValue : "Default";

				using (var deployFileStream = OpenDeployFile(sourcePath))
				{
					deployXml = XElement.Load(deployFileStream);
				}
				var targets = deployXml.Element(ns + "Targets").Elements(ns + "Target").Where(t => t.Attribute("Name").Value == deploymentConfiguration);
				if (!targets.Any()) throw new DeployerException("Deployment target not found for configuration.");
				var targetXml = targets.Where(t => t.Attribute("Environment") == null || t.Attribute("Environment").Value == buildEnvironment).FirstOrDefault();
				if (targetXml == null)
				{
					if (deployOptions?.MissingEnvironmentErrorAction == ErrorAction.Stop)
					{
						logger.LogInformation("Deployment not valid for current environment.");
						logger.LogInformation("Deployment stopped.");
						return;
					}
					else
					{
						throw new DeployerException("Deployment not valid for current environment.");
					}
				}

				foreach (var setting in deployXml.Element(ns + "Settings")?.Elements() ?? Enumerable.Empty<XElement>())
				{
					settings[setting.Attribute("Name").Value] = setting.Attribute("Value")?.Value ?? setting.Value;
				}

				foreach (var setting in targetXml.Element(ns + "Settings")?.Elements() ?? Enumerable.Empty<XElement>())
				{
					settings[setting.Attribute("Name").Value] = setting.Attribute("Value")?.Value ?? setting.Value;
				}

				if (targetXml.Attribute("Settings") != null)
				{
					foreach (Match match in Regex.Matches(targetXml.Attribute("Settings").Value, @"(?<name>[A-Za-z0-9_]+)=(?<value>[^;]*)"))
					{
						settings[match.Groups["name"].Value] = match.Groups["value"].Value;
					}
				}

				var expansionsMade = false;
				do
				{
					expansionsMade = false;
					foreach (var target in settings.ToList())
					{
						string expanded = ExpandValue(target.Value, settings);
						if (expanded != target.Value)
						{
							settings[target.Key] = expanded;
							expansionsMade |= true;
						}
					}
				} while (expansionsMade);

				if (targetXml.Attribute("Command") != null)
				{
					steps.AddLast((1, "Run Command", targetXml.Attribute("Command").Value));
				}
				else
				{
					int stepId = 1;
					foreach (var step in targetXml.Element(ns + "Steps").Elements())
					{
						steps.AddLast((stepId++, step.Attribute("Description")?.Value, step.Attribute("Command")?.Value ?? step.Value));
					}
				}

				foreach (var secret in deployXml.XPathSelectElements("/ns:Secrets/*", xn))
				{
					secretsData[secret.Attribute("Name").Value] = secret.Attribute("Value")?.Value ?? secret.Value;
				}
			}
			finally
			{
				logger.LogInformation("Settings:");
				foreach (var setting in settings)
				{
					logger.LogInformation("   {name}: {value}", setting.Key, setting.Value);
				}
				logger.LogInformation("");
			}

			var processStartInfo = new ProcessStartInfo
			{
				FileName = "cmd.exe",
				WorkingDirectory = sourcePath,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			var datEncryption = new SecureStorage();
			foreach (var variable in settings)
			{
				processStartInfo.EnvironmentVariables[variable.Key] = ExpandSecrets(variable.Value, secretsData);
			}

			foreach (var (Id, Description, Command) in steps)
			{
				using (var taskLogger = logger.BeginScope($"Step {Id} of {steps.Count}{(string.IsNullOrWhiteSpace(Description) ? $" [{Description}]" : "")}:"))
				{
					ExecuteStep(Id, $"DatDeploy.{settings["DAT_DEPLOYMENT_NAME"]}.{settings["TIMESTAMP"]}.{Id}.cmd", ExpandSecrets(ExpandValue(Command, settings), secretsData), processStartInfo);
				}
				logger.LogInformation("");
			}
		}

		private static string ExpandValue(string value, Dictionary<string, string> settings)
		{
			const int maxSettingLength = short.MaxValue;
			string expanded;
			string evaluator(Match m) => settings.TryGetValue(m.Groups["name"].Value, out var expansion) ? expansion : m.Value;
			while ((expanded = Regex.Replace(Environment.ExpandEnvironmentVariables(value), "%(?<name>[A-Za-z0-9_]+)%", evaluator))
				!= value && expanded.Length < maxSettingLength)
				value = expanded;
			return value;
		}

		private static string ExpandSecrets(string value, Dictionary<string, string> secretsData)
		{
			return Regex.Replace(value, @"\$\(Secrets:(?<secret>[A-Za-z0-9_]+)\)", (Match match) =>
			{
				var secret = match.Groups["secret"].Value;
				try
				{
					return datEncryption.Value.Decrypt(secretsData[secret]);
				}
				catch (Exception)
				{
					return secretsData[secret];
				}
			});
		}
		private static Lazy<SecureStorage> datEncryption = new Lazy<SecureStorage>();

		internal virtual void ExecuteStep(int id, string cmdFileName, string command, ProcessStartInfo processStartInfo)
		{
			var stepFile = new FileInfo(Path.Combine(Path.GetTempPath(), cmdFileName));
			try
			{
				File.AppendAllText(stepFile.FullName, command);
				processStartInfo.Arguments = $"/c {stepFile.FullName}";

				using (var process = new Process { StartInfo = processStartInfo })
				{
					process.OutputDataReceived += Process_DataReceived;
					process.ErrorDataReceived += Process_DataReceived;
					process.Start();
					process.BeginOutputReadLine();
					process.BeginErrorReadLine();
					process.WaitForExit();
					if (process.ExitCode != 0)
					{
						throw new DeployerException($"Deployment failed at step {id}. Exit code={process.ExitCode}");
					}
				}
				void Process_DataReceived(object sender, DataReceivedEventArgs e)
				{
					if (e.Data != null)
						logger.LogInformation(e.Data);
				}
			}
			finally
			{
				if (stepFile.Exists)
					stepFile.Delete();
			}
		}

		internal virtual Stream OpenDeployFile(string sourcePath)
			=> File.OpenRead(Path.Combine(sourcePath, "Build.Deploy.xml"))
				?? throw new FileNotFoundException($"Deploy file missing from '{sourcePath}'.", "Build.Deploy.xml");

		internal virtual Stream OpenBuildToolsPropsFile(string sourcePath)
		{
			FileInfo propsFile;
			var sourceDir = new DirectoryInfo(sourcePath);
			while ((propsFile = sourceDir.GetFiles("eServices.BuildTools.props").FirstOrDefault()) == null && sourceDir.Parent != null)
			{
				sourceDir = sourceDir.Parent;
			}
			if (propsFile != null)
			{
				return File.OpenRead(propsFile.FullName);
			}
			return null;
		}

		public void AutoDeployTestedShelf(string buildConfiguration, string deploymentConfiguration, string sourcePath, string binPath, TaskInfo taskInfo)
			=> throw new NotImplementedException();

		public void TeardownTestedShelf(string buildConfiguration, string deploymentConfiguration, TaskInfo taskInfo)
			=> throw new NotImplementedException();
	}
}
