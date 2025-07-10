using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Recruiter.Business.Testing;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(GlbAccreditationHighestLevelByProgramFilter))]
	public class GlbAccreditationHighestLevelByProgramFilterTest : ModuleTextFilterTest
	{
		public void TestSerialize_Deserialize_PropertiesFromToXml()
		{
			var group = Factory.NewWithValidTestData<GlbAccreditationGroup>();
			group.HAG_Description = "ABA";
			Factory.Save();
			AssertEquals("Precondition", "ABA", group.HAG_Description);
			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter = new GlbAccreditationHighestLevelByProgramFilter(GlbAccreditationAttemptFilterProvider.FilterDescription.HighestLevelReachedByProgram);
			filterStripBizO.AddModuleFilterForTest(filter);
			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;
			filter.AccreditationGroupDescription = group.HAG_Description;
			filter.IsCompleted = true;
			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, "layoutName", false, false, SaveColumnLayout.Ignore);
			strip.FilterDescription = "";
			strip.Delete();
			var loadedFilter = (GlbAccreditationHighestLevelByProgramFilter)filterStripBizO[filter.Description];
			filterStripBizO.LoadLayout(savedLayout);
			AssertEquals("AccreditationGroupDescription", group.HAG_Description, loadedFilter.AccreditationGroupDescription);
			AssertEquals("AccreditationGroupPK", group.PK, loadedFilter.AccreditationGroupPK);
			AssertEquals("IsCompleted", true, loadedFilter.IsCompleted);
		}

		public void TestAccreditationGroups()
		{
			var groupTestHelper = new GlbAccreditationGroupTestHelper(Factory);
			groupTestHelper.SetupAccreditations();
			groupTestHelper.AccreditationGroup1.HAG_Description = "ABBA";
			groupTestHelper.AccreditationGroup2.HAG_Description = "VBBVB";
			Factory.Save();
			var filterBizO = new GlbAccreditationAttemptFilterBusinessObject();
			var filter = (GlbAccreditationHighestLevelByProgramFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.HighestLevelReachedByProgram];
			filter.IsActive = true;
			AssertEquals("Groups list should include ABBA", true, filter.AccreditationGroups.ContainsCode("ABBA"));
			AssertEquals("Groups list should include VBBVB", true, filter.AccreditationGroups.ContainsCode("VBBVB"));
		}

		public void TestAccreditationGroupPK()
		{
			var groupTestHelper = new GlbAccreditationGroupTestHelper(Factory);
			groupTestHelper.SetupAccreditations();
			groupTestHelper.AccreditationGroup1.HAG_Description = "ABBA";
			groupTestHelper.AccreditationGroup2.HAG_Description = "VBBVB";
			Factory.Save();
			var filterBizO = new GlbAccreditationAttemptFilterBusinessObject();
			var filter = (GlbAccreditationHighestLevelByProgramFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.HighestLevelReachedByProgram];
			filter.IsActive = true;
			filter.AccreditationGroupDescription = groupTestHelper.AccreditationGroup1.HAG_Description;
			AssertEquals("GroupPK should match ABBA", groupTestHelper.AccreditationGroup1.PK, filter.AccreditationGroupPK);
			filter.AccreditationGroupDescription = groupTestHelper.AccreditationGroup2.HAG_Description;
			AssertEquals("GroupPK should match VBBVB", groupTestHelper.AccreditationGroup2.PK, filter.AccreditationGroupPK);
		}

		public void TestIsCompletedShouldBeTrueByDefault()
		{
			var filterBizO = new GlbAccreditationAttemptFilterBusinessObject();
			var filter = (GlbAccreditationHighestLevelByProgramFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.HighestLevelReachedByProgram];
			AssertEquals("Should default to true", true, filter.IsCompleted);
		}

		public void TestIsCompletedReadOnly()
		{
			var group = Factory.NewWithValidTestData<GlbAccreditationGroup>();
			group.HAG_Description = "ABA";
			Factory.Save();
			var filterBizO = new GlbAccreditationAttemptFilterBusinessObject();
			var filter = (GlbAccreditationHighestLevelByProgramFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.HighestLevelReachedByProgram];
			AssertEquals("Should be readonly", true, filter.IsCompletedInfo.ReadOnly);
			filter.AccreditationGroupDescription = "blah";
			AssertEquals("Precondition", true, filter.AccreditationGroupPK.IsEmpty);
			AssertEquals("Should be readonly since no valid group pk", true, filter.IsCompletedInfo.ReadOnly);
			filter.AccreditationGroupDescription = group.HAG_Description;
			AssertEquals("Precondition", false, filter.AccreditationGroupPK.IsEmpty);
			AssertEquals("Should not be readonly since group pk is valid", false, filter.IsCompletedInfo.ReadOnly);
		}

		#region Filter
		public void TestFilterShouldGetLatestAttempt()
		{
			var groupTestHelper = new GlbAccreditationGroupTestHelper(Factory);
			groupTestHelper.SetupAccreditations();
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var attempt1 = Factory.New<GlbAccreditationAttempt>();
			attempt1.HAA_HAC = groupTestHelper.Accreditation1.PK;
			attempt1.HAA_PER = person1.PK;
			attempt1.HAA_CommencementDate = ZDate.Today;
			attempt1.HAA_CompletionDueDate = ZDate.Today.AddDays(5);
			attempt1.HAA_CompletionDate = ZDate.Empty;
			var attempt2 = Factory.New<GlbAccreditationAttempt>();
			attempt2.HAA_HAC = groupTestHelper.Accreditation1.PK;
			attempt2.HAA_PER = person1.PK;
			attempt2.HAA_CommencementDate = ZDate.Today;
			attempt2.HAA_CompletionDueDate = ZDate.Today.AddDays(10);
			attempt2.HAA_CompletionDate = ZDate.Empty;
			Factory.Save();
			var collection = new GlbAccreditationAttemptCollection(Factory);
			var filterBizO = new GlbAccreditationAttemptFilterBusinessObject();
			var filter = (GlbAccreditationHighestLevelByProgramFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.HighestLevelReachedByProgram];
			filter.IsActive = true;
			filter.AccreditationGroupDescription = groupTestHelper.AccreditationGroup1.HAG_Description;
			filter.IsCompleted = false;
			collection.Load(filterBizO.Filter);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
			AssertEquals("Attempt2 has the latest expiry", attempt2.PK, collection[0].PK);
		}

		public void TestFilterShouldGetHighestLevelAttempt()
		{
			var groupTestHelper = new GlbAccreditationGroupTestHelper(Factory);
			groupTestHelper.SetupAccreditations();
			groupTestHelper.Accreditation3.Requirements.Add(groupTestHelper.Accreditation1);
			groupTestHelper.Accreditation1Refresher.Requirements.Add(groupTestHelper.Accreditation1);
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var attempt1 = Factory.New<GlbAccreditationAttempt>();
			attempt1.HAA_HAC = groupTestHelper.Accreditation1.PK;
			attempt1.HAA_PER = person1.PK;
			attempt1.HAA_CommencementDate = ZDate.Today;
			attempt1.HAA_CompletionDueDate = ZDate.Today.AddDays(5);
			attempt1.HAA_CompletionDate = ZDate.Empty;
			var attempt2 = Factory.New<GlbAccreditationAttempt>();
			attempt2.HAA_HAC = groupTestHelper.Accreditation3.PK;
			attempt2.HAA_PER = person1.PK;
			attempt2.HAA_CommencementDate = ZDate.Today;
			attempt2.HAA_CompletionDueDate = ZDate.Today.AddDays(10);
			attempt2.HAA_CompletionDate = ZDate.Empty;
			var attempt3 = Factory.New<GlbAccreditationAttempt>();
			attempt3.HAA_HAC = groupTestHelper.Accreditation1Refresher.PK;
			attempt3.HAA_PER = person1.PK;
			attempt3.HAA_CommencementDate = ZDate.Today;
			attempt3.HAA_CompletionDueDate = ZDate.Today.AddDays(15);
			attempt3.HAA_CompletionDate = ZDate.Empty;
			AssertEquals("Precondition", attempt1.HAA_CompletionDueDate, attempt1.HAA_ExpiryDate);
			AssertEquals("Precondition", attempt2.HAA_CompletionDueDate, attempt2.HAA_ExpiryDate);
			AssertEquals("Precondition", attempt3.HAA_CompletionDueDate, attempt3.HAA_ExpiryDate);
			Factory.Save();
			var collection = new GlbAccreditationAttemptCollection(Factory);
			var filterBizO = new GlbAccreditationAttemptFilterBusinessObject();
			var filter = (GlbAccreditationHighestLevelByProgramFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.HighestLevelReachedByProgram];
			filter.IsActive = true;
			filter.AccreditationGroupDescription = groupTestHelper.AccreditationGroup1.HAG_Description;
			filter.IsCompleted = false;
			collection.Load(filterBizO.Filter);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
			AssertEquals("Attempt 1 should not be included since groupTestHelper.Accreditation3 is higher level", false, collection.Contains(attempt1));
			AssertEquals("Attempt 2 should be included since groupTestHelper.Accreditation3 is the highest level", true, collection.Contains(attempt2));
			AssertEquals("Attempt 3 should not be included since groupTestHelper.Accreditation3 is higher level", false, collection.Contains(attempt3));
		}

		public void TestFilterIsCompleted()
		{
			var groupTestHelper = new GlbAccreditationGroupTestHelper(Factory);
			groupTestHelper.SetupAccreditations();
			groupTestHelper.Accreditation3.Requirements.Add(groupTestHelper.Accreditation1);
			groupTestHelper.Accreditation1Refresher.Requirements.Add(groupTestHelper.Accreditation1);
			groupTestHelper.Accreditation1.HAC_ValidityMonths = 12;
			groupTestHelper.Accreditation1Refresher.HAC_ValidityMonths = groupTestHelper.Accreditation1.HAC_ValidityMonths;
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.ApplicantCollection.AddNew();
			var attempt1 = Factory.New<GlbAccreditationAttempt>();
			attempt1.HAA_HAC = groupTestHelper.Accreditation1.PK;
			attempt1.HAA_PER = person1.PK;
			attempt1.HAA_CommencementDate = ZDate.Today;
			attempt1.HAA_CompletionDueDate = ZDate.Today.AddDays(5);
			attempt1.HAA_CompletionDate = ZDate.Today.AddDays(5);
			var attempt2 = Factory.New<GlbAccreditationAttempt>();
			attempt2.HAA_HAC = groupTestHelper.Accreditation3.PK;
			attempt2.HAA_PER = person1.PK;
			attempt2.HAA_CommencementDate = ZDate.Today;
			attempt2.HAA_CompletionDueDate = ZDate.Today.AddDays(10);
			attempt2.HAA_CompletionDate = ZDate.Empty;
			var attempt3 = Factory.New<GlbAccreditationAttempt>();
			attempt3.HAA_HAC = groupTestHelper.Accreditation1Refresher.PK;
			attempt3.HAA_PER = person1.PK;
			attempt3.HAA_CommencementDate = ZDate.Today;
			attempt3.HAA_CompletionDueDate = attempt1.HAA_ExpiryDate.AddDays(10);
			attempt3.HAA_CompletionDate = ZDate.Empty;
			var attempt4 = Factory.New<GlbAccreditationAttempt>();
			attempt4.HAA_HAC = groupTestHelper.Accreditation1Refresher.PK;
			attempt4.HAA_PER = person1.PK;
			attempt4.HAA_CommencementDate = ZDate.Today;
			attempt4.HAA_CompletionDueDate = ZDate.Today.AddDays(5);
			attempt4.HAA_CompletionDate = ZDate.Today.AddDays(4);
			AssertEquals("Precondition", attempt1.HAA_CompletionDate.AddMonths(12), attempt1.HAA_ExpiryDate);
			AssertEquals("Precondition", attempt2.HAA_CompletionDueDate, attempt2.HAA_ExpiryDate);
			AssertEquals("Precondition", attempt3.HAA_CompletionDueDate, attempt3.HAA_ExpiryDate);
			AssertEquals("Precondition", attempt1.HAA_ExpiryDate.AddMonths(12), attempt4.HAA_ExpiryDate);
			attempt4.HAA_ExpiryDate = attempt1.HAA_ExpiryDate.AddDays(-1);
			Factory.Save();
			var collection = new GlbAccreditationAttemptCollection(Factory);
			var filterBizO = new GlbAccreditationAttemptFilterBusinessObject();
			var filter = (GlbAccreditationHighestLevelByProgramFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.HighestLevelReachedByProgram];
			filter.IsActive = true;
			filter.AccreditationGroupDescription = groupTestHelper.AccreditationGroup1.HAG_Description;
			filter.IsCompleted = true;
			collection.Load(filterBizO.Filter);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
			AssertEquals("Attempt 1 should be included as it's the latest completed attempt", true, collection.Contains(attempt1));
			AssertEquals("Attempt 2 should not be included despite being highest level since it's not completed", false, collection.Contains(attempt2));
			AssertEquals("Attempt 3 should not be included despite being latest expiry date since it's not completed", false, collection.Contains(attempt3));
			AssertEquals("Attempt 4 should not be included since attempt 1 expires later", false, collection.Contains(attempt4));
		}

		public void TestFilterRefresherShouldBeGroupedWithMainAccreditation()
		{
			var groupTestHelper = new GlbAccreditationGroupTestHelper(Factory);
			groupTestHelper.SetupAccreditations();
			groupTestHelper.Accreditation1Refresher.Requirements.Add(groupTestHelper.Accreditation1);
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var attempt1 = Factory.New<GlbAccreditationAttempt>();
			attempt1.HAA_HAC = groupTestHelper.Accreditation1.PK;
			attempt1.HAA_PER = person1.PK;
			attempt1.HAA_CommencementDate = ZDate.Today;
			attempt1.HAA_CompletionDueDate = ZDate.Today.AddDays(5);
			attempt1.HAA_ExpiryDate = ZDate.Today.AddDays(5);
			attempt1.HAA_CompletionDate = ZDate.Empty;
			var attempt2 = Factory.New<GlbAccreditationAttempt>();
			attempt2.HAA_HAC = groupTestHelper.Accreditation1Refresher.PK;
			attempt2.HAA_PER = person1.PK;
			attempt2.HAA_CommencementDate = ZDate.Today;
			attempt2.HAA_CompletionDueDate = ZDate.Today.AddDays(10);
			attempt2.HAA_CompletionDate = ZDate.Empty;
			AssertEquals("Precondition", attempt1.HAA_CompletionDueDate, attempt1.HAA_ExpiryDate);
			AssertEquals("Precondition", attempt2.HAA_CompletionDueDate, attempt2.HAA_ExpiryDate);
			Factory.Save();
			var collection = new GlbAccreditationAttemptCollection(Factory);
			var filterBizO = new GlbAccreditationAttemptFilterBusinessObject();
			var filter = (GlbAccreditationHighestLevelByProgramFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.HighestLevelReachedByProgram];
			filter.IsActive = true;
			filter.IsCompleted = false;
			filter.AccreditationGroupDescription = groupTestHelper.AccreditationGroup1.HAG_Description;
			collection.Load(filterBizO.Filter);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
			AssertEquals("Attempt 1 should not be included since attempt2 was more recent", false, collection.Contains(attempt1));
			AssertEquals("Attempt 2 should be included since it is the most recent", true, collection.Contains(attempt2));
			var attempt3 = Factory.New<GlbAccreditationAttempt>();
			attempt3.HAA_HAC = groupTestHelper.Accreditation1.PK;
			attempt3.HAA_CommencementDate = ZDate.Today;
			attempt3.HAA_CompletionDueDate = ZDate.Today.AddDays(15);
			attempt3.HAA_CompletionDate = ZDate.Empty;
			attempt3.HAA_PER = person1.PK;
			AssertEquals("Precondition", attempt3.HAA_CompletionDueDate, attempt3.HAA_ExpiryDate);
			Factory.Save();
			collection.Load(filterBizO.Filter);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
			AssertEquals("Attempt 1 should not be included since attempt3 was more recent", false, collection.Contains(attempt1));
			AssertEquals("Attempt 2 should not be included since attempt3 was more recent", false, collection.Contains(attempt2));
			AssertEquals("Attempt 3 should be included since it is the most recent", true, collection.Contains(attempt3));
		}

		public void TestFilterShouldIncludeAllHighestLevelAccreditations()
		{
			var groupTestHelper = new GlbAccreditationGroupTestHelper(Factory);
			groupTestHelper.SetupAccreditations();
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("C02", (NoResString)"C02");
			certCodes.Add("C03", (NoResString)"C03");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);
			var accreditation4 = Factory.NewWithValidTestData<GlbAccreditation>();
			var accreditation5 = Factory.NewWithValidTestData<GlbAccreditation>();
			groupTestHelper.AccreditationGroup1.Accreditations.Add(accreditation4);
			groupTestHelper.AccreditationGroup1.Accreditations.Add(accreditation5);
			accreditation4.Requirements.Add(groupTestHelper.Accreditation1);
			accreditation5.Requirements.Add(groupTestHelper.Accreditation3);
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var attempt1 = Factory.New<GlbAccreditationAttempt>();
			attempt1.HAA_HAC = groupTestHelper.Accreditation1.PK;
			attempt1.HAA_PER = person1.PK;
			attempt1.HAA_CommencementDate = ZDate.Today;
			attempt1.HAA_CompletionDueDate = ZDate.Today.AddDays(15);
			attempt1.HAA_CompletionDate = ZDate.Empty;
			var attempt2 = Factory.New<GlbAccreditationAttempt>();
			attempt2.HAA_HAC = groupTestHelper.Accreditation3.PK;
			attempt2.HAA_PER = person1.PK;
			attempt2.HAA_CommencementDate = ZDate.Today;
			attempt2.HAA_CompletionDueDate = ZDate.Today.AddDays(10);
			attempt2.HAA_CompletionDate = ZDate.Empty;
			var attempt3 = Factory.New<GlbAccreditationAttempt>();
			attempt3.HAA_HAC = accreditation4.PK;
			attempt3.HAA_PER = person1.PK;
			attempt3.HAA_CommencementDate = ZDate.Today;
			attempt3.HAA_CompletionDueDate = ZDate.Today.AddDays(15);
			attempt3.HAA_CompletionDate = ZDate.Empty;
			var attempt4 = Factory.New<GlbAccreditationAttempt>();
			attempt4.HAA_HAC = accreditation5.PK;
			attempt4.HAA_PER = person1.PK;
			attempt4.HAA_CommencementDate = ZDate.Today;
			attempt4.HAA_CompletionDueDate = ZDate.Today.AddDays(10);
			attempt4.HAA_CompletionDate = ZDate.Empty;
			Factory.Save();
			var collection = new GlbAccreditationAttemptCollection(Factory);
			var filterBizO = new GlbAccreditationAttemptFilterBusinessObject();
			var filter = (GlbAccreditationHighestLevelByProgramFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.HighestLevelReachedByProgram];
			filter.IsActive = true;
			filter.AccreditationGroupDescription = groupTestHelper.AccreditationGroup1.HAG_Description;
			filter.IsCompleted = false;
			collection.Load(filterBizO.Filter);
			AssertEquals("There should be 2 items in the list", 2, collection.Count);
			AssertEquals("Attempt 1 should not be included since attempt3 is higher level", false, collection.Contains(attempt1));
			AssertEquals("Attempt 2 should not be included since attempt3 is higher level", false, collection.Contains(attempt2));
			AssertEquals("Attempt 3 should be included since it is the highest level", true, collection.Contains(attempt3));
			AssertEquals("Attempt 4 should be included since it is the highest level", true, collection.Contains(attempt4));
		}

		public void TestFilterShouldOnlyIncludeAttemptsFromGroup()
		{
			var groupTestHelper = new GlbAccreditationGroupTestHelper(Factory);
			groupTestHelper.SetupAccreditations();
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			var attempt1 = Factory.New<GlbAccreditationAttempt>();
			attempt1.HAA_HAC = groupTestHelper.Accreditation1.PK;
			attempt1.HAA_PER = person1.PK;
			attempt1.HAA_CommencementDate = ZDate.Today;
			attempt1.HAA_CompletionDueDate = ZDate.Today.AddDays(5);
			attempt1.HAA_CompletionDate = ZDate.Empty;
			var attempt2 = Factory.New<GlbAccreditationAttempt>();
			attempt2.HAA_HAC = groupTestHelper.Accreditation2.PK;
			attempt2.HAA_PER = person1.PK;
			attempt2.HAA_CommencementDate = ZDate.Today;
			attempt2.HAA_CompletionDueDate = ZDate.Today.AddDays(10);
			attempt2.HAA_CompletionDate = ZDate.Empty;
			var attempt3 = Factory.New<GlbAccreditationAttempt>();
			attempt3.HAA_HAC = groupTestHelper.Accreditation3.PK;
			attempt3.HAA_PER = person2.PK;
			attempt3.HAA_CommencementDate = ZDate.Today;
			attempt3.HAA_CompletionDueDate = ZDate.Today.AddDays(10);
			attempt3.HAA_CompletionDate = ZDate.Empty;
			Factory.Save();
			var collection = new GlbAccreditationAttemptCollection(Factory);
			var filterBizO = new GlbAccreditationAttemptFilterBusinessObject();
			var filter = (GlbAccreditationHighestLevelByProgramFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.HighestLevelReachedByProgram];
			filter.IsActive = true;
			filter.AccreditationGroupDescription = groupTestHelper.AccreditationGroup1.HAG_Description;
			filter.IsCompleted = false;
			collection.Load(filterBizO.Filter);
			AssertEquals("There should be 2 items in the list", 2, collection.Count);
			AssertEquals("Attempt 1 should be included since groupTestHelper.Accreditation1 is in the group (level 1)", true, collection.Contains(attempt1));
			AssertEquals("Attempt 2 should not be included since groupTestHelper.Accreditation2 is not in the group", false, collection.Contains(attempt2));
			AssertEquals("Attempt 3 should be included since groupTestHelper.Accreditation3 is in the group (level 1)", true, collection.Contains(attempt3));
		}

		public void TestEmptyFilter()
		{
			var group = Factory.NewWithValidTestData<GlbAccreditationGroup>();
			group.HAG_Description = "ABA";
			Factory.Save();
			var filterBizO = new GlbAccreditationAttemptFilterBusinessObject();
			var filter = (GlbAccreditationHighestLevelByProgramFilter)filterBizO[GlbAccreditationAttemptFilterProvider.FilterDescription.HighestLevelReachedByProgram];
			filter.IsActive = true;
			AssertEquals("Precondition", true, filter.IsCompleted);
			AssertEquals("Precondition", true, filter.AccreditationGroupPK.IsEmpty);
			AssertEquals("Filter should be empty since no group is selected", string.Empty, filterBizO.Filter.LiteralTextADO);
			filter.IsCompleted = false;
			AssertEquals("Precondition", true, filter.AccreditationGroupPK.IsEmpty);
			AssertEquals("Filter should still be blank regardless of IsCompleted's value", string.Empty, filterBizO.Filter.LiteralTextADO);
			filter.AccreditationGroupDescription = "blah";
			AssertEquals("Precondition", true, filter.AccreditationGroupPK.IsEmpty);
			AssertEquals("Filter should be empty since no valid group pk", string.Empty, filterBizO.Filter.LiteralTextADO);
			filter.AccreditationGroupDescription = group.HAG_Description;
			AssertEquals("Precondition", false, filter.AccreditationGroupPK.IsEmpty);
			AssertNotEquals("Filter should not be empty since now we have a valid group pk", string.Empty, filterBizO.Filter.LiteralTextADO);
		}

		#endregion
		#region Implementation
		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			return new GlbAccreditationHighestLevelByProgramFilter("Test");
		}
		#endregion
	}
}
