using System;
using System.IO;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.Testing.Common;
using NUnit.Framework;

[assembly: WithAttachedAppServerLogs]

namespace CargoWise.Blazor.SessionBroker.Test
{
	[SetUpFixture]
	[AssemblySetup]
	public class SetUpTests
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			SetAspNetCoreEnvironment();
			SetContentRoot();
			SetTempPathForAddressFiles();
		}

		static void SetAspNetCoreEnvironment()
		{
			if (bool.TryParse(Environment.GetEnvironmentVariable("DAT_IS_TESTING"), out var onDat) && onDat)
			{
				Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "DAT", EnvironmentVariableTarget.Process);
			}
		}

		static void SetContentRoot()
		{
			// ASP.NET Core integration test host doesn't work without this on DAT because of the weird paths
			// Setting this is a shim between what DAT provides and what the integration test host needs
			// the variable is specific to this project so shouldn't affect anything else and doesn't need to be cleared
			// because it is scoped to the project/process, so should go away when the test run finishes
			var sourceRoot = Environment.GetEnvironmentVariable("DAT_TestSupplementaryContentPath");
			if (!string.IsNullOrEmpty(sourceRoot))
			{
				var settingName = $"ASPNETCORE_TEST_CONTENTROOT_{BuildFileSystem.SessionBrokerProject.ContentRootName}";
				var contentRoot = Path.Combine(sourceRoot, BuildFileSystem.SessionBrokerProject.ProjectDirectoryPath);
				Environment.SetEnvironmentVariable(settingName, contentRoot, EnvironmentVariableTarget.Process);
			}
		}

		public static string BlazorAppProcInfoDirectory => Environment.GetEnvironmentVariable(TempFolderSettingName);
		const string TempFolderSettingName = "BlazorAppProcInfoDirectory";

		static void SetTempPathForAddressFiles()
		{
			var tempFolderForAddressFiles = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempFolderForAddressFiles);
			Environment.SetEnvironmentVariable(TempFolderSettingName, tempFolderForAddressFiles);
		}

		[OneTimeTearDown]
		public void DeleteTempFolder()
		{
			var folder = Environment.GetEnvironmentVariable(TempFolderSettingName);
			if (Directory.Exists(folder))
			{
				Directory.Delete(folder, recursive: true);
			}
			Environment.SetEnvironmentVariable(TempFolderSettingName, null);
		}
	}
}
