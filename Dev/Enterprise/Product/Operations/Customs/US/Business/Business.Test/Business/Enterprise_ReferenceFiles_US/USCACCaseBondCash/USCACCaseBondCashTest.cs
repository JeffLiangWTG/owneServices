using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCACCaseBondCash))]
	sealed class USCACCaseBondCashTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIndicatorDesc()
		{
			var bondCash = Factory.New<USCACCaseBondCash>();
			bondCash.U8_Indicator = BondCashIndicatorList.Codes.BOC;
			AssertEquals(BondCashIndicatorList.Descriptions.BOC, bondCash.IndicatorDesc);
		}
	}
}
