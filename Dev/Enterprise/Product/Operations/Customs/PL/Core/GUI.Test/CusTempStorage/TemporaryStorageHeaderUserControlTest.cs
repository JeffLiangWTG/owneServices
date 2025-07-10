using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.CusTempStorage;
using Enterprise.Customs.PL.GUI.CusTempStorage;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

class TemporaryStorageHeaderUserControlTest : TestCaseWithFactory
{
	public void TestControls()
	{
		var header = CusTempStorageJobHeader.New(Factory);
		using (var userControl = new TemporaryStorageHeaderUserControl())
		{
			using (var form = new ZForm(header))
			{
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("CustomersReferenceTextBox", true, userControl.FindSingleOrDefault<ZTextBox>("CustomersReferenceTextBox").Visible);
					AssertEquals("CustomerOrganisation", true, userControl.FindSingleOrDefault<ZOrganisationControl>("CustomerOrganisation").Visible);
					AssertEquals("PresenterOrgAddress", true, userControl.FindSingleOrDefault<ZOrgAddressControl>("PresenterOrgAddress").Visible);
					AssertEquals("RepresentativeOrgAddress", true, userControl.FindSingleOrDefault<ZOrgAddressControl>("RepresentativeOrgAddress").Visible);
					AssertNotNull("CustomsDetailsUserControl", userControl.FindSingle<CustomsDetailsUserControl>("CustomsDetailsUserControl"));
					AssertNotNull("TransportDetailsUserControl", userControl.FindSingle<CusTempStorage.TransportDetailsUserControl>("TransportDetailsUserControl"));
				});
			}
		}
	}
}
