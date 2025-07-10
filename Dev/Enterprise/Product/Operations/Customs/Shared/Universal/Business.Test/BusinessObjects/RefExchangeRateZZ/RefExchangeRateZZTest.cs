using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefExchangeRateZZ))]
	public class RefExchangeRateZZTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var businessObject = factory.New<RefExchangeRateZZ>();
			businessObject.ZZN_ExRateType = "CUS";
			businessObject.ZZN_StartDate = ZDateTime.Now.AddDays(-1);
			businessObject.ZZN_EndDate = ZDateTime.Now;
			businessObject.ZZN_Rate = 10;
			businessObject.ZZN_RX_NKExCurrency = "USD";
			businessObject.ZZN_RN_NKCountry = "US";
			businessObject.ZZN_AsPublished = string.Empty;
			return businessObject;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}
	}
}
