using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Testing
{
	[TestedType(typeof(TWManifestLayouts))]
	sealed class TWManifestLayoutsTest : LayoutsAbstractTest
	{
		public void TestVisibility()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration;
			manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			manifest.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;

			using (var form = new BriefDeclarationForm(manifest))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>("mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;
				var manifestGroupBox = mainTabPage.FindSingle<ZGroupBox>("ManifestGroupBox");

				Assert("GuaranteeTextBox Visible", manifestGroupBox.FindSingle<Control>(CommonManifestControlBag.Instance.GuaranteeTextBox.Name).Visible);
				Assert("ETADateEdit Visible", manifestGroupBox.FindSingle<Control>(CommonManifestControlBag.Instance.ETADateEdit.Name).Visible);
				Assert("PaymentMethodDropEdit Visible", manifestGroupBox.FindSingle<Control>(CommonManifestControlBag.Instance.PaymentMethodDropEdit.Name).Visible);
				Assert("BagNumberTextBox Visible", manifestGroupBox.FindSingle<Control>(TWManifestControlBag.Instance.BagNumberTextBox.Name).Visible);
				Assert("CarrierReferenceTextBox not Visible", !manifestGroupBox.FindSingle<Control>(CommonManifestControlBag.Instance.CarrierReferenceTextBox.Name).Visible);
				Assert("VehicleRegistrationTextBox not Visible", !manifestGroupBox.FindSingle<Control>(CommonManifestControlBag.Instance.VehicleRegistrationTextBox.Name).Visible);
				Assert("ManifestNumberTextBox not Visible", !manifestGroupBox.FindSingle<Control>(CommonManifestControlBag.Instance.ManifestNumberTextBox.Name).Visible);

				manifest.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
				manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
				Assert("GuaranteeTextBox not Visible", !manifestGroupBox.FindSingle<Control>(CommonManifestControlBag.Instance.GuaranteeTextBox.Name).Visible);
				Assert("ETADateEdit not Visible", !manifestGroupBox.FindSingle<Control>(CommonManifestControlBag.Instance.ETADateEdit.Name).Visible);
				Assert("PaymentMethodDropEdit not Visible", !manifestGroupBox.FindSingle<Control>(CommonManifestControlBag.Instance.PaymentMethodDropEdit.Name).Visible);
				Assert("CarrierReferenceTextBox not Visible", !manifestGroupBox.FindSingle<Control>(CommonManifestControlBag.Instance.CarrierReferenceTextBox.Name).Visible);
				Assert("VehicleRegistrationTextBox not Visible", !manifestGroupBox.FindSingle<Control>(CommonManifestControlBag.Instance.VehicleRegistrationTextBox.Name).Visible);
				Assert("ManifestNumberTextBox not Visible", !manifestGroupBox.FindSingle<Control>(CommonManifestControlBag.Instance.ManifestNumberTextBox.Name).Visible);
				Assert("ExporterAddressUserControl Visible", manifestGroupBox.FindSingle<Control>(TWManifestControlBag.Instance.ExporterAddressUserControl.Name).Visible);
				Assert("ImporterAddressUserControl not Visible", !manifestGroupBox.FindSingle<Control>(TWManifestControlBag.Instance.ImporterAddressUserControl.Name).Visible);

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				Assert("CarrierReferenceTextBox Visible", manifestGroupBox.FindSingle<Control>(CommonManifestControlBag.Instance.CarrierReferenceTextBox.Name).Visible);
				Assert("VehicleRegistrationTextBox Visible", manifestGroupBox.FindSingle<Control>(CommonManifestControlBag.Instance.VehicleRegistrationTextBox.Name).Visible);
				Assert("ManifestNumberTextBox not Visible", !manifestGroupBox.FindSingle<Control>(CommonManifestControlBag.Instance.ManifestNumberTextBox.Name).Visible);
				Assert("BagNumberTextBox not Visible", !manifestGroupBox.FindSingle<Control>(TWManifestControlBag.Instance.BagNumberTextBox.Name).Visible);

				manifest.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
				Assert("ManifestNumberTextBox Visible", manifestGroupBox.FindSingle<Control>(CommonManifestControlBag.Instance.ManifestNumberTextBox.Name).Visible);
				Assert("ImporterAddressUserControl Visible", manifestGroupBox.FindSingle<Control>(TWManifestControlBag.Instance.ImporterAddressUserControl.Name).Visible);
				Assert("ExporterAddressUserControl not Visible", !manifestGroupBox.FindSingle<Control>(TWManifestControlBag.Instance.ExporterAddressUserControl.Name).Visible);
			}
		}

		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ManifestLayoutBuilder<AsycudaManifestHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.NatureDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ManifestTypeShortCodeLengthDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CustomsOfficeShortCodeLengthDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.GoodsLocationCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.MasterBillTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CarrierReferenceTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.PaymentMethodDropEdit, ControlWidthClass.Long);
				yield return (TWManifestControlBag.Instance.DeclarationDateEdit, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.ETADateEdit, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.GuaranteeTextBox, ControlWidthClass.Long);
				yield return (TWManifestControlBag.Instance.ImporterAddressUserControl, ControlWidthClass.LongNoCaption);
				yield return (TWManifestControlBag.Instance.ExporterAddressUserControl, ControlWidthClass.LongNoCaption);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (TWManifestControlBag.Instance.EntryNumberUserControl, ControlWidthClass.Long);
				yield return (TWManifestControlBag.Instance.BagNumberTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VoyageFlightTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.LloydsNumberTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.RadioCallSignTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.VehicleRegistrationTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.ManifestNumberTextBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.RecipientReferenceDropEdit, ControlWidthClass.Long);
				yield return (TWManifestControlBag.Instance.CustomsAgentCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CustomsProfileDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CarrierAddressControl, ControlWidthClass.Long);
				yield return (TWManifestControlBag.Instance.PersonGuidFindBox, ControlWidthClass.Long);
			}
		}
	}
}
