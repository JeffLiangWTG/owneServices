using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ReSequencerForm))]
	class ReSequencerFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using (var form = new ReSequencerForm(ReSequencer))
			{
				form.Show();
				AssertEquals("Re-Sequencer", form.FormCaption);
			}
		}

		public void TestCancelButton()
		{
			using (var form = new ReSequencerForm(ReSequencer))
			{
				var isClosed = false;
				form.FormClosed += (object sender, FormClosedEventArgs e) =>
				{
					isClosed = true;
				};
				form.Show();
				form.DialogResult = DialogResult.None;
				var button = form.FindSingle<ZButton>("cancelButton");
				button.PerformClick();
				AssertEquals("isClosed", true, isClosed);
				AssertEquals("form.DialogResult", DialogResult.Cancel, form.DialogResult);
			}
		}

		public void TestOKButton()
		{
			using (var form = new ReSequencerForm(ReSequencer))
			{
				var isClosed = false;
				form.FormClosed += (object sender, FormClosedEventArgs e) =>
				{
					isClosed = true;
				};
				form.Show();
				form.DialogResult = DialogResult.None;
				var button = form.FindSingle<ZButton>("OKButton");
				ReSequencer.StartSequence = 0;
				ReSequencer.SequenceStep = 1;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				button.PerformClick();
				AssertEquals("isClosed", false, isClosed);
				AssertEquals("form.DialogResult", DialogResult.None, form.DialogResult);
				AssertEquals("Please fix all errors before proceeding.", UnitTestUserNotification.Instance.LastMessage.Text);
				ReSequencer.StartSequence = 1;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				button.PerformClick();
				AssertEquals("isClosed", true, isClosed);
				AssertEquals("form.DialogResult", DialogResult.OK, form.DialogResult);
				AssertNull("Please fix all errors before proceeding.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation

		ReSequencer ReSequencer => reSequencer ?? (reSequencer = new ReSequencer(new[] { new Business.Testing.SequenceNumberForTesting() { SequenceNumber = 1 }, new Business.Testing.SequenceNumberForTesting() { SequenceNumber = 2 } }, Factory));
		ReSequencer reSequencer;

		protected override Form GetFormToBashCore()
		{
			return new ReSequencerForm(ReSequencer);
		}

		#endregion
	}
}
