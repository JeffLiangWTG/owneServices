using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.GUI.Test
{
	[TestedType(typeof(CloseProjectPopupForm))]
	public class CloseProjectPopupFormTest : BaseProjectPopupFormTest
	{
		protected override Form GetFormToBashCore()
		{
			Project project = Factory.New<Project>();
			ProjectAction action = new ProjectAction(project);
			return new CloseProjectPopupForm(action);
		}

		public override void TestCloseButton()
		{
			Project project = Factory.New<Project>();
			ProjectAction action = new ProjectAction(project);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (CloseProjectPopupForm form = new CloseProjectPopupForm(action))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.CloseButton_Exposed.PerformClick();
				AssertEquals("Make sure property is empty to begin", ZString.Empty, action.CloseType);
				AssertEquals("Check for errors", true, action.CloseTypeInfo.HasNotifications());
				AssertEquals("LastMessage", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				action.CloseType = "CLS";
				form.CloseButton_Exposed.PerformClick();
				AssertEquals("Check for no errors", false, action.CloseTypeInfo.HasNotifications());
				project.WKP_Status = "CLS";
			}
		}
	}
}
