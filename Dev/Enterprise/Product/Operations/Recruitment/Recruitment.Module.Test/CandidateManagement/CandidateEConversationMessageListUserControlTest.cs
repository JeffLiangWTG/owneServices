using System.Linq;
using System.Windows.Forms;
using Enterprise.EConversation.GUI;
using Enterprise.EConversation.Testing.GUI;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Module;
using Enterprise.ZArchitecture.GUI;

using static Enterprise.Recruitment.Testing.RecruitmentDataHelpers;

namespace Enterprise.Recruitment.Testing.Module
{
	sealed class CandidateEConversationMessageListUserControlTest : EConversationMessageListUserControlTest
	{
		public void TestBindingNullClearsControls()
		{
			var candidate = CreateCandidate(Factory, "Borris Johnson");
			var conversation = CreateConversation(candidate, "borris@gmail.com", "recruiter1@wisetech.com");
			conversation.AddMessageFromCurrentUser("Test", isInternal: false);
			Factory.Save();

			using (ShowFormWithControl(candidate.EConversation, out var control))
			{
				var messageLayoutPanel = (DoubleBufferedStackLayoutPanel)control.Controls.Find("messagesLayoutPanel", true).Single();
				var messagebox = FindMessageBoxWithText(control, "bodyTextbox", "Test");

				AssertEquals(5, messageLayoutPanel.Controls.Count); //Date, Candidate created, borris added to conversation, recruter1 added to conversation and Test
				AssertNotNull(messagebox);

				control.SetDataBinding(null, string.Empty);
				messagebox = FindMessageBoxWithText(control, "bodyTextbox", "Test");
				AssertEquals(0, messageLayoutPanel.Controls.Count);
				AssertNull(messagebox);
			}
		}

		public void TestBinding()
		{
			var candidate1 = CreateCandidate(Factory, "Boris Johnson");
			var candidate2 = CreateCandidate(Factory, "Chris Johnson");

			var conversation1 = CreateConversation(candidate1, "borris@gmail.com", "recruiter1@wisetech.com");
			var conversation2 = CreateConversation(candidate2, "chris@gmail.com", "recruiter2@wisetech.com");

			conversation1.AddMessageFromCurrentUser("Test1", isInternal: false);
			conversation2.AddMessageFromCurrentUser("Test2", isInternal: false);

			Factory.Save();

			using (ShowFormWithControl(candidate1.EConversation, out var control))
			{
				var messagebox1 = FindMessageBoxWithText(control, "bodyTextbox", "Test1");
				AssertNotNull(messagebox1);

				control.SetDataBinding(candidate2.EConversation, string.Empty);
				var messagebox2 = FindMessageBoxWithText(control, "bodyTextbox", "Test2");
				AssertNotNull(messagebox2);
			}
		}

		static ZForm ShowFormWithControl(GroupedEConversation convo, out CandidateEConversationMessageListUserControl control)
		{
			var form = new ZForm(convo);
			var eConv = new CandidateEConversationControl { Dock = DockStyle.Fill };

			form.Controls.Add(eConv);
			form.Show();

			control = (CandidateEConversationMessageListUserControl)eConv.Controls.Find("chatboxControl", true).Single();

			return form;
		}

		Control FindMessageBoxWithText(Control control, string controlName, string testMessage) => control.Controls.Find(controlName, true).FirstOrDefault(m => Equals(m.Text, testMessage));
	}
}
