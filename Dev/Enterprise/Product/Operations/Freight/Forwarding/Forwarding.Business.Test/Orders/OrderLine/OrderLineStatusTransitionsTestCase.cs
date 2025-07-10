using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class OrderLineStatusTransitionsTestCase : TestCaseWithFactory
	{
		public void TestStatus_Delivered_NoPreAdvice()
		{
			fOrderLine.JO_Quantity = 2;
			fOrderLine.JO_QtyReceived = 1;
			Factory.Save();
			AssertEquals("Status should not be set to delivered yet", Constants.OrderStatus.Incomplete, fOrderLine.JO_LineStatus);

			fOrderLine.JO_QtyReceived = 2;
			Factory.Save();
			AssertEquals("Status should still not be delivered as no containers", Constants.OrderStatus.Incomplete, fOrderLine.JO_LineStatus);

			OrderLineDeliverContainer container = fDelivery.Containers.AddNew();
			container.J5_InstoreDate = ZDateTime.Now;
			Factory.Save();
			AssertEquals("Status should still not be delivered as no containers", Constants.OrderStatus.Delivered, fOrderLine.JO_LineStatus);
		}

		public void TestStatus_Delivered_PreAdvice()
		{
			JobShipmentPreplanning preAdvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			preAdvice.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			fOrderLine.Order.JD_EF_ShipmentPrePlanning = preAdvice.PK;

			fOrderLine.JO_Quantity = 2;
			fOrderLine.JO_QtyReceived = 1;
			Factory.Save();
			AssertEquals("Status should update to Part Delivered", Constants.OrderStatus.PartDelivered, fOrderLine.JO_LineStatus);

			fOrderLine.JO_QtyReceived = 2;
			Factory.Save();
			AssertEquals("Status should update to Delivered", Constants.OrderStatus.Delivered, fOrderLine.JO_LineStatus);

			fOrderLine.JO_QtyReceived = 1;
			Factory.Save();
			AssertEquals("Status should update back to Part Delivered", Constants.OrderStatus.PartDelivered, fOrderLine.JO_LineStatus);
		}

		public void TestStatus_NotDeliveredWhenQtyOrderedZero()
		{
			fOrderLine.JO_Quantity = 0m;
			fOrderLine.JO_QtyReceived = 0m;
			Factory.Save();
			AssertEquals("No quantity ordered should be an incomplete order", Constants.OrderStatus.Incomplete, fOrderLine.JO_LineStatus);
		}

		public void TestStatus_PartDelivered()
		{
			OrderLineDeliverContainer container = fDelivery.Containers.AddNew();
			fOrderLine.JO_Quantity = 2;
			fOrderLine.JO_QtyReceived = 1;
			Factory.Save();
			AssertEquals("Status should not be set to part delivered yet as no container has been delivered", Constants.OrderStatus.Incomplete, fOrderLine.JO_LineStatus);

			container.J5_InstoreDate = ZDateTime.Now;
			Factory.Save();
			AssertEquals("Status should indicate part delivered", Constants.OrderStatus.PartDelivered, fOrderLine.JO_LineStatus);
		}

		public void TestStatus_PartDeliveredQuantitySetToZero()
		{
			JobShipmentPreplanning preAdvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			preAdvice.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			fOrderLine.Order.JD_EF_ShipmentPrePlanning = preAdvice.PK;

			fOrderLine.JO_Quantity = 2;
			fOrderLine.JO_QtyReceived = 1;
			Factory.Save();
			AssertEquals("Status should update to Part Delivered", Constants.OrderStatus.PartDelivered, fOrderLine.JO_LineStatus);

			fOrderLine.JO_QtyReceived = 0;
			Factory.Save();
			AssertEquals("Status should update to 'Part Delivered but quantity amended to zero'", Constants.OrderStatus.PartDeliveredQuantityAmendedToZero, fOrderLine.JO_LineStatus);
		}

		public void TestStatus_DontChangeIfCancelled()
		{
			fOrderLine.JO_Quantity = 1;
			OrderLineDeliverContainer container = fDelivery.Containers.AddNew();
			fOrderLine.JO_LineStatus = Constants.OrderStatus.Cancelled;

			Factory.Save();
			AssertEquals("Status should still indicate cancelled", Constants.OrderStatus.Cancelled, fOrderLine.JO_LineStatus);
		}

		public void TestStatus_DontChangeIfCustomStatusSet()
		{
			fOrderLine.JO_Quantity = 1;
			OrderLineDeliverContainer container = fDelivery.Containers.AddNew();
			CodeDescriptionPairList customStatusList = new CodeDescriptionPairList();
			customStatusList.AddPair("XXX", "custom");
			fOrderLine.Order.Buyer.MiscServ.OrderLineStatusList = customStatusList.ToXMLByteArray();
			fOrderLine.JO_LineStatus = "XXX";

			Factory.Save();
			AssertEquals("Status should still indicate custom status", "XXX", fOrderLine.JO_LineStatus);
		}

		public void TestStatus_DontChangeIfRegistryStatusSet()
		{
			JobShipmentPreplanning preAdvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			preAdvice.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			fOrderLine.Order.JD_EF_ShipmentPrePlanning = preAdvice.PK;

			fOrderLine.JO_Quantity = 1;
			OrderLineDeliverContainer container = fDelivery.Containers.AddNew();
			CodeDescriptionPairList testOrderCodeDescPairList = new CodeDescriptionPairList();
			testOrderCodeDescPairList.AddPair("RGL", "Reg Status Description");
			Env.Registry.OrderLineStatusList = testOrderCodeDescPairList;
			fOrderLine.JO_LineStatus = "RGL";

			Factory.Save();
			AssertEquals("Status should still indicate registry status", "RGL", fOrderLine.JO_LineStatus);
		}

		#region IWorkflowTriggerFieldChangeSource

		public void TestParentWorkflowProviders()
		{
			IWorkflowTriggerFieldChangeSource fieldChangeSource = fOrderLine;
			AssertEquals(1, fieldChangeSource.ParentWorkflowProviders.Count);
			AssertEquals("Order is a related workflow provider", fOrderLine.Order, fieldChangeSource.ParentWorkflowProviders[0]);

			JobShipmentPreplanning preadvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			fOrderLine.Order.JD_EF_ShipmentPrePlanning = preadvice.PK;
			AssertEquals("Preadvice is a related workflow provider", 2, fieldChangeSource.ParentWorkflowProviders.Count);
			AssertEquals("Preadvice is a related workflow provider", fOrderLine.Order.PreAdvice, fieldChangeSource.ParentWorkflowProviders[1]);
		}

		#endregion

		#region Implementation

		Order fOrder;
		OrderLine fOrderLine;
		OrderLineDelivery fDelivery;

		protected override void SetUp()
		{
			base.SetUp();
			AdvOrmFeatureHelper.RunTestWith(isEnabled: true, () =>
			{
				fOrder = Factory.New<Order>();
				fOrder.JD_OrderNumber = "123";
				fOrder.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
				fOrder.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
				fOrderLine = fOrder.OrderLines.AddNew();
				fOrderLine.JO_LineNo = 1;
				fDelivery = fOrderLine.Deliveries.AddNew();
			});
		}

		#endregion
	}
}
