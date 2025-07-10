using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	class PatternMatchingOrgContactMaintenanceTest : PatternMatchingSourceTest<OrgContact, PatternMatchingOrgContactMaintenance>
	{
		protected override OrgContact BizO => contact ?? (contact = CreateOrganisation().Contacts.AddNew());
		OrgContact contact;

		public void TestCreateOrUpdatePatternMatchingTablesWithoutDummyContactToSuppressDocs()
		{
			var contact = CreateOrganisation().Contacts.AddNew();
			contact.OC_ContactName = "DUMMY CONTACT TO SUPPRESS DOCS";

			Factory.Save();

			AssertEquals(expected: false, new PatternMatchingOrgContactMaintenance(contact).CreateOrUpdatePatternMatchingTables());
			AssertEquals(0, Factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, contact.PK)).Length);
		}

		protected override PatternMatchingOrgContactMaintenance MaintenanceObject => new PatternMatchingOrgContactMaintenance(BizO);

		protected override void AssertPatternMatchingDataExists()
		{
			var patternMatchingAddresses = Factory.Load<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, BizO.PK));
			var patternMatchingDomains = Factory.Load<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, BizO.PK));
			var patternMatchingEmails = Factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, BizO.PK));
			var patternMatchingNames = Factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, BizO.PK));
			var patternMatchingPhones = Factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, BizO.PK));
			var patternMatchingRegCodes = Factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, BizO.PK));

			var valueDomainToHash = "wisetechglobal.com".ToUpperInvariant();
			var valueEmailToHash = "edward@wisetechglobal.com".ToUpperInvariant();
			var valueNameToHash = "Edward Mills".ToUpperInvariant();

			var patternDomain = patternMatchingDomains[0];
			var patternEmail = patternMatchingEmails[0];
			var patternName = patternMatchingNames[0];

			AssertEquals(0, patternMatchingAddresses.Length);
			AssertEquals(1, patternMatchingDomains.Length);
			AssertEquals(1, patternMatchingEmails.Length);
			AssertEquals(1, patternMatchingNames.Length);
			AssertEquals(5, patternMatchingPhones.Length);
			AssertEquals(0, patternMatchingRegCodes.Length);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueDomainToHash), patternDomain.PMD_HashedValue);
			AssertEquals(contact.PK, patternDomain.PMD_ParentId);
			AssertEquals(ZGuid.Empty, patternDomain.PMD_PER);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueEmailToHash), patternEmail.PME_HashedValue);
			AssertEquals(contact.PK, patternEmail.PME_ParentId);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueNameToHash), patternName.PMN_HashedValue);
			AssertEquals(contact.PK, patternName.PMN_ParentId);

			AssertContainsExactElementsInAnyOrder(
				new ZInt[]
				{
					TextStandardizerHelper.ComputeStringHashFast("61449743938"),
					TextStandardizerHelper.ComputeStringHashFast("2889232"),
					TextStandardizerHelper.ComputeStringHashFast("61416021377"),
					TextStandardizerHelper.ComputeStringHashFast("612690138438"),
					TextStandardizerHelper.ComputeStringHashFast("613038838992")
				},
				Factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, contact.PK))
					.Select(p => p.PMP_HashedValue).ToArray());
			AssertEquals(contact.PK, patternMatchingPhones[0].PMP_ParentId);
		}

		protected override void SetupBizO()
		{
			BizO.OC_Email = "edward@wisetechglobal.com";
			BizO.OC_Phone = "+61449743938";
			BizO.OC_Fax = "2889232";
			BizO.OC_Mobile = "+61416021377";
			BizO.OC_OtherPhone = "+612690138438";
			BizO.OC_HomePhone = "+613038838992";
			BizO.OC_ContactName = "Edward Mills";

			Factory.Save();
		}
	}
}
