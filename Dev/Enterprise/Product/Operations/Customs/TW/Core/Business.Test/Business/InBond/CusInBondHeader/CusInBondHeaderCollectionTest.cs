using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusInBondHeaderCollection))]
	sealed class CusInBondHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondHeaderCollection>
	{
		public void TestDoesNotLoadNonInBondHeaderType()
		{
			var obj1 = Factory.New<CusInBondHeader>();
			obj1.BH_CarrierSCAC = "C1";
			AssertEquals(Common.CusInBondApplicationCodeList.Codes.TWTranshipment, obj1.BH_ApplicationCode);
			var obj2 = Factory.New<CusInBondHeader>();
			obj2.BH_CarrierSCAC = "C2";
			obj2.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.AMS;
			var obj3 = Factory.New<CusInBondHeader>();
			obj3.BH_CarrierSCAC = "D1";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var collection = new CusInBondHeaderCollection(newFactory);
			AssertNotNull("Collection should have obj1", collection.FindByPK(obj1.PK));
			AssertNull("Collection should not have obj2", collection.FindByPK(obj2.PK));
			AssertNotNull("Collection should have obj3", collection.FindByPK(obj3.PK));
		}

		protected override CusInBondHeaderCollection GetCollectionToTest()
		{
			return new CusInBondHeaderCollection(Factory);
		}
	}
}
