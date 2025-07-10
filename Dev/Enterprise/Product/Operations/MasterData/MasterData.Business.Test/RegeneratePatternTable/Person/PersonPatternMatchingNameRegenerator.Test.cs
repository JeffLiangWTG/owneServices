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
	public class PersonPatternMatchingNameRegeneratorForTest : PersonPatternMatchingNameRegenerator
	{
		public PersonPatternMatchingNameRegeneratorForTest(PatternMatchingRecalculator<GlbPerson> recalculator) : base(recalculator)
		{
		}

		public List<string> TablesPrefixListForTest => TablesPrefixList;
	}
	public class PersonPatternMatchingNameRegeneratorTest : PatternGeneratorForPersonTest<PersonPatternMatchingNameRegeneratorForTest, PatternMatchingName>
	{
		protected override SchemaGuidColumn PatternMatchingPersonColumn { get { return PatternMatchingNameSchema.PMN_PER; } }

		protected override SchemaStringColumn PatternMatchingParentTableCodeColumn { get { return PatternMatchingNameSchema.PMN_ParentTableCode; } }

		public void TestGenerateWithSecurity()
		{
			bool oldValue = Env.Security.PersonIntelligenceViewHomeAddress.IsAllowed;
			try
			{
				Env.Security.PersonIntelligenceViewHomeAddress.IsAllowed = false;
				Person.PER_FullName = "Fake Name";
				Contact1.OC_ContactName = "Fake Name2";
				Staff1.GS_FullName = "Fake Name3";
				HRJobApplicant1.HA_FullName = "Fake Name4";
				Contact2.OC_ContactName = "Fake Name5";
				Staff2.GS_FullName = "Fake Name6";
				HRJobApplicant2.HA_FullName = "Fake Name7";

				Factory.Save();

				var patternMatchingRecalculator = new PatternMatchingRecalculator<GlbPerson>(Person);
				var patternMatchingNameGenerator = new PersonPatternMatchingNameRegeneratorForTest(patternMatchingRecalculator);

				patternMatchingNameGenerator.InitializeDataCount(Person, Factory);

				AssertNoExceptionThrown(() =>
				{
					patternMatchingNameGenerator.Regenerate(Person, Factory);
				});

				Person.PER_FullName = "New Name";
				Contact1.OC_ContactName = "New Name2";
				Staff1.GS_FullName = "New Name3";
				HRJobApplicant1.HA_FullName = "New Name4";
				Contact2.OC_ContactName = "New Name5";
				Staff2.GS_FullName = "New Name6";
				HRJobApplicant2.HA_FullName = "New Name7";

				Factory.Save();

				patternMatchingNameGenerator = new PersonPatternMatchingNameRegeneratorForTest(patternMatchingRecalculator);
				patternMatchingNameGenerator.InitializeDataCount(Person, Factory);

				AssertNoExceptionThrown(() =>
				{
					patternMatchingNameGenerator.Regenerate(Person, Factory);
				});
			}
			finally
			{
				Env.Security.PersonIntelligenceViewHomeAddress.IsAllowed = oldValue;
			}
		}

		public void TestGenerateWithoutDummyContactToSuppressDocs()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "DUMMY CONTACT TO SUPPRESS DOCS";
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "DUMMY CONTACT TO SUPPRESS DOCS";
			contact.OC_PER = Person.PK;

			Factory.Save();

			var patternMatchingNameGenerator = new PersonPatternMatchingNameRegeneratorForTest(new PatternMatchingRecalculator<GlbPerson>(person));

			AssertEquals("Expected: DUMMY CONTACT TO SUPPRESS DOCS should be filtered", 0, patternMatchingNameGenerator.InitializeDataCount(person, Factory));
		}

		public override void TestGenerate()
		{
			#region Set up data & Test Add New Data

			Person.PER_FullName = "Fake Name";
			Contact1.OC_ContactName = "Fake Name2";
			Staff1.GS_FullName = "Fake Name3";
			HRJobApplicant1.HA_FullName = "Fake Name4";
			Contact2.OC_ContactName = "Fake Name5";
			Staff2.GS_FullName = "Fake Name6";
			HRJobApplicant2.HA_FullName = "Fake Name7";

			Factory.Save();

			var patternMatchingRecalculator = new PatternMatchingRecalculator<GlbPerson>(Person);
			var patternMatchingNameGenerator = new PersonPatternMatchingNameRegeneratorForTest(patternMatchingRecalculator);

			patternMatchingNameGenerator.InitializeDataCount(Person, Factory);
			patternMatchingNameGenerator.Regenerate(Person, Factory);

			var query = new ZQuery(PatternMatchingNameSchema.PMN_PER, Person.PK);
			query.AddToFilter(PatternMatchingNameSchema.PMN_ParentTableCode, patternMatchingNameGenerator.TablesPrefixListForTest);

			var result = Factory.Load<PatternMatchingName>(query);
			AssertEquals("Expected:create correct number of patterns.", 5, result.Length);
			AssertContainsExactElementsInAnyOrder("Expected: hash values must be equal ", GetHashedValue(Person), result.Select(u => (int)u.PMN_HashedValue));

			#endregion

			#region Change Data

			Person.PER_FullName = "New Name";
			Contact1.OC_ContactName = "New Name2";
			Staff1.GS_FullName = "New Name3";
			HRJobApplicant1.HA_FullName = "New Name4";
			Contact2.OC_ContactName = "New Name5";
			Staff2.GS_FullName = "New Name6";
			HRJobApplicant2.HA_FullName = "New Name7";

			Factory.Save();

			#endregion

			#region Regenerate

			patternMatchingNameGenerator = new PersonPatternMatchingNameRegeneratorForTest(patternMatchingRecalculator);
			var totalCount = patternMatchingNameGenerator.InitializeDataCount(Person, Factory);
			var effectCount = patternMatchingNameGenerator.Regenerate(Person, Factory);

			#endregion

			#region Assert

			result = Factory.Load<PatternMatchingName>(query);
			AssertEquals("Expected: should generate correct numbers of name patterns ", 5, result.Length);
			AssertEquals("Expected: plan numbers must equal actual numbers ", totalCount, effectCount);
			AssertContainsExactElementsInAnyOrder("Expected: hash values must be equal ", GetHashedValue(Person), result.Select(u => (int)u.PMN_HashedValue));

			#endregion

			#region Need Delete Data

			CreateRecord(ZGuid.NewZGuid(), OrgContactSchema.Constants.Prefix);

			Factory.Save();

			result = Factory.Load<PatternMatchingName>(query);
			AssertEquals("Expected: should generate correct numbers of name patterns ", 6, result.Length);

			#endregion

			#region Regenerate

			patternMatchingNameGenerator = new PersonPatternMatchingNameRegeneratorForTest(patternMatchingRecalculator);
			totalCount = patternMatchingNameGenerator.InitializeDataCount(Person, Factory);
			effectCount = patternMatchingNameGenerator.Regenerate(Person, Factory);

			#endregion

			#region Assert

			result = Factory.Load<PatternMatchingName>(query);
			AssertEquals("Expected: should generate correct numbers of name patterns ", 5, result.Length);
			AssertEquals("Expected: plan numbers must equal actual numbers ", totalCount, effectCount);
			AssertContainsExactElementsInAnyOrder("Expected: hash values must be equal ", GetHashedValue(Person), result.Select(u => (int)u.PMN_HashedValue));

			#endregion
		}

		protected override void AssertResult(PatternMatchingName[] result)
		{
			AssertEquals("Expected: Placeholder name is not added", 0, result.Length);
		}

		protected override void CreateExtraBusinessObjects()
		{
			Staff1.GS_FullName = "TEST TEST";
			Contact1.OC_ContactName = "TEST TEST";
			Contact2.OC_ContactName = "TEST";
			Staff2.GS_FullName = "TEST";
			HRJobApplicant1.HA_FullName = "TEST";
			HRJobApplicant2.HA_FullName = "TEST";
			Person.UpdateFromApplicant(HRJobApplicant2);
			Factory.Save(); // Will update the person name
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
			var patternRecord = Factory.NewWithValidTestData<PatternMatchingName>();

			patternRecord.PMN_HashedValue = 2238232;
			patternRecord.PMN_IsActive = true;
			patternRecord.PMN_PER = Person.PK;
			patternRecord.PMN_ParentId = parentId;
			patternRecord.PMN_ParentTableCode = parentTableCode;
			patternRecord.PMN_RN_NKCountryCode = Person.PER_RN_NKCountry;
		}

		protected override string[] GetValueToHash(GlbPerson person)
		{
			return new[] { TextStandardizerHelper.StandardizePersonName(person.PER_FullName) };
		}

		protected override string[] GetValueToHash(OrgContact contact)
		{
			return new[] { TextStandardizerHelper.StandardizePersonName(contact?.OC_ContactName ?? ZString.Empty) };
		}

		protected override string[] GetValueToHash(GlbStaff staff)
		{
			return new[] { TextStandardizerHelper.StandardizePersonName(staff?.GS_FullName ?? ZString.Empty) };
		}

		protected override string[] GetValueToHash(IHRJobApplicant applicant)
		{
			return new[] { string.Empty };
		}
	}
}
