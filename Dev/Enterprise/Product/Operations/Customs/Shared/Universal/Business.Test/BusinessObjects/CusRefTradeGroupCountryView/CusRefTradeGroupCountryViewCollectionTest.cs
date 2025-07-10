using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefTradeGroupCountryViewCollection))]
	public class CusRefTradeGroupCountryViewCollectionTest : ActiveBusinessObjectCollectionTestCase<CusRefTradeGroupCountryViewCollection>
	{
		protected override CusRefTradeGroupCountryViewCollection GetCollectionToTest()
		{
			return new CusRefTradeGroupCountryViewCollection(Factory.New<CusRefTradeGroupView>());
		}
	}
}
