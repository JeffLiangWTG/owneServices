using System.Windows.Forms;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.GUI.Test
{
	[TestedType(typeof(BaseProjectPopupForm))]
	public class BaseProjectPopupFormTest : ZFormBasherTest
	{
		public void TestFormVerb()
		{
			Project project = Factory.New<Project>();
			ProjectAction action = new ProjectAction(project);
			using (BaseProjectPopupForm form = new BaseProjectPopupForm(action))
			{
				AssertEquals("", form.FormVerb);
			}
		}

		public virtual void TestCloseButton()
		{
			Project project = Factory.New<Project>();
			ProjectAction action = new ProjectAction(project);

			using (BaseProjectPopupForm form = new BaseProjectPopupForm(action))
			{
				form.Show();
				form.CloseButton_Exposed.PerformClick();
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(DialogResult.None, form.DialogResult);
			}
		}

		public void TestNoAcceptButton()
		{
			using (var form = GetFormToBashCore())
			{
				AssertNull("Should have no accept button so Enter can be used for new line", form.AcceptButton);
			}
		}

		public void TestCancelButton()
		{
			Project project = Factory.New<Project>();
			ProjectAction action = new ProjectAction(project);

			using (BaseProjectPopupForm form = new BaseProjectPopupForm(action))
			{
				form.Show();
				form.CancelButton.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		protected override Form GetFormToBashCore()
		{
			Project project = Factory.New<Project>();
			ProjectAction action = new ProjectAction(project);
			return new BaseProjectPopupForm(action);
		}
	}
}
