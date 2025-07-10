using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusPackingListCollection))]
	sealed class CusPackingListBizoCollectionTest : ActiveBusinessObjectCollectionTestCase<CusPackingListCollection>
	{
		protected override CusPackingListCollection GetCollectionToTest() => new CusPackingListCollection(Factory);
	}
}
