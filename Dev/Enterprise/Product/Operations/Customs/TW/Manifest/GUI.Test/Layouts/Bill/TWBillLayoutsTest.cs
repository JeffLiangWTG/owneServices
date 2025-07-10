using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TW.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.GUI.Testing
{
	[TestedType(typeof(TWBillLayouts))]
	sealed class TWBillLayoutsTest : LayoutsAbstractTest
	{
		public void TestVisibility()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = TWManifestTypes.Codes.MAN;
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

				CombineAssertions(() =>
				{
					var commonBag = new BillLayoutBuilder<AsycudaBill>().CommonBag;
					Assert("ShipmentType visible", asycudaBillUserControl.FindSingle<Control>(commonBag.ShipmentTypeDropEdit.Name).Visible);
					Assert("BillNumberTextBox visible", asycudaBillUserControl.FindSingle<Control>(commonBag.BillNumberTextBox.Name).Visible);
					Assert("UCRNumberTextBox visible", asycudaBillUserControl.FindSingle<Control>(commonBag.UCRNumberTextBox.Name).Visible);
					Assert("RemarksTextBox visible", asycudaBillUserControl.FindSingle<Control>(commonBag.RemarksTextBox.Name).Visible);

					Assert("FinalDestinationCodeFindBox Visible", asycudaBillUserControl.FindSingle<Control>(commonBag.FinalDestinationCodeFindBox.Name).Visible);
					Assert("LocationInformation Visible", asycudaBillUserControl.FindSingle<Control>(commonBag.LocationInformationTextBox.Name).Visible);
					Assert("GoodsLocationCodeFindBox Visible", asycudaBillUserControl.FindSingle<Control>(commonBag.GoodsLocationCodeFindBox.Name).Visible);
					Assert("GrossWeightCalcDropEdit Visible", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.GrossWeightCalcDropEdit.Name).Visible);
					Assert("VolumeCalcDropEdit Visible", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.VolumeCalcDropEdit.Name).Visible);

					AssertNull("CarrierReferenceTextBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CarrierReferenceTextBox.Name));
					AssertNull("GoodsDescriptionTextBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.GoodsDescriptionTextBox.Name));
					AssertNull("IncotermDropEdit", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.IncotermDropEdit.Name));
					AssertNull("MarksAndNumbersTextBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.MarksAndNumbersTextBox.Name));
					AssertNull("PrepaidCollectDropEdit", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.PrepaidCollectDropEdit.Name));
					AssertNull("ProcedureCodeFindBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.ProcedureCodeFindBox.Name));
					AssertNull("OriginCodeFindBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.OriginCodeFindBox.Name));
					AssertNull("AgentAddressControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.AgentAddressControl.Name));
					AssertNull("AssociatedPacksGroupBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.AssociatedPacksGroupBox.Name));
					AssertNull("BillIssuerCodeFindBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.BillIssuerCodeFindBox.Name));
					AssertNull("BillIssuerName", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.BillIssuerNameTextBox.Name));
					AssertNull("BillIssuerTextBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.BillIssuerTextBox.Name));
					AssertNull("BillStatus", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.BillStatusDropEdit.Name));
					AssertNull("CargoStatus", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CargoStatusDropEdit.Name));
					AssertNull("ConsigneeAddressControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.ConsigneeAddressControl.Name));
					AssertNull("CusJobNumber", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CusJobNumberCodeFindBox.Name));
					AssertNull("CustomsEntryNumber", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CustomsEntryNumberTextBox.Name));
					AssertNull("CustomsEntryNumberType", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CustomsEntryNumberTypeDropEdit.Name));
					AssertNull("CustomsNumbersGroupBox", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CustomsNumbersGroupBox.Name));
					AssertNull("CustomsValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CustomsValueConvertToLocalCurrencyControl.Name));
					AssertNull("DiscountValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.DiscountValueConvertToLocalCurrencyControl.Name));
					AssertNull("ForwarderAddressControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.ForwarderAddressControl.Name));
					AssertNull("FreightValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.FreightValueConvertToLocalCurrencyControl.Name));
					AssertNull("GoodsLocation", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.GoodsLocationDropEditWithFixedWidth.Name));
					AssertNull("GoodsLocationAddressControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.GoodsLocationAddressControl.Name));
					AssertNull("InsuranceValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.InsuranceValueConvertToLocalCurrencyControl.Name));
					AssertNull("ManifestQtyCalcEdit", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.ManifestQtyCalcEdit.Name));
					AssertNull("MessageStatus", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.MessageStatusTextBox.Name));
					AssertNull("NetWeightCalcDropEdit", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.NetWeightCalcDropEdit.Name));
					AssertNull("NotifyPartyAddressControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.NotifyPartyAddressControl.Name));
					AssertNull("OtherChargesValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.OtherChargesValueConvertToLocalCurrencyControl.Name));
					AssertNull("RegistrationDate", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.RegistrationDateEdit.Name));
					AssertNull("ShipperAddressControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.ShipperAddressControl.Name));
					AssertNull("TransportValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.TransportValueConvertToLocalCurrencyControl.Name));
					AssertNull("SpecialCargoCodesDropEdit", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.SpecialCargoCodesDropEdit.Name));

					var twBag = TWBillControlBag.Instance;
					Assert("PortOfLoadingCodeFindBox Visible", asycudaBillUserControl.FindSingle<Control>(twBag.PortOfLoadingCodeFindBox.Name).Visible);
					Assert("GoodsDescriptionLongTextControl Visible", asycudaBillUserControl.FindSingle<Control>(twBag.GoodsDescriptionLongTextControl.Name).Visible);
					Assert("ManifestQtyCalcDropEdit Visible", asycudaBillUserControl.FindSingleOrDefault<Control>(twBag.ManifestQtyCalcDropEdit.Name).Visible);
					Assert("SplitQuantityCalcDropEdit Visible", asycudaBillUserControl.FindSingle<Control>(twBag.SplitQuantityCalcDropEdit.Name).Visible);
					manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
					Assert("BagNumberDropEdit Visible only when transportmode is AIR", asycudaBillUserControl.FindSingle<Control>(twBag.BagNumberDropEdit.Name).Visible);
					manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
					Assert("BagNumberDropEdit not Visible", !asycudaBillUserControl.FindSingle<Control>(twBag.BagNumberDropEdit.Name).Visible);
				});
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
				yield return (CommonBillControlBag.Instance.ShipmentTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.BillNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.UCRNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.RemarksTextBox, ControlWidthClass.Long);
				yield return (TWBillControlBag.Instance.GoodsDescriptionLongTextControl, ControlWidthClass.Long);
				yield return (TWBillControlBag.Instance.BagNumberDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (TWBillControlBag.Instance.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
				yield return (TWBillControlBag.Instance.SplitQuantityCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Long);
				yield return (TWBillControlBag.Instance.MarksAndNumbersLongTextControl, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (TWBillControlBag.Instance.TariffFindBox, ControlWidthClass.Long);
				yield return (TWBillControlBag.Instance.DGUNNOCodeFindBox, ControlWidthClass.Long);
				yield return (TWBillControlBag.Instance.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.LocationInformationTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.FinalDestinationCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GoodsLocationCodeFindBox, ControlWidthClass.Long);
				yield return (TWBillControlBag.Instance.IsEscortRequiredCheckBox, ControlWidthClass.Long);
			}
		}
	}
}
