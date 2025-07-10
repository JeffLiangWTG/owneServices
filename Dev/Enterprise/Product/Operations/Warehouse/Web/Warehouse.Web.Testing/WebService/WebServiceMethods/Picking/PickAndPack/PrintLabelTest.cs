using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class PrintLabelTest : WhsSecureServiceTestCase
	{
		#region TestPrintLabel

		public void TestPrintLabel()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var printer = Helper.CreatePrintQueue("PRINTER");
			order.PackageJob.Packages.AddNew("PLT", "ABC");

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PrintLabel(order.WD_DocketID, "ABC", printer.PK.ToGuid(), 2);

			AssertSuccessfulResponse(response, webService);

			var printJobQuery = new ZQuery();
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_SQ, printer.PK);
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_ParentGuid, order.PackageJob.PK);
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_JobType, "PRN");
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_Copies, (short)2);
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_DocumentName, "Product/Delivery" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling);
			AssertNotNull("Should have created the correct Print Job.", webService.Factory.LoadTop1<IStmPrintJob>(printJobQuery));
		}

		public void TestPrintLabel_DocketIDDoesNotExist()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var printer = Helper.CreatePrintQueue("PRINTER");
			order.PackageJob.Packages.AddNew("PLT", "ABC");

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PrintLabel(receive.WD_DocketID, "ABC", printer.PK.ToGuid(), 2);

			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals($"No Warehouse Order found with Docket ID: {receive.WD_DocketID}.", response.ErrorMessage);
		}

		public void TestPrintLabel_PackageIDDoesNotExist()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var printer = Helper.CreatePrintQueue("PRINTER");
			order.PackageJob.Packages.AddNew("PLT", "ABC");

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PrintLabel(order.WD_DocketID, "DEF", printer.PK.ToGuid(), 2);

			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals($"Package ID 'DEF' does not exist.", response.ErrorMessage);
		}

		public void TestPrintLabel_InvalidPrinter()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var printer = Helper.CreatePrintQueue("PRINTER");
			order.PackageJob.Packages.AddNew("PLT", "ABC");

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PrintLabel(order.WD_DocketID, "ABC", Guid.NewGuid(), 2);

			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("No valid Printer provided.", response.ErrorMessage);
		}

		#endregion
	}
}
