using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	public static class CusRefRateCodeViewTestHelper
	{
		public static CusRefRateCodeView CreateAndSaveCusRateCodeForCountryIfNotExists(BusinessObjectFactory factory, string countryCode, string rateType, string rateCode, bool isSystem = true, bool internalUse = false, string description = "")
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var refCusRateTypePK = ZGuid.Empty;
			if (isSystem)
			{
				refCusRateTypePK = helper.CreateNewOrGetExistingRateType(countryCode, rateType).PK;
			}

			var refCusRateCode = helper.LoadOrCreateNewCusRateCode(factory, rateCode, refCusRateTypePK, isSystem, internalUse, description, cusRateType: rateType, countryCode: countryCode);
			return refCusRateCode;
		}
	}
}
