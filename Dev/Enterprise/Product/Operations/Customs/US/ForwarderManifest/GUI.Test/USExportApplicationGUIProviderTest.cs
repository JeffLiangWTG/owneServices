using System;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.GUI.Test
{
	[TestedType(typeof(USExportApplicationGUIProvider))]
	sealed class USExportApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<USExportApplicationGUIProvider, USExportAsycudaManifestHeader>
	{
		protected override Type ExpectedMenuBuilderType => typeof(USExportMenuBuilder);

		protected override Type ExpectedBillLayoutType => typeof(USExportManifestBillLayouts);

		protected override USExportAsycudaManifestHeader CreateNewManifest()
		{
			var header = Factory.New<USExportAsycudaManifestHeader>();
			header.AMA_ManifestType = USExportManifestTypes.Codes.EFM;
			return header;
		}

		protected override void AssertGetPacksGridColumnsOrder(string[] columnsOrder)
		{
			CombineAssertions(() =>
			{
				AssertEquals(18, columnsOrder.Length);
				AssertContainsExactElementsInExactOrder(
					new[]
					{
						AsycudaPack.Schema.ContainerPK,
						AsycudaPack.Schema.APA_PackQty,
						AsycudaPack.Schema.APA_PackUQ,
						AsycudaPack.Schema.APA_CommodityCode,
						AsycudaPack.Schema.APA_GoodsDescription,
						AsycudaPack.Schema.APA_MarksAndNumbers,
						AsycudaPack.Schema.APA_VINNumber,
						AsycudaPack.Schema.APA_Weight,
						AsycudaPack.Schema.APA_WeightUQ,
						AsycudaPack.Schema.APA_Volume,
						AsycudaPack.Schema.APA_VolumeUQ,
						UNDGDataItemFormManager.ColumnNames.UNDGSubstanceManagerValue,
						UNDGDataItemFormManager.ColumnNames.UNDGClassManagerValue,
						USExportAsycudaPack.Schema.PSN,
						USExportAsycudaPack.Schema.ContactPK,
						USExportAsycudaPack.Schema.ContactPhone,
						USExportAsycudaPack.Schema.FlashpointTemperatureC,
						USExportAsycudaPack.Schema.FlashpointTemperatureF,
					},
					columnsOrder);
			});
		}

		public void TestGetPacksGridMandatoryOrderedColumns()
		{
			var header = CreateNewManifest();
			var applicationGUIProvider = USExportApplicationGUIProvider.GetApplicationGuiProvider(header);
			var packsGrid = applicationGUIProvider.GetPacksGridMandatoryColumns();
			AssertEquals(2, packsGrid.Length);
		}

		protected override void AssertGetPacksGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertEquals(true, columnInfos.FirstOrDefault(i => i.ColumnName == USExportAsycudaPack.Schema.PSN).IsVisible);
			AssertEquals(true, columnInfos.FirstOrDefault(i => i.ColumnName == USExportAsycudaPack.Schema.ContactPK).IsVisible);
			AssertEquals(true, columnInfos.FirstOrDefault(i => i.ColumnName == USExportAsycudaPack.Schema.ContactPhone).IsVisible);
			AssertEquals(true, columnInfos.FirstOrDefault(i => i.ColumnName == USExportAsycudaPack.Schema.FlashpointTemperatureC).IsVisible);
			AssertEquals(true, columnInfos.FirstOrDefault(i => i.ColumnName == USExportAsycudaPack.Schema.FlashpointTemperatureF).IsVisible);
		}

		public void TestBillsGridColumnAvailability()
		{
			var manifest = CreateNewManifest();
			manifest.Bills.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");

				AssertEquals("ABL_RX_NKCustomsValueCurrency IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency).IsUnavailable);
				AssertEquals("DiscountValueCurrency IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.DiscountValueCurrency).IsUnavailable);
				AssertEquals("ABL_RX_NKFreightValueCurrency IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency).IsUnavailable);
				AssertEquals("ABL_RX_NKTransportValueCurrency IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKTransportValueCurrency).IsUnavailable);
				AssertEquals("ABL_RX_NKInsuranceValueCurrency IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency).IsUnavailable);
				AssertEquals("OtherChargesValueCurrency IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.OtherChargesValueCurrency).IsUnavailable);
				AssertEquals("ABL_BolType IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_BolType).IsUnavailable);
			}
		}

		protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertContainsExactElementsInExactOrder(new[]
				{
					"AESITNNumbers",
					"InBondNumbers",
				}, columnInfos.Select(s => s.ColumnName));
		}

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[] { typeof(AsycudaPackUserControl), typeof(USExportVisitedPortUserControl) };

		protected override void SetUp()
		{
			enableExportManifest = USCustomsDataRegistry.Instance.EnableExportManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			base.SetUp();
		}

		protected override void TearDown()
		{
			enableExportManifest.Dispose();
			base.TearDown();
		}

		IDisposable enableExportManifest;
	}
}
