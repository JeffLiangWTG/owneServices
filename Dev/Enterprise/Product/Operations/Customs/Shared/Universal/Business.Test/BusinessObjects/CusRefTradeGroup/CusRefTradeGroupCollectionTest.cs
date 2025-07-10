using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefTradeGroupCollection))]
	public class CusRefTradeGroupCollectionTest : ActiveBusinessObjectCollectionTestCase<CusRefTradeGroupCollection>
	{
		protected override CusRefTradeGroupCollection GetCollectionToTest()
		{
			return new CusRefTradeGroupCollection(Factory);
		}
	}
}
