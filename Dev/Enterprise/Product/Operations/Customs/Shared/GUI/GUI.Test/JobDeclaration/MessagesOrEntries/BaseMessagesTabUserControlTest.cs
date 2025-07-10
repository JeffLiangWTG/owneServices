using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TestBaseMessagesTabUserControl : TestCaseWithFactory
	{
		public void TestQueryInterchangeOnEHubMenu()
		{
			using (var control = new BaseMessagesTabUserControl())
			{
				AssertNotNull("Should find the Query Ehub menu.", control.FindSingle<ZGrid>("MessagesGrid").ContextMenu.MenuItems.FindByText("Query Interchange On eHub"));
			}
		}
	}
}
