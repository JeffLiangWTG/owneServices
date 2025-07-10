using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(OutturnAndGateInOutMainUserControl))]
	sealed class OutturnAndGateInOutMainUserControlTest : TestCaseWithFactory
	{
		public void TestzCodeFindBoxVesselPopupSelected()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "VESSEL";
			vessel.RV_LloydsNumber = "9832343";
			vessel.RV_RadioCallSign = "CALLME";
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Australia;
			Factory.Save();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			using (var form = new OutturnAndGateInOutForm(header))
			using (var control = new OutturnAndGateInOutMainUserControl())
			{
				control.ManifestHeader = header;
				form.Controls.Add(control);
				form.Show();
				var vesselCodeFindBox = control.FindSingle<ZCodeFindBox>("zCodeFindBoxVessel");
				((IFindBox)vesselCodeFindBox).Code = "VESSEL";
				vesselCodeFindBox.SelectFromPopupForm(true);
				using (((IFindBox)vesselCodeFindBox).PopupForm)
				{
					CombineAssertions(() =>
					{
						AssertEquals("AMA_VesselName", "VESSEL", header.AMA_VesselName);
						AssertEquals("AMA_LloydsNumber", "9832343", header.AMA_LloydsNumber);
						AssertEquals("AMA_RadioCallSign", "CALLME", header.AMA_RadioCallSign);
						AssertEquals("AMA_RN_NKConveyanceNationality", Core.Constants.CountryCodes.Australia, header.AMA_RN_NKConveyanceNationality);
					});
				}
			}
		}

		public void TestVoyageFlightNoLabel()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			using (var form = new OutturnAndGateInOutForm(header))
			{
				using (var control = new OutturnAndGateInOutMainUserControl())
				{
					control.ManifestHeader = header;
					form.Controls.Add(control);
					form.Show();
					header.AMA_TransportMode = ZString.Empty;
					AssertEquals("Flight/Voyage", control.zTextBoxVoyage.GetExtension<ILabelCaptionRenderer>().Caption);
					header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
					AssertEquals("Voyage", control.zTextBoxVoyage.GetExtension<ILabelCaptionRenderer>().Caption);
					header.AMA_TransportMode = Core.Constants.TransportModes.Air;
					AssertEquals("Flight", control.zTextBoxVoyage.GetExtension<ILabelCaptionRenderer>().Caption);
				}
			}
		}

		public void TestCargoReportingInstructionsGroupBox()
		{
			var expectedControls = new[]
			{
				"Message Sender/Outturn Provider",
				"Customs Office",
				"Depot",
				"Terminal",
				"COSTCO Type",
				"Date Time Fully Loaded/Unloaded",
				"Date Time Unpacked",
				"Excess/Short Indicator",
				"Inturn/Outturn Status",
				"GOVGIO Type",
				"Gate In/Out Date Time",
				"Gate In/Out Status",
			};

			var header = Factory.New<AsycudaManifestHeader>();
			using var form = new OutturnAndGateInOutForm(header);
			using var control = new OutturnAndGateInOutMainUserControl();

			control.ManifestHeader = header;
			form.Controls.Add(control);
			form.Show();
			var groupBox = control.FindSingle<ZGroupBox>("zGroupBoxCargoReportingInstructions");

			AssertEquals("Caption", "Cargo Reporting Instructions", groupBox.GetExtension<ILabelCaptionRenderer>().Caption);
			AssertContainsExactElementsInExactOrder("Controls", expectedControls, groupBox.Controls.Cast<Control>().OrderBy(c => c.TabIndex).Select(c => c.GetExtension<ILabelCaptionRenderer>().Caption));

			foreach (var c in groupBox.Controls.Cast<Control>())
			{
				AssertEquals($"{c.Name} visible", expected: true, c.Visible);
			}
		}

		public void TestTransportDocumentDetailsGroupBox()
		{
			var expectedControls = new[]
			{
				"Transport Mode",
				"Agent Type",
				"Nature",
				"Carrier",
				"Vessel",
				"Flight/Voyage",
				"Port of Loading",
				"Estimated Departure Time",
				"Port of Discharge",
				"Arrival Date",
				"Booking Number",
				"Master Bill",
				"Issue Date",
				"Parent Bill",
				"Container Mode",
			};

			var header = Factory.New<AsycudaManifestHeader>();
			using var form = new OutturnAndGateInOutForm(header);
			using var control = new OutturnAndGateInOutMainUserControl();

			control.ManifestHeader = header;
			form.Controls.Add(control);
			form.Show();
			var groupBox = control.FindSingle<ZGroupBox>("zGroupBoxTransportDocumentDetails");

			AssertEquals("Caption", "Transport Document Details", groupBox.GetExtension<ILabelCaptionRenderer>().Caption);
			AssertContainsExactElementsInExactOrder("Controls", expectedControls, groupBox.Controls.Cast<Control>().OrderBy(c => c.TabIndex).Select(c => c.GetExtension<ILabelCaptionRenderer>().Caption));

			foreach (var c in groupBox.Controls.Cast<Control>())
			{
				AssertEquals($"{c.Name} visible", expected: true, c.Visible);
			}
		}

		public void TestCOSTCOControlsEnabled()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			using var form = new OutturnAndGateInOutForm(header);
			using var control = new OutturnAndGateInOutMainUserControl();

			control.ManifestHeader = header;
			form.Controls.Add(control);
			form.Show();

			AssertNullOrEmpty("PRE-CONDITION", header.GateInOutMessageType);
			AssertCOSTCOControlsEnabled(true);

			header.GateInOutMessageType = "DGI";
			AssertCOSTCOControlsEnabled(false);

			header.GateInOutMessageType = ZString.Empty;
			AssertCOSTCOControlsEnabled(true);

			void AssertCOSTCOControlsEnabled(bool enabled)
			{
				var controls = new[] { "zDropEditWithFixedWidthManifestType", "zDateEditFullyLoadedUnloadedDate", "zDropEditWithFixedWidthExcessIndicator", "zDateEditUnpacked" };
				foreach (var c in controls)
				{
					AssertEquals($"{c} enabled: {enabled}", enabled, control.FindSingle<Control>(c).Enabled);
				}
			}
		}

		public void TestGOVGIOControlsEnabled()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			using var form = new OutturnAndGateInOutForm(header);
			using var control = new OutturnAndGateInOutMainUserControl();

			control.ManifestHeader = header;
			form.Controls.Add(control);
			form.Show();

			AssertNullOrEmpty("PRE-CONDITION", header.AMA_ManifestType);
			AssertGOVGIOControlsEnabled(true);

			header.AMA_ManifestType = "AOR";
			AssertGOVGIOControlsEnabled(false);

			header.AMA_ManifestType = ZString.Empty;
			AssertGOVGIOControlsEnabled(true);

			void AssertGOVGIOControlsEnabled(bool enabled)
			{
				var controls = new[] { "zDropGateInOutMessageType", "zDateEditGateInOutDate", "GateInOutStatusDropEdit" };
				foreach (var c in controls)
				{
					AssertEquals($"{c} enabled: {enabled}", enabled, control.FindSingle<Control>(c).Enabled);
				}
			}
		}
	}
}
