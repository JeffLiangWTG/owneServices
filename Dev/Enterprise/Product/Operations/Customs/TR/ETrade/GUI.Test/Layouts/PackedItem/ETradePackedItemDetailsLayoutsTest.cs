using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TR.ETrade.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.GUI.Testing
{
	[TestedType(typeof(ETradePackedItemDetailsLayouts))]
	sealed class ETradePackedItemDetailsLayoutsTest : LayoutsAbstractTest
	{
		public void TestFieldVisibilty()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
			var bill = manifest.Bills.AddNew();
			bill.ABL_ShipmentType = Universal.Helper.ShipmentTypeList.Codes.Import23;

			Factory.Save();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var packsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_AsycudaPackUserControl");
				billsAndPacksTabControl.SelectedTab = packsTabPage;
				var asycudaPackUserControl = packsTabPage.FindSingle<AsycudaPackUserControl>(c => c.Name == "AsycudaPackUserControl");
				var asycudaPackedItemsUserControl = asycudaPackUserControl.FindSingle<AsycudaPackedItemsUserControl>(c => c.Name == "AsycudaPackedItemsUserControl");

				var goodsDescriptionTextBox = asycudaPackedItemsUserControl.Controls.Find("GoodsDescriptionTextBox", true).First();
				var serialNoTextBox = asycudaPackedItemsUserControl.Controls.Find("SerialNoTextBox", true).First();

				var aPI_BrandTextBox = asycudaPackedItemsUserControl.Controls.Find("BrandTextBox", true).First();
				var aPI_ModelTextBox = asycudaPackedItemsUserControl.Controls.Find("ModelTextBox", true).First();
				var aPI_TariffCodeFindBox = asycudaPackedItemsUserControl.Controls.Find("tariffFindBox", true).First() as Universal.GUI.TariffFindBox;
				var usedGoodsCodeTextBox = asycudaPackedItemsUserControl.Controls.Find("UsedGoodsCodeTextBox", true).First();
				var aPI_RN_NKGoodsOriginCodeFindBox = asycudaPackedItemsUserControl.Controls.Find("GoodsOriginCodeFindBox", true).First();
				var aPI_CustomsQty2CalcDropEdit = asycudaPackedItemsUserControl.Controls.Find("CustomsQty2CalcDropEdit", true).First();
				var aPI_CustomsQty3CalcDropEdit = asycudaPackedItemsUserControl.Controls.Find("CustomsQty3CalcDropEdit", true).First();
				var agriculturePolicyTextBox = asycudaPackedItemsUserControl.Controls.Find("AgriculturePolicyTextBox", true).First();
				var valueDeclarationFormTextBox = asycudaPackedItemsUserControl.Controls.Find("ValueDeclarationFormTextBox", true).First();
				var calculationMethodTextBox = asycudaPackedItemsUserControl.Controls.Find("CalculationMethodTextBox", true).First();
				var quotaCheckBox = asycudaPackedItemsUserControl.Controls.Find("QuotaCheckBox", true).First();
				var banderolTariffFindBox =
					asycudaPackedItemsUserControl.FindSingle<Universal.GUI.TariffFindBox>(
						nameof(ETradePackedItemDetailsControlBag.BanderolTariffFindBox));
				var tariffAdditionalCodeEditBox =
					asycudaPackedItemsUserControl.FindSingle<ZDropEdit>(
						nameof(ETradePackedItemDetailsControlBag.TariffAdditionalCodeDropEdit));

				CombineAssertions(() =>
				{
					AssertEquals(true, goodsDescriptionTextBox.Visible);
					AssertEquals(true, serialNoTextBox.Visible);
					AssertEquals(true, asycudaPackedItemsUserControl.FindSingle<ZCalcDropEdit>(nameof(CommonPackedItemDetailsControlBag.CustomsQtyCalcDropEdit)).Visible);
					AssertEquals(true, aPI_BrandTextBox.Visible);
					AssertEquals(true, aPI_ModelTextBox.Visible);
					AssertEquals(true, aPI_TariffCodeFindBox.Visible);
					AssertEquals(true, aPI_TariffCodeFindBox.ShowDescriptionBox);
					AssertEquals(true, usedGoodsCodeTextBox.Visible);
					AssertEquals(true, aPI_RN_NKGoodsOriginCodeFindBox.Visible);
					AssertEquals(true, aPI_CustomsQty2CalcDropEdit.Visible);
					AssertEquals(true, aPI_CustomsQty3CalcDropEdit.Visible);
					AssertEquals(true, asycudaPackedItemsUserControl.FindSingle<ConvertToLocalCurrencyControl>(nameof(CommonPackedItemDetailsControlBag.GoodsValueLocalCurrencyControl)).Visible);
					AssertEquals(true, agriculturePolicyTextBox.Visible);
					AssertEquals(true, valueDeclarationFormTextBox.Visible);
					AssertEquals(true, calculationMethodTextBox.Visible);
					AssertEquals(true, quotaCheckBox.Visible);
					AssertEquals(true, banderolTariffFindBox.Visible);
					AssertEquals(true, banderolTariffFindBox.ShowDescriptionBox);
					AssertEquals(true, tariffAdditionalCodeEditBox.Visible);
					AssertEquals(true, tariffAdditionalCodeEditBox.ShowDescriptionBox);
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
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new PackedItemDetailsLayoutBuilder<AsycudaPack>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonPackedItemDetailsControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Long);
				yield return (ETradePackedItemDetailsControlBag.Instance.SerialNoTextBox, ControlWidthClass.Long);
				yield return (CommonPackedItemDetailsControlBag.Instance.CustomsQtyCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonPackedItemDetailsControlBag.Instance.BrandTextBox, ControlWidthClass.Long);
				yield return (CommonPackedItemDetailsControlBag.Instance.ModelTextBox, ControlWidthClass.Long);
				yield return (CommonPackedItemDetailsControlBag.Instance.TariffFindBox, ControlWidthClass.Long);
				yield return (ETradePackedItemDetailsControlBag.Instance.TariffAdditionalCodeDropEdit, ControlWidthClass.Long);
				yield return (ETradePackedItemDetailsControlBag.Instance.BanderolTariffFindBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (ETradePackedItemDetailsControlBag.Instance.UsedGoodsCodeTextBox, ControlWidthClass.Long);
				yield return (CommonPackedItemDetailsControlBag.Instance.GoodsOriginCodeFindBox, ControlWidthClass.Long);
				yield return (CommonPackedItemDetailsControlBag.Instance.CustomsQty2CalcDropEdit, ControlWidthClass.Long);
				yield return (CommonPackedItemDetailsControlBag.Instance.CustomsQty3CalcDropEdit, ControlWidthClass.Long);
				yield return (CommonPackedItemDetailsControlBag.Instance.GoodsValueLocalCurrencyControl, ControlWidthClass.Long);
				yield return (ETradePackedItemDetailsControlBag.Instance.StatisticalValueCalcFindBox, ControlWidthClass.Long);
				yield return (ETradePackedItemDetailsControlBag.Instance.AgriculturePolicyTextBox, ControlWidthClass.Long);
				yield return (ETradePackedItemDetailsControlBag.Instance.ValueDeclarationFormTextBox, ControlWidthClass.Long);
				yield return (ETradePackedItemDetailsControlBag.Instance.CalculationMethodTextBox, ControlWidthClass.Long);
				yield return (ETradePackedItemDetailsControlBag.Instance.QuotaCheckBox, ControlWidthClass.Long);
			}
		}
	}
}
