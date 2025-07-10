using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TR.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.Manifest.GUI.Testing
{
	class TRBillPartiesCountrySpecificUserControlTest : TestCaseWithFactory
	{
		public void TestTRCountrySpecificFieldsVisibility()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var billPartiesTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billPartiesTabPage");
				billsAndPacksTabControl.SelectedTab = billPartiesTabPage;
				var toOrderCheckBox = billsAndPacksTabControl.Controls.Find("ToOrderCheckBox", true).FirstOrDefault();
				Assert(toOrderCheckBox.Visible);
				var notOwnedCheckBox = billsAndPacksTabControl.Controls.Find("NotOwnedCheckBox", true).FirstOrDefault();
				Assert(notOwnedCheckBox.Visible);
			}
		}
	}
}
