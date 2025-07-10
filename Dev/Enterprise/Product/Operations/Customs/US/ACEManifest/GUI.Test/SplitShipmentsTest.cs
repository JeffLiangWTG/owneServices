using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.ACEManifest.GUI.Testing
{
	[TestedType(typeof(SplitShipments))]
	class SplitShipmentsTest : ZFormBasherTest
	{
		public void TestOKButtonState()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new SplitShipments(new AdditionalMessageInformation(header), AIMMessageSubTypes.FRI))
			{
				form.Show();
				AssertEquals(true, form.OKButton.Enabled);
			}
		}

		public void TestCancel_ButtonClick()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new SplitShipments(new AdditionalMessageInformation(header), AIMMessageSubTypes.FRI))
			{
				form.Show();
				form.Cancel_Button.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		public void TestVisibility()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new SplitShipments(new AdditionalMessageInformation(header), AIMMessageSubTypes.FRI))
			{
				form.Show();
				AssertEquals("IsSplitShipmentCheckBox", true, form.IsSplitShipmentCheckBox.Visible);
				AssertEquals("BoardedQtyCalcEdit", true, form.BoardedQtyCalcEdit.Visible);
				AssertEquals("BoardedWeightCalcEdit", true, form.BoardedWeightCalcEdit.Visible);
				AssertEquals("BoardedWeightUQ", true, form.BoardedWeightUQ.Visible);
			}

			using (var form = new SplitShipments(new AdditionalMessageInformation(header), AIMMessageSubTypes.FRX))
			{
				form.Show();
				AssertEquals("IsSplitShipmentCheckBox", false, form.IsSplitShipmentCheckBox.Visible);
				AssertEquals("BoardedQtyCalcEdit", false, form.BoardedQtyCalcEdit.Visible);
				AssertEquals("BoardedWeightCalcEdit", false, form.BoardedWeightCalcEdit.Visible);
				AssertEquals("BoardedWeightUQ", false, form.BoardedWeightUQ.Visible);
			}

			using (var form = new SplitShipments(new AdditionalMessageInformation(header), AIMMessageSubTypes.FXX))
			{
				form.Show();
				AssertEquals("IsSplitShipmentCheckBox", false, form.IsSplitShipmentCheckBox.Visible);
				AssertEquals("BoardedQtyCalcEdit", false, form.BoardedQtyCalcEdit.Visible);
				AssertEquals("BoardedWeightCalcEdit", false, form.BoardedWeightCalcEdit.Visible);
				AssertEquals("BoardedWeightUQ", false, form.BoardedWeightUQ.Visible);
			}
		}

		public void TestOKButtonClickNeedsSelectedFlightRow()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			additionalMessageInformation.FlightArrivalDetails.Cast<FlightDetail>().ForEach(x => x.Selected = false);
			using (var form = new SplitShipments(additionalMessageInformation, AIMMessageSubTypes.FRI))
			{
				form.Show();
				form.OKButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals("Information Please select the required Flight information from the Flight Details grid first, in order to proceed.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestOKButtonClick()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			arrivalHeader.ATH_VoyageFlightNo = "UA101";
			arrivalHeader.ATH_ETAAtDischargePort = ZDate.Today;
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			additionalMessageInformation.AM_FlightNo = "UA101";
			additionalMessageInformation.AM_FlightArrivalDate = ZDate.Today;
			AssertEquals("Pre-condition", 1, additionalMessageInformation.FlightArrivalDetails.Count);
			using (var form = new SplitShipments(additionalMessageInformation, AIMMessageSubTypes.FRI))
			{
				form.Show();
				form.FlightDetailsGrid.Select(0);
				form.OKButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return new SplitShipments(new AdditionalMessageInformation(header), AIMMessageSubTypes.FRI);
		}
	}
}
