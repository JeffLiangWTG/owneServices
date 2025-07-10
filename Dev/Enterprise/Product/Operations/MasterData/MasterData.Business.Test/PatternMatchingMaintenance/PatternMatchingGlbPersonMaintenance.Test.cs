using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	class PatternMatchingGlbPersonMaintenanceTest : PatternMatchingSourceTest<GlbPerson, PatternMatchingGlbPersonMaintenance>
	{
		protected override GlbPerson BizO => person ?? (person = Factory.NewWithValidTestData<GlbPerson>());
		GlbPerson person;

		protected override PatternMatchingGlbPersonMaintenance MaintenanceObject => new PatternMatchingGlbPersonMaintenance(BizO);

		public void TestCreateOrUpdatePatternMatchingTablesWithoutDummyContactToSuppressDocs()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "DUMMY CONTACT TO SUPPRESS DOCS";

			Factory.Save();

			AssertEquals(expected: false, new PatternMatchingGlbPersonMaintenance(person).CreateOrUpdatePatternMatchingTables());
			AssertEquals(0, Factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, person.PK)).Length);
		}

		protected override void AssertPatternMatchingDataExists()
		{
			var patternMatchingAddresses = Factory.Load<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, BizO.PK));
			var patternMatchingDomains = Factory.Load<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, BizO.PK));
			var patternMatchingEmails = Factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, BizO.PK));
			var patternMatchingNames = Factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, BizO.PK));
			var patternMatchingPhones = Factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, BizO.PK));
			var patternMatchingRegCodes = Factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, BizO.PK));

			var valueEmailToHash = "edward@wisetechglobal.com".ToUpperInvariant();
			var valueNameToHash = "Edward Mills".ToUpperInvariant();
			var valueAddressToHash = (BizO.PER_HomeAddress1 + BizO.PER_HomeAddress2 + BizO.PER_City + BizO.PER_Postcode + BizO.PER_State).ToUpperInvariant();

			var patternEmail = patternMatchingEmails[0];
			var patternName = patternMatchingNames[0];
			var patternAddress = patternMatchingAddresses[0];
			var patternRegcode = patternMatchingRegCodes[0];

			AssertEquals(1, patternMatchingAddresses.Length);
			AssertEquals(0, patternMatchingDomains.Length);
			AssertEquals(1, patternMatchingEmails.Length);
			AssertEquals(1, patternMatchingNames.Length);
			AssertEquals(4, patternMatchingPhones.Length);
			AssertEquals(1, patternMatchingRegCodes.Length);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueEmailToHash), patternEmail.PME_HashedValue);
			AssertEquals(person.PK, patternEmail.PME_ParentId);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueNameToHash), patternName.PMN_HashedValue);
			AssertEquals(person.PK, patternName.PMN_ParentId);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueAddressToHash), patternAddress.PMA_HashedValue);
			AssertEquals(person.PK, patternAddress.PMA_ParentId);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast("1"), patternRegcode.PMR_HashedValue);
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

		public void TestAccessingCountryCode_WithoutAccessRights_ShouldNotShowException()
		{
			var regularUser = Factory.NewWithValidTestData<GlbStaff>();
			var targetPerson = Factory.NewWithValidTestData<GlbPerson>();

			regularUser.GS_RN_NKCountryCode = "AU";
			targetPerson.PER_RN_NKCountry = "NZ";
			targetPerson.PER_FullName = "Peter Luis";

			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily(regularUser.GS_LoginName))
			{
				var patternMatchMaintenance = new PatternMatchingGlbPersonMaintenance(targetPerson);
				ExceptionReporterTestListener.Instance.Clear();
				CombineAssertions(() =>
				{
					AssertEquals("Precondition: No errors reported", 0, ExceptionReporterTestListener.Instance.Count);
					AssertEquals("Precondition: Staff shouldn't be able to view by default", "** View Denied **", targetPerson.PER_RN_NKCountry);
					AssertEquals("Precondition: We can bypass security", "NZ", targetPerson.PER_RN_NKCountryInternal);
					AssertEquals("Precondition: Security checkpoint should not be allowed", false, Env.Security.PersonIntelligenceViewHomeAddress.IsAllowed);
				});

				try
				{
					patternMatchMaintenance.CreateOrUpdatePatternMatchingTables();
				}
				finally
				{
					AssertEquals("Error should not be reported", 0, ExceptionReporterTestListener.Instance.Count);
				}
			}
		}

		public void TestAccessingAddressInformation_WithoutAccessRights_ProcessCorrectly()
		{
			var userWithoutSecurity = Factory.NewWithValidTestData<GlbStaff>();
			var targetPerson = Factory.NewWithValidTestData<GlbPerson>();
			targetPerson.PER_HomeAddress1 = "User Address 123";
			targetPerson.PER_HomeAddress2 = "User Address 234";
			targetPerson.PER_City = "SYD";
			targetPerson.PER_State = "DUMMY";
			targetPerson.PER_RN_NKCountry = "AU";
			targetPerson.PER_Postcode = "ABC";

			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily(userWithoutSecurity.GS_LoginName))
			{
				var patternMatchMaintenance = new PatternMatchingGlbPersonMaintenance(targetPerson);
				AssertEquals("Precondition: Security checkpoint should not be allowed", false, Env.Security.StaffViewHomeAddressDetails.IsAllowed);
				AssertEquals(true, patternMatchMaintenance.CreateOrUpdatePatternMatchingTables());

				var patternMatchingAddress = Factory.LoadTop1<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, targetPerson.PK));
				CombineAssertions(() =>
				{
					AssertNotNull(patternMatchingAddress);
					AssertEquals(TextStandardizerHelper.ComputeStringHashFast("USER ADDRESS 123USER ADDRESS 234SYDABCDUMMY"), patternMatchingAddress.PMA_HashedValue);
					AssertEquals("AU", patternMatchingAddress.PMA_RN_NKCountryCode);
				});
			}
		}

		protected override void SetupBizO()
		{
			BizO.PER_EmailAddress = "edward@wisetechglobal.com";

			BizO.PER_MobilePhone = "+61449743938";
			BizO.PER_FaxNumber = "2889232";
			BizO.PER_MobilePhone2 = "+612690138438";
			BizO.PER_HomePhone = "+613038838992";
			BizO.PER_FullName = "Edward Mills";

			BizO.PER_HomeAddress1 = "72 O'Riordan Street";
			BizO.PER_HomeAddress2 = "";
			BizO.PER_City = "SYDNEY";
			BizO.PER_Postcode = "2015";
			BizO.PER_State = "NSW";

			BizO.PER_BirthDate = new ZDate(1753, 1, 2);

			Factory.Save();
		}
	}
}
