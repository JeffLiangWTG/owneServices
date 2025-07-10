using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDocketPallet))]
	internal class WhsDocketPalletTest : WhsBusinessObjectTestCase
	{
		#region Business Object Overrides

		public void TestClone()
		{
			AssertEquals(true, GetNewBusinessObject().SupportsClone());
		}

		#endregion

		#region Related Business Objects

		public void TestDocket()
		{
			WhsReceive receive = Factory.New<WhsReceive>();
			WhsOrder order = Factory.New<WhsOrder>();
			WhsTransfer transfer = Factory.New<WhsTransfer>();
			WhsAdjustment adjustment = Factory.New<WhsAdjustment>();

			WhsDocketPallet pallet1 = receive.Pallets.AddNew();
			WhsDocketPallet pallet2 = order.Pallets.AddNew();
			WhsDocketPallet pallet3 = transfer.Pallets.AddNew();
			WhsDocketPallet pallet4 = adjustment.Pallets.AddNew();

			AssertEquals(receive, pallet1.Docket);
			AssertEquals(order, pallet2.Docket);
			AssertEquals(transfer, pallet3.Docket);
			AssertEquals(adjustment, pallet4.Docket);

			Assert("Pallet1 belongs to an Receive", pallet1.Docket is WhsReceive);
			Assert("Pallet2 belongs to an Order", pallet2.Docket is WhsOrder);
			Assert("Pallet3 belongs to an Transfer", pallet3.Docket is WhsTransfer);
			Assert("Pallet4 belongs to an Adjustment", pallet4.Docket is WhsAdjustment);
		}

		public void TestSetDefaultValues()
		{
			WhsDocketPallet pallet = Factory.New<WhsDocketPallet>();
			AssertEquals(CodeLists.PalletType.Codes.Chep, pallet.W2_PalletType);
		}

		#endregion
	}
}
