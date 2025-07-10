using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class MessagesStatusErrorsUserControlTest : TestCaseWithFactory
	{
		public void TestMessagesStatusErrorsUserControlMinSize()
		{
			using (var control = new MessagesStatusErrorsUserControl())
			{
				AssertEquals(new System.Drawing.Size(420, 330), control.MinimumSize);
			}
		}
	}
}
