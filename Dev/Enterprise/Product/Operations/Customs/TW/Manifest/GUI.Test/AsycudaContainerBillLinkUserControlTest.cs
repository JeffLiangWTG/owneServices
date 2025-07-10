using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TW.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.TW.Manifest.GUI.Testing
{
	sealed class AsycudaContainerBillLinkUserControlTest : TestCaseWithFactory
	{
		public void TestIAdditionalTabPageMembersForVisitedPortsOnBillLevel()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "MAN";
			header.AMA_ApplicationCode = RefCusCodeListTypes.Codes.NVC;
			var bill = header.Bills.AddNew();

			using (var form = new ManifestForm(header))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");

				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var additionalTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_AsycudaContainerBillLinkUserControl");
				billsAndPacksTabControl.SelectedTab = additionalTabPage;
				AssertEquals("Containers", additionalTabPage.CaptionResourceString.Caption);

				var asycudaContainerBillLinkUserControl = additionalTabPage.FindSingle<AsycudaContainerBillLinkUserControl>(c => c.Name == "AsycudaContainerBillLinkUserControl");
				var asycudaBillLinkAsycudaContainersGrid = asycudaContainerBillLinkUserControl.FindSingle<ZGrid>(c => c.Name == "AsycudaBillLinkAsycudaContainersGrid");
				var actualList = asycudaBillLinkAsycudaContainersGrid.Columns.Cast<ZGridColumn>().Select(x => x.ColumnName);
				var expectedList = GetExpectedPacksGridColumnNames();
				AssertContainsExactElementsInExactOrder(expectedList, actualList);
			}
		}

		string[] GetExpectedPacksGridColumnNames()
		{
			return new string[]
			{
				AsycudaBillLinkAsycudaContainer.Schema.Link,
				AsycudaBillLinkAsycudaContainer.Schema.ContainerNumber,
				AsycudaBillLinkAsycudaContainer.Schema.ContainerType,
				AsycudaBillLinkAsycudaContainer.Schema.ContainerMode,
				AsycudaBillLinkAsycudaContainer.Schema.Seal1,
				AsycudaBillLinkAsycudaContainer.Schema.Seal2,
				AsycudaBillLinkAsycudaContainer.Schema.Seal3,
			};
		}
	}
}
