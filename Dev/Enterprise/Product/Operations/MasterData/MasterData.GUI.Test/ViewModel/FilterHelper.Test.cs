using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;

namespace Enterprise.MasterData.GUI.ViewModel.Tests
{
	class FilterHelperTest : TestCaseWithFactory
	{
		public void TestFilterConditionType()
		{
			Func<FilterCondition, string, IEnumerable<string>, bool> testFilter = FilterHelper.Filter;

			bool isContains = testFilter.Invoke(new FilterCondition(FilterConditionType.Contains), "abc", new string[] { null });
			AssertEquals(false, isContains);
			isContains = testFilter.Invoke(new FilterCondition(FilterConditionType.Contains), "abc", new string[] { "123" });
			AssertEquals(false, isContains);
			isContains = testFilter.Invoke(new FilterCondition(FilterConditionType.Contains), "abc", new string[] { "abcde" });
			AssertEquals(true, isContains);
			isContains = testFilter.Invoke(new FilterCondition(FilterConditionType.Contains), "abc", new string[] { "abc" });
			AssertEquals(true, isContains);

			bool isExactMatch = testFilter.Invoke(new FilterCondition(FilterConditionType.ExactMatch), "abc", new string[] { null });
			AssertEquals(false, isExactMatch);
			isExactMatch = testFilter.Invoke(new FilterCondition(FilterConditionType.ExactMatch), "abc", new string[] { "123" });
			AssertEquals(false, isExactMatch);
			isExactMatch = testFilter.Invoke(new FilterCondition(FilterConditionType.ExactMatch), "abc", new string[] { "abcde" });
			AssertEquals(false, isExactMatch);
			isExactMatch = testFilter.Invoke(new FilterCondition(FilterConditionType.ExactMatch), "abc", new string[] { "abc" });
			AssertEquals(true, isExactMatch);
		}

		public void TestPersonEmailFilterForNullViewModel()
		{
			AssertPersonEmailFilter(null, string.Empty, FilterConditionType.Contains, false);
			AssertPersonEmailFilter(null, string.Empty, FilterConditionType.ExactMatch, false);
		}

		public void TestPersonEmailFilterForGlbPerson()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_EmailAddress = "Test@123.com";
			person.PER_EmailAddress2 = "Dummy@123.com";

			var deduplicationGlbPerson = person.CreateIGlbPerson() as DeduplicationGlbPerson;
			CombineAssertions("PersonEmailFilterForGlbPerson", () =>
			{
				AssertPersonEmailFilter(deduplicationGlbPerson, "teST@123", FilterConditionType.Contains, true);
				AssertPersonEmailFilter(deduplicationGlbPerson, "dummy.com", FilterConditionType.Contains, false);
				AssertPersonEmailFilter(deduplicationGlbPerson, "DUMMY@123.com", FilterConditionType.ExactMatch, true);
				AssertPersonEmailFilter(deduplicationGlbPerson, "Dummy", FilterConditionType.ExactMatch, false);
			});
		}

		public void TestPersonEmailFilterForGlbStaff()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var staff = person.StaffCollection.AddNew();
			staff.GS_PER = person.PK;
			staff.GS_EmailAddress = "dummyStaff@123.com";

