using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.PE.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PE.Manifest.GUI.Testing
{
	[TestedType(typeof(PEBillLayouts))]
	sealed class PEBillLayoutsTest : LayoutsAbstractTest
	{
		public void TestPESpecificFiedsVisibilty()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();

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

				var issueDate = asycudaBillUserControl.FindSingleOrDefault<ZDateEdit>(c => c.Name == "BillIssueDateEdit");
				var cargoNature = asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "CargoNatureDropEdit");
				var cargoCondition = asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "CargoConditionDropEdit");

				var shipmentType = asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "ShipmentTypeDropEdit");
				var locationInfo = asycudaBillUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "LocationInformationTextBox");
				var carrierReference = asycudaBillUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "CarrierReferenceTextBox");

				AssertEquals("BillIssueDateEdit", true, issueDate.Visible);
				AssertNull("ShipmentTypeDropEdit", asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "ShipmentTypeDropEdit"));
				AssertNull("LocationInformationTextBox", asycudaBillUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "LocationInformationTextBox"));
				AssertNull("CarrierReferenceTextBox", asycudaBillUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "CarrierReferenceTextBox"));

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals(true, cargoNature.Visible);
				AssertEquals(true, cargoCondition.Visible);

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals(false, cargoNature.Visible);
				AssertEquals(false, cargoCondition.Visible);
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
				yield return (CommonBillControlBag.Instance.OriginCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.FinalDestinationCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GoodsLocationDropEditWithFixedWidth, ControlWidthClass.Long);
				yield return (PEBillControlBag.Instance.BillIssueDateEdit, ControlWidthClass.Auto);
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
				yield return (CommonBillControlBag.Instance.AgentAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.IncotermDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.PrepaidCollectDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.FreightValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.InsuranceValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.DiscountValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.OtherChargesValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CustomsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (PEBillControlBag.Instance.CargoNatureDropEdit, ControlWidthClass.Long);
				yield return (PEBillControlBag.Instance.CargoConditionDropEdit, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillLayoutBuilder<AsycudaBill>();
	}
}
