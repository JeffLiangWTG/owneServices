using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.GUI.Testing
{
	[TestedType(typeof(DepartureSelectionDialog))]
	class DepartureSelectionDialogTest : ZFormBasherTest
	{
		[TestDate(2020, 07, 24, 07, 30, 00, 00)]
		public void TestDepartureTimeSelectionDialog()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_RL_NKPortOfLoading = "NZAKL";
			header.AMA_RL_NKPortOfDischarge = "USCHI";
			header.AMA_ManifestType = "IAM";
			header.AMA_Voyage = "KLM325";
			header.AMA_E_DEP = new ZDateTime(2020, 07, 24, 00, 00, 00, 00);
			header.AMA_E_ARV = new ZDateTime(2020, 07, 25, 06, 30, 00, 00);
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			using (var form = new DepartureSelectionDialog(additionalMessageInformation))
			{
				form.Show();
				AssertEquals("Lift Off Time", form.Text);
				var datePicker = form.FindSingle<ZDateEdit>("DatePicker");
				AssertEquals(ZDateTimePickerFormat.Long, datePicker.DateTimeFormat);
				var timezoneLabel = form.FindSingle<ZArchitecture.ZLabel>("TimeZoneLabel");
				AssertEquals("in the Time Zone of the load port NZAKL", timezoneLabel.Text);
			}
		}

		public void TestSingleSelect()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			arrivalHeader.ATH_VoyageFlightNo = "UA101";
			arrivalHeader.ATH_ETAAtDischargePort = ZDate.Today;
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			additionalMessageInformation.FlightArrivalDetails.AddNew();
			using (var form = new DepartureSelectionDialog(additionalMessageInformation))
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

		protected override Form GetFormToBashCore()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return new DepartureSelectionDialog(new AdditionalMessageInformation(header));
		}
	}
}
