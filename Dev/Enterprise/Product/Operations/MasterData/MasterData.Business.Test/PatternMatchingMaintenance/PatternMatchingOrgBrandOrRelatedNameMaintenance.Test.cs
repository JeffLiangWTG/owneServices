using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	class PatternMatchingOrgBrandOrRelatedNameMaintenanceTest : PatternMatchingSourceTest<OrgBrandOrRelatedName, PatternMatchingOrgBrandOrRelatedNameMaintenance>
	{
		protected override OrgBrandOrRelatedName BizO => brand ?? (brand = CreateOrganisation().BrandsOrRelatedNames.AddNew());
		OrgBrandOrRelatedName brand;

		protected override PatternMatchingOrgBrandOrRelatedNameMaintenance MaintenanceObject => new PatternMatchingOrgBrandOrRelatedNameMaintenance(BizO);

		protected override void AssertPatternMatchingDataExists()
		{
			var patternMatchingAddresses = Factory.Load<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, BizO.PK));
			var patternMatchingDomains = Factory.Load<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, BizO.PK));
			var patternMatchingEmails = Factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, BizO.PK));
			var patternMatchingNames = Factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, BizO.PK));
			var patternMatchingPhones = Factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, BizO.PK));
			var patternMatchingRegCodes = Factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, BizO.PK));

			var valueNameToHash = "COSTCO".ToUpperInvariant();
			var patternName = patternMatchingNames[0];

			AssertEquals(0, patternMatchingAddresses.Length);
			AssertEquals(0, patternMatchingDomains.Length);
			AssertEquals(0, patternMatchingEmails.Length);
			AssertEquals(1, patternMatchingNames.Length);
			AssertEquals(0, patternMatchingPhones.Length);
			AssertEquals(0, patternMatchingRegCodes.Length);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueNameToHash), patternName.PMN_HashedValue);
			AssertEquals(brand.PK, patternName.PMN_ParentId);
		}

		protected override void SetupBizO()
		{
			BizO.P1_RelatedName = "COSTCO LTD";

			Factory.Save();
		}
	}
}
