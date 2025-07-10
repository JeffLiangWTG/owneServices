using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.Manifest.GUI.Testing
{
	public class VisitedPortsForBillUserControlTest : TestCaseWithFactory
	{
		public void TestIAdditionalTabPageMembersForVisitedPortsOnBillLevel()
		{
			var header = Factory.NewWithValidTestData<Business.AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			var bill = header.Bills.AddNew();
			using (var form = new ASYCUDA.GUI.ManifestForm(header))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<ASYCUDA.GUI.AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var additionalTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_VisitedPortsForBillUserControl");
				billsAndPacksTabControl.SelectedTab = additionalTabPage;
				AssertEquals("Visited Ports", additionalTabPage.CaptionResourceString.Caption);
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				Assert(additionalTabPage.TabVisible);
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				Assert(additionalTabPage.TabVisible);
			}
		}
	}
}
