using System.Drawing;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class ConsolLegDetailsControlTest : BaseFreightTest
	{
		#region TestWarningVisibility

		public void TestWarningVisibility()
		{
			AssertEquals("precondition: ", 1, Consol.Transports.Count);

			using (ConsolLegDetailsTestForm form = new ConsolLegDetailsTestForm(Consol))
			{
				form.Show();
				AssertEquals("should not show the warning as the consol only has one leg", false, form.warningLabel.Visible);

				consol.Transports.AddNew();
				AssertEquals("should show the warning as the consol now has more than one leg", true, form.warningLabel.Visible);

				consol.Transports.AddNew();
				AssertEquals("dont just toggle when adding a transport", true, form.warningLabel.Visible);

				consol.Transports[0].Delete();
				AssertEquals("dont just toggle when removing a transport", true, form.warningLabel.Visible);

				consol.Transports[0].Delete();
				AssertEquals("should not show the warning as we are back to only one leg", false, form.warningLabel.Visible);
			}
		}

		#endregion

		#region TestPanelSelection

		public void TestPanelSelection()
		{
			Transport transport = Consol.Transports[0];

			using (ConsolLegDetailsTestForm form = new ConsolLegDetailsTestForm(Consol))
			{
				form.Show();

				transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
				AssertTransportMode("setting to sea", Core.Constants.TransportModes.Sea, form);

				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				AssertTransportMode("setting to air", Core.Constants.TransportModes.Air, form);

				transport.JW_TransportMode = Core.Constants.TransportModes.Road;
				AssertTransportMode("setting to road", Core.Constants.TransportModes.Road, form);

				transport.JW_TransportMode = Core.Constants.TransportModes.Rail;
				AssertTransportMode("setting to rail", Core.Constants.TransportModes.Rail, form);

				transport.JW_TransportMode = "";
				AssertTransportMode("setting to an invalid value", "", form);
			}
		}

		void AssertTransportMode(ZString message, ZString mode, ConsolLegDetailsTestForm form)
		{
			AssertEquals(message + ": SelectedTransportMode", mode, form.control.SelectedTransportMode);
			AssertEquals(message + ": SeaPanel visibility", (mode == Core.Constants.TransportModes.Sea || mode == ""), form.SeaPanel.Visible);
			AssertEquals(message + ": AirPanel visibility", (mode == Core.Constants.TransportModes.Air), form.AirPanel.Visible);
			AssertEquals(message + ": RoadPanel visibility", (mode == Core.Constants.TransportModes.Road), form.RoadPanel.Visible);
			AssertEquals(message + ": RailPanel visibility", (mode == Core.Constants.TransportModes.Rail), form.RailPanel.Visible);
		}

		#endregion

		#region TestIsCargoControlVisibility

		public void TestIsCargoControlVisibility()
		{
			Transport transport = Consol.MostInterestingTransportForBinding[0];

			using (ConsolLegDetailsTestForm form = new ConsolLegDetailsTestForm(Consol))
			{
				form.Show();

				transport.JW_IsLinked = true;
				transport.JW_TransportMode = Core.Constants.TransportModes.Sea;

				AssertEquals("should not show the cargoonly control as transportmode is sea.", false, form.CargoOnlyControl.Visible);

				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("should show the cargoonly control.", true, form.CargoOnlyControl.Visible);
			}
		}

		#endregion

		#region TestAircraftTypeControlVisibility

		public void TestAircraftTypeControlVisibility()
		{
			var transport = Consol.MostInterestingTransportForBinding[0];

			using (ConsolLegDetailsTestForm form = new ConsolLegDetailsTestForm(Consol))
			{
				form.Show();

				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				var aircraftTypeEdit = form.Controls.Find("JW_AircraftTypeTextBox", true).FirstOrDefault();
				AssertNotNull(aircraftTypeEdit);
				AssertEquals("aircraftTypeEdit should be visible", true, aircraftTypeEdit.Visible);

				transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
				aircraftTypeEdit = form.Controls.Find("JW_AircraftTypeTextBox", true).FirstOrDefault();
				AssertNotNull(aircraftTypeEdit);
				AssertEquals("aircraftTypeEdit should be invisible", false, aircraftTypeEdit.Visible);
			}
		}

		#endregion

		#region Flight Status

		public void TestFlightStatusLabel_Format()
		{
			var transport = Consol.MostInterestingTransportForBinding[0];
			transport.JW_IsLinked = true;
			transport.JW_OnlineScheduleStatus = Core.Constants.FlightScheduleStatus.Unknown;

			using (var testControl = new ConsoLegDetailsControl())
			{
				var flightStatusControl = testControl.Controls.Find("FlightStatusLabel", true).FirstOrDefault();
				AssertNoExceptionThrown(() => flightStatusControl.Text = "XXXX");

				testControl.SetDataBinding(Consol, "");

				transport.JW_OnlineScheduleStatus = Core.Constants.FlightScheduleStatus.Matched;
				flightStatusControl.Text = "Matched";
				AssertEquals("flight status control colour: Matched", Color.FromArgb(215, 255, 215), flightStatusControl.BackColor);

				transport.JW_OnlineScheduleStatus = Core.Constants.FlightScheduleStatus.Unknown;
				flightStatusControl.Text = "Unknown";
				AssertEquals("flight status control colour: Unknown", Color.FromArgb(255, 215, 215), flightStatusControl.BackColor);

				transport.JW_OnlineScheduleStatus = Core.Constants.FlightScheduleStatus.PartiallyMatched;
				flightStatusControl.Text = "Partially Matched";
				AssertEquals("flight status control colour: PartiallyMatched", Color.FromArgb(255, 210, 166), flightStatusControl.BackColor);

				transport.JW_OnlineScheduleStatus = Core.Constants.FlightScheduleStatus.Unmatched;
				flightStatusControl.Text = "Unmatched";
				AssertEquals("flight status control colour: Unmatched", Color.FromArgb(255, 215, 215), flightStatusControl.BackColor);

				transport.JW_OnlineScheduleStatus = Core.Constants.FlightScheduleStatus.Active;
				flightStatusControl.Text = "Active";
				AssertEquals("flight status control colour: Active", Color.FromArgb(215, 255, 215), flightStatusControl.BackColor);

				transport.JW_OnlineScheduleStatus = Core.Constants.FlightScheduleStatus.Cancelled;
				flightStatusControl.Text = "Cancelled";
				AssertEquals("flight status control colour: Cancelled", Color.FromArgb(255, 215, 215), flightStatusControl.BackColor);

				transport.JW_OnlineScheduleStatus = Core.Constants.FlightScheduleStatus.PreDeparture;
				flightStatusControl.Text = "Pre-departure";
				AssertEquals("flight status control colour: PreDeparture", Color.FromArgb(215, 255, 215), flightStatusControl.BackColor);

				transport.JW_OnlineScheduleStatus = Core.Constants.FlightScheduleStatus.DepartureDelay;
				flightStatusControl.Text = "Departure Delay";
				AssertEquals("flight status control colour: DepartureDelay", Color.FromArgb(255, 210, 166), flightStatusControl.BackColor);

				transport.JW_OnlineScheduleStatus = Core.Constants.FlightScheduleStatus.Departed;
				flightStatusControl.Text = "Departed";
				AssertEquals("flight status control colour: Departed", Color.FromArgb(215, 255, 215), flightStatusControl.BackColor);

				transport.JW_OnlineScheduleStatus = Core.Constants.FlightScheduleStatus.Diversion;
				flightStatusControl.Text = "Diversion";
				AssertEquals("flight status control colour: Diversion", Color.FromArgb(255, 210, 166), flightStatusControl.BackColor);

				transport.JW_OnlineScheduleStatus = Core.Constants.FlightScheduleStatus.ArrivalDelay;
				flightStatusControl.Text = "Arrival Delay";
				AssertEquals("flight status control colour: ArrivalDelay", Color.FromArgb(255, 210, 166), flightStatusControl.BackColor);

				transport.JW_OnlineScheduleStatus = Core.Constants.FlightScheduleStatus.PreArrival;
				flightStatusControl.Text = "Pre-arrival";
				AssertEquals("flight status control colour: PreArrival", Color.FromArgb(215, 255, 215), flightStatusControl.BackColor);

				transport.JW_OnlineScheduleStatus = Core.Constants.FlightScheduleStatus.Arrived;
				flightStatusControl.Text = "Arrived";
				AssertEquals("flight status control colour: Arrived", Color.FromArgb(215, 255, 215), flightStatusControl.BackColor);
			}
		}

		public void TestFlightStatusLabel_Visibility()
		{
			var transport = Consol.MostInterestingTransportForBinding[0];

			using (var testControl = new ConsoLegDetailsControl())
			{
				testControl.SetDataBinding(Consol, "");
				var flightStatusLabel = testControl.Controls.Find("FlightStatusLabel", true).FirstOrDefault();

				transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("should not show the flight status control as transportmode is sea.", false, flightStatusLabel.Visible);

				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("should show the flight status control", true, flightStatusLabel.Visible);
			}
		}

		#endregion

		#region Consol

		CommonConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Factory.New<CommonConsol>();
				}
				return consol;
			}
		}
		CommonConsol consol;

		#endregion

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();

			consol = null;
		}

		#endregion
	}
}
