using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class PalletLabelAutoPrinterTest : WhsTestCaseWithFactory
	{
		public void TestPalletLabelAutoPrinter_NullInventory()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new PalletLabelAutoPrinter(null));
		}

		public void TestPrintPalletLabel()
		{
			LabelPrinter.PrintPalletLabel(Printer.PK.ToGuid(), 1);

			var printJobQuery = new ZQuery();
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_SQ, Printer.PK);
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_ParentGuid, Inventory.Docket.PK);
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_JobType, "PRN");
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_Copies, (short)1);
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_DocumentName,
				"Pallet Labels" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling);
			AssertNotNull("Should have created the correct Print Job.", Factory.LoadTop1<IStmPrintJob>(printJobQuery));
			AssertNull(Message);
		}

		public void TestPrintPalletLabel_InvalidPrinter()
		{
			LabelPrinter.PrintPalletLabel(new Guid(), 1);
			AssertEquals("Invalid Printer provided.", Message);
		}

		public void TestPrintPalletLabel_PalletLabelsDocumentMissing()
		{
			var palletLabelsGuid = "DB1EE942-E732-429D-8BAD-5A82E1764BE3";
			TestConnection.ExecuteNonQuery(string.Format($@"
DELETE documentConfigItem
FROM
	dbo.StmMenuDocumentConfigItem documentConfigItem
	JOIN dbo.StmMenuDocumentConfig documentConfig ON documentConfigItem.S4_S3 = documentConfig.S3_PK
	JOIN dbo.StmMenuTemplatePivot documentTemplatePivot ON documentConfig.S3_SI = documentTemplatePivot.SI_PK
WHERE
	SI_SU = '{palletLabelsGuid}'

DELETE documentConfig
FROM
	dbo.StmMenuDocumentConfig documentConfig
	JOIN dbo.StmMenuTemplatePivot documentTemplatePivot ON documentTemplatePivot.SI_PK = documentConfig.S3_SI
WHERE
	SI_SU = '{palletLabelsGuid}'

DELETE FROM dbo.StmMenuTemplatePivot WHERE SI_SU = '{palletLabelsGuid}'
DELETE FROM dbo.StmMenuItem WHERE SU_PK = '{palletLabelsGuid}'"));

			LabelPrinter.PrintPalletLabel(Printer.PK.ToGuid(), 1);
			AssertEquals("Pallet Labels document does not exist.", Message);
		}

		protected override void SetUp()
		{
			base.SetUp();

			Printer = Factory.New<IStmPrintQueue>();
			Printer.QueueName = "ZDesigner LP 2var 844";

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			LabelPrinter = new PalletLabelAutoPrinter(Inventory);

			LabelPrinter.PrintFailed += (sender, e) =>
			{
				Message = e.Message;
			};
		}

		WhsInventoryView Inventory;
		PalletLabelAutoPrinter LabelPrinter;
		IStmPrintQueue Printer;
		string Message;

		protected new WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;
	}
}
