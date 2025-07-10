using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefRateUOMViewCollection))]
	public class CusRefRateUOMViewCollection_RateView_Test : ActiveBusinessObjectCollectionTestCase<CusRefRateUOMViewCollection>
	{
		protected override CusRefRateUOMViewCollection GetCollectionToTest()
		{
			return Factory.New<RateView>().UnitsOfMeasure;
		}
	}
}
