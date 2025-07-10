using System.Collections.Generic;
using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TR.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.GUI.Testing
{
	[TestedType(typeof(TRBillLayouts))]
	sealed class TRBillLayoutsTest : LayoutsAbstractTest
	{
		public void TestCustomsNumbersGroupBoxVisibilty()
		{
			var manifest = Factory.New<Business.AsycudaManifestHeader>();
			manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			manifest.AMA_ManifestType = TRManifestTypes.Codes.EMANIF;
			manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			var bill = manifest.Bills.AddNew();
			bill.ABL_BillNumber = "B1234";
			Factory.Save();
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var billsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsTabPage");
				billsAndPacksTabControl.SelectedTab = billsTabPage;
				var billUserControl = billsTabPage.Controls.Find("asycudaBillUserControl", true).First() as AsycudaBillUserControl;
				billUserControl.Focus();
				var zGroupBoxCustomsNumbers = billUserControl.Controls.Find("CustomsNumbersGroupBox", true).FirstOrDefault();
				AssertEquals(true, zGroupBoxCustomsNumbers.Visible);
				manifest.AMA_ManifestType = TRManifestTypes.Codes.VARONC;
				AssertEquals(true, zGroupBoxCustomsNumbers.Visible);
			}
		}

		public void TestTRSpecificFiedsVisibilty()
		{
			var manifest = Factory.NewWithValidTestData<Business.AsycudaManifestHeader>();
			manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			manifest.AMA_ManifestType = TRManifestTypes.Codes.DENITH;
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
				asycudaBillUserControl.Focus();
				var paymentTypeDropEdit = asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "PaymentTypeDropEdit");
				var transshipmentTypeDropEdit = asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "TransshipmentTypeDropEdit");
				var roroCheckBox = asycudaBillUserControl.FindSingleOrDefault<ZCheckBox>(c => c.Name == "RoRoCheckBox");
				var billStampDutyValueCalcEdit = asycudaBillUserControl.FindSingleOrDefault<ZCalcEdit>(c => c.Name == "BillStampDutyValueCalcEdit");
				var airBillStampDutyABSValueCalcEdit = asycudaBillUserControl.FindSingleOrDefault<ZCalcEdit>(c => c.Name == "AirBillStampDutyABSValueCalcEdit");
				var customsValueConvertToLocalCurrencyControl = asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "CustomsValueConvertToLocalCurrencyControl");
				var goodsLocationCodeFindBox = asycudaBillUserControl.FindSingleOrDefault<ZCodeFindBox>(c => c.Name == "GoodsLocationCodeFindBox");
				CombineAssertions("TR Specific Fieds Visibilty", () =>
				{
					AssertEquals("PaymentTypeDropEdit", true, paymentTypeDropEdit.Visible);
					AssertEquals("TransshipmentTypeDropEdit", true, transshipmentTypeDropEdit.Visible);
					AssertEquals("RoRoCheckBox", true, roroCheckBox.Visible);
					AssertEquals("CustomsValueConvertToLocalCurrencyControl", true, customsValueConvertToLocalCurrencyControl.Visible);
					AssertEquals("BillStampDutyValueCalcEdit for SEA", true, billStampDutyValueCalcEdit.Visible);
					AssertEquals("AirBillStampDutyABSValueCalcEdit for SEA", false, airBillStampDutyABSValueCalcEdit.Visible);
					AssertEquals("GoodsLocationCodeFindBox", true, goodsLocationCodeFindBox.Visible);
				});
			}
		}

		public void TestStampDutyCaptions()
		{
			var manifest = Factory.NewWithValidTestData<Business.AsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			manifest.AMA_ManifestType = TRManifestTypes.Codes.DENITH;
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
				asycudaBillUserControl.Focus();
				var billStampDutyValueCalcEdit = asycudaBillUserControl.FindSingleOrDefault<ZCalcEdit>(nameof(TRBillControlBag.BillStampDutyValueCalcEdit));
				var airBillStampDutyABSValueCalcEdit = asycudaBillUserControl.FindSingleOrDefault<ZCalcEdit>(nameof(TRBillControlBag.AirBillStampDutyABSValueCalcEdit));
				CombineAssertions("TR Specific Fields Caption", () =>
				{
					AssertEquals("Caption of BillStampDutyValueCalcEdit for SEA", "Stamp Duty", billStampDutyValueCalcEdit.GetExtension<LabelCaptionRenderer>().Caption);
					manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
					manifest.AMA_ManifestType = TRManifestTypes.Codes.HAVITH;
					AssertEquals("Caption of BillStampDutyValueCalcEdit for AIR", "Stamp Duty (OBS)", billStampDutyValueCalcEdit.GetExtension<LabelCaptionRenderer>().Caption);
					AssertEquals("Caption of AirBillStampDutyABSValueCalcEdit for AIR", "Stamp Duty (ABS)", airBillStampDutyABSValueCalcEdit.GetExtension<LabelCaptionRenderer>().Caption);
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

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.BillNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ShipmentTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.OriginCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.FinalDestinationCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GoodsLocationCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.LocationInformationTextBox, ControlWidthClass.Long);
				yield return (TRBillControlBag.Instance.PaymentTypeDropEdit, ControlWidthClass.Long);
				yield return (TRBillControlBag.Instance.TransshipmentTypeDropEdit, ControlWidthClass.Long);
				yield return (TRBillControlBag.Instance.RoRoCheckBox, ControlWidthClass.Auto);
				yield return (CommonBillControlBag.Instance.SpecialCargoCodesDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.ShipperAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.NotifyPartyAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.AgentAddressControl, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.CustomsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CustomsEntryNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CustomsEntryNumberTypeDropEdit, ControlWidthClass.Long);
				yield return (TRBillControlBag.Instance.BillStampDutyValueCalcEdit, ControlWidthClass.Long);
				yield return (TRBillControlBag.Instance.AirBillStampDutyABSValueCalcEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CustomsNumbersGroupBox, ControlWidthClass.LongNoCaption);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillLayoutBuilder<Business.AsycudaBill>();
	}
}
