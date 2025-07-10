using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(BusinessObjectCollectionWrapper<DummyDocWrapper>))]
	sealed class BusinessObjectCollectionWrapperTest : NonPersistentBusinessObjectCollectionTestCase<BusinessObjectCollectionWrapper<DummyDocWrapper>>
	{
		protected override BusinessObjectCollectionWrapper<DummyDocWrapper> GetCollectionToTest()
		{
			var input = new[]
			{
				new DummyDocWrapper(),
				new DummyDocWrapper(),
				new DummyDocWrapper(),
			};
			return new BusinessObjectCollectionWrapper<DummyDocWrapper>(input);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DummyDocWrapper();
		}

		public void TestAllowNew()
		{
			var testCollection = GetCollectionToTest();
			Assert(!testCollection.AllowNew);
		}

		public void TestConstruction()
		{
			var testCollection = GetCollectionToTest();
			AssertEquals(3, testCollection.Count);
		}
	}
}
