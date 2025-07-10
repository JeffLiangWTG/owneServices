using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingOrderLine))]
	sealed class TrackingOrderLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestContainerQuantity()
		{
			TrackingOrderLine line1 = (TrackingOrderLine)this.GetNewBusinessObject();
			AssertEquals("ContainerQuantity - precondition", ZDecimal.Zero, line1.ContainerQuantity);

			line1.SetContainerNumber("123456");
			line1.JO_ContainerNumber = "2345";
			line1.JO_QtyInvoiced = 100;
			AssertEquals("ContainerQuantity should be Zero because Container Numbers do not match", ZDecimal.Zero, line1.ContainerQuantity);

			line1.JO_ContainerNumber = "123456";
			OrderLineDelivery delivery0 = line1.Deliveries.AddNew();
			OrderLineDeliverContainer container0 = delivery0.Containers.AddNew();
			container0.J5_ContainerNum = "123456";
			container0.J5_QuantityInvoiced = 30;

			AssertEquals("ContainerQuantity == JO_QtyInvoiced", line1.JO_QtyInvoiced, line1.ContainerQuantity);

			TrackingOrderLine line2 = (TrackingOrderLine)this.GetNewBusinessObject();
			AssertEquals("ContainerQuantity - precondition", ZDecimal.Zero, line2.ContainerQuantity);

			line2.SetContainerNumber("112233");

			OrderLineDelivery delivery1 = line2.Deliveries.AddNew();
			OrderLineDeliverContainer container1 = delivery1.Containers.AddNew();
			container1.J5_ContainerNum = "123";
			container1.J5_QuantityInvoiced = 20;
			OrderLineDeliverContainer container2 = delivery1.Containers.AddNew();
			container2.J5_ContainerNum = "112233";
			container2.J5_QuantityInvoiced = 25;

			OrderLineDelivery delivery2 = line2.Deliveries.AddNew();
			OrderLineDeliverContainer container3 = delivery2.Containers.AddNew();
			container3.J5_ContainerNum = "112233";
			container3.J5_QuantityInvoiced = 50;

			AssertEquals("ContainerQuantity", container2.J5_QuantityInvoiced + container3.J5_QuantityInvoiced, line2.ContainerQuantity);
		}

		public new void TestCallsBaseSetDefaultValues()
		{
			Assert("This is for display purposes only - SetDefaultValues will never be called", true);
		}

		public new void TestLightValidation()
		{
			Assert("This is for display purposes only - it is never validated", true);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			TrackingOrder order = Factory.NewWithValidTestData<TrackingOrder>();
			OrderLine line = order.OrderLines.AddNew();
			Factory.Save();

			return Factory.Load<TrackingOrderLine>(line.PK);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var buyer = factory.NewWithValidTestData<OrgHeader>();
			var order = factory.New<TrackingOrder>();

			order.JD_OrderNumber = "x";
			order.BuyerPK = buyer.PK;
			order.SupplierPK = buyer.PK;

			var line = factory.New<TrackingOrderLine>();
			line.JO_JD = order.PK;
			line.JO_LineNo = 99;

			return line;
		}

		protected override bool IsDeleteSupported()
		{
			return false;
		}

		protected override bool CanPersistedObjectBeDeleted
		{
			get { return false; }
		}

		#endregion
	}
}
