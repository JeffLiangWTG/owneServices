using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCTariffValue))]
	sealed class USCTariffValueTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			USCTariffValue result = factory.New<USCTariffValue>();
			result.UA_ValueEditCode = "X";
			return result;
		}
	}
}
