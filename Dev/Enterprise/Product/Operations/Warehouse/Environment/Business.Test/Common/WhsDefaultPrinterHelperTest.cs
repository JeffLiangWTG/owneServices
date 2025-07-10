using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Shared;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	#region WhsDefaultPrinterHelperTest class
	public class WhsDefaultPrinterHelperTest : WhsTestCaseWithFactoryEnv
	{
		public void TestCheckForStmDefaultPrinterRecords()
		{
			var queue = Factory.New<IStmPrintQueue>();
			((BusinessObject)queue).FillWithValidTestData();
			queue.SQ_ServerName = "TEST";
			queue.QueueName = "TestPrintQueue";

			var defaultPrinter1 = Factory.New<StmDefaultPrinter>();
			defaultPrinter1.SDP_SubjectTableCode = WhsWarehouseSchema.Constants.Prefix;
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseName = "TestWarehouse";
			defaultPrinter1.SDP_SubjectID = warehouse.PK;
			defaultPrinter1.SDP_SQ_Printer = queue.PK;

			var defaultPrinter2 = Factory.New<StmDefaultPrinter>();
			defaultPrinter2.SDP_SubjectTableCode = WhsAreaSchema.Constants.Prefix;
			var area = Factory.NewWithValidTestData<WhsArea>();
			area.WA_Name = "TestArea";
			defaultPrinter2.SDP_SubjectID = area.PK;
			defaultPrinter2.SDP_SQ_Printer = queue.PK;

			Factory.Save();

			var sb = new ZStringBuilder();

			var helper = new WhsDefaultPrinterHelper();
			helper.CheckForStmDefaultPrinterRecords(sb, queue.PK, Factory);

			var errorMessage = @"This record is in use by one or more record(s) of the module Warehouse with the following names, and thus cannot be deleted.
TestWarehouse

This record is in use by one or more record(s) of the module Areas with the following names, and thus cannot be deleted.
TestArea
";
			AssertEquals("error should be same", errorMessage, sb.ToString());
		}
	}

	#endregion
}
