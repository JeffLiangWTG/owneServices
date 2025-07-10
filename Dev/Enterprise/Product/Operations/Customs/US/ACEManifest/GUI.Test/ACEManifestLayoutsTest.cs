using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.GUI.Testing
{
	[TestedType(typeof(ACEManifestLayouts))]
	sealed class ACEManifestLayoutsTest : LayoutsAbstractTest
	{
		public void TestVisibility()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>("mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;
				var manifestGroupBox = mainTabPage.FindSingle<ZGroupBox>("ManifestGroupBox");
				var commonBag = new ManifestLayoutBuilder<AsycudaManifestHeader>().CommonBag;
				Assert("TransportModeDropEdit Visible", manifestGroupBox.FindSingle<Control>(commonBag.TransportModeDropEdit.Name).Visible);
				Assert("PortOfFirstArrival Visible", manifestGroupBox.FindSingle<Control>(commonBag.PortOfFirstArrivalCodeFindBox.Name).Visible);
				Assert("PortOfDischargeCodeFindBox Visible", manifestGroupBox.FindSingle<Control>(commonBag.PortOfDischargeCodeFindBox.Name).Visible);
				Assert("DeconsolidateAddressControl Visible", manifestGroupBox.FindSingle<Control>(commonBag.DeconsolidateAddressControl.Name).Visible);
				AssertNull("CustomsDischargePort", manifestGroupBox.FindSingleOrDefault<Control>(commonBag.CustomsDischargePortCodeFindBox.Name));
				AssertNull("CustomsLoadPort", manifestGroupBox.FindSingleOrDefault<Control>(commonBag.CustomsLoadPortCodeFindBox.Name));
				AssertNull("DateAtCustomsOfficeDateEdit", manifestGroupBox.FindSingleOrDefault<Control>(commonBag.DateAtCustomsOfficeDateEdit.Name));
				AssertNull("LloydsNumberTextBox", manifestGroupBox.FindSingleOrDefault<Control>(commonBag.LloydsNumberTextBox.Name));
				AssertNull("CustomsOffice", manifestGroupBox.FindSingleOrDefault<Control>(commonBag.CustomsOfficeDropEdit.Name));
				AssertNull("DischargeTerminalAddressControl", manifestGroupBox.FindSingleOrDefault<Control>(commonBag.DischargeTerminalAddressControl.Name));
				AssertNull("EstArrivalDateEdit", manifestGroupBox.FindSingleOrDefault<Control>(commonBag.EstArrivalDateEdit.Name));
				AssertNull("AgentTypeDropEdit", manifestGroupBox.FindSingleOrDefault<Control>(commonBag.AgentTypeDropEdit.Name));
				AssertNull("ShippingAgentAddress", manifestGroupBox.FindSingleOrDefault<Control>(commonBag.ShippingAgentAddressControl.Name));
				AssertNull("RegistrationDateEdit", manifestGroupBox.FindSingleOrDefault<Control>(commonBag.RegistrationDateEdit.Name));
				AssertNull("RegistrationNumberTextBox", manifestGroupBox.FindSingleOrDefault<Control>(commonBag.RegistrationNumberTextBox.Name));
				AssertNull("CustomsStatusDropEdit", manifestGroupBox.FindSingleOrDefault<Control>(commonBag.CustomsStatusDropEdit.Name));
				AssertNull("MessageStatusTextBox", manifestGroupBox.FindSingleOrDefault<Control>(commonBag.MessageStatusTextBox.Name));
				AssertNull("MessageStatusDropEdit", manifestGroupBox.FindSingleOrDefault<Control>(commonBag.MessageStatusDropEdit.Name));
				var aceBag = ACEManifestControlBag.Instance;
				AssertNotNull("EstDateAtFirstArrivalDateEdit", manifestGroupBox.FindSingleOrDefault<Control>(aceBag.EstDateAtFirstArrivalDateEdit.Name));
				AssertNotNull("BillStatusTextBox", manifestGroupBox.FindSingleOrDefault<Control>(aceBag.BillStatusTextBox.Name));
				AssertNotNull("BillStatusDescriptionTextBox", manifestGroupBox.FindSingleOrDefault<Control>(aceBag.BillStatusDescriptionTextBox.Name));
				AssertNotNull("EstArrivalDateEditShort", manifestGroupBox.FindSingleOrDefault<Control>(commonBag.ShortEstArrivalDateEdit.Name));
				AssertNotNull("ExpressCourierCheckBox", manifestGroupBox.FindSingleOrDefault<Control>(aceBag.ExpressCourierCheckBox.Name));
				AssertNotNull("FIRMSTextBox", manifestGroupBox.FindSingleOrDefault<Control>(aceBag.FIRMSTextBox.Name));
			}
		}

		public void TestCaptions()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>("mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;
				var manifestGroupBox = mainTabPage.FindSingle<ZGroupBox>("ManifestGroupBox");
				var commonBag = new ManifestLayoutBuilder<AsycudaManifestHeader>().CommonBag;
				var portOfFirstArrivalFindBoxHints = manifestGroupBox.FindSingle<Control>(commonBag.PortOfFirstArrivalCodeFindBox.Name).GetExtension<IHintExtension>();
				AssertEquals("Port of First Arrival", portOfFirstArrivalFindBoxHints.Caption);
				AssertEquals("First Airport of Arrival in the United States", portOfFirstArrivalFindBoxHints.Description);
				var deconsolidateAddressControl = manifestGroupBox.FindSingle<Control>(commonBag.DeconsolidateAddressControl.Name).GetExtension<IHintExtension>();
				AssertEquals("CFS Address", deconsolidateAddressControl.Caption);
				var portOfDischargeFindBoxHints = manifestGroupBox.FindSingle<Control>(commonBag.PortOfDischargeCodeFindBox.Name).GetExtension<IHintExtension>();
				AssertEquals("Discharge Port", portOfDischargeFindBoxHints.Caption);
				AssertEquals("Discharge Port (PTP port if different to Port of First Arrival)", portOfDischargeFindBoxHints.Description);
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
				yield return (CommonManifestControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ManifestTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.NatureDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.BuyersConsolidationCheckBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Long);
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
				yield return (CommonManifestControlBag.Instance.PortOfFirstArrivalCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.DeconsolidateAddressControl, ControlWidthClass.Long);
				yield return (ACEManifestControlBag.Instance.FIRMSTextBox, ControlWidthClass.Medium);
				yield return (ACEManifestControlBag.Instance.ExpressCourierCheckBox, ControlWidthClass.Medium);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.JobReferenceTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.MasterBOLTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.IssueDateDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.CarrierAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CarrierCodeTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.EstDepartureDateEdit, ControlWidthClass.Auto);
				yield return (ACEManifestControlBag.Instance.EstDateAtFirstArrivalDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.ShortEstArrivalDateEdit, ControlWidthClass.Auto);
				yield return (ACEManifestControlBag.Instance.BillStatusTextBox, ControlWidthClass.Auto);
				yield return (ACEManifestControlBag.Instance.BillStatusDescriptionTextBox, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ManifestLayoutBuilder<AsycudaManifestHeader>();
	}
}
