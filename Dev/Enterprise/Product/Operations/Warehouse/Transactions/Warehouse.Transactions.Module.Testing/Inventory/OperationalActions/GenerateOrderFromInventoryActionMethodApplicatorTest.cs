using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(GenerateOrderFromInventoryActionMethodApplicator))]
	public class GenerateOrderFromInventoryActionMethodApplicatorTest : GenerateOrderFromExistingInventoryActionMethodApplicatorTest
	{
		#region TestGenerateOrder_NonReceiveInventory

		public void TestGenerateOrder_NonReceiveInventory_TransferInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var consignee = Helper.CreateClient("CONSIGNEE");
			Applicator.ConsigneePK = consignee.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, data.Whs1.FindLocation("A-1"));
			receive.WD_ArrivalDate = ZDateTimeOffset.Today;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			transferLine.RunPreSaveValidation(); // to commit inventory
			transfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transfer);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transferLine);

			var transferInventory = Factory.Load<WhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, transferLine.PK)).Single();

			string expectedLogText =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000003] has been generated.\r\n";

			const bool SaveOnSuccess = true;
			AssertNoExceptionThrown("There should be no exceptions thrown.", () => ApplyApplicator(new[] { transferInventory }, expectedLogText, SaveOnSuccess));

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Should be 1 order", 1, orders.Length);
			AssertEquals("Should be 1 order lines", 1, orders[0].Lines.Count);
			AssertEquals("Order line's transaction quantity is transfer line's transaction quantity.", 10m, orders[0].Lines[0].WE_TransactionQuantity);
			Factory.Save();

			AssertNull("No consignee address is created for transfer.", JobDocAddress.Load(transfer, DocAddressType.ConsigneeAddress, false));
			var transferLineConsigneeAddress = JobDocAddress.Load(transferLine, DocAddressType.ConsigneeAddress, false);
			AssertNotNull("Consignee address was created for the transfer line.", transferLineConsigneeAddress);
			AssertEquals("Transfer line consignee address is not persisted to DB.", false, transferLineConsigneeAddress.IsPersistent);
			AssertEquals("Transfer line consignee address is not persisted to DB.", false, transferLineConsigneeAddress.IsInDatabase);

			var orderConsigneeAddress = JobDocAddress.Load(orders.Single(), DocAddressType.ConsigneeAddress, false);
			AssertNotNull("Order has a consignee address.", orderConsigneeAddress);
			AssertEquals("Consignee is correct.", consignee.PK, orderConsigneeAddress.OrganisationPK);
		}

		public void TestGenerateOrder_NonReceiveInventory_AdjustmentInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var consignee = Helper.CreateClient("CONSIGNEE");
			Applicator.ConsigneePK = consignee.PK;

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, data.Whs1.DefaultLocation);
			adjustment.FinaliseDocket();

			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(adjustment);
			Factory.Save();

			var adjustmentInventory = Factory.Load<WhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, adjustmentLine.PK)).Single();

			string expectedLogText =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000002] has been generated.\r\n";

			const bool SaveOnSuccess = true;
			AssertNoExceptionThrown("There should be no exceptions thrown.", () => ApplyApplicator(new[] { adjustmentInventory }, expectedLogText, SaveOnSuccess));

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Should be 1 order", 1, orders.Length);
			AssertEquals("Should be 1 order lines", 1, orders[0].Lines.Count);
			AssertEquals("Order line's transaction quantity is adjustment line's transaction quantity.", 10m, orders[0].Lines[0].WE_TransactionQuantity);
			Factory.Save();

			AssertNull("No consignee address is created for adjustment.", JobDocAddress.Load(adjustment, DocAddressType.ConsigneeAddress, false));
			var adjustmentLineConsigneeAddress = JobDocAddress.Load(adjustmentLine, DocAddressType.ConsigneeAddress, false);
			AssertNotNull("Consignee address was created for the adjustment line.", adjustmentLineConsigneeAddress);
			AssertEquals("Adjustment line consignee address is not persisted to DB.", false, adjustmentLineConsigneeAddress.IsPersistent);
			AssertEquals("Adjustment line consignee address is not persisted to DB.", false, adjustmentLineConsigneeAddress.IsInDatabase);

			var orderConsigneeAddress = JobDocAddress.Load(orders.Single(), DocAddressType.ConsigneeAddress, false);
			AssertNotNull("Order has a consignee address.", orderConsigneeAddress);
			AssertEquals("Consignee is correct.", consignee.PK, orderConsigneeAddress.OrganisationPK);
		}

		#endregion

		#region TestGenerateOrder_TransferredInventoryInBondedWarehouse

		public void TestGenerateOrder_TransferredInventoryInBondedWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedAreaWhs = Helper.CreateArea(data.Whs1, "C", AreaTypes.Codes.Bonded);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "CC");
			Factory.Save();

			var bondedLocation = data.Whs1.FindLocation("CC");
			bondedLocation.WLV_WA_PickingArea = bondedAreaWhs.PK;
			bondedLocation.WLV_WA_PutawayArea = bondedAreaWhs.PK;

			var consignee = Helper.CreateClient("CONSIGNEE");
			Applicator.ConsigneePK = consignee.PK;
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R4");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 15m, location1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T4");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer);
			Factory.Save();

			string expectedLogText =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000003] has been generated.\r\n";

			const bool SaveOnSuccess = true;
			ApplyApplicator(new[] { transferLine.Inventory[0] }, expectedLogText, SaveOnSuccess);

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Should be 1 order", 1, orders.Length);
			AssertEquals("Should be normal order", OrderType.Codes.Order, orders[0].WD_DocketSubType);
			AssertEquals("Should be 1 order lines", 1, orders[0].Lines.Count);
			AssertEquals("Order line's transaction quantity is receive line's client ordered units.", 10m, orders[0].Lines[0].WE_TransactionQuantity);
		}

		#endregion

		#region TestGenerateOrder_InterWhsTransferredInventoryInBondedWarehouse

		public void TestGenerateOrder_InterWhsTransferredInventoryInBondedWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var whs2 = Helper.CreateWarehouse("Warehouse1", "A", 3, 3);
			whs2.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			Helper.EnableWarehouseForBond(whs2, true);

			var bondedAreaWhs1 = Helper.CreateArea(data.Whs1, "C", AreaTypes.Codes.Bonded);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "CC");

			var bondedAreaWhs2 = Helper.CreateArea(whs2, "D", AreaTypes.Codes.Bonded);
			Helper.CreateRowAndGenerateLocations(whs2, "DD");
			Factory.Save();

			var bondedLocation1 = data.Whs1.FindLocation("CC");
			bondedLocation1.WLV_WA_PickingArea = bondedAreaWhs1.PK;
			bondedLocation1.WLV_WA_PutawayArea = bondedAreaWhs1.PK;

			var bondedLocation2 = whs2.FindLocation("DD");
			bondedLocation2.WLV_WA_PickingArea = bondedAreaWhs2.PK;
			bondedLocation2.WLV_WA_PutawayArea = bondedAreaWhs2.PK;

			var consignee = Helper.CreateClient("CONSIGNEE");
			Applicator.ConsigneePK = consignee.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R4");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var interWhsDestTransfer = Helper.CreateWhsTransfer(data.Org1, whs2, "TR2");
			interWhsDestTransfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			var interWhsDestTransferLine = Helper.CreateWhsTransferLine(interWhsDestTransfer, data.Part1.PK, 10m, "A-1", data.Whs1.PK, "A-1-2");
			interWhsDestTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(interWhsDestTransfer);
			Factory.Save();

			string expectedLogText =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000004] has been generated.\r\n";

			const bool SaveOnSuccess = true;
			ApplyApplicator(new[] { interWhsDestTransferLine.Inventory[0] }, expectedLogText, SaveOnSuccess);

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Should be 1 order", 1, orders.Length);
			var order = orders[0];
			AssertEquals("Should be normal order", OrderType.Codes.Order, order.WD_DocketSubType);
			AssertEquals("Should be in correct WHS", whs2.PK, order.WD_WW_Whs);
			AssertEquals("Should be 1 order lines", 1, order.Lines.Count);
			AssertEquals("Order line's transaction quantity is receive line's client ordered units.", 10m, order.Lines[0].WE_TransactionQuantity);
		}

		#endregion

		#region Implementation

		protected override BusinessObject[] GetInventoryOrReceives()
		{
			return Factory.Load<WhsInventoryView>(new ZQuery());
		}

		protected override WhsOperationalActionSupporter GetOperationalActionSupporter()
		{
			return new InventoryOperationalActionSupporter();
		}

		protected override GenerateOrderFromExistingInventoryActionMethodApplicator GetApplicator(BusinessObjectFactory factory)
		{
			return new GenerateOrderFromInventoryActionMethodApplicator(factory);
		}

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion

		new GenerateOrderFromInventoryActionMethodApplicator Applicator => (GenerateOrderFromInventoryActionMethodApplicator)base.Applicator;
	}
}
