using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Text;
using System.Xml.Linq;
using Dat.Integration;
using eServices.BuildTools.DatDeploy;

namespace eServices.BuildTools.DatTool
{
	internal class Program
	{
		static async Task<int> Main(string[] args)
		{
			var initCommand = new Command("init", "Initialize DAT build.");
			var outputOption = new Option<DirectoryInfo>(new[] { "--output", "-o" }, () => new DirectoryInfo("Bin\\DatTool"), "Output folder for build items.") { IsRequired = false };
			initCommand.Add(outputOption);
			var frameworkOption = new Option<string?>(new[] { "--framework" }, "Specific target framework to initialize for.") { IsRequired = false };
			initCommand.Add(frameworkOption);
			initCommand.SetHandler(InitCommandHandler, outputOption, frameworkOption);

			var deployCommand = new Command("deploy", "Execute deployments.");
			var deployTargetArg = new Argument<string>("target", "Deployment target name.");
			deployCommand.AddArgument(deployTargetArg);
			var workingDirOption = new Option<DirectoryInfo>("--working-dir", () => new DirectoryInfo("."), "Working directory.") { IsRequired = false };
			deployCommand.AddOption(workingDirOption);
			var binDirOption = new Option<DirectoryInfo>("--bin-dir", "Binaries directory. [default: <working-dir>\\Bin]") { IsRequired = false };
			deployCommand.AddOption(binDirOption);
			deployCommand.SetHandler((deployTargetArgValue, workingDirOptionValue, binDirOptionValue) =>
			{
				try
				{
					var deployer = new Deployer(new TaskLogger());
					deployer.Deploy(deployTargetArgValue, workingDirOptionValue.FullName, binDirOptionValue?.FullName ?? Path.Combine(workingDirOptionValue.FullName, "Bin"));
				}
				catch (Exception ex) when (ex is not DeployerException)
				{
					throw new DeployerException("Deployment has failed. See inner exception:", ex);
				}
			}, deployTargetArg, workingDirOption, binDirOption);

			var rootCommand = new RootCommand("eService DAT Tool")
			{
				initCommand,
				deployCommand
			};

			var parser = new CommandLineBuilder(rootCommand)
				.UseDefaults()
				.UseExceptionHandler((exception, context) =>
				{
					var originalColor = Console.ForegroundColor;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.Error.WriteLine(exception switch
					{
						DeployerException and { InnerException: null }
							=> $"\nERROR: {exception.Message}\n",
						DeployerException and { InnerException: not null }
							=> $"\nERROR: {exception.Message}\n{exception.InnerException}\n",
						_ => exception.ToString()
					});
					Console.ForegroundColor = originalColor;
					context.ExitCode = 1;
				}).Build();

			return await parser.InvokeAsync(args);
		}

		internal static void InitCommandHandler(DirectoryInfo outputOptionValue, string? frameworkOptionValue)
		{
			var toolDir = new DirectoryInfo(AppContext.BaseDirectory);
			DirectoryInfo? contentDir;
			while ((contentDir = toolDir.GetDirectories("content").FirstOrDefault()) == null)
				toolDir = toolDir.Parent ?? throw new InvalidOperationException("Content folder not found.");
			if (!string.IsNullOrEmpty(frameworkOptionValue))
			{
				contentDir = contentDir.GetDirectories(frameworkOptionValue).FirstOrDefault() ?? throw new InvalidOperationException("Invalid framework specified.");
			}

			if (!outputOptionValue.Exists) { outputOptionValue.Create(); }
			var source = contentDir.FullName;
			var target = outputOptionValue.FullName;
			if (!string.IsNullOrEmpty(frameworkOptionValue))
			{
				foreach (string newPath in Directory.GetFiles(source, "*.*"))
					try { File.Copy(newPath, newPath.Replace(source, target), false); } catch { }
				target = Directory.CreateDirectory(Path.Combine(target, "DatTool")).FullName;
			}
			foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
				Directory.CreateDirectory(dirPath.Replace(source, target));
			foreach (string newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
				File.Copy(newPath, newPath.Replace(source, target), true);

			Console.WriteLine($"Copied DAT build files to '{target}'.");

			var buildFile = new FileInfo("Build.xml");
			var buildDeployFile = new FileInfo("Build.Deploy.xml");
			if (buildFile.Exists)
			{
				XNamespace nsb = "http://wisetechglobal.com/DevTools/Build.xsd";
				XNamespace nsd = "http://eservices.buildtools/datdeploy";
				var buildXml = XElement.Load(buildFile.FullName);
				var datSettings = buildXml.Element(nsb + "DatSettings");
				var deployerType = datSettings?.Attribute("BuildDeployerType")?.Value;
				if (deployerType != null && Type.GetType(deployerType) == typeof(Deployer))
				{
					var buildTargets = datSettings!.Element(nsb + "DeploymentTargets")?.Elements(nsb + "DeploymentTarget") ?? Enumerable.Empty<XElement>();
					var deployTargets = XElement.Load(buildDeployFile.FullName)?.Element(nsd + "Targets")?.Elements(nsd + "Target") ?? Enumerable.Empty<XElement>();
					var missingDeploy = buildTargets.Where(b => !deployTargets.Any(d => d.Attribute("Name")?.Value == b.Attribute("DeploymentConfiguration")?.Value)).ToList();
					if (missingDeploy.Count > 0)
					{
						var missingDeployMsg = new StringBuilder().AppendLine("Found DeploymentTargets in Build.xml with no matching Target in Build.Deploy.xml:");
						missingDeploy.ForEach(d => missingDeployMsg.Append("   ").AppendLine(d.Attribute("DeploymentConfiguration")?.Value));
						var missingDeployText = missingDeployMsg.ToString();
						Console.Error.Write(missingDeployText);
						throw new InvalidOperationException(missingDeployText);
					}
				}
			}
		}
	}
}