using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using XH.Framework.Deployment.XT.Controllers;

namespace XT.IntegrationTesting.Tasks
{
	public class Shared
	{
		public static ConfigurationManager ConfigurationManager { get; set; } = new ConfigurationManager();

		static void LoadProfile(string path, ConfigurationManager configurationManager, bool shouldThrow = true)
		{
			if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
			{
				var content = File.ReadAllText(path);
				try
				{
					configurationManager.LoadProfile(content);
				}
				catch (JsonException ex)
				{
					throw new InvalidDataException("Profile configuration should be in JSON.", ex);
				}
			}
			else if (shouldThrow)
			{
				throw new ArgumentException($"File path should be provided and the file must exist.");
			}
		}

		public static void LoadAllProfiles(string defaultProfileFile, string profileFile)
		{
			ConfigurationManager = new ConfigurationManager();
			LoadProfile(defaultProfileFile, ConfigurationManager);
			LoadProfile(profileFile, ConfigurationManager, false);
		}

		public static DeployManager CreateDeployManager(string workingDirectory, string interfaceVersion)
		{
			if (!string.IsNullOrWhiteSpace(workingDirectory)) ConfigurationManager.Profile.WorkingDirectory = workingDirectory;
			if (!string.IsNullOrWhiteSpace(interfaceVersion)) ConfigurationManager.Profile.InterfaceVersion = interfaceVersion;
			var deployManager = new DeployManager(ConfigurationManager);
			return deployManager;
		}

		public static BuildManager CreateBuildManager(string workingDirectory, IBuildEngine buildEngine)
		{
			if (!string.IsNullOrWhiteSpace(workingDirectory)) ConfigurationManager.Profile.WorkingDirectory = workingDirectory;
			return new BuildManager(ConfigurationManager)
			{
				BuildProject = (path, component) =>
				{
					new MSBuild()
					{
						Projects = new[] { new TaskItem(path) },
						UnloadProjectsOnCompletion = true,
						Targets = new[] { "Clean", "Restore", "Build" },
						BuildEngine = buildEngine
					}.Execute();
				}
			};
		}

		public static void LogResult(bool success, TaskLoggingHelper log)
		{
			if (success)
			{
				log.LogMessage(MessageImportance.High, "\tDone.");
			}
			else
			{
				log.LogMessage(MessageImportance.High, "\tFailed.");
			}
		}
	}
}
