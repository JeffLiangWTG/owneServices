using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVItemInOriginLoadListCollection))]
	public class HVLVItemInOriginLoadListCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			Assert("HVLVItemInOriginLoadListCollection is not supposed to allow new", !collection.AllowNew);
		}

		public void TestReadOnly()
		{
			var collection = GetCollectionToTest();
			Assert("HVLVItemInOriginLoadListCollection is supposed to be read only", collection.ReadOnly);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new HVLVItemInOriginLoadListCollection(Factory.New<HVLVOriginLoadList>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<HVLVItem>();
		}

		#endregion
	}
}
