using System;
using System.Diagnostics;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class Upgrader
	{
		public void StoreNewUpgrade(string eDPFileName, byte[] eDPFileContent)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			StoreNewUpgrade(factory, eDPFileName, eDPFileContent);
			factory.Save();
		}

		public void StoreNewUpgrade(BusinessObjectFactory factory, string eDPFileName, byte[] eDPFileContent)
		{
			if (eDPFileName.ToLower().EndsWith(".edp"))
			{
				StmUpgrade upgrade = factory.New<StmUpgrade>();
				upgrade.UpdateVersionDetails(eDPFileName);
				upgrade.SZ_UpgradeData_Compressed = eDPFileContent;
			}
			else
			{
				ErrorReporter.ReportOnce("Received Upgrade file is not an edp file and is ignored: " + eDPFileName);
			}
		}

		public static UpgradeManager NewUpgradeManager()
		{
			return UpgradeManagerFactory.NewUpgradeManager();
		}

		public IDisposable InstallPackage(StmUpgrade upgradeToApply)
		{
			return InstallPackage(upgradeToApply, null);
		}

		public IDisposable InstallPackage(StmUpgrade upgradeToApply, Upgrades.Progress progress)
		{
			string packageDirectory = GetPackageDirectory(upgradeToApply.VersionNumber);
			var upgradeManager = NewUpgradeManager();
			return upgradeManager.InstallUpgradePackage(new UpgradeInfo(upgradeToApply.PK.ToGuid(), upgradeToApply.VersionNumber.ToVersion()), packageDirectory, progress);
		}

		public void WriteInstallationCurrentVersion()
		{
			WriteInstallationVersion(ReleaseInfo.Instance.VersionNumber);
		}

		public void WriteInstallationVersion(VersionNumber version)
		{
			UpgradeManager upgradeManager = NewUpgradeManager();
			upgradeManager.RunCurrentVersionWriter(Path.Combine(InstallationEnvironment.Instance.BaseInstallPath, version.ToString()));
		}

		string GetPackageDirectory(VersionNumber version)
		{
			return InstallationEnvironment.Instance.GetTargetInstallPath(version);
		}

		public void LaunchUpgrade(StmUpgrade upgradeToApply)
		{
			var arguments = CommandLineArguments.UsedToLaunchApplication.Clone();
			arguments.OptionalArgs["-Upgrade:"] = upgradeToApply.PK.ToString();
			LaunchInstalledPackage(upgradeToApply.VersionNumber, arguments);
		}

		public void LaunchInstalledPackage(VersionNumber version)
		{
			LaunchInstalledPackage(version, CommandLineArguments.UsedToLaunchApplication);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "False alarm, we are starting Enterprise not opening a file or url")]
		public void LaunchInstalledPackage(VersionNumber version, CommandLineArguments arguments)
		{
			string exeFilename = null;
			try
			{
				var path = GetPackageDirectory(version);
				exeFilename = ExeFileNames.GetMainExeFileName(path);
				var startInfo = new ProcessStartInfo(Path.Combine(path, exeFilename), arguments.ToString())
				{
					WorkingDirectory = path, UseShellExecute = false
				};
				Process.Start(startInfo);  // We are starting Enterprise not opening a file or url
				Env.ExitApplication();
			}
			catch (System.ComponentModel.Win32Exception ex)
			{
#if DEBUG
				if (!Globals.IsTest)
#endif
				{
					if (ex.NativeErrorCode != ERROR_CANCELLED)
					{
						string message = string.Format((NoResString)"Error occurred while restarting for upgrade. The exe file path was {0}.", exeFilename);
						Globals.Message.ShowDeveloperException("LaunchUpgrade", message, ex);
					}
				}

				throw new InvalidOperationException(string.Format("An unexpected error occurred. Please exit {0} and run it again to complete the upgrade.", Core.Constants.ProductName));
			}
		}

		const int ERROR_CANCELLED = 1223;
	}
}
