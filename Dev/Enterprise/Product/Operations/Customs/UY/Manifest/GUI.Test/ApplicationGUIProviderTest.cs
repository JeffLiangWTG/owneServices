using System;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.UY.Manifest.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[] { typeof(AsycudaPackUserControl) };

		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Type ExpectedBillLayoutType => typeof(UYBillLayouts);

		protected override Type ExpectedBillPartiesLayoutType => typeof(UYBillPartiesLayouts);

		protected override void AssertGetPacksGridColumnsOrder(string[] columnsOrder)
		{
			AssertEquals(12, columnsOrder.Length);
		}

		public void TestUYSpecificFiedsVisibilty()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.Packs.AddNew();

			Factory.Save();

			using (var form = new ManifestForm(header))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingleOrDefault<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var packsTabPage = billsAndPacksTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_AsycudaPackUserControl");
				billsAndPacksTabControl.SelectedTab = packsTabPage;
				var asycudaPackUserControl = packsTabPage.FindSingleOrDefault<AsycudaPackUserControl>(c => c.Name == "AsycudaPackUserControl");
				var packsGrid = asycudaPackUserControl.FindSingle<ZGrid>(c => c.Name == "PacksGrid");

				AssertEquals(true, packsGrid.GetColumnStyle(Business.AsycudaPack.Schema.APA_ArrivedQuantity).IsVisible);
				AssertEquals(true, packsGrid.GetColumnStyle(Business.AsycudaPack.Schema.APA_ArrivedWeight).IsVisible);
			}
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

				AssertEquals("ABL_BolType IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_BolType).IsUnavailable);
			}
		}

		protected override void AssertGetPacksGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			var expectedColumns = new[] { "APA_ArrivedQuantity", "APA_ArrivedWeight" };
			AssertContainsExactElementsInExactOrder(expectedColumns, columnInfos.Select(s => s.ColumnName));
		}
	}
}
