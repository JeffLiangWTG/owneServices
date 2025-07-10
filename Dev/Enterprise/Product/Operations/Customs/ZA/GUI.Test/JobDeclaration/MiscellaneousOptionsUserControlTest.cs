using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	sealed class MiscellaneousOptionsUserControlTest : TestCaseWithFactory
	{
		public void TestBOESightGroupBoxVisibleWhenIsImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (JobDeclarationForm testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageUserControl = testForm.CustomsBrokerageUserControl;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.DeclarationTabPage;
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.MiscOptionsTabPage;
				var userControl = brokerageUserControl.MiscOptions as MiscOptionsUserControl;
				AssertEquals(false, userControl.IsDisposed);
				AssertEquals("BOE Sight Group Box should not be visible for Export", false, userControl.BOESightGroupBox.Visible);
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.DeclarationTabPage;
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.MiscOptionsTabPage;
				userControl = brokerageUserControl.MiscOptions as MiscOptionsUserControl;
				AssertEquals(false, userControl.IsDisposed);
				AssertEquals("BOE Sight Group Box should be visible for Import", true, userControl.BOESightGroupBox.Visible);
			}
		}
	}
}
