using System.Collections.Generic;
using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TR.Manifest.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Balloons;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.GUI.Testing
{
	[TestedType(typeof(TRManifestLayouts))]
	sealed class TRManifestLayoutsTest : LayoutsAbstractTest
	{
		public void TestTIRNumberTextBox()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var numberTextBox = asycudaManifestUserControl.FindSingle<ZTextBox>(nameof(TRManifestControlBag.TIRNumberTextBox));
				CombineAssertions(() =>
				{
					manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
					manifest.AMA_ManifestType = TRManifestTypes.Codes.GRUPAJ;
					AssertEquals("Groupage Bill No", numberTextBox.GetExtension<LabelCaptionRenderer>().Caption);
					manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
					AssertEquals("Groupage Bill No", numberTextBox.GetExtension<LabelCaptionRenderer>().Caption);
					manifest.AMA_TransportMode = Core.Constants.TransportModes.Road;
					AssertEquals("TIR/ATA Carnet No", numberTextBox.GetExtension<LabelCaptionRenderer>().Caption);
					manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
					manifest.AMA_ManifestType = TRManifestTypes.Codes.ATAIHR;
					AssertEquals("TIR/ATA Carnet No", numberTextBox.GetExtension<LabelCaptionRenderer>().Caption);
				});
			}
		}

		public void TestTRCountrySpecificFieldsVisibility()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;
				var lloydsNumber = asycudaManifestUserControl.FindSingle<ZTextBox>(nameof(CommonManifestControlBag.LloydsNumberTextBox));
				var vesselNameTextBox = asycudaManifestUserControl.Controls.Find("VesselNameTextBox", true).FirstOrDefault();
				var conveyanceCountryCodeFindBox = asycudaManifestUserControl.Controls.Find("ConveyanceCountryCodeFindBox", true).FirstOrDefault();

				CombineAssertions("TRCountrySpecificFieldsVisibility", () =>
				{
					manifest.AMA_TransportMode = "ROA";
					AssertEquals("LloydsNumberTextBox visible the ROA", false, lloydsNumber.Visible);
					AssertEquals("VesselNameTextBox visible the ROA", false, vesselNameTextBox.Visible);
					AssertEquals("ConveyanceCountryCodeFindBox visible the ROA", false, conveyanceCountryCodeFindBox.Visible);
					manifest.AMA_TransportMode = "SEA";
					AssertEquals("LloydsNumberTextBox visible the SEA", true, lloydsNumber.Visible);
					AssertEquals("VesselNameTextBox visible the SEA", false, vesselNameTextBox.Visible);
					AssertEquals("ConveyanceCountryCodeFindBox visible the SEA", true, conveyanceCountryCodeFindBox.Visible);
					manifest.AMA_TransportMode = "AIR";
					AssertEquals("LloydsNumberTextBox not visible the AIR", false, lloydsNumber.Visible);
					AssertEquals("VesselNameTextBox visible the AIR", true, vesselNameTextBox.Visible);
					AssertEquals("ConveyanceCountryCodeFindBox visible the AIR", true, conveyanceCountryCodeFindBox.Visible);
					manifest.AMA_ManifestType = TRManifestTypes.Codes.HAVITH;
					AssertEquals("LloydsNumberTextBox visible for AIR and HAVITH", true, lloydsNumber.Visible);
					manifest.AMA_ManifestType = TRManifestTypes.Codes.VARONC;
					AssertEquals("LloydsNumberTextBox visible for AIR and VARONC", true, lloydsNumber.Visible);
				});

				var messagesTabPage = asycudaManifestUserControl.FindSingle<ZTabPage>("mainTabControl_TabPage_MessagesUserControl");
				mainTabControl.SelectedTab = messagesTabPage;
				var messagesGrid = asycudaManifestUserControl.Controls.Find("messagesGrid", true).First() as ZGrid;
				manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;

				Assert("EM_CreateUserFullName is visible", messagesGrid.GetColumnStyle("EM_CreateUserFullName").IsVisible);
			}
		}

		public void TestLloydsNumberTextBoxCaption()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = "SEA";
			manifest.AMA_ManifestType = TRManifestTypes.Codes.ATAIHR;
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var lloydsNumber = asycudaManifestUserControl.FindSingle<ZTextBox>(nameof(CommonManifestControlBag.LloydsNumberTextBox));
				AssertEquals("Caption of LloydsNumberTextBox for the SEA", "Vessel IMO Number", lloydsNumber.GetExtension<LabelCaptionRenderer>().Caption);
				manifest.AMA_TransportMode = "AIR";
				manifest.AMA_ManifestType = TRManifestTypes.Codes.HAVITH;
				manifest.RefreshBinding();
				AssertEquals("Caption of LloydsNumberTextBox for AIR and HAVITH", "Reference No", lloydsNumber.GetExtension<LabelCaptionRenderer>().Caption);
				manifest.AMA_ManifestType = TRManifestTypes.Codes.VARONC;
				manifest.RefreshBinding();
				AssertEquals("Caption of LloydsNumberTextBox for AIR and VARONC", "Reference No", lloydsNumber.GetExtension<LabelCaptionRenderer>().Caption);
				manifest.AMA_ManifestType = TRManifestTypes.Codes.GRUPAJ;
				manifest.RefreshBinding();
				AssertEquals("Caption of LloydsNumberTextBox for AIR and GRUPAJ", "Reference No", lloydsNumber.GetExtension<LabelCaptionRenderer>().Caption);
			}
		}

		public void TestLloydsNumberTextBoxBalloonCaptionIsResourceNo()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = "SEA";
			manifest.AMA_ManifestType = TRManifestTypes.Codes.DEMITH;
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var lloydsNumber = asycudaManifestUserControl.FindSingle<ZTextBox>(nameof(CommonManifestControlBag.LloydsNumberTextBox));
				var balloon = Balloon.Instance;
				balloon.Show(lloydsNumber);
				AssertEquals("Caption of LloydsNumberTextBox's Balloon for the SEA and DEMITH", "Vessel IMO Number", balloon.BalloonWindowExposedForTesting.Descriptor.Caption);
				manifest.AMA_TransportMode = "AIR";
				manifest.AMA_ManifestType = TRManifestTypes.Codes.HAVITH;
				manifest.RefreshBinding();
				Balloon.Instance.Show(lloydsNumber);
				AssertEquals("Caption of LloydsNumberTextBox's Balloon for the AIR and HAVITH", "Reference No", balloon.BalloonWindowExposedForTesting.Descriptor.Caption);
				balloon.Hide();
			}
		}

		public void TestDateAtCustomsOfficeDateEdit()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;
				var dateAtCustomsOffice = asycudaManifestUserControl.FindSingle<ZDateEdit>(nameof(CommonManifestControlBag.DateAtCustomsOfficeDateEdit));
				manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;
				manifest.RefreshBinding();
				AssertEquals("Departure Date", dateAtCustomsOffice.GetExtension<LabelCaptionRenderer>().Caption);
				manifest.AMA_Nature = ShipmentTypeList.Codes.Import23;
				manifest.RefreshBinding();
				AssertEquals("Arrival Date", dateAtCustomsOffice.GetExtension<LabelCaptionRenderer>().Caption);
			}
		}

		public void TestCustomsPortVisibility()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;
				var dischargePort = asycudaManifestUserControl.FindSingle<ZCodeFindBox>(nameof(CommonManifestControlBag.CustomsDischargePortCodeFindBox));
				var loadPort = asycudaManifestUserControl.FindSingle<ZCodeFindBox>(nameof(CommonManifestControlBag.CustomsLoadPortCodeFindBox));
				manifest.AMA_TransportMode = "SEA";
				AssertEquals(true, dischargePort.Visible);
				AssertEquals(true, loadPort.Visible);
				manifest.AMA_TransportMode = "AIR";
				AssertEquals(false, dischargePort.Visible);
				AssertEquals(false, loadPort.Visible);
			}
		}

		public void TestVisibilityDutyTaxFields()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;
				var globalManifestStampDutyValue = asycudaManifestUserControl.Controls.Find("GlobalManifestStampDutyValueCalcEdit", true).FirstOrDefault();
				var masterBillStampDutyValue = asycudaManifestUserControl.Controls.Find("MasterBillStampDutyValueCalcEdit", true).FirstOrDefault();
				var totalStampDutyValue = asycudaManifestUserControl.Controls.Find("TotalStampDutyValueCalcEdit", true).FirstOrDefault();
				AssertEquals(true, globalManifestStampDutyValue.Visible);
				AssertEquals(true, masterBillStampDutyValue.Visible);
				AssertEquals(true, totalStampDutyValue.Visible);
			}
		}

		public void TestLloydsNumberTextBoxVisibility()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;
				var lloydsNumber = asycudaManifestUserControl.FindSingle<ZTextBox>(nameof(CommonManifestControlBag.LloydsNumberTextBox));
				manifest.AMA_TransportMode = "SEA";
				AssertEquals(true, lloydsNumber.Visible);
				manifest.AMA_TransportMode = "AIR";
				manifest.AMA_ManifestType = TRManifestTypes.Codes.HAVITH;
				AssertEquals(true, lloydsNumber.Visible);
				manifest.AMA_TransportMode = "AIR";
				manifest.AMA_ManifestType = TRManifestTypes.Codes.GRUPAJ;
				AssertEquals(true, lloydsNumber.Visible);
				manifest.AMA_TransportMode = "AIR";
				manifest.AMA_ManifestType = TRManifestTypes.Codes.VARONC;
				AssertEquals(true, lloydsNumber.Visible);
				manifest.AMA_TransportMode = "ROA";
				AssertEquals(false, lloydsNumber.Visible);
			}
		}

		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.CountryTextBox, ControlWidthClass.Medium);
				yield return (TRManifestControlBag.Instance.RegistrationDateEdit, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.RegistrationNumberTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (TRManifestControlBag.Instance.TransportTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ManifestTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.NatureDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.AgentTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.BuyersConsolidationCheckBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VesselNameTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.LloydsNumberTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VoyageFlightTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.RadioCallSignTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.ConveyanceCountryCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.MastersNameTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VehicleRegistrationTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer1RegNoTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer1RegCountryCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.Trailer2RegNoTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer2RegCountryCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CustomsLoadPortCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.EstDepartureDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.PortOfFirstArrivalCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CustomsDischargePortCodeFindBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.JobReferenceTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.MessageStatusTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.MessageStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CustomsStatusDropEdit, ControlWidthClass.Long);
				yield return (TRManifestControlBag.Instance.PresentationCustomsOfficeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.MasterBOLTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.IssueDateDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.CarrierAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CarrierCodeTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.ShippingAgentAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.DeconsolidateAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.EstArrivalDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.CustomsOfficeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.DateAtCustomsOfficeDateEdit, ControlWidthClass.Auto);
				yield return (TRManifestControlBag.Instance.ManifestDescriptionTextBox, ControlWidthClass.Long);
				yield return (TRManifestControlBag.Instance.TIRNumberTextBox, ControlWidthClass.Long);
				yield return (TRManifestControlBag.Instance.InspectionClerkTextBox, ControlWidthClass.Long);
				yield return (TRManifestControlBag.Instance.InternalInspectionNoTextBox, ControlWidthClass.Long);
				yield return (TRManifestControlBag.Instance.TempStorageStartDateEdit, ControlWidthClass.Auto);
				yield return (TRManifestControlBag.Instance.TempStorageDueDateEdit, ControlWidthClass.Auto);
				yield return (TRManifestControlBag.Instance.GlobalManifestStampDutyValueCalcEdit, ControlWidthClass.Long);
				yield return (TRManifestControlBag.Instance.MasterBillStampDutyValueCalcEdit, ControlWidthClass.Long);
				yield return (TRManifestControlBag.Instance.TotalStampDutyValueCalcEdit, ControlWidthClass.Long);
			}
		}

		public void TestManifestNumberFromMasterBillCaption()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var manifestNumberFromMasterBillTextBox = asycudaManifestUserControl.FindSingle<ZTextBox>(nameof(CommonManifestControlBag.ManifestNumberFromMasterBillTextBox));
				CombineAssertions(() =>
				{
					manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
					manifest.AMA_ManifestType = TRManifestTypes.Codes.GRUPAJ;
					AssertEquals("Previous Dec. No", manifestNumberFromMasterBillTextBox.GetExtension<LabelCaptionRenderer>().Caption);
					manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
					AssertEquals("BOL", manifestNumberFromMasterBillTextBox.GetExtension<LabelCaptionRenderer>().Caption);
					manifest.AMA_TransportMode = Core.Constants.TransportModes.Road;
					AssertEquals("BOL", manifestNumberFromMasterBillTextBox.GetExtension<LabelCaptionRenderer>().Caption);
					manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
					manifest.AMA_ManifestType = TRManifestTypes.Codes.ATAIHR;
					AssertEquals("BOL", manifestNumberFromMasterBillTextBox.GetExtension<LabelCaptionRenderer>().Caption);
				});
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ManifestLayoutBuilder<AsycudaManifestHeader>();
	}
}
