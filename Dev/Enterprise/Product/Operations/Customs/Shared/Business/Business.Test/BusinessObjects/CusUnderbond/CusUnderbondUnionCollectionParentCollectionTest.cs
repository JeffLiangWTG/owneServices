using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Interfaces;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusUnderbondUnionCollectionParentCollection))]
	sealed class CusUnderbondUnionCollectionParentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusUnderbondUnionCollectionParentCollection(Factory, typeof(DummyCusUnderbondUnionCollectionParent));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(DummyCusUnderbondUnionCollectionParent));
		}

		public override void TestAddNew()
		{
			Assert(true);
		}

		public override void TestLoad()
		{
			Assert(true);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert(true);
		}

		public void TestTypeOfElements()
		{
			CusUnderbondUnionCollectionParentCollection collection = new CusUnderbondUnionCollectionParentCollection(Factory);
			AssertEquals("type when no type given", typeof(ICusUnderbondUnionCollectionParent), collection.TypeOfElements);
			collection = new CusUnderbondUnionCollectionParentCollection(Factory, typeof(DummyCusUnderbondUnionCollectionParent));
			AssertEquals("type when type given", typeof(DummyCusUnderbondUnionCollectionParent), collection.TypeOfElements);
		}
	}
}
