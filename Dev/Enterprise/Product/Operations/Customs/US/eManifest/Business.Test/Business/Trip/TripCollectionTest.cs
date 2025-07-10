using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(TripCollection))]
	sealed class TripCollectionTest : ActiveBusinessObjectCollectionTestCase<TripCollection>
	{
		public void TestDoesNotLoadNoEManifestType()
		{
			var trip1 = Factory.New<Trip>();
			trip1.BH_CarrierSCAC = "T1";
			trip1.BH_ApplicationCode = "MAN";
			var trip2 = Factory.New<Trip>();
			trip2.BH_CarrierSCAC = "T2";
			trip2.BH_ApplicationCode = "INB";
			var trip3 = Factory.New<Trip>();
			trip3.BH_CarrierSCAC = "D1";
			trip3.BH_ApplicationCode = "MAN";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var collection = new TripCollection(newFactory);
			AssertEquals(2, collection.Count);
			AssertNotNull("Collection should have trip1", collection.FindByPK(trip1.PK));
			AssertNotNull("Collection should have trip3", collection.FindByPK(trip3.PK));
			collection = new TripCollection(newFactory, new ZQuery(CusInBondHeaderSchema.BH_CarrierSCAC, SQLComparisonOperator.StartsWith, "D"));
			AssertEquals(1, collection.Count);
			AssertNotNull("Collection should have trip3", collection.FindByPK(trip3.PK));
		}
	}
}
