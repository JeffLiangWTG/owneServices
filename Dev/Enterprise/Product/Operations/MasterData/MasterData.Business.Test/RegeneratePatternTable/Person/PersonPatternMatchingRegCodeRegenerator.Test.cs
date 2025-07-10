using System;
using System.Collections.Generic;
using System.Globalization;
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
	public class PersonPatternMatchingRegCodeRegeneratorForTest : PersonPatternMatchingRegCodeRegenerator
	{
		public PersonPatternMatchingRegCodeRegeneratorForTest(PatternMatchingRecalculator<GlbPerson> recalculator) : base(recalculator)
		{
		}

		public List<string> TablesPrefixListForTest => TablesPrefixList;
	}
	public class PatternMatchingRegCodeGeneratorForPersonTest : PatternGeneratorForPersonTest<PersonPatternMatchingRegCodeRegeneratorForTest, PatternMatchingRegCode>
	{
		protected override SchemaGuidColumn PatternMatchingPersonColumn { get { return PatternMatchingRegCodeSchema.PMR_OH; } }

		protected override SchemaStringColumn PatternMatchingParentTableCodeColumn { get { return PatternMatchingRegCodeSchema.PMR_ParentTableCode; } }

		public void TestGenerateWithSecurity()
		{
			bool oldValue = Env.Security.PersonIntelligenceViewHomeAddress.IsAllowed;
			try
			{
				Env.Security.PersonIntelligenceViewHomeAddress.IsAllowed = false;
				Staff1.GS_Passport = "445566";
				Staff1.GS_EnterpriseCertificationID = "243456354";

				Applicant1["HA_DriversLicenseNumber"] = "789974564";
				Applicant1["HA_Birthdate"] = new ZDate(1959, 1, 2);
				Applicant1["HA_Passport"] = "asdfasdfasfasdf";

				Person.PER_Passport = "445566";

				Factory.Save();
				var patternMatchingRecalculator = new PatternMatchingRecalculator<GlbPerson>(Person);
				var patternMatchingRegCodeGenerator = new PersonPatternMatchingRegCodeRegeneratorForTest(patternMatchingRecalculator);
				patternMatchingRegCodeGenerator.InitializeDataCount(Person, Factory);
				AssertNoExceptionThrown(() =>
				{
					patternMatchingRegCodeGenerator.Regenerate(Person, Factory);
				});

				Staff1.GS_Passport = "445516";
				Staff1.GS_EnterpriseCertificationID = "243456314";

				Applicant1["HA_DriversLicenseNumber"] = "789974514";
				Applicant1["HA_Birthdate"] = new ZDate(1959, 2, 2);
				Applicant1["HA_Passport"] = "asdfasdfaasdf";

				Person.PER_Passport = "445516";

				Factory.Save();
				patternMatchingRecalculator = new PatternMatchingRecalculator<GlbPerson>(Person);
				patternMatchingRegCodeGenerator = new PersonPatternMatchingRegCodeRegeneratorForTest(patternMatchingRecalculator);
				patternMatchingRegCodeGenerator.InitializeDataCount(Person, Factory);
				AssertNoExceptionThrown(() =>
				{
					patternMatchingRegCodeGenerator.Regenerate(Person, Factory);
				});
			}
			finally
			{
				Env.Security.PersonIntelligenceViewHomeAddress.IsAllowed = oldValue;
			}
		}

		public override void TestGenerate()
		{
			TestGenerate_Staff();
			TestGenerate_Applicant();
			TestGenerate_Delete();
		}

		void TestGenerate_Staff()
		{
			#region Set up data & Test Add New Data

			Staff1.GS_Passport = "445566";
			Staff1.GS_EnterpriseCertificationID = "243456354";

			Person.PER_Passport = "445566";
			Factory.Save();

			var patternMatchingRecalculator = new PatternMatchingRecalculator<GlbPerson>(Person);
			var patternMatchingRegCodeGenerator = new PersonPatternMatchingRegCodeRegeneratorForTest(patternMatchingRecalculator);

			var query = new ZQuery(PatternMatchingRegCodeSchema.PMR_PER, Person.PK);
			query.AddToFilter(PatternMatchingRegCodeSchema.PMR_ParentTableCode, patternMatchingRegCodeGenerator.TablesPrefixListForTest);

			RegenerateAndAssert(patternMatchingRecalculator, query, 3);

			#endregion

			#region Change Data

			Staff1.GS_Passport = "1234567890";
			Staff1.GS_Birthdate = new ZDate(1979, 1, 2);
			Staff1.GS_EnterpriseCertificationID = "7536563456";

			Person.PER_Passport = "1234567890";
			Person.PER_BirthDate = new ZDate(1979, 1, 2);
			Factory.Save();

			RegenerateAndAssert(patternMatchingRecalculator, query, 5);

			#endregion
		}

		void TestGenerate_Applicant()
		{
			#region Set up data & Test Add New Data

			ResetPerson();

			Applicant1["HA_DriversLicenseNumber"] = "789974564";
			Applicant1["HA_Birthdate"] = new ZDate(1959, 1, 2);
			Applicant1["HA_Passport"] = "asdfasdfasfasdf";

			Person.PER_Passport = "asdfasdfasfasdf";
			Person.PER_BirthDate = new ZDate(1959, 1, 2);
			Person.PER_DriversLicenseNumber = "789974564";
			Factory.Save();

			var patternMatchingRecalculator = new PatternMatchingRecalculator<GlbPerson>(Person);
			var patternMatchingRegCodeGenerator = new PersonPatternMatchingRegCodeRegeneratorForTest(patternMatchingRecalculator);

			var query = new ZQuery(PatternMatchingRegCodeSchema.PMR_PER, Person.PK);
			query.AddToFilter(PatternMatchingRegCodeSchema.PMR_ParentTableCode, patternMatchingRegCodeGenerator.TablesPrefixListForTest);

			RegenerateAndAssert(patternMatchingRecalculator, query, 3);

			#endregion

			#region Change Data

			Applicant1["HA_DriversLicenseNumber"] = "45654678979";
			Applicant1["HA_Birthdate"] = new ZDate(1999, 1, 2);
			Applicant1["HA_Passport"] = "11223344";
			Applicant1["HA_OtherIdentityDocument"] = "45646346346";

			Person.PER_DriversLicenseNumber = "45654678979";
			Person.PER_BirthDate = new ZDate(1999, 1, 2);
			Person.PER_Passport = "11223344";
			Factory.Save();

			RegenerateAndAssert(patternMatchingRecalculator, query, 4);

			#endregion
		}

		void TestGenerate_Delete()
		{
			var patternMatchingRecalculator = new PatternMatchingRecalculator<GlbPerson>(Person);
			var patternMatchingRegCodeGenerator = new PersonPatternMatchingRegCodeRegeneratorForTest(patternMatchingRecalculator);
			var query = new ZQuery(PatternMatchingRegCodeSchema.PMR_PER, Person.PK);
			query.AddToFilter(PatternMatchingRegCodeSchema.PMR_ParentTableCode, patternMatchingRegCodeGenerator.TablesPrefixListForTest);

			CreateRecord(ZGuid.NewZGuid(), GlbStaffSchema.Constants.Prefix);
			Factory.Save();

			var result = Factory.Load<PatternMatchingRegCode>(query);
			AssertEquals("Expected: should generate correct numbers of name patterns ", result.Length, 5);

			RegenerateAndAssert(patternMatchingRecalculator, query, 4);
		}

		void RegenerateAndAssert(PatternMatchingRecalculator<GlbPerson> patternMatchingRecalculator, ZQuery query, int expectedPatternAmount)
		{
			#region Regenerate

			var patternMatchingRegCodeGenerator = new PersonPatternMatchingRegCodeRegenerator(patternMatchingRecalculator);
			var totalCount = patternMatchingRegCodeGenerator.InitializeDataCount(Person, Factory);
			var effectCount = patternMatchingRegCodeGenerator.Regenerate(Person, Factory);

			#endregion

			#region Assert

			var result = Factory.Load<PatternMatchingRegCode>(query);

			AssertEquals("Expected:should regenerate correct amount of patterns ", expectedPatternAmount, result.Length);
			AssertEquals("Expected:plan numbers must equal actual numbers ", totalCount, effectCount);
			AssertContainsExactElementsInAnyOrder("Expected: hash values must be equal ", GetHashedValue(Person), result.Select(u => (int)u.PMR_HashedValue));

			#endregion
		}

		protected override void AssertResult(PatternMatchingRegCode[] result)
		{
			AssertEquals("Expected: Placeholder regcode is not added", 0, result.Length);
		}

		protected override void CreateExtraBusinessObjects()
		{
			Person.PER_BirthDate = ZDate.Empty;
			Staff1.GS_Passport = "";
			Applicant1["HA_Birthdate"] = ZDateTime.Empty;
			Staff2.GS_Birthdate = ZDate.Empty;
			Applicant2["HA_DriversLicenseNumber"] = "";
		}

		protected override void CreateExtraPatternMatchingRecords()
		{
			CreateRecord(Person.PK, Person.TablePrefix);
			CreateRecord(Staff1.PK, Staff1.TablePrefix);
			CreateRecord(Applicant1.PK, Applicant1.TablePrefix);
			CreateRecord(Staff2.PK, Staff2.TablePrefix);
			CreateRecord(Applicant2.PK, Applicant2.TablePrefix);
		}

		void CreateRecord(ZGuid parentId, ZString parentTableCode)
		{
			var patternRecord = Factory.NewWithValidTestData<PatternMatchingRegCode>();

			patternRecord.PMR_HashedValue = 2238232;
			patternRecord.PMR_IsActive = true;
			patternRecord.PMR_PER = Person.PK;
			patternRecord.PMR_ParentId = parentId;
			patternRecord.PMR_ParentTableCode = parentTableCode;
			patternRecord.PMR_RN_NKCountryCode = Person.PER_RN_NKCountry;
		}

		readonly DateTime gregorianStartDate = new DateTime(1753, 1, 1);

		protected override string[] GetValueToHash(GlbPerson person)
		{
			return new[] { GetPassport(person.PER_Passport), GetBirthDate(person.PER_BirthDate), GetLicense(Person.PER_DriversLicenseNumber) };
		}

		protected override string[] GetValueToHash(OrgContact contact)
		{
			return new[] { string.Empty };
		}

		protected override string[] GetValueToHash(GlbStaff staff)
		{
			return new[] { GetBirthDate(staff.GS_Birthdate), GetPassport(staff.GS_Passport), GetCerificate(staff.GS_EnterpriseCertificationID) };
		}

		protected override string[] GetValueToHash(IHRJobApplicant applicant)
		{
			return new[] { GetOther((ZString)((BusinessObject)applicant)["HA_OtherIdentityDocument"]) };
		}

		string GetBirthDate(ZDateTime birthDate)
		{
			return !birthDate.IsEmpty ? (birthDate.ToDateTime() - gregorianStartDate).Days.ToString(CultureInfo.InvariantCulture) : string.Empty;
		}

		string GetPassport(ZString passport)
		{
			return !passport.IsEmpty ? CodeTypeIdentifier.Passport + TextStandardizerHelper.StandardizeRegCode(passport) : string.Empty;
		}

		string GetCerificate(ZString certificate)
		{
			return !certificate.IsEmpty ? CodeTypeIdentifier.Certificate + TextStandardizerHelper.StandardizeRegCode(certificate) : string.Empty;
		}

		string GetLicense(ZString license)
		{
			return !license.IsEmpty ? CodeTypeIdentifier.License + TextStandardizerHelper.StandardizeRegCode(license) : string.Empty;
		}

		string GetOther(ZString other)
		{
			return !other.IsEmpty ? CodeTypeIdentifier.Other + TextStandardizerHelper.StandardizeRegCode(other) : string.Empty;
		}
	}
}
