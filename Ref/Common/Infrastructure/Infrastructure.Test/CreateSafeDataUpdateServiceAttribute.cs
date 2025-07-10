using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	[SupportedOSPlatform("windows")]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class CreateSafeDataUpdateServiceAttribute : Attribute, ITestAction
	{
		const string DefaultPathSafeDataUpdateService = @"..\..\Server\net8.0\CargoWise.RefDbRepo.NewSafeDataUpdateService.exe";
		public const string Urls = "http://localhost:37016/";
		public string UniqueDbName { get; }
		BackgroundProcessRunner _backgroundProcessRunner;

		public CreateSafeDataUpdateServiceAttribute(string uniqueDbName, ActionTargets targets = ActionTargets.Default)
		{
			UniqueDbName = uniqueDbName;
			Targets = targets;
		}

		public ActionTargets Targets { get; }

		public void AfterTest(ITest test)
		{
			_backgroundProcessRunner.Kill();

			var outcome = TestContext.CurrentContext.Result.Outcome;
			if (outcome != ResultState.Success)
			{
				throw new TaskSchedulerException(@$"
This test failed. Check log of the SafeUpdateService:
{_backgroundProcessRunner.OutputLog}");
			}
		}

		public void BeforeTest(ITest test)
		{
			InitializeSelfHostedServices();
		}

		void InitializeSelfHostedServices()
		{
			var safeDbName = CreateDatabaseAttribute.GetDbName(UniqueDbName);
			var safeEntityConnectionString = TestConnectionString.GetAdmin(safeDbName).Replace("\"", "\\\"");
			var baseDir = AppDomain.CurrentDomain.BaseDirectory.EndsWith("\\")
				? AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.Length - 1)
				: AppDomain.CurrentDomain.BaseDirectory;

			var processStartInfo = new ProcessStartInfo(DefaultPathSafeDataUpdateService)
			{
				CreateNoWindow = true,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden,
				Arguments = $"--urls \"{Urls}\" --baseDir \"{baseDir}\" --entityConnectionString \"{safeEntityConnectionString}\""
			};
			_backgroundProcessRunner = new BackgroundProcessRunner(processStartInfo);
			_backgroundProcessRunner.Run();
		}
	}
}
