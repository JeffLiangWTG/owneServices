using System;
using System.IO;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.Testing.Common;
using NUnit.Framework;
using Env = System.Environment;

[assembly: DatabaseVersionTestCheck]
[assembly: WithAttachedAppServerLogs]

namespace CargoWise.Winzor.AppServer.Test
{
	[SetUpFixture]
	[AssemblySetup]
	public class SetUpTests
	{
		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			// The integration test host needs to be able to find the source tree in order to work correctly. On DAT, this is located
			// on a network share, so we need to set an environment variable to allow the integration test host to find it
			// See https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Testing/src/WebApplicationFactory.cs#L169
			var sourceRoot = Env.GetEnvironmentVariable("DAT_TestSourcePath");
			if (!string.IsNullOrEmpty(sourceRoot))
			{
				var settingName = $"ASPNETCORE_TEST_CONTENTROOT_{BuildFileSystem.AppServerProject.ContentRootName}";
				var contentRoot = Path.Combine(sourceRoot, BuildFileSystem.AppServerProject.ProjectDirectoryPath);
				Env.SetEnvironmentVariable(settingName, contentRoot, EnvironmentVariableTarget.Process);
			}

			// Razor Class Libraries can bundle static web assets (e.g. CSS/JS).  In the development environment, these are
			// referenced via a special file provider that references the nuget package, or referenced source project, etc.
			// On DAT, this doesn't work because the source tree is not available at its original path. The published manifest file contains
			// source paths *on the build server*.
			// https://stackoverflow.com/questions/58436497/what-are-asp-net-core-static-web-assets
			// For now, there are no integration tests that require these assets, so to workaround we can set config to change the hosting
			// environment - the runtime only attempts to load the manifest in Development. (In other environments, the assets are included in
			// the published output.)  If in future these assets are needed, workarounds are either to rewrite the manifest file to set the
			// correct paths to the source tree share, or execute the published app instead of using the integration test host.
			if (bool.TryParse(Env.GetEnvironmentVariable("DAT_IS_TESTING"), out var onDat) && onDat)
			{
				Env.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "DAT", EnvironmentVariableTarget.Process);
			}
		}
	}
}
