using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(CusUSLVItemPGAWrapperCollection))]
	public class CusUSLVItemPGAWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CusUSLVItemPGAWrapperCollection>
	{
		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			Assert(!collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = GetCollectionToTest();
			Assert(!collection.AllowRemove);
		}

		protected override CusUSLVItemPGAWrapperCollection GetCollectionToTest()
		{
			return new CusUSLVItemPGAWrapperCollection(new CusUSLVItemPGAAgencyRequirementsProvider(Factory.New<CusUSLVItem>()));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CusUSLVItemPGAWrapper(Factory, new CusUSLVItemPGAAgencyRequirementsProvider(Factory.New<CusUSLVItem>()), "TST");
		}
	}
}
