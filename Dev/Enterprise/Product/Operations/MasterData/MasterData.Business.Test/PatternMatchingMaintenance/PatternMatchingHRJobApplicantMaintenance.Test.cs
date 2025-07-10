using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	class PatternMatchingHRJobApplicantMaintenanceTest : PatternMatchingSourceTest<HRJobApplicant, PatternMatchingHRJobApplicantMaintenance>
	{
		protected override HRJobApplicant BizO => person ?? (person = Factory.New<HRJobApplicant>());
		HRJobApplicant person;

		protected override PatternMatchingHRJobApplicantMaintenance MaintenanceObject => new PatternMatchingHRJobApplicantMaintenance(BizO);

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
			var valueAddressToHash = (BizO.HA_UserAddress1 + BizO.HA_UserAddress2 + BizO.HA_City + BizO.HA_Postcode + BizO.HA_State).ToUpperInvariant();

			var patternDomain = patternMatchingDomains[0];
			var patternEmail = patternMatchingEmails[0];
			var patternName = patternMatchingNames[0];
			var patternAddress = patternMatchingAddresses[0];
			var patternRegcode = patternMatchingRegCodes[0];

			AssertEquals(1, patternMatchingAddresses.Length);
			AssertEquals(1, patternMatchingDomains.Length);
			AssertEquals(1, patternMatchingEmails.Length);
			AssertEquals(1, patternMatchingNames.Length);
			AssertEquals(4, patternMatchingPhones.Length);
			AssertEquals(1, patternMatchingRegCodes.Length);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueDomainToHash), patternDomain.PMD_HashedValue);
			AssertEquals(person.PK, patternDomain.PMD_ParentId);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueEmailToHash), patternEmail.PME_HashedValue);
			AssertEquals(person.PK, patternEmail.PME_ParentId);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueNameToHash), patternName.PMN_HashedValue);
			AssertEquals(person.PK, patternName.PMN_ParentId);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueAddressToHash), patternAddress.PMA_HashedValue);
			AssertEquals(person.PK, patternAddress.PMA_ParentId);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast((BizO.HA_Birthdate.ToDateTime() - new DateTime(1753, 1, 1)).Days.ToString(CultureInfo.InvariantCulture)), patternRegcode.PMR_HashedValue);
			AssertEquals(person.PK, patternRegcode.PMR_ParentId);

			AssertContainsExactElementsInAnyOrder(
				new ZInt[]
				{
					TextStandardizerHelper.ComputeStringHashFast("61449743938"),
					TextStandardizerHelper.ComputeStringHashFast("2889232"),
					TextStandardizerHelper.ComputeStringHashFast("612690138438"),
					TextStandardizerHelper.ComputeStringHashFast("613038838992")
				},
				Factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, person.PK))
					.Select(p => p.PMP_HashedValue).ToArray());
			AssertEquals(person.PK, patternMatchingPhones[0].PMP_ParentId);
		}

		protected override void SetupBizO()
		{
			BizO.HA_EmailAddress = "edward@wisetechglobal.com";
			BizO.HA_MobilePhone = "+61449743938";
			BizO.HA_FaxNum = "2889232";
			BizO.HA_WorkPhone = "+612690138438";
			BizO.HA_HomePhone = "+613038838992";
			BizO.HA_FullName = "Edward Mills";

			BizO.HA_UserAddress1 = "72 O'Riordan Street";
			BizO.HA_UserAddress2 = "";
			BizO.HA_City = "SYDNEY";
			BizO.HA_Postcode = "2015";
			BizO.HA_State = "NSW";

			BizO.HA_Birthdate = new ZDate(1990, 1, 1);

			Factory.Save();
		}
	}
}
