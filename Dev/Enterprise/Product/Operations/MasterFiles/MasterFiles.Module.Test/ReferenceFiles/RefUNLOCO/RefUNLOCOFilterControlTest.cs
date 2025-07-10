using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class RefUNLOCOFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestGridColumnNames()
		{
			using (var form = new ZForm())
			using (var filterControl = GetNewFilterControl())
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.Grid;

				CombineAssertions(() =>
				{
					foreach (var (columnName, columnCaption, _, _) in GridColumns)
					{
						AssertEquals(columnName, columnCaption, grid.GetColumnCaption(columnName));
					}
				});
			}
		}

		[RequiresSTA]
		public void TestGridDefaultColumnsInOrder()
		{
			using (var form = new ZForm())
			using (var filterControl = GetNewFilterControl())
			{
				form.Controls.Add(filterControl);
				form.Show();

				AssertSequencesEqual(GridColumns.Where(x => x.IsDefault).Select(x => x.ColumnName), filterControl.Grid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
			}
		}

		[RequiresSTA]
		public void TestGridColumnWidths()
		{
			using (var filterControl = GetNewFilterControl())
			{
				var grid = filterControl.Grid;
				CombineAssertions(() =>
				{
					foreach (var (columnName, _, columnWidth, _) in GridColumns)
					{
						AssertEquals(columnName, columnWidth, grid.GetColumnStyle(columnName).Width);
					}
				});
			}
		}

		RefUNLOCOFilterControl GetNewFilterControl()
		{
			var collection = new RefUNLOCOCollection(Factory);
			var filterBizO = new RefUNLOCOFilterBusinessObject();
			return new RefUNLOCOFilterControl(collection, filterBizO);
		}

		static (string ColumnName, string ColumnCaption, int ColumnWidth, bool IsDefault)[] GridColumns => new[]
		{
			(RefUNLOCO.Schema.RL_Code, "Code", 80, true),
			(RefUNLOCO.Schema.RL_PortName, "Port Name", 180, true),
			(RefUNLOCO.Schema.RL_IATA, "IATA", 80, true),
			(RefUNLOCO.Schema.RL_IATARegionCode, "IATA Region Code", 80, true),
			(RefUNLOCO.Schema.CoordinateText, "Coordinates", 100, true),
			(RefUNLOCO.Schema.StandardZoneUTCOffset, "GMT Offset", 80, true),
			(RefUNLOCO.Schema.HasDaylightSavingsZone, "Daylight Savings", 80, true),
			(RefUNLOCO.Schema.IsInCurrentCompanysCountry, "Is In Current Country/Region", 120, true),
			(RefUNLOCO.Schema.IsInEU, "Is In EU", 50, true),
			(RefUNLOCO.Schema.CountryEconomicGroupDescription, "Economic Group", 200, true),
			(RefUNLOCO.Schema.RL_HasDischarge, "Discharge", 80, false),
			(RefUNLOCO.Schema.RL_HasOutport, "Has Outport", 80, false),
			(RefUNLOCO.Schema.RL_HasPost, "Has Post", 80, false),
			(RefUNLOCO.Schema.RL_HasRail, "Has Rail", 80, false),
			(RefUNLOCO.Schema.RL_HasRoad, "Has Road", 80, false),
			(RefUNLOCO.Schema.RL_HasSeaport, "Has Seaport", 80, false),
			(RefUNLOCO.Schema.RL_HasStore, "Has Store", 80, false),
			(RefUNLOCO.Schema.RL_HasTerminal, "Has Terminal", 80, false),
			(RefUNLOCO.Schema.RL_HasUnload, "Has Unload", 80, false),
			(RefUNLOCO.Schema.RL_NameWithDiacriticals, "Proper Name", 180, false),
			(RefUNLOCO.Schema.RL_IsActive, "Is Active", 80, false),
			(RefUNLOCO.Schema.RL_IsUpdatable, "Is System Updatable", 80, false),
			(RefUNLOCO.Schema.RL_IsSystem, "Is System", 80, false),
			(RefUNLOCO.Schema.IsInEU, "Is In EU", 50, false),
			(RefUNLOCO.Schema.RL_RW, "Country/Region States Code", 80, false),
			(RefUNLOCO.Schema.RL_HasAirport, "Has Airport", 80, false),
			(RefUNLOCO.Schema.RL_HasBorderCrossing, "Has Border Crossing", 80, false),
			(RefUNLOCO.Schema.RL_HasCustomsLodge, "Has Customs Lodge", 80, false)
		};
	}
}
