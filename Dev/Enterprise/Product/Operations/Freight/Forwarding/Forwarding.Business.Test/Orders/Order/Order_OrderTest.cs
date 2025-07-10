using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	sealed class Order_OrderTest : Freight.Business.Testing.BaseFreightTest
	{
		public void TestFullQuantityAndDeliveryTimeSetsCorrectStatus()
		{
			using (OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var order = Factory.New<Order>();
				var buyer = Factory.NewWithValidTestData<OrgHeader>();
				buyer.OH_Code = "f";
				order.BuyerPK = buyer.PK;
				var line1 = order.OrderLines.AddNew();
				var line2 = order.OrderLines.AddNew();

				order.JD_OrderStatus = Constants.OrderStatus.Incomplete;
				Factory.Save();
				AssertEquals("incorrect Order status", Constants.OrderStatus.Incomplete, order.JD_OrderStatus);
				line1.JO_Quantity = 5;
				Factory.Save();
				AssertEquals("incorrect Order status", Constants.OrderStatus.Incomplete, order.JD_OrderStatus);
				line1.JO_QtyInvoiced = 1;
				Factory.Save();
				AssertEquals("incorrect Order status", Constants.OrderStatus.Incomplete, order.JD_OrderStatus);

				order.JD_OrderStatus = Constants.OrderStatus.Incomplete;
				line1.JO_QtyInvoiced = 0;
				Factory.Save();
				AssertEquals("incorrect Order status", Constants.OrderStatus.Incomplete, order.JD_OrderStatus);

				line2.JO_Quantity = 2;
				line2.JO_QtyReceived = 1;
				Factory.Save();
				AssertEquals("incorrect Order status", Constants.OrderStatus.Incomplete, order.JD_OrderStatus);

				line1.JO_QtyReceived = 5;
				line2.JO_QtyReceived = 2;
				order.UpdateMilestoneActual(Events.DeliveryCartageCompleteFinalised, ZDateTimeOffset.Now);
				Factory.Save();
				AssertEquals("incorrect Order status", Constants.OrderStatus.Delivered, order.JD_OrderStatus);
			}
		}

		public void TestCanBeUpdatedByImport()
		{
			Order order = Factory.New<Order>();

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();

			AssertEquals("order can be imported", true, order.CanBeUpdatedByImport);

			order.JD_JE = declaration.PK;
			order.JD_JS = ZGuid.Empty;
			AssertEquals("order can be imported", false, order.CanBeUpdatedByImport);

			order.JD_JE = ZGuid.Empty;
			order.JD_JS = shipment.PK;
			AssertEquals("order can be imported", false, order.CanBeUpdatedByImport);

			shipment.JS_IsForwardRegistered = false;
			AssertEquals("order can be imported", true, order.CanBeUpdatedByImport);

			order.JD_JE = ZGuid.Empty;
			order.JD_JS = ZGuid.Empty;
			AssertEquals("order can be imported", true, order.CanBeUpdatedByImport);
		}

		public void TestDetachingShipment_OrderDates()
		{
			Order order = Factory.New<Order>();
			order.EnsureProcessTasksAreCreated();
			CommonShipment shipment = CommonShipment.New(Factory);
			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();

			var now = ZDateTime.SmallDateTimeNow;
			var eventsThatProxyFromShipment = Order.GetEventsThatProxyFromShipment();
			foreach (var eventThatProxiesFromShipment in eventsThatProxyFromShipment)
			{
				ProcessTask milestone = order.WorkflowItems.Milestones[eventThatProxiesFromShipment.EventType];
				if (milestone != null)
				{
					if (eventThatProxiesFromShipment.SyncEstimate)
					{
						milestone.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(now));
					}

					milestone.SetMilestoneActualDateForTest(now);
				}
			}
			order.JD_JS = shipment.PK;
			order.JD_JS = ZGuid.Empty;
			foreach (var eventThatProxiesFromShipment in eventsThatProxyFromShipment)
			{
				ProcessTask milestone = order.WorkflowItems.Milestones[eventThatProxiesFromShipment.EventType];
				if (milestone != null)
				{
					if (eventThatProxiesFromShipment.SyncEstimate)
					{
						AssertEquals("Order dates must retain order date", now, milestone.P9_ScheduledDate.ToZDateTime());
					}

					AssertEquals("Order dates must retain order date", now, milestone.P9_ActualDate.ToZDateTime());
				}
			}

			order.JD_JE = declaration.PK;
			var eventsThatProxyFromDeclaration = order.GetEventsThatProxyFromDeclaration();
			foreach (var eventThatProxyFromDeclaration in eventsThatProxyFromDeclaration)
			{
				ProcessTask milestone = order.WorkflowItems.Milestones[eventThatProxyFromDeclaration.EventType];
				if (milestone != null)
				{
					if (eventThatProxyFromDeclaration.SyncEstimate)
					{
						milestone.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(now));
					}

					milestone.SetMilestoneActualDateForTest(now);
				}
			}

			order.JD_JS = ZGuid.Empty;
			order.JD_JE = ZGuid.Empty;
			foreach (var eventThatProxyFromDeclaration in eventsThatProxyFromDeclaration)
			{
				ProcessTask task = order.WorkflowItems.Milestones[eventThatProxyFromDeclaration.EventType];
				if (task != null)
				{
					if (eventThatProxyFromDeclaration.SyncEstimate)
					{
						AssertEquals("Order dates must retain order date", now, task.P9_ScheduledDate);
					}

					AssertEquals("Order dates must retain order date", now, task.P9_ActualDate);
				}
			}
		}

		#region TestIsAutoLogged

		public void TestIsAutoLogged()
		{
			var order = Factory.New<Order>() as IAutoLog;
			AssertEquals("IsAutoLogged", true, order.IsAutoAdminBusinessObjectLoggerEnabled);
		}

		#endregion TestIsAutoLogged
	}
}
