using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsLocationType))]
	class WhsLocationTypeAffectLocationViewTestCase : AffectLocationViewTestCase
	{
		#region TestReloadLocations

		protected override void TestReloadLocationForAffectorsCore()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var warehouse = helper.CreateWarehouse("WHS");
			var row = warehouse.Rows.AddNew();
			row.WR_Name = "A";
			row.WR_Columns = 2;
			AssertEquals("Precondition", 0, row.Locations.Count);

			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { LocationClasses.Codes.NOR, LocationClasses.Codes.NOR }, row.Locations.Select(l => l.WLV_LocationClass));
			AssertContainsExactElementsInAnyOrder(new[] { "RNO", "RNO" }, row.Locations.Select(l => l.WLV_LocationTypeCode));

			var locationType = row.Locations[0].LocationType;
			locationType.WLT_LocationClass = LocationClasses.Codes.HPL;
			locationType.WLT_Code = "XYZ";
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { LocationClasses.Codes.HPL, LocationClasses.Codes.HPL }, row.Locations.Select(l => l.WLV_LocationClass));
			AssertContainsExactElementsInAnyOrder(new[] { "XYZ", "XYZ" }, row.Locations.Select(l => l.WLV_LocationTypeCode));
		}

		#endregion

		#region Implementation

		protected override IEnumerable<WhsLocation> GetLocations(IAffectLocationView p)
		{
			var parent = (WhsLocationType)p;

			return parent.Factory.Load<WhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WLT_LocationType, parent.PK));
		}

		protected override IAffectLocationView GetNewParent()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var warehouse = helper.CreateWarehouse("WHS");
			var row = helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2);
			var locationType = helper.CreateLocationType("ABC");
			row.Locations.ForEach(l => l.WLV_WLT_LocationType = locationType.PK);
			return locationType;
		}

		protected override IEnumerable<SchemaColumn> ExpectedColumnsThatAffectLocationView => new[] { WhsLocationTypeSchema.WLT_LocationClass, WhsLocationTypeSchema.WLT_Code };

		#endregion
	}
}
