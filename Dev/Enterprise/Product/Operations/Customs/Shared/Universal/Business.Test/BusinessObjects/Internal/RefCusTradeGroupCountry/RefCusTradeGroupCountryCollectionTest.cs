using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTradeGroupCountryCollection))]
	internal class RefCusTradeGroupCountryCollectionTest : ActiveBusinessObjectCollectionTestCase<RefCusTradeGroupCountryCollection>
	{
		protected override RefCusTradeGroupCountryCollection GetCollectionToTest()
		{
			return new RefCusTradeGroupCountryCollection(Factory.New<RefCusTradeGroup>());
		}
	}
}
