using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.ZA.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.GUI.Testing
{
	[TestedType(typeof(ZAManifestLayouts))]
	sealed class ZAManifestLayoutsTest : LayoutsAbstractTest
	{
		public void TestZACountrySpecificFieldsVisibility()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Road;
			manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.RFM);

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;
				var dropEditForPlaceOfEntry = asycudaManifestUserControl.Controls.Find("PlaceOfEntryDropEdit", true).FirstOrDefault();
				var dropEditForPlaceOfExit = asycudaManifestUserControl.Controls.Find("PlaceOfExitDropEdit", true).FirstOrDefault();
				var textBoxForTrailer1 = asycudaManifestUserControl.FindSingle<ZTextBox>(nameof(CommonManifestControlBag.Trailer1RegNoTextBox));
				var textBoxForTrailer2 = asycudaManifestUserControl.FindSingle<ZTextBox>(nameof(CommonManifestControlBag.Trailer2RegNoTextBox));
				var zDateEditEstLoad = asycudaManifestUserControl.Controls.Find("EstLoadDateEdit", true).FirstOrDefault();
				var separatorTextUserControl = asycudaManifestUserControl.Controls.Find("SeparatorTextUserControl", true).FirstOrDefault();
				var voyageFlightTextBox = asycudaManifestUserControl.Controls.Find("VoyageFlightTextBox", true).FirstOrDefault();
				var vesselCodeFindBox = asycudaManifestUserControl.Controls.Find("VesselCodeFindBox", true).FirstOrDefault();
				var radioCallSignFindBox = asycudaManifestUserControl.Controls.Find("RadioCallSignCodeFindBox", true).FirstOrDefault();
				var cargoCarrierCodeCodeFindBoxTss = asycudaManifestUserControl.Controls.Find("TssCargoCarrierCodeCodeFindBox", true).FirstOrDefault();
				var dateOfDepartureDateEdit = asycudaManifestUserControl.Controls.Find("DateOfDepartureDateEdit", true).FirstOrDefault();
				var vesselCodeFindBoxTss = asycudaManifestUserControl.Controls.Find("TssVesselCodeFindBox", true).FirstOrDefault();
				var radioCallSignTextBoxTss = asycudaManifestUserControl.Controls.Find("TssRadioCallSignCodeFindBox", true).FirstOrDefault();

				AssertEquals(false, dropEditForPlaceOfEntry.Visible);
				AssertEquals(false, dropEditForPlaceOfExit.Visible);
				AssertEquals(true, textBoxForTrailer1.Visible);
				AssertEquals(true, textBoxForTrailer2.Visible);
				AssertEquals(false, zDateEditEstLoad.Visible);

				AssertEquals(false, vesselCodeFindBoxTss.Visible);
				AssertEquals(false, radioCallSignTextBoxTss.Visible);

				manifest.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
				AssertEquals(false, separatorTextUserControl.Visible);
				AssertEquals(false, voyageFlightTextBox.Visible);
				AssertEquals(false, vesselCodeFindBox.Visible);
				AssertEquals(false, radioCallSignFindBox.Visible);
				AssertEquals(false, cargoCarrierCodeCodeFindBoxTss.Visible);
				AssertEquals(false, dateOfDepartureDateEdit.Visible);

				AssertEquals(true, dropEditForPlaceOfEntry.Visible);
				AssertEquals(true, dropEditForPlaceOfExit.Visible);
				AssertEquals(true, textBoxForTrailer1.Visible);
				AssertEquals(true, textBoxForTrailer2.Visible);
				AssertEquals(false, zDateEditEstLoad.Visible);

				AssertEquals(false, vesselCodeFindBoxTss.Visible);
				AssertEquals(false, radioCallSignTextBoxTss.Visible);

				manifest.AMA_ManifestType = nameof(ManifestDocumentType.ALH);

				AssertEquals(true, dropEditForPlaceOfEntry.Visible);
				AssertEquals(true, dropEditForPlaceOfExit.Visible);
				AssertEquals(true, textBoxForTrailer1.Visible);
				AssertEquals(true, textBoxForTrailer2.Visible);
				AssertEquals(false, zDateEditEstLoad.Visible);

				manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;

				AssertEquals(false, dropEditForPlaceOfEntry.Visible);
				AssertEquals(false, dropEditForPlaceOfExit.Visible);
				AssertEquals(true, textBoxForTrailer1.Visible);
				AssertEquals(true, textBoxForTrailer2.Visible);
				AssertEquals(true, zDateEditEstLoad.Visible);

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				manifest.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
				manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;
				manifest.RefreshBinding();

				AssertEquals(false, dropEditForPlaceOfEntry.Visible);
				AssertEquals(false, dropEditForPlaceOfExit.Visible);
				AssertEquals(false, textBoxForTrailer1.Visible);
				AssertEquals(false, textBoxForTrailer2.Visible);
				AssertEquals(true, zDateEditEstLoad.Visible);

				manifest.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
				AssertEquals(true, separatorTextUserControl.Visible);
				AssertEquals(true, radioCallSignFindBox.Visible);
				AssertEquals(true, cargoCarrierCodeCodeFindBoxTss.Visible);
				AssertEquals(true, dateOfDepartureDateEdit.Visible);

				AssertEquals(true, voyageFlightTextBox.Visible);
				AssertEquals(true, vesselCodeFindBox.Visible);

				AssertEquals(true, vesselCodeFindBoxTss.Visible);
				AssertEquals(true, radioCallSignTextBoxTss.Visible);

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals(false, vesselCodeFindBox.Visible);

				AssertEquals(false, vesselCodeFindBoxTss.Visible);
				AssertEquals(true, radioCallSignTextBoxTss.Visible);
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

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ManifestLayoutBuilder<AsycudaManifestHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.CountryTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.RegistrationDateEdit, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.RegistrationNumberTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ManifestTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.NatureDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.AgentTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.BuyersConsolidationCheckBox, ControlWidthClass.Long);
				yield return (ZAManifestControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Long);
				yield return (ZAManifestControlBag.Instance.RadioCallSignCodeFindBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.VoyageFlightTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.ConveyanceCountryCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VehicleRegistrationTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer1RegNoTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer2RegNoTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
				yield return (ZAManifestControlBag.Instance.EstLoadDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.EstDepartureDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.PortOfFirstArrivalCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.EstArrivalDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.CustomsOfficeDropEdit, ControlWidthClass.Long);
				yield return (ZAManifestControlBag.Instance.CallPurposeCodeDropEdit, ControlWidthClass.Long);
				yield return (ZAManifestControlBag.Instance.DateAtCustomsOfficeDateEdit, ControlWidthClass.Auto);
				yield return (ZAManifestControlBag.Instance.PlaceOfEntryDropEdit, ControlWidthClass.Long);
				yield return (ZAManifestControlBag.Instance.PlaceOfExitDropEdit, ControlWidthClass.Long);
				yield return (ZAManifestControlBag.Instance.SeparatorTextUserControl, ControlWidthClass.LongNoCaption);
				yield return (ZAManifestControlBag.Instance.VoyageFlightTextBox, ControlWidthClass.Medium);
				yield return (ZAManifestControlBag.Instance.TssVesselCodeFindBox, ControlWidthClass.Long);
				yield return (ZAManifestControlBag.Instance.TssRadioCallSignCodeFindBox, ControlWidthClass.Medium);
				yield return (ZAManifestControlBag.Instance.TssCargoCarrierCodeCodeFindBox, ControlWidthClass.Long);
				yield return (ZAManifestControlBag.Instance.DateOfDepartureDateEdit, ControlWidthClass.Medium);
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
				yield return (CommonManifestControlBag.Instance.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.MasterBOLTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.IssueDateDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.CarrierAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CarrierCodeTextBox, ControlWidthClass.Medium);
				yield return (ZAManifestControlBag.Instance.MasterCarrierCodeTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.ShippingAgentAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.DeconsolidateAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.DischargeTerminalAddressControl, ControlWidthClass.Long);
				yield return (ZAManifestControlBag.Instance.CaseNumberManifestHeaderGroupBox, ControlWidthClass.LongNoCaption);
			}
		}
	}
}
