using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public class WhsRFRegistryDocumentEngineHelperTest : WhsTestCaseWithFactoryEnv
	{
		public void TestCheckForWhsRFRegistryRecords()
		{
			var queue = Factory.New<IStmPrintQueue>();
			((BusinessObject)queue).FillWithValidTestData();
			queue.SQ_ServerName = "TEST";
			queue.QueueName = "TestPrintQueue";

			var whs = Helper.CreateWarehouse("WH1");
			var staff1 = Helper.CreateGlbStaff("US1", "User1");
			var staff2 = Helper.CreateGlbStaff("US2", "User2");

			var registry1 = Factory.New<WhsRFRegistry>();
			registry1.WRR_WW_Whs = whs.PK;
			registry1.WRR_GS_NKAssignedTo = staff1.GS_Code;
			registry1.WRR_SQ_Printer = queue.PK;
			var registry2 = Factory.New<WhsRFRegistry>();
			registry2.WRR_WW_Whs = whs.PK;
			registry2.WRR_GS_NKAssignedTo = staff2.GS_Code;
			registry2.WRR_SQ_Printer = queue.PK;

			Factory.Save();

			var sb = new ZStringBuilder();
			var helper = new WhsRFRegistryDocumentEngineHelper();
			helper.CheckForWhsRFRegistryRecords(sb, queue.PK, Factory);

			var errorMessage = @"This record is in use by one or more record(s) of the RF Register Settings for the following users, and thus cannot be deleted.";
			AssertContains("should contain", errorMessage, sb.ToString());
			AssertContains("should contain", registry1.WRR_GS_NKAssignedTo, sb.ToString());
			AssertContains("should contain", registry2.WRR_GS_NKAssignedTo, sb.ToString());
		}
	}
}
