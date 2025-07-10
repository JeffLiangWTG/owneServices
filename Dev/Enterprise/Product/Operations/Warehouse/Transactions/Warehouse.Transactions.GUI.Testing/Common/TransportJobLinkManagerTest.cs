using System.Drawing;
using System.Windows.Forms;
using Enterprise.Freight.Common.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportConsignment;
using Enterprise.TransportBookings.Shared;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class TransportJobLinkManagerTest : WhsTestCaseWithFactory
	{
		#region TestManageLink

		public void TestManageLink()
		{
			AssertNotEquals("Precondition", SystemColors.WindowText, LinkLabel.LinkColor);
			AssertEquals("Precondition", "", LinkLabel.Text);

			TransportJobLinkManager.ManageLink();
			AssertEquals("ManageLink() should perform an initial update.", SystemColors.WindowText, LinkLabel.LinkColor);
			AssertEquals("ManageLink() should perform an initial update.", "No Transport Job exists.", LinkLabel.Text);

			// Create and save a TB
			var consol = Factory.New<IDtbBookingConsolidation>();
			consol.KB_ParentID = Order.PK;
			consol.KB_ParentTableCode = WhsDocketSchema.Constants.Prefix;

			var booking = Factory.New<IDtbBooking>();
			booking.KM_KB_Booking = consol.PK;
			booking.KM_JobID = "Booking";
			Factory.Save();

			// simulate DtbDeliveryManger calling this method on creation
			((IDtbBookingParent)Order).TransportBookingCreatedOrUpdated();
			AssertEquals("Link Colour should be auto-updated when the TB is first saved.", Color.Red, LinkLabel.LinkColor);
			AssertEquals("Link Text should be auto-updated when the TB is first saved.", "Booking", LinkLabel.Text);

			// simulate user clicking the link
			LinkLabelAsButtonControl.PerformClick();
			AssertEquals(ControllerIDs.DtbBooking, TransportJobLinkManager.Controller.LastShownForm.ControllerID);
			TransportJobLinkManager.Controller.LastShownForm.Dispose();

			// create and save a cartage job
			Helper.CreateCartageJob(Order);
			Factory.Save();

			// simulate cartage manager calling this method on first save
			((ICartageParent)Order).CartageCreatedAndSaved();
			AssertEquals("Link Colour should be auto-updated when the CartageJob is first saved.", Color.Red, LinkLabel.LinkColor);
			AssertEquals("Link Text should be auto-updated when the CartageJob is first saved.", "Order1", LinkLabel.Text);

			// simulate user clicking the link
			LinkLabelAsButtonControl.PerformClick();
			AssertEquals(ControllerIDs.Cartage, TransportJobLinkManager.Controller.LastShownForm.ControllerID);
			TransportJobLinkManager.Controller.LastShownForm.Dispose();

			// create and save another cartage job (this can happen functionally via TBs)
			var cartage2 = Helper.CreateCartageJob(Order);
			cartage2.JJ_ConsignmentID = "PT2";
			Factory.Save();

			// simulate cartage manager calling this method on first save
			((ICartageParent)Order).CartageCreatedAndSaved();
			AssertEquals("Link Colour should be auto-updated when the CartageJob is first saved.", SystemColors.WindowText, LinkLabel.LinkColor);
			AssertEquals("Link Text should be auto-updated when the CartageJob is first saved.", "Multiple Jobs", LinkLabel.Text);
		}

		#endregion

		#region TestUpdateLink

		public void TestUpdateLink()
		{
			AssertNotEquals("Precondition", SystemColors.WindowText, LinkLabel.LinkColor);
			AssertEquals("Precondition", "", LinkLabel.Text);

			TransportJobLinkManager.UpdateLink();
			AssertEquals(SystemColors.WindowText, LinkLabel.LinkColor);
			AssertEquals("No Transport Job exists.", LinkLabel.Text);

			LinkLabelAsButtonControl.PerformClick();
			AssertNull("Clicking the link should not open any form as no Cartage Job exists.", TransportJobLinkManager.Controller);

			// create and save a cartage job
			Helper.CreateCartageJob(Order);
			Factory.Save();

			TransportJobLinkManager.UpdateLink();
			AssertEquals(Color.Red, LinkLabel.LinkColor);
			AssertEquals("Order1", LinkLabel.Text);

			// simulate user clicking the link
			LinkLabelAsButtonControl.PerformClick();
			AssertEquals(ControllerIDs.Cartage, TransportJobLinkManager.Controller.LastShownForm.ControllerID);
			TransportJobLinkManager.Controller.LastShownForm.Dispose();
		}

		public void TestUpdateLinkDoesNotIncludeCSNBookings()
		{
			var consol = Factory.New<IDtbBookingConsolidation>();
			consol.KB_ParentID = Order.PK;
			consol.KB_ParentTableCode = WhsDocketSchema.Constants.Prefix;

			var booking = Factory.New<IDtbBooking>();
			booking.KM_KB_Booking = consol.PK;
			booking.KM_JobID = "Booking";

			var consignmentConsol = Factory.New<IDtbConsignmentConsolidation>();
			consignmentConsol.KB_ParentID = booking.PK;
			consignmentConsol.KB_ParentTableCode = DtbBookingSchema.Constants.Prefix;

			var consignment = Factory.New<IDtbBookingConsignment>();
			consignment.KM_KB_Booking = consignmentConsol.PK;
			consignment.KM_JobID = "ConsignmentBooking";

			Factory.Save();

			TransportJobLinkManager.UpdateLink();
			AssertEquals("Link text should be correct.", "Booking", LinkLabel.Text);

			// simulate user clicking the link
			LinkLabelAsButtonControl.PerformClick();
			AssertEquals("The last form opened should be a Transport Booking form.", ControllerIDs.DtbBooking, TransportJobLinkManager.Controller.LastShownForm.ControllerID);
			TransportJobLinkManager.Controller.LastShownForm.Dispose();
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var data = new TestDataSimpleEnvironment(Factory);

			Order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Order.WD_DocketID = "Order1";

			LinkLabel = new ZLinkLabel();
			LinkLabel.LinkColor = Color.Azure;
			LinkLabelAsButtonControl = LinkLabel;

			TransportJobLinkManager = new TransportJobLinkManager(Order, LinkLabel);
		}

		protected override void TearDown()
		{
			base.TearDown();
			TransportJobLinkManager.Dispose();
			LinkLabel.Dispose();
		}

		WhsOrder Order;
		ZLinkLabel LinkLabel;
		IButtonControl LinkLabelAsButtonControl;
		TransportJobLinkManager TransportJobLinkManager;

		#endregion
	}
}
