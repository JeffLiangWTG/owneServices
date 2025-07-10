using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Deployment.Test
{
	[TestFixture]
	public class DeployScriptFixture
	{
		[Test]
		public void CheckTransformIsIncludedForProjectsWithConfigFiles()
		{
			var deployPROFiles = Directory.GetFiles(startDirectory, $"{ConfigFileName}*", SearchOption.AllDirectories);
			Assert.Greater(deployPROFiles.Length, 1);
			deployPROFiles = deployPROFiles.Select(x => Path.GetDirectoryName(x).Replace(startDirectory, string.Empty)).ToArray();
			var projectMissingTransform = new StringBuilder();
			foreach (var deployPROFile in deployPROFiles)
			{
				if (exceptionsWithoutTransform.Any(x => deployPROFile.Contains(x, StringComparison.OrdinalIgnoreCase)))
				{
					continue;
				}
				if (!transformScriptLines.Any(x => x.Contains(deployPROFile, StringComparison.OrdinalIgnoreCase)))
				{
					projectMissingTransform.AppendLine(deployPROFile);
				}
			}
			Assert.AreEqual(string.Empty, projectMissingTransform.ToString(), "No transform in deploy.ps1 for the following project(s): " + projectMissingTransform.ToString());
		}

		[Test]
		public void CheckNoTransformIsRunOnIncorrectFolder()
		{
			var binFolder = currentDirectory.Replace(@"\Tools\net8.0", string.Empty);
			var validDirectories = Directory.GetDirectories(binFolder);
			var validFolders = validDirectories.Select(x => $@"$binPath\deploy{x.Replace(binFolder, string.Empty)}").ToList();

			foreach (var transformLine in transformScriptLines)
			{
				if (!validFolders.Any(x => transformLine.Contains(x, StringComparison.OrdinalIgnoreCase)))
				{
					Assert.Fail($@"Incorrect folder set for transformation on the following line: {transformLine}

binFolder:
{binFolder}

validFolders:
{string.Join(Environment.NewLine , validFolders)}");
				}
			}
		}

		[Test]
		public void CheckTransformProjectsAndConfigs()
		{
			foreach (var transformLine in transformScriptLines)
			{
				var startIndex = transformLine.IndexOf("$sourcePath") + 12;
				var projectPath = transformLine.Substring(startIndex);
				var projectFilePath = Path.Combine(startDirectory, projectPath);
				var configFilePath = Directory.GetFiles(Path.GetDirectoryName(projectFilePath), $"{ConfigFileName}*");
				if (!File.Exists(projectFilePath))
				{
					Assert.Fail($"Invalid Transform, project does not exist: {projectPath}");
				}
				if (configFilePath.Length == 0)
				{
					Assert.Fail($"{projectPath} has transform but does not have {ConfigFileName} file");
				}
				if (transformLine.StartsWith("TransformJson "))
				{
					Assert.True(configFilePath.Any(x => x.EndsWith($"{ConfigFileName}.json")), $"TransformJson should be used with {ConfigFileName}.json file: {transformLine}");
				}
				else
				{
					Assert.True(configFilePath.Any(x => x.EndsWith($"{ConfigFileName}")), $"Transform should be used with {ConfigFileName} file: {transformLine}");
				}
			}
		}

		[Test]
		public void CheckTurnApplicationsOfflineBeforeDbUpgrade()
		{
			var upgradeDatabaseLines = new[] { @"& ""$binPath\deploy\Server\net8.0\CargoWise.RefDbRepo.Service.UpgradeManagerRunner.exe""",
			@"& ""$binPath\deploy\Staging\net8.0\CargoWise.RefDbRepo.Staging.DbUpgrader.exe""" };

			var recycleStopAppLines = powershellScriptLines.Where(x => x.Trim().StartsWith("RecycleAppPool", StringComparison.OrdinalIgnoreCase) && x.Trim().EndsWith(@"""StopAppPool""", StringComparison.OrdinalIgnoreCase));
			var higherRecycleLineNumber = 0;
			var lowerUpgradeDatabaseLineNumber = 0;
			for (var lineNum = 0; lineNum < powershellScriptLines.Length; lineNum++)
			{
				var lineInfo = powershellScriptLines[lineNum];
				if (recycleStopAppLines.Contains(lineInfo))
				{
					higherRecycleLineNumber = lineNum;
				}
				else if (lowerUpgradeDatabaseLineNumber == 0 && upgradeDatabaseLines.Any(x => lineInfo.Contains(x)))
				{
					lowerUpgradeDatabaseLineNumber = lineNum;
				}
			}
			Assert.That(higherRecycleLineNumber, Is.GreaterThan(0));
			Assert.That(lowerUpgradeDatabaseLineNumber, Is.GreaterThan(0));
			Assert.That(higherRecycleLineNumber, Is.LessThan(lowerUpgradeDatabaseLineNumber));
		}

		const string ConfigFileName = "deploy.PRO.config";
		string currentDirectory;
		string startDirectory;
		string[] powershellScriptLines;
		IEnumerable<string> transformScriptLines;
		readonly string[] exceptionsWithoutTransform = new[] { "Bin", "Service\\NewService", "Service\\SafeDataUpdateService\\NewSafeDataUpdateService", "\\Staging\\PushNotification\\PollingService" };

		[OneTimeSetUp]
		public void SetUp()
		{
			currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			startDirectory = TestSourcePathHelper.DATTestSourcePath;
			powershellScriptLines = File.ReadAllLines(currentDirectory + "\\Deploy.ps1");
			transformScriptLines = powershellScriptLines.Where(x => x.StartsWith("Transform ", StringComparison.OrdinalIgnoreCase) || x.StartsWith("TransformJson ", StringComparison.OrdinalIgnoreCase));
		}
	}
}
