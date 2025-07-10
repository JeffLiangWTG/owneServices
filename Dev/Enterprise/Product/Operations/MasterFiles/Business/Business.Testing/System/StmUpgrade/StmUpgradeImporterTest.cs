using System;
using System.Data;
using System.IO;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class StmUpgradeImporterTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportUpgradeFromFile()
		{
			Enterprise.MasterFiles.Business.VersionReport.VersionReportBuilderFactory.ClearSent();
			using (TempDirectory tempDirectory = new TempDirectory())
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();

				string fileName = Path.Combine(tempDirectory.DirectoryName, @"Package20090629_101500_1_2_3_4.edp");
				File.Copy(TestEdpFileName, fileName);

				ZGuid importedPK = new StmUpgradeImporter().ImportPackage(fileName);
				AssertEquals("Have got ImportedPK", false, importedPK.IsEmpty);

				var imported = Factory.Load<StmUpgrade>(importedPK);
				AssertNotNull("Found imported Upgrade", imported);

				AssertEquals("SZ_ExeVersionDate", new ZDateTime(2009, 6, 29, 10, 15, 00), imported.SZ_ExeVersionDate);

				AssertEquals("SZ_MajorVersion", 1, imported.SZ_MajorVersion);
				AssertEquals("SZ_MinorVersion", 2, imported.SZ_MinorVersion);
				AssertEquals("SZ_Release", 3, imported.SZ_Release);
				AssertEquals("SZ_Patch", 4, imported.SZ_Patch);

				Assert("SZ_UpgradeData_Compressed should be the same as source file",
					Utilities.IsByteArrayEqual(File.ReadAllBytes(TestEdpFileName), imported.SZ_UpgradeData_Compressed));

				AssertEquals("Version report sent", 1, Enterprise.MasterFiles.Business.VersionReport.VersionReportBuilderFactory.SentCount);
			}
		}

		public void TestImportUpgradeFromCorruptedFile()
		{
			using (TempDirectory tempDirectory = new TempDirectory())
			{
				string fileName = Path.Combine(tempDirectory.DirectoryName, @"Package20050112_121314_1_2_3_4.edp");
				byte[] testData = new byte[] { 1, 2, 3, 4, 5 };
				using (FileStream file = File.Create(fileName))
				{
					file.Write(testData, 0, testData.Length);
				}

				try
				{
					new StmUpgradeImporter().ImportPackage(fileName);
				}
				catch (Exception ex)
				{
					string expectedMessage = "Package " + fileName + " is corrupted.";
					AssertEquals("Exception Message", expectedMessage, ex.Message);
				}
			}
		}

		[UseSnapshotProtection(true)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFailToCleanupOldPackagesDuringDbUpgrade()
		{
			// Arrange
			using var extraConnection = Db.NewExtraConnectionToMainDb();
			extraConnection.TryGetLock(Core.Constants.SystemUpgrade.DbUpgradeLock, out var sqlLock);
			AssertNotNull(sqlLock);

			using (sqlLock)
			using (var tempDirectory = new TempDirectory())
			{
				var oldPackage = AddUpgradePackage(new Version(1, 0, 0, 0), "APL", new DateTime(2010, 1, 1), "EDP");

				var fileName = Path.Combine(tempDirectory.DirectoryName, "Package20090629_101500_1_2_3_4.edp");
				File.Copy(TestEdpFileName, fileName);

				// Act
				new StmUpgradeImporter().ImportPackage(fileName);

				// Assert
				AssertEquals(true, UpgradePackageExists(oldPackage));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCannotOverwriteCurrentVersion()
		{
			StmUpgrade upgrade = Factory.New<StmUpgrade>();
			upgrade.VersionNumber = new VersionNumber(1, 2, 3, 4);
			upgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.CurrentVersion;
			Factory.Save();

			AssertExceptionThrown(typeof(CurrentVersionException), delegate
			{
				using (TempDirectory tempDirectory = new TempDirectory())
				{
					string fileName = Path.Combine(tempDirectory.DirectoryName, @"Package20090629_101500_1_2_3_4.edp");
					File.Copy(TestEdpFileName, fileName);
					new StmUpgradeImporter().ImportPackage(fileName);
				}
			});

			UpgradeManager upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
			UpgradeInfo currentVersion = upgradeManager.QueryCurrentVersion();
			AssertNotNull("Current version should be found", currentVersion);
			AssertEquals(upgrade.PK, currentVersion.PK);
			AssertEquals(upgrade.VersionNumber.ToVersion(), currentVersion.Version);
		}

		public void TestPreviousUpgradeRemovedFromDB()
		{
			Guid dELPackage = AddUpgradePackage(new Version(1, 0, 0, 1), "DEL", new DateTime(2010, 1, 1), "CMC");
			Guid nAPPackage = AddUpgradePackage(new Version(1, 0, 0, 2), "NAP", new DateTime(2010, 1, 1), "EDP");
			Guid oBSPackage = AddUpgradePackage(new Version(1, 0, 0, 3), "OBS", new DateTime(2010, 1, 1), "EDP");
			Guid aPLPackage1 = AddUpgradePackage(new Version(1, 0, 0, 4), "APL", new DateTime(2010, 1, 7), "CMM");
			Guid aPLPackage2 = AddUpgradePackage(new Version(1, 0, 0, 5), "APL", new DateTime(2010, 1, 3), "EDP");
			Guid cURPackage = AddUpgradePackage(new Version(1, 0, 1, 0), "CUR", new DateTime(2010, 1, 6), "EDP");
			Guid rDYPackage1 = AddUpgradePackage(new Version(1, 0, 1, 2), "RDY", new DateTime(2010, 1, 5), "EDP");
			Guid rDYPackage2 = AddUpgradePackage(new Version(1, 0, 1, 1), "RDY", new DateTime(2010, 1, 6, 13, 35, 0), "EDP");

			var version = new Version(1, 0, 1, 1);

			new StmUpgradeImporter().RemoveOldDownloadedPackages(version);

			AssertEquals(true, UpgradePackageExists(dELPackage));
			AssertEquals(false, UpgradePackageExists(nAPPackage));
			AssertEquals(false, UpgradePackageExists(oBSPackage));
			AssertEquals(true, UpgradePackageExists(aPLPackage1));
			AssertEquals(false, UpgradePackageExists(aPLPackage2));
			AssertEquals(true, UpgradePackageExists(cURPackage));
			AssertEquals(true, UpgradePackageExists(rDYPackage1));
			AssertEquals(true, UpgradePackageExists(rDYPackage2));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAttemptToReImportSameVersionDoesNotOverwriteExistingRecord()
		{
			using (var tempDirectory = new TempDirectory())
			{
				var fileName = Path.Combine(tempDirectory.DirectoryName, @"Package20090629_101500_1_2_3_4.edp");
				File.Copy(TestEdpFileName, fileName);
				var pk1 = new StmUpgradeImporter().ImportPackage(fileName);
				var pk2 = new StmUpgradeImporter().ImportPackage(fileName);
				AssertEquals(pk1, pk2);
			}
		}

		Guid AddUpgradePackage(Version version, string status, DateTime statusTime, string type)
		{
			Guid pk = Guid.NewGuid();
			using (var cmd = Db.Connection.Command("insert into dbo.StmUpgrade (SZ_PK, SZ_ExeVersionDate, SZ_MajorVersion, SZ_MinorVersion, SZ_Release, SZ_Patch, SZ_Status, SZ_StatusTime, SZ_Type, SZ_SystemCreateTimeUtc, SZ_SystemCreateUser, SZ_SystemLastEditTimeUtc, SZ_SystemLastEditUser) values (@Pk, GetDate(), @MajorVersion, @MinorVersion, @Release, @Patch, @Status, @StatusTime, @Type, GetUtcDate(), '~BP', GetUtcDate(), '~BP')")) // No need to create the whole BO for this test
			{
				cmd.AddParameter("Pk", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("MajorVersion", SqlDbType.Int, version.Major);
				cmd.AddParameter("MinorVersion", SqlDbType.Int, version.Minor);
				cmd.AddParameter("Release", SqlDbType.Int, version.Build);
				cmd.AddParameter("Patch", SqlDbType.Int, version.Revision);
				cmd.AddParameter("Status", SqlDbType.VarChar, status);
				cmd.AddParameter("StatusTime", SqlDbType.DateTime, statusTime);
				cmd.AddParameter("Type", SqlDbType.VarChar, type);
				cmd.ExecuteNonQuery();
			}
			return pk;
		}

		bool UpgradePackageExists(Guid pk)
		{
			using (var cmd = Db.Connection.Command("select count(*) from dbo.StmUpgrade where SZ_PK = @Pk"))
			{
				cmd.AddParameter("Pk", SqlDbType.UniqueIdentifier, pk);
				return (int)cmd.ExecuteScalar() > 0;
			}
		}

		static string TestEdpFileName
		{
			get { return Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\MasterFiles\Business\MasterFiles.Business\System\StmUpgrade\testing.edp"); }
		}
	}
}
