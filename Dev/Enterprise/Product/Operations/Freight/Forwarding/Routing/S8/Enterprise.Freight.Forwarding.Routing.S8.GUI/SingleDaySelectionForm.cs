namespace Enterprise.Freight.Forwarding.Routing.S8.GUI
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Windows.Forms;
	using CargoWise.Common;
	using CargoWise.Types;
	using Enterprise.Freight.Forwarding.Routing.S8.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.GUI;

	public partial class SingleDaySelectionForm : ZChildForm
	{
		public SingleDaySelectionForm(ZDateTime requestedDate, RoutingResponseHeader routingResponseHeader)
		{
			var allRadioButtons = new[]
			{
				radioButton1,
				radioButton2,
				radioButton3,
				radioButton4,
				radioButton5,
				radioButton6,
				radioButton7
			};

			var foundFirstEnabledRadioButton = false;

			DayOptions = new List<DayOption>();

			for (var i = 0; i < 7; i++)
			{
				var date = requestedDate.AddDays(i);
				var radioButton = allRadioButtons[i];

				radioButton.Enabled = routingResponseHeader.HasOperation(date);
				radioButton.Text = ResString.GetMultilingualString("C7B9BBFF-4DC3-4442-BDE2-12E53634668C", "{0} ({1})", WeekDayDescriptions[date.DayOfWeek], date.ToShortDateString());

				if (radioButton.Enabled && !foundFirstEnabledRadioButton)
				{
					radioButton.Checked = true;
					foundFirstEnabledRadioButton = true;
				}

				DayOptions.Add(new DayOption(radioButton, date));
			}

			if (!foundFirstEnabledRadioButton)
			{
				var routingLineInformation = GetRoutingLineInformation(requestedDate, routingResponseHeader);
				ErrorReporter.ReportOnce("S8_SingleDaySelectionForm_InvalidOperationException", routingLineInformation); // Internal error description
			}
		}

		public ZDateTime SelectedDepartureDate { get; private set; } = ZDateTime.Empty;

		void SelectButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Yes;
			SelectedDepartureDate = DayOptions.Where(x => x.RadioButton.Checked).Select(x => x.Date).FirstOrDefault();
			Close();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		public class DayOption
		{
			public ZRadioButton RadioButton;
			public ZDateTime Date;

			public DayOption(ZRadioButton radionButton, ZDateTime date)
			{
				RadioButton = radionButton;
				Date = date;
			}
		}

		protected List<DayOption> DayOptions { get; set; }

		readonly Dictionary<DayOfWeek, MultilingualString> WeekDayDescriptions = new Dictionary<DayOfWeek, MultilingualString>
		{
			{ DayOfWeek.Monday,    AutoDayOfWeekCodeList.Descriptions.Monday },
			{ DayOfWeek.Tuesday,   AutoDayOfWeekCodeList.Descriptions.Tuesday },
			{ DayOfWeek.Wednesday, AutoDayOfWeekCodeList.Descriptions.Wednesday },
			{ DayOfWeek.Thursday,  AutoDayOfWeekCodeList.Descriptions.Thursday },
			{ DayOfWeek.Friday,    AutoDayOfWeekCodeList.Descriptions.Friday },
			{ DayOfWeek.Saturday,  AutoDayOfWeekCodeList.Descriptions.Saturday },
			{ DayOfWeek.Sunday,    AutoDayOfWeekCodeList.Descriptions.Sunday },
		};

		ZString GetRoutingLineInformation(ZDateTime requestedDate, RoutingResponseHeader routingResponseHeader)
		{
			return string.Format(CultureInfo.InvariantCulture,
(NoResString)"Operation is not valid due to the current state of the object.\r\n" +
(NoResString)"Current routing line is:\r\nOrigin: {0}\r\nDestination: {1}\r\nDuration: {2}\r\nDepartureTime: {3}\r\nArrivalTime: {4}\r\nCarrier1: {5}\r\nCarrier2: {6}\r\nCarrier3: {7}\r\nCarrier4: {8}\r\nEffectiveDate: {9}\r\nDiscontinuedDate: {10}\r\nOperationDay: {11}\r\nFlight: {12}\r\nFlightType: {13}\r\nAircraft: {14}\r\nVia: {15}\r\nRequestedDate: {16}\r\n", // Developer Only
				routingResponseHeader.Origin,
				routingResponseHeader.Destination,
				routingResponseHeader.Duration,
				routingResponseHeader.DepartureTime,
				routingResponseHeader.ArrivalTime,
				routingResponseHeader.Carrier1,
				routingResponseHeader.Carrier2,
				routingResponseHeader.Carrier3,
				routingResponseHeader.Carrier4,
				routingResponseHeader.EffectiveDate,
				routingResponseHeader.DiscontinuedDate,
				routingResponseHeader.OperationDay,
				routingResponseHeader.Flight,
				routingResponseHeader.FlightType,
				routingResponseHeader.Aircraft,
				routingResponseHeader.Via,
				requestedDate.ToString()
			);
		}
	}
}
