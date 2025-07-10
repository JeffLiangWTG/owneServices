using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.PatternMatchingResult;
using static Enterprise.MasterFiles.Module.DeduplicationFilterUtils;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(DeduplicationOrganisationFilterBusinessObject))]
	sealed class DeduplicationOrganisationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestSystemDefinedOrgExcludedInQuery()
		{
			var queryText = FilterStripBizo.Filter.LiteralTextSqlFormatted;
			Assert(queryText.Contains("DOH_PK <> '" + OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation + "'"));
			Assert(queryText.Contains("DOH_PK <> '" + OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation + "'"));
		}

		public void TestNoSystemDefinedOrgFilterInCollection()
		{
			Assert(!FilterStripBizo.ModuleFilters.Any(f => f.Description == "SystemDefinedOrg"));
		}

		public void TestDeduplicationFiltersCollection()
		{
			FilterStripBizo.LoadModuleFilters();
			var collection = typeof(DeduplicationOrganisationFilterBusinessObject).GetProperty("DeduplicationFilters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(FilterStripBizo, null) as List<ModuleFilter>;
			AssertContainsExactElementsInAnyOrder(new[]
			{
				FilterDescriptionConstants.TotalResults,
				FilterDescriptionConstants.HighConfidenceResults,
				FilterDescriptionConstants.MediumConfidenceResults,
				FilterDescriptionConstants.LowConfidenceResults,
				FilterDescriptionConstants.Status,
				FilterDescriptionConstants.ExcludedBy,
				FilterDescriptionConstants.IgnoredBy,
				FilterDescriptionConstants.IgnoredStatus,
				FilterDescriptionConstants.MaximumConfidenceScore
			}, collection.Select(f => f.Description));

			var organisationBizoFilters = new OrganisationFilterBusinessObject().ModuleFilters;
			var dedupFilters = FilterStripBizo.ModuleFilters.Where(f => !organisationBizoFilters.Any(filter => filter.Description == f.Description)).ToList();
			dedupFilters.Remove(dedupFilters.First(f => f.Description == "Active Status"));
			AssertEquals("To add filters, please make sure you add them into DeduplicationFilters also unless they are from base", 9, dedupFilters.Count);
		}

		public void TestExcludedByFilter()
		{
			var excludedByFilter = FilterStripBizo["Excluded By"] as ModuleNkFilter;
			AssertNotNull("Excluded by filter exists", excludedByFilter);

			AssertEquals(GlbStaffSchema.GS_Code.MaxLength, excludedByFilter.MaxLength);

			excludedByFilter.IsActive = true;
			excludedByFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			excludedByFilter.Property = GlbStaff.CurrentUser.GS_Code;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";
			Factory.Save();
			((IDeduplicatable)org).IsExcludedFromDeduplication = true;

			AssertNotNull(Factory.LoadTop1<DeduplicationOrganisation>(excludedByFilter.Query.AddToFilter(MDMAdminPanelOrganisationViewSchema.DOH_Code, "TESTORG")));
			excludedByFilter.Property = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, SQLComparisonOperator.NotEqual, GlbStaff.CurrentUser.GS_Code)).GS_Code;
			AssertNull(Factory.LoadTop1<DeduplicationOrganisation>(excludedByFilter.Query.AddToFilter(MDMAdminPanelOrganisationViewSchema.DOH_Code, "TESTORG")));
		}

		public void TestStatusFilter()
		{
			var statusFilter = FilterStripBizo["Status"] as ModuleTextFilter;
			AssertNotNull("Status filter exists", statusFilter);
			statusFilter.IsActive = true;
			statusFilter.Property = "To Be Processed";

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";
			Factory.Save();

			AssertNotNull(Factory.LoadTop1<DeduplicationOrganisation>(statusFilter.Query.AddToFilter(MDMAdminPanelOrganisationViewSchema.DOH_Code, "TESTORG")));
			statusFilter.Property = "Processed";
			AssertNull(Factory.LoadTop1<DeduplicationOrganisation>(statusFilter.Query.AddToFilter(MDMAdminPanelOrganisationViewSchema.DOH_Code, "TESTORG")));

			var result = Factory.NewWithValidTestData<PatternMatchingResult>();
			result.PMT_MasterPK = org.PK;
			result.PMT_TargetPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			result.PMT_Status = "PDU";
			result.PMT_MasterTableCode = result.PMT_TargetTableCode = "OH";
			result.PMT_GS_NKExcludeBy = "";
			Factory.Save();

			AssertNotNull(new BusinessObjectFactory().LoadTop1<DeduplicationOrganisation>(statusFilter.Query.AddToFilter(MDMAdminPanelOrganisationViewSchema.DOH_Code, "TESTORG")));

			statusFilter.Property = "To Be Processed";
			AssertNull(new BusinessObjectFactory().LoadTop1<DeduplicationOrganisation>(statusFilter.Query.AddToFilter(MDMAdminPanelOrganisationViewSchema.DOH_Code, "TESTORG")));
		}

		#region Ignored By Filter

		public void TestIgnoredByFilterForTemporaryIgnore()
		{
			AssertIgnoredByFilterResult(StatusCodes.TemporaryIgnore);
		}

		public void TestIgnoredByFilterForPermanentIgnore()
		{
			AssertIgnoredByFilterResult(StatusCodes.PermanentIgnore);
		}

		#endregion

		#region Ignored Status Filter

		public void TestIgnoredStatusFilterForAllIgnores()
		{
			SetupOrgData();

			AssertIgnoredStatusFilterResult(StatusCodes.PermanentIgnore, DeduplicationHelper.IgnoredStatusConstants.AllIgnores, DeduplicationHelper.IgnoredStatusConstants.NoIgnores);
			AssertIgnoredStatusFilterResult(StatusCodes.TemporaryIgnore, DeduplicationHelper.IgnoredStatusConstants.AllIgnores, DeduplicationHelper.IgnoredStatusConstants.NoIgnores);

			AssertIgnoredStatusFilterResult(StatusCodes.PermanentIgnore, DeduplicationHelper.IgnoredStatusConstants.AllIgnores, DeduplicationHelper.IgnoredStatusConstants.NoIgnores, true);
			AssertIgnoredStatusFilterResult(StatusCodes.TemporaryIgnore, DeduplicationHelper.IgnoredStatusConstants.AllIgnores, DeduplicationHelper.IgnoredStatusConstants.NoIgnores, true);
		}

		public void TestIgnoredStatusFilterForTemporaryIgnores()
		{
			SetupOrgData();

			AssertIgnoredStatusFilterResult(StatusCodes.TemporaryIgnore, DeduplicationHelper.IgnoredStatusConstants.TemporaryIgnores, DeduplicationHelper.IgnoredStatusConstants.PermanentIgnores);
			AssertIgnoredStatusFilterResult(StatusCodes.TemporaryIgnore, DeduplicationHelper.IgnoredStatusConstants.TemporaryIgnores, DeduplicationHelper.IgnoredStatusConstants.PermanentIgnores, true);
		}

		public void TestIgnoredStatusFilterForPermanentIgnores()
		{
			SetupOrgData();

			AssertIgnoredStatusFilterResult(StatusCodes.PermanentIgnore, DeduplicationHelper.IgnoredStatusConstants.PermanentIgnores, DeduplicationHelper.IgnoredStatusConstants.TemporaryIgnores);
			AssertIgnoredStatusFilterResult(StatusCodes.PermanentIgnore, DeduplicationHelper.IgnoredStatusConstants.PermanentIgnores, DeduplicationHelper.IgnoredStatusConstants.TemporaryIgnores, true);
		}

		public void TestIgnoredStatusFilterForNoIgnores()
		{
			SetupOrgData();

			AssertIgnoredStatusFilterResult(StatusCodes.PermanentIgnore, DeduplicationHelper.IgnoredStatusConstants.NoIgnores, DeduplicationHelper.IgnoredStatusConstants.PermanentIgnores);
			AssertIgnoredStatusFilterResult(StatusCodes.TemporaryIgnore, DeduplicationHelper.IgnoredStatusConstants.NoIgnores, DeduplicationHelper.IgnoredStatusConstants.TemporaryIgnores);

			AssertIgnoredStatusFilterResult(StatusCodes.PermanentIgnore, DeduplicationHelper.IgnoredStatusConstants.NoIgnores, DeduplicationHelper.IgnoredStatusConstants.PermanentIgnores, true);
			AssertIgnoredStatusFilterResult(StatusCodes.TemporaryIgnore, DeduplicationHelper.IgnoredStatusConstants.NoIgnores, DeduplicationHelper.IgnoredStatusConstants.TemporaryIgnores, true);
		}

		public void TestIgnoredStatusFilterForOtherInputs()
		{
			SetupOrgData();

			AssertIgnoredStatusFilterResult(StatusCodes.PermanentIgnore, "123456", DeduplicationHelper.IgnoredStatusConstants.PermanentIgnores);
			AssertIgnoredStatusFilterResult(StatusCodes.TemporaryIgnore, "123456", DeduplicationHelper.IgnoredStatusConstants.TemporaryIgnores);

			AssertIgnoredStatusFilterResult(StatusCodes.PermanentIgnore, "123456", DeduplicationHelper.IgnoredStatusConstants.PermanentIgnores, true);
			AssertIgnoredStatusFilterResult(StatusCodes.TemporaryIgnore, "123456", DeduplicationHelper.IgnoredStatusConstants.TemporaryIgnores, true);
		}

		#endregion

		public void TestLayoutContext()
		{
			AssertEquals("DeduplicationOrganisation", ((IFilterStripBusinessObjectInternals)FilterStripBizo).LayoutContext);
		}

		public void TestMaximumConfidenceScoreFilter()
		{
			var orgMaster1 = Factory.NewWithValidTestData<OrgHeader>();
			orgMaster1.OH_Code = "TESTORG1";
			var orgTarget1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgTarget2 = Factory.NewWithValidTestData<OrgHeader>();

			var orgMaster2 = Factory.NewWithValidTestData<OrgHeader>();
			orgMaster2.OH_Code = "TESTORG2";
			var orgTarget3 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			CreateOrgPatternMatchingResult(Factory, orgMaster1, orgTarget1, 70);
			CreateOrgPatternMatchingResult(Factory, orgMaster1, orgTarget2, 85);
			CreateOrgPatternMatchingResult(Factory, orgMaster2, orgTarget3, 80);

			Factory.Save();

			var codeFilter = FilterStripBizo["Code"] as ModuleTextFilter;
			AssertNotNull(codeFilter);

			codeFilter.IsActive = true;
			codeFilter.Property = "TESTORG";

			var maximumConfidenceScoreFilter = FilterStripBizo["Maximum Confidence Score"] as ModuleNumberRangeFilter;
			AssertNotNull(maximumConfidenceScoreFilter);

			CombineAssertions(() =>
			{
				AssertEquals(80m, maximumConfidenceScoreFilter.GreaterThanOrEqualToDefaultProperty);
				AssertEquals(ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo.GetUnresolvedString(), maximumConfidenceScoreFilter.DefaultPropertySearch);
				AssertEquals(maximumConfidenceScoreFilter.DefaultPropertySearch, maximumConfidenceScoreFilter.PropertySearch);
			});

			maximumConfidenceScoreFilter.IsActive = true;
			maximumConfidenceScoreFilter.Property1 = 80;

			var collection = new DeduplicationOrganisationCollection(Factory);
			collection.Load(FilterStripBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "TESTORG1", "TESTORG2" }, collection.Cast<DeduplicationOrganisation>().Select(u => u.DOH_Code));

			maximumConfidenceScoreFilter.Property1 = 82;
			collection.Load(FilterStripBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "TESTORG1" }, collection.Cast<DeduplicationOrganisation>().Select(u => u.DOH_Code));
		}

		public void TestFiltersExistences()
		{
			AssertNotNull(FilterStripBizo["Organization – Account Type"]);
			AssertNotNull(FilterStripBizo["Category"]);
			AssertNotNull(FilterStripBizo["Language"]);
			AssertNotNull(FilterStripBizo["Secondary Type"]);
			AssertNotNull(FilterStripBizo["Created"]);
			AssertNotNull(FilterStripBizo["Main UNLOCO"]);
			AssertNotNull(FilterStripBizo["Active Status"]);
			var orgFilterBizo = new OrganisationFilterBusinessObject();
			orgFilterBizo.ModuleFilters.ForEach(filter =>
			{
				var category = filter.Category.Description.GetUnresolvedString();
				if (filter.Description != "SystemDefinedOrg" &&
				(category == "Text Search" ||
					category == "Organization Type" ||
					category == "Registration Numbers"))
				{
					AssertNotNull("Filter exists", FilterStripBizo[filter.Description]);
				}
			});
		}

		public void TestOverrideProperties()
		{
			var type = typeof(DeduplicationOrganisationFilterBusinessObject);
			var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

			CombineAssertions(() =>
			{
				AssertEquals("ShouldAddWorkflowCustomFieldsFilters", false, (bool)type.GetProperty("ShouldAddWorkflowCustomFieldsFilters", flags).GetValue(FilterStripBizo));
				AssertEquals("ShouldAddCustomSqlFilter", true, (bool)type.GetProperty("ShouldAddCustomSqlFilter", flags).GetValue(FilterStripBizo));
				AssertEquals("ShouldUseHelperFilter", false, (bool)type.GetProperty("ShouldUseHelperFilter", flags).GetValue(FilterStripBizo));
				AssertEquals("IsActiveStatusFilterAlwaysApplied", true, (bool)type.GetMethod("IsActiveStatusFilterAlwaysApplied", flags).Invoke(FilterStripBizo, null));
			});
		}

		public void TestFilterQueryColumnIsMDMAdminPanelOrganisationViewSchemaPK()
		{
			var collection = typeof(DeduplicationOrganisationFilterBusinessObject).GetProperty("DeduplicationFilters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(FilterStripBizo, null) as List<ModuleFilter>;
			FilterStripBizo.ModuleFilters.ForEach(filter =>
			{
				if (!collection.Any(f => f.Description == filter.Description))
				{
					filter.IsActive = true;
					Assert("It's a query for MDMAdminOrganisationView", FilterStripBizo.Filter.LiteralTextADO.TrimStart('(').StartsWith(MDMAdminPanelOrganisationViewSchema.PK.Name));
					filter.IsActive = false;
				}
			});
		}

		DeduplicationOrganisation DeDupOrg;

		public void TestDuplicationConfidenceFilters()
		{
			AssertNumberRangeFilterMatches("Total Results", (DeDupOrg.DOH_TotalDuplicates = 60));
			AssertNumberRangeFilterMatches("High Confidence Results", (DeDupOrg.DOH_HighDuplicates = 5));
			AssertNumberRangeFilterMatches("Low Confidence Results", (DeDupOrg.DOH_LowDuplicates = 20));
			AssertNumberRangeFilterMatches("Medium Confidence Results", (DeDupOrg.DOH_MediumDuplicates = 35));
		}

		void AssertNumberRangeFilterMatches(string filterDescription, int value)
		{
			FilterStripBizo.ResetModuleFilters();
			var filter = FilterStripBizo[filterDescription] as ModuleNumberRangeFilter;
			filter.IsActive = true;
			filter.Property1 = value;
			AssertCollectionContains($"Filter {filterDescription} matches", DeDupOrg, Factory.Load<DeduplicationOrganisation>(filter.Query));
		}

		public void TestTotalFilterProperties()
		{
			FilterStripBizo.ResetModuleFilters();
			var totalFilter = FilterStripBizo["Total Results"] as ModuleNumberRangeFilter;
			AssertEquals(new ZString(ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo.GetUnresolvedString()), totalFilter.DefaultPropertySearch);
			AssertEquals(new ZString(ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo.GetUnresolvedString()), totalFilter.PropertySearch);
			AssertEquals(1m, totalFilter.GreaterThanOrEqualToDefaultProperty);
		}

		public void TestDuplicationConfidenceFiltersWithOrCategory()
		{
			FilterStripBizo.ResetModuleFilters();
			var orgMaster1 = Factory.NewWithValidTestData<OrgHeader>();
			orgMaster1.OH_Code = "TESTORG1";

			var orgMaster2 = Factory.NewWithValidTestData<OrgHeader>();
			orgMaster2.OH_Code = "TESTORG2";

			var orgMaster3 = Factory.NewWithValidTestData<OrgHeader>();
			orgMaster3.OH_Code = "TESTORG3";

			var orgMaster4 = Factory.NewWithValidTestData<OrgHeader>();
			orgMaster4.OH_Code = "AAATESTORG4";

			var orgTarget1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgTarget2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgTarget3 = Factory.NewWithValidTestData<OrgHeader>();
			var orgTarget4 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			CreateOrgPatternMatchingResult(Factory, orgMaster1, orgTarget1, 81);
			CreateOrgPatternMatchingResult(Factory, orgMaster2, orgTarget2, 82);
			CreateOrgPatternMatchingResult(Factory, orgMaster3, orgTarget3, 83);
			CreateOrgPatternMatchingResult(Factory, orgMaster4, orgTarget4, 50);

			Factory.Save();

			var codeFilter = FilterStripBizo["Code"] as ModuleTextFilter;
			AssertNotNull(codeFilter);

			codeFilter.IsActive = true;

			var maximumConfidenceScoreFilter = FilterStripBizo["Maximum Confidence Score"] as ModuleNumberRangeFilter;
			AssertNotNull(maximumConfidenceScoreFilter);
			maximumConfidenceScoreFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals(80m, maximumConfidenceScoreFilter.GreaterThanOrEqualToDefaultProperty);
				AssertEquals(ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo.GetUnresolvedString(), maximumConfidenceScoreFilter.DefaultPropertySearch);
				AssertEquals(maximumConfidenceScoreFilter.DefaultPropertySearch, maximumConfidenceScoreFilter.PropertySearch);
			});

			var collection = new DeduplicationOrganisationCollection(Factory);

			maximumConfidenceScoreFilter.Property1 = 80;
			codeFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			codeFilter.Property = "AAA";
			collection.Load(FilterStripBizo.Filter);
			Assert(collection.Cast<DeduplicationOrganisation>().Select(u => u.DOH_Code).IsCountEqualTo(0));

			codeFilter.OrCategory = FilterOrCategory.Blue;
			maximumConfidenceScoreFilter.OrCategory = FilterOrCategory.Blue;
			collection.Load(FilterStripBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "TESTORG1", "TESTORG2", "TESTORG3", "AAATESTORG4" }, collection.Cast<DeduplicationOrganisation>().Select(u => u.DOH_Code));
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeDupOrg = Factory.New<DeduplicationOrganisation>();
			((IBusinessObjectInternals)DeDupOrg).Row.AcceptChanges();
			FilterStripBizo = (DeduplicationOrganisationFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		public DeduplicationOrganisationFilterBusinessObject FilterStripBizo { get; set; }

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new DeduplicationOrganisationFilterBusinessObject();
		}

		void AssertIgnoredByFilterResult(string status)
		{
			var ignoredByFilter = FilterStripBizo["Ignored By"] as ModuleNkFilter;
			AssertNotNull("Ignored By filter exists", ignoredByFilter);
			AssertEquals(GlbStaffSchema.GS_Code.MaxLength, ignoredByFilter.MaxLength);

			ignoredByFilter.IsActive = true;
			ignoredByFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			ignoredByFilter.Property = GlbStaff.CurrentUser.GS_Code;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";
			var patternMatchingResult = Factory.New<PatternMatchingResult>();
			patternMatchingResult.PMT_MasterPK = org.PK;
			patternMatchingResult.PMT_Status = status;
			patternMatchingResult.PMT_GS_NKExcludeBy = GlbStaff.CurrentUser.GS_Code;
			patternMatchingResult.PMT_FoundTimeUtc = ZDateTime.UtcNow;
			patternMatchingResult.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			patternMatchingResult.PMT_TargetTableCode = OrgHeaderSchema.Constants.Prefix;
			patternMatchingResult.PMT_TargetPK = ZGuid.NewZGuid();
			Factory.Save();

			AssertNotNull(Factory.LoadTop1<DeduplicationOrganisation>(ignoredByFilter.Query.AddToFilter(MDMAdminPanelOrganisationViewSchema.DOH_Code, "TESTORG")));
			ignoredByFilter.Property = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, SQLComparisonOperator.NotEqual, GlbStaff.CurrentUser.GS_Code)).GS_Code;
			AssertNull(Factory.LoadTop1<DeduplicationOrganisation>(ignoredByFilter.Query.AddToFilter(MDMAdminPanelOrganisationViewSchema.DOH_Code, "TESTORG")));
		}

		void SetupOrgData()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "TESTORG1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "TESTORG2";

			Factory.Save();
			org1PK = org1.PK;
			org2PK = org2.PK;
		}

		ZGuid org1PK, org2PK;

		void AssertIgnoredStatusFilterResult(string status, string filterValue, string filterNewValue, bool shouldInverse = false)
		{
			var ignoredStatusFilter = FilterStripBizo["Ignored Status"] as ModuleTextFilter;
			AssertNotNull("Ignored Status filter exists", ignoredStatusFilter);
			ignoredStatusFilter.IsActive = true;
			ignoredStatusFilter.Property = filterValue;

			var patternMatchingResult = Factory.New<PatternMatchingResult>();
			patternMatchingResult.PMT_Status = status;
			patternMatchingResult.PMT_GS_NKExcludeBy = GlbStaff.CurrentUser.GS_Code;
			patternMatchingResult.PMT_FoundTimeUtc = ZDateTime.UtcNow;
			patternMatchingResult.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			patternMatchingResult.PMT_TargetTableCode = OrgHeaderSchema.Constants.Prefix;

			if (!shouldInverse)
			{
				patternMatchingResult.PMT_MasterPK = org1PK;
				patternMatchingResult.PMT_TargetPK = org2PK;
			}
			else
			{
				patternMatchingResult.PMT_MasterPK = org2PK;
				patternMatchingResult.PMT_TargetPK = org1PK;
			}

			Factory.Save();

			if (filterValue == DeduplicationHelper.IgnoredStatusConstants.AllIgnores || filterValue == DeduplicationHelper.IgnoredStatusConstants.PermanentIgnores || filterValue == DeduplicationHelper.IgnoredStatusConstants.TemporaryIgnores)
			{
				AssertNotNull(Factory.LoadTop1<DeduplicationOrganisation>(ignoredStatusFilter.Query.AddToFilter(MDMAdminPanelOrganisationViewSchema.DOH_Code, "TESTORG1")));
				ignoredStatusFilter.Property = filterNewValue;
				AssertNull(Factory.LoadTop1<DeduplicationOrganisation>(ignoredStatusFilter.Query.AddToFilter(MDMAdminPanelOrganisationViewSchema.DOH_Code, "TESTORG1")));
			}
			else
			{
				AssertNull(Factory.LoadTop1<DeduplicationOrganisation>(ignoredStatusFilter.Query.AddToFilter(MDMAdminPanelOrganisationViewSchema.DOH_Code, "TESTORG1")));
				ignoredStatusFilter.Property = filterNewValue;
				AssertNotNull(Factory.LoadTop1<DeduplicationOrganisation>(ignoredStatusFilter.Query.AddToFilter(MDMAdminPanelOrganisationViewSchema.DOH_Code, "TESTORG1")));
			}

			patternMatchingResult.Delete();
			Factory.Save();
		}

		static void CreateOrgPatternMatchingResult(BusinessObjectFactory factory, OrgHeader master, OrgHeader target, ZByte scorePercent)
		{
			var patternMatchingResult = factory.New<PatternMatchingResult>();
			patternMatchingResult.PMT_MasterPK = master.PK;
			patternMatchingResult.PMT_ScorePercent = scorePercent;
			patternMatchingResult.PMT_TargetPK = target.PK;
			patternMatchingResult.PMT_FoundTimeUtc = DateTime.Now;
			patternMatchingResult.PMT_Status = PatternMatchingResult.StatusCodes.PotentialDuplicate;
			patternMatchingResult.PMT_TargetTableCode = OrgHeaderSchema.Constants.Prefix;
			patternMatchingResult.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
		}

		#endregion
	}
}
