using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	class PatternMatchingGenRegCertAccredMaintListMaintenanceTest : PatternMatchingSourceTest<GenRegCertAccredMaintList, PatternMatchingGenRegCertAccredMaintListMaintenance>
	{
		protected override GenRegCertAccredMaintList BizO => cert ?? (cert = Factory.NewWithValidTestData<GenRegCertAccredMaintList>());
		GenRegCertAccredMaintList cert;

		protected override PatternMatchingGenRegCertAccredMaintListMaintenance MaintenanceObject => new PatternMatchingGenRegCertAccredMaintListMaintenance(BizO);

		protected override void AssertPatternMatchingDataExists()
		{
			var patternMatchingAddresses = Factory.Load<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, BizO.PK));
			var patternMatchingDomains = Factory.Load<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, BizO.PK));
			var patternMatchingEmails = Factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, BizO.PK));
			var patternMatchingNames = Factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, BizO.PK));
			var patternMatchingPhones = Factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, BizO.PK));
			var patternMatchingRegCodes = Factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, BizO.PK));

			var valueRegCodeToHash = "PASAA12834";

			var patternRegCode = patternMatchingRegCodes[0];

			AssertEquals(0, patternMatchingAddresses.Length);
			AssertEquals(0, patternMatchingDomains.Length);
			AssertEquals(0, patternMatchingEmails.Length);
			AssertEquals(0, patternMatchingNames.Length);
			AssertEquals(0, patternMatchingPhones.Length);
			AssertEquals(1, patternMatchingRegCodes.Length);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueRegCodeToHash), patternRegCode.PMR_HashedValue);
			AssertEquals(cert.PK, patternRegCode.PMR_ParentId);
		}

		protected override void SetupBizO()
		{
			BizO.XZ_RefNumber = "AA12834";
			BizO.XZ_Type = "PAS";
			BizO.XZ_ParentTableCode = GlbPersonSchema.Constants.Prefix;
			BizO.XZ_ParentID = Factory.NewWithValidTestData<GlbPerson>().PK;

			Factory.Save();
		}
	}
}
