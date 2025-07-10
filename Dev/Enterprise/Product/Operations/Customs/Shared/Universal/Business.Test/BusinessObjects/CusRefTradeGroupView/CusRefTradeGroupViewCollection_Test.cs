using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefTradeGroupViewCollection))]
	public class CusRefTradeGroupViewCollection_Test : ActiveBusinessObjectCollectionTestCase<CusRefTradeGroupViewCollection>
	{
		protected override CusRefTradeGroupViewCollection GetCollectionToTest() => new CusRefTradeGroupViewCollection(Factory);
	}
}
