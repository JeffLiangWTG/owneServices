using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	partial class ApplicationGUIProviderTest
	{
		public void TestTariffFindBox()
		{
			var helper = new ZZDataTestHelper(Factory);
			Factory.Save();

			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = TransportTypeList.Codes.Air;

			var bill = manifest.Bills.AddNew();
			bill.Packs.AddNew();

			var previousValue = Env.Registry.ExternalBorderComplianceTool;

			void AssertTariffFindBox(string externalBorderComplianceTool, bool isTariffFindBoxWithBorderWise)
			{
				using (var form = new ManifestForm(manifest))
				using (new DisposableAction(() => Env.Registry.ExternalBorderComplianceTool = externalBorderComplianceTool, () => Env.Registry.ExternalBorderComplianceTool = previousValue))
				{
					form.Show();
					Application.DoEvents();

					var asycudaPackUserControl = FindPackUserControl(form);
					AssertEquals(isTariffFindBoxWithBorderWise, !asycudaPackUserControl.FindSingle<Universal.GUI.TariffFindBox>(nameof(CommonPackedItemDetailsControlBag.TariffFindBox)).Visible);
					AssertEquals(isTariffFindBoxWithBorderWise, asycudaPackUserControl.FindSingle<V4.GUI.TariffFindBox>(nameof(SGPackedItemDetailsControlBag.SGEdiTariffFindBox)).Visible);
				}
			}

			AssertTariffFindBox(ExternalBorderComplianceToolList.Codes.None, false);
			AssertTariffFindBox(ExternalBorderComplianceToolList.Codes.BorderWiseWeb, true);
		}

		public void TestPacksGridVisibility()
		{
			var accessEnable = (RegistryItemWrapper)ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable;
			using (accessEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				manifest.AMA_TransportMode = TransportTypeList.Codes.Air;

				var bill = manifest.Bills.AddNew();
				bill.Packs.AddNew();

				using (var form = new ManifestForm(manifest))
				{
					form.Show();
					Application.DoEvents();

					var asycudaPackUserControl = FindPackUserControl(form);
					var packsGrid = asycudaPackUserControl.FindSingle<ZGrid>(c => c.Name == "PacksGrid");

					var packItemStandAloneCountryProperty = nameof(AsycudaPack.PackedItem) + "+";

					var tariffColumnName = packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_FormattedTariff;
					var tariffColumnStyleInfo = (V4.GUI.TariffColumnStyleInfo)packsGrid.GetColumnStyle(tariffColumnName);

					CombineAssertions(() =>
					{
						Assert("API_FormattedTariff Column Style Info IsUnavailable", !tariffColumnStyleInfo.IsUnavailable);
						Assert("API_FormattedTariff Column Style Info IsVisible", tariffColumnStyleInfo.IsVisible);
						Assert("API_FormattedTariff Column IsVisible", packsGrid.Columns.FirstOrDefault(c => c.ColumnName == tariffColumnName).IsVisible);
					});

					var goodsTypeColumnName = packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.GoodsType;
					var goodsTypeColumnStyleInfo = (ZDropEditColumnStyleInfo)packsGrid.GetColumnStyle(goodsTypeColumnName);

					CombineAssertions(() =>
					{
						Assert("GoodsType Column Style Info IsUnavailable", !goodsTypeColumnStyleInfo.IsUnavailable);
						Assert("GoodsType Column Style Info IsVisible", goodsTypeColumnStyleInfo.IsVisible);
						Assert("GoodsType Column IsVisible", packsGrid.Columns.FirstOrDefault(c => c.ColumnName == goodsTypeColumnName).IsVisible);
					});
				}
			}
		}

		public void TestPacksGridColumnNames()
		{
			var helper = new ZZDataTestHelper(Factory);
			Factory.Save();

			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = TransportTypeList.Codes.Air;

			var bill = manifest.Bills.AddNew();
			bill.Packs.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();

				var asycudaPackUserControl = FindPackUserControl(form);
				var packsGrid = asycudaPackUserControl.FindSingle<ZGrid>(c => c.Name == "PacksGrid");

				var actualList = packsGrid.Columns.Cast<ZGridColumn>().Select(x => x.ColumnName);
				var expectedList = GetExpectedPacksGridColumnNames();

				foreach (string column in expectedList)
				{
					AssertCollectionContains("All expected columns are accounted for", column, actualList);
				}
			}
		}

		AsycudaPackUserControl FindPackUserControl(ManifestForm form)
		{
			var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
			var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
			var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
			mainTabControl.SelectedTab = billsAndPacksTabPage;
			var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
			var packsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_AsycudaPackUserControl");
			billsAndPacksTabControl.SelectedTab = packsTabPage;
			return packsTabPage.FindSingle<AsycudaPackUserControl>(c => c.Name == "AsycudaPackUserControl");
		}

		string[] GetExpectedPacksGridColumnNames()
		{
			var packedItemProperty = nameof(AsycudaPack.PackedItem) + "+";
			return new string[]
			{
				AsycudaPack.Schema.APA_PackQty,
				AsycudaPack.Schema.APA_PackUQ,
				AsycudaPack.Schema.APA_CommodityCode,
				AsycudaPack.Schema.APA_GoodsDescription,
				AsycudaPack.Schema.APA_MarksAndNumbers,
				AsycudaPack.Schema.APA_Weight,
				AsycudaPack.Schema.APA_WeightUQ,
				AsycudaPack.Schema.APA_Volume,
				AsycudaPack.Schema.APA_VolumeUQ,
				AsycudaPack.Schema.LinePrice,
				AsycudaPack.Schema.LinePriceCurrency,
				packedItemProperty + AsycudaPackedItem.Schema.API_GoodsDescription,
				packedItemProperty + AsycudaPackedItem.Schema.API_FormattedTariff,
				packedItemProperty + AsycudaPackedItem.Schema.API_CustomsQty,
				packedItemProperty + AsycudaPackedItem.Schema.API_CustomsUQ,
				packedItemProperty + AsycudaPackedItem.Schema.API_CustomsValue,
				packedItemProperty + AsycudaPackedItem.Schema.API_DutyAmount
			};
		}
	}
}
