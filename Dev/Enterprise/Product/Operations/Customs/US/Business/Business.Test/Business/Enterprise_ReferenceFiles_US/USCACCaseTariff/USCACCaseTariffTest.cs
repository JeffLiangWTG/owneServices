using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCACCaseTariff))]
	sealed class USCACCaseTariffTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFormattedTariff()
		{
			var caseTariff = Factory.New<USCACCaseTariff>();
			caseTariff.U9_TariffNumber = "0000000000";
			AssertEquals("0000.00.0000", caseTariff.FormattedTariff);
		}
	}
}
