using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business.Express;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Test
{
	[TestedType(typeof(CusStorageDocPivotCollection))]
	sealed class CusStorageDocPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var scaOceanBill = Factory.New<CusSCAOceanBill>();
			return scaOceanBill.EDocPivotCollection;
		}

		public void TestSetCollectionRelationships()
		{
			var collection = (CusStorageDocPivotCollection)GetCollectionToTest();
			var item = collection.AddNew();
			AssertEquals("CB", item.CSD_ParentTableCode);
			AssertNotNull(item.Parent);
		}

		public void TestMaster()
		{
			var collection = (CusStorageDocPivotCollection)GetCollectionToTest();
			Assert(collection.Master is ICusStorageDocPivotParent);
		}
	}
}
