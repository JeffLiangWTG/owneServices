using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class DrawbackCustomsBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			using (USDrawbackCustomsBrokerageUserControl userControl = new USDrawbackCustomsBrokerageUserControl())
			{
				AssertEquals("MessagesTabPage.Text", "Messages", userControl.MessagesTabPage.Text);
				AssertEquals("InvoiceLinesTabPage.Text", "Lines", userControl.InvoiceLinesTabPage.Text);
				Assert("InvoicesTabPage not visible", !userControl.InvoicesTabPage.TabRelevant);
				Assert("InvoiceGroupingTabPage not visible", !userControl.InvoiceGroupingTabPage.TabRelevant);
			}
		}

		public void TestGetMessageUserControl()
		{
			using (ZForm form = new ZForm(Declaration))
			using (DrawbackCustomsBrokerageUserControlForTest userControl = new DrawbackCustomsBrokerageUserControlForTest())
			{
				userControl.JobDeclaration = Declaration;
				form.Controls.Add(userControl);
				form.Show();
				userControl.MainTabControl.SelectedTab = userControl.MessagesTabPage;
				AssertEquals("Message Tab should be default", typeof(ImportMessagesUserControl), userControl.MessageUserControl.GetType());
			}
		}

		public void TestInvoiceLineUserControl()
		{
			using (ZForm form = new ZForm(Declaration))
			using (USDrawbackCustomsBrokerageUserControl userControl = new USDrawbackCustomsBrokerageUserControl())
			{
				userControl.JobDeclaration = Declaration;
				form.Controls.Add(userControl);
				form.Show();
				userControl.MainTabControl.SelectedTab = userControl.InvoiceLinesTabPage;
				AssertEquals("Drawback Invoice Line is expected", typeof(DrawbackInvoiceLineUserControl), userControl.InvoiceLinesUserControl.GetType());
			}
		}

		public void TestDeclarationUserControlForDrawback()
		{
			using (ZForm form = new ZForm(Declaration))
			using (USDrawbackCustomsBrokerageUserControl userControl = new USDrawbackCustomsBrokerageUserControl())
			{
				userControl.JobDeclaration = Declaration;
				form.Controls.Add(userControl);
				form.Show();
				userControl.MainTabControl.SelectedTab = userControl.DeclarationTabPage;
				AssertEquals("Drawback declaration control is expected", typeof(DrawbackJobDeclarationUserControl), userControl.DeclarationUserControl.GetType());
			}
		}

		public void TestTabVisibilityForDrawback()
		{
			using (var form = new ZForm(Declaration))
			using (var userControl = new USDrawbackCustomsBrokerageUserControl())
			{
				userControl.JobDeclaration = Declaration;
				form.Controls.Add(userControl);
				form.Show();
				Assert("MiscOptionsTabPage is not visible", !userControl.MiscOptionsTabPage.TabVisible);
				Assert("StatusTabPage is not visible", !userControl.StatusTabPage.TabVisible);
				Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				form.Show();
				Assert("MiscOptionsTabPage is visible", userControl.MiscOptionsTabPage.TabVisible);
				Assert("StatusTabPage is visible", userControl.StatusTabPage.TabVisible);
			}
		}

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.US_EntryFilerCode = "XJS";
					declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				}

				return declaration;
			}
		}

		sealed class DrawbackCustomsBrokerageUserControlForTest : USDrawbackCustomsBrokerageUserControl
		{
			internal BaseCustomsEntryUserControl GetDeclarationUserControlInternal() => GetDeclarationUserControl();

			internal BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControlInternal() => GetSupplierHeaderUserControl();

			internal BaseInvoiceLineUserControl GetInvoiceLinesUserControlInternal() => GetInvoiceLinesUserControl();

			internal BaseCustomsEntryUserControl GetMessageUserControlInternal() => GetMessageUserControl();
		}
	}
}
