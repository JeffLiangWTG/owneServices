using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefApplicabilityViewCollection))]
	public class CusRefApplicabilityViewCollection_Rate_Test : ActiveBusinessObjectCollectionTestCase<CusRefApplicabilityViewCollection>
	{
		protected override CusRefApplicabilityViewCollection GetCollectionToTest()
		{
			return Factory.New<RateView>().RateApplicabilities;
		}
	}
}
