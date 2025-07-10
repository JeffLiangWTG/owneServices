using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Routing.S8.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Routing.S8.GUI.Test
{
	[TestedType(typeof(SingleDaySelectionForm))]
	public class SingleDaySelectionFormTest : ZFormBasherTest
	{
		public void TestSingleDaySelection_NotSelectDepartureDate()
		{
			ZDateTime selectedDepartureDate;

			using (var form = CreateSingleDaySelectionFormForTest())
			{
				form.Show();
				form.Close();
				selectedDepartureDate = form.SelectedDepartureDate;
			}

			AssertEquals(ZDateTime.Empty, selectedDepartureDate);
		}

		public void TestSingleDaySelection_SelectDepartureDate()
		{
			ZDateTime selectedDepartureDate;

			using (var form = CreateSingleDaySelectionFormForTest())
			{
				form.Show();
				form.SelectButton_Exposed.PerformClick();
				selectedDepartureDate = form.SelectedDepartureDate;
			}

			AssertEquals(new ZDateTime(2018, 6, 29), selectedDepartureDate);
		}

		public void TestSingleDaySelection_DialogResult_SelectDay()
		{
			ZDateTime selectedDepartureDate;

			using (var form = CreateSingleDaySelectionFormForTest())
			{
				form.Show();

				var dayOptions = form.DayOptions_Exposed;

				AssertEquals(false, dayOptions[0].RadioButton.Enabled);
				AssertEquals("Thursday (28-Jun-18)", dayOptions[0].RadioButton.Text);
				AssertEquals(new ZDateTime(2018, 6, 28), dayOptions[0].Date);

				AssertEquals(true, dayOptions[1].RadioButton.Enabled);
				AssertEquals("Friday (29-Jun-18)", dayOptions[1].RadioButton.Text);
				AssertEquals(new ZDateTime(2018, 6, 29), dayOptions[1].Date);

				AssertEquals(true, dayOptions[2].RadioButton.Enabled);
				AssertEquals("Saturday (30-Jun-18)", dayOptions[2].RadioButton.Text);
				AssertEquals(new ZDateTime(2018, 6, 30), dayOptions[2].Date);

				AssertEquals(false, dayOptions[3].RadioButton.Enabled);
				AssertEquals("Sunday (01-Jul-18)", dayOptions[3].RadioButton.Text);
				AssertEquals(new ZDateTime(2018, 7, 1), dayOptions[3].Date);

				AssertEquals(true, dayOptions[4].RadioButton.Enabled);
				AssertEquals("Monday (02-Jul-18)", dayOptions[4].RadioButton.Text);
				AssertEquals(new ZDateTime(2018, 7, 2), dayOptions[4].Date);

				AssertEquals(false, dayOptions[5].RadioButton.Enabled);
				AssertEquals("Tuesday (03-Jul-18)", dayOptions[5].RadioButton.Text);
				AssertEquals(new ZDateTime(2018, 7, 3), dayOptions[5].Date);

				AssertEquals(false, dayOptions[6].RadioButton.Enabled);
				AssertEquals("Wednesday (04-Jul-18)", dayOptions[6].RadioButton.Text);
				AssertEquals(new ZDateTime(2018, 7, 4), dayOptions[6].Date);

				dayOptions[4].RadioButton.PerformClick();
				form.SelectButton_Exposed.PerformClick();

				selectedDepartureDate = form.SelectedDepartureDate;
			}

			AssertEquals(new ZDateTime(2018, 7, 2), selectedDepartureDate);
		}

		public void TestSingleDaySelection_ExceptionContainsRoutingLineInformation()
		{
			ErrorReporter.Instance.Clear();

			var routingLineInformation = @"Operation is not valid due to the current state of the object.
Current routing line is:
Origin: SYD
Destination: WUH
Duration: 10:55
DepartureTime: 11:20
ArrivalTime: 20:15
Carrier1: MU
Carrier2: 
Carrier3: 
Carrier4: 
EffectiveDate: 21-Jun-18 00:00:00
DiscontinuedDate: 06-Oct-18 00:00:00
OperationDay: _______
Flight: MU750
FlightType: PAS
Aircraft: 332
Via: Direct
RequestedDate: 28-Jun-18 00:00:00
";
			using (_ = CreateSingleDaySelectionFormForTestWithAllOperationDaysOff())
			{
				AssertEquals(routingLineInformation, ErrorReporter.LastMessageReported);
			}

			ErrorReporter.Instance.Clear();
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return CreateSingleDaySelectionFormForTest();
		}

		RealTimeRoutingFormForTest CreateSingleDaySelectionFormForTest()
		{
			var messageLine = "000 SYD WUH 10:55   11:20   20:15 MU           2018/06/21 2018/10/06 1...56. <SYD 1 WUH     11:20   20:15 MU   750    332     0  5063                                        1..4.6. 18/06/21 18/10/06 J> ";
			var header = new RoutingResponseHeader(messageLine, Factory);
			return new RealTimeRoutingFormForTest(new ZDateTime(2018, 6, 28), header);
		}

		RealTimeRoutingFormForTest CreateSingleDaySelectionFormForTestWithAllOperationDaysOff()
		{
			var messageLine = "000 SYD WUH 10:55   11:20   20:15 MU           2018/06/21 2018/10/06 ....... <SYD 1 WUH     11:20   20:15 MU   750    332     0  5063                                        1..4.6. 18/06/21 18/10/06 J> ";
			var header = new RoutingResponseHeader(messageLine, Factory);
			return new RealTimeRoutingFormForTest(new ZDateTime(2018, 6, 28), header);
		}

		class RealTimeRoutingFormForTest : SingleDaySelectionForm
		{
			public RealTimeRoutingFormForTest(ZDateTime requestedDate, RoutingResponseHeader routingResponseHeader)
				: base(requestedDate, routingResponseHeader)
			{
			}

			public List<DayOption> DayOptions_Exposed => base.DayOptions;

			public ZButton SelectButton_Exposed => SelectButton;
		}

		#endregion
	}
}
