using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class QueryInterchangeCreatorTest : TestCaseWithFactory
	{
		public void TestAddColumnAndMenuForQuery()
		{
			var originalName = GlbStaff.CurrentUser.GS_LoginName;
			GlbStaff.CurrentUser.GS_LoginName = "AAA";
			try
			{
				using (CustomsDataRegistry.Instance.EnableQueryInterchangeByEHubPortalWebservice.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertColumnAndMenuExist(false);
					GlbStaff.CurrentUser.GS_LoginName = User.SupportUserName;
					AssertColumnAndMenuExist(true);
					GlbStaff.CurrentUser.GS_LoginName = "XXX";
					AssertColumnAndMenuExist(false);
				}

				using (CustomsDataRegistry.Instance.EnableQueryInterchangeByEHubPortalWebservice.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertColumnAndMenuExist(true);
				}
			}
			finally
			{
				GlbStaff.CurrentUser.GS_LoginName = originalName;
			}
		}

		public void TestColumnVisibility()
		{
			using (var grid = new ZGrid())
			{
				var creator = new QueryInterchangeCreator(grid);
				creator.AddColumnAndMenuForQuery();
				AssertColumnVisible(grid, false);
			}

			using (var grid = new ZGrid())
			{
				var creator = new QueryInterchangeCreator(grid);
				creator.AddColumnAndMenuForQuery(false);
				AssertColumnVisible(grid, false);
			}

			using (var grid = new ZGrid())
			{
				var creator = new QueryInterchangeCreator(grid);
				creator.AddColumnAndMenuForQuery(true);
				AssertColumnVisible(grid, true);
			}
		}

		void AssertColumnAndMenuExist(bool isMenuExist)
		{
			using (var grid = new ZGrid())
			{
				var count = grid.Columns.Count;
				AssertEquals(0, count);
				var menuItem = grid.ContextMenu.MenuItems.FindByText("Query Interchange On eHub");
				AssertNull("Shoud not find the menu item as default.", menuItem);
				var creator = new QueryInterchangeCreator(grid);
				creator.AddColumnAndMenuForQuery();
				count = grid.ColumnStyles.Count;
				AssertEquals(1, count);
				menuItem = grid.ContextMenu.MenuItems.FindByText("Query Interchange On eHub");
				if (isMenuExist)
				{
					AssertNotNull("Should find the menu item.", menuItem);
				}
				else
				{
					AssertNull("Should not find the menu item.", menuItem);
				}
			}
		}

		void AssertColumnVisible(ZGrid grid, bool visible)
		{
			var columnStyle = grid.ColumnStyles[0] as ZGridColumnInfo;
			AssertNotNull("Should find the column.", columnStyle);
			AssertEquals("Column should have visibility.", visible, columnStyle.IsVisible);
		}
	}
}
