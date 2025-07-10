using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingOrderWebUserVisibleNotesTest : IWebUserVisibleNotesSupportTest
	{
		#region TestShowAgentNotes

		public void TestShowAgentNotesIfSendingAgentIsSet()
		{
			var helper = new TestHelper(Factory);
			var order = helper.CreateOrder();
			var shipment = Factory.New<TrackingShipment>();
			shipment.SiteUser = helper.TestSiteUser;
			shipment.Consols.AddNew().SetDefaultSendingForwarderAddress(order.LoggedInOrganisation);

			order.LoggedInContact = helper.TestContact;

			Factory.Save();

			order.JD_JS = shipment.PK;

			AssertEquals("SendingAgent should be set", order.LoggedInOrganisation.PK, order.JD_OH_SendingAgent);
			Assert("Should show Agent Notes", order.ShowAgentNotes);
		}

		public void TestShowAgentNotesIfReceivingAgentIsSet()
		{
			var helper = new TestHelper(Factory);
			var order = helper.CreateOrder();
			var shipment = Factory.New<TrackingShipment>();
			shipment.SiteUser = helper.TestSiteUser;
			shipment.Consols.AddNew().SetDefaultReceivingForwarderAddress(order.LoggedInOrganisation);

			order.LoggedInContact = helper.TestContact;

			Factory.Save();

			order.JD_JS = shipment.PK;

			AssertEquals("ReceivingAgent should be set", order.LoggedInOrganisation.PK, order.JD_OH_ReceivingAgent);
			Assert("Should show Agent Notes", order.ShowAgentNotes);
		}

		public void TestShowAgentNotesIfDeliveryAgentIsSet()
		{
			TrackingOrder order = GetOrderAndAssertPreconditions();

			order.Shipment.JS_OH_DeliveryAgent = order.LoggedInOrganisation.PK;

			Assert("Should show Agent Notes", order.ShowAgentNotes);

			order.LoggedInContact = null;

			Assert("Should show Agent Notes", order.ShowAgentNotes);
		}

		#endregion

		#region Implementation

		protected override IWebUserVisibleNotesSupport GetNewBusinessObject()
		{
			TestHelper helper = new TestHelper(Factory);
			TrackingOrder testOrder = helper.CreateOrder();
			testOrder.JD_JS = helper.CreateShipment().PK;
			testOrder.LoggedInContact = helper.TestContact;
			testOrder.Shipment.SiteUser = helper.TestSiteUser;
			return testOrder;
		}

		protected override void SetAgentNotesVisibility(IWebUserVisibleNotesSupport notesSupport, bool visibility)
		{
			TrackingOrder order = notesSupport as TrackingOrder;
			AssertNotNull("Should be TrackingOrder", order);
			AssertNotNull("SiteUser should be logged in", order.LoggedInContact);
			if (visibility)
			{
				order.Shipment.JS_OH_DeliveryAgent = order.LoggedInOrganisation.PK;
			}
			else
			{
				if (order.Shipment.JS_OH_DeliveryAgent == order.LoggedInOrganisation.PK)
				{
					order.Shipment.JS_OH_DeliveryAgent = ZGuid.Empty;
				}

				if (order.Shipment.JS_OH_TranshipAgent == order.LoggedInOrganisation.PK)
				{
					order.Shipment.JS_OH_TranshipAgent = ZGuid.Empty;
				}
			}
		}

		protected override BusinessObject GetRelatedBusinessObject(IWebUserVisibleNotesSupport parent)
		{
			return null;
		}

		TrackingOrder GetOrderAndAssertPreconditions()
		{
			TrackingOrder order = BizObj as TrackingOrder;
			AssertNotNull("Should be TrackingOrder", order);
			AssertNotNull("Should be logged in", order.LoggedInOrganisation);
			AssertEquals("Should not show Agent Notes", false, order.ShowAgentNotes);
			return order;
		}

		#endregion
	}
}
