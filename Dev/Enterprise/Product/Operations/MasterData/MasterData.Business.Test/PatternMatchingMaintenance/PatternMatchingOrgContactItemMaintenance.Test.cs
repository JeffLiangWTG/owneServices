using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	class PatternMatchingOrgContactItemMaintenanceTest : PatternMatchingSourceTest<OrgContactItem, PatternMatchingOrgContactItemMaintenance>
	{
		protected override OrgContactItem BizO => contactItem ?? (contactItem = CreateOrganisation().Contacts.AddNew().ContactItems.AddNew());
		OrgContactItem contactItem;

		protected override PatternMatchingOrgContactItemMaintenance MaintenanceObject => new PatternMatchingOrgContactItemMaintenance(BizO);

		protected override void AssertPatternMatchingDataExists()
		{
			var patternMatchingAddresses = Factory.Load<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, BizO.PK));
			var patternMatchingDomains = Factory.Load<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, BizO.PK));
			var patternMatchingEmails = Factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, BizO.PK));
			var patternMatchingNames = Factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, BizO.PK));
			var patternMatchingPhones = Factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, BizO.PK));
			var patternMatchingRegCodes = Factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, BizO.PK));

			var valueEmailToHash = "edward@wisetechglobal.com".ToUpperInvariant();
			var patternEmail = patternMatchingEmails[0];

			AssertEquals(0, patternMatchingAddresses.Length);
			AssertEquals(0, patternMatchingDomains.Length);
			AssertEquals(1, patternMatchingEmails.Length);
			AssertEquals(0, patternMatchingNames.Length);
			AssertEquals(0, patternMatchingPhones.Length);
			AssertEquals(0, patternMatchingRegCodes.Length);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueEmailToHash), patternEmail.PME_HashedValue);
			AssertEquals(contactItem.PK, patternEmail.PME_ParentId);
		}

		protected override void SetupBizO()
		{
			BizO.OI_ContactItemType = OrgContactItemTypes.Codes.Email;
			BizO.OI_Address = "edward@wisetechglobal.com";
			Factory.Save();
		}
	}
}
