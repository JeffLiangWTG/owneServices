using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class CustomsBillsUserControlTest : TestCaseWithFactory
	{
		public void TestInitLayout()
		{
			using (TWCustomsDataRegistry.Instance.EnableCustomsDeclarationPackingList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					Application.DoEvents();
					var customsBrokerageUserControl = form.CustomsBrokerageUserControl;
					customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.PackingTabPage;
					var packingDetailsGroupBox = customsBrokerageUserControl.FindSingle<ZGroupBox>("PackingDetailsGroupBox");
					var houseBillsGrid = customsBrokerageUserControl.FindSingle<ZGrid>("HouseBillsGrid");
					Assert("CustomsBillsUserControl should hide the packingDetailsGroupBox.", !packingDetailsGroupBox.Visible);
					AssertEquals("The dock style of HouseBillsGrid should be fill", DockStyle.Fill, houseBillsGrid.Dock);
				}
			}

			using (TWCustomsDataRegistry.Instance.EnableCustomsDeclarationPackingList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					Application.DoEvents();
					var customsBrokerageUserControl = form.CustomsBrokerageUserControl;
					customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.PackingTabPage;
					var packingDetailsGroupBox = customsBrokerageUserControl.FindSingle<ZGroupBox>("PackingDetailsGroupBox");
					var houseBillsGrid = customsBrokerageUserControl.FindSingle<ZGrid>("HouseBillsGrid");
					Assert("CustomsBillsUserControl should display the packingDetailsGroupBox.", packingDetailsGroupBox.Visible);
					AssertEquals("The dock style of HouseBillsGrid should be top", DockStyle.Top, houseBillsGrid.Dock);
				}
			}
		}
	}
}
