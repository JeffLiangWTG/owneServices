using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using CargoWise.RefDbRepo.Tools.Common;
using WTG.DevTools.Common;

namespace CargoWise.RefDbRepo.T4Runner
{
	public static class Application
	{
		public static string VisualStudioPath { get; private set; }
		public static string SharedRefDataCommonPath { get; private set; }
		public static string T4Include { get; private set; }

		public static string DatasetTTIncludePath { get; private set; }

		public static string DefaultPathEDMXSafe { get; private set; }
		public static string DefaultPathEDMXStaging { get; private set; }

		static string DefaultPathSafeDataUpdateService = @"..\..\..\bin\Server\net8.0\CargoWise.RefDbRepo.NewSafeDataUpdateService.exe";

		public static void ConfigEnvironment()
		{
			VisualStudioPath = VisualStudioHelpers.GetRootPath(VisualStudioVersion.VisualStudio2022);
			var rootProjectPath = Path.GetFullPath(@"..\..\..\");

			SharedRefDataCommonPath = GitFactory.GetLocalPathFromServerMappingString(ApplicationConfig.SharedPath);
			DatasetTTIncludePath = Path.GetFullPath(Path.Combine(rootProjectPath, @"Common\Infrastructure\Utils\SafeDataSetStructure"));
			T4Include = Path.GetFullPath(Path.Combine(rootProjectPath, @"ThirdParty\T4Includes"));
		}

		static Thread OpenThreadToRunSelfHostedServices;

		public static void InitializeSelfHostedServices()
		{
			var task = new ThreadStart(() =>
			{
				var procStartInfo = new ProcessStartInfo(DefaultPathSafeDataUpdateService)
				{
					CreateNoWindow = true,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
					UseShellExecute = false,
					WindowStyle = ProcessWindowStyle.Hidden,
					Arguments = "--urls http://0.0.0.0:37016"
				};

				WinProcessor.RunProcessWithNoExceptionHandler(procStartInfo);
			});

			OpenThreadToRunSelfHostedServices = new Thread(task)
			{
				IsBackground = true,
			};
			OpenThreadToRunSelfHostedServices.Start();
			Thread.Sleep(TimeSpan.FromSeconds(5));
		}

		public static void CloseSafeDataUpdateService()
		{
			var processes = Process.GetProcessesByName("CargoWise.RefDbRepo.NewSafeDataUpdateService");
			foreach (var process in processes)
			{
				process.Kill();
			}
		}
	}
}
