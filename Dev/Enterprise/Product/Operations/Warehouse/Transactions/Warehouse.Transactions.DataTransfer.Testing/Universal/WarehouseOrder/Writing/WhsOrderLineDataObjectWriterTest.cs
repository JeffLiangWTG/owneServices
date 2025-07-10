using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsOrderLineDataObjectWriterTest : WhsPickableDocketLineDataObjectWriterTest<WhsOrderLine, WhsOrderLineDataObjectWriter>
	{
		#region LinksDictionary

		public void TestWhsOrderLineDataObjectWriter_NullLinksDictionary()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new WhsOrderLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, GetOrderLine(Factory.BOFactory))), null));
		}

		public void TestWhsOrderLineDataObjectWriter_LinksDictionary()
		{
			var factory = new BusinessObjectFactory();
			var warehouseHelper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, saveFactory_doNotUseForNewTests: false);
			var receive = warehouseHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			factory.Save();

			Assert("Precondition", receive.IsFinalised);
			var order = warehouseHelper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.CarrierBookingAgentDocAddress.OrganisationPK = warehouseHelper.CreateClient("RTUS").PK;
			var orderLine1 = warehouseHelper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = warehouseHelper.CreateWhsOrderLine(order, data.Part1, 15m);
			warehouseHelper.CreatePickNew(order);
			factory.Save();

			var dictionary = new Dictionary<ZGuid, ZInt>();
			var writer1 = new WhsOrderLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderLine1)), dictionary);
			writer1.GetDataObject(orderLine1);
			AssertEquals(1, writer1.LinksDictionary.Count);
			AssertEquals(1, dictionary.Count);
			Assert(writer1.LinksDictionary.ContainsKey(orderLine1.ReleaseLines[0].PK));

			var writer2 = new WhsOrderLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderLine2)), dictionary);
			writer2.GetDataObject(orderLine2);
			AssertEquals(2, writer1.LinksDictionary.Count);
			AssertEquals(2, dictionary.Count);
			Assert(writer2.LinksDictionary.ContainsKey(orderLine1.ReleaseLines[0].PK));
			Assert(writer2.LinksDictionary.ContainsKey(orderLine2.ReleaseLines[0].PK));
		}

		#endregion

		#region TestPalletID

		public void TestPalletID_ExportPalletID()
		{
			var factory = new BusinessObjectFactory();
			var docketLine = GetNewDocketLine();
			docketLine.WE_PalletID = "PLT1";
			AssertNotNullOrEmpty("Precondition: Non empty pallet id", docketLine.WE_PalletID);

			var writer = GetNewDataObjectWriter(docketLine.Docket);
			var docketLineDataObject = writer.GetDataObject(docketLine);
			AssertNotNull("Precondition: docketLineDataObject", docketLineDataObject);
			AssertEquals("PalletID is expected", "PLT1", docketLineDataObject.PalletID);
		}

		#endregion

		#region TestHeldCode

		public void TestHeldCode_ExportOrderedHeldCode()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10);
			orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Damaged;

			var writer = GetNewDataObjectWriter(order);
			var docketLineDataObject = writer.GetDataObject(orderLine);
			AssertNotNull("Precondition: docketLineDataObject", docketLineDataObject);
			AssertNotNull("CurrentHoldCode is expected", docketLineDataObject.CurrentHoldCode);
			AssertEquals(InventoryHoldCodes.Codes.Damaged, docketLineDataObject.CurrentHoldCode.Code);
			AssertEquals(InventoryHoldCodes.Descriptions.Damaged, docketLineDataObject.CurrentHoldCode.Description);
		}

		#endregion

		#region Implementation

		protected override DataContextType DataContextType => DataContextType.WarehouseOrder;

		protected override WhsOrderLine GetNewDocketLine()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			return Helper.CreateWhsOrderLine(order, data.Part1, 10m);
		}

		protected override WhsOrderLineDataObjectWriter GetNewDataObjectWriter(BusinessObject topLevelBO)
		{
			return new WhsOrderLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, topLevelBO)), new Dictionary<ZGuid, ZInt>());
		}

		protected override void GetOrderLineCore(WhsTestHelperFunctions helper, WhsOrderLine docketLine)
		{
			helper.CreateReservePickLine(docketLine, helper.Factory.NewWithValidTestData<WhsInventoryView>(), 2.4m);
		}

		protected override void AssertContentsCore(OrderLine lineData)
		{
			AssertEquals("lineData.ReservedQuantity", 2.4m, lineData.ReservedQuantity);
			AssertEquals("lineData.ShortfallQuantity", 11.2m, lineData.ShortfallQuantity);
		}

		protected override WhsPickableDocket GetDocket(BusinessObjectFactory factory) => Factory.NewWithValidTestData<WhsOrder>();

		protected override void AssertCustomsData(CustomsEntryInfo customsData)
		{
			WhsOrderBondedWarehouseAttributeDataObjectWriterTest.AssertContents(customsData);
		}

		#endregion
	}
}
