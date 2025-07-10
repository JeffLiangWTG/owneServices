using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	public class PersonPatternMatchingAddressRegeneratorForTest : PersonPatternMatchingAddressRegenerator
	{
		public PersonPatternMatchingAddressRegeneratorForTest(PatternMatchingRecalculator<GlbPerson> recalculator) : base(recalculator)
		{
		}

		public List<string> TablesPrefixListForTest => TablesPrefixList;
	}
	public class PersonPatternMatchingAddressRegeneratorTest : PatternGeneratorForPersonTest<PersonPatternMatchingAddressRegeneratorForTest, PatternMatchingAddress>
	{
		protected override SchemaGuidColumn PatternMatchingPersonColumn { get { return PatternMatchingAddressSchema.PMA_PER; } }

		protected override SchemaStringColumn PatternMatchingParentTableCodeColumn { get { return PatternMatchingAddressSchema.PMA_ParentTableCode; } }

		public void TestGenerateWithSecurity()
		{
			bool oldValue = Env.Security.PersonIntelligenceViewHomeAddress.IsAllowed;
			try
			{
				Env.Security.PersonIntelligenceViewHomeAddress.IsAllowed = false;
				Person.Address1 = "123 Fake St";
				Person.City = "Alexandria";
				Person.State = "NSW";
				Person.Postcode = "2015";

				Staff1.Address1 = "345 Fake St";
				Staff1.City = "Alexandria";
				Staff1.State = "NSW";
				Staff1.Postcode = "2015";

				Applicant1["HA_UserAddress1"] = "456 Fake St";
				Applicant1["HA_City"] = "123 Fake St";
				Applicant1["HA_State"] = "NSW";
				Applicant1["HA_Postcode"] = "2015";

				Factory.Save();

				var patternMatchingRecalculator = new PatternMatchingRecalculator<GlbPerson>(Person);
				var patternMatchingAddressGenerator = new PersonPatternMatchingAddressRegeneratorForTest(patternMatchingRecalculator);

				patternMatchingAddressGenerator.InitializeDataCount(Person, Factory);
				AssertNoExceptionThrown(() =>
				{
					patternMatchingAddressGenerator.Regenerate(Person, Factory);
				});

				Person.Address2 = "1 Dooley St";
				Staff1.Address1 = "35 Fake St";
				Applicant1["HA_UserAddress1"] = "45 Fake St";

				Factory.Save();

				patternMatchingAddressGenerator = new PersonPatternMatchingAddressRegeneratorForTest(patternMatchingRecalculator);

				patternMatchingAddressGenerator.InitializeDataCount(Person, Factory);

				AssertNoExceptionThrown(() =>
				{
					patternMatchingAddressGenerator.Regenerate(Person, Factory);
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

			Person.Address1 = "123 Fake St";
			Person.City = "Alexandria";
			Person.State = "NSW";
			Person.Postcode = "2015";

			Staff1.Address1 = "345 Fake St";
			Staff1.City = "Alexandria";
			Staff1.State = "NSW";
			Staff1.Postcode = "2015";

			Staff2.Address1 = "345 Fake St2";
			Staff2.City = "Alexandria2";
			Staff2.State = "NSW2";
			Staff2.Postcode = "2016";

			Applicant1["HA_UserAddress1"] = "456 Fake St";
			Applicant1["HA_City"] = "123 Fake St";
			Applicant1["HA_State"] = "NSW";
			Applicant1["HA_Postcode"] = "2015";

			Applicant2["HA_UserAddress1"] = "456 Fake St2";
			Applicant2["HA_City"] = "123 Fake St2";
			Applicant2["HA_State"] = "NSW2";
			Applicant2["HA_Postcode"] = "2016";

			Factory.Save();

			var patternMatchingRecalculator = new PatternMatchingRecalculator<GlbPerson>(Person);
			var patternMatchingAddressGenerator = new PersonPatternMatchingAddressRegeneratorForTest(patternMatchingRecalculator);

			patternMatchingAddressGenerator.InitializeDataCount(Person, Factory);
			patternMatchingAddressGenerator.Regenerate(Person, Factory);

			var query = new ZQuery(PatternMatchingAddressSchema.PMA_PER, Person.PK);
			query.AddToFilter(PatternMatchingAddressSchema.PMA_ParentTableCode, patternMatchingAddressGenerator.TablesPrefixListForTest);

			var result = Factory.Load<PatternMatchingAddress>(query);
			AssertEquals("Expected: correct amount of address patterns were created", 3, result.Length);
			AssertContainsExactElementsInAnyOrder("Expected: hash values must be equal ", GetHashedValue(Person), result.Select(u => (int)u.PMA_HashedValue));

			#endregion

			#region Change Data

			Person.Address2 = "12 Dooley St";

			Staff1.Address1 = "14 Dooley St";
			Staff2.Address1 = "14 Dooley St2";

			Applicant1["HA_UserAddress1"] = "15 Dooley St";
			Applicant2["HA_UserAddress1"] = "15 Dooley St2";

			Factory.Save();

			#endregion

			#region ReGenerate

			patternMatchingAddressGenerator = new PersonPatternMatchingAddressRegeneratorForTest(patternMatchingRecalculator);

			var totalCount = patternMatchingAddressGenerator.InitializeDataCount(Person, Factory);
			var effectCount = patternMatchingAddressGenerator.Regenerate(Person, Factory);

			#endregion

			#region Assert

			result = Factory.Load<PatternMatchingAddress>(query);

			AssertEquals("Expected: should generate correct numbers of address patterns ", 3, result.Length);
			AssertEquals("Expected: plan numbers must equal actual numbers ", totalCount, effectCount);
			AssertContainsExactElementsInAnyOrder("Expected: hash values must be equal ", GetHashedValue(Person), result.Select(u => (int)u.PMA_HashedValue));

			#endregion

			#region Need Delete Data

			CreateRecord(ZGuid.NewZGuid(), GlbStaffSchema.Constants.Prefix);

			Factory.Save();

			result = Factory.Load<PatternMatchingAddress>(query);
			AssertEquals("Expected: should generate correct numbers of name patterns ", 4, result.Length);

			Factory.Save();

			#endregion

			#region ReGenerate

			patternMatchingAddressGenerator = new PersonPatternMatchingAddressRegeneratorForTest(patternMatchingRecalculator);

			totalCount = patternMatchingAddressGenerator.InitializeDataCount(Person, Factory);
			effectCount = patternMatchingAddressGenerator.Regenerate(Person, Factory);

			#endregion

			#region Assert

			result = Factory.Load<PatternMatchingAddress>(query);

			AssertEquals("Expected: should generate correct numbers of address patterns ", 3, result.Length);
			AssertEquals("Expected: plan numbers must equal actual numbers ", totalCount, effectCount);
			AssertContainsExactElementsInAnyOrder("Expected: hash values must be equal ", GetHashedValue(Person), result.Select(u => (int)u.PMA_HashedValue));

			#endregion
		}

		protected override void AssertResult(PatternMatchingAddress[] result)
		{
			AssertEquals("Expected: Placeholder address is not added", 0, result.Length);
		}

		protected override void CreateExtraBusinessObjects()
		{
			Person.Address1 = "Test";
			Staff1.Address1 = "Test";
			Applicant1["HA_UserAddress1"] = "Test";
			Staff2.Address1 = "Test";
			Applicant2["HA_UserAddress1"] = "Test";
		}

		protected override void CreateExtraPatternMatchingRecords()
		{
			CreateRecord(Person.PK, Person.TablePrefix);
			CreateRecord(Staff1.PK, Staff1.TablePrefix);
			CreateRecord(Applicant1.PK, Applicant1.TablePrefix);
			CreateRecord(Staff2.PK, Staff1.TablePrefix);
			CreateRecord(Applicant2.PK, Applicant1.TablePrefix);
		}

		void CreateRecord(ZGuid parentId, ZString parentTableCode)
		{
			var patternRecord = Factory.NewWithValidTestData<PatternMatchingAddress>();
			patternRecord.PMA_HashedValue = 2238232;
			patternRecord.PMA_IsActive = true;
			patternRecord.PMA_PER = Person.PK;
			patternRecord.PMA_ParentId = parentId;
			patternRecord.PMA_ParentTableCode = parentTableCode;
			patternRecord.PMA_RN_NKCountryCode = Person.PER_RN_NKCountry;
		}

		protected override string[] GetValueToHash(GlbPerson person)
		{
			return new[] { (person.Address1 + person.Address2 + person.City + person.Postcode + person.StateCode).ToUpperInvariant() };
		}

		protected override string[] GetValueToHash(OrgContact contact)
		{
			return new[] { string.Empty };
		}

		protected override string[] GetValueToHash(GlbStaff staff)
		{
			return new[] { staff == null ? string.Empty : (staff.Address1 + staff.Address2 + staff.City + staff.Postcode + staff.StateCode).ToUpperInvariant() };
		}

		protected override string[] GetValueToHash(IHRJobApplicant applicant)
		{
			return new[] { string.Empty };
		}
	}
}
