using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TR.ETrade.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.GUI.Testing
{
	[TestedType(typeof(ETradeBillDetailsLayouts))]
	sealed class ETradeBillDetailsLayoutsTest : LayoutsAbstractTest
	{
		public void TestFieldVisibiltyAndBehaviours()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness, "Nature Of Business");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExemptionCode, "Exemption Code");

			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
			manifest.AMA_Nature = ShipmentTypeList.Codes.Import23;
			var bill = manifest.Bills.AddNew();

			Factory.Save();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingleOrDefault<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var billsTabPage = billsAndPacksTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsTabPage");
				billsAndPacksTabControl.SelectedTab = billsTabPage;
				var asycudaBillUserControl = billsTabPage.FindSingleOrDefault<AsycudaBillUserControl>(c => c.Name == "asycudaBillUserControl");
				var shipmentTypeDropEdit = asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "ShipmentTypeDropEdit");
				var natureOfBusinessDropEdit = asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "NatureOfBusinessDropEdit");
				var exemptionCode1DropEdit = asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "ExemptionCode1DropEdit");
				var exemptionCode2DropEdit = asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "ExemptionCode2DropEdit");
				var departureCountryCodeFindBox = asycudaBillUserControl.FindSingleOrDefault<ZCodeFindBox>(c => c.Name == "DepartureCountryCodeFindBox");
				var tradeCountryCodeFindBox = asycudaBillUserControl.FindSingleOrDefault<ZCodeFindBox>(c => c.Name == "TradeCountryCodeFindBox");
				var exportCountryCodeFindBox = asycudaBillUserControl.FindSingleOrDefault<ZCodeFindBox>(c => c.Name == "ExportCountryCodeFindBox");
				var arrivalCountryCodeFindBox = asycudaBillUserControl.FindSingleOrDefault<ZCodeFindBox>(c => c.Name == "ArrivalCountryCodeFindBox");
				var paymentMethodDropEdit = asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "PaymentMethodDropEdit");
				var accountantTextBox = asycudaBillUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "AccountantTextBox");
				var accountantVATTextBox = asycudaBillUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "AccountantVATTextBox");
				var netWeightTextBox = asycudaBillUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "NetWeightTextBox");
				var specialCargoCodesDropEdit = asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "SpecialCargoCodesDropEdit");
				var notifyPartyAddressControl = asycudaBillUserControl.FindSingle<ZAddressControl>("NotifyPartyAddressControl");
				var addressStatusButton = notifyPartyAddressControl.FindSingleOrDefault<ZButton>(c => c.Name == "AddressStatusButton");

				AssertEquals("ShipmentTypeDropEdit", true, shipmentTypeDropEdit.Visible);
				AssertEquals("NatureOfBusinessDropEdit", true, natureOfBusinessDropEdit.Visible);
				AssertEquals("ExemptionCode1DropEdit", true, exemptionCode1DropEdit.Visible);
				AssertEquals("ExemptionCode2DropEdit", true, exemptionCode2DropEdit.Visible);
				AssertEquals("DepartureCountryCodeFindBox", true, departureCountryCodeFindBox.Visible);
				AssertEquals("TradeCountryCodeFindBox", true, tradeCountryCodeFindBox.Visible);
				AssertEquals("ExportCountryCodeFindBox", true, exportCountryCodeFindBox.Visible);
				AssertEquals("ArrivalCountryCodeFindBox", true, arrivalCountryCodeFindBox.Visible);
				AssertEquals("IncotermDropEdit", true, asycudaBillUserControl.FindSingle<ZDropEdit>(nameof(CommonBillControlBag.IncotermDropEdit)).Visible);
				AssertEquals("PaymentMethodDropEdit", true, paymentMethodDropEdit.Visible);
				AssertEquals("AccountantTextBox", true, accountantTextBox.Visible);
				AssertEquals("AccountantVATTextBox", true, accountantVATTextBox.Visible);
				AssertNull("NetWeightTextBox", netWeightTextBox);
				AssertEquals("OtherValueCalcFindBox", true, asycudaBillUserControl.FindSingle<ZCalcFindBox>(nameof(ETradeBillDetailsControlBag.OtherValueCalcFindBox)).Visible);
				AssertEquals("ProcedureDropEdit", true, asycudaBillUserControl.FindSingle<ZCodeFindBox>(nameof(CommonBillControlBag.ProcedureCodeFindBox)).Visible);
				AssertEquals("BillStatus", true, asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(nameof(CommonBillControlBag.BillStatusDropEdit)).Visible);
				AssertEquals("CargoStatus", true, asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(nameof(CommonBillControlBag.CargoStatusDropEdit)).Visible);
				AssertEquals("ContainerNumberDropEdit", true, asycudaBillUserControl.FindSingle<ZDropEdit>(nameof(ETradeBillDetailsControlBag.ContainerNumberDropEdit)).Visible);
				AssertEquals("SpecialCargoCodesDropEdit", true, asycudaBillUserControl.FindSingle<ZDropEdit>(nameof(CommonBillControlBag.SpecialCargoCodesDropEdit)).Visible);
				AssertEquals("SeparatedCheckBox", true, asycudaBillUserControl.FindSingle<ZCheckBox>(nameof(ETradeBillDetailsControlBag.SeparatedCheckBox)).Visible);
				AssertEquals(true, notifyPartyAddressControl.ShowOrganisationName);
				AssertEquals(false, notifyPartyAddressControl.ShowAddressDropEdit);
				AssertNotNull("AddressStatusButton should not be null", addressStatusButton);
				AssertEquals("AddressStatusButton's visibility should be false", false, addressStatusButton.Visible);
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
				yield return (ETradeBillDetailsControlBag.Instance.ShipmentTypeDropEdit, ControlWidthClass.Long);
				yield return (ETradeBillDetailsControlBag.Instance.DepartureCountryCodeFindBox, ControlWidthClass.Long);
				yield return (ETradeBillDetailsControlBag.Instance.TradeCountryCodeFindBox, ControlWidthClass.Long);
				yield return (ETradeBillDetailsControlBag.Instance.ExportCountryCodeFindBox, ControlWidthClass.Long);
				yield return (ETradeBillDetailsControlBag.Instance.ArrivalCountryCodeFindBox, ControlWidthClass.Long);
				yield return (ETradeBillDetailsControlBag.Instance.PaymentMethodDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ProcedureCodeFindBox, ControlWidthClass.Long);
				yield return (ETradeBillDetailsControlBag.Instance.NatureOfBusinessDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GoodsLocationDropEditWithFixedWidth, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.IncotermDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.LocationInformationTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CustomsEntryNumberTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CustomsEntryNumberTextBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.ShipperAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
				yield return (ETradeBillDetailsControlBag.Instance.AccountantTextBox, ControlWidthClass.Long);
				yield return (ETradeBillDetailsControlBag.Instance.AccountantVATTextBox, ControlWidthClass.Long);
				yield return (ETradeBillDetailsControlBag.Instance.ContainerNumberDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.MarksAndNumbersTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Long);
				yield return (ETradeBillDetailsControlBag.Instance.ExemptionCode1DropEdit, ControlWidthClass.Long);
				yield return (ETradeBillDetailsControlBag.Instance.ExemptionCode2DropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.NotifyPartyAddressControl, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.CustomsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (ETradeBillDetailsControlBag.Instance.GoodsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (ETradeBillDetailsControlBag.Instance.PrecedentFreightCostLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.InsuranceValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (ETradeBillDetailsControlBag.Instance.OtherValueCalcFindBox, ControlWidthClass.Long);
				yield return (ETradeBillDetailsControlBag.Instance.GuaranteeTypeDropEdit, ControlWidthClass.Long);
				yield return (ETradeBillDetailsControlBag.Instance.GuaranteeRefNoTextBox, ControlWidthClass.Long);
				yield return (ETradeBillDetailsControlBag.Instance.GuaranteeAmountCalcEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.BillStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CargoStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.SpecialCargoCodesDropEdit, ControlWidthClass.Long);
				yield return (ETradeBillDetailsControlBag.Instance.SeparatedCheckBox, ControlWidthClass.Long);
			}
		}

		public void TestCaptions()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			var grossWeightControl = CommonBag.GrossWeightCalcDropEdit;
			var separatedCheckBox = ETradeBillDetailsControlBag.Instance.SeparatedCheckBox;
			var notifyPartyAddress = CommonBag.NotifyPartyAddressControl;

			Layout.TryGetCaption(grossWeightControl, bill, out var grossWeightCaption);
			Layout.TryGetCaption(separatedCheckBox, bill, out var separatedCaption);
			Layout.TryGetCaption(notifyPartyAddress, bill, out var marketPlaceCaption);

			CombineAssertions("Captions", () =>
			{
				AssertEquals("Gross Weight", grossWeightCaption.Caption);
				AssertEquals("Separated", separatedCaption.Caption);
				AssertEquals("Market Place", marketPlaceCaption.Caption);
			});
		}

		public PanelLayout Layout => layout ??= ((IPanelLayoutProvider)new ETradeBillDetailsLayouts()).Layout;
		PanelLayout layout;

		CommonBillControlBag CommonBag => CommonBillControlBag.Instance;
	}
}
