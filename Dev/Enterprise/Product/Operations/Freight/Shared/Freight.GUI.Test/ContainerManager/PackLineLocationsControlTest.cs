using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class PackLineLocationsControlTest : TestCaseWithFactory
	{
		public void TestPackLocationsOnPackLine()
		{
			PackLine packLine = Factory.New<PackLine>();
			AssertNotNull(packLine.PackLocations);
			AssertEquals(typeof(PackLocationCollection), packLine.PackLocations.GetType());
		}

		public void TestGrid()
		{
			using (PackLineLocationsControl locationsControl = new PackLineLocationsControl())
			{
				AssertEquals(locationsControl.Grid, locationsControl.Controls.Find("locationsGrid", true)[0]);
			}
		}

		public void TestProcessLocationGridColumns()
		{
			WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (PackLineLocationsControl locationsControl = new PackLineLocationsControl())
			{
				AssertNull(GetColumnInfo(locationsControl.Grid, PackLocation.Schema.JQ_WarehouseLocation));
				AssertNotNull(GetColumnInfo(locationsControl.Grid, PackLocation.Schema.LocationString));
				AssertNotNull(GetColumnInfo(locationsControl.Grid, PackLocation.Schema.LocationWhsGuid));
			}

			WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (PackLineLocationsControl locationsControl = new PackLineLocationsControl())
			{
				AssertNotNull(GetColumnInfo(locationsControl.Grid, PackLocation.Schema.JQ_WarehouseLocation));
				AssertNull(GetColumnInfo(locationsControl.Grid, PackLocation.Schema.LocationString));
				AssertNull(GetColumnInfo(locationsControl.Grid, PackLocation.Schema.LocationWhsGuid));
			}
		}

		ZGridColumnInfo GetColumnInfo(ZGrid grid, string columnName)
		{
			return grid.ColumnStyles
				.Cast<ZGridColumnInfo>()
				.FirstOrDefault(columnStyle => columnStyle.ColumnName == columnName);
		}
	}
}
