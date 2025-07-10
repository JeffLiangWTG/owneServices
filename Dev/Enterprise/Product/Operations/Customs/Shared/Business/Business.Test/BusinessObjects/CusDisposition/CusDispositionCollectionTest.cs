using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusDispositionCollection))]
	sealed class CusDispositionCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var master = Factory.New<CusDispositionTestBO>();
			return new CusDispositionCollection(master);
		}

		public void TestSetDefaultsForNewChild()
		{
			var master = Factory.New<CusDispositionTestBO>();
			var collection = new CusDispositionCollection(master);
			var child = collection.AddNew();

			AssertEquals(child.CDI_ParentID, master.PK);
			AssertEquals(child.CDI_Type, "TST");
			AssertEquals(child.CDI_ParentTableCode, "JE");
		}

		public void TestCusDispositionParent()
		{
			var master = Factory.New<CusDispositionTestBO>();
			var collection = new CusDispositionCollection(master);
			var child = collection.AddNew();

			AssertEquals(child.Parent.PK, master.PK);
		}
	}
}
