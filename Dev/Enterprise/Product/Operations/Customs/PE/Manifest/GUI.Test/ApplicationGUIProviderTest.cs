using System;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.PE.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.PE.Manifest.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Type ExpectedBillLayoutType => typeof(PEBillLayouts);

		protected override Type ExpectedBillPartiesLayoutType => typeof(PEBillPartiesLayouts);

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[] { typeof(AsycudaPackUserControl) };

		protected override void AssertGetPacksGridColumnsOrder(string[] columnsOrder)
		{
			AssertEquals(13, columnsOrder.Length);
		}

		protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertContainsExactElementsInExactOrder(new[] { "ABL_BillIssueDate" }, columnInfos.Select(s => s.ColumnName));
		}

		public void TestBillsGridColumnAvailability()
		{
			var manifest = CreateNewManifest();
			var bill = manifest.Bills.AddNew();

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
	}
}
