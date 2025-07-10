using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	public class CustomsBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestPlugins()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (ZForm form = new ZForm(declaration))
			{
				CustomsBrokerageUserControl userControl = new CustomsBrokerageUserControl();
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();
				AssertNotNull("Should have MAFeBACCaPlugIn.", userControl.MainTabControl.PlugIns.GetPlugIn(Enterprise.ZArchitecture.Modules.ControllerIDs.Customs.NZ.MAFeBACCaDeclarationPlugin));
			}
		}

		public void TestMergeDoesNotSelectEntriesTab()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			TestFormalEntryCreator decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			using (CustomsBrokerageUserControl userControl = new CustomsBrokerageUserControl())
			{
				userControl.JobDeclaration = declaration;
				AssertEquals("SelectedTab.Name", userControl.DeclarationTabPage.Name, userControl.MainTabControl.SelectedTab.Name);
				decCreator.MergeDeclaration();
				AssertEquals("SelectedTab.Name", userControl.DeclarationTabPage.Name, userControl.MainTabControl.SelectedTab.Name);
			}
		}

		public void TestWarningMessageIfMiscOrganizationDoesNotExist()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (ZForm form = new ZForm(declaration))
			{
				CustomsBrokerageUserControl userControl = new CustomsBrokerageUserControl();
				userControl.ReturnEmptyMiscOrganisation = true;
				userControl.JobDeclaration = declaration;
				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Controls.Add(userControl);
				form.Show();
				AssertEquals("LastMessage.Text", CustomsBrokerageUserControl.MiscOrganizaionDoesNotExistWarningMessage, Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestWarningMessageIfMiscOrganizationExists()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (ZForm form = new ZForm(declaration))
			{
				CustomsBrokerageUserControl userControl = new CustomsBrokerageUserControl();
				userControl.ReturnEmptyMiscOrganisation = false;
				userControl.JobDeclaration = declaration;
				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Controls.Add(userControl);
				form.Show();
				AssertEquals("LastMessage.Text", null, Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
