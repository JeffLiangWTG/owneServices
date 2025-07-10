using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class PrintPackageLabelsProcessorTest : TestCaseWithFactory
	{
		#region TestProcess

		public void TestProcess()
		{
			var printer = Factory.New<IStmPrintQueue>();
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var helper = new WhsTestHelperFunctions(Factory);

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			helper.CreateProductUnit(data.Part1, "CAS", 5);
			helper.CreateProductUnit(data.Part1, "PLT", 10);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType = UOMPackTypesList.Codes.Pallet;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 15m);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 25m);
			var pick = helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("Precondition: no documents delivered", 0, printJobs.Length);

			new PrintPackageLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());
			printJobs = Factory.Load<StmPrintJob>(new ZQuery()).OrderBy(spj => spj.SP_Sequence).ToArray();

			AssertEquals("Should be 6 documents.", 6, printJobs.Length);
			AssertEquals("Product/Delivery" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJobs[0].SP_DocumentName);
			AssertEquals("Product/Delivery" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJobs[1].SP_DocumentName);
			AssertEquals("End of Pallet" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJobs[2].SP_DocumentName);
			AssertEquals("Product/Delivery" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJobs[3].SP_DocumentName);
			AssertEquals("End of Case" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJobs[4].SP_DocumentName);
			AssertEquals("End of Area" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJobs[5].SP_DocumentName);

			foreach (var printJob in printJobs)
			{
				AssertEquals("JobType should be PRN", "PRN", printJob.SP_JobType);
				AssertEquals("Parent", order.PackageJob.PK, printJob.SP_ParentGuid);
				AssertNotNull("Should have set printer.", printJob.PrintQueue);
				AssertEquals("Should have set printer.", printer.PK, printJob.SP_SQ);
			}

			foreach (var package in pick.OuterPackages)
			{
				AssertEquals(true, package.IsLabelPrinted);
			}
		}

		public void TestProcess_PickGroup()
		{
			var printer = Factory.New<IStmPrintQueue>();
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var helper = new WhsTestHelperFunctions(Factory);

			data.Part1.PartUnits.RemoveAndDeleteAll();
			helper.CreateProductUnit(data.Part1, "CAS", 5);
			helper.CreateProductUnit(data.Part1, "PLT", 10);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType = UOMPackTypesList.Codes.Pallet;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 25m);
			Factory.Save();

			var pickGroupCollection = new PickGroupCollection();
			var pickGroup1 = pickGroupCollection.AddNew();
			var pickGroup2 = pickGroupCollection.AddNew();
			pickGroup1.Description = (NoResString)"Group 1";
			pickGroup2.Description = (NoResString)"Group 2";
			WarehouseDataRegistry.Instance.PickGroups.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pickGroupCollection);

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			order1.Lines[0].WE_PickGroup = new ZShort(pickGroup2.Code);
			order2.Lines[0].WE_PickGroup = new ZShort(pickGroup1.Code);

			var pick = helper.CreatePickNew(order1, order2);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("Precondition: No documents delivered", 0, printJobs.Length);

			new PrintPackageLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());
			printJobs = Factory.Load<StmPrintJob>(new ZQuery()).OrderBy(spj => spj.SP_Sequence).ToArray();

			AssertEquals("Should be 7 documents.", 7, printJobs.Length);
			AssertEquals("Product/Delivery" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJobs[0].SP_DocumentName);
			AssertEquals("End of Pick Group" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJobs[1].SP_DocumentName);
			AssertEquals("End of Pallet" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJobs[2].SP_DocumentName);
			AssertEquals("Product/Delivery" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJobs[3].SP_DocumentName);
			AssertEquals("End of Pick Group" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJobs[4].SP_DocumentName);
			AssertEquals("End of Case" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJobs[5].SP_DocumentName);
			AssertEquals("End of Area" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJobs[6].SP_DocumentName);

			foreach (var printJob in printJobs)
			{
				AssertEquals("JobType should be PRN", "PRN", printJob.SP_JobType);
				AssertNotNull("Should have set printer.", printJob.PrintQueue);
				AssertEquals("Should have set printer.", printer.PK, printJob.SP_SQ);
			}

			foreach (var package in pick.OuterPackages)
			{
				AssertEquals(true, package.IsLabelPrinted);
			}
		}

		#endregion
	}
}
