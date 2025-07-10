using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Freight.GUI.Testing
{
	public class ActionsMenuItemsHelperTest : TestCaseWithFactory
	{
		public static void AssertActionsMenuItemsNotAvailableInViewMode(ZForm form)
		{
			ZFormMenuStrategyTest.AssertActionsMenuItemsNotAvailableInViewMode(form);
		}
	}
}
