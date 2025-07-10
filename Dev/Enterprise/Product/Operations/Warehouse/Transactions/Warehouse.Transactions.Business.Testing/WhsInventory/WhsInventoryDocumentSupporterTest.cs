using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsInventoryDocumentSupporter))]
	internal class WhsInventoryDocumentSupporterTest : WhsDocumentSupporterTestCase
	{
		public override void TestGetDocBusinessObjects()
		{
			IBODocDataProvider[] docBizoList = DocSupporter.GetDocumentWrappers(DataContext, null);
			AssertEquals(1, docBizoList.Length);
			AssertNotNull(docBizoList[0]);
			AssertEquals(docBizoList[0].ParentBusinessObject, ((WhsInventoryView)BusinessObject).InDocketLine);
		}

		public override void TestBusinessContext()
		{
			AssertEquals(BusinessContext.WhsInventory, DocSupporter.BusinessContext);
		}

		public override void TestSupportedDataContexts()
		{
			var inventory = (WhsInventoryView)GetNewBusinessObject();
			AssertEquals("Core.Constants.DataContext.WhsInventory is Supported", true, DocSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.WhsInventory)));
			AssertEquals("Core.Constants.DataContext.WhsReceive is Supported", true, DocSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.WhsReceive)));
			AssertEquals("Core.Constants.DataContext.GenericFreightJob is Supported", true, DocSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericFreightJob)));
		}

		#region TestGetDocumentWrappers

		#region TestGetDocumentWrappers

		public void TestGetDocumentWrappers()
		{
			TestDataForInventory data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryWithSaveFactoryForWarehouse();

			DocumentWrapper[] wrapper = data.Line111.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.WhsPick, null);
			AssertNull("Shouldn't return wrappers for documents with wrong DataContext", wrapper);

			wrapper = data.Line111.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.WhsInventory, null);
			AssertNotNull(wrapper);
			AssertEquals(1, wrapper.Length);
			AssertEquals("Wrapper should be correct type", "DocWhsInventory", wrapper[0].GetType().Name);
			AssertNull(data.Receive11.InventoryToPrintPalletLabelFor);

			wrapper = data.Line111.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.WhsReceive, null);
			AssertNotNull(wrapper);
			AssertEquals(1, wrapper.Length);
			AssertEquals("Wrapper should be correct type", "DocWhsReceive", wrapper[0].GetType().Name);
			AssertEquals(data.Line111, data.Receive11.InventoryToPrintPalletLabelFor);

			WhsAdjustment adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			WhsAdjustmentLine adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10, data.Line111.Location);
			adjustment.FinaliseDocket();
			var wrapperFromAdjustmentLine = adjustmentLine.Inventory[0].DocumentSupporter.GetDocumentWrappers(Constants.DataContext.WhsReceive, null);

			AssertNull("Shouldn't return wrappers for Inventories created from Adjustments or Transfers", wrapperFromAdjustmentLine);
		}

		#endregion

		#region TestGetDocumentWrappers_ForPalletIDLabels_DocStripDocument

		public void TestGetDocumentWrappers_ForPalletIDLabels_DocStripDocument()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			// Received Inventory
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT-1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation, "PLT-2");
			var inventoryReceivedDocumentSupporter = new WhsInventoryDocumentSupporter(inventory2);
			var inventoryReceivedWrappers = inventoryReceivedDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, null);
			AssertEquals("Wrapper should be created", 1, inventoryReceivedWrappers.Length);
			AssertEquals("Wrapper type should be FreightWrapperFromWhsBO", "FreightWrapperFromWhsBO", inventoryReceivedWrappers[0].GetType().Name);
			AssertEquals("When printing a doc strip Pallet Label Document from Inventory, a single inventory line should be specified.", inventory2, receive.InventoryToPrintPalletLabelFor);

			// Adjusted Inventory
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ1", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, "A-1", "PLT-3");
			adjustment.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustment);

			Factory.Save();

			var adjustedInventory = Factory.LoadTop1<WhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, adjustmentLine.PK));
			var inventoryAdjustedDocumentSupporter = new WhsInventoryDocumentSupporter(adjustedInventory);
			var inventoryAdjustedWrappers = inventoryAdjustedDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, null);
			AssertEquals("Wrapper should be created", 1, inventoryAdjustedWrappers.Length);
			AssertEquals("Wrapper type should be FreightWrapperFromWhsBO", "FreightWrapperFromWhsBO", inventoryAdjustedWrappers[0].GetType().Name);
			AssertEquals("When printing a doc strip Pallet Label Document from Inventory, a single inventory line should be specified.", adjustedInventory, adjustment.InventoryToPrintPalletLabelFor);

			// Transfered Inventory
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-3", "A-2", "PLT-4");
			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer);

			var inventoryTransferedDocumentSupporter = new WhsInventoryDocumentSupporter(transferLine.Inventory[0]);
			var inventoryTransferedWrappers = inventoryTransferedDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, null);
			AssertEquals("Wrapper should be created", 1, inventoryTransferedWrappers.Length);
			AssertEquals("Wrapper type should be FreightWrapperFromWhsBO", "FreightWrapperFromWhsBO", inventoryTransferedWrappers[0].GetType().Name);
			AssertEquals("When printing a doc strip Pallet Label Document from Inventory, a single inventory line should be specified.", transferLine.Inventory[0], transfer.InventoryToPrintPalletLabelFor);
		}

		#endregion

		#region TestGetDocumentWrappers_ProductLabels

		#region TestGetDocumentWrappers_ProductLabelsForReceive

		public void TestGetDocumentWrappers_ProductLabelsForReceive()
		{
			var continueToPrint = false;
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inv1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "122");
			var inv2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "123");
			Factory.Save();

			inv1.WI_TotalUnits = 5;
			inv2.WI_TotalUnits = 1;
			inv1.OnInventoryPrint += delegate(object sender, WhsDocumentInventoryEventArgs e)
			{ e.ContinueToPrint = continueToPrint; };
			var wrappers = inv1.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductLabel, null);
			AssertEquals("Wrapper should be null", 0, wrappers.Length);

			continueToPrint = true;
			wrappers = inv1.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductLabel, null);
			AssertEquals("Wrapper should be created", 1, wrappers.Length);
			AssertEquals("Wrapper type should be DocWhsReceive", "FreightWrapperFromWhsBO", wrappers[0].GetType().Name);
			AssertEquals(typeof(WhsReceive), wrappers[0].WrappedObject.GetType());
			AssertEquals(receive, wrappers[0].WrappedObject);

			var wrapper = wrappers[0];
			var iWrapper = (IDocTypeCode)wrapper;
			iWrapper.DocTypeCode = "WDL";

			var warehouseJobWrapper = (DocumentWrapper)wrapper["WarehouseJob"];
			var jobLines = (DocumentWrapperCollection)warehouseJobWrapper["JobLines"];
			AssertEquals(5, jobLines.Count);
			AssertEquals(inv1.InDocketLine, jobLines[0].WrappedObject);
			AssertEquals(inv1.InDocketLine, jobLines[1].WrappedObject);
			AssertEquals(inv1.InDocketLine, jobLines[2].WrappedObject);
			AssertEquals(inv1.InDocketLine, jobLines[3].WrappedObject);
			AssertEquals(inv1.InDocketLine, jobLines[4].WrappedObject);
		}

		#endregion

		#region TestGetDocumentWrappers_ProductLabelsForAdjustments

		public void TestGetDocumentWrappers_ProductLabelsForAdjustments()
		{
			var continueToPrint = false;

			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "1", Notify);
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2, "A");
			adjustment.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(adjustment);

			var inventoryLine = line.Inventory.Cast<WhsInventoryView>().Single();

			inventoryLine.OnInventoryPrint += (sender, e) => e.ContinueToPrint = continueToPrint;
			var wrappers = inventoryLine.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductLabel, null);
			AssertEquals("Wrapper should be null", 0, wrappers.Length);

			continueToPrint = true;
			wrappers = inventoryLine.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductLabel, null);
			AssertEquals("Wrapper should be created", 1, wrappers.Length);
			AssertEquals("Wrapper type should be FreightWrapperFromWhsBO", "FreightWrapperFromWhsBO", wrappers[0].GetType().Name);
			AssertEquals(typeof(WhsAdjustment), wrappers[0].WrappedObject.GetType());
			AssertEquals(adjustment, wrappers[0].WrappedObject);

			var wrapper = wrappers[0];
			var iWrapper = (IDocTypeCode)wrapper;
			iWrapper.DocTypeCode = "WDL";

			var warehouseJobWrapper = (DocumentWrapper)wrapper["WarehouseJob"];
			var jobLines = (DocumentWrapperCollection)warehouseJobWrapper["JobLines"];
			AssertEquals("Precondition", 2, jobLines.Count);
			AssertEquals(inventoryLine.InDocketLine, jobLines[0].WrappedObject);
			AssertEquals(inventoryLine.InDocketLine, jobLines[1].WrappedObject);
		}

		#endregion

		#region TestGetDocumentWrappers_ProductLabelsForInnerWarehouseTransfers

		public void TestGetDocumentWrappers_ProductLabelsForInnerWarehouseTransfers()
		{
			var continueToPrint = false;

			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var sourceLocation = data.Whs1.FindLocation("A-1-1");
			var destinationLocation = data.Whs1.FindLocation("A-1-2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 3, sourceLocation, "", true, true);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			var line = Helper.CreateWhsTransferLine(transfer, data.Part1, 2, sourceLocation, destinationLocation);
			transfer.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(transfer);

			line.Inventory.Load();
			var inventoryLine = line.Inventory.Cast<WhsInventoryView>().Single();
			inventoryLine.OnInventoryPrint += (sender, e) => e.ContinueToPrint = continueToPrint;
			var wrappers = inventoryLine.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductLabel, null);
			AssertEquals("Wrapper should be null", 0, wrappers.Length);

			continueToPrint = true;
			wrappers = inventoryLine.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductLabel, null);
			AssertEquals("Wrapper should be created", 1, wrappers.Length);
			AssertEquals("Wrapper type should be FreightWrapperFromWhsBO", "FreightWrapperFromWhsBO", wrappers[0].GetType().Name);
			AssertEquals(typeof(WhsReceive), wrappers[0].WrappedObject.GetType());
			AssertEquals(receive, wrappers[0].WrappedObject);

			var wrapper = wrappers[0];
			var iWrapper = (IDocTypeCode)wrapper;
			iWrapper.DocTypeCode = "WDL";

			var warehouseJobWrapper = (DocumentWrapper)wrapper["WarehouseJob"];
			var jobLines = (DocumentWrapperCollection)warehouseJobWrapper["JobLines"];
			AssertEquals("Precondition", 2, jobLines.Count);
			AssertEquals(inventoryLine.InDocketLine, jobLines[0].WrappedObject);
			AssertEquals(inventoryLine.InDocketLine, jobLines[1].WrappedObject);
		}

		#endregion

		#region TestGetDocumentWrappers_ProductLabelsForInterWarehouseTransfers

		public void TestGetDocumentWrappers_ProductLabelsForInterWarehouseTransfers()
		{
			var continueToPrint = false;

			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var destinationWarehouse = Helper.CreateWarehouse("W2", "R1");
			Factory.Save();
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 3, data.Whs1.FindLocation("A"), "", true, true);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", new TestNotificationBuffer(), TransferType.Codes.InterWhsSource);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2, "A", destinationWarehouse.PK, "R1");
			transfer.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(transfer);

			var destinationTransfer = transfer.ChildTransfers.Single();
			var line = transfer.ChildTransfers.ElementAt(0).Lines.Single();
			var inventoryLine = line.Inventory.Cast<WhsInventoryView>().Single();
			inventoryLine.OnInventoryPrint += (sender, e) => e.ContinueToPrint = continueToPrint;
			var wrappers = inventoryLine.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductLabel, null);
			AssertEquals("Wrapper should be null", 0, wrappers.Length);

			continueToPrint = true;
			wrappers = inventoryLine.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductLabel, null);
			AssertEquals("Wrapper should be created", 1, wrappers.Length);
			AssertEquals("Wrapper type should be FreightWrapperFromWhsBO", "FreightWrapperFromWhsBO", wrappers[0].GetType().Name);
			AssertEquals(typeof(WhsTransfer), wrappers[0].WrappedObject.GetType());
			AssertEquals(destinationTransfer, wrappers[0].WrappedObject);

			var wrapper = wrappers[0];
			var iWrapper = (IDocTypeCode)wrapper;
			iWrapper.DocTypeCode = "WDL";

			var warehouseJobWrapper = (DocumentWrapper)wrapper["WarehouseJob"];
			var jobLines = (DocumentWrapperCollection)warehouseJobWrapper["JobLines"];
			AssertEquals("Precondition", 2, jobLines.Count);
			AssertEquals(inventoryLine.InDocketLine, jobLines[0].WrappedObject);
			AssertEquals(inventoryLine.InDocketLine, jobLines[1].WrappedObject);
		}

		#endregion

		#endregion

		#endregion

		#region TestGetContactOrganisation

		public override void TestGetContactOrganisation()
		{
			var inventory = (WhsInventoryView)BusinessObject;
			var client = Factory.New<OrgHeader>();
			inventory.WI_OH_Client = client.PK;
			var contact = inventory.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Warehouse3PL, DocumentDirection.ANY);
			AssertEquals("The contact should be the client", inventory.Client.PK, contact.OrgHeader.PK);
		}

		#endregion

		#region TestGetIsNonPersistent

		public void TestGetIsNonPersistent()
		{
			//in order to show menu   MenuShouldShowFor(DocumentSupporter documentSupporter)
			// it needs to set as non persistant object when is in database is always false
			//(documentSupporter.IsInDatabase || documentSupporter.IsNonPersistent) && (documentSupporter.IgnoreHasChanges || !documentSupporter.HasChanges);

			var data = new TestDataSimpleEnvironment(Factory);
			var destinationWarehouse = Helper.CreateWarehouse("W2", "R1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 3, data.Whs1.FindLocation("A"), "", true, false);
			Factory.Save();

			var inventory = receive.Inventory[0];
			Assert("For inventory view is always true", inventory.DocumentSupporter.IsNonPersistent);
			AssertEquals("Precondition", false, inventory.DocumentSupporter.HasChanges);

			inventory.OriginalInventoryHeldCode = "AAA";
			Assert("After change receive line should return true", inventory.DocumentSupporter.HasChanges);
		}

		#endregion

		#region TestGetBODocDataProvidersNotFoundMessageReturnOneReason

		protected override IEnumerable<Tuple<Constants.DataContext, string>> SupportedDataContextAndNotFoundMessageReasonPairs
		{
			get
			{
				var dataContextAndMessagePairs = new List<Tuple<Constants.DataContext, string>>();

				dataContextAndMessagePairs.Add(new Tuple<Constants.DataContext, string>(Constants.DataContext.GenericProductLabel, "Cannot find Product Label to print."));
				dataContextAndMessagePairs.Add(new Tuple<Constants.DataContext, string>(Constants.DataContext.WhsInventory, "Cannot find Docket Line."));
				dataContextAndMessagePairs.Add(new Tuple<Constants.DataContext, string>(Constants.DataContext.GenericFreightJob, "Cannot find Docket."));
				dataContextAndMessagePairs.Add(new Tuple<Constants.DataContext, string>(Constants.DataContext.WhsReceive, "Cannot find Original Docket."));

				return dataContextAndMessagePairs;
			}
		}

		#endregion

		#region Implementation

		protected override Constants.DataContext DataContext
		{
			get { return Constants.DataContext.WhsInventory; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Ref1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 11m, "A");
			receive.RunPreSaveValidation();
			return inventory;
		}

		protected override ISecurityCheckpoint ExpectedCustomisationSecurityCheckpoint => Env.Security.WhsInventoryCustomiseDocuments;

		#endregion
	}
}
