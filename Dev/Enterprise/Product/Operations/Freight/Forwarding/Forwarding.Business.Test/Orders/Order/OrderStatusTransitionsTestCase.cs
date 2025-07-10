using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class OrderStatusTransitionsTestCase : TestCaseWithFactory
	{
		public void TestFetchStrategy()
		{
			Order newOrder = Order.New(Factory);
			AssertEquals("Order Fetch Strategy", typeof(OrderFetchStrategy), newOrder.FetchStrategy.GetType());
		}

		public void TestStatus_Confirmed()
		{
			OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Precondition - AutomaticallySetOrderStatus should be false", false, OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.Value);

			Factory.Save();
			AssertEquals("Status should not be set to confirmed yet as no confirm. #", Constants.OrderStatus.Incomplete, fOrder.JD_OrderStatus);

			fOrder.JD_BookingConfRef = "xxx";
			Factory.Save();

			AssertEquals("Status should not change unless AutomaticallySetOrderStatus is true", Constants.OrderStatus.Incomplete, fOrder.JD_OrderStatus);

			OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			fOrder.JD_CustomAttrib1 = "splaty"; // need to poke the object's HasChanges for this to work
			fOrder.OrderLines.DeleteAll();
			Factory.Save();

			AssertEquals("Status should indicate confirmed", Constants.OrderStatus.Confirmed, fOrder.JD_OrderStatus);
		}

		public void TestStatus_Shipped()
		{
			OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Precondition - AutomaticallySetOrderStatus should be false", false, OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.Value);

			Factory.Save();
			AssertEquals("Status should not be set to shipped yet as no shipment attached", Constants.OrderStatus.Incomplete, fOrder.JD_OrderStatus);

			var ship = Factory.NewWithValidTestData<ForwardingShipment>();
			fOrder.JD_JS = ship.PK;
			Factory.Save();

			AssertEquals("Status should not change unless AutomaticallySetOrderStatus is true", Constants.OrderStatus.Incomplete, fOrder.JD_OrderStatus);

			OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			fOrder.HasChanges = true;
			Factory.Save();

			AssertEquals("Status should indicate shipped for shipment", Constants.OrderStatus.Shipped, fOrder.JD_OrderStatus);

			ship.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Now;
			fOrder.HasChanges = true;
			Factory.Save();

			AssertEquals("Status should indicate delivered once shipment has goods delivered date", Constants.OrderStatus.Delivered, fOrder.JD_OrderStatus);

			fOrder.JD_OrderStatus = "";
			fOrder.JD_JS = ZGuid.Empty;
			BusinessObject declaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			fOrder.JD_JE = declaration.PK;
			Factory.Save();
			AssertEquals("Status should indicate shipped for declaration", Constants.OrderStatus.Shipped, fOrder.JD_OrderStatus);

			JobDocsAndCartage decDocs = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent((IShipmentWithDocsAndCartage)declaration);
			decDocs.JP_DeliveryCartageCompleted = ZDateTime.Now;
			fOrder.HasChanges = true;
			Factory.Save();
			AssertEquals("Status should indicate Delivered once declaration has goods delivered date", Constants.OrderStatus.Delivered, fOrder.JD_OrderStatus);
		}

		public void TestStatus_Delivered_CartageCompanyInternal()
		{
			OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var ship = Factory.NewWithValidTestData<ForwardingShipment>();
			ship.DocsAndCartage.DeliveryCartageCoPK = GlbBranch.CurrentBranch.OrgProxy.PK;
			fOrder.JD_JS = ship.PK;
			Factory.Save();

			AssertEquals("Status should indicate shipped for shipment", Constants.OrderStatus.Shipped, fOrder.JD_OrderStatus);

			ship.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Now;
			fOrder.HasChanges = true;
			Factory.Save();

			AssertEquals("Status should indicate delivered once shipment has goods delivered date", Constants.OrderStatus.Delivered, fOrder.JD_OrderStatus);
		}

		public void TestStatus_Delivered_CartageCompanyExternal()
		{
			OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			OrgHeader externalOrg = Factory.NewWithValidTestData<OrgHeader>();

			var ship = Factory.NewWithValidTestData<ForwardingShipment>();
			fOrder.JD_JS = ship.PK;
			Factory.Save();
			AssertEquals("Status should indicate shipped for shipment", Constants.OrderStatus.Shipped, fOrder.JD_OrderStatus);

			ship.DocsAndCartage.DeliveryCartageCoPK = externalOrg.PK;
			fOrder.HasChanges = true;
			Factory.Save();
			AssertEquals("Status should remain as shipped as registry not on for external cartage", Constants.OrderStatus.Shipped, fOrder.JD_OrderStatus);

			OrdersDataRegistry.Instance.AutoSetDeliveredForOrdersForExternalCartage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			fOrder.HasChanges = true;
			Factory.Save();
			AssertEquals("Status should indicate delivered as the shipment's cartage is being done externally and registry on", Constants.OrderStatus.Delivered, fOrder.JD_OrderStatus);
		}

		public void TestAttemptToUpdateOrderStatus()
		{
			//Order Status will be updated to delivered when DocsAndCartage.JP_DeliveryCartageCompleted is set
			//no matter if Order.HasChanges is false
			OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Precondition - AutomaticallySetOrderStatus should be true after setting", true, OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.Value);

			fOrderLine.JO_QtyInvoiced = 0;
			fOrderLine.JO_QtyReceived = 0;

			Factory.Save();
			AssertEquals("Status should not be set to shipped yet as no shipment attached", Constants.OrderStatus.Incomplete, fOrder.JD_OrderStatus);

			var ship = Factory.NewWithValidTestData<ForwardingShipment>();
			fOrder.JD_JS = ship.PK;
			Factory.Save();

			AssertEquals("Status should indicate shipped for shipment", Constants.OrderStatus.Shipped, fOrder.JD_OrderStatus);

			ship.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Now;
			Factory.Save();

			AssertEquals("Status should indicate delivered once shipment has goods delivered date", Constants.OrderStatus.Delivered, fOrder.JD_OrderStatus);

			fOrder.JD_JS = ZGuid.Empty;
			AssertEquals("Status was reset to Incomplete after shipment was detached", Constants.OrderStatus.Incomplete, fOrder.JD_OrderStatus);

			BusinessObject declaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			fOrder.JD_JE = declaration.PK;
			Factory.Save();
			AssertEquals("Status changed to Shipped on saving", Constants.OrderStatus.Shipped, fOrder.JD_OrderStatus);

			fOrder.JD_OrderStatus = "";
			Factory.Save();
			AssertEquals("Status should indicate shipped for declaration", Constants.OrderStatus.Shipped, fOrder.JD_OrderStatus);

			JobDocsAndCartage decDocs = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent((IShipmentWithDocsAndCartage)declaration);
			decDocs.JP_DeliveryCartageCompleted = ZDateTime.Now;
			Factory.Save();
			AssertEquals("Status should indicate Delivered once declaration has goods delivered date", Constants.OrderStatus.Delivered, fOrder.JD_OrderStatus);
		}

		public void TestAttemptToUpdateOrderStatus_ShouldUpdateStatus_EvenIfLinkedToSupplierBooking()
		{
			using (OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var order = Factory.NewWithValidTestData<Order>();
				order.JD_BookingConfRef = "Test";
				order.JD_OrderStatus = Constants.OrderStatus.Incomplete;
				var orderLine = Factory.NewWithValidTestData<OrderLine>();
				order.OrderLines.Add(orderLine);

				var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
				var bookingLine = supplierBooking.SupplierBookingLines.AddNew();
				bookingLine.JSL_JO_OrderLine = orderLine.PK;

				Factory.Save();

				order.JD_OrderStatus = Constants.OrderStatus.Incomplete;
				Factory.Save();

				order.AttemptToUpdateOrderStatus();

				AssertEquals("should update order status even if linked to supplier bookings", Constants.OrderStatus.Confirmed, order.JD_OrderStatus);
			}
		}

		public void TestStatus_Delivered()
		{
			OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Precondition - AutomaticallySetOrderStatus should be false", false, OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.Value);

			fOrder.JD_OrderStatus = Constants.OrderStatus.Incomplete;
			fOrderLine.JO_Quantity = 2;
			fOrderLine.JO_QtyReceived = 1;
			Factory.Save();
			AssertEquals("Status should not be set to delivered yet", Constants.OrderStatus.Incomplete, fOrder.JD_OrderStatus);

			fOrder.JD_OrderStatus = Constants.OrderStatus.Incomplete;
			fOrderLine.JO_Quantity = 2;
			fOrderLine.JO_QtyReceived = 2;

			Factory.Save();

			AssertEquals("Status should not change unless AutomaticallySetOrderStatus is true", Constants.OrderStatus.Incomplete, fOrder.JD_OrderStatus);

			OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			fOrder.JD_CustomAttrib1 = "splaty"; // need to poke the object's HasChanges for this to work
			fOrder.UpdateEvent(Events.DeliveryCartageCompleteFinalised, ZDateTimeOffset.Now);
			Factory.Save();
			AssertEquals("Status should be delivered as no containers", Constants.OrderStatus.Delivered, fOrder.JD_OrderStatus);

			fOrder.JD_OrderStatus = Constants.OrderStatus.Incomplete;
			OrderLineDeliverContainer container = fDelivery.Containers.AddNew();
			fOrderLine.JO_Quantity = 0;
			fOrderLine.JO_QtyReceived = 0;
			Factory.Save();
			AssertEquals("Status should not be delivered as there are containers that aren't delivered", Constants.OrderStatus.Incomplete, fOrder.JD_OrderStatus);

			fOrder.JD_OrderStatus = Constants.OrderStatus.Incomplete;
			container.J5_InstoreDate = ZDateTime.Now;
			Factory.Save();
			AssertEquals("Status should now be delivered as the container has been delivered", Constants.OrderStatus.Delivered, fOrder.JD_OrderStatus);

			fOrder.JD_OrderStatus = Constants.OrderStatus.Incomplete;
			container.Delete();
			fOrder.UpdateEvent(Events.DeliveryCartageCompleteFinalised, ZDateTimeOffset.Now);
			Factory.Save();
			AssertEquals("Status should now be delivered as the order has been delivered", Constants.OrderStatus.Delivered, fOrder.JD_OrderStatus);

			fOrder.JD_OrderStatus = Constants.OrderStatus.Incomplete;
			fOrder.WorkflowItems.Milestones.Cast<ProcessTask>().First(f => f.TriggerConditions.TriggerEventCode == Events.DeliveryCartageCompleteFinalisedCode).P9_ActualDateForBinding = ZDateTimeOffset.Empty;
			fOrderLine.Delete();
			Factory.Save();
			AssertEquals("Status should not be delivered as no order lines", Constants.OrderStatus.Incomplete, fOrder.JD_OrderStatus);
		}

		public void TestStatus_NotDeliveredWhenQtyOrderedZero()
		{
			OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Precondition - AutomaticallySetOrderStatus should be false", false, OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.Value);

			fOrderLine.JO_Quantity = 0m;
			fOrderLine.JO_QtyReceived = 0m;
			Factory.Save();
			AssertEquals("No quantity ordered should be an incomplete order", Constants.OrderStatus.Incomplete, fOrder.JD_OrderStatus);
		}

		public void TestStatus_Delivered_WithSplits()
		{
			bool valueBefore = OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.Value;
			try
			{
				OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertEquals("Precondition - AutomaticallySetOrderStatus should be false", false, OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.Value);

				OrderLineDeliverContainer container = fDelivery.Containers.AddNew();
				container.J5_InstoreDate = ZDateTime.Now;
				fOrderLine.JO_Quantity = 10;
				fOrderLine.JO_QtyReceived = 3;
				Factory.Save();
				AssertEquals("Status should not change unless AutomaticallySetOrderStatus is true", Constants.OrderStatus.Incomplete, fOrder.JD_OrderStatus);

				OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				fOrder.JD_CustomAttrib1 = "splaty"; // need to poke the object's HasChanges for this to work
				Factory.Save();
				AssertEquals("Status is only part delivered", Constants.OrderStatus.PartDelivered, fOrder.JD_OrderStatus);

				Order splitOrder = fOrder.SplitOrder(CreateOrderType.Split);
				Factory.Save();
				AssertEquals("SplitOrder should have 7 items", 7, splitOrder.OrderLines[0].JO_Quantity.ToZInt());
				AssertEquals("SplitOrder should have 0 items received", 0, splitOrder.OrderLines[0].JO_QtyReceived.ToZInt());
				AssertEquals("Main Order - Status should still be only part delivered - as the whole order (including split) is not completely delivered", Constants.OrderStatus.PartDelivered, fOrder.JD_OrderStatus);

				splitOrder.OrderLines[0].JO_QtyReceived = 7;
				Factory.Save();
				fOrder.JD_CustomAttrib1 = "splaty2"; // need to poke the object's HasChanges for this to work
				fOrder.UpdateEvent(Events.DeliveryCartageCompleteFinalised, ZDateTimeOffset.Now);
				splitOrder.UpdateEvent(Events.DeliveryCartageCompleteFinalised, ZDateTimeOffset.Now);
				Factory.Save();
				AssertEquals("Main Order - Status should now be delivered", Constants.OrderStatus.Delivered, fOrder.JD_OrderStatus);
				AssertEquals("SplitOrder - Status should now be delivered", Constants.OrderStatus.Delivered, splitOrder.JD_OrderStatus);
			}
			finally
			{
				OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, valueBefore);
			}
		}

		public void TestStatus_PartDelivered()
		{
			OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Precondition - AutomaticallySetOrderStatus should be false", false, OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.Value);

			OrderLineDeliverContainer container = fDelivery.Containers.AddNew();
			fOrderLine.JO_Quantity = 2;
			fOrderLine.JO_QtyReceived = 1;
			Factory.Save();
			AssertEquals("Status should not be set to part delivered yet as no container has been delivered", Constants.OrderStatus.Incomplete, fOrder.JD_OrderStatus);

			container.J5_InstoreDate = ZDateTime.Now;
			Factory.Save();
			AssertEquals("Status should not change unless AutomaticallySetOrderStatus is true", Constants.OrderStatus.Incomplete, fOrder.JD_OrderStatus);

			OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			fOrder.JD_CustomAttrib1 = "splaty"; // need to poke the object's HasChanges for this to work
			Factory.Save();
			AssertEquals("Status should indicate part delivered", Constants.OrderStatus.PartDelivered, fOrder.JD_OrderStatus);
		}

		public void TestStatus_DontChangeIfCancelled()
		{
			OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Precondition - AutomaticallySetOrderStatus should be true", true, OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.Value);

			fOrderLine.JO_Quantity = 1;
			OrderLineDeliverContainer container = fDelivery.Containers.AddNew();
			fOrder.JD_OrderStatus = Constants.OrderStatus.Cancelled;

			Factory.Save();
			AssertEquals("Status should still indicate cancelled", Constants.OrderStatus.Cancelled, fOrder.JD_OrderStatus);
		}

		public void TestStatus_DontChangeIfCustomStatusSet()
		{
			OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Precondition - AutomaticallySetOrderStatus should be true", true, OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.Value);

			OrderLineDeliverContainer container = fDelivery.Containers.AddNew();
			CodeDescriptionPairList customStatusList = new CodeDescriptionPairList();
			customStatusList.AddPair("XXX", "custom");
			fOrder.Buyer.MiscServ.OrderStatusList = customStatusList.ToXMLByteArray();
			fOrder.JD_OrderStatus = "XXX";

			Factory.Save();
			AssertEquals("Status should still indicate custom status", "XXX", fOrder.JD_OrderStatus);
		}

		public void TestSetDefaultValues()
		{
			Env.Registry.FreightVolumeUnit = Enterprise.Core.Constants.Volume.CubicYards;
			Env.Registry.FreightWeightUnit = Enterprise.Core.Constants.Weight.Ounces;

			Order newOrder = Factory.New<Order>();
			AssertEquals("Volume Unit", Enterprise.Core.Constants.Volume.CubicYards, newOrder.JD_UnitOfVolume);
			AssertEquals("Weight Unit", Enterprise.Core.Constants.Weight.Ounces, newOrder.JD_UnitOfWeight);
			AssertEquals("Package Type", Enterprise.Core.Constants.PkgUnit.Pallet, newOrder.JD_F3_NKPackType);
		}

		#region Implementation

		Order fOrder;
		OrderLine fOrderLine;
		OrderLineDelivery fDelivery;

		protected override void SetUp()
		{
			base.SetUp();
			fOrder = Factory.New<Order>();
			fOrder.JD_OrderNumber = "123";
			fOrder.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			fOrder.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			fOrderLine = fOrder.OrderLines.AddNew();
			fOrderLine.JO_LineNo = 1;
			fOrderLine.JO_Quantity = 2;
			fOrderLine.JO_QtyReceived = 1;
			fDelivery = fOrderLine.Deliveries.AddNew();
		}

		#endregion

		public void TestOrderPacksAndWeightAndVolume()
		{
			CommonShipment newShipment = CommonShipment.New(Factory);
			newShipment.JS_ActualVolume = 10.23M;
			newShipment.JS_UnitOfVolume = Constants.Volume.Litre;
			newShipment.JS_ActualWeight = 52.36M;
			newShipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			newShipment.JS_OuterPacks = 12;
			newShipment.JS_F3_NKPackType = Constants.PkgUnit.Bottle;

			Order orderWithShipmentAttached = Factory.New<Order>();
			orderWithShipmentAttached.JD_JS = newShipment.PK;

			AssertEquals("10.23", orderWithShipmentAttached.Volume.ToString());
			AssertEquals(Constants.Volume.Litre, orderWithShipmentAttached.VolumeUnit.ToString());
			AssertEquals("52.36", orderWithShipmentAttached.Weight.ToString());
			AssertEquals(Constants.Weight.Kilograms, orderWithShipmentAttached.WeightUnit.ToString());
			AssertEquals("12", orderWithShipmentAttached.Packs.ToString());
			AssertEquals(Constants.PkgUnit.Bottle, orderWithShipmentAttached.JD_Calc_PackType.ToString());

			BusinessObject newDeclaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			newDeclaration[JobDeclarationSchema.Constants.JE_TotalVolume] = 22.23M;
			newDeclaration[JobDeclarationSchema.Constants.JE_TotalVolumeUnit] = Constants.Volume.CubicYards;
			newDeclaration[JobDeclarationSchema.Constants.JE_TotalWeight] = 50.36M;
			newDeclaration[JobDeclarationSchema.Constants.JE_TotalWeightUnit] = Constants.Weight.Kilograms;
			newDeclaration[JobDeclarationSchema.Constants.JE_TotalNoOfPacks] = 13;
			newDeclaration[JobDeclarationSchema.Constants.JE_TotalNoOfPacksPackType] = Constants.PkgUnit.Basket;

			Order orderWithDeclarationAttached = Factory.New<Order>();
			orderWithDeclarationAttached.JD_JE = newDeclaration.PK;

			AssertEquals("22.23", orderWithDeclarationAttached.Volume.ToString());
			AssertEquals(Constants.Volume.CubicYards, orderWithDeclarationAttached.VolumeUnit.ToString());
			AssertEquals("50.36", orderWithDeclarationAttached.Weight.ToString());
			AssertEquals(Constants.Weight.Kilograms, orderWithDeclarationAttached.WeightUnit.ToString());
			AssertEquals("13", orderWithDeclarationAttached.Packs.ToString());
			AssertEquals(Constants.PkgUnit.Basket, orderWithDeclarationAttached.JD_Calc_PackType.ToString());

			Order orderWithoutAttachment = Factory.New<Order>();
			orderWithoutAttachment.JD_ActualVolume = 32.33M;
			orderWithoutAttachment.JD_UnitOfVolume = Constants.Volume.CubicInches;
			orderWithoutAttachment.JD_ActualWeight = 69.54M;
			orderWithoutAttachment.JD_UnitOfWeight = Constants.Weight.Tonnes;
			orderWithoutAttachment.JD_Packs = 17;
			orderWithoutAttachment.JD_F3_NKPackType = Constants.PkgUnit.Coil;
			orderWithoutAttachment.JD_ContainerMode = Constants.ContainerModes.LCL;

			AssertEquals("32.33", orderWithoutAttachment.Volume.ToString());
			AssertEquals(Constants.Volume.CubicInches, orderWithoutAttachment.VolumeUnit.ToString());
			AssertEquals("69.54", orderWithoutAttachment.Weight.ToString());
			AssertEquals(Constants.Weight.Tonnes, orderWithoutAttachment.WeightUnit.ToString());
			AssertEquals("17", orderWithoutAttachment.Packs.ToString());
			AssertEquals(Constants.PkgUnit.Coil, orderWithoutAttachment.JD_Calc_PackType.ToString());

			orderWithoutAttachment.JD_ContainerMode = Constants.ContainerModes.FCL;
			AssertEquals("32.33", orderWithoutAttachment.Volume.ToString());
			AssertEquals(Constants.Volume.CubicInches, orderWithoutAttachment.VolumeUnit.ToString());
			AssertEquals("69.54", orderWithoutAttachment.Weight.ToString());
			AssertEquals(Constants.Weight.Tonnes, orderWithoutAttachment.WeightUnit.ToString());
			AssertEquals("17", orderWithoutAttachment.Packs.ToString());
			AssertEquals(Constants.PkgUnit.Coil, orderWithoutAttachment.JD_Calc_PackType.ToString());
		}

		public void TestOrderWeightAndVolumePriority()
		{
			Order order = Factory.New<Order>();
			order.JD_ActualVolume = 32.33M;
			order.JD_ContainerMode = Constants.ContainerModes.LCL;
			AssertEquals("First it will look at Actual volume", "32.33", order.Volume.ToString());

			BusinessObject newDeclaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			newDeclaration[JobDeclarationSchema.Constants.JE_TotalVolume] = 22.23M;
			order.JD_JE = newDeclaration.PK;
			AssertEquals("The priority of Declaration is more therefore it will look at Declaration", "22.23", order.Volume.ToString());

			CommonShipment newShipment = CommonShipment.New(Factory);
			newShipment.JS_ActualVolume = 10.23M;
			order.JD_JS = newShipment.PK;
			AssertEquals("The priority of Declaration is the most therefore it will look at Declaration", "10.23", order.Volume.ToString());
		}
	}
}
