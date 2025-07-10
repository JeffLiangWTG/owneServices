using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Common.Testing
{
	sealed class PartialEventsControlTestCase : TestCaseWithFactory
	{
		#region TestArrivalEvents

		public void TestArrivalEvents()
		{
			var consol = CreateNewConsol("AUSYD", "USLAX");
			using (var form = new ZForm(consol))
			using (var control = new PartialEventsInfoControl())
			{
				control.EventsToShow = PartialEventsInfoControl.EventTypes.Arrival;

				form.Controls.Add(control);

				form.Show();

				AssertMultilineASCIIEquals("events details",
@"USLAX QF001 2015-01-02 1 of 5 pieces
USLAX QF002 2015-01-02 3 of 5 pieces",
					control.Controls.Find("detailsTextBox", true).First().Text);

				AssertMultilineASCIIEquals("total",
					"Running Total: 4 of 5",
					control.Controls.Find("totalTextBox", true).First().Text);
			}
		}

		#endregion

		#region TestArrivalEvents_NoEventsOnParent

		[RequiresSTA]
		public void TestArrivalEvents_NoEventsOnParent()
		{
			var consol = Factory.New<ForwardingConsol>();

			using (var form = new ZForm(consol))
			using (var control = new PartialEventsInfoControl())
			{
				control.EventsToShow = PartialEventsInfoControl.EventTypes.Arrival;

				form.Controls.Add(control);

				form.Show();

				AssertMultilineASCIIEquals("events details",
					string.Empty,
					control.Controls.Find("detailsTextBox", true).First().Text);

				AssertMultilineASCIIEquals("total",
					string.Empty,
					control.Controls.Find("totalTextBox", true).First().Text);
			}
		}

		#endregion

		#region TestArrivalEvents_InconsitentTotals

		public void TestArrivalEvents_InconsitentTotals()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "SGSIN";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;

			AddLog(consol, Events.FreightUnloadedCode, "QF001", new ZDateTimeOffset(2015, 1, 2), "AUSYD", 1, 5);
			AddLog(consol, Events.FreightUnloadedCode, "QF002", new ZDateTimeOffset(2015, 1, 2), "AUSYD", 3, 7);

			using (var form = new ZForm(consol))
			using (var control = new PartialEventsInfoControl())
			{
				control.EventsToShow = PartialEventsInfoControl.EventTypes.Arrival;

				form.Controls.Add(control);

				form.Show();

				AssertMultilineASCIIEquals("events details",
@"AUSYD QF001 2015-01-02 1 of 5 pieces
AUSYD QF002 2015-01-02 3 of 7 pieces",
					control.Controls.Find("detailsTextBox", true).First().Text);

				AssertMultilineASCIIEquals("total",
					"Running Total: 4 of 7",
					control.Controls.Find("totalTextBox", true).First().Text);
			}
		}

		#endregion

		#region TestDepartureEvents

		public void TestDepartureEvents()
		{
			var consol = CreateNewConsol("AUSYD", "USLAX");

			using (var form = new ZForm(consol))
			using (var control = new PartialEventsInfoControl())
			{
				control.EventsToShow = PartialEventsInfoControl.EventTypes.Departure;

				form.Controls.Add(control);

				form.Show();

				AssertMultilineASCIIEquals("events details",
@"AUSYD QF001 2015-01-01 1 of 5 pieces
AUSYD QF002 2015-01-01 3 of 5 pieces",
					control.Controls.Find("detailsTextBox", true).First().Text);

				AssertMultilineASCIIEquals("total",
					"Running Total: 4 of 5",
					control.Controls.Find("totalTextBox", true).First().Text);
			}
		}

		#endregion

		#region TestNoExceptionsWHenBoundToNonLogSupporter

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestNoExceptionsWHenBoundToNonLogSupporter()
		{
			var dummy = Factory.New<DummyBusinessObject>();

			using (var form = new ZForm(dummy))
			using (var control = new PartialEventsInfoControl())
			{
				control.EventsToShow = PartialEventsInfoControl.EventTypes.Arrival;

				form.Controls.Add(control);

				form.Show();
			}
		}

		#endregion

		#region TestShowControlIfNotEmpty

		public void TestShowControlIfThereAreAnyApplicableLogs()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "SGSIN";
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;

			AddLog(consol, Events.FreightUnloadedCode, "QF001", new ZDateTimeOffset(2015, 1, 2), "AUSYD", 1, 5);

			using (var form = new ZForm(consol))
			using (var control = new PartialEventsInfoControl())
			{
				control.EventsToShow = PartialEventsInfoControl.EventTypes.Arrival;

				form.Controls.Add(control);

				form.Show();

				AssertEquals("Control is visible", true, control.Visible);
			}
		}

		#endregion

		#region TestHideControlIfThereAreNotAnyApplicableLogs

		public void TestHideControlIfThereAreNotAnyApplicableLogs()
		{
			var consol = Factory.New<ForwardingConsol>();

			using (var form = new ZForm(consol))
			using (var control = new PartialEventsInfoControl())
			{
				control.EventsToShow = PartialEventsInfoControl.EventTypes.Arrival;

				form.Controls.Add(control);

				form.Show();

				AssertEquals("Control is not visible", false, control.Visible);
			}
		}

		#endregion

		#region Implementation

		void AddLog(IStmALogParent logParent, string eventCode, ZString flightNumber, ZDateTimeOffset flightDate, ZString location, int partial, int total)
		{
			logParent.Logs.AddNew(Events.All[eventCode],
				ZString.Empty,
				flightDate,
				new[]
				{
					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location,
						location),

					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Partial,
						partial.ToString()),

					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Total,
						total.ToString()),

					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.VoyageFlightNumber,
						flightNumber),

					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.FlightDate,
						flightDate.ToZDateTime().ToISO8601ShortDateString()),

					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility,
						"CTO")
				});
		}

		ForwardingConsol CreateNewConsol(ZString loadPort, ZString dischargePort)
		{
			var transhipPort1 = "SGSIN";
			var transhipPort2 = "HKHKG";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = loadPort;
			consol.JK_RL_NKDischargePort = dischargePort;

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = loadPort;
			transport1.JW_RL_NKDiscPort = transhipPort1;
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = transhipPort1;
			transport2.JW_RL_NKDiscPort = transhipPort2;
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;

			var transport3 = consol.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = transhipPort2;
			transport3.JW_RL_NKDiscPort = dischargePort;
			transport3.JW_TransportMode = Core.Constants.TransportModes.Air;

			AssertEquals("First Air Leg", transport1, consol.Transports.FirstTransportWithTransportMode("AIR"));
			AssertEquals("Last Air Leg", transport3, consol.Transports.LastTransportWithTransportMode("AIR"));

			AddLog(consol, Events.FreightLoadedCode, "QF001", new ZDateTimeOffset(2015, 1, 1), loadPort, 1, 5);
			AddLog(consol, Events.FreightLoadedCode, "QF002", new ZDateTimeOffset(2015, 1, 1), loadPort, 3, 5);
			AddLog(consol, Events.FreightLoadedCode, "QF001", new ZDateTimeOffset(2015, 1, 1), transhipPort1, 1, 5);
			AddLog(consol, Events.FreightLoadedCode, "QF002", new ZDateTimeOffset(2015, 1, 1), transhipPort1, 3, 5);
			AddLog(consol, Events.FreightLoadedCode, "QF001", new ZDateTimeOffset(2015, 1, 1), transhipPort2, 1, 5);
			AddLog(consol, Events.FreightLoadedCode, "QF002", new ZDateTimeOffset(2015, 1, 1), transhipPort2, 3, 5);
			AddLog(consol, Events.FreightLoadedCode, "QF001", new ZDateTimeOffset(2015, 1, 1), dischargePort, 1, 5);
			AddLog(consol, Events.FreightLoadedCode, "QF002", new ZDateTimeOffset(2015, 1, 1), dischargePort, 3, 5);

			AddLog(consol, Events.FreightUnloadedCode, "QF001", new ZDateTimeOffset(2015, 1, 2), loadPort, 1, 5);
			AddLog(consol, Events.FreightUnloadedCode, "QF002", new ZDateTimeOffset(2015, 1, 2), loadPort, 3, 5);
			AddLog(consol, Events.FreightUnloadedCode, "QF001", new ZDateTimeOffset(2015, 1, 2), transhipPort1, 1, 5);
			AddLog(consol, Events.FreightUnloadedCode, "QF002", new ZDateTimeOffset(2015, 1, 2), transhipPort1, 3, 5);
			AddLog(consol, Events.FreightUnloadedCode, "QF001", new ZDateTimeOffset(2015, 1, 2), transhipPort2, 1, 5);
			AddLog(consol, Events.FreightUnloadedCode, "QF002", new ZDateTimeOffset(2015, 1, 2), transhipPort2, 3, 5);
			AddLog(consol, Events.FreightUnloadedCode, "QF001", new ZDateTimeOffset(2015, 1, 2), dischargePort, 1, 5);
			AddLog(consol, Events.FreightUnloadedCode, "QF002", new ZDateTimeOffset(2015, 1, 2), dischargePort, 3, 5);

			AddLog(consol, Events.ReceivedCode, "QF001", new ZDateTimeOffset(2015, 1, 3), loadPort, 1, 5);
			AddLog(consol, Events.ReceivedCode, "QF002", new ZDateTimeOffset(2015, 1, 3), loadPort, 3, 5);
			AddLog(consol, Events.ReceivedCode, "QF001", new ZDateTimeOffset(2015, 1, 3), transhipPort1, 1, 5);
			AddLog(consol, Events.ReceivedCode, "QF002", new ZDateTimeOffset(2015, 1, 3), transhipPort1, 3, 5);
			AddLog(consol, Events.ReceivedCode, "QF001", new ZDateTimeOffset(2015, 1, 3), transhipPort2, 1, 5);
			AddLog(consol, Events.ReceivedCode, "QF002", new ZDateTimeOffset(2015, 1, 3), transhipPort2, 3, 5);
			AddLog(consol, Events.ReceivedCode, "QF001", new ZDateTimeOffset(2015, 1, 3), dischargePort, 1, 5);
			AddLog(consol, Events.ReceivedCode, "QF002", new ZDateTimeOffset(2015, 1, 3), dischargePort, 3, 5);

			return consol;
		}

		#endregion
	}
}
