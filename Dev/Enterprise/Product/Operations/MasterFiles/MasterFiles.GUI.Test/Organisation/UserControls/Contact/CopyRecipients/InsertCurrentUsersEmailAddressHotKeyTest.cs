using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class InsertCurrentUsersEmailAddressHotKeyTest : TestCaseWithFactory
	{
		public void TestTestRegisterAddEmailAddressHotKey()
		{
			var email = "blah@blah.org";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = email;
			Factory.Save();

			using (Env.Instance.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var m = new Message();
				using (var columnStyle = new AddressOverrideColumnStyle<StmScheduleTaskCopyRecipient, StmScheduleTaskCopyRecipientCollection, StmScheduleTaskRecipient>(new AddressOverrideColumnStyleInfo<StmScheduleTaskCopyRecipient, StmScheduleTaskCopyRecipientCollection, StmScheduleTaskRecipient>()))
				{
					columnStyle.ProcessCmdKey(ref m, Keys.Control | Keys.E);
					AssertEquals(email, ((AddressOverrideCombinationControl<StmScheduleTaskCopyRecipient, StmScheduleTaskCopyRecipientCollection, StmScheduleTaskRecipient>)columnStyle.EditControl).CurrentEditor.Text);
				}

				using (var columnStyle = new NonPersistentAddressOverrideColumnStyle<DocDeliveryContact>(new NonPersistentAddressOverrideColumnStyleInfo<DocDeliveryContact>()))
				{
					columnStyle.ProcessCmdKey(ref m, Keys.Control | Keys.E);
					AssertEquals(email, ((NonPersistentAddressOverrideCombinationControl<DocDeliveryContact>)columnStyle.EditControl).CurrentEditor.Text);
				}
			}
		}

		public void TestHotKeyWithHighlightedText()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "blah@blah.org";
			Factory.Save();

			using (Env.Instance.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var m = new Message();
				using (var columnStyle = new AddressOverrideColumnStyle<StmScheduleTaskCopyRecipient, StmScheduleTaskCopyRecipientCollection, StmScheduleTaskRecipient>(new AddressOverrideColumnStyleInfo<StmScheduleTaskCopyRecipient, StmScheduleTaskCopyRecipientCollection, StmScheduleTaskRecipient>()))
				{
					var control = ((AddressOverrideCombinationControl<StmScheduleTaskCopyRecipient, StmScheduleTaskCopyRecipientCollection, StmScheduleTaskRecipient>)columnStyle.EditControl).CurrentEditor;

					control.Text = "blah@blah.org";
					control.SelectionStart = 0;
					control.SelectionLength = 13;
					columnStyle.ProcessCmdKey(ref m, Keys.Control | Keys.E);
					AssertEquals("blah@blah.org", control.Text);

					control.Text = "blah@blah.org, test@test.com";
					control.SelectionStart = 0;
					control.SelectionLength = 28;
					columnStyle.ProcessCmdKey(ref m, Keys.Control | Keys.E);
					AssertEquals("blah@blah.org", control.Text);

					control.Text = "test2@test.com, test3@test.com";
					control.SelectionStart = 16;
					control.SelectionLength = 14;
					columnStyle.ProcessCmdKey(ref m, Keys.Control | Keys.E);
					AssertEquals("test2@test.com, blah@blah.org", control.Text);

					control.Text = "test4@test.com, test5@test.com";
					control.SelectionStart = 0;
					control.SelectionLength = 14;
					columnStyle.ProcessCmdKey(ref m, Keys.Control | Keys.E);
					AssertEquals("blah@blah.org, test5@test.com", control.Text);

					AssertEquals(control.SelectionStart, control.Text.Length);

					control.Text = "test6@test.com, blah@blah.org";
					control.SelectionStart = 0;
					control.SelectionLength = 14;
					columnStyle.ProcessCmdKey(ref m, Keys.Control | Keys.E);
					AssertEquals("test6@test.com, blah@blah.org", control.Text);

					AssertEquals(control.SelectionStart, 0);
				}
			}
		}
	}
}
