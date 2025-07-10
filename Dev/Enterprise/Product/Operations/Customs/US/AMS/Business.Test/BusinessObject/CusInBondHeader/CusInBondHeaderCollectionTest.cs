using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
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

			var row = ((INeedRow)obj2).Row;

			var typeDecider = new CusInBondHeaderTypeDecider();
			_ = typeDecider.GetTypeForLoad(row, Factory);
			AssertEquals("ErrorReporter Key", "CusInBondHeader for Application Code 'SDF' is unknown", ErrorReporter.LastKeyReported);
			AssertEquals("ErrorReporter Message", "Cannot determine the CusInBondHeader object for Application Code 'SDF'", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			obj2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.AMS;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var collection = new CusInBondHeaderCollection(newFactory);
			AssertEquals(3, collection.Count);
			AssertNotNull("Collection should have obj1", collection.FindByPK(obj1.PK));
			AssertNotNull("Collection should have obj3", collection.FindByPK(obj3.PK));
			collection = new CusInBondHeaderCollection(newFactory, new ZQuery(CusInBondHeaderSchema.BH_CarrierSCAC, SQLComparisonOperator.StartsWith, "D"));
			AssertEquals(1, collection.Count);
			AssertNotNull("Collection should have obj3", collection.FindByPK(obj3.PK));
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert(true);
		}

		public override void TestAddAndDeleteOfElementAsThoughBinding()
		{
			Assert(true);
		}
	}
}
