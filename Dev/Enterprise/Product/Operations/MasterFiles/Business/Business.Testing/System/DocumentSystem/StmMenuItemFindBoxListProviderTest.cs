using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class StmMenuItemFindBoxListProviderTest : TestCaseWithFactory
	{
		public void TestDescriptionFromCode()
		{
			var menuItems = new StmMenuItemCollection(Factory);
			var findBoxListProvider = new StmMenuItemFindBoxListProvider(menuItems);
			var menuItem = CreateMenuItem("Context", "Name", "Path");

			AssertEquals("DescriptionFromCode()", menuItem.DocumentId, findBoxListProvider.DescriptionFromCode(menuItem.DocumentId));
		}

		public void TestNearestMatch()
		{
			var menuItems = new StmMenuItemCollection(Factory);
			var findBoxListProvider = new StmMenuItemFindBoxListProvider(menuItems);
			var menuItem = CreateMenuItem("Context", "Name", "Path");

			AssertEquals("NearestMatch()", menuItem.DocumentId, findBoxListProvider.NearestMatch("Con : Na", true, -1).Item1);
			AssertEquals("NearestMatch()", "Context : TestUniqueCode : Path", findBoxListProvider.NearestMatch("Context : TestUniqueCode : Path", true, -1).Item1);
			AssertEquals("NearestMatch()", "TestNoColons", findBoxListProvider.NearestMatch("TestNoColons", true, -1).Item1);
		}

		public void TestPrimaryKeyFromCode()
		{
			var menuItems = new StmMenuItemCollection(Factory);
			var findBoxListProvider = new StmMenuItemFindBoxListProvider(menuItems);
			var menuItem = CreateMenuItem("Context", "Name", "Path");

			AssertEquals("PrimaryKeyFromCode()", menuItem.PK, findBoxListProvider.PrimaryKeyFromCode(menuItem.DocumentId));
		}

		public void TestPrimaryKeyFromCodeWhenPathIsRoot()
		{
			var menuItems = new StmMenuItemCollection(Factory);
			var findBoxListProvider = new StmMenuItemFindBoxListProvider(menuItems);
			var menuItem = CreateMenuItem("Context", "Name", "");

			AssertEquals("PrimaryKeyFromCode()", menuItem.PK, findBoxListProvider.PrimaryKeyFromCode(menuItem.DocumentId));
		}

		public void TestPrimaryKeyFormCodeForLegacyPath()
		{
			var findBoxListProvider = new StmMenuItemFindBoxListProvider(new StmMenuItemCollection(Factory));
			var menuItem = CreateMenuItem("Context", "Name", "Legacy Documents/Path");

			AssertEquals(menuItem.PK, findBoxListProvider.PrimaryKeyFromCode(menuItem.DocumentId));
			AssertEquals(menuItem.PK, findBoxListProvider.PrimaryKeyFromCode("Context : Name : Path"));
			AssertEquals(menuItem.PK, findBoxListProvider.PrimaryKeyFromCode("Context : Name : Legacy Documents/Path"));
		}

		#region Implementation

		StmMenuItem CreateMenuItem(ZString businessContext, ZString menuName, ZString menuPath)
		{
			var result = Factory.New<StmMenuItem>();

			result.SU_BusinessContext = businessContext;
			result.SU_MenuName = menuName;
			result.SU_MenuPath = menuPath;

			return result;
		}

		#endregion
	}
}
