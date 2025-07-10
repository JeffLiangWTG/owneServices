using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDocketContainer))]
	internal class WhsDocketContainerTest : WhsBusinessObjectTestCase
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

			WhsDocketContainer con1 = receive.Containers.AddNew();
			WhsDocketContainer con2 = order.Containers.AddNew();
			WhsDocketContainer con3 = transfer.Containers.AddNew();
			WhsDocketContainer con4 = adjustment.Containers.AddNew();

			Assert("Con1 belongs to a Receive", con1.Docket is WhsReceive);
			Assert("Con2 belongs to an Order", con2.Docket is WhsOrder);
			Assert("Con3 belongs to a Transfer", con3.Docket is WhsTransfer);
			Assert("Con4 belongs to an Adjustment", con4.Docket is WhsAdjustment);
		}

		#endregion

		#region TestICartageContainer

		public void TestICartageContainer()
		{
			WhsDocketContainer container = (WhsDocketContainer)GetNewBusinessObject();
			ICartageContainer cartageContainer = container;

			container.WC_WD = Factory.New<WhsOrder>().PK;
			container.Docket.WD_WeightSentUserEntered = 11m;
			container.WC_ContainerNum = "123";
			container.WC_SealNum = "SEAL";

			AssertEquals("", cartageContainer.ContainerMode);
			AssertEquals("123", cartageContainer.ContainerNumber);
			AssertEquals(container.WC_RC, cartageContainer.ContainerRC);
			AssertEquals(ZGuid.Empty, cartageContainer.JobContainerPK);
			AssertEquals(0, cartageContainer.LooseCargo.Count);

			AssertEquals(11m, cartageContainer.NetWeight);
			AssertEquals("SEAL", cartageContainer.Seal);
		}

		#endregion
	}
}
