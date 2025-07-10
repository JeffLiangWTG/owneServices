using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class PrintPalletIdTest : WhsSecureServiceTestCase
	{
		#region TestPrintPalletId

		public void TestPrintPalletId()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			inventory.WI_PalletID = "ABC123";
			var printer = Helper.CreatePrintQueue("PRINTER");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PrintPalletID("ABC123", printer.PK.ToGuid());

			AssertEquals("Successful response, no error returned.", true, string.IsNullOrEmpty(response.ErrorMessage));

			var printJobQuery = new ZQuery();
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_SQ, printer.PK);
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_ParentGuid, inventory.Docket.PK);
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_JobType, "PRN");
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_Copies, (short)1);
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_DocumentName, "Pallet Labels" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling);
			AssertNotNull("Should have created the correct Print Job.", webService.Factory.LoadTop1<IStmPrintJob>(printJobQuery));
		}

		public void TestPrintPalletId_NoInventory()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var printer = Helper.CreatePrintQueue("PRINTER");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PrintPalletID("ABC123", printer.PK.ToGuid());

			AssertEquals("Pallet Id ABC123 does not exist on any inventory in this warehouse.", response.ErrorMessage);
		}

		public void TestPrintPalletId_InventoryNotInWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			inventory.WI_PalletID = "ABC123";
			var printer = Helper.CreatePrintQueue("PRINTER");
			var whs2 = Helper.CreateWarehouse("Whs2");
			Helper.Factory.Save();

			var webService = GetNewWebService(whs2);
			var response = webService.PrintPalletID("ABC123", printer.PK.ToGuid());

			AssertEquals("Pallet Id ABC123 does not exist on any inventory in this warehouse.", response.ErrorMessage);
		}

		public void TestPrintPalletId_InvalidPrinter()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			inventory.WI_PalletID = "ABC123";
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PrintPalletID("ABC123", new Guid());

			AssertEquals("Invalid Printer provided.", response.ErrorMessage);
		}

		#endregion
	}
}
