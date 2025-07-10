using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class EntriesWithMessagesOnDeclarationUserControlTest : TestCaseWithFactory
	{
		public void TestUserControl()
		{
			using (var control = new EntriesWithMessagesOnDeclarationUserControl())
			{
				AssertEquals(typeof(EntriesAndEntryLinesUserControl), control.NewMessageUserControl.GetType());
				control.NewMessageUserControl.Dispose();
			}
		}
	}
}
