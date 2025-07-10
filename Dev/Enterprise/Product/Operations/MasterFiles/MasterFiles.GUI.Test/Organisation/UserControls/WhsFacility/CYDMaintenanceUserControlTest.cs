using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing.Organisation.UserControls.WhsFacility
{
	public class CYDMaintenanceUserControlTest : TestCaseWithFactory
	{
		public void TestAllControlsAreVisible()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (var form = new ZOrganisationsForm(orgHeader))
			using (var maintenanceUserControl = new CYDMaintenanceUserControl())
			{
				form.Controls.Add(maintenanceUserControl);
				form.Show();
				var mnrGroupBoxControl = maintenanceUserControl.FindSingle<ZGroupBox>("MNRGroupBox");
				var cedexRadioButton = maintenanceUserControl.FindSingle<ZRadioButton>("CedexRadioButton");
				var mercRadioButton = maintenanceUserControl.FindSingle<ZRadioButton>("MercRadioButton");
				AssertEquals(true, mnrGroupBoxControl.Visible);
				AssertEquals(true, cedexRadioButton.Visible);
				AssertEquals(true, mercRadioButton.Visible);
			}	
		}

		public void TestCedexRadioButton()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (var form = new ZOrganisationsForm(orgHeader))
			using (var maintenanceUserControl = new CYDMaintenanceUserControl())
			{
				form.Controls.Add(maintenanceUserControl);
				form.Show();
				var cedexRadioButton = maintenanceUserControl.FindSingle<ZRadioButton>("CedexRadioButton");
				var mercRadioButton = maintenanceUserControl.FindSingle<ZRadioButton>("MercRadioButton");
				AssertEquals(true, cedexRadioButton.Checked);
				AssertEquals(false, mercRadioButton.Checked);
			}
		}

		public void TestMercRadioButton()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CompanyData.OB_YardMNRCodeGroup = "MRC";
			Factory.Save();

			using (var form = new ZOrganisationsForm(orgHeader))
			using (var maintenanceUserControl = new CYDMaintenanceUserControl())
			{
				form.Controls.Add(maintenanceUserControl);
				form.Show();
				var cedexRadioButton = maintenanceUserControl.FindSingle<ZRadioButton>("CedexRadioButton");
				var mercRadioButton = maintenanceUserControl.FindSingle<ZRadioButton>("MercRadioButton");
				AssertEquals(false, cedexRadioButton.Checked);
				AssertEquals(true, mercRadioButton.Checked);
			}
		}
	}
}
