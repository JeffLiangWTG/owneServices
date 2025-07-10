using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.EConversation.GUI;
using Enterprise.ZArchitecture.GUI;

using static Enterprise.Recruitment.Testing.RecruitmentDataHelpers;

namespace Enterprise.Recruitment.Testing.Module
{
	sealed class CandidateEConversationControlTest : TestCaseWithFactory
	{
		public void TestAddNote()
		{
			var boris = CreateCandidate(Factory, "Borris Johnson");
			Factory.Save();

			using (var form = GetFormToBash(out var control))
			{
				var testMsg = "test string";
				form.Show();
				Application.DoEvents();

				control.SetDataBinding(boris.EConversation, string.Empty);
				Application.DoEvents();

				var messages = boris.EConversation.RootConversation.Messages;
				AssertEquals("Candidate created", messages.Last().JCM_Body); //Default Message

				var textbox = (ZAutoCompleteTextBox)control.Controls.Find("econversationMessageTextBox", true).Single();
				textbox.Text = testMsg;

				var sendButton = (ZButton)control.Controls.Find("AddInternalCommentButton", true).Single();
				sendButton.PerformClick();

				AssertEquals(2, messages.Count);
				Assert(messages.Last().JCM_IsInternal);
				AssertEquals(testMsg, messages.First().JCM_Body);
				Assert(string.IsNullOrEmpty(textbox.Text));
			}
		}

		public void TestAddNote_HotKey()
		{
			var boris = CreateCandidate(Factory, "Borris Johnson");
			Factory.Save();

			using (var form = GetFormToBash(out var control))
			{
				var testMsg = "test string";
				form.Show();
				Application.DoEvents();

				control.SetDataBinding(boris.EConversation, string.Empty);
				Application.DoEvents();

				var messages = boris.EConversation.RootConversation.Messages;
				AssertEquals("Candidate created", messages.Last().JCM_Body); //Default Message

				var textbox = (ZAutoCompleteTextBox)control.Controls.Find("econversationMessageTextBox", true).Single();
				textbox.Text = testMsg;

				textbox.Hotkeys.ProcessCmdKey(textbox, Keys.Control | Keys.Enter);

				AssertEquals(2, messages.Count);
				Assert(messages.Last().JCM_IsInternal);
				AssertEquals(testMsg, messages.First().JCM_Body);
				Assert(string.IsNullOrEmpty(textbox.Text));
			}
		}

		public void TestTemplatesExist()
		{
			using (var form = GetFormToBash(out var control))
			{
				form.Show();
				Application.DoEvents();

				var eConversationTextBox = (ZAutoCompleteTextBox)control.Controls.Find("econversationMessageTextBox", true).FirstOrDefault();
				var templateMenu = (ToolStripMenuItem)eConversationTextBox.ContextMenuStrip.Items[0];

				AssertEquals("Insert Template should be the first control in the context menu", "Insert Template", templateMenu.Text);
				AssertEquals("Insert Template menu item should be visible", true, templateMenu.Available);
			}
		}

		public void TestDisabledInitially()
		{
			var boris = CreateCandidate(Factory, "Borris Johnson");
			Factory.Save();

			using (var form = GetFormToBash(out var control))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(false, control.Enabled);
			}
		}

		public void TestDisabling_BindingNull()
		{
			var boris = CreateCandidate(Factory, "Borris Johnson");
			Factory.Save();

			using (var form = GetFormToBash(out var control))
			{
				form.Show();
				Application.DoEvents();

				control.SetDataBinding(boris.EConversation, string.Empty);
				Assert(control.Enabled);

				control.SetDataBinding(null, string.Empty);
				Assert(!control.Enabled);
			}
		}

		public void TestClearPreviousText()
		{
			var boris = CreateCandidate(Factory, "Borris Johnson");
			var chris = CreateCandidate(Factory, "Chris Johnson");

			Factory.Save();

			using (var form = GetFormToBash(out var control))
			{
				var testMsg = "test string";
				form.Show();
				Application.DoEvents();

				control.SetDataBinding(boris.EConversation, string.Empty);
				Application.DoEvents();

				var messages = boris.EConversation.RootConversation.Messages;
				AssertEquals("Candidate created", messages.Last().JCM_Body); //Default Message

				var textbox = (ZAutoCompleteTextBox)control.Controls.Find("econversationMessageTextBox", true).Single();
				textbox.Text = testMsg;

				control.SetDataBinding(chris.EConversation, string.Empty);
				Assert(string.IsNullOrEmpty(textbox.Text));
			}
		}

		Form GetFormToBash(out CandidateEConversationControl control)
		{
			var form = new ZChildForm();
			form.Controls.Add(control = new CandidateEConversationControl { Dock = DockStyle.Fill });
			return form;
		}
	}
}
