using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.NZ.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Manifest.GUI.Testing
{
	[TestedType(typeof(NZBillLayouts))]
	sealed class NZBillLayoutsTest : LayoutsAbstractTest
	{
		public void TestVisibility()
		{
			var manifest = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.NewZealand, NZManifestTypes.Codes.OCR, ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_ManifestType = NZManifestTypes.Codes.OCR;
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
				var commonBag = new BillLayoutBuilder<Business.AsycudaBill>().CommonBag;
				AssertNull("PrepaidCollectDropEdit", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.PrepaidCollectDropEdit.Name));
				AssertNull("AssociatedPacksGroupBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.AssociatedPacksGroupBox.Name));
				AssertNull("BillIssuerCodeFindBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.BillIssuerCodeFindBox.Name));
				AssertNull("BillIssuerName", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.BillIssuerNameTextBox.Name));
				AssertNull("BillIssuerTextBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.BillIssuerTextBox.Name));
				AssertNull("BillStatus", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.BillStatusDropEdit.Name));
				AssertNull("CustomsEntryNumberType", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CustomsEntryNumberTypeDropEdit.Name));
				AssertNull("CusJobNumber", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CusJobNumberCodeFindBox.Name));
				AssertNull("CustomsNumbersGroupBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CustomsNumbersGroupBox.Name));
				AssertNull("CustomsValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CustomsValueConvertToLocalCurrencyControl.Name));
				AssertNull("DiscountValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.DiscountValueConvertToLocalCurrencyControl.Name));
				AssertNull("ForwarderAddressControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.ForwarderAddressControl.Name));
				AssertNull("FreightValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.FreightValueConvertToLocalCurrencyControl.Name));
				AssertNull("InsuranceValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.InsuranceValueConvertToLocalCurrencyControl.Name));
				AssertNull("OtherChargesValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.OtherChargesValueConvertToLocalCurrencyControl.Name));
				AssertNull("RegistrationDate", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.RegistrationDateEdit.Name));
				AssertNull("TransportValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.TransportValueConvertToLocalCurrencyControl.Name));
				AssertNull("UCRNumberTextBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.UCRNumberTextBox.Name));
				AssertNull("ManifestQtyCalcDropEdit", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.ManifestQtyCalcDropEdit.Name));
			}
		}

		protected override int ControlBagCount => 1;

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
				yield return (CommonBillControlBag.Instance.BillNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.OriginCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.FinalDestinationCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ManifestQtyCalcEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ShipmentTypeDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.MarksAndNumbersTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.RemarksTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ShipperAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.NotifyPartyAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CustomsEntryNumberTextBox, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new NZBillLayoutBuilder();
	}
}
