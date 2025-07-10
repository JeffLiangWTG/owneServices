using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.CusTempStorage;
using Enterprise.Customs.PL.GUI.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

class TemporyStorageUserControlForPluginTest : TestCaseWithFactory
{
	public void TestNoCurrentDataItem()
	{
		using (var frm = new ZForm())
		{
			var userControl = new TemporyStorageUserControlForPlugin();
			frm.Controls.Add(userControl);
			frm.Show();
			var coveringLabel = userControl.FindSingle<ZLabel>(c => c.Name == "CoveringLabel");

			CombineAssertions(() =>
			{
				AssertEquals("CoveringLabel should not be visible", false, coveringLabel.Visible);
				SelectDeclarationTabPage(frm);
				AssertEquals("CoveringLabel should be visible", true, coveringLabel.Visible);
				AssertEquals("CoveringLabel text", string.Format("You have chosen not to create a Declaration now.\r\nPlease change to another tab. You can click back to this tab when/if you need to create a Declaration."), coveringLabel.Text);
			});
		}
	}

	public void TestDecPanelVisible()
	{
		using (var frm = new ZForm())
		{
			var userControl = new TemporyStorageUserControlForPlugin();
			frm.Controls.Add(userControl);
			frm.Show();
			userControl.SetDataBinding(header, ZString.Empty);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			SelectDeclarationTabPage(frm);
			Assert(!userControl.FindSingle<ZLabel>(c => c.Name == "CoveringLabel").Visible);
			var decPanel = userControl.FindSingle<ZPanel>(c => c.Name == "DeclarationPanel");
			CombineAssertions(() =>
			{
				AssertNotNull("Panel should exist", decPanel);
				AssertEquals("Panel should be visible", true, decPanel.Visible);
			});
		}
	}

	void SelectDeclarationTabPage(ZForm frm)
	{
		var tabControl2 = frm.FindSingle<ZTabControl>(c => c.Name == "MainTabControl");
		var tabPage2 = frm.FindSingle<ZTabPage>(c => c.Name == "EntrySummaryDeclarationTabPage");
		tabControl2.SelectedTab = tabPage2;
		var tabControl = frm.FindSingle<ZTabControl>(c => c.Name == "EntrySummaryDeclarationTabControl");
		var tabPage = frm.FindSingle<ZTabPage>(c => c.Name == "DeclarationTabPage");
		tabControl.SelectedTab = tabPage;
	}

	protected override void SetUp()
	{
		base.SetUp();
		var customerOrg = Factory.NewWithValidTestData<OrgHeader>();
		header = CusTempStorageJobHeader.New(Factory);
		header.SJH_OH_Customer = customerOrg.PK;
	}
	CusTempStorageJobHeader header;
}
