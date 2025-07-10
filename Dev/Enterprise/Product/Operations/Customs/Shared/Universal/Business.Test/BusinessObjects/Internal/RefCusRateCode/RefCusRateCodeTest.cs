using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusRateCode))]
	internal class RefCusRateCodeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return RefCusRateCodeHelperForTest.CreateAndSaveCusRateCodeForCountryIfNotExists(factory, Core.Constants.CountryCodes.SouthAfrica, "DJC", "DJC");
		}
	}
}
