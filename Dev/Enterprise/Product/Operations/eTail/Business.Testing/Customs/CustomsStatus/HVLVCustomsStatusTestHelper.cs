using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.eTail.Business.Testing
{
	public static class HVLVCustomsStatusTestHelper
	{
		public static void AddRefCusCodeList(BusinessObjectFactory factory, string code, string description, string releaseStatus = HVLVReleaseStatus.Held, string codeType = RefCusCodeListTypes.Codes.CustomsStatus, string countryCode = CountryCodes.Australia)
		{
			var testHelper = new UniversalReferenceTestDataHelper(factory);
			testHelper.CreateCusCodeListWithAttribute(countryCode, codeType, code, description, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), eCommerceReleaseStatus, releaseStatus);
			factory.Save();
		}

		const string eCommerceReleaseStatus = "EcommerceReleaseStatus";
	}
}
