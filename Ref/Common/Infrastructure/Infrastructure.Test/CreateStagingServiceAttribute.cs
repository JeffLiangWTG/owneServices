using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	[SupportedOSPlatform("windows")]
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class CreateStagingServiceAttribute : Attribute, ITestAction
	{
		BackgroundProcessRunner _backgroundProcessRunner;

		public const string Urls = "http://localhost:17488/";
		const string ExecutablePath = @"..\..\StagingService\net8.0\CargoWise.RefDbRepo.Staging.NewService.exe";

		public string UniqueDbName { get; }
		public ActionTargets Targets { get; }

		public CreateStagingServiceAttribute(string uniqueDbName, ActionTargets targets = ActionTargets.Default)
		{
			Targets = targets;
			UniqueDbName = uniqueDbName;
		}

		public void BeforeTest(ITest test)
		{
			var stagingDbName = CreateDatabaseAttribute.GetDbName(UniqueDbName);
			var stagingDbConnectionString = TestConnectionString.GetAdmin(stagingDbName).Replace("\"", "\\\"");
			var processStartInfo = new ProcessStartInfo(ExecutablePath)
			{
				CreateNoWindow = true,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden,
				Arguments = $"--urls {Urls} --stagingDbConnectionString \"{stagingDbConnectionString}\""
			};
			_backgroundProcessRunner = new BackgroundProcessRunner(processStartInfo);
			_backgroundProcessRunner.Run();
		}


		public void AfterTest(ITest test)
		{
			_backgroundProcessRunner.Kill();
		}
	}
}