			var deduplicationGlbPerson = person.CreateIGlbPerson() as DeduplicationGlbPerson;
			CombineAssertions("PersonEmailFilterForGlbStaff", () =>
			{
				AssertPersonEmailFilter(deduplicationGlbPerson, "Staff", FilterConditionType.Contains, true);
				AssertPersonEmailFilter(deduplicationGlbPerson, "Staffdummy", FilterConditionType.Contains, false);
				AssertPersonEmailFilter(deduplicationGlbPerson, "dummyStaff@123.com", FilterConditionType.ExactMatch, true);
				AssertPersonEmailFilter(deduplicationGlbPerson, "dummyStaff@123", FilterConditionType.ExactMatch, false);
			});
		}

		public void TestPersonEmailFilterForOrgContact()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var contact = person.ContactCollection.AddNew();
			contact.OC_PER = person.PK;
			contact.OC_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			contact.OC_Email = "dummyContact@123.com";

			var deduplicationGlbPerson = person.CreateIGlbPerson() as DeduplicationGlbPerson;
			CombineAssertions("PersonEmailFilterForOrgContact", () =>
			{
				AssertPersonEmailFilter(deduplicationGlbPerson, "Contact", FilterConditionType.Contains, true);
				AssertPersonEmailFilter(deduplicationGlbPerson, "Contactdummy", FilterConditionType.Contains, false);
				AssertPersonEmailFilter(deduplicationGlbPerson, "dummyContact@123.com", FilterConditionType.ExactMatch, true);
				AssertPersonEmailFilter(deduplicationGlbPerson, "dummyContact@123", FilterConditionType.ExactMatch, false);
			});
		}

		public void TestPersonEmailFilterForHRJobApplicant()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var applicant = person.ApplicantCollection.AddNew() as HRJobApplicant;
			applicant.HA_PER = person.PK;
			applicant.HA_EmailAddress = "dummyApplicant@123.com";

			var deduplicationGlbPerson = person.CreateIGlbPerson() as DeduplicationGlbPerson;
			CombineAssertions("PersonEmailFilterForHRJobApplicant", () =>
			{
				AssertPersonEmailFilter(deduplicationGlbPerson, "Applicant", FilterConditionType.Contains, true);
				AssertPersonEmailFilter(deduplicationGlbPerson, "Applicantdummy", FilterConditionType.Contains, false);
				AssertPersonEmailFilter(deduplicationGlbPerson, "dummyApplicant@123.com", FilterConditionType.ExactMatch, true);
				AssertPersonEmailFilter(deduplicationGlbPerson, "dummyApplicant@123", FilterConditionType.ExactMatch, false);
			});
		}

		public void TestPersonPhoneFilterForNullViewModel()
		{
			AssertPersonPhoneFilter(null, string.Empty, FilterConditionType.Contains, false);
			AssertPersonPhoneFilter(null, string.Empty, FilterConditionType.ExactMatch, false);
		}

		public void TestPersonPhoneFilterForGlbPerson()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_MobilePhone = "1111";
			person.PER_MobilePhone2 = "2222";
			person.PER_HomePhone = "3333";
			person.PER_FaxNumber = "4444";

			var deduplicationGlbPerson = person.CreateIGlbPerson() as DeduplicationGlbPerson;
			CombineAssertions("PersonPhoneFilterForGlbPerson", () =>
			{
				AssertPersonPhoneFilter(deduplicationGlbPerson, "11", FilterConditionType.Contains, true);
				AssertPersonPhoneFilter(deduplicationGlbPerson, "0", FilterConditionType.Contains, false);
				AssertPersonPhoneFilter(deduplicationGlbPerson, "5555", FilterConditionType.ExactMatch, false);
				AssertPersonPhoneFilter(deduplicationGlbPerson, "2222", FilterConditionType.ExactMatch, true);
			});
		}

		public void TestPersonPhoneFilterForGlbStaff()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var staff = person.StaffCollection.AddNew();
			staff.GS_PER = person.PK;
			staff.GS_MobilePhone = "1111";
			staff.GS_WorkPhone = "2222";
			staff.GS_HomePhone = "3333";
			staff.GS_FaxNum = "4444";

			var deduplicationGlbPerson = person.CreateIGlbPerson() as DeduplicationGlbPerson;
			CombineAssertions("PersonPhoneFilterForGlbStaff", () =>
			{
				AssertPersonPhoneFilter(deduplicationGlbPerson, "11", FilterConditionType.Contains, true);
				AssertPersonPhoneFilter(deduplicationGlbPerson, "0", FilterConditionType.Contains, false);
				AssertPersonPhoneFilter(deduplicationGlbPerson, "5555", FilterConditionType.ExactMatch, false);
				AssertPersonPhoneFilter(deduplicationGlbPerson, "2222", FilterConditionType.ExactMatch, true);
			});
		}

		public void TestPersonPhoneFilterForOrgContact()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var contact = person.ContactCollection.AddNew();
			contact.OC_PER = person.PK;
			contact.OC_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			contact.OC_Mobile = "1111";
			contact.OC_HomePhone = "2222";
			contact.OC_OtherPhone = "3333";
			contact.OC_Phone = "4444";
			contact.OC_Fax = "5555";

			var deduplicationGlbPerson = person.CreateIGlbPerson() as DeduplicationGlbPerson;
			CombineAssertions("PersonPhoneFilterForOrgContact", () =>
			{
				AssertPersonPhoneFilter(deduplicationGlbPerson, "11", FilterConditionType.Contains, true);
				AssertPersonPhoneFilter(deduplicationGlbPerson, "0", FilterConditionType.Contains, false);
				AssertPersonPhoneFilter(deduplicationGlbPerson, "5555", FilterConditionType.ExactMatch, true);
				AssertPersonPhoneFilter(deduplicationGlbPerson, "6666", FilterConditionType.ExactMatch, false);
				AssertPersonPhoneFilter(deduplicationGlbPerson, "2222", FilterConditionType.ExactMatch, true);
			});
		}

		public void TestPersonPhoneFilterForHRJobApplicant()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var applicant = person.ApplicantCollection.AddNew() as HRJobApplicant;
			applicant.HA_PER = person.PK;
			applicant.HA_HomePhone = "1111";
			applicant.HA_MobilePhone = "2222";
			applicant.HA_WorkPhone = "3333";
			applicant.HA_FaxNum = "4444";

			var deduplicationGlbPerson = person.CreateIGlbPerson() as DeduplicationGlbPerson;
			CombineAssertions("PersonPhoneFilterForHRJobApplicant", () =>
			{
				AssertPersonPhoneFilter(deduplicationGlbPerson, "11", FilterConditionType.Contains, true);
				AssertPersonPhoneFilter(deduplicationGlbPerson, "0", FilterConditionType.Contains, false);
				AssertPersonPhoneFilter(deduplicationGlbPerson, "5555", FilterConditionType.ExactMatch, false);
				AssertPersonPhoneFilter(deduplicationGlbPerson, "2222", FilterConditionType.ExactMatch, true);
			});
		}

		public void TestPersonNameFilterForNullViewModel()
		{
			AssertPersonNameFilter(null, "", FilterConditionType.Contains, false);
			AssertPersonNameFilter(null, "", FilterConditionType.ExactMatch, false);
		}

		public void TestPersonNameFilterForGlbPerson()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "Dummy";

			var deduplicationGlbPerson = person.CreateIGlbPerson() as DeduplicationGlbPerson;
			CombineAssertions("PersonNameFilterForGlbPerson", () =>
			{
				AssertPersonNameFilter(deduplicationGlbPerson, "teST@123", FilterConditionType.Contains, false);
				AssertPersonNameFilter(deduplicationGlbPerson, "dummy.com", FilterConditionType.Contains, false);
				AssertPersonNameFilter(deduplicationGlbPerson, "DUMMY123", FilterConditionType.ExactMatch, false);
				AssertPersonNameFilter(deduplicationGlbPerson, "Dummy", FilterConditionType.ExactMatch, true);
			});
		}

		public void TestPersonNameFilterForGlbStaff()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var staff = person.StaffCollection.AddNew();
			staff.GS_PER = person.PK;
			staff.GS_FullName = "dummyStaff";

			var deduplicationGlbPerson = person.CreateIGlbPerson() as DeduplicationGlbPerson;
			CombineAssertions("PersonNameFilterForGlbStaff", () =>
			{
				AssertPersonNameFilter(deduplicationGlbPerson, "Staff", FilterConditionType.Contains, true);
				AssertPersonNameFilter(deduplicationGlbPerson, "Staffdummy", FilterConditionType.Contains, false);
				AssertPersonNameFilter(deduplicationGlbPerson, "dummyStaff123", FilterConditionType.ExactMatch, false);
				AssertPersonNameFilter(deduplicationGlbPerson, "dummyStaff", FilterConditionType.ExactMatch, true);
			});
		}

		public void TestPersonNameFilterForOrgContact()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var contact = person.ContactCollection.AddNew();
			contact.OC_PER = person.PK;
			contact.OC_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			contact.OC_ContactName = "dummyContact";

			var deduplicationGlbPerson = person.CreateIGlbPerson() as DeduplicationGlbPerson;
			CombineAssertions("PersonNameFilterForOrgContact", () =>
			{
				AssertPersonNameFilter(deduplicationGlbPerson, "Contact", FilterConditionType.Contains, true);
				AssertPersonNameFilter(deduplicationGlbPerson, "Contactdummy", FilterConditionType.Contains, false);
				AssertPersonNameFilter(deduplicationGlbPerson, "dummyContact123", FilterConditionType.ExactMatch, false);
				AssertPersonNameFilter(deduplicationGlbPerson, "dummyContact", FilterConditionType.ExactMatch, true);
			});
		}

		public void TestPersonNameFilterForHRJobApplicant()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var applicant = person.ApplicantCollection.AddNew() as HRJobApplicant;
			applicant.HA_PER = person.PK;
			applicant.HA_FullName = "dummyApplicant";

			var deduplicationGlbPerson = person.CreateIGlbPerson() as DeduplicationGlbPerson;
			CombineAssertions("PersonNameFilterForHRJobApplicant", () =>
			{
				AssertPersonNameFilter(deduplicationGlbPerson, "Applicant", FilterConditionType.Contains, true);
				AssertPersonNameFilter(deduplicationGlbPerson, "Applicantdummy", FilterConditionType.Contains, false);
				AssertPersonNameFilter(deduplicationGlbPerson, "dummyApplicant123", FilterConditionType.ExactMatch, false);
				AssertPersonNameFilter(deduplicationGlbPerson, "dummyApplicant", FilterConditionType.ExactMatch, true);
			});
		}

		#region Implementation

		void AssertPersonEmailFilter(DeduplicationGlbPerson person, string searchTerm, FilterConditionType filterType, bool expectedResult)
		{
			var message = string.Format("Email filter matches {0}, MatchType: {1}", searchTerm, filterType.ToString());
			AssertEquals(message, expectedResult, FilterHelper.PersonEmailFilter(person, new FilterCondition(filterType), searchTerm));
		}

		void AssertPersonNameFilter(DeduplicationGlbPerson person, string searchTerm, FilterConditionType filterType, bool expectedResult)
		{
			var message = string.Format("Name filter matches {0}, MatchType: {1}", searchTerm, filterType.ToString());
			AssertEquals(message, expectedResult, FilterHelper.PersonNameFilter(person, new FilterCondition(filterType), searchTerm));
		}

		void AssertPersonPhoneFilter(DeduplicationGlbPerson person, string searchTerm, FilterConditionType filterType, bool expectedResult)
		{
			var message = string.Format("Phone filter matches {0}, MatchType: {1}", searchTerm, filterType.ToString());
			AssertEquals(message, expectedResult, FilterHelper.PersonPhoneFilter(person, new FilterCondition(filterType), searchTerm));
		}

		#endregion
	}
}
