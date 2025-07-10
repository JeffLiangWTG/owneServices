using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	class PatternMatchingOrgWebURLMaintenanceTest : PatternMatchingSourceTest<OrgWebURL, PatternMatchingOrgWebURLMaintenance>
	{
		protected override OrgWebURL BizO => url ?? (url = CreateOrganisation().OrgWebURLs.AddNew());
		OrgWebURL url;

		protected override PatternMatchingOrgWebURLMaintenance MaintenanceObject => new PatternMatchingOrgWebURLMaintenance(BizO);

		protected override void AssertPatternMatchingDataExists()
		{
			var patternMatchingAddresses = Factory.Load<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, BizO.PK));
			var patternMatchingDomains = Factory.Load<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, BizO.PK));
			var patternMatchingEmails = Factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, BizO.PK));
			var patternMatchingNames = Factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, BizO.PK));
			var patternMatchingPhones = Factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, BizO.PK));
			var patternMatchingRegCodes = Factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, BizO.PK));

			var valueDomainToHash = "wisetechglobal.com".ToUpperInvariant();

			var patternDomain = patternMatchingDomains[0];

			AssertEquals(0, patternMatchingAddresses.Length);
			AssertEquals(1, patternMatchingDomains.Length);
			AssertEquals(0, patternMatchingEmails.Length);
			AssertEquals(0, patternMatchingNames.Length);
			AssertEquals(0, patternMatchingPhones.Length);
			AssertEquals(0, patternMatchingRegCodes.Length);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueDomainToHash), patternDomain.PMD_HashedValue);
			AssertEquals(url.PK, patternDomain.PMD_ParentId);
			AssertEquals(url.Header.CountryCode, patternDomain.PMD_RN_NKCountryCode);
		}

		protected override void SetupBizO()
		{
			BizO.PU_URL = "http://www.wisetechglobal.com";

			Factory.Save();
		}
	}
}
