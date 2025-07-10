using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal.Testing
{
	internal static class RefCusRateCodeHelperForTest
	{
		public static RefCusRateCode CreateAndSaveCusRateCodeForCountryIfNotExists(BusinessObjectFactory factory, string countryCode, string rateType, string rateCode)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var refCusRateType = helper.CreateNewOrGetExistingRateType(countryCode, rateType);
			var query = new ZQuery(RefCusRateCodeSchema.ZY1_RateCode, rateCode);
			query.AddToFilter(RefCusRateCodeSchema.ZY1_ZZR_RateType, refCusRateType.PK);
			var refCusRateCode = factory.LoadTop1<RefCusRateCode>(query);
			if (refCusRateCode == null)
			{
				refCusRateCode = factory.New<RefCusRateCode>();
				refCusRateCode.ZY1_RateCode = rateCode;
				refCusRateCode.ZY1_ZZR_RateType = refCusRateType.PK;
				refCusRateCode.ZY1_Description = rateCode + " DESC";
			}

			factory.Save();
			return refCusRateCode;
		}
	}
}
