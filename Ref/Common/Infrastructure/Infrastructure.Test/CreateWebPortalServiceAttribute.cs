using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	[SupportedOSPlatform("windows")]
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class CreateWebPortalServiceAttribute : Attribute, ITestAction
	{
		public ActionTargets Targets { get; }

		const string ExecutablePath = @"..\..\Portal\net8.0\CargoWise.RefDbRepo.ReferenceDataUpdateService.Web.exe";

		readonly string _webrootPath = Path.Join(
			TestSourcePathHelper.DATTestSupplementaryContentPath,
			"Web", "ReferenceDataUpdateService.Web", "wwwroot"
		);

		public const string Urls = "http://localhost:19488";
		BackgroundProcessRunner _backgroundProcessRunner;

		public CreateWebPortalServiceAttribute(ActionTargets targets = ActionTargets.Default)
		{
			Targets = targets;
		}

		public void BeforeTest(ITest test)
		{
			var processStartInfo = new ProcessStartInfo(ExecutablePath)
			{
				CreateNoWindow = true,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden,
				Arguments = $"--urls \"{Urls}\" --webroot \"{_webrootPath}\""
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
