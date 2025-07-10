using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.OrgMatching.Testing
{
	class OrganizationAddressMatchEngineTest : TestCaseWithFactory
	{
		public void TestFindDuplicationsByDeduplicationOrgHeader_ScoreHigh()
		{
			var targetOrg = Factory.NewWithValidTestData<OrgHeader>();
			targetOrg.OH_Code = "TESTYO2";
			targetOrg.OH_FullName = "ORGANISATION";
			targetOrg.CustomsCodes.AddNew("ATF", "1234F", "AU");

			var targetAddress = targetOrg.MainAddress;
			targetAddress.Address1 = "PADDINGTON NSW";
			targetAddress.OA_Email = "ABCD@TEST.COM";
			targetAddress.OA_Phone = "1504444444";

			var targetContact = targetOrg.Contacts.AddNew();
			targetContact.OC_ContactName = "Jimmy Yang";
			targetContact.OC_Email = "ABCD@TEST.COM";
			targetContact.OC_Phone = "1504444444";

			AssertScorerResult(1, ConfidenceRating.High, targetOrg, targetAddress);
		}

		public void TestFindDuplicationsByDeduplicationOrgHeader_ScoreMedium()
		{
			var targetOrg = Factory.NewWithValidTestData<OrgHeader>();
			targetOrg.OH_Code = "TESTYO2";
			targetOrg.OH_FullName = "ORGANISATION";
			targetOrg.CustomsCodes.AddNew("ATF", "1234F", "AU");

			var targetAddress = targetOrg.MainAddress;
			targetAddress.Address1 = "PADDINGTON NSW";
			targetAddress.OA_Email = "DDD@FADR.COM";
			targetAddress.OA_Phone = "5551231234";

			var targetContact = targetOrg.Contacts.AddNew();
			targetContact.OC_ContactName = "Jimmy Yang";
			targetContact.OC_Email = "DDD@FADR.COM";
			targetContact.OC_Phone = "5551231234";

			AssertScorerResult(1, ConfidenceRating.Medium, targetOrg, targetAddress);
		}

		public void TestFindDuplicationsByDeduplicationOrgHeader_ScoreLow()
		{
			var targetOrg = Factory.NewWithValidTestData<OrgHeader>();
			targetOrg.OH_Code = "TESTYO2";
			targetOrg.OH_FullName = "ORGANISATION";
			targetOrg.CustomsCodes.AddNew("ATF", "55334", "AU");

			var targetAddress = targetOrg.MainAddress;
			targetAddress.Address1 = "PADDINGTON NSW";
			targetAddress.OA_Email = "DDD@FADR.COM";
			targetAddress.OA_Phone = "5551231234";

			var targetContact = targetOrg.Contacts.AddNew();
			targetContact.OC_ContactName = "Peter Zhang";
			targetContact.OC_Email = "DDD@FADR.COM";
			targetContact.OC_Phone = "5551231234";

			AssertScorerResult(0, ConfidenceRating.Low, targetOrg, targetAddress);
		}

		public void TestNotThrowExceptionWhenDataIsEmpty()
		{
			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var duplicationFinder = new OrgHeaderDuplicationFinder(OrganizationAddressTransformHelper.GetDeduplicationOrgHeader(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance), Factory), false, false);
				AssertNoExceptionThrown(() => duplicationFinder.FindPotentialDuplicates(false));

				duplicationFinder = new OrgHeaderDuplicationFinder(OrganizationAddressTransformHelper.GetDeduplicationOrgHeader(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { Contact = "HAHA" }, Factory), false, false);
				AssertNoExceptionThrown(() => duplicationFinder.FindPotentialDuplicates(false));

				DeduplicationOrgHeader deduplicationOrgHeader = null;
				duplicationFinder = new OrgHeaderDuplicationFinder(deduplicationOrgHeader, false, false);
				AssertNoExceptionThrown(() => duplicationFinder.FindPotentialDuplicates(false));
			}
		}

		public void TestCanFindDuplicateWhenNotContainPhoneInfo()
		{
			var targetOrg = Factory.NewWithValidTestData<OrgHeader>();
			targetOrg.OH_Code = "TESTYO2";
			targetOrg.OH_FullName = "ORGANISATION";

			var targetAddress = targetOrg.MainAddress;
			targetAddress.Address1 = "PADDINGTON NSW";

			var patternMatchingName = Factory.New<PatternMatchingName>();
			patternMatchingName.PMN_OH = targetOrg.PK;
			patternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(targetOrg.OH_FullName);
			patternMatchingName.PMN_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			patternMatchingName.PMN_ParentId = targetOrg.PK;
			patternMatchingName.PMN_RN_NKCountryCode = "AU";

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var duplicationFinder = new OrgHeaderDuplicationFinder(OrganizationAddressTransformHelper.GetDeduplicationOrgHeader(GetOrganizationAddress(false), Factory), false, false);
				var duplicates = duplicationFinder.FindPotentialDuplicates(false).ToList();

				AssertEquals(1, duplicates.Count);
				AssertEquals(ConfidenceRating.High, duplicates[0].ConfidenceRating);
				AssertEquals(2, duplicates[0].ChildResults.Count());
			}
		}

		#region Implementation

		void AssertScorerResult(int duplicatesCount, ConfidenceRating confidenceRating, OrgHeader targetOrg, OrgAddress targetAddress)
		{
			var hashedAddress = TextStandardizerHelper.ComputeStringHashFast(targetAddress.Address1);
			var hashedEmail = TextStandardizerHelper.ComputeStringHashFast(targetAddress.OA_Email);
			var hashedName = TextStandardizerHelper.ComputeStringHashFast(targetOrg.OH_FullName);
			var hashedRegCode = TextStandardizerHelper.ComputeStringHashFast("1234F");

			var patternMatchingName = Factory.New<PatternMatchingName>();
			patternMatchingName.PMN_OH = targetOrg.PK;
			patternMatchingName.PMN_HashedValue = hashedName;
			patternMatchingName.PMN_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			patternMatchingName.PMN_ParentId = targetOrg.PK;
			patternMatchingName.PMN_RN_NKCountryCode = "AU";

			var patternMatchingAddress = Factory.New<PatternMatchingEmail>();
			patternMatchingAddress.PME_OH = targetOrg.PK;
			patternMatchingAddress.PME_HashedValue = hashedEmail;
			patternMatchingAddress.PME_ParentTableCode = OrgAddressSchema.Constants.Prefix;
			patternMatchingAddress.PME_ParentId = targetAddress.PK;
			patternMatchingAddress.PME_RN_NKCountryCode = "AU";

			var patternMathcingEmail = Factory.New<PatternMatchingAddress>();
			patternMathcingEmail.PMA_OH = targetOrg.PK;
			patternMathcingEmail.PMA_HashedValue = hashedAddress;
			patternMathcingEmail.PMA_ParentTableCode = OrgAddressSchema.Constants.Prefix;
			patternMathcingEmail.PMA_ParentId = targetAddress.PK;
			patternMathcingEmail.PMA_RN_NKCountryCode = "AU";

			var patternMathchingRegCode = Factory.New<PatternMatchingRegCode>();
			patternMathchingRegCode.PMR_OH = targetOrg.PK;
			patternMathchingRegCode.PMR_HashedValue = hashedRegCode;
			patternMathchingRegCode.PMR_ParentTableCode = OrgCusCodeSchema.Constants.Prefix;
			patternMathchingRegCode.PMR_ParentId = targetAddress.PK;
			patternMathchingRegCode.PMR_RN_NKCountryCode = "AU";

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var duplicationFinder = new OrgHeaderDuplicationFinder(OrganizationAddressTransformHelper.GetDeduplicationOrgHeader(GetOrganizationAddress(), Factory), false, false);
				var duplicates = duplicationFinder.FindPotentialDuplicates(false).ToList();

				AssertEquals(duplicatesCount, duplicates.Count);

				if (duplicatesCount > 0)
				{
					AssertEquals(confidenceRating, duplicates[0].ConfidenceRating);
					AssertEquals(4, duplicates[0].ChildResults.Count());
				}
			}
		}

		OrganizationAddress GetOrganizationAddress(bool hasExtraInfo = true)
		{
			var organizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "TestType",
				AddressShortCode = "THEMOMENT",
				AddressOverride = false,

				OrganizationCode = "TESTYO2",
				CompanyName = "ORGANISATION",
				Address1 = "PADDINGTON NSW",
				Address2 = "",
				City = "",
				State = "",
				Postcode = "",
				Port = new UNLOCO { Code = "AUMEL", Name = "Melbourne" },
				Country = new Country { Code = "AU", Name = "Australia" },

				Fax = "",
				Mobile = ""
			};

			if (hasExtraInfo)
			{
				organizationAddress.Contact = "Jimmy Yang";
				organizationAddress.Email = "ABCD@TEST.COM";
				organizationAddress.Phone = "1504444444";

				organizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>()
				{
					new UniversalDataBuss.DataObjects.Universal.RegistrationNumber()
					{
						Type = new RegistrationNumberType { Code = "ATF", Description = "Approved Transitional Facility" },
						CountryOfIssue = new Country { Code = "AU", Name = "Australia" },
						Value = "1234F",
					}
				});
			}

			return organizationAddress;
		}

		#endregion
	}
}
