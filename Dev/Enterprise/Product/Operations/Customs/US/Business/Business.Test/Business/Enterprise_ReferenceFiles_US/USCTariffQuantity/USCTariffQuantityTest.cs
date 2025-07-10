using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCTariffQuantity))]
	sealed class USCTariffQuantityTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			USCTariffQuantity result = factory.New<USCTariffQuantity>();
			result.UQ_QuantityEditCode = "X";
			return result;
		}
	}
}
