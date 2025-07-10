using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	sealed class CustomsBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestEntryInstructionsTabPage()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (ZForm form = new ZForm(declaration))
			using (CustomsBrokerageUserControl userControl = new CustomsBrokerageUserControl())
			{
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();
				AssertEquals("EntryInstructionDetailsUserControl not contructed", 0, userControl.EntryInstructionDetailsTabPage.Controls.OfType<EntryInstructionDetailsUserControl>().Count());
				userControl.MainTabControl.SelectedTab = userControl.EntryInstructionDetailsTabPage;
				AssertEquals("EntryInstructionDetailsUserControl is contructed", 1, userControl.EntryInstructionDetailsTabPage.Controls.OfType<EntryInstructionDetailsUserControl>().Count());
			}
		}

		public void TestDeclarationUserControlNotExWarehouse()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			using (ZForm form = new ZForm(declaration))
			using (CustomsBrokerageUserControl userControl = new CustomsBrokerageUserControl())
			{
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();
				userControl.MainTabControl.SelectedTab = userControl.DeclarationTabPage;
				AssertEquals("Normal declaration control is expected", typeof(ZADeclarationUserControl), userControl.DeclarationUserControl.GetType());
			}
		}

		public void TestDeclarationUserControlForExWarehouse()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			using (ZForm form = new ZForm(declaration))
			using (CustomsBrokerageUserControl userControl = new CustomsBrokerageUserControl())
			{
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();
				userControl.MainTabControl.SelectedTab = userControl.DeclarationTabPage;
				AssertEquals("Normal declaration control is expected", typeof(ZADeclarationUserControl), userControl.DeclarationUserControl.GetType());
			}
		}

		public void TestApplicationCodeChangeRefreshControlsOnTab()
		{
			CombineAssertions(() =>
			{
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				using (JobDeclarationForm form = new JobDeclarationForm(declaration))
				{
					var userControl = form.CustomsBrokerageUserControl;
					form.Show();
					declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
					AssertEquals("Initial Invoice Tab Controls", 0, userControl.InvoiceLinesTabPage.Controls.Count);
					userControl.MainTabControl.SelectedTab = userControl.InvoiceLinesTabPage;
					AssertNotEquals("Loaded Invoice Tab Controls", 0, userControl.InvoiceLinesTabPage.Controls.Count);
					userControl.MainTabControl.SelectedTab = userControl.DeclarationTabPage;
					AssertNotEquals("Loaded Invoice Tab Controls", 0, userControl.InvoiceLinesTabPage.Controls.Count);
					declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
					AssertEquals("Refreshed Invoice Tab Controls", 0, userControl.InvoiceLinesTabPage.Controls.Count);
				}
			});
		}
	}
}
