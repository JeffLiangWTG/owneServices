using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Testing
{
	[TestedType(typeof(TWBillLayouts))]
	sealed class TWBillLayoutsTest : LayoutsAbstractTest
	{
		public void TestVisibility()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = TWManifestTypes.Codes.ExportLowValueGoods;
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaBillUserControl = FindAsycudaBillUserControl(form);

				CombineAssertions(() =>
				{
					var commonBag = new BillLayoutBuilder<AsycudaBill>().CommonBag;
					Assert("BillNumberTextBox visible", asycudaBillUserControl.FindSingle<Control>(commonBag.BillNumberTextBox.Name).Visible);
					Assert("IncotermDropEdit visible", asycudaBillUserControl.FindSingle<Control>(commonBag.IncotermDropEdit.Name).Visible);
					AssertNull("ShipmentType is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.ShipmentTypeDropEdit.Name));
					AssertNull("UCRNumberTextBox is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.UCRNumberTextBox.Name));
					AssertNull("RemarksTextBox is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.RemarksTextBox.Name));
					AssertNull("FinalDestinationCodeFindBox is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.FinalDestinationCodeFindBox.Name));
					AssertNull("GoodsLocationCodeFindBox is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.GoodsLocationCodeFindBox.Name));
					AssertNull("VolumeCalcDropEdit is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.VolumeCalcDropEdit.Name));
					AssertNull("CarrierReferenceTextBox is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CarrierReferenceTextBox.Name));
					AssertNull("GoodsDescriptionTextBox is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.GoodsDescriptionTextBox.Name));
					AssertNull("MarksAndNumbersTextBox is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.MarksAndNumbersTextBox.Name));
					AssertNull("PrepaidCollectDropEdit is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.PrepaidCollectDropEdit.Name));
					AssertNull("ProcedureCodeFindBox is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.ProcedureCodeFindBox.Name));
					AssertNull("OriginCodeFindBox is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.OriginCodeFindBox.Name));
					AssertNull("AgentAddressControl is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.AgentAddressControl.Name));
					AssertNull("AssociatedPacksGroupBox is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.AssociatedPacksGroupBox.Name));
					AssertNull("BillIssuerCodeFindBox is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.BillIssuerCodeFindBox.Name));
					AssertNull("BillIssuerName is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.BillIssuerNameTextBox.Name));
					AssertNull("BillIssuerTextBox is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.BillIssuerTextBox.Name));
					AssertNull("BillStatus is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.BillStatusDropEdit.Name));
					AssertNull("CargoStatus is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CargoStatusDropEdit.Name));
					AssertNull("ConsigneeAddressControl is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.ConsigneeAddressControl.Name));
					AssertNull("CusJobNumber is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CusJobNumberCodeFindBox.Name));
					AssertNull("CustomsEntryNumber is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CustomsEntryNumberTextBox.Name));
					AssertNull("CustomsEntryNumberType is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CustomsEntryNumberTypeDropEdit.Name));
					AssertNull("CustomsNumbersGroupBox is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CustomsNumbersGroupBox.Name));
					AssertNull("DiscountValueConvertToLocalCurrencyControl is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.DiscountValueConvertToLocalCurrencyControl.Name));
					AssertNull("ForwarderAddressControl is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.ForwarderAddressControl.Name));
					AssertNull("FreightValueConvertToLocalCurrencyControl is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.FreightValueConvertToLocalCurrencyControl.Name));
					AssertNull("GoodsLocation is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.GoodsLocationDropEditWithFixedWidth.Name));
					AssertNull("GoodsLocationAddressControl is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.GoodsLocationAddressControl.Name));
					AssertNull("InsuranceValueConvertToLocalCurrencyControl is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.InsuranceValueConvertToLocalCurrencyControl.Name));
					AssertNull("ManifestQtyCalcEdit is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.ManifestQtyCalcEdit.Name));
					AssertNull("MessageStatus is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.MessageStatusTextBox.Name));
					AssertNull("NetWeightCalcDropEdit is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.NetWeightCalcDropEdit.Name));
					AssertNull("NotifyPartyAddressControl is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.NotifyPartyAddressControl.Name));
					AssertNull("OtherChargesValueConvertToLocalCurrencyControl is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.OtherChargesValueConvertToLocalCurrencyControl.Name));
					AssertNull("RegistrationDate is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.RegistrationDateEdit.Name));
					AssertNull("ShipperAddressControl is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.ShipperAddressControl.Name));
					AssertNull("TransportValueConvertToLocalCurrencyControl is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.TransportValueConvertToLocalCurrencyControl.Name));
					AssertNull("SpecialCargoCodesDropEdit is null", asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.SpecialCargoCodesDropEdit.Name));

					var twBag = TWBillControlBag.Instance;
					Assert("SequenceNumberCalcEdit Visible", asycudaBillUserControl.FindSingle<Control>(twBag.SequenceNumberCalcEdit.Name).Visible);
					Assert("RemarksLongTextControl Visible", asycudaBillUserControl.FindSingleOrDefault<Control>(twBag.RemarksLongTextControl.Name).Visible);
					Assert("ProcedureDropEdit Visible", asycudaBillUserControl.FindSingle<Control>(twBag.ProcedureDropEdit.Name).Visible);
					manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;

					Assert("ManifestQtyCalcDropEdit Visible", asycudaBillUserControl.FindSingle<Control>(twBag.ManifestQtyCalcDropEdit.Name).Visible);
					Assert("GrossWeightCalcDropEdit Visible", asycudaBillUserControl.FindSingle<Control>(twBag.GrossWeightCalcDropEdit.Name).Visible);
					Assert("GoodsValueConvertToLocalCurrencyControl Visible", asycudaBillUserControl.FindSingle<Control>(twBag.GoodsValueConvertToLocalCurrencyControl.Name).Visible);

					manifest.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
					Assert("PortOfLoadingCodeFindBox Visible only when message type is IMP", asycudaBillUserControl.FindSingle<Control>(twBag.PortOfLoadingCodeFindBox.Name).Visible);
					Assert("PortOfDischargeCodeFindBox Visible only when message type is EXP", !asycudaBillUserControl.FindSingle<Control>(twBag.PortOfDischargeCodeFindBox.Name).Visible);
					Assert("LocationInformationTextBox Visible only when message type is IMP", !asycudaBillUserControl.FindSingle<Control>(commonBag.LocationInformationTextBox.Name).Visible);

					manifest.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
					Assert("PortOfLoadingCodeFindBox Visible only when message type is IMP", !asycudaBillUserControl.FindSingle<Control>(twBag.PortOfLoadingCodeFindBox.Name).Visible);
					Assert("PortOfDischargeCodeFindBox Visible only when message type is EXP", asycudaBillUserControl.FindSingle<Control>(twBag.PortOfDischargeCodeFindBox.Name).Visible);
					Assert("LocationInformationTextBox Visible only when message type is IMP", asycudaBillUserControl.FindSingle<Control>(commonBag.LocationInformationTextBox.Name).Visible);
				});
			}
		}

		AsycudaBillUserControl FindAsycudaBillUserControl(ManifestForm form)
		{
			var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
			var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
			var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
			mainTabControl.SelectedTab = billsAndPacksTabPage;
			var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>("billsAndPacksTabControl");
			var billsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>("billsTabPage");
			billsAndPacksTabControl.SelectedTab = billsTabPage;
			return billsTabPage.FindSingle<AsycudaBillUserControl>("asycudaBillUserControl");
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

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillLayoutBuilder<AsycudaBill>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (TWBillControlBag.Instance.SequenceNumberCalcEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.BillNumberTextBox, ControlWidthClass.Long);
				yield return (TWBillControlBag.Instance.RemarksLongTextControl, ControlWidthClass.Long);
				yield return (TWBillControlBag.Instance.ProcedureDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.IncotermDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (TWBillControlBag.Instance.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
				yield return (TWBillControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (TWBillControlBag.Instance.GoodsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (TWBillControlBag.Instance.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
				yield return (TWBillControlBag.Instance.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.LocationInformationTextBox, ControlWidthClass.Long);
			}
		}
	}
}
