using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.GUI.Testing
{
	class SailingUserControlTest : TestCaseWithFactory
	{
		public void TestEditSailingButton()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var header = Factory.New<CusInBondHeader>();
			using (var form = new USAMSForm(header))
			{
				form.Show();
				form.sailingUserControl.EditSailingButton.Click += new System.EventHandler(form.sailingUserControl.EditSailingButton_Click_ForTest);
				var editSailingBtn = form.Controls.Find("EditSailingButton", true)[0] as ZButton;
				editSailingBtn.PerformClick();
				Application.DoEvents();
				AssertEquals(SailingUserControl.SailingScheduleNotCreated, UnitTestUserNotification.Instance.LastMessage.Text);
				var sailing = Factory.NewWithValidTestData<JobSailing>();
				header.BH_ParentID = sailing.PK;
				header.BH_ParentTableCode = JobSailingSchema.Constants.Prefix;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				editSailingBtn.PerformClick();
				Application.DoEvents();
				AssertNotEquals(SailingUserControl.SailingScheduleNotCreated, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertType(typeof(ZJobVoyageForm), form.sailingUserControl.LastOpenedFormForTest);
				form.sailingUserControl.LastOpenedFormForTest.Dispose();
			}
		}

		public void TestClearSailingButton()
		{
			var header = Factory.New<CusInBondHeader>();
			using (var form = new USAMSForm(header))
			{
				form.Show();
				var clearSailingBtn = form.Controls.Find("ClearSailingButton", true)[0] as ZButton;
				clearSailingBtn.PerformClick();
				Application.DoEvents();
				AssertNull(header.Sailing);
			}
		}

		public void TestCanChangeSailing()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var header = Factory.New<CusInBondHeader>();
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			header.ChangeSailing(sailing.PK);
			header.MovementHeader.Messages.AddNew();
			using (var form = new USAMSForm(header))
			{
				form.Show();
				var selectSailingBtn = form.Controls.Find("SelectSailingButton", true)[0] as ZButton;
				selectSailingBtn.PerformClick();
				Application.DoEvents();
				AssertEquals(SailingUserControl.CannotChangeSailingMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var clearSailingBtn = form.Controls.Find("ClearSailingButton", true)[0] as ZButton;
				clearSailingBtn.PerformClick();
				Application.DoEvents();
				AssertEquals(SailingUserControl.CannotChangeSailingMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestATDDateEdit()
		{
			var header = Factory.New<CusInBondHeader>();
			using (var form = new USAMSForm(header))
			{
				form.Show();
				var aTDDateEdit = form.Controls.Find("SailingATDDateEdit", true)[0] as ZDateEdit;
				AssertNotNull(aTDDateEdit);
				Assert("ATD should be editable", !aTDDateEdit.ReadOnly);
			}

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			header.ChangeSailing(sailing.PK);
			using (var form = new USAMSForm(header))
			{
				form.Show();
				var aTDDateEdit = form.Controls.Find("SailingATDDateEdit", true)[0] as ZDateEdit;
				AssertNotNull(aTDDateEdit);
				Assert("ATD should be editable", !aTDDateEdit.ReadOnly);
			}
		}
	}
}
