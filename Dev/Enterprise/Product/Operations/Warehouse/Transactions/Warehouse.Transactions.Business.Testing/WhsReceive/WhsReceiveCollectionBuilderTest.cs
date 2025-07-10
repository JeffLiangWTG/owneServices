using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsReceiveCollectionBuilderTest : WhsDocketCollectionBuilderTestCase<WhsReceive, LineData>
	{
		#region Finalise All Dockets

		protected override void ProcessDocketBeforeTestFinaliseAllDockets(WhsReceive docket)
		{
			base.ProcessDocketBeforeTestFinaliseAllDockets(docket);
			docket.WD_ArrivalDate = ZDateTimeOffset.Now;
			docket.AllocateLocationsWithMock();
		}

		#endregion

		#region Implementation

		protected override string SetTestAddLineDocketSubType => ReceiveType.Codes.Receipt;

		public void TestPostAddLine()
		{
			var whs = Helper.CreateWarehouse("A", "A");
			var client = Helper.CreateClient("B");
			var part = Helper.CreateProduct(client, "C");
			Factory.Save();

			var receiveCollectionBuilder = new WhsReceiveCollectionBuilder(Factory);
			receiveCollectionBuilder.AddLine(whs, client, "R", ReceiveType.Codes.Receipt, part, 10, "A", "C", "");
			var docket = receiveCollectionBuilder.Dockets.Single();

			AssertEquals(docket.Lines[0].PK, docket.Inventory[0].WI_WE_InDocketLine);
			AssertEquals(docket.Lines[0].PK, docket.Inventory[0].WI_WE_OriginalInDocketLineForRating);
			AssertEquals(CodeLists.DocketType.Codes.Receive, docket.Inventory[0].WI_InDocketLineType);
			AssertEquals(docket.Lines[0].WE_WL, docket.Inventory[0].WI_WL);
			AssertEquals(docket.WD_ArrivalDate, docket.Inventory[0].WI_ArrivalDate);
			AssertEquals(docket.WD_OH_Client, docket.Inventory[0].WI_OH_Client);
			AssertEquals(docket.Lines[0].WE_OP, docket.Inventory[0].WI_OP);
			AssertEquals(docket.Lines[0].WE_TransactionQuantity, docket.Inventory[0].WI_InDocketLineUnits);
			AssertEquals(docket.Lines[0].WE_ClientOrderedUnits, docket.Inventory[0].WI_ExpectedReceiptQuantity);
			AssertEquals(docket.Lines[0].WE_ClientOrderedUnits, docket.Lines[0].WE_TransactionQuantity);
			AssertEquals(docket.Lines[0].LocationString, docket.Inventory[0].LocationString);
			AssertEquals(docket.Lines[0].WE_BondedEntryKey, docket.Inventory[0].WI_BondedEntryKey);
			AssertEquals(docket.Lines[0].WE_ExpiryDate, docket.Inventory[0].WI_ExpiryDate);
			AssertEquals(docket.Lines[0].WE_PackingDate, docket.Inventory[0].WI_PackingDate);
			AssertEquals(docket.Lines[0].WE_PartAttrib1, docket.Inventory[0].WI_PartAttrib1);
			AssertEquals(docket.Lines[0].WE_PartAttrib2, docket.Inventory[0].WI_PartAttrib2);
			AssertEquals(docket.Lines[0].WE_PartAttrib3, docket.Inventory[0].WI_PartAttrib3);
			AssertEquals(docket.Lines[0].WE_SerialNumber, docket.Inventory[0].WI_SerialNumber);
			Assert(docket.IsAutoCreatingReceive);
		}

		public void TestPostAddLine_WithAttributes()
		{
			var whs = Helper.CreateWarehouse("A", "A");
			var client = Helper.CreateClient("B");
			var part = Helper.CreateProduct(client, "C");
			Factory.Save();

			var receiveCollectionBuilder = new WhsReceiveCollectionBuilder(Factory);
			receiveCollectionBuilder.AddLine(whs.PK, client.PK, "R", ReceiveType.Codes.Receipt, part.PK, 1m, "", "", "", "", ZDate.Empty, ZDate.Empty, "ATTR1", "ATTR2", "ATTR3", "SN1");
			var docket = receiveCollectionBuilder.Dockets.Single();

			AssertEquals(DocketType.Codes.Receive, docket.Inventory[0].WI_InDocketLineType);
			AssertEquals(1m, docket.Inventory[0].WI_InDocketLineUnits);
			AssertEquals(1m, docket.Inventory[0].WI_ExpectedReceiptQuantity);
			AssertEquals(1m, docket.Lines[0].WE_TransactionQuantity);
			AssertEquals(ZDate.Empty, docket.Inventory[0].WI_ExpiryDate);
			AssertEquals(ZDate.Empty, docket.Inventory[0].WI_PackingDate);
			AssertEquals("ATTR1", docket.Inventory[0].WI_PartAttrib1);
			AssertEquals("ATTR2", docket.Inventory[0].WI_PartAttrib2);
			AssertEquals("ATTR3", docket.Inventory[0].WI_PartAttrib3);
			AssertEquals("SN1", docket.Inventory[0].WI_SerialNumber);
		}

		protected override WhsDocketCollectionBuilder<WhsReceive, LineData> GetNewDocketBuilder(BusinessObjectFactory factory)
		{
			return new WhsReceiveCollectionBuilder(factory);
		}

		protected override WhsReceive GetNewDocket()
		{
			return Factory.New<WhsReceive>();
		}

		#endregion
	}
}
