using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	class UpgraderTest : TestCaseWithFactory
	{
		public void TestStoreNewUpgrade()
		{
			byte[] fileContent = System.Text.Encoding.ASCII.GetBytes("TestFile");
			var anUpgrader = new Upgrader();
			anUpgrader.StoreNewUpgrade("Package20040719_121209_1_2_3_4.edp", fileContent);

			StmUpgrade anUpgrade = Factory.LoadTop1<StmUpgrade>(new ZQuery());
			AssertNotNull("An StmUpgrade record should be loaded", anUpgrade);

			AssertEquals("SZ_ExeVersionDate", new ZDateTime(2004, 7, 19, 12, 12, 0), anUpgrade.SZ_ExeVersionDate); //Note we ignore seconds.
			AssertEquals("SZ_UpgradeData", fileContent, (byte[])anUpgrade.SZ_UpgradeData_Compressed);
		}

		public void TestStoreNewUpgradeCalledWithWrongFileType()
		{
			byte[] fileContent = System.Text.Encoding.ASCII.GetBytes("TestFile");
			var anUpgrader = new Upgrader();

			anUpgrader.StoreNewUpgrade("Package20040719_121209.zzz", fileContent);
			AssertEquals("LastMessageReported", "Received Upgrade file is not an edp file and is ignored: Package20040719_121209.zzz", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			StmUpgrade anUpgrade = Factory.LoadTop1<StmUpgrade>(new ZQuery());
			AssertNull("No StmUpgrade record should be loaded", anUpgrade);
		}

		#region Implementation

		protected StmUpgrade CreateUpgrade(int addMajor, int addRelease, ZDateTime statusTime, string status, string statusComment)
		{
			StmUpgrade result = Factory.New<StmUpgrade>();
			result.VersionNumber = ReleaseInfo.Instance.VersionNumber.Add(addMajor, 0, addRelease, 0);
			result.SZ_StatusTime = statusTime;
			result.SZ_Status = status;
			result.SZ_StatusComment = statusComment;
			return result;
		}

		protected override void SetUp()
		{
			TestCaseHelper.ClearTable(StmUpgradeSchema.Constants.TableName);
		}

		#endregion
	}
}
