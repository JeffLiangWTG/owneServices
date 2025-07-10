using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	public class PersonPatternMatchingEmailRegeneratorForTest : PersonPatternMatchingEmailRegenerator
	{
		public PersonPatternMatchingEmailRegeneratorForTest(PatternMatchingRecalculator<GlbPerson> recalculator) : base(recalculator)
		{
		}

		public List<string> TablesPrefixListForTest => TablesPrefixList;
	}

	public class PersonPatternMatchingEmailRegeneratorTest : PatternGeneratorForPersonTest<PersonPatternMatchingEmailRegeneratorForTest, PatternMatchingEmail>
	{
		protected override SchemaGuidColumn PatternMatchingPersonColumn { get { return PatternMatchingEmailSchema.PME_PER; } }

		protected override SchemaStringColumn PatternMatchingParentTableCodeColumn { get { return PatternMatchingEmailSchema.PME_ParentTableCode; } }

		public void TestGenerateWithSecurity()
		{
			bool oldValue = Env.Security.PersonIntelligenceViewHomeAddress.IsAllowed;
			try
			{
				Env.Security.PersonIntelligenceViewHomeAddress.IsAllowed = false;
				using (Person.GetValidationSuspender())
				{
					Person.PER_EmailAddress = "fake1@wisetechglobal.com";
					Contact1.OC_Email = "fake2@wisetechglobal.com";
					Staff1.GS_EmailAddress = "fake3@wisetechglobal.com";
					Applicant1["HA_EmailAddress"] = "fake4@wisetechglobal.com";
					Contact2.OC_Email = "fake2@wisetechglobal2.com";
					Staff2.GS_EmailAddress = "fake3@wisetechglobal2.com";
					Applicant2["HA_EmailAddress"] = "fake4@wisetechglobal2.com";
				}

				Factory.Save();

				var patternMatchingRecalculator = new PatternMatchingRecalculator<GlbPerson>(Person);
				var patternMatchingEmailGenerator = new PersonPatternMatchingEmailRegeneratorForTest(patternMatchingRecalculator);

				patternMatchingEmailGenerator.InitializeDataCount(Person, Factory);

				AssertNoExceptionThrown(() =>
				{
					patternMatchingEmailGenerator.Regenerate(Person, Factory);
				});

				Person.PER_RN_NKCountry = "CN";
				Person.PER_EmailAddress2 = "Bell@www.com";
				Contact1.OC_Email = "obj1@wisetechglobal.com";
				Contact1.Header.OH_RL_NKClosestPort = "NZAKL";
				Staff1.GS_EmailAddress = "obj2@wisetechglobal.com";
				Applicant1["HA_EmailAddress"] = "obj3@wisetechglobal.com";
				Contact2.OC_Email = "obj1@wisetechglobal2.com";
				Contact2.Header.OH_RL_NKClosestPort = "NZAKL";
				Staff2.GS_EmailAddress = "obj2@wisetechglobal2.com";
				Applicant2["HA_EmailAddress"] = "obj3@wisetechglobal2.com";

				Factory.Save();

				patternMatchingEmailGenerator = new PersonPatternMatchingEmailRegeneratorForTest(patternMatchingRecalculator);
				patternMatchingEmailGenerator.InitializeDataCount(Person, Factory);

				AssertNoExceptionThrown(() =>
				{
					patternMatchingEmailGenerator.Regenerate(Person, Factory);
				});
			}
			finally
			{
				Env.Security.PersonIntelligenceViewHomeAddress.IsAllowed = oldValue;
			}
		}

		public override void TestGenerate()
		{
			#region Set up data & Test Add New Data

			using (Person.GetValidationSuspender())
			{
				Person.PER_EmailAddress = "fake1@wisetechglobal.com";
				Contact1.OC_Email = "fake2@wisetechglobal.com";
				Staff1.GS_EmailAddress = "fake3@wisetechglobal.com";
				Applicant1["HA_EmailAddress"] = "fake4@wisetechglobal.com";
				Contact2.OC_Email = "fake2@wisetechglobal2.com";
				Staff2.GS_EmailAddress = "fake3@wisetechglobal2.com";
				Applicant2["HA_EmailAddress"] = "fake4@wisetechglobal2.com";
			}

			Factory.Save();

			var patternMatchingRecalculator = new PatternMatchingRecalculator<GlbPerson>(Person);
			var patternMatchingEmailGenerator = new PersonPatternMatchingEmailRegeneratorForTest(patternMatchingRecalculator);

			patternMatchingEmailGenerator.InitializeDataCount(Person, Factory);
			patternMatchingEmailGenerator.Regenerate(Person, Factory);
			var query = new ZQuery(PatternMatchingEmailSchema.PME_PER, Person.PK);
			query.AddToFilter(PatternMatchingEmailSchema.PME_ParentTableCode, patternMatchingEmailGenerator.TablesPrefixListForTest);
			var result = Factory.Load<PatternMatchingEmail>(query);

			AssertEquals("Expected:correct amount of domain patterns should be created.", result.Length, 7);
			AssertContainsExactElementsInAnyOrder("Expected: hash values must be equal ", GetHashedValue(Person), result.Select(u => (int)u.PME_HashedValue));
			AssertNotNull(result.FirstOrDefault(u => u.PME_ParentId == Contact1.PK && u.PME_OH == Contact1.Header.PK && u.PME_RN_NKCountryCode == Contact1.Header.CountryCode));
			AssertNotNull(result.FirstOrDefault(u => u.PME_ParentId == Contact2.PK && u.PME_OH == Contact2.Header.PK && u.PME_RN_NKCountryCode == Contact2.Header.CountryCode));

			#endregion

			#region Change Data

			Person.PER_RN_NKCountry = "CN";
			Contact1.OC_Email = "obj1@wisetechglobal.com";
			Contact1.Header.OH_RL_NKClosestPort = "NZAKL";
			Staff1.GS_EmailAddress = "obj2@wisetechglobal.com";
			Applicant1["HA_EmailAddress"] = "obj3@wisetechglobal.com";

			Contact2.OC_Email = "obj1@wisetechglobal2.com";
			Contact2.Header.OH_RL_NKClosestPort = "NZAKL";
			Staff2.GS_EmailAddress = "obj2@wisetechglobal2.com";
			Applicant2["HA_EmailAddress"] = "obj3@wisetechglobal2.com";

			Factory.Save();

			Person.PER_EmailAddress2 = "Bell@www.com";

			Factory.Save();

			#endregion

			#region Regenerate

			patternMatchingEmailGenerator = new PersonPatternMatchingEmailRegeneratorForTest(patternMatchingRecalculator);
			var totalCount = patternMatchingEmailGenerator.InitializeDataCount(Person, Factory);
			var effectCount = patternMatchingEmailGenerator.Regenerate(Person, Factory);

			#endregion

			#region Assert

			result = Factory.Load<PatternMatchingEmail>(query);
			AssertEquals("Expected: the amount of patterns should be correct ", result.Length, 8);
			AssertEquals("Expected: plan numbers must equal actual numbers ", totalCount, effectCount);
			AssertContainsExactElementsInAnyOrder("Expected: hash values must be equal ", GetHashedValue(Person), result.Select(u => (int)u.PME_HashedValue));
			AssertNotNull(result.FirstOrDefault(u => u.PME_ParentId == Contact1.PK && u.PME_OH == Contact1.Header.PK && u.PME_RN_NKCountryCode == Contact1.Header.CountryCode));
			AssertNotNull(result.FirstOrDefault(u => u.PME_ParentId == Contact2.PK && u.PME_OH == Contact2.Header.PK && u.PME_RN_NKCountryCode == Contact2.Header.CountryCode));

			#endregion

			#region Need Delete Data

			CreateRecord(ZGuid.NewZGuid(), GlbStaffSchema.Constants.Prefix);

			Factory.Save();

			result = Factory.Load<PatternMatchingEmail>(query);
			AssertEquals("Expected: should generate correct numbers of name patterns ", result.Length, 9);

			Factory.Save();

			#endregion

			#region Regenerate

			patternMatchingEmailGenerator = new PersonPatternMatchingEmailRegeneratorForTest(patternMatchingRecalculator);
			totalCount = patternMatchingEmailGenerator.InitializeDataCount(Person, Factory);
			effectCount = patternMatchingEmailGenerator.Regenerate(Person, Factory);

			#endregion

			#region Assert

			result = Factory.Load<PatternMatchingEmail>(query);
			AssertEquals("Expected: the amount of patterns should be correct ", result.Length, 8);
			AssertEquals("Expected: plan numbers must equal actual numbers ", totalCount, effectCount);
			AssertContainsExactElementsInAnyOrder("Expected: hash values must be equal ", GetHashedValue(Person), result.Select(u => (int)u.PME_HashedValue));

			#endregion
		}

		protected override void AssertResult(PatternMatchingEmail[] result)
		{
			AssertEquals("Expected: Placeholder email is not added", 0, result.Length);
		}

		protected override void CreateExtraBusinessObjects()
		{
			Person.PER_EmailAddress = "Test@TEST.com";
			Contact1.OC_Email = "TEST@test.com";
			Contact2.OC_Email = "TEST@test.com";
			Applicant1["HA_EmailAddress"] = "TEST@test.com";
			Applicant2["HA_EmailAddress"] = "12345@1.com";
			Staff1.GS_EmailAddress = "TEST@test.com";
			Staff2.GS_EmailAddress = "TEST@test.com";
		}

		protected override void CreateExtraPatternMatchingRecords()
		{
			CreateRecord(Person.PK, Person.TablePrefix);
			CreateRecord(Staff1.PK, Staff1.TablePrefix);
			CreateRecord(Contact1.PK, Contact1.TablePrefix);
			CreateRecord(Applicant1.PK, Applicant1.TablePrefix);
			CreateRecord(Staff2.PK, Staff2.TablePrefix);
			CreateRecord(Contact2.PK, Contact2.TablePrefix);
			CreateRecord(Applicant2.PK, Applicant2.TablePrefix);
		}

		void CreateRecord(ZGuid parentId, ZString parentTableCode)
		{
			var patternRecord = Factory.NewWithValidTestData<PatternMatchingEmail>();

			patternRecord.PME_HashedValue = 2238232;
			patternRecord.PME_IsActive = true;
			patternRecord.PME_PER = Person.PK;
			patternRecord.PME_ParentId = parentId;
			patternRecord.PME_ParentTableCode = parentTableCode;
			patternRecord.PME_RN_NKCountryCode = Person.PER_RN_NKCountry;
		}

		protected override string[] GetValueToHash(GlbPerson person)
		{
			return new[] { TextStandardizerHelper.StandardizeEmail(person.PER_EmailAddress), TextStandardizerHelper.StandardizeEmail(person.PER_EmailAddress2) };
		}

		protected override string[] GetValueToHash(OrgContact contact)
		{
			return new[] { TextStandardizerHelper.StandardizeEmail(contact?.OC_Email ?? ZString.Empty) };
		}

		protected override string[] GetValueToHash(GlbStaff staff)
		{
			return new[] { TextStandardizerHelper.StandardizeEmail(staff?.GS_EmailAddress ?? ZString.Empty) };
		}

		protected override string[] GetValueToHash(IHRJobApplicant applicant)
		{
			return new[] { TextStandardizerHelper.StandardizeEmail(applicant?.HA_EmailAddress ?? ZString.Empty) };
		}
	}
}
