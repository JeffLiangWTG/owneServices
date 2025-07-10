using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transit.Module.Testing.DispatchConsignment
{
	abstract class WhsTransitFilterControlTest<TFilterControl> : TestCaseWithFactory where TFilterControl : ZFilterStripControl
	{
		protected void AssertColumn(string columnDisplayName, string columnName, bool expectedColumnVisible = true)
		{
			using (var filterControl = GetFilterControl())
			{
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any(col => col.ColumnName == columnName && col.IsVisible == expectedColumnVisible);

				Assert($"{columnDisplayName} column should exist and should {(expectedColumnVisible ? "" : "NOT ")}be visible.", columnExistsAndNotVisible);
			}
		}

		protected abstract TFilterControl GetFilterControl();
	}
}
