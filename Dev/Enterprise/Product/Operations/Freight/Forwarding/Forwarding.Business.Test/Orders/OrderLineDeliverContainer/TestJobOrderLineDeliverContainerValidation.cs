using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class TestJobOrderLineDeliverContainerValidation : BusinessObjectValidationTestCase
	{
		[TestDate(2005, 01, 01)]
		public void TestValidateJ5_ETA()
		{
			OrderLineDeliverContainer container = fDelivery.Containers.AddNew();

			container.J5_ETD = new ZDateTime(2005, 1, 1);
			container.J5_ETA = new ZDateTime(2005, 1, 1);
			AssertNoErrors("No error when ETA is the same as ETD", container.J5_ETAInfo);

			container.J5_ETD = new ZDateTime(2005, 1, 1);
			container.J5_ETA = new ZDateTime(2005, 2, 2);
			AssertNoErrors("No error when ETA is after ETD", container.J5_ETAInfo);

			container.J5_ETD = new ZDateTime(2005, 2, 2);
			container.J5_ETA = new ZDateTime(2005, 1, 1);
			AssertHasErrors("The Arrival Date must occur after the Departure Date.", container.J5_ETAInfo);
		}
		#region Implementation

		OrderLineDelivery fDelivery;

		protected override void SetUp()
		{
			base.SetUp();
			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "ordernum";
			order.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			order.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineNo = 5;
			orderLine.JO_Partno = "1234";
			fDelivery = orderLine.Deliveries.AddNew();
		}

		#endregion
	}
}
