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
	public class PersonPatternMatchingPhoneRegeneratorForTest : PersonPatternMatchingPhoneRegenerator
	{
		public PersonPatternMatchingPhoneRegeneratorForTest(PatternMatchingRecalculator<GlbPerson> recalculator) : base(recalculator)
		{
		}

		public List<string> TablesPrefixListForTest => TablesPrefixList;
	}
	public class PersonPatternMatchingPhoneRegeneratorTest : PatternGeneratorForPersonTest<PersonPatternMatchingPhoneRegeneratorForTest, PatternMatchingPhone>
	{
		protected override SchemaGuidColumn PatternMatchingPersonColumn { get { return PatternMatchingPhoneSchema.PMP_PER; } }

		protected override SchemaStringColumn PatternMatchingParentTableCodeColumn { get { return PatternMatchingPhoneSchema.PMP_ParentTableCode; } }

		public override void TestGenerate()
		{
			TestGenerate_Staff();
			TestGenerate_Applicant();
			TestGenerate_Contact();
			TestGenerate_Delete();
		}

		public void TestGenerateWithSecurity()
		{
			bool oldValue = Env.Security.PersonIntelligenceViewHomeAddress.IsAllowed;
			try
			{
				Env.Security.PersonIntelligenceViewHomeAddress.IsAllowed = false;
				AssertNoExceptionThrown(() =>
				{
					ResetPerson();
					Applicant1["HA_FaxNum"] = "+1 (111) 1111-1234";
					Applicant1["HA_MobilePhone"] = "+1 (333) 1111-6543";
					Applicant1["HA_WorkPhone"] = "+1 (222)1111-8756";

					Staff1.GS_MobilePhone = "+1 (111) 1111-1115";
					Staff1.GS_FaxNum = "+1 (111) 1111-1112";

					Contact1.OC_Phone = "111111111114";
					Contact1.OC_HomePhone = "+1 (111) 1111-2222";
					Contact1.Header.OH_RL_NKClosestPort = "CNNJ";

					Person.PER_MobilePhone = "+1 (333) 1111-6543";
					Person.PER_FaxNumber = "+1 (111) 1111-1234";

					Factory.Save();
					var patternMatchingRecalculator = new PatternMatchingRecalculator<GlbPerson>(Person);
					var patternMatchingPhoneGenerator = new PersonPatternMatchingPhoneRegeneratorForTest(patternMatchingRecalculator);

					patternMatchingPhoneGenerator.InitializeDataCount(Person, Factory);
					patternMatchingPhoneGenerator.Regenerate(Person, Factory);

					Applicant1["HA_FaxNum"] = "+1 (211) 1111-1234";
					Applicant1["HA_MobilePhone"] = "+1 (333) 1211-6543";
					Applicant1["HA_WorkPhone"] = "+1 (212)1111-8756";

					Staff1.GS_MobilePhone = "+1 (111) 1111-1111";
					Staff1.GS_FaxNum = "+1 (111) 1111-1122";

					Contact1.OC_Phone = "111111111124";
					Contact1.OC_HomePhone = "+1 (111) 1111-2212";

					Person.PER_MobilePhone = "+1 (333) 1111-6523";
					Person.PER_FaxNumber = "+1 (111) 1111-1214";

					Factory.Save();
					patternMatchingRecalculator = new PatternMatchingRecalculator<GlbPerson>(Person);
					patternMatchingPhoneGenerator = new PersonPatternMatchingPhoneRegeneratorForTest(patternMatchingRecalculator);
					patternMatchingPhoneGenerator.InitializeDataCount(Person, Factory);
					patternMatchingPhoneGenerator.Regenerate(Person, Factory);
				});
			}
			finally
			{
				Env.Security.PersonIntelligenceViewHomeAddress.IsAllowed = oldValue;
			}
		}

		void TestGenerate_Staff()
		{
			#region Set up data & Test Add New Data

			Staff1.GS_MobilePhone = "+1 (111) 1111-1115";
			Staff1.GS_FaxNum = "+1 (111) 1111-1112";

			Person.PER_MobilePhone = "+1 (111) 1111-1115";
			Person.PER_FaxNumber = "+1 (111) 1111-1112";
			Factory.Save();

			var patternMatchingRecalculator = new PatternMatchingRecalculator<GlbPerson>(Person);
			var patternMatchingPhoneGenerator = new PersonPatternMatchingPhoneRegeneratorForTest(patternMatchingRecalculator);

			var query = new ZQuery(PatternMatchingPhoneSchema.PMP_PER, Person.PK);
			query.AddToFilter(PatternMatchingPhoneSchema.PMP_ParentTableCode, patternMatchingPhoneGenerator.TablesPrefixListForTest);

			RegenerateAndAssert(patternMatchingRecalculator, query, 4);

			#endregion

			#region Change Data

			Staff1.GS_FaxNum = string.Empty;
			Staff1.GS_HomePhone = "+1 (111) 2222-3214";
			Staff1.GS_WorkPhone = "+1 (111) 1234-6666";

			Person.PER_FaxNumber = string.Empty;
			Person.PER_HomePhone = "+1 (111) 2222-3214";
			Person.PER_MobilePhone2 = "54767657657";
			Factory.Save();

			RegenerateAndAssert(patternMatchingRecalculator, query, 6); //remove 2 add 4

			#endregion
		}

		void TestGenerate_Applicant()
		{
			#region Set up data & Test Add New Data

			ResetPerson();

			Applicant1["HA_FaxNum"] = "+1 (111) 1111-1234";
			Applicant1["HA_MobilePhone"] = "+1 (333) 1111-6543";
			Applicant1["HA_WorkPhone"] = "+1 (222)1111-8756";

			Person.PER_MobilePhone = "+1 (333) 1111-6543";
			Person.PER_FaxNumber = "+1 (111) 1111-1234";
			Factory.Save();

			var patternMatchingRecalculator = new PatternMatchingRecalculator<GlbPerson>(Person);
			var patternMatchingPhoneGenerator = new PersonPatternMatchingPhoneRegeneratorForTest(patternMatchingRecalculator);

			var query = new ZQuery(PatternMatchingPhoneSchema.PMP_PER, Person.PK);
			query.AddToFilter(PatternMatchingPhoneSchema.PMP_ParentTableCode, patternMatchingPhoneGenerator.TablesPrefixListForTest);

			RegenerateAndAssert(patternMatchingRecalculator, query, 3);

			#endregion

			#region Change Data

			Applicant1["HA_HomePhone"] = "+1 (444) 1111-4321";
			Applicant1["HA_MobilePhone"] = "+1 (333) 1111-6543";
			Applicant1["HA_WorkPhone"] = "+1 (111)1111-8756";

			Person.PER_HomePhone = "+1 (444) 1111-4321";
			Person.PER_MobilePhone = "5645245";
			Factory.Save();

			RegenerateAndAssert(patternMatchingRecalculator, query, 4); //update 3 add 2

			#endregion
		}

		void TestGenerate_Contact()
		{
			#region Set up data & Test Add New Data

			ResetPerson();

			Contact1.OC_Phone = "111111111114";
			Contact1.OC_HomePhone = "+1 (111) 1111-2222";
			Contact1.Header.OH_RL_NKClosestPort = "CNNJ";

			Person.PER_HomePhone = "+1 (111) 1111-2222";
			Factory.Save();

			var patternMatchingRecalculator = new PatternMatchingRecalculator<GlbPerson>(Person);
			var patternMatchingPhoneGenerator = new PersonPatternMatchingPhoneRegeneratorForTest(patternMatchingRecalculator);

			var query = new ZQuery(PatternMatchingPhoneSchema.PMP_PER, Person.PK);
			query.AddToFilter(PatternMatchingPhoneSchema.PMP_ParentTableCode, patternMatchingPhoneGenerator.TablesPrefixListForTest);

			var result = RegenerateAndAssert(patternMatchingRecalculator, query, 3);
			AssertNotNull(result.FirstOrDefault(a => a.PMP_ParentId.Equals(Contact1.PK) && a.PMP_OH == Contact1.Header.PK && a.PMP_RN_NKCountryCode.Equals(Contact1.Header.CountryCode)));

			#endregion

			#region Change Data

			Contact1.OC_Phone = "+1 (111) 1111-2222";
			Contact1.OC_HomePhone = "+1 (111) 1111-2222";
			Contact1.OC_Mobile = "+1(111)1111-3333";
			Contact1.OC_OtherPhone = "+1(111)1111-4444";
			Contact1.OC_Fax = "+1(111)5434-4444";
			Contact1.Header.OH_RL_NKClosestPort = "NZAKL";

			Person.PER_HomePhone = "+1 (111) 1111-2222";
			Person.PER_MobilePhone = "+1(111)1111-3333";
			Person.PER_RN_NKCountry = "CN";
			Factory.Save();

			result = RegenerateAndAssert(patternMatchingRecalculator, query, 7); //update 3 add 4
			AssertNotNull(result.FirstOrDefault(a => a.PMP_ParentId.Equals(Person.PK) && a.PMP_RN_NKCountryCode.Equals(Person.PER_RN_NKCountry)));
			AssertNotNull(result.FirstOrDefault(a => a.PMP_ParentId.Equals(Contact1.PK) && a.PMP_OH == Contact1.Header.PK && a.PMP_RN_NKCountryCode.Equals(Contact1.Header.CountryCode)));

			#endregion
		}

		void TestGenerate_Delete()
		{
			var patternMatchingRecalculator = new PatternMatchingRecalculator<GlbPerson>(Person);
			var patternMatchingPhoneGenerator = new PersonPatternMatchingPhoneRegeneratorForTest(patternMatchingRecalculator);

			var query = new ZQuery(PatternMatchingPhoneSchema.PMP_PER, Person.PK);
			query.AddToFilter(PatternMatchingPhoneSchema.PMP_ParentTableCode, patternMatchingPhoneGenerator.TablesPrefixListForTest);

			CreateRecord(ZGuid.NewZGuid(), GlbStaffSchema.Constants.Prefix);
			Factory.Save();

			var result = Factory.Load<PatternMatchingPhone>(query);
			AssertEquals("Expected: should generate correct numbers of name patterns ", result.Length, 8);

			RegenerateAndAssert(patternMatchingRecalculator, query, 7);
		}

		PatternMatchingPhone[] RegenerateAndAssert(PatternMatchingRecalculator<GlbPerson> patternMatchingRecalculator, ZQuery query, int expectedPatternAmount)
		{
			#region Regenerate

			var patternMatchingPhoneGenerator = new PersonPatternMatchingPhoneRegeneratorForTest(patternMatchingRecalculator);
			var totalCount = patternMatchingPhoneGenerator.InitializeDataCount(Person, Factory);
			var effectCount = patternMatchingPhoneGenerator.Regenerate(Person, Factory);

			#endregion

			#region Assert

			var result = Factory.Load<PatternMatchingPhone>(query);

			AssertEquals("Expected:should regenerate correct amount of patterns ", expectedPatternAmount, result.Length);
			AssertEquals("Expected:plan numbers must equal actual numbers ", totalCount, effectCount);
			AssertContainsExactElementsInAnyOrder("Expected: hash values must be equal ", GetHashedValue(Person), result.Select(u => (int)u.PMP_HashedValue));

			return result;

			#endregion
		}

		protected override void AssertResult(PatternMatchingPhone[] result)
		{
			AssertEquals("Expected: Placeholder phone is not added", 0, result.Length);
		}

		protected override void CreateExtraBusinessObjects()
		{
			Contact1.OC_Phone = "1111";
			Staff1.GS_MobilePhone = "2222";
			Applicant1["HA_HomePhone"] = string.Empty;
		}

		protected override void CreateExtraPatternMatchingRecords()
		{
			CreateRecord(Person.PK, Person.TablePrefix);
			CreateRecord(Staff1.PK, Staff1.TablePrefix);
			CreateRecord(Contact1.PK, Contact1.TablePrefix);
			CreateRecord(Applicant1.PK, Applicant1.TablePrefix);
		}

		void CreateRecord(ZGuid parentId, ZString parentTableCode)
		{
			var patternRecord = Factory.NewWithValidTestData<PatternMatchingPhone>();

			patternRecord.PMP_HashedValue = 2238232;
			patternRecord.PMP_IsActive = true;
			patternRecord.PMP_PER = Person.PK;
			patternRecord.PMP_ParentId = parentId;
			patternRecord.PMP_ParentTableCode = parentTableCode;
			patternRecord.PMP_RN_NKCountryCode = Person.PER_RN_NKCountry;
		}

		protected override string[] GetValueToHash(GlbPerson person)
		{
			return new[] { GetValueToHash(person.PER_HomePhone), GetValueToHash(person.PER_FaxNumber), GetValueToHash(person.PER_MobilePhone), GetValueToHash(person.PER_MobilePhone2) };
		}

		protected override string[] GetValueToHash(OrgContact contact)
		{
			return new[] { GetValueToHash(contact.OC_Phone), GetValueToHash(contact.OC_HomePhone), GetValueToHash(contact.OC_Mobile), GetValueToHash(contact.OC_OtherPhone), GetValueToHash(contact.OC_Fax) };
		}

		protected override string[] GetValueToHash(GlbStaff staff)
		{
			return new[] { GetValueToHash(staff.GS_FaxNum), GetValueToHash(staff.GS_HomePhone), GetValueToHash(staff.GS_MobilePhone), GetValueToHash(staff.GS_WorkPhone) };
		}

		protected override string[] GetValueToHash(IHRJobApplicant applicant)
		{
			return new[] { GetValueToHash(applicant.HA_WorkPhone) };
		}

		string GetValueToHash(ZString phoneNumber)
		{
			ZString result;

			if (phoneNumber.IsEmpty || TextStandardizerHelper.IsPlaceholderPhone(phoneNumber))
			{
				result = ZString.Empty;
			}
			else
			{
				result = TextStandardizerHelper.StandardizePhone(phoneNumber);
			}

			return result;
		}
	}
}
