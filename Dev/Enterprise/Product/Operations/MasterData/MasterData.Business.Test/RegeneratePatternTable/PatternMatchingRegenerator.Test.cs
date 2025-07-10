using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business.Tests
{
	public class PatternMatchingRegeneratorForTest<TBizo> : PatternMatchingRegenerator<TBizo>
		where TBizo : BusinessObject, IDeduplicatable, IPatternMatchingRegenerationEntities<TBizo>
	{
		public PatternMatchingRegeneratorForTest(PatternMatchingRecalculator<TBizo> recalculator) : base(recalculator)
		{
			patternMatchingRecalculator = recalculator;
		}

		public List<string> TablesPrefixListForTest => TablesPrefixList;

		protected override List<string> TablesPrefixList => new List<string>();

		public override int InitializeDataCount(TBizo bizo, BusinessObjectFactory factory)
		{
			throw new NotImplementedException();
		}

		protected override int Add(TBizo bizo, BusinessObjectFactory factory)
		{
			throw new NotImplementedException();
		}

		protected override int Delete()
		{
			throw new NotImplementedException();
		}

		protected override int GetActualAddCount()
		{
			throw new NotImplementedException();
		}

		protected override int Update(TBizo bizo, BusinessObjectFactory factory)
		{
			throw new NotImplementedException();
		}
	}

	#region OrgHeader
	public abstract class PatternGeneratorForOrgHeaderTest<TBizo, TPattern> : TestCaseWithFactory
		where TBizo : PatternMatchingRegenerator<OrgHeader>
		where TPattern : BusinessObject, IPatternMatchingBusinessObjects
	{
		OrgHeader orgHeader;

		public OrgHeader OrgHeader
		{
			get
			{
				if (orgHeader == null)
				{
					orgHeader = Factory.NewWithValidTestData<OrgHeader>();
					orgHeader.OH_FullName = "Test Organization";
					orgHeader.OH_RL_NKClosestPort = "AUSYD";
					orgHeader.OH_Code = "GB333";
				}
				return orgHeader;
			}
		}

		public abstract void TestGenerate();

		public void TestGenerate_ExcludesAndRemovesPlaceholders()
		{
			//	Arrange
			CreateExtraBusinessObjects();
			CreateExtraPatternMatchingRecords();

			Factory.Save();

			//	Act
			var patternMatchingRecalculator = new PatternMatchingRecalculator<OrgHeader>(OrgHeader);
			var patternGenerator = (TBizo)Activator.CreateInstance(typeof(TBizo), patternMatchingRecalculator);
			var patternGeneratorForTest = (PatternMatchingRegeneratorForTest<OrgHeader>)Activator.CreateInstance(typeof(PatternMatchingRegeneratorForTest<OrgHeader>), patternMatchingRecalculator);

			patternGenerator.InitializeDataCount(OrgHeader, Factory);
			patternGenerator.Regenerate(OrgHeader, Factory);

			var query = new ZQuery(PatternMatchingOrgHeaderColumn, OrgHeader.PK);
			query.AddToFilter(PatternMatchingParentTableCodeColumn, patternGeneratorForTest.TablesPrefixListForTest);

			var result = Factory.Load<TPattern>(query);

			//	Assert
			AssertResult(result);
		}

		protected abstract void CreateExtraBusinessObjects();

		protected abstract void CreateExtraPatternMatchingRecords();

		protected abstract SchemaGuidColumn PatternMatchingOrgHeaderColumn { get; }

		protected abstract SchemaStringColumn PatternMatchingParentTableCodeColumn { get; }

		protected abstract void AssertResult(TPattern[] result);
	}

	public class PatternGeneratorFunctionForOrgHeaderTest : TestCaseWithFactory
	{
		public void TestTotalCount()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Toll Australia";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_Code = "GB333";

			List<OrgAddress> testAddressDataList = new List<OrgAddress>();
			int testAddressCount = 4;
			orgHeader.MainAddress.Address1 = "72 ORiordan Street";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";
			orgHeader.MainAddress.OA_Code = "DDFF";
			testAddressDataList.Add(orgHeader.MainAddress);
			for (int i = 0; i < testAddressCount; i++)
			{
				var address = orgHeader.Addresses.AddNew();
				address.OA_RN_NKCountryCode = "AU";
				address.OA_Code = "ACD" + i;
				address.OA_Address1 = "Bourke Street" + (i + 1).ToString();
				testAddressDataList.Add(address);
			}
			testAddressCount = testAddressDataList.Count;
			Factory.Save();

			var patternMatchingRecalculator = new PatternMatchingRecalculator<OrgHeader>(orgHeader);
			var patternMatchingAddressGenerator = new OrganisationPatternMatchingAddressRegenerator(patternMatchingRecalculator);

			AssertEquals("Total count must be correct.", patternMatchingAddressGenerator.InitializeDataCount(orgHeader, Factory), testAddressCount);

			//delete data
			var delAddress = Factory.Load<OrgAddress>(testAddressDataList[testAddressCount - 1].PK);
			ZGuid delAddressPK = delAddress.PK;
			delAddress.Delete();
			testAddressCount--;

			//new data
			for (int i = testAddressCount; i < testAddressCount + 2; i++)
			{
				var addAddress = Factory.NewWithValidTestData<OrgAddress>();
				addAddress.OA_OH = orgHeader.PK;
				addAddress.OA_RN_NKCountryCode = "AU";
				addAddress.OA_Address1 = "Bourke Street Wow";
				addAddress.OA_Code = "WOW" + i;
				testAddressDataList.Add(addAddress);
			}

			testAddressCount += 2;
			Factory.Save();
			patternMatchingAddressGenerator = new OrganisationPatternMatchingAddressRegenerator(patternMatchingRecalculator);
			AssertEquals("Total count must be correct.", patternMatchingAddressGenerator.InitializeDataCount(orgHeader, Factory), testAddressCount);
		}
	}

	#endregion

	#region Person

	public abstract class PatternGeneratorForPersonTest<TBizo, TPattern> : TestCaseWithFactory
	where TBizo : PatternMatchingRegenerator<GlbPerson>
	where TPattern : BusinessObject, IPatternMatchingBusinessObjects
	{
		GlbPerson person;

		public GlbPerson Person
		{
			get
			{
				if (person == null)
				{
					person = Factory.NewWithValidTestData<GlbPerson>();
					person.PER_FullName = "Fake Name";
				}
				return person;
			}
		}

		public void ResetPerson()
		{
			person = null;
		}

		#region Contacts

		OrgContact contact1;

		public OrgContact Contact1
		{
			get
			{
				if (contact1 == null)
				{
					contact1 = Factory.NewWithValidTestData<OrgContact>();
					contact1.OC_ContactName = "Fake Name1";
					contact1.OC_PER = Person.PK;
				}

				return contact1;
			}
		}

		OrgContact contact2;

		public OrgContact Contact2
		{
			get
			{
				if (contact2 == null)
				{
					contact2 = Factory.NewWithValidTestData<OrgContact>();
					contact2.OC_ContactName = "Fake Name2";
					contact2.OC_PER = Person.PK;
					contact2.Header.OH_RL_NKClosestPort = "DEBRE";
				}

				return contact2;
			}
		}

		#endregion

		#region Staffs

		GlbStaff staff1;

		public GlbStaff Staff1
		{
			get
			{
				if (staff1 == null)
				{
					staff1 = Factory.NewWithValidTestData<GlbStaff>();
					staff1.GS_FullName = "Fake Name1";
					staff1.GS_PER = Person.PK;
				}

				return staff1;
			}
		}

		GlbStaff staff2;

		public GlbStaff Staff2
		{
			get
			{
				if (staff2 == null)
				{
					staff2 = Factory.NewWithValidTestData<GlbStaff>();
					staff2.GS_FullName = "Fake Name2";
					staff2.GS_PER = Person.PK;
				}

				return staff2;
			}
		}

		#endregion

		#region Applicants

		public IHRJobApplicant HRJobApplicant1 => applicant1 ?? (IHRJobApplicant)Applicant1;

		IHRJobApplicant applicant1;
		public BusinessObject Applicant1
		{
			get
			{
				if (applicant1 == null)
				{
					applicant1 = Factory.New<IHRJobApplicant>();
					applicant1.HA_FullName = "Fake Name1";
					applicant1.HA_PER = Person.PK;
					((BusinessObject)applicant1)["HA_EmailAddress"] = "TEST1@TEST.COM";
				}

				return (BusinessObject)applicant1;
			}
		}

		public IHRJobApplicant HRJobApplicant2 => applicant2 ?? (IHRJobApplicant)Applicant2;

		IHRJobApplicant applicant2;
		public BusinessObject Applicant2
		{
			get
			{
				if (applicant2 == null)
				{
					applicant2 = Factory.New<IHRJobApplicant>();
					applicant2.HA_FullName = "Fake Name2";
					applicant2.HA_PER = Person.PK;
					((BusinessObject)applicant2)["HA_EmailAddress"] = "TEST2@TEST.COM";
				}

				return (BusinessObject)applicant2;
			}
		}

		#endregion

		public abstract void TestGenerate();

		public void TestGenerate_ExcludesAndRemovesPlaceholders()
		{
			//	Arrange
			CreateExtraBusinessObjects();
			CreateExtraPatternMatchingRecords();

			Factory.Save();

			//	Act
			var patternMatchingRecalculator = new PatternMatchingRecalculator<GlbPerson>(Person);
			var patternGenerator = (TBizo)Activator.CreateInstance(typeof(TBizo), patternMatchingRecalculator);
			var patternGeneratorForTest = (PatternMatchingRegeneratorForTest<GlbPerson>)Activator.CreateInstance(typeof(PatternMatchingRegeneratorForTest<GlbPerson>), patternMatchingRecalculator);

			patternGenerator.InitializeDataCount(Person, Factory);
			patternGenerator.Regenerate(Person, Factory);

			var query = new ZQuery(PatternMatchingPersonColumn, Person.PK);
			query.AddToFilter(PatternMatchingParentTableCodeColumn, patternGeneratorForTest.TablesPrefixListForTest);

			var result = Factory.Load<TPattern>(query);

			//	Assert
			AssertResult(result);
		}

		protected abstract void CreateExtraBusinessObjects();

		protected abstract void CreateExtraPatternMatchingRecords();

		protected abstract SchemaGuidColumn PatternMatchingPersonColumn { get; }

		protected abstract SchemaStringColumn PatternMatchingParentTableCodeColumn { get; }

		protected abstract void AssertResult(TPattern[] result);

		protected List<int> GetHashedValue(GlbPerson person)
		{
			var values = new List<int>();
			AddValue(GetValueToHash(person));

			foreach (var contact in person.ContactCollection.Cast<OrgContact>())
			{
				AddValue(GetValueToHash(contact));
			}

			foreach (var staff in person.StaffCollection)
			{
				AddValue(GetValueToHash(staff));
			}

			foreach (var applicant in person.ApplicantCollection.Cast<IHRJobApplicant>())
			{
				AddValue(GetValueToHash(applicant));
			}

			return values;

			void AddValue(string[] valuesStr)
			{
				foreach (var value in valuesStr.Where(u => !string.IsNullOrEmpty(u)))
				{
					values.Add(TextStandardizerHelper.ComputeStringHashFast(value));
				}
			}
		}

		protected abstract string[] GetValueToHash(GlbPerson person);

		protected abstract string[] GetValueToHash(OrgContact contact);

		protected abstract string[] GetValueToHash(GlbStaff staff);

		protected abstract string[] GetValueToHash(IHRJobApplicant applicant);
	}

	public class PatternGeneratorFunctionForPersonTest : TestCaseWithFactory
	{
		public void TestTotalCount()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "Fake Name";
			person.Address1 = "123 Fake St";
			Factory.Save();

			var patternMatchingRecalculator = new PatternMatchingRecalculator<GlbPerson>(person);
			var patternMatchingAddressGenerator = new PersonPatternMatchingAddressRegenerator(patternMatchingRecalculator);

			AssertEquals("Total count must be correct.", patternMatchingAddressGenerator.InitializeDataCount(person, Factory), 1);
			//@Todo: delete data with OrgContacts, GlbStaff, JobApplicant
			//@Todo: new data with OrgContacts, GlbStaff, JobApplicant
		}
	}

	#endregion
}
