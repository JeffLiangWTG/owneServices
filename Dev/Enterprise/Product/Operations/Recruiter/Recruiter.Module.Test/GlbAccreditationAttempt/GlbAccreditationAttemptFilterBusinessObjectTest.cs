using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(GlbAccreditationAttemptFilterBusinessObject))]
	public class GlbAccreditationAttemptFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		[TestDate(2019, 10, 17)]
		public void TestStatusFilter()
		{
			var accred = Factory.NewWithValidTestData<GlbAccreditation>();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "full name";
			var attemptStarted = Factory.New<GlbAccreditationAttempt>();
			attemptStarted.HAA_CommencementDate = new ZDate(2019, 1, 1);
			attemptStarted.HAA_CompletionDueDate = new ZDate(2020, 7, 7);
			attemptStarted.HAA_HAC = accred.PK;
			attemptStarted.HAA_PER = person.PK;
			var attemptFailed = Factory.New<GlbAccreditationAttempt>();
			attemptFailed.HAA_CommencementDate = new ZDate(2019, 1, 1);
			attemptFailed.HAA_ExpiryDate = new ZDate(2019, 7, 7);
			attemptFailed.HAA_CompletionDueDate = new ZDate(2019, 7, 7);
			attemptFailed.HAA_HAC = accred.PK;
			attemptFailed.HAA_PER = person.PK;
			var attemptCompleted = Factory.New<GlbAccreditationAttempt>();
			attemptCompleted.HAA_CommencementDate = new ZDate(2019, 1, 1);
			attemptCompleted.HAA_CompletionDate = new ZDate(2019, 5, 5);
			attemptCompleted.HAA_ExpiryDate = new ZDate(2020, 7, 7);
			attemptCompleted.HAA_CompletionDueDate = new ZDate(2020, 7, 7);
			attemptCompleted.HAA_HAC = accred.PK;
			attemptCompleted.HAA_PER = person.PK;
			var attemptExpired = Factory.New<GlbAccreditationAttempt>();
			attemptExpired.HAA_CommencementDate = new ZDate(2019, 1, 1);
			attemptExpired.HAA_CompletionDate = new ZDate(2019, 5, 5);
			attemptExpired.HAA_ExpiryDate = new ZDate(2019, 7, 7);
			attemptExpired.HAA_CompletionDueDate = new ZDate(2019, 7, 7);
			attemptExpired.HAA_HAC = accred.PK;
			attemptExpired.HAA_PER = person.PK;
			Factory.Save();
			var filterBizO = (GlbAccreditationAttemptFilterBusinessObject)CachedBusinessObject;
			var statusFilter = (ModuleTextFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.Status];
			statusFilter.IsActive = true;
			var nameFilter = (ModuleTextFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.PersonFullName];
			nameFilter.IsActive = true;
			nameFilter.Property = "full name";
			statusFilter.Property = "Commenced";
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertEquals(Collection.Contains(attemptStarted), true);
			statusFilter.Property = "Completed";
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertEquals(Collection.Contains(attemptCompleted), true);
			statusFilter.Property = "Failed To Complete";
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertEquals(Collection.Contains(attemptFailed), true);
			statusFilter.Property = "Expired";
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertEquals(Collection.Contains(attemptExpired), true);
		}

		public void TestAccreditationAttemptFilters()
		{
			AssertEquals(5, Collection.Count);
			var filterBizO = (GlbAccreditationAttemptFilterBusinessObject)CachedBusinessObject;
			var commencementDateFilter = (ModuleDateFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.CommencementDate];
			commencementDateFilter.IsActive = true;
			commencementDateFilter.Property1 = Attempt1.HAA_CommencementDate.AddDays(-1);
			commencementDateFilter.Property2 = Attempt1.HAA_CommencementDate.AddDays(1);
			commencementDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertEquals(Collection.Contains(Attempt1), true);
			var completionDueDateFilter = (ModuleDateFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.CompletionDueDate];
			completionDueDateFilter.IsActive = true;
			commencementDateFilter.IsActive = false;
			completionDueDateFilter.Property1 = Attempt2.HAA_CompletionDueDate.AddDays(-1);
			completionDueDateFilter.Property2 = Attempt2.HAA_CompletionDueDate.AddDays(1);
			completionDueDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertEquals(Collection.Contains(Attempt2), true);
			var completionDateFilter = (ModuleDateFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.CompletionDate];
			completionDateFilter.IsActive = true;
			completionDueDateFilter.IsActive = false;
			commencementDateFilter.IsActive = false;
			completionDateFilter.Property1 = Attempt3.HAA_CompletionDate.AddDays(-1);
			completionDateFilter.Property2 = Attempt3.HAA_CompletionDate.AddDays(1);
			completionDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertEquals(Collection.Contains(Attempt3), true);
			var expiryDateFilter = (ModuleDateFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.ExpiryDate];
			expiryDateFilter.IsActive = true;
			completionDateFilter.IsActive = false;
			completionDueDateFilter.IsActive = false;
			commencementDateFilter.IsActive = false;
			expiryDateFilter.Property1 = Attempt4.HAA_ExpiryDate.AddDays(-1);
			expiryDateFilter.Property2 = Attempt4.HAA_ExpiryDate.AddDays(1);
			expiryDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertEquals(Collection.Contains(Attempt4), true);
			var statusFilter = (ModuleTextFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.Status];
			statusFilter.IsActive = true;
			expiryDateFilter.IsActive = false;
			completionDateFilter.IsActive = false;
			completionDueDateFilter.IsActive = false;
			commencementDateFilter.IsActive = false;
			statusFilter.Property = "Completed";
			Collection.Load(filterBizO.Filter);
			AssertEquals(2, Collection.Count);
			AssertEquals(Collection.Contains(Attempt1), true);
			AssertEquals(Collection.Contains(Attempt3), true);
		}

		public void TestPersonFilters()
		{
			Attempt1.Person.PER_FullName = "Test";
			Attempt2.Person.PER_FullName = "Person Tester";
			Attempt2.Person.PER_EmailAddress = "tester@test.com";
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_PER = Attempt3.HAA_PER;
			applicant.HA_EmailAddress = "applicant@test.com";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_PER = Attempt4.HAA_PER;
			staff.GS_EmailAddress = "staff@test.com";
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_PER = Attempt5.HAA_PER;
			contact.OC_Email = "contact@test.com";
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_CompanyNameOverride = "TEST TEST ADDRESS";
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			contact.OC_OA_OrgAddress = address.PK;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = Attempt3.HAA_PER;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "TEST TEST ORG";
			org.OH_RL_NKClosestPort = "NZAUK";
			contact2.OC_OH = org.PK;
			Factory.Save();
			AssertEquals(5, Collection.Count);
			var filterBizO = (GlbAccreditationAttemptFilterBusinessObject)CachedBusinessObject;
			var nameFilter = (ModuleTextFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.PersonFullName];
			nameFilter.IsActive = true;
			nameFilter.Property = "Test";
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertEquals(Collection.Contains(Attempt1), true);
			var personFilter = (ModuleGuidFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.Person];
			personFilter.IsActive = true;
			nameFilter.IsActive = false;
			personFilter.Property = Attempt2.Person.PK;
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertEquals(Collection.Contains(Attempt2), true);
			var emailFilter = (ModuleTextFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.EmailAddress];
			emailFilter.IsActive = true;
			nameFilter.IsActive = false;
			personFilter.IsActive = false;
			emailFilter.Property = "tester@test.com";
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertEquals(Collection.Contains(Attempt2), true);
			emailFilter.Property = "applicant@test.com";
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertEquals(Collection.Contains(Attempt3), true);
			emailFilter.Property = "staff@test.com";
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertEquals(Collection.Contains(Attempt4), true);
			emailFilter.Property = "contact@test.com";
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertEquals(Collection.Contains(Attempt5), true);
			var orgFilter = (ModuleTextFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.RelatedOrganizationName];
			orgFilter.IsActive = true;
			emailFilter.IsActive = false;
			nameFilter.IsActive = false;
			personFilter.IsActive = false;
			orgFilter.Property = "TEST TEST ADDRESS";
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertEquals(Collection.Contains(Attempt5), true);
			orgFilter.Property = "TEST TEST ORG";
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertEquals(Collection.Contains(Attempt3), true);
			var locationFilter = (ModuleNkFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.WorkingLocation];
			locationFilter.IsActive = true;
			orgFilter.IsActive = false;
			emailFilter.IsActive = false;
			nameFilter.IsActive = false;
			personFilter.IsActive = false;
			locationFilter.Property = "AUSYD";
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertEquals(Collection.Contains(Attempt5), true);
			locationFilter.Property = "NZAUK";
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertEquals(Collection.Contains(Attempt3), true);
		}

		public void TestAccreditationFilters()
		{
			Attempt1.Accreditation.HAC_Code = "TAC";
			Attempt1.Accreditation.HAC_IsWebPublished = true;
			Factory.Save();
			AssertEquals(5, Collection.Count);
			var filterBizO = (GlbAccreditationAttemptFilterBusinessObject)CachedBusinessObject;
			var codeFilter = (ModuleNkFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.AccreditationCode];
			codeFilter.IsActive = true;
			codeFilter.Property = "TAC";
			var webPublishedFilter = (ModuleFlagsFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.WebPublished];
			webPublishedFilter.IsActive = true;
			webPublishedFilter.Property0 = true;
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertEquals(Collection.Contains(Attempt1), true);
		}

		#region Implementation
		protected GlbAccreditationAttemptCollection Collection;
		protected GlbAccreditationAttempt Attempt1, Attempt2, Attempt3, Attempt4, Attempt5;
		protected override void SetUp()
		{
			base.SetUp();
			Collection = new GlbAccreditationAttemptCollection(Factory);
			Attempt1 = NewGlbAccreditationAttempt(ZDate.Today.AddDays(-5), ZDate.Today.AddDays(5), ZDate.Today.AddDays(-2), ZDate.Today.AddDays(10));
			Attempt2 = NewGlbAccreditationAttempt(ZDate.Today.AddDays(-15), ZDate.Today.AddDays(-5), ZDate.Empty, ZDate.Today.AddDays(-1));
			Attempt3 = NewGlbAccreditationAttempt(ZDate.Today.AddDays(-15), ZDate.Today.AddDays(10), ZDate.Today, ZDate.Today.AddDays(20));
			Attempt4 = NewGlbAccreditationAttempt(ZDate.Today.AddDays(-15), ZDate.Today.AddDays(10), ZDate.Empty, ZDate.Today.AddDays(30));
			Attempt5 = NewGlbAccreditationAttempt(ZDate.Today.AddDays(-15), ZDate.Today.AddDays(10), ZDate.Empty, ZDate.Today.AddDays(20));
			Factory.Save();
		}

		GlbAccreditationAttempt NewGlbAccreditationAttempt(ZDate commencementDate, ZDate completionDueDate, ZDate completionDate, ZDate expiryDate)
		{
			var attempt = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt.HAA_CommencementDate = commencementDate;
			attempt.HAA_CompletionDueDate = completionDueDate;
			attempt.HAA_CompletionDate = completionDate;
			attempt.HAA_ExpiryDate = expiryDate;
			Collection.Add(attempt);
			return attempt;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			base.GetFiltersExcludedFromSubgroupCheck();
			var exclusions = base.GetFiltersExcludedFromSubgroupCheck();
			exclusions.Add(TableFilter(GlbPersonSchema.Constants.TableName, "Person Related Email Address"));
			exclusions.Add(TableFilter(GlbStaffSchema.Constants.TableName, "Person Related Email Address"));
			exclusions.Add(TableFilter(OrgContactSchema.Constants.TableName, "Person Related Email Address"));
			exclusions.Add(TableFilter(HRJobApplicantSchema.Constants.TableName, "Person Related Email Address"));
			exclusions.Add(TableFilter(OrgContactSchema.Constants.TableName, "Related Organization Name"));
			exclusions.Add(TableFilter(OrgHeaderSchema.Constants.TableName, "Related Organization Name"));
			exclusions.Add(TableFilter(OrgAddressSchema.Constants.TableName, "Related Organization Name"));
			return exclusions;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var exclusions = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();
			exclusions.Add(TableFilter(GlbPersonSchema.Constants.TableName, "Person Related Email Address"));
			exclusions.Add(TableFilter(OrgContactSchema.Constants.TableName, "Related Organization Name"));
			return exclusions;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlbAccreditationAttemptFilterBusinessObject();
		}
		#endregion
	}
}
