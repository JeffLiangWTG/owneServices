using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefZoneCountryCollection))]
	sealed class RefZoneCountryCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new RefZoneCountryCollection(Factory.New<RefZoneHeader>());
		}

		public void TestCollectionReadonly()
		{
			var wrsZone = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_ZoneType, RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean));
			var collection = wrsZone.Countries;

			Assert("Collection for WRS zone should be read-only", collection.ReadOnly);

			var schedulesZone = Factory.NewWithValidTestData<RefZoneHeader>();
			schedulesZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Schedules;
			collection = schedulesZone.Countries;

			Assert("Collection for SCH zone should be read-only", collection.ReadOnly);

			var zoneHeader = Factory.NewWithValidTestData<RefZoneHeader>();
			zoneHeader.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.All;
			collection = zoneHeader.Countries;
			AssertEquals("Collection for ALL zone should not be read-only", false, collection.ReadOnly);
		}
	}
}
