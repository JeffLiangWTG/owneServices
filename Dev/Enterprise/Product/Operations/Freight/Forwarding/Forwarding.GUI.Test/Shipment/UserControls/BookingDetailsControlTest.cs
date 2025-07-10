using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	class BookingDetailsControlTest : TestCaseWithFactory
	{
		public void TestControlsCaption()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			using (var form = new BookingDetailsFormForTest(shipment))
			{
				form.Show();

				var voyageNumberBoundTextBox = form.FindControl("VoyageNumberBoundTextBox") as ZTextBox;
				AssertNotNull(voyageNumberBoundTextBox);
				AssertEquals("Voyage No", voyageNumberBoundTextBox.GetExtension<LabelCaptionRenderer>().Caption);

				var lclCutOffBoundReadOnlyDateEdit = form.FindControl("JS_Calc_LCLCutOffBoundReadOnlyDateEdit") as ZDateEdit;
				AssertNotNull(lclCutOffBoundReadOnlyDateEdit);
				AssertEquals("CFS Cut Off", lclCutOffBoundReadOnlyDateEdit.GetExtension<LabelCaptionRenderer>().Caption);

				var sailingSummarygroupBox = form.FindControl("SailingSummarygroupBox") as ZGroupBox;
				AssertNotNull(sailingSummarygroupBox);
				AssertEquals("Sailing Summary", sailingSummarygroupBox.Text);

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				AssertEquals("Flight No", voyageNumberBoundTextBox.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("Flight Summary", sailingSummarygroupBox.Text);

				shipment.JS_TransportMode = Constants.TransportModes.Rail;
				AssertEquals("Journey", voyageNumberBoundTextBox.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("Journey Summary", sailingSummarygroupBox.Text);

				shipment.JS_TransportMode = Constants.TransportModes.Road;
				AssertEquals("Truck Ref.", voyageNumberBoundTextBox.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("Journey Summary", sailingSummarygroupBox.Text);
			}
		}

		[RequiresSTA]
		public void TestControlsVisibleBasedOnTransportMode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			using (var form = new BookingDetailsFormForTest(shipment))
			{
				form.Show();

				var vesselBoundTextBox = form.FindControl("VesselBoundTextBox");
				AssertNotNull(vesselBoundTextBox);
				Assert(vesselBoundTextBox.Visible);

				var fclCutOffBoundReadOnlyDateEdit = form.FindControl("JS_Calc_FCLCutOffBoundReadOnlyDateEdit");
				AssertNotNull(fclCutOffBoundReadOnlyDateEdit);
				Assert(fclCutOffBoundReadOnlyDateEdit.Visible);

				shipment.JS_TransportMode = Constants.TransportModes.Rail;
				Assert(vesselBoundTextBox.Visible);
				Assert(fclCutOffBoundReadOnlyDateEdit.Visible);

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				Assert(!vesselBoundTextBox.Visible);
				Assert(!fclCutOffBoundReadOnlyDateEdit.Visible);

				shipment.JS_TransportMode = Constants.TransportModes.Road;
				Assert(!vesselBoundTextBox.Visible);
				Assert(fclCutOffBoundReadOnlyDateEdit.Visible);
			}
		}

		[RequiresSTA]
		public void TestControlsVisibleBasedOnPackingMode()
		{
			AssertControlsVisibleBasedOnPackingMode(Constants.ContainerModes.FCL, true, true, false, false);
			AssertControlsVisibleBasedOnPackingMode(Constants.ContainerModes.LCL, true, true, false, false);
			AssertControlsVisibleBasedOnPackingMode(Constants.ContainerModes.RollOnRollOff, false, false, false, false);
			AssertControlsVisibleBasedOnPackingMode(Constants.ContainerModes.Liquid, false, false, false, false);
			AssertControlsVisibleBasedOnPackingMode(Constants.ContainerModes.Bulk, false, false, false, false);
			AssertControlsVisibleBasedOnPackingMode(Constants.ContainerModes.BreakBulk, false, false, false, false);
			AssertControlsVisibleBasedOnPackingMode(Constants.ContainerModes.BuyersConsol, false, false, true, true);
		}

		[RequiresSTA]
		public void TestControlsReadOnly()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (var form = new BookingDetailsFormForTest(shipment))
			{
				form.Show();

				var bookedShippingLineBoundOrgFindBox = form.FindControl("BookedShippingLineBoundOrgFindBox") as ZOrganisationFindBox;
				AssertNotNull(bookedShippingLineBoundOrgFindBox);
				Assert(bookedShippingLineBoundOrgFindBox.ReadOnly);
			}
		}

		#region Implementation

		void AssertControlsVisibleBasedOnPackingMode(ZString packingMode, bool bookedShippingLineBoundOrgFindBoxVisible, bool bookingRefTextBoxVisible,
					bool sailingTotalVolumeCalcDropEditVisible, bool sailingTotalWeightCalcDropEditVisible)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = packingMode;

			using (var form = new BookingDetailsFormForTest(shipment))
			{
				form.Show();

				var bookedShippingLineBoundOrgFindBox = form.FindControl("BookedShippingLineBoundOrgFindBox");
				AssertNotNull(bookedShippingLineBoundOrgFindBox);
				AssertEquals(bookedShippingLineBoundOrgFindBoxVisible, bookedShippingLineBoundOrgFindBox.Visible);

				var bookingRefTextBox = form.FindControl("BookingRefTextBox");
				AssertNotNull(bookingRefTextBox);
				AssertEquals(bookingRefTextBoxVisible, bookingRefTextBox.Visible);

				var sailingTotalVolumeCalcDropEdit = form.FindControl("SailingTotalVolumeCalcDropEdit");
				AssertNotNull(sailingTotalVolumeCalcDropEdit);
				AssertEquals(sailingTotalVolumeCalcDropEditVisible, sailingTotalVolumeCalcDropEdit.Visible);

				var sailingTotalWeightCalcDropEdit = form.FindControl("SailingTotalWeightCalcDropEdit");
				AssertNotNull(sailingTotalWeightCalcDropEdit);
				AssertEquals(sailingTotalWeightCalcDropEditVisible, sailingTotalWeightCalcDropEdit.Visible);
			}
		}

		class BookingDetailsFormForTest : ZForm
		{
			public BookingDetailsFormForTest(ForwardingShipment shipment)
				: base(shipment)
			{
				bookingDetailsControl = new BookingDetailsControl();
				Controls.Add(bookingDetailsControl);
			}

			public Control FindControl(string name)
			{
				return bookingDetailsControl.Controls.Find(name, true).FirstOrDefault();
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					bookingDetailsControl.Dispose();
				}

				base.Dispose(disposing);
			}

			readonly BookingDetailsControl bookingDetailsControl;
		}

		#endregion
	}
}
