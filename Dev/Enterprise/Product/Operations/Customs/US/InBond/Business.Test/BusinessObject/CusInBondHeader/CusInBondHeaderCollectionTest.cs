using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondHeaderCollection))]
	sealed class CusInBondHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondHeaderCollection>
	{
		public void TestDoesNotLoadNonInBondHeaderType()
		{
			var obj1 = Factory.New<CusInBondHeader>();
			obj1.BH_CarrierSCAC = "C1";
			var obj2 = Factory.New<CusInBondHeader>();
			obj2.BH_CarrierSCAC = "C2";
			obj2.BH_ApplicationCode = "SDF";
			var obj3 = Factory.New<CusInBondHeader>();
			obj3.BH_CarrierSCAC = "D1";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var collection = new CusInBondHeaderCollection(newFactory);
			AssertEquals(2, collection.Count);
			AssertNotNull("Collection should have obj1", collection.FindByPK(obj1.PK));
			AssertNotNull("Collection should have obj3", collection.FindByPK(obj3.PK));
			collection = new CusInBondHeaderCollection(newFactory, new ZQuery(CusInBondHeaderSchema.BH_CarrierSCAC, SQLComparisonOperator.StartsWith, "D"));
			AssertEquals(1, collection.Count);
			AssertNotNull("Collection should have obj3", collection.FindByPK(obj3.PK));
		}
	}
}
