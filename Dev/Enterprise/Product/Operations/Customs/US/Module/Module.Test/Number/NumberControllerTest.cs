using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(NumberController))]
	abstract class NumberControllerTest : ZSingletonControllerBasherTest
	{
		public void TestDisplayMode()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = (ZForm)controller.ShowNewForm())
			{
				form.Show();
				Application.DoEvents();
				form.Close();
				AssertNotContains("Would you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZArchitecture.Core.ODisplayMode.Browse, form.DisplayMode);
			}
		}
	}
}
