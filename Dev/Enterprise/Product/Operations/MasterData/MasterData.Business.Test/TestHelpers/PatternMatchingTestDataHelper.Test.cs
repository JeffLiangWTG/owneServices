#if DEBUG
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	public static class PatternMatchingTestDataHelper
	{
		public static PatternMatchingAddress CreatePatternMatchingAddressFromMainAddress(this OrgHeader orgHeader, BusinessObjectFactory factory)
		{
			var hashedValue = TextStandardizerHelper.ComputeStringHashFast(orgHeader.MainAddress.OA_Address1 + orgHeader.MainAddress.OA_Address2 + orgHeader.MainAddress.OA_City + orgHeader.MainAddress.OA_PostCode + orgHeader.MainAddress.OA_State);

			var patternMatchingAddress = factory.NewWithValidTestData<PatternMatchingAddress>();
			patternMatchingAddress.PMA_OH = orgHeader.PK;
			patternMatchingAddress.PMA_ParentId = orgHeader.MainAddress.PK;
			patternMatchingAddress.PMA_HashedValue = hashedValue;
			patternMatchingAddress.PMA_RN_NKCountryCode = orgHeader.MainAddress.OA_RN_NKCountryCode;
			patternMatchingAddress.PMA_ParentTableCode = OrgAddressSchema.Constants.Prefix;
			patternMatchingAddress.PMA_IsActive = true;
			return patternMatchingAddress;
		}

		public static PatternMatchingName CreatePatternMatchingName(this OrgHeader orgHeader, BusinessObjectFactory factory)
		{
			var orgHeaderPK = orgHeader.PK;
			var countryCode = orgHeader.Country.Code;

			var patternMatchingName = factory.New<PatternMatchingName>();
			patternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(orgHeader.OH_FullName, countryCode));
			patternMatchingName.PMN_OH = orgHeaderPK;
			patternMatchingName.PMN_ParentId = orgHeaderPK;
			patternMatchingName.PMN_RN_NKCountryCode = countryCode;
			patternMatchingName.PMN_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			patternMatchingName.PMN_IsActive = true;
			return patternMatchingName;
		}
	}
}
#endif
