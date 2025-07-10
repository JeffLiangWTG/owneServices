using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusTWProductLabelRangeCollection))]
	sealed class CusTWProductLabelRangeCollectionTest : BusinessObjectCollectionTestCase
	{
		[ExpectNoExceptions]
		public void TestAllowNewCore()
		{
			var testCollection = new CusTWProductLabelRangeCollection(Factory.New<CusTWControllingMessageHeader>());
			NUnit.Framework.Assert.That(testCollection.MaxCount, NUnit.Framework.Is.EqualTo(99));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CusTWProductLabelRangeCollection(Factory.New<CusTWControllingMessageHeader>());
		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusTWProductLabelRange>();
	}
}
