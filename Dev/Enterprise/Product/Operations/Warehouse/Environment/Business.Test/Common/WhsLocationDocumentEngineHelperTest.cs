using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	#region WhsLocationDocumentEngineHelperTest class
	public class WhsLocationDocumentEngineHelperTest : WhsTestCaseWithFactoryEnv
	{
		public void TestCheckForWhsLocationRecords()
		{
			var queue = Factory.New<IStmPrintQueue>();
			((BusinessObject)queue).FillWithValidTestData();
			queue.SQ_ServerName = "TEST";
			queue.QueueName = "TestPrintQueue";

			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 2);
			var location = data.Whs1.DefaultOutboundDockDoorLocation;
			location.WLV_SQ_DefaultPrintQueue = queue.PK;

			Factory.Save();

			var sb = new ZStringBuilder();
			var helper = new WhsLocationDocumentEngineHelper();
			helper.CheckForWhsLocationRecords(sb, queue.PK, Factory);

			var errorMessage = @"This record is in use by one or more record(s) of the module Locations with the following descriptions, and thus cannot be deleted.";
			AssertContains("should contain", errorMessage, sb.ToString());
		}
	}

	#endregion
}
