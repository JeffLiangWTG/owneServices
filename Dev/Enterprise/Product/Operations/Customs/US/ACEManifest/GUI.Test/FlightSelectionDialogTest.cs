using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.GUI.Testing
{
	[TestedType(typeof(FlightSelectionDialog))]
	class FlightSelectionDialogTest : ZFormBasherTest
	{
		public void TestOKButtonClick()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			arrivalHeader.ATH_VoyageFlightNo = "UA101";
			arrivalHeader.ATH_ETAAtDischargePort = ZDate.Today;
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			AssertEquals("Pre-condition", 1, additionalMessageInformation.FlightArrivalDetails.Count);
			using (var form = new FlightSelectionDialog(additionalMessageInformation, "Arrivals"))
			{
				form.Show();
				form.FlightDetailsGrid.Select(0);
				form.OKButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestOKButtonClick_BusinessEntityHasErrors()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			arrivalHeader.ATH_VoyageFlightNo = "UA101";
			arrivalHeader.ATH_ETAAtDischargePort = ZDate.Today;
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			additionalMessageInformation.FlightArrivalDetails.AddNew();
			using (var form = new FlightSelectionDialog(additionalMessageInformation, "Arrivals"))
			{
				form.Show();
				var flights = additionalMessageInformation.FlightArrivalDetails;
				AssertEquals(2, flights.Count);
				flights[0].Selected = true;
				flights[1].Selected = true;
				AssertHasErrors(flights[1].SelectedInfo);
				form.OKButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals("Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestOKButtonClick_NeedsFlightSelected()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			arrivalHeader.ATH_VoyageFlightNo = "UA101";
			arrivalHeader.ATH_ETAAtDischargePort = ZDate.Today;
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			AssertEquals("Pre-condition", 1, additionalMessageInformation.FlightArrivalDetails.Count);
			additionalMessageInformation.FlightArrivalDetails[0].Selected = false;
			using (var form = new FlightSelectionDialog(additionalMessageInformation, "Arrivals"))
			{
				form.Show();
				form.OKButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals("Information Please select the required Flight information from the Flight Details grid first, in order to proceed.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestCancelButtonClick()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new FlightSelectionDialog(new AdditionalMessageInformation(header), "Arrivals"))
			{
				form.Show();
				form.Cancel_Button.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return new FlightSelectionDialog(new AdditionalMessageInformation(header), "Arrivals");
		}
	}
}
