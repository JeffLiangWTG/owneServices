using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderToBulkUpdate))]
	sealed class OrderToBulkUpdateTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOrder()
		{
			var orderToBulkUpdate = DummyParent.SelectedOrders.AddNew();
			AssertEquals("Order should be null initially", null, orderToBulkUpdate.Order);
			orderToBulkUpdate.ValidateOrderNumber();
			AssertEquals("Should have an error due to no order found", true, orderToBulkUpdate.OrderNumberInfo.HasErrors());

			var order = Factory.New<Order>();
			order.JD_OrderNumber = "splaty";
			AssertNull("Order should still be null", orderToBulkUpdate.Order);
			AssertEquals("Should have an error due to no order found", true, orderToBulkUpdate.OrderNumberInfo.HasErrors());

			orderToBulkUpdate.OrderNumber = "splaty";
			AssertNull("Order should still be null as no buyer", orderToBulkUpdate.Order);
			AssertEquals("Should have an error due to no order found", true, orderToBulkUpdate.OrderNumberInfo.HasErrors());

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			order.BuyerPK = buyer.PK;
			Factory.Save();

			orderToBulkUpdate.BuyerFK = buyer.PK;
			AssertEquals("Order should be populated now", "splaty", orderToBulkUpdate.Order.JD_OrderNumber);
			AssertEquals("Should not have an error due to order has been found", false, orderToBulkUpdate.OrderNumberInfo.HasErrors());

			orderToBulkUpdate.BuyerFK = ZGuid.Invalid;
			AssertEquals("Should have an error due to no order found", true, orderToBulkUpdate.BuyerFKInfo.HasErrors());

			orderToBulkUpdate.OrderNumber = "blah";
			AssertNull("Order should again be null as order number doesnt exist", orderToBulkUpdate.Order);
			AssertEquals("Should have an error due to no order found", true, orderToBulkUpdate.OrderNumberInfo.HasErrors());
		}

		public void TestBuyerFKPopulatedFromOrderNumberIf1Found()
		{
			var buyer1 = Factory.NewWithValidTestData<OrgHeader>();
			var order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_OrderNumber = "splaty";
			order1.JD_OrderNumberSplit = 1;
			order1.BuyerPK = buyer1.PK;

			var buyer2 = Factory.NewWithValidTestData<OrgHeader>();
			var order2 = Factory.NewWithValidTestData<Order>();
			order2.JD_OrderNumber = "splaty";
			order2.JD_OrderNumberSplit = 1;
			order2.BuyerPK = buyer2.PK;

			OrderToBulkUpdate orderToBulkUpdate = DummyParent.SelectedOrders.AddNew();
			orderToBulkUpdate.OrderNumber = "splaty";
			AssertEquals("Should not populate buyer as more than 1 match found", ZGuid.Empty, orderToBulkUpdate.BuyerFK);

			order2.Delete();
			orderToBulkUpdate.OrderNumber = "";
			orderToBulkUpdate.OrderNumber = "splaty";
			orderToBulkUpdate.OrderNumberSplit = 1;
			AssertEquals("Should populate buyer as 1 match found", buyer1.PK, orderToBulkUpdate.BuyerFK);
		}

		public void TestValidateOrderUnique()
		{
			OrderDetailsBulkUpdateBusinessObject bO = new OrderDetailsBulkUpdateBusinessObject(Factory);
			OrgHeader buyer = Factory.New<OrgHeader>();

			OrderToBulkUpdate order1 = bO.SelectedOrders.AddNew();
			order1.OrderNumber = "order";
			order1.OrderNumberSplit = 1;
			order1.BuyerFK = buyer.PK;

			OrderToBulkUpdate order2 = bO.SelectedOrders.AddNew();
			order2.OrderNumber = "order";
			AssertEquals("Shouldn't have an error yet as no duplicate", true, order2.OrderNumberInfo.HasErrors());
			order2.OrderNumberSplit = 1;
			order2.BuyerFK = buyer.PK;
			AssertEquals("Should have an error due to a duplicate", true, order2.OrderNumberInfo.HasErrors());
		}

		public void TestWarningOnSplitIfMoreThan1Split()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();

			var order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_OrderNumber = "order";
			order1.JD_OrderNumberSplit = 0;
			order1.BuyerPK = buyer.PK;

			var order2 = Factory.NewWithValidTestData<Order>();
			order2.JD_OrderNumber = "order";
			order2.JD_OrderNumberSplit = 1;
			order2.BuyerPK = buyer.PK;

			Factory.Save();

			var bo = DummyParent.SelectedOrders.AddNew();
			bo.OrderNumber = "order";
			bo.BuyerFK = buyer.PK;
			AssertEquals("Should have a warning due to the multiple order splits", true, bo.OrderNumberSplitInfo.HasWarnings());

			bo.OrderNumberSplit = 1;
			AssertEquals("Should no longer have a warning as we're specifying a split number", false, bo.OrderNumberSplitInfo.HasWarnings());
		}

		[TestDate(2013, 9, 15)]
		public void TestUpdateFrom()
		{
			OrderDetailsBulkUpdateBusinessObject bO = new OrderDetailsBulkUpdateBusinessObject(Factory);
			foreach (string propertyName in OrderToBulkUpdate.PropertiesToUpdate)
			{
				AssertEquals("All properties should be empty initially for the user", true, ((IZType)bO[propertyName]).IsEmpty);
			}

			foreach (string propertyName in OrderToBulkUpdate.PropertiesToUpdate)
			{
				TestUpdatingProperty(bO, propertyName);
			}

			TestUpdatingMilestones(bO);
			TestUpdatingNotes(bO);
			TestUpdatingAuditDetails(bO);
		}

		void TestUpdatingProperty(OrderDetailsBulkUpdateBusinessObject bO, string propertyName)
		{
			var orderToBulkUpdate = DummyParent.SelectedOrders.AddNew();
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "splaty";
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();

			orderToBulkUpdate.SetOrder(order);

			object emptyValue = null;
			object someValue = null;
			object someOtherValue = null;
			if (bO[propertyName] is ZDateTime)
			{
				emptyValue = ZDateTime.Invalid;
				someValue = ZDateTime.Now;
				someOtherValue = ZDateTime.Now.AddDays(1);
			}
			else if (bO[propertyName] is ZString)
			{
				emptyValue = ZString.Empty;
				someValue = "spl";
				someOtherValue = "sp2";
			}
			else
			{
				Fail("Don't know how to handle this type " + bO[propertyName].GetType().Name);
			}

			orderToBulkUpdate.Order[propertyName] = someValue;
			bO[propertyName] = emptyValue;
			orderToBulkUpdate.UpdateFrom(bO);
			AssertEquals("Should not change the value because it is empty", someValue, orderToBulkUpdate.Order[propertyName]);

			bO[propertyName] = someOtherValue;
			orderToBulkUpdate.UpdateFrom(bO);
			AssertEquals("Property " + propertyName + " should update as a value is specified", someOtherValue, orderToBulkUpdate.Order[propertyName]);

			bO[propertyName] = emptyValue;
		}

		void TestUpdatingMilestones(OrderDetailsBulkUpdateBusinessObject bO)
		{
			foreach (var propertyToMilestone in OrderToBulkUpdate.PropertiesToMilestones)
			{
				AssertEquals("All properties should be empty initially for the user", true, ((IZType)bO[propertyToMilestone.EstimatedProperty]).IsEmpty);
				AssertEquals("All properties should be empty initially for the user", true, ((IZType)bO[propertyToMilestone.ActualProperty]).IsEmpty);
			}

			foreach (var propertyToMilestone in OrderToBulkUpdate.PropertiesToMilestones)
			{
				TestUpdatingMilestone(bO, propertyToMilestone);
			}
		}

		void TestUpdatingMilestone(OrderDetailsBulkUpdateBusinessObject bO, OrderToBulkUpdate.PropertyToMilestone propertyToMilestone)
		{
			OrderToBulkUpdate orderToBulkUpdate = DummyParent.SelectedOrders.AddNew();
			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "splaty";
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			orderToBulkUpdate.SetOrder(order);

			var emptyValue = ZDateTime.Invalid;
			var someValue = ZDateTime.Now;
			var someOtherValue = ZDateTime.Now.AddDays(1);
			var someValue2 = ZDateTime.Now.AddDays(2);
			var someOtherValue2 = ZDateTime.Now.AddDays(3);

			orderToBulkUpdate.Order.UpdateEventEstimate(propertyToMilestone.EventType, someValue.ToOffset());
			bO[propertyToMilestone.EstimatedProperty] = emptyValue;
			orderToBulkUpdate.Order.UpdateEvent(propertyToMilestone.EventType, someValue2.ToOffset());
			bO[propertyToMilestone.ActualProperty] = emptyValue;
			orderToBulkUpdate.UpdateFrom(bO);
			AssertEquals("Should not change the value because it is empty", someValue, orderToBulkUpdate.Order.GetMilestoneEstimatedDate(propertyToMilestone.EventType).ToZDateTime());
			AssertEquals("Should not change the value because it is empty", someValue2, orderToBulkUpdate.Order.GetMilestoneActualDate(propertyToMilestone.EventType).ToZDateTime());

			bO[propertyToMilestone.EstimatedProperty] = someOtherValue;
			bO[propertyToMilestone.ActualProperty] = someOtherValue2;
			orderToBulkUpdate.UpdateFrom(bO);
			AssertEquals("Property " + propertyToMilestone.EstimatedProperty + " should update as a value is specified", someOtherValue, orderToBulkUpdate.Order.GetMilestoneEstimatedDate(propertyToMilestone.EventType).ToZDateTime());
			AssertEquals("Property " + propertyToMilestone.ActualProperty + " should update as a value is specified", someOtherValue2, orderToBulkUpdate.Order.GetMilestoneActualDate(propertyToMilestone.EventType).ToZDateTime());

			bO[propertyToMilestone.EstimatedProperty] = emptyValue;
			bO[propertyToMilestone.ActualProperty] = emptyValue;
		}

		void TestUpdatingNotes(OrderDetailsBulkUpdateBusinessObject bO)
		{
			OrderToBulkUpdate orderToBulkUpdate = DummyParent.SelectedOrders.AddNew();
			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "splaty";
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			orderToBulkUpdate.SetOrder(order);

			StmNote orderToBulkUpdateNote = orderToBulkUpdate.Order.Notes.AddNew(false, PredefinedNoteTypes.Instance.ExtraOrderDetails.Description, "Order Note to update");
			bO.Notes.AddNew(false, PredefinedNoteTypes.Instance.ExtraOrderDetails.Description, "Heya Splaty Order Note");
			orderToBulkUpdate.UpdateFrom(bO);
			Assert("note should be deleted for overriding", orderToBulkUpdateNote.IsDeleted);
			orderToBulkUpdateNote = orderToBulkUpdate.Order.Notes.FindByDescription(PredefinedNoteTypes.Instance.ExtraOrderDetails.Description)[0];
			AssertEquals("Should be 1 note", 1, orderToBulkUpdate.Order.Notes.GetAllNotes().Count);
			AssertEquals("Note should have updated as a value is specified", "Heya Splaty Order Note", orderToBulkUpdateNote.ST_NoteText);

			bO.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "New spec note");
			orderToBulkUpdate.UpdateFrom(bO);
			orderToBulkUpdateNote = orderToBulkUpdate.Order.Notes.FindByDescription(PredefinedNoteTypes.Instance.SpecialInstructions.Description)[0];
			AssertEquals("Should be 2 notes", 2, orderToBulkUpdate.Order.Notes.GetAllNotes().Count);
			AssertEquals("Note should have updated as a value is specified", "New spec note", orderToBulkUpdateNote.ST_NoteText);
		}

		void TestUpdatingAuditDetails(OrderDetailsBulkUpdateBusinessObject bo)
		{
			OrderToBulkUpdate orderToBulkUpdate = DummyParent.SelectedOrders.AddNew();
			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "splaty";
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			orderToBulkUpdate.SetOrder(order);

			Assert("Precondition: last edit time", !orderToBulkUpdate.Order.JD_SystemLastEditTimeUtc.IsValid);
			Assert("Precondition: last edit user", orderToBulkUpdate.Order.JD_SystemLastEditUser.IsEmpty);

			orderToBulkUpdate.UpdateFrom(bo);

			var expectedDate = new ZDate(2013, 9, 15);
			AssertEquals("Last Edit time should be updated", expectedDate, orderToBulkUpdate.Order.JD_SystemLastEditTimeUtc.Date);
			AssertEquals("Last Edit user should be updated", GlbStaff.CurrentUser.GS_Code, orderToBulkUpdate.Order.JD_SystemLastEditUser);
		}

		OrderDetailsBulkUpdateBusinessObject DummyParent;

		protected override void SetUp()
		{
			DummyParent = new OrderDetailsBulkUpdateBusinessObject(Factory);
			base.SetUp();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrderToBulkUpdate(Factory, DummyParent);
		}
	}
}
