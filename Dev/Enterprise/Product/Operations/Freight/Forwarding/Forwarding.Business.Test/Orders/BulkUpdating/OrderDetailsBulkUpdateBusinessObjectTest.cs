using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderDetailsBulkUpdateBusinessObject))]
	sealed class OrderDetailsBulkUpdateBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateFieldsThatMustNotBeUpdatedWhenShipmentAttached()
		{
			var bo = new OrderDetailsBulkUpdateBusinessObject(Factory);
			var orderWithShipment = Factory.New<Order>();
			orderWithShipment.JD_OrderNumber = "splaty";
			orderWithShipment.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			var orderToBulkUpdateWithShipment = bo.SelectedOrders.AddNew();
			orderToBulkUpdateWithShipment.SetOrder(orderWithShipment);

			foreach (ZPropertyInfo info in FindFieldsThatMustNotBeUpdatedWhenShipmentAttached(bo))
			{
				info.Value = ZDateTime.Empty;
				info.Value = ZDateTime.Now;
				AssertNoErrors("Shouldnt have errors without shipment attached", info);
			}

			orderWithShipment.JD_JS = CommonShipment.New(Factory).PK;

			foreach (ZPropertyInfo info in FindFieldsThatMustNotBeUpdatedWhenShipmentAttached(bo))
			{
				info.Value = ZDateTime.Empty;
				info.Value = ZDateTime.Now;
				AssertHasErrors("Should have errors when shipment attached", info);
			}
		}

		public void TestValidateOrdersEntered()
		{
			OrderDetailsBulkUpdateBusinessObject bO = new OrderDetailsBulkUpdateBusinessObject(Factory);
			OrderToBulkUpdate order = bO.SelectedOrders.AddNew();
			bO.ValidatePropertyForEnsuringDataEntered();
			bO.JD_Milestone_E_ARV = ZDateTime.Now;
			bO.ValidatePropertyForEnsuringDataEntered();
			AssertEquals("Should have no errors when order details to change entered", false, bO.PropertyForEnsuringDataEnteredInfo.HasErrors());

			bO.SelectedOrders.Remove(order);
			bO.ValidatePropertyForEnsuringDataEntered();
			AssertEquals("Should have errors when no orders selected", true, bO.PropertyForEnsuringDataEnteredInfo.HasErrors());
		}

		public void TestValidateDataToUpdateEntered()
		{
			OrderDetailsBulkUpdateBusinessObject bO = new OrderDetailsBulkUpdateBusinessObject(Factory);
			OrderToBulkUpdate order = bO.SelectedOrders.AddNew();
			bO.ValidatePropertyForEnsuringDataEntered();
			bO.JD_Milestone_E_ARV = ZDateTime.Now;
			bO.ValidatePropertyForEnsuringDataEntered();
			AssertEquals("Should have no errors when order entered", false, bO.PropertyForEnsuringDataEnteredInfo.HasErrors());

			bO.JD_Milestone_E_ARV = ZDateTime.Invalid;
			bO.ValidatePropertyForEnsuringDataEntered();
			AssertEquals("Should have errors when no data entered", true, bO.PropertyForEnsuringDataEnteredInfo.HasErrors());
		}

		public void TestSaveSucceeded()
		{
			bool valueBefore = OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.Value;
			try
			{
				OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				OrderDetailsBulkUpdateBusinessObject bO = new OrderDetailsBulkUpdateBusinessObject(Factory);
				bO.HasChanges = true;
				bO.SaveSucceeded += new EventHandler(OnSaveSucceeded);
				Factory.Save();
				AssertEquals("SaveSucceeded event should have fired", true, OnSaveSucceededCalled);
			}
			finally
			{
				OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, valueBefore);
			}
		}

		bool OnSaveSucceededCalled;
		void OnSaveSucceeded(object sender, EventArgs e)
		{
			OnSaveSucceededCalled = true;
		}

		ZPropertyInfo[] FindFieldsThatMustNotBeUpdatedWhenShipmentAttached(OrderDetailsBulkUpdateBusinessObject order)
		{
			return new ZPropertyInfo[]
			{
				order.JD_E_ARVInfo,
				order.JD_A_DEPInfo,
				order.JD_A_UNPInfo,
				order.JD_A_ISTInfo,
				order.JD_A_PUPInfo,
				order.JD_A_CCCInfo,
				order.JD_E_DEPInfo,
				order.JD_A_ARVInfo,
				order.JD_A_CLRInfo
			};
		}
	}
}
