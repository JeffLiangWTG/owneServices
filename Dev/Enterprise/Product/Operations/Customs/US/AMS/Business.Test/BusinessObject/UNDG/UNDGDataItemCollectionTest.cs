using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(UNDGDataItemCollection))]
	class UNDGDataItemCollectionTest : ActiveBusinessObjectCollectionTestCase<UNDGDataItemCollection>
	{
		protected override UNDGDataItemCollection GetCollectionToTest()
		{
			return new UNDGDataItemCollection(Factory.New<CusInBondContainer>());
		}
	}
}
