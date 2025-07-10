using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	using System.Linq;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.MasterFiles.Integration;

	internal class GenShapeGeographyLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSHG_ParentTableCode_List()
		{
			var shape = Factory.New<GenShapeGeography>();
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					(RefCountrySchema.Constants.Prefix, "Country/Region"),
					(RefCityTownSchema.Constants.Prefix, "City/Town"),
					(RefZoneHeaderSchema.Constants.Prefix, "Zone")
				},
				shape.Lookups.SHG_ParentTableCode_List.ToArray().Select(u => (u.Code, u.Description)));
		}

		public void TestSHG_ParentID_List()
		{
			var shape = Factory.New<GenShapeGeography>();
			shape.SHG_ParentTableCode = RefCountrySchema.Constants.Prefix;
			Assert(shape.Lookups.SHG_ParentID_List is IRefCountryCollection);

			shape.SHG_ParentTableCode = RefCityTownSchema.Constants.Prefix;
			Assert(shape.Lookups.SHG_ParentID_List is RefCityTownCollection);

			shape.SHG_ParentTableCode = RefZoneHeaderSchema.Constants.Prefix;
			Assert(shape.Lookups.SHG_ParentID_List is RefZoneHeaderCollection);

			shape.SHG_ParentTableCode = "DUM";
			Assert(shape.Lookups.SHG_ParentID_List is RefZoneHeaderCollection);
		}
	}
}
