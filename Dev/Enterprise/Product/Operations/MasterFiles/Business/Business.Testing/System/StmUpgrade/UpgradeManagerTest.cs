using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Environment.Testing;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Microsoft.CSharp;
using Microsoft.Win32;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.CW;
using ServiceManager.Integration.ServiceHostUtilities;
using static System.FormattableString;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UpgradeManagerTest : TestCaseWithFactory
	{
		public void TestQueryAndSetCurrentVersion()
		{
			UpgradeManager mgr = UpgradeManagerFactory.NewUpgradeManager();
			UpgradeInfo match = mgr.QueryCurrentVersion();
			AssertNull(match);
			StmUpgrade upgrade0 = CreateUpgrade(0, 1, ZDateTime.Now, StmUpgrade.StmUpgradeStatus.Ready, "");
			StmUpgrade upgrade1 = CreateUpgrade(1, 1, ZDateTime.Now, StmUpgrade.StmUpgradeStatus.Ready, "");
			StmUpgrade upgrade2 = CreateUpgrade(1, 2, ZDateTime.Now, StmUpgrade.StmUpgradeStatus.Ready, "");
			StmUpgrade upgrade3 = CreateUpgrade(1, 3, ZDateTime.Now, StmUpgrade.StmUpgradeStatus.NotApplied, "");
			StmUpgrade upgrade4 = CreateUpgrade(1, 4, ZDateTime.Now, StmUpgrade.StmUpgradeStatus.Deleted, "");
			Factory.Save();
			match = mgr.QueryCurrentVersion();
			AssertNull(match);
			mgr.SetAsCurrentVersion(new UpgradeInfo(upgrade2.PK.ToGuid(), upgrade2.VersionNumber.ToVersion()), GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaff.CurrentUser.GS_LoginName);
			match = mgr.QueryCurrentVersion();
			AssertMatch(upgrade2, match);

			upgrade0.Reload();
			AssertEquals(StmUpgrade.StmUpgradeStatus.Obsolete, upgrade0.SZ_Status);
			upgrade1.Reload();
			AssertEquals(StmUpgrade.StmUpgradeStatus.NotApplied, upgrade1.SZ_Status);
			upgrade2.Reload();
			AssertEquals(StmUpgrade.StmUpgradeStatus.CurrentVersion, upgrade2.SZ_Status);
			upgrade3.Reload();
			AssertEquals(StmUpgrade.StmUpgradeStatus.Ready, upgrade3.SZ_Status);
			upgrade4.Reload();
			AssertEquals(StmUpgrade.StmUpgradeStatus.Deleted, upgrade4.SZ_Status);

			mgr.SetAsCurrentVersion(new UpgradeInfo(upgrade3.PK.ToGuid(), upgrade3.VersionNumber.ToVersion()), GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaff.CurrentUser.GS_LoginName);
			match = mgr.QueryCurrentVersion();
			AssertMatch(upgrade3, match);

			upgrade0.Reload();
			AssertEquals(StmUpgrade.StmUpgradeStatus.Obsolete, upgrade0.SZ_Status);
			upgrade1.Reload();
			AssertEquals(StmUpgrade.StmUpgradeStatus.NotApplied, upgrade1.SZ_Status);
			upgrade2.Reload();
			AssertEquals(StmUpgrade.StmUpgradeStatus.Applied, upgrade2.SZ_Status);
			upgrade3.Reload();
			AssertEquals(StmUpgrade.StmUpgradeStatus.CurrentVersion, upgrade3.SZ_Status);
			upgrade4.Reload();
			AssertEquals(StmUpgrade.StmUpgradeStatus.Deleted, upgrade4.SZ_Status);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInstallPackage()
		{
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				StmUpgrade upgrade = Factory.New<StmUpgrade>();
				upgrade.SZ_ExeVersionDate = new ZDateTime(2005, 1, 12, 12, 13, 14);
				upgrade.SZ_MajorVersion = 1;
				upgrade.SZ_MinorVersion = 2;
				upgrade.SZ_Release = 3;
				upgrade.SZ_Patch = 4;
				upgrade.SZ_UpgradeData_Compressed = File.ReadAllBytes(TestEdpFileName);
				Factory.Save();

				string[] expectedInstalledDirectories = new string[] { "Install" };
				for (int i = 0; i < expectedInstalledDirectories.Length; i++)
				{
					expectedInstalledDirectories[i] = Path.Combine(InstallationEnvironment.Instance.GetTargetInstallPath(upgrade.VersionNumber), expectedInstalledDirectories[i]);
				}
				new Upgrader().InstallPackage(upgrade).Dispose();
				AssertContainsExactElementsInAnyOrder(testEdpApplicationFiles, Directory.GetFiles(InstallationEnvironment.Instance.GetTargetInstallPath(upgrade.VersionNumber)).Select(file => Path.GetFileName(file)));
				AssertContainsExactElementsInAnyOrder(expectedInstalledDirectories, Directory.GetDirectories(InstallationEnvironment.Instance.GetTargetInstallPath(upgrade.VersionNumber)));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInstallPackageProgress()
		{
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				StmUpgrade upgrade = Factory.New<StmUpgrade>();
				upgrade.SZ_ExeVersionDate = new ZDateTime(2005, 1, 12, 12, 13, 14);
				upgrade.SZ_MajorVersion = 1;
				upgrade.SZ_MinorVersion = 2;
				upgrade.SZ_Release = 3;
				upgrade.SZ_Patch = 4;
				upgrade.SZ_UpgradeData_Compressed = File.ReadAllBytes(TestEdpFileName);
				Factory.Save();

				string[] expectedInstalledDirectories = new string[] { "Install" };
				for (int i = 0; i < expectedInstalledDirectories.Length; i++)
				{
					expectedInstalledDirectories[i] = Path.Combine(InstallationEnvironment.Instance.GetTargetInstallPath(upgrade.VersionNumber), expectedInstalledDirectories[i]);
				}
				new Upgrader().InstallPackage(upgrade, OnProgress).Dispose();
				AssertContainsExactElementsInAnyOrder(testEdpApplicationFiles, Directory.GetFiles(InstallationEnvironment.Instance.GetTargetInstallPath(upgrade.VersionNumber)).Select(file => Path.GetFileName(file)));
				AssertContainsExactElementsInAnyOrder(expectedInstalledDirectories, Directory.GetDirectories(InstallationEnvironment.Instance.GetTargetInstallPath(upgrade.VersionNumber)));
				Assert("Progress callback should be called", progressCalled);
			}
		}
		bool progressCalled;
		bool OnProgress(string status, int percentComplete)
		{
			progressCalled = true;
			Assert("Percent complete should be between 0 and 100", percentComplete >= 0 && percentComplete <= 100);
			return true;
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInstallPackageProgressCancel()
		{
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				StmUpgrade upgrade = Factory.New<StmUpgrade>();
				upgrade.SZ_ExeVersionDate = new ZDateTime(2005, 1, 12, 12, 13, 14);
				upgrade.SZ_MajorVersion = 1;
				upgrade.SZ_MinorVersion = 2;
				upgrade.SZ_Release = 3;
				upgrade.SZ_Patch = 4;
				upgrade.SZ_UpgradeData_Compressed = File.ReadAllBytes(TestEdpFileName);
				Factory.Save();

				AssertExceptionThrown(typeof(CancelledException), () => new Upgrader().InstallPackage(upgrade, OnProgressCancel).Dispose());
				Assert("Install directory should not exist if installation was cancelled", !Directory.Exists(InstallationEnvironment.Instance.GetTargetInstallPath(upgrade.VersionNumber)));
			}
		}
		bool OnProgressCancel(string status, int percentComplete)
		{
			return false;
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInstallPackageOverRunningInstallation()
		{
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				StmUpgrade upgrade = Factory.New<StmUpgrade>();
				upgrade.SZ_ExeVersionDate = new ZDateTime(2005, 1, 12, 12, 13, 14);
				upgrade.SZ_MajorVersion = 1;
				upgrade.SZ_MinorVersion = 2;
				upgrade.SZ_Release = 3;
				upgrade.SZ_Patch = 4;
				upgrade.SZ_UpgradeData_Compressed = File.ReadAllBytes(TestEdpFileName);
				Factory.Save();

				Directory.CreateDirectory(InstallationEnvironment.Instance.GetTargetInstallPath(upgrade.VersionNumber));
				string[] expectedInstalledFiles = new string[] { "one.txt", "two.txt", "three.txt" };
				for (int i = 0; i < expectedInstalledFiles.Length; i++)
				{
					expectedInstalledFiles[i] = Path.Combine(InstallationEnvironment.Instance.GetTargetInstallPath(upgrade.VersionNumber), expectedInstalledFiles[i]);
					File.WriteAllText(expectedInstalledFiles[i], "bla");
				}
				Exception installException = null;
				using (File.Open(expectedInstalledFiles[1], FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					try
					{
						using (new Upgrader().InstallPackage(upgrade))
						{ }
					}
					catch (Exception ex)
					{
						installException = ex;
					}
				}
				AssertNotNull(installException);
				AssertContainsExactElementsInAnyOrder(expectedInstalledFiles, Directory.GetFiles(InstallationEnvironment.Instance.GetTargetInstallPath(upgrade.VersionNumber)));
				AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), Directory.GetDirectories(InstallationEnvironment.Instance.GetTargetInstallPath(upgrade.VersionNumber)));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRepairCurrentVersionReplacesMissingFiles()
		{
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				StmUpgrade upgrade = Factory.New<StmUpgrade>();
				upgrade.SZ_ExeVersionDate = new ZDateTime(2005, 1, 12, 12, 13, 14);
				upgrade.SZ_MajorVersion = 1;
				upgrade.SZ_MinorVersion = 2;
				upgrade.SZ_Release = 3;
				upgrade.SZ_Patch = 4;
				upgrade.SZ_UpgradeData_Compressed = File.ReadAllBytes(TestEdpFileName);
				Factory.Save();

				string[] expectedInstalledDirectories = new string[] { "Install" };
				for (int i = 0; i < expectedInstalledDirectories.Length; i++)
				{
					expectedInstalledDirectories[i] = Path.Combine(InstallationEnvironment.Instance.GetTargetInstallPath(upgrade.VersionNumber), expectedInstalledDirectories[i]);
				}

				new Upgrader().InstallPackage(upgrade, OnProgress).Dispose();
				string targetInstallPath = InstallationEnvironment.Instance.GetTargetInstallPath(upgrade.VersionNumber);
				AssertContainsExactElementsInAnyOrder(testEdpApplicationFiles, Directory.GetFiles(targetInstallPath).Select(file => Path.GetFileName(file)));
				AssertContainsExactElementsInAnyOrder(expectedInstalledDirectories, Directory.GetDirectories(targetInstallPath));

				upgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.CurrentVersion;
				Factory.Save();

				File.Delete(Path.Combine(targetInstallPath, "beta.txt"));
				File.Delete(Path.Combine(targetInstallPath, "Enterprise.Upgrades.Postinstall4.0.exe"));

				var mockAssemblyLoader = new Mock<IAssemblyLoader>();
				try
				{
					AssemblyLoader.Instance = mockAssemblyLoader.Object;
					mockAssemblyLoader.Setup(m => m.GetBinPath()).Returns(InstallationEnvironment.Instance.GetTargetInstallPath(upgrade.VersionNumber));
					TopLevelExceptionHandler.RepairCurrentInstallation();
					AssertContainsExactElementsInAnyOrder(testEdpApplicationFiles, Directory.GetFiles(InstallationEnvironment.Instance.GetTargetInstallPath(upgrade.VersionNumber)).Select(file => Path.GetFileName(file)));
					AssertContainsExactElementsInAnyOrder(expectedInstalledDirectories, Directory.GetDirectories(InstallationEnvironment.Instance.GetTargetInstallPath(upgrade.VersionNumber)));
				}
				finally
				{
					AssemblyLoader.Instance = null;
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRepairCurrentVersionReplacesCorruptedFiles()
		{
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				StmUpgrade upgrade = Factory.New<StmUpgrade>();
				upgrade.SZ_ExeVersionDate = new ZDateTime(2005, 1, 12, 12, 13, 14);
				upgrade.SZ_MajorVersion = 1;
				upgrade.SZ_MinorVersion = 2;
				upgrade.SZ_Release = 3;
				upgrade.SZ_Patch = 4;
				upgrade.SZ_UpgradeData_Compressed = File.ReadAllBytes(TestEdpFileName);
				Factory.Save();

				string[] expectedInstalledDirectories = new string[] { "Install" };
				for (int i = 0; i < expectedInstalledDirectories.Length; i++)
				{
					expectedInstalledDirectories[i] = Path.Combine(InstallationEnvironment.Instance.GetTargetInstallPath(upgrade.VersionNumber), expectedInstalledDirectories[i]);
				}

				new Upgrader().InstallPackage(upgrade, OnProgress).Dispose();
				string targetInstallPath = InstallationEnvironment.Instance.GetTargetInstallPath(upgrade.VersionNumber);
				AssertContainsExactElementsInAnyOrder(testEdpApplicationFiles, Directory.GetFiles(targetInstallPath).Select(file => Path.GetFileName(file)));
				AssertContainsExactElementsInAnyOrder(expectedInstalledDirectories, Directory.GetDirectories(targetInstallPath));

				upgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.CurrentVersion;
				Factory.Save();

				File.WriteAllText(Path.Combine(targetInstallPath, "alpha.txt"), "This is a corrupted File");

				var mockAssemblyLoader = new Mock<IAssemblyLoader>();
				try
				{
					AssemblyLoader.Instance = mockAssemblyLoader.Object;
					mockAssemblyLoader.Setup(m => m.GetBinPath()).Returns(InstallationEnvironment.Instance.GetTargetInstallPath(upgrade.VersionNumber));
					TopLevelExceptionHandler.RepairCurrentInstallation();
					AssertFileSameAsString(Path.Combine(targetInstallPath, "alpha.txt"), "This is file alpha.");
				}
				finally
				{
					AssemblyLoader.Instance = null;
				}
			}
		}

		public void TestWriteCurrentVersion()
		{
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				string installPath = InstallationEnvironment.Instance.GetTargetInstallPath(ReleaseInfo.Instance.VersionNumber);
				CopyBinaries(installPath);

				new Upgrader().WriteInstallationCurrentVersion();

				AssertContains("CurrentVersion", ReleaseInfo.Instance.VersionNumber.ToString(), File.ReadAllText(Path.Combine(InstallationEnvironment.Instance.BaseInstallPath, "CurrentVersion")));
			}
		}

		public void TestWriteCurrentVersionLogsUsage()
		{
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				var applicationUsageLogFile = new ApplicationUsageLogFile();
				var existingLog = applicationUsageLogFile.RetrieveAllTheLogs().SingleOrDefault(log => log.ServerName == Db.ServerName && log.DbName == Db.DatabaseName);
				if (existingLog != null)
				{
					applicationUsageLogFile.DeleteLog(existingLog);
				}
				AssertNull(applicationUsageLogFile.RetrieveAllTheLogs().SingleOrDefault(log => log.ServerName == Db.ServerName && log.DbName == Db.DatabaseName));

				string installPath = InstallationEnvironment.Instance.GetTargetInstallPath(ReleaseInfo.Instance.VersionNumber);
				CopyBinaries(installPath);

				new Upgrader().WriteInstallationCurrentVersion();

				AssertNotNull(applicationUsageLogFile.RetrieveAllTheLogs().SingleOrDefault(log => log.ServerName == Db.ServerName && log.DbName == Db.DatabaseName));
			}
		}

		[TestRequiresAdministrativePrivileges("Writes to the windows registry")]
		public void TestWriteCurrentVersionUpdatesHostSource()
		{
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				string newVersionPath = InstallationEnvironment.Instance.GetTargetInstallPath(ReleaseInfo.Instance.VersionNumber);
				string oldVersionPath = InstallationEnvironment.Instance.GetTargetInstallPath(new VersionNumber("0.0.0.0"));

				CopyBinaries(newVersionPath);

				var registryKeyName = string.Format("System\\CurrentControlSet\\Services\\{0}", ServiceHostProcess.GetServiceName(ServiceType.ProcessController, Db.ServerName, Db.Connection.CurrentDatabase));
				string serviceFileName = ServiceManagerConstants.ServiceManagerHostExe;
				using (var root = Microsoft.Win32.Registry.LocalMachine)
				{
					bool isRegistryCreated = root.GetSubKeyNames().Contains(registryKeyName);
					try
					{
						using (var registryKey = root.CreateSubKey(registryKeyName))
						{
							string oldImagePath = (string)registryKey.GetValue("ImagePath");

							registryKey.DeleteValue("ImagePath", false);
							try
							{
								string expected = '"' + Path.Combine(newVersionPath, serviceFileName) + '"' + " some parameter list";

								string imagePathNoVersion = '"' + Path.Combine(InstallationEnvironment.Instance.BaseInstallPath, serviceFileName) + '"' + " some parameter list";
								registryKey.SetValue("ImagePath", imagePathNoVersion, RegistryValueKind.ExpandString);
								AssertEquals("Precondition", imagePathNoVersion, (string)registryKey.GetValue("ImagePath"));
								File.WriteAllText(Path.Combine(InstallationEnvironment.Instance.BaseInstallPath, "CurrentVersion"), "");
								new Upgrader().WriteInstallationCurrentVersion();
								AssertEquals("registry value should be modified", expected, (string)registryKey.GetValue("ImagePath"));

								string imagePathOldVersion = '"' + Path.Combine(oldVersionPath, serviceFileName) + '"' + " some parameter list";
								registryKey.SetValue("ImagePath", imagePathOldVersion, RegistryValueKind.ExpandString);
								AssertEquals("Precondition", imagePathOldVersion, (string)registryKey.GetValue("ImagePath"));
								File.WriteAllText(Path.Combine(InstallationEnvironment.Instance.BaseInstallPath, "CurrentVersion"), "");
								new Upgrader().WriteInstallationCurrentVersion();
								AssertEquals("registry value should be modified", expected, (string)registryKey.GetValue("ImagePath"));
							}
							finally
							{
								if (oldImagePath != null)
								{
									registryKey.SetValue("ImagePath", oldImagePath);
								}
							}
						}
					}
					finally
					{
						if (!isRegistryCreated)
						{
							try
							{
								root.DeleteSubKey(registryKeyName);
							}
							catch (ArgumentException) { } //already deleted!
						}
					}
				}
			}
		}

		[TestRequiresAdministrativePrivileges("Writes to the windows registry")]
		public void TestWriteCurrentVersionUpdatesHostSourceWhenCurrentVersionFileIsAlreadyUpdated()
		{
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				string newVersionPath = InstallationEnvironment.Instance.GetTargetInstallPath(ReleaseInfo.Instance.VersionNumber);
				string oldVersionPath = InstallationEnvironment.Instance.GetTargetInstallPath(new VersionNumber("0.0.0.0"));

				CopyBinaries(newVersionPath);

				var registryKeyName = string.Format("System\\CurrentControlSet\\Services\\{0}", ServiceHostProcess.GetServiceName(ServiceType.ProcessController, Db.ServerName, Db.Connection.CurrentDatabase));
				string serviceFileName = ServiceManagerConstants.ServiceManagerHostExe;
				using (var root = Microsoft.Win32.Registry.LocalMachine)
				{
					bool isRegistryCreated = root.OpenSubKey(registryKeyName) != null;
					try
					{
						using (var registryKey = root.CreateSubKey(registryKeyName))
						{
							string oldImagePath = (string)registryKey.GetValue("ImagePath");

							registryKey.DeleteValue("ImagePath", false);
							try
							{
								string expected = '"' + Path.Combine(newVersionPath, serviceFileName) + '"' + " some parameter list";

								registryKey.SetValue("Description", "Manages and controls CargoWise One background tasks such as Email Processing, Report Scheduling, Database Consistency checks and backups.");
								registryKey.SetValue("DisplayName", string.Format("CargoWise One Process Controller ({0} {1})", Db.ServerName, Db.Connection.CurrentDatabase));
								string imagePathOldNameNoVersion = '"' + Path.Combine(oldVersionPath, serviceFileName) + '"' + " some parameter list";
								registryKey.SetValue("ImagePath", imagePathOldNameNoVersion, RegistryValueKind.ExpandString);
								File.WriteAllText(Path.Combine(InstallationEnvironment.Instance.BaseInstallPath, "CurrentVersion"), "");
								new Upgrader().WriteInstallationCurrentVersion();
								registryKey.SetValue("ImagePath", imagePathOldNameNoVersion, RegistryValueKind.ExpandString);
								new Upgrader().WriteInstallationCurrentVersion();
								AssertEquals("registry value should be modified", expected, (string)registryKey.GetValue("ImagePath"));
							}
							finally
							{
								if (oldImagePath != null)
								{
									registryKey.SetValue("ImagePath", oldImagePath);
								}
							}
						}
					}
					finally
					{
						if (!isRegistryCreated)
						{
							try
							{
								root.DeleteSubKey(registryKeyName);
							}
							catch (ArgumentException) { } //already deleted!
						}
					}
				}
			}
		}

		public void TestWriteCurrentCopiesCargoWiseStart()
		{
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				string installPath = InstallationEnvironment.Instance.GetTargetInstallPath(ReleaseInfo.Instance.VersionNumber);
				CopyBinaries(installPath);

				new Upgrader().WriteInstallationCurrentVersion();

				Assert(File.Exists(Path.Combine(InstallationEnvironment.Instance.BaseInstallPath, "CargoWise.Start.exe")));
				Assert(File.Exists(Path.Combine(InstallationEnvironment.Instance.BaseInstallPath, "CargoWise.Start.exe.config")));
				Assert(File.Exists(Path.Combine(InstallationEnvironment.Instance.BaseInstallPath, "applog.json")));
			}
		}

		public void TestWriteCurrentOnlyOverwritesOlderCargoWiseStart()
		{
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				string installPath = InstallationEnvironment.Instance.GetTargetInstallPath(ReleaseInfo.Instance.VersionNumber);
				CopyBinaries(installPath);

				string targetCargoWiseStartFile = Path.Combine(InstallationEnvironment.Instance.BaseInstallPath, "CargoWise.Start.exe");
				CreateExe(targetCargoWiseStartFile, ReleaseInfo.Instance.VersionNumber.AddRelease(1).ToString());
				new Upgrader().WriteInstallationCurrentVersion();
				AssertEquals(ReleaseInfo.Instance.VersionNumber.AddRelease(1).ToString(), FileVersionInfo.GetVersionInfo(targetCargoWiseStartFile).FileVersion);
			}
		}

		public void TestWriteCurrentVersionExceptionThrown()
		{
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				string installPath = InstallationEnvironment.Instance.GetTargetInstallPath(ReleaseInfo.Instance.VersionNumber);
				CopyBinaries(installPath);

				var currentVersionFile = Path.Combine(InstallationEnvironment.Instance.BaseInstallPath, "CurrentVersion");
				using (var fileLock = File.Open(currentVersionFile, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					var exception = AssertExceptionThrown<Exception>(() => new Upgrader().WriteInstallationCurrentVersion());
					CombineAssertions(() =>
					{
						AssertNotNull(exception);
						Assert(exception.Message.Contains("exited with exception."));
						Assert(exception.Message.Contains($"The process cannot access the file '{currentVersionFile}' because it is being used by another process."));
						Assert(exception.Message.Contains("(Exception details logged to Windows Event Log)."));
					});
				}
			}
		}

		public void TestQueryRunnablePackages()
		{
			UpgradeManager mgr = UpgradeManagerFactory.NewUpgradeManager();
			UpgradeInfoExtendedCollection matches = mgr.QueryRunnablePackages();
			AssertEquals(0, matches.Count);
			StmUpgrade upgrade0 = CreateUpgrade(new VersionNumber(14, 10, 11, 36), ZDateTime.Now.AddDays(-4), StmUpgrade.StmUpgradeStatus.Ready, "");
			StmUpgrade upgrade1 = CreateUpgrade(0, 1, ZDateTime.Now.AddDays(-3), StmUpgrade.StmUpgradeStatus.Ready, "");
			StmUpgrade upgrade2 = CreateUpgrade(1, 0, ZDateTime.Now.AddDays(-2), StmUpgrade.StmUpgradeStatus.Ready, "");
			StmUpgrade upgrade3 = CreateUpgrade(1, 1, ZDateTime.Now.AddDays(-1), StmUpgrade.StmUpgradeStatus.NotApplied, "");
			StmUpgrade upgrade4 = CreateUpgrade(1, 2, ZDateTime.Now, StmUpgrade.StmUpgradeStatus.Deleted, "");
			StmUpgrade upgrade5 = CreateUpgrade(new VersionNumber(1, 3, 6666, 0), ZDateTime.Now, StmUpgrade.StmUpgradeStatus.Ready, "");
			StmUpgrade upgrade6 = CreateUpgrade(new VersionNumber(1, 4, 3532, 4), ZDateTime.Now, StmUpgrade.StmUpgradeStatus.Ready, "");
			Factory.Save();

			matches = mgr.QueryRunnablePackages();
			AssertEquals(3, matches.Count);
			AssertEquals(upgrade2.PK, matches[0].PK);
			AssertEquals(upgrade1.PK, matches[1].PK);
			AssertEquals(upgrade0.PK, matches[2].PK);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSafeCopyDirectory()
		{
			using (TempDirectory tempDirectory = new TempDirectory())
			{
				EdpFile.Unpack(TestEdpFileName, Path.Combine(tempDirectory, "1"));
				UpgradeManager.SafeCopyDirectory(Path.Combine(tempDirectory, "1"), Path.Combine(tempDirectory, "2"));
				AssertDirectoryStructure(Path.Combine(tempDirectory, "1"), Path.Combine(tempDirectory, "2"));

				Directory.CreateDirectory(Path.Combine(tempDirectory, "3"));
				File.WriteAllText(Path.Combine(Path.Combine(tempDirectory, "3"), "test"), "Test");
				UpgradeManager.SafeCopyDirectory(Path.Combine(tempDirectory, "1"), Path.Combine(tempDirectory, "3"));
				AssertDirectoryStructure(Path.Combine(tempDirectory, "1"), Path.Combine(tempDirectory, "3"));

				Directory.CreateDirectory(Path.Combine(tempDirectory, "4"));
				File.WriteAllText(Path.Combine(Path.Combine(tempDirectory, "4"), "test"), "Test");
				UpgradeManager.SafeCopyDirectory(Path.Combine(tempDirectory, "4"), Path.Combine(tempDirectory, "4.0"));
				AssertDirectoryStructure(Path.Combine(tempDirectory, "4"), Path.Combine(tempDirectory, "4.0"));
				using (File.Open(Path.Combine(Path.Combine(tempDirectory, "4"), "test"), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
				{
					Exception exception = null;
					try
					{
						UpgradeManager.SafeCopyDirectory(Path.Combine(tempDirectory, "1"), Path.Combine(tempDirectory, "4"));
					}
					catch (Exception ex)
					{
						exception = ex;
					}
					AssertNotNull(exception);
					AssertType(typeof(IOException), exception);
					AssertEquals(string.Format("Access to the path '{0}' is denied.", Path.Combine(tempDirectory, "4")), exception.Message);
				}
				AssertDirectoryStructure(Path.Combine(tempDirectory, "4.0"), Path.Combine(tempDirectory, "4"));

				string mainExeFile = Path.Combine(Path.Combine(tempDirectory, "1"), @"Distribution\Application\" + ExeFileNames.CargoWiseOneExeForVersionInfo);
				using (File.Open(mainExeFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
				{
					Exception exception = null;
					try
					{
						UpgradeManager.SafeCopyDirectory(Path.Combine(tempDirectory, "1"), Path.Combine(tempDirectory, "4"));
					}
					catch (Exception ex)
					{
						exception = ex;
					}
					AssertNotNull(exception);
					AssertType(typeof(IOException), exception);
					AssertEquals(string.Format("The process cannot access the file '{0}' because it is being used by another process.", mainExeFile), exception.Message);
				}
				AssertDirectoryStructure(Path.Combine(tempDirectory, "4.0"), Path.Combine(tempDirectory, "4"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPackageUploadDownload()
		{
			UpgradeManager upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
			var package = upgradeManager.UploadUpgradePackage(TestEdpFileName, new Version(1, 2, 3, 4), ZDateTime.BrettsBirthday.ToDateTime(), "RDY", "", null);
			using (TempFile tempFile = TempFile.New())
			{
				upgradeManager.DownloadUpgradePackageFile(package.PK, tempFile.Filename);
				AssertFileSameAsBytes(TestEdpFileName, File.ReadAllBytes(tempFile.Filename));
			}
		}

		public void TestDownloadUpgradePackageFile_ThrowsException_WhenPackageNotAvailable()
		{
			Exception expectedException = null;
			var expectedErrorMessage = string.Empty;
			var upgradePk = Guid.NewGuid();

			UpgradeManager upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
			var commandLineArgs = CommandLineArguments.UsedToLaunchApplication;

			using (new DisposableAction(() => CommandLineArguments.UsedToLaunchApplication = commandLineArgs))
			using (var tempFile = TempFile.New())
			{
				try
				{
					CommandLineArguments.UsedToLaunchApplication = null;
					upgradeManager.DownloadUpgradePackageFile(upgradePk, tempFile.Filename, null);
				}
				catch (Exception ex) when (ex is InvalidPackageException)
				{
					expectedException = ex;
					expectedErrorMessage = ex.Message;
				}
			}

			AssertNotNull("InvalidPackageException thrown when package not available", expectedException);
			Assert(!string.IsNullOrEmpty(expectedErrorMessage));
			Assert(expectedErrorMessage.Contains(Invariant($"Failed to find package {upgradePk} for download")));
		}

		public void TestDownloadUpgradePackageStream_ThrowsException_WhenPackageNotAvailable()
		{
			Exception expectedException = null;
			var expectedErrorMessage = string.Empty;
			var upgradePk = Guid.NewGuid();

			UpgradeManager upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
			var commandLineArgs = CommandLineArguments.UsedToLaunchApplication;

			using (new DisposableAction(() => CommandLineArguments.UsedToLaunchApplication = commandLineArgs))
			using (var stream = new MemoryStream())
			{
				try
				{
					CommandLineArguments.UsedToLaunchApplication = null;
					upgradeManager.DownloadUpgradePackageStream(upgradePk, stream);
				}
				catch (Exception ex) when (ex is InvalidPackageException)
				{
					expectedException = ex;
					expectedErrorMessage = ex.Message;
				}
			}

			AssertNotNull("InvalidPackageException thrown when package not available", expectedException);
			Assert(!string.IsNullOrEmpty(expectedErrorMessage));
			Assert(expectedErrorMessage.Contains(Invariant($"Failed to find package {upgradePk} for download")));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPackageUploadOnImageColumn()
		{
			Db.Connection.ExecuteNonQuery("ALTER TABLE dbo.StmUpgrade ALTER COLUMN SZ_UpgradeData_Compressed image"); // Cannot alter a column type with a business object factory
			UpgradeManager upgradeManager = UpgradeManagerFactory.NewUpgradeManager();

			var expectedMessage = "This database is too old to upload a package to using this application version";
			AssertExceptionThrown(typeof(InvalidPackageException), expectedMessage, () => upgradeManager.UploadUpgradePackage(TestEdpFileName, new Version(1, 2, 3, 4), ZDateTime.BrettsBirthday.ToDateTime(), "RDY", "", null));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestClientSpecificDllCopiedToSharedBinaries()
		{
			using (var installDirectory = new TempDirectory())
			{
				var upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
				var packageVersion = new Version(1, 2, 3, 4);
				var package = upgradeManager.UploadUpgradePackage(TestEdpFileName, packageVersion, ZDateTime.BrettsBirthday.ToDateTime(), "RDY", "", null);
				upgradeManager.InstallUpgradePackage(new UpgradeInfo(package.PK, packageVersion), installDirectory.DirectoryName).Dispose();

				using (var tempFile = TempFile.New())
				using (var packageTempDir = new TempDirectory())
				{
					var appDir = Path.Combine(packageTempDir, "Distribution", "Application");
					Directory.CreateDirectory(appDir);
					File.WriteAllText(Path.Combine(appDir, "ZClientXYZ.dll"), "ZClientXYZ.dll");
					ZipCompression.Zip(packageTempDir.DirectoryName, tempFile.Filename);
					Db.Connection.ExecuteNonQuery("truncate table StmUpgrade");// clear data that was set without using business objects
					package = upgradeManager.UploadUpgradePackage(tempFile.Filename, packageVersion, ZDateTime.BrettsBirthday.ToDateTime(), "RDY", "", null);
				}
				upgradeManager.InstallUpgradePackage(new UpgradeInfo(package.PK, packageVersion), installDirectory.DirectoryName).Dispose();
				Assert(!File.Exists(Path.Combine(installDirectory.DirectoryName, "ZClientXYZ.dll")));
				Assert(File.Exists(Path.Combine(installDirectory.DirectoryName, "Enterprise.Upgrades.dll")));
				DataRegistry.Instance.ExpectedClientDLL = "ZClientXYZ";
				upgradeManager.InstallUpgradePackage(new UpgradeInfo(package.PK, packageVersion), installDirectory.DirectoryName).Dispose();
				Assert(File.Exists(Path.Combine(installDirectory.DirectoryName, "ZClientXYZ.dll")));
				Assert(File.Exists(Path.Combine(installDirectory.DirectoryName, "Enterprise.Upgrades.dll")));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInstallMutexIsNotAbandoned()
		{
			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				var upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
				var packageVersion = new Version(1, 2, 3, 4);
				var installDirectory = InstallationEnvironment.Instance.GetTargetInstallPath(new VersionNumber(packageVersion));
				var package = upgradeManager.UploadUpgradePackage(TestEdpFileName, packageVersion, ZDateTime.BrettsBirthday.ToDateTime(), "RDY", "", null);
				var task = Task.Run(() => upgradeManager.InstallUpgradePackage(new UpgradeInfo(package.PK, packageVersion), installDirectory).Dispose());
				upgradeManager.InstallUpgradePackage(new UpgradeInfo(package.PK, packageVersion), installDirectory).Dispose();
				task.Wait();
				AssertContainsExactElementsInAnyOrder(testEdpApplicationFiles, Directory.GetFiles(installDirectory).Select(file => Path.GetFileName(file)));
			}
		}

		void CreateExe(string path, string version)
		{
			CSharpCodeProvider compiler = new CSharpCodeProvider();
			CompilerParameters options = new CompilerParameters();
			options.ReferencedAssemblies.Add("System.dll");
			options.GenerateExecutable = true;
			options.OutputAssembly = Path.Combine(path);
			CompilerResults result = compiler.CompileAssemblyFromSource(options, string.Format(
@"					[assembly: System.Reflection.AssemblyFileVersion(""{0}"")]
					public static class Program
					{{
						public static void Main(string[] cmd)
						{{
						}}
					}}", version));
			string[] output = new string[result.Output.Count];
			result.Output.CopyTo(output, 0);
			AssertEquals(string.Join("\r\n", output), 0, result.Errors.Count);
		}

		static void CopyBinaries(string path)
		{
			File.Copy(Path.Combine(AssemblyLoader.GetBinPath(), "CurrentVersionWriter.exe"), Path.Combine(path, "CurrentVersionWriter.exe"));
			File.Copy(Path.Combine(AssemblyLoader.GetBinPath(), "Enterprise.Upgrades.dll"), Path.Combine(path, "Enterprise.Upgrades.dll"));
			File.Copy(Path.Combine(AssemblyLoader.GetBinPath(), "CargoWise.Shared.40.dll"), Path.Combine(path, "CargoWise.Shared.40.dll"));
			File.Copy(Path.Combine(AssemblyLoader.GetBinPath(), "CargoWise.ApplicationManager.Common.dll"), Path.Combine(path, "CargoWise.ApplicationManager.Common.dll"));
			File.Copy(Path.Combine(AssemblyLoader.GetBinPath(), "ServiceManager.Integration.ServiceHostUtilities.dll"), Path.Combine(path, "ServiceManager.Integration.ServiceHostUtilities.dll"));
			File.Copy(Path.Combine(AssemblyLoader.GetBinPath(), "CargoWise.Loader.Client.dll"), Path.Combine(path, "CargoWise.Loader.Client.dll"));
			File.Copy(Path.Combine(AssemblyLoader.GetBinPath(), "CargoWise.Loader.Common.dll"), Path.Combine(path, "CargoWise.Loader.Common.dll"));
			File.Copy(Path.Combine(AssemblyLoader.GetBinPath(), "CargoWise.Start.exe"), Path.Combine(path, "CargoWise.Start.exe"));
			File.Copy(Path.Combine(AssemblyLoader.GetBinPath(), "CargoWise.Start.exe.config"), Path.Combine(path, "CargoWise.Start.exe.config"));
			File.Copy(Path.Combine(AssemblyLoader.GetBinPath(), "applog.json"), Path.Combine(path, "applog.json"));
		}

		#region Implementation

		void AssertMatch(StmUpgrade expected, UpgradeInfo actual)
		{
			AssertNotNull(actual);
			AssertEquals(expected.PK.ToGuid(), actual.PK);
			AssertEquals(expected.VersionNumber.Major, actual.Version.Major);
			AssertEquals(expected.VersionNumber.Minor, actual.Version.Minor);
			AssertEquals(expected.VersionNumber.Release, actual.Version.Build);
			AssertEquals(expected.VersionNumber.Patch, actual.Version.Revision);
		}

		StmUpgrade CreateUpgrade(int addMajor, int addRelease, ZDateTime statusTime, string status, string statusComment)
		{
			return CreateUpgrade(ReleaseInfo.Instance.VersionNumber.Add(addMajor, 0, addRelease, 0), statusTime, status, statusComment);
		}

		StmUpgrade CreateUpgrade(VersionNumber version, ZDateTime statusTime, string status, string statusComment)
		{
			StmUpgrade result = Factory.New<StmUpgrade>();
			result.VersionNumber = version;
			result.SZ_ExeVersionDate = statusTime;
			result.SZ_StatusTime = statusTime;
			result.SZ_Status = status;
			result.SZ_StatusComment = statusComment;
			return result;
		}

		void AssertDirectoryStructure(string expectedDirectory, string actualDirectory)
		{
			AssertContainsExactElementsInAnyOrder(GetFileNames(Directory.GetFiles(expectedDirectory)), GetFileNames(Directory.GetFiles(actualDirectory)));
			AssertContainsExactElementsInAnyOrder(GetFileNames(Directory.GetDirectories(expectedDirectory)), GetFileNames(Directory.GetDirectories(actualDirectory)));
			foreach (string subDirectory in Directory.GetDirectories(expectedDirectory))
			{
				AssertDirectoryStructure(subDirectory, Path.Combine(actualDirectory, Path.GetFileName(subDirectory)));
			}
		}

		string[] GetFileNames(string[] files)
		{
			for (int i = 0; i < files.Length; i++)
			{
				files[i] = Path.GetFileName(files[i]);
			}
			return files;
		}

		protected override void SetUp()
		{
			TestCaseHelper.ClearTable(StmUpgradeSchema.Constants.TableName);
		}

		static string TestEdpFileName
		{
			get { return Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\MasterFiles\Business\MasterFiles.Business\System\StmUpgrade\testing.edp"); }
		}

		readonly string[] testEdpApplicationFiles = new string[] { "alpha.txt", "beta.txt", ExeFileNames.CargoWiseOneExeForVersionInfo, "ediLoad.exe", "ediUninstall.exe", "Enterprise.Upgrades.Preinstall4.0.exe", "Enterprise.Upgrades.Postinstall4.0.exe", "CurrentVersionWriter.exe", "Enterprise.Upgrades.dll", "CargoWise.ApplicationManager.Common.dll", "CargoWiseOne.Start.exe", "CargoWiseOne.Start.exe.config" };

		#endregion
	}
}
