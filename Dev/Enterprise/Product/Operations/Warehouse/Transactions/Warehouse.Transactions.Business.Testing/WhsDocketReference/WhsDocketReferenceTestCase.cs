using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDocketReference))]
	internal class WhsDocketReferenceTestCase : WhsBusinessObjectTestCase
	{
		#region Business Object Overrides

		public void TestClone()
		{
			AssertEquals(true, GetNewBusinessObject().SupportsClone());
		}

		public void TestOnElementChanged()
		{
			WhsReceive docket = Factory.New<WhsReceive>();
			docket.References.AddNew();
			docket.References[0].WX_Reference = "aaa";
			Assert(docket.RefreshProxyPropertiesWasRun);
		}

		#endregion

		#region Related Business Objects

		public void TestDocket()
		{
			WhsReceive receive = Factory.New<WhsReceive>();
			WhsOrder order = Factory.New<WhsOrder>();
			WhsTransfer transfer = Factory.New<WhsTransfer>();
			WhsAdjustment adjustment = Factory.New<WhsAdjustment>();

			WhsDocketReference ref1 = receive.References.AddNew();
			WhsDocketReference ref2 = order.References.AddNew();
			WhsDocketReference ref3 = transfer.References.AddNew();
			WhsDocketReference ref4 = adjustment.References.AddNew();

			Assert("Ref1 belongs to an Receive", ref1.Docket is WhsReceive);
			Assert("Ref2 belongs to an Order", ref2.Docket is WhsOrder);
			Assert("Ref3 belongs to an Transfer", ref3.Docket is WhsTransfer);
			Assert("Ref4 belongs to an Adjustment", ref4.Docket is WhsAdjustment);
		}

		#endregion
	}
}
