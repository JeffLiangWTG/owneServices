using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmUpgrade))]
	sealed class StmUpgradeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUpdateVersionDetails()
		{
			StmUpgrade upgrade = Factory.New<StmUpgrade>();
			upgrade.UpdateVersionDetails("package20040719_121209_999_88_77777_6.edp");

			AssertEquals("SZ_ExeVersionDate", new ZDateTime(2004, 7, 19, 12, 12, 0), upgrade.SZ_ExeVersionDate); //Note we ignore seconds.
			AssertEquals("SZ_MajorVersion", 999, (int)upgrade.SZ_MajorVersion);
			AssertEquals("SZ_MinorDataVersion", 88, (int)upgrade.SZ_MinorVersion);
			AssertEquals("SZ_Release", 77777, (int)upgrade.SZ_Release);
			AssertEquals("SZ_Patch", 6, (int)upgrade.SZ_Patch);
			AssertEquals("SZ_Tye", "EDP", upgrade.SZ_Type);
		}

		[UseSnapshotProtection(true)]
		public void TestShouldNotUpdateDuringDbUpgrade()
		{
			using var extraConnection = Db.NewExtraConnectionToMainDb();
			extraConnection.TryGetLock(Core.Constants.SystemUpgrade.DbUpgradeLock, out var sqlLock);
			AssertNotNull(sqlLock);

			var factory = new BusinessObjectFactory();
			StmUpgrade upgrade = factory.New<StmUpgrade>();
			upgrade.UpdateVersionDetails("package20040719_121209_999_88_77777_6.edp");
			factory.Save();

			using (sqlLock)
			{
				upgrade.SZ_UpgradeData_Compressed = new byte[] { 1, 2, 3 };

				AssertExceptionThrown<ZSaveException>(() => factory.Save());
			}
		}

		[UseSnapshotProtection(true)]
		public void TestShouldNotDeleteCurrentVersion()
		{
			var factory = new BusinessObjectFactory();
			StmUpgrade upgrade = factory.New<StmUpgrade>();
			upgrade.UpdateVersionDetails("package20040719_121209_999_88_77777_6.edp");
			upgrade.SetStatus(StmUpgrade.StmUpgradeStatus.CurrentVersion);
			factory.Save();

			upgrade.Delete();
			AssertExceptionThrown<ZSaveException>(() => factory.Save());
		}

		[UseSnapshotProtection(true)]
		public void TestShouldNotDeleteDuringDbUpgrade()
		{
			using var extraConnection = Db.NewExtraConnectionToMainDb();
			extraConnection.TryGetLock(Core.Constants.SystemUpgrade.DbUpgradeLock, out var sqlLock);
			AssertNotNull(sqlLock);

			var factory = new BusinessObjectFactory();
			StmUpgrade upgrade = factory.New<StmUpgrade>();
			upgrade.UpdateVersionDetails("package20040719_121209_999_88_77777_6.edp");
			factory.Save();

			using (sqlLock)
			{
				upgrade.Delete();
				AssertExceptionThrown<ZSaveException>(() => factory.Save());
			}
		}

		public void TestFilenameForVersionUpdate()
		{
			StmUpgrade upgrade = Factory.New<StmUpgrade>();
			upgrade.UpdateVersionDetails("Package20040719_121209_999_88_77777_6.edp");
			AssertEquals("Filename", "Package20040719_121200_999_88_77777_6.edp", upgrade.Filename);
		}

		public void TestIsExecutingVersion()
		{
			StmUpgrade upgradeCurrent = Factory.New<StmUpgrade>();
			VersionNumber versionNumber = ReleaseInfo.Instance.VersionNumber;
			upgradeCurrent.UpdateVersionDetails(
				"Package20040719_121209_" +
				versionNumber.Major + "_" +
				versionNumber.Minor + "_" +
				versionNumber.Release + "_" +
				versionNumber.Patch + ".edp");
			AssertEquals("IsExecutingVersion", true, upgradeCurrent.IsExecutingVersion);

			StmUpgrade upgradeNotCurrent = Factory.New<StmUpgrade>();
			upgradeNotCurrent.UpdateVersionDetails("Package20040719_121209_999_88_77777_6.edp");
			AssertEquals("IsExecutingVersion", false, upgradeNotCurrent.IsExecutingVersion);
		}

		public void TestUpdateVersionDetailsWhenFileNameIsWrong()
		{
			StmUpgrade upgrade = Factory.New<StmUpgrade>();
			upgrade.UpdateVersionDetails("Package.edp");

			AssertEquals("SZ_ExeVersionDate", ZDateTime.Empty, upgrade.SZ_ExeVersionDate); //Note we ignore seconds.
			AssertEquals("SZ_MajorVersion", 0, (int)upgrade.SZ_MajorVersion);
			AssertEquals("SZ_MinorDataVersion", 0, (int)upgrade.SZ_MinorVersion);
			AssertEquals("SZ_Release", 0, (int)upgrade.SZ_Release);
			AssertEquals("SZ_Patch", 0, (int)upgrade.SZ_Patch);
		}

		public void TestSize()
		{
			StmUpgrade upgrade = Factory.New<StmUpgrade>();
			upgrade.SZ_UpgradeData_Compressed = new byte[] { 1, 2, 3 };
			AssertEquals("Size", 0, upgrade.Size);
			Factory.Save();
			AssertEquals("Size", 3, upgrade.Size);
		}

		public void TestIsNotDeployable()
		{
			StmUpgrade upgrade = Factory.New<StmUpgrade>();

			upgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;
			Assert("IsNotDeployable should be false", !upgrade.IsNotDeployable);

			upgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.Applied;
			Assert("IsNotDeployable should be false", !upgrade.IsNotDeployable);

			upgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.NotApplied;
			Assert("IsNotDeployable should be false", !upgrade.IsNotDeployable);

			upgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.Deleted;
			Assert("IsNotDeployable should be true", upgrade.IsNotDeployable);

			upgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.Obsolete;
			Assert("IsNotDeployable should be true", upgrade.IsNotDeployable);
		}

		public void TestFullVersionString()
		{
			StmUpgrade upgrade = Factory.New<StmUpgrade>();
			upgrade.SZ_MajorVersion = 1;
			upgrade.SZ_MinorVersion = 2;
			upgrade.SZ_Release = 3;
			upgrade.SZ_Patch = 4;
			AssertEquals("FullVersionString", "1.2.3.4", upgrade.FullVersionString);
		}

		public void TestSetStatus()
		{
			StmUpgrade upgrade1 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade2 = Factory.New<StmUpgrade>();

			upgrade1.SetStatus(StmUpgrade.StmUpgradeStatus.Applied);
			AssertEquals("Upgrade1.SZ_Status", StmUpgrade.StmUpgradeStatus.Applied, upgrade1.SZ_Status);
			Assert("Upgrade1.SZ_StatusTime should be within 1 minute from the current time", ZDateTime.Now.AddMinutes(-1) <= upgrade1.SZ_StatusTime &&
				upgrade1.SZ_StatusTime <= ZDateTime.Now.AddMinutes(1));
			AssertEquals("Upgrade1.SZ_StatusComment", "", upgrade1.SZ_StatusComment);

			upgrade2.SetStatus(StmUpgrade.StmUpgradeStatus.Ready, "New Comment");
			AssertEquals("Upgrade2.SZ_Status", StmUpgrade.StmUpgradeStatus.Ready, upgrade2.SZ_Status);
			Assert("Upgrade2.SZ_StatusTime should be within 1 minute from the current time", ZDateTime.Now.AddMinutes(-1) <= upgrade2.SZ_StatusTime &&
				upgrade2.SZ_StatusTime <= ZDateTime.Now.AddMinutes(1));
			AssertEquals("Upgrade2.SZ_StatusComment", "New Comment", upgrade2.SZ_StatusComment);
		}

		public void TestStatusDescription()
		{
			StmUpgrade upgrade1 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade2 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade3 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade4 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade5 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade6 = Factory.New<StmUpgrade>();

			upgrade1.SZ_Status = StmUpgrade.StmUpgradeStatus.CurrentVersion;
			upgrade2.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;
			upgrade3.SZ_Status = StmUpgrade.StmUpgradeStatus.Applied;
			upgrade4.SZ_Status = StmUpgrade.StmUpgradeStatus.NotApplied;
			upgrade5.SZ_Status = StmUpgrade.StmUpgradeStatus.Deleted;
			upgrade6.SZ_Status = StmUpgrade.StmUpgradeStatus.Obsolete;

			upgrade1.VersionNumber = ReleaseInfo.Instance.VersionNumber;

			AssertEquals("upgrade1.StatusDescription", "Current Version", upgrade1.StatusDescription);
			AssertEquals("upgrade2.StatusDescription", "Ready", upgrade2.StatusDescription);
			AssertEquals("upgrade3.StatusDescription", "Applied", upgrade3.StatusDescription);
			AssertEquals("upgrade4.StatusDescription", "Not Applied", upgrade4.StatusDescription);
			AssertEquals("upgrade5.StatusDescription", "Deleted", upgrade5.StatusDescription);
			AssertEquals("upgrade6.StatusDescription", "Obsolete", upgrade6.StatusDescription);
		}

		public void TestIsOlderThanCurrentVersion()
		{
			StmUpgrade upgrade1 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade2 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade3 = Factory.New<StmUpgrade>();

			VersionNumber versionNumber = ReleaseInfo.Instance.VersionNumber;

			upgrade1.VersionNumber = versionNumber.AddRelease(-1);
			upgrade2.VersionNumber = versionNumber;
			upgrade3.VersionNumber = versionNumber.AddRelease(1);

			AssertEquals("upgrade1.IsOlderThanCurrentVersion", true, upgrade1.IsOlderThanCurrentVersion);
			AssertEquals("upgrade2.IsOlderThanCurrentVersion", false, upgrade2.IsOlderThanCurrentVersion);
			AssertEquals("upgrade3.IsOlderThanCurrentVersion", false, upgrade3.IsOlderThanCurrentVersion);
		}

		public void TestUpdateVersionDetailsWithCMC()
		{
			StmUpgrade upgrade = Factory.New<StmUpgrade>();

			upgrade.UpdateVersionDetails("2005-08-07-change.tar.gz");

			AssertEquals("Type", "CMC", upgrade.SZ_Type);
			AssertEquals("Major version", 20050807, upgrade.SZ_MajorVersion);
			AssertEquals("fudged exe version", new ZDateTime(2020, 01, 01), upgrade.SZ_ExeVersionDate);
			AssertEquals("is cmr ref", true, upgrade.IsCMRReferenceFiles);
		}

		public void TestUpdateVersionDetailsWithCMM()
		{
			StmUpgrade upgrade = Factory.New<StmUpgrade>();

			upgrade.UpdateVersionDetails("2005-08-09-main.tar.gz");

			AssertEquals("Type", "CMM", upgrade.SZ_Type);
			AssertEquals("Major version", 20050809, upgrade.SZ_MajorVersion);
			AssertEquals("fudged exe version", new ZDateTime(2020, 01, 01), upgrade.SZ_ExeVersionDate);
			AssertEquals("is cmr ref", true, upgrade.IsCMRReferenceFiles);
		}

		public void TestIsCMRReferenceFiles()
		{
			StmUpgrade upgrade1 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade2 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade3 = Factory.New<StmUpgrade>();

			upgrade1.SZ_Type = "CMC";
			upgrade2.SZ_Type = "EDP";
			upgrade3.SZ_Type = "CMM";

			AssertEquals("cmc", true, upgrade1.IsCMRReferenceFiles);
			AssertEquals("edp", false, upgrade2.IsCMRReferenceFiles);
			AssertEquals("cmm", true, upgrade3.IsCMRReferenceFiles);
		}

		public void TestVersionNumber()
		{
			StmUpgrade upgrade = Factory.New<StmUpgrade>();
			upgrade.VersionNumber = new VersionNumber(1, 2, 3, 4);
			AssertEquals("VersionNumber", new VersionNumber(1, 2, 3, 4), upgrade.VersionNumber);
			AssertEquals("SZ_MajorVersion", 1, upgrade.SZ_MajorVersion);
			AssertEquals("SZ_MinorVersion", 2, upgrade.SZ_MinorVersion);
			AssertEquals("SZ_Release", 3, upgrade.SZ_Release);
			AssertEquals("SZ_Patch", 4, upgrade.SZ_Patch);
		}

		public void TestUserCompressedColumn()
		{
			StmUpgrade upgrade = Factory.New<StmUpgrade>();

			byte[] data = new byte[500000]; // should be larger than ZLargeColumnSaver.MaxChunkSize
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = (byte)(i % 100);
			}

			upgrade.SZ_UpgradeData_Compressed = data;
			Factory.Save();

			string query = string.Format("SELECT {0} FROM {1} WHERE {2} = '{3}'",
										 StmUpgradeSchema.Constants.SZ_UpgradeData_Compressed,
										 StmUpgradeSchema.Constants.TableName, StmUpgradeSchema.Constants.PK, upgrade.PK);
			using (DbCommand cmd = Db.Connection.Command(query))
			{
				byte[] dbData = (byte[])cmd.ExecuteScalar();
				AssertEquals(data, dbData);
			}
		}

		protected override void SetUp()
		{
			TestCaseHelper.ClearTable(Enterprise.ZArchitecture.Schema.StmUpgradeSchema.Constants.TableName);
		}
	}
}
