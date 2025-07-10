using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.GUI.Testing
{
	[TestedType(typeof(ACEBillLayouts))]
	sealed class ACEBillLayoutsTest : LayoutsAbstractTest
	{
		public void TestVisibilityOrCaption()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			manifest.Bills.RemoveAndDeleteAll(); // Ensure that controls are visible even without a bill
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>("billsAndPacksTabControl");
				var billsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>("billsTabPage");
				billsAndPacksTabControl.SelectedTab = billsTabPage;
				var asycudaBillUserControl = billsTabPage.FindSingle<AsycudaBillUserControl>("asycudaBillUserControl");
				var commonBag = new BillLayoutBuilder<AsycudaBill>().CommonBag;
				AssertEquals("MessageStatus Visible", true, asycudaBillUserControl.FindSingle<Control>(commonBag.MessageStatusTextBox.Name).Visible);
				AssertNull("PrepaidCollectDropEdit", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.PrepaidCollectDropEdit.Name));
				AssertNull("AgentAddressControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.AgentAddressControl.Name));
				AssertNull("AssociatedPacksGroupBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.AssociatedPacksGroupBox.Name));
				AssertNull("BillIssuerCodeFindBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.BillIssuerCodeFindBox.Name));
				AssertNull("BillIssuerName", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.BillIssuerNameTextBox.Name));
				AssertNull("BillIssuerTextBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.BillIssuerTextBox.Name));
				AssertNull("BillStatus", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.BillStatusDropEdit.Name));
				AssertNull("CustomsEntryNumber", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CustomsEntryNumberTextBox.Name));
				AssertNull("CustomsEntryNumberType", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CustomsEntryNumberTypeDropEdit.Name));
				AssertNull("CusJobNumber", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CusJobNumberCodeFindBox.Name));
				AssertNull("CustomsNumbersGroupBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CustomsNumbersGroupBox.Name));
				AssertNull("CustomsValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CustomsValueConvertToLocalCurrencyControl.Name));
				AssertNull("DiscountValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.DiscountValueConvertToLocalCurrencyControl.Name));
				AssertNull("ForwarderAddressControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.ForwarderAddressControl.Name));
				AssertNull("FreightValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.FreightValueConvertToLocalCurrencyControl.Name));
				AssertNull("GoodsLocation", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.GoodsLocationDropEditWithFixedWidth.Name));
				AssertNull("InsuranceValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.InsuranceValueConvertToLocalCurrencyControl.Name));
				AssertNull("LocationInformation", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.LocationInformationTextBox.Name));
				AssertNull("OtherChargesValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.OtherChargesValueConvertToLocalCurrencyControl.Name));
				AssertNull("RegistrationDate", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.RegistrationDateEdit.Name));
				AssertNull("TransportValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.TransportValueConvertToLocalCurrencyControl.Name));
				AssertNull("UCRNumberTextBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.UCRNumberTextBox.Name));
				AssertNull("VolumeCalcDropEdit", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.VolumeCalcDropEdit.Name));
				AssertNull("ShipmentType", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.ShipmentTypeDropEdit.Name));
				AssertNull("MarksAndNumbersTextBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.MarksAndNumbersTextBox.Name));
				AssertNull("RemarksTextBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.RemarksTextBox.Name));
				AssertNull("CarrierReferenceTextBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CarrierReferenceTextBox.Name));
				AssertNull("ManifestQtyCalcDropEdit", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.ManifestQtyCalcDropEdit.Name));
				var originCodeFindBox = asycudaBillUserControl.FindSingle<ZCodeFindBox>(commonBag.OriginCodeFindBox.Name);
				AssertEquals("OriginCodeFindBox Visible", true, originCodeFindBox.Visible);
				AssertEquals("OriginCodeFindBox Caption", "Flight Origin", originCodeFindBox.CaptionResourceString.Caption);
				var aceBag = ACEBillControlBag.Instance;
				AssertEquals("FDAIndicatorCheckBox Visible", true, asycudaBillUserControl.FindSingle<Control>(aceBag.FDAIndicatorCheckBox.Name).Visible);
				AssertEquals("BillStatusTextBox Visible", true, asycudaBillUserControl.FindSingle<Control>(aceBag.BillStatusTextBox.Name).Visible);
				AssertEquals("BillStatusDescriptionTextBox Visible", true, asycudaBillUserControl.FindSingle<Control>(aceBag.BillStatusDescriptionTextBox.Name).Visible);
				AssertEquals("ManifestQtyCalcEdit Visible", true, asycudaBillUserControl.FindSingle<Control>(commonBag.ManifestQtyCalcEdit.Name).Visible);
				AssertEquals("GoodsValueConvertToLocalCurrencyControl Visible", true, asycudaBillUserControl.FindSingle<Control>(aceBag.GoodsValueConvertToLocalCurrencyControl.Name).Visible);
				AssertEquals("EntryNumberTypeDropEdit", true, asycudaBillUserControl.FindSingle<Control>(aceBag.EntryNumberTypeDropEdit.Name).Visible);
				AssertEquals("EntryNumberTextBox", true, asycudaBillUserControl.FindSingle<Control>(aceBag.EntryNumberTextBox.Name).Visible);
				AssertEquals("GoodsOriginCodeFindBox Visible", true, asycudaBillUserControl.FindSingle<Control>(aceBag.GoodsOriginCodeFindBox.Name).Visible);
				AssertEquals("TariffCodeFindBox Visible", true, asycudaBillUserControl.FindSingle<Control>(aceBag.TariffCodeFindBox.Name).Visible);
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

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillLayoutBuilder<AsycudaBill>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.BillNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.OriginCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.FinalDestinationCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ManifestQtyCalcEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (ACEBillControlBag.Instance.GoodsValueConvertToLocalCurrencyControl, ControlWidthClass.Auto);
				yield return (ACEBillControlBag.Instance.GoodsOriginCodeFindBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ShipperAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.NotifyPartyAddressControl, ControlWidthClass.Long);
				yield return (ACEBillControlBag.Instance.FDAIndicatorCheckBox, ControlWidthClass.Auto);
				yield return (ACEBillControlBag.Instance.EntryNumberTypeDropEdit, ControlWidthClass.Long);
				yield return (ACEBillControlBag.Instance.TariffCodeFindBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.MessageStatusTextBox, ControlWidthClass.Long);
				yield return (ACEBillControlBag.Instance.BillStatusTextBox, ControlWidthClass.Auto);
				yield return (ACEBillControlBag.Instance.BillStatusDescriptionTextBox, ControlWidthClass.Long);
				yield return (ACEBillControlBag.Instance.EntryNumberTextBox, ControlWidthClass.Auto);
			}
		}
	}
}
