using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.PatternMatchingResult;
using static Enterprise.MasterFiles.Module.DeduplicationFilterUtils;

namespace Enterprise.MasterData.GUI.Tests
{
	[TestedType(typeof(DeduplicationPersonFilterBusinessObject))]
	public class DeduplicationPersonFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestDeduplicationFiltersCollection()
		{
			FilterStripBizo.LoadModuleFilters();
			var collection = typeof(DeduplicationPersonFilterBusinessObject).GetProperty("DeduplicationFilters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(FilterStripBizo, null) as List<ModuleFilter>;

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

			var personBizOFilters = new GlbPersonFilterBusinessObject().ModuleFilters;
			var dedupFilters = FilterStripBizo.ModuleFilters.Where(f => personBizOFilters.All(filter => filter.Description != f.Description)).ToList();
			dedupFilters.Remove(dedupFilters.First(f => f.Description == "Active Status"));
			AssertEquals("To add filters, please make sure any newly created filters are added to DeduplicationFilters unless they are from based", 9, dedupFilters.Count);
		}

		public void TestExcludedByFilter()
		{
			var excludedByFilter = FilterStripBizo["Excluded By"] as ModuleNkFilter;
			AssertNotNull("Excluded by filter exists", excludedByFilter);
			AssertEquals(GlbStaffSchema.GS_Code.MaxLength, excludedByFilter.MaxLength);

			excludedByFilter.IsActive = true;
			excludedByFilter.Property = GlbStaff.CurrentUser.GS_Code;

			var person = Factory.New<GlbPerson>();
			person.PER_FullName = "JOHN SNOW";
			Factory.Save();

			GlbPersonDuplicationFinderProxy.GetInstance(person).AddExclusion(GlbStaff.CurrentUser.GS_Code);
			AssertNotNull(Factory.LoadTop1<DeduplicationPerson>(excludedByFilter.Query.AddToFilter(MDMAdminPanelPersonViewSchema.DPE_FullName, "JOHN SNOW")));

			excludedByFilter.Property = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, SQLComparisonOperator.NotEqual, GlbStaff.CurrentUser.GS_Code)).GS_Code;
			AssertNull(Factory.LoadTop1<DeduplicationPerson>(excludedByFilter.Query.AddToFilter(MDMAdminPanelPersonViewSchema.DPE_FullName, "JOHN SNOW")));
		}

		public void TestStatusFilter()
		{
			var statusFilter = FilterStripBizo["Status"] as ModuleTextFilter;
			AssertNotNull("Status filter exists", statusFilter);
			statusFilter.IsActive = true;
			statusFilter.Property = "To Be Processed";

			var person = Factory.New<GlbPerson>();
			person.PER_FullName = "JOHN SNOW";

			var targetPerson = Factory.NewWithValidTestData<GlbPerson>();
			targetPerson.PER_FullName = "WILLIAM HOPKINS";

			Factory.Save();
			AssertNotNull(Factory.LoadTop1<DeduplicationPerson>(statusFilter.Query.AddToFilter(MDMAdminPanelPersonViewSchema.DPE_FullName, "JOHN SNOW")));

			statusFilter.Property = "Processed";
			AssertNull(Factory.LoadTop1<DeduplicationPerson>(statusFilter.Query.AddToFilter(MDMAdminPanelPersonViewSchema.DPE_FullName, "JOHN SNOW")));

			var result = Factory.NewWithValidTestData<PatternMatchingResult>();
			result.PMT_MasterPK = person.PK;
			result.PMT_TargetPK = targetPerson.PK;
			result.PMT_Status = "PDU";
			result.PMT_MasterTableCode = result.PMT_TargetTableCode = GlbPersonSchema.Constants.Prefix;
			result.PMT_GS_NKExcludeBy = "";
			Factory.Save();

			AssertNotNull(new BusinessObjectFactory().LoadTop1<DeduplicationPerson>(statusFilter.Query.AddToFilter(MDMAdminPanelPersonViewSchema.DPE_FullName, "JOHN SNOW")));

			statusFilter.Property = "To Be Processed";
			AssertNull(new BusinessObjectFactory().LoadTop1<DeduplicationPerson>(statusFilter.Query.AddToFilter(MDMAdminPanelPersonViewSchema.DPE_FullName, "JOHN SNOW")));
		}

		public void TestLayoutContext()
		{
			AssertEquals("DeduplicationPerson", ((IFilterStripBusinessObjectInternals)FilterStripBizo).LayoutContext);
		}

		public void TestMaximumConfidenceScoreFilter()
		{
			var perMaster1 = Factory.NewWithValidTestData<GlbPerson>();
			perMaster1.PER_FullName = "TESTPER1";
			var perTarget1 = Factory.NewWithValidTestData<GlbPerson>();
			var perTarget2 = Factory.NewWithValidTestData<GlbPerson>();

			var perMaster2 = Factory.NewWithValidTestData<GlbPerson>();
			perMaster2.PER_FullName = "TESTPER2";
			var perTarget3 = Factory.NewWithValidTestData<GlbPerson>();

			Factory.Save();

			CreatePersonPatternMatchingResult(Factory, perMaster1, perTarget1, 70);
			CreatePersonPatternMatchingResult(Factory, perMaster1, perTarget2, 85);
			CreatePersonPatternMatchingResult(Factory, perMaster2, perTarget3, 80);

			Factory.Save();

			var fullNameFilter = FilterStripBizo["Full Name"] as ModuleTextFilter;
			AssertNotNull(fullNameFilter);

			fullNameFilter.IsActive = true;
			fullNameFilter.Property = "TESTPER";

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

			var collection = new DeduplicationPersonCollection(Factory);
			collection.Load(FilterStripBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "TESTPER1", "TESTPER2" }, collection.Cast<DeduplicationPerson>().Select(u => u.DPE_FullName));

			maximumConfidenceScoreFilter.Property1 = 82;
			collection.Load(FilterStripBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "TESTPER1" }, collection.Cast<DeduplicationPerson>().Select(u => u.DPE_FullName));
		}

		public void TestFiltersExistences()
		{
			AssertNotNull(FilterStripBizo["Active Status"]);

			var personFilterBizO = new GlbPersonFilterBusinessObject();
			personFilterBizO.ModuleFilters.ForEach(filter =>
			{
				var category = filter.Category.Description.GetUnresolvedString();
				if (category == "Text Search" ||
					category == "Other")
				{
					AssertNotNull("Filter exists", FilterStripBizo[filter.Description]);
				}
			});
		}

		public void TestOverrideProperties()
		{
			var type = typeof(DeduplicationPersonFilterBusinessObject);
			var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

			CombineAssertions(() =>
			{
				AssertEquals(true, (bool)type.GetProperty("ShouldAddCustomSqlFilter", flags).GetValue(FilterStripBizo));
				AssertEquals(false, (bool)type.GetProperty("ShouldUseHelperFilter", flags).GetValue(FilterStripBizo));
				AssertEquals(true, (bool)type.GetMethod("IsActiveStatusFilterAlwaysApplied", flags).Invoke(FilterStripBizo, null));
			});
		}

		public void TestFilterQueryColumnIsMDMAdminPanelPersonViewSchemaPK()
		{
			var collection = typeof(DeduplicationPersonFilterBusinessObject).GetProperty("DeduplicationFilters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(FilterStripBizo, null) as List<ModuleFilter>;
			var result = false;

			FilterStripBizo.ModuleFilters.ForEach(filter =>
			{
				if (!collection.Any(f => f.Description == filter.Description))
				{
					filter.IsActive = true;
					Assert("It's a query for MDMAdminPanelPersonView", FilterStripBizo.Filter.LiteralTextADO.TrimStart('(').StartsWith(MDMAdminPanelPersonViewSchema.PK.Name));
					result = true;
				}
			});

			Assert(result);
		}

		public void TestDuplicationConfidenceFilters()
		{
			AssertNumberRangeFilterMatches("Total Results", DeDupPerson.DPE_TotalDuplicates = 60);
			AssertNumberRangeFilterMatches("High Confidence Results", DeDupPerson.DPE_HighDuplicates = 5);
			AssertNumberRangeFilterMatches("Low Confidence Results", DeDupPerson.DPE_LowDuplicates = 20);
			AssertNumberRangeFilterMatches("Medium Confidence Results", DeDupPerson.DPE_MediumDuplicates = 35);
		}

		public void TestDuplicationConfidenceFiltersWithOrCategory()
		{
			FilterStripBizo.ResetModuleFilters();
			var perMaster1 = Factory.NewWithValidTestData<GlbPerson>();
			perMaster1.PER_FullName = "TESTPerson1";

			var perMaster2 = Factory.NewWithValidTestData<GlbPerson>();
			perMaster2.PER_FullName = "TESTPerson2";

			var perMaster3 = Factory.NewWithValidTestData<GlbPerson>();
			perMaster3.PER_FullName = "TESTPerson3";

			var perMaster4 = Factory.NewWithValidTestData<GlbPerson>();
			perMaster4.PER_FullName = "AAATESTPerson4";

			var perTarget1 = Factory.NewWithValidTestData<GlbPerson>();
			var perTarget2 = Factory.NewWithValidTestData<GlbPerson>();
			var perTarget3 = Factory.NewWithValidTestData<GlbPerson>();
			var perTarget4 = Factory.NewWithValidTestData<GlbPerson>();

			Factory.Save();

			CreatePersonPatternMatchingResult(Factory, perMaster1, perTarget1, 81);
			CreatePersonPatternMatchingResult(Factory, perMaster2, perTarget2, 82);
			CreatePersonPatternMatchingResult(Factory, perMaster3, perTarget3, 83);
			CreatePersonPatternMatchingResult(Factory, perMaster4, perTarget4, 50);

			Factory.Save();

			var fullNameFilter = FilterStripBizo["Full Name"] as ModuleTextFilter;
			AssertNotNull(fullNameFilter);

			fullNameFilter.IsActive = true;

			var maximumConfidenceScoreFilter = FilterStripBizo["Maximum Confidence Score"] as ModuleNumberRangeFilter;
			AssertNotNull(maximumConfidenceScoreFilter);
			maximumConfidenceScoreFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals(80m, maximumConfidenceScoreFilter.GreaterThanOrEqualToDefaultProperty);
				AssertEquals(ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo.GetUnresolvedString(), maximumConfidenceScoreFilter.DefaultPropertySearch);
				AssertEquals(maximumConfidenceScoreFilter.DefaultPropertySearch, maximumConfidenceScoreFilter.PropertySearch);
			});

			var collection = new DeduplicationPersonCollection(Factory);

			maximumConfidenceScoreFilter.Property1 = 80;
			fullNameFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			fullNameFilter.Property = "AAA";
			collection.Load(FilterStripBizo.Filter);
			Assert(collection.Cast<DeduplicationPerson>().Select(u => u.DPE_FullName).IsCountEqualTo(0));

			fullNameFilter.OrCategory = FilterOrCategory.Blue;
			maximumConfidenceScoreFilter.OrCategory = FilterOrCategory.Blue;
			collection.Load(FilterStripBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "TESTPerson1", "TESTPerson2", "TESTPerson3", "AAATESTPerson4" }, collection.Cast<DeduplicationPerson>().Select(u => u.DPE_FullName));
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
			SetupPersonData();

			AssertIgnoredStatusFilterResult(StatusCodes.PermanentIgnore, DeduplicationHelper.IgnoredStatusConstants.AllIgnores, DeduplicationHelper.IgnoredStatusConstants.NoIgnores);
			AssertIgnoredStatusFilterResult(StatusCodes.TemporaryIgnore, DeduplicationHelper.IgnoredStatusConstants.AllIgnores, DeduplicationHelper.IgnoredStatusConstants.NoIgnores);

			AssertIgnoredStatusFilterResult(StatusCodes.PermanentIgnore, DeduplicationHelper.IgnoredStatusConstants.AllIgnores, DeduplicationHelper.IgnoredStatusConstants.NoIgnores, true);
			AssertIgnoredStatusFilterResult(StatusCodes.TemporaryIgnore, DeduplicationHelper.IgnoredStatusConstants.AllIgnores, DeduplicationHelper.IgnoredStatusConstants.NoIgnores, true);
		}

		public void TestIgnoredStatusFilterForTemporaryIgnores()
		{
			SetupPersonData();

			AssertIgnoredStatusFilterResult(StatusCodes.TemporaryIgnore, DeduplicationHelper.IgnoredStatusConstants.TemporaryIgnores, DeduplicationHelper.IgnoredStatusConstants.PermanentIgnores);
			AssertIgnoredStatusFilterResult(StatusCodes.TemporaryIgnore, DeduplicationHelper.IgnoredStatusConstants.TemporaryIgnores, DeduplicationHelper.IgnoredStatusConstants.PermanentIgnores, true);
		}

		public void TestIgnoredStatusFilterForPermanentIgnores()
		{
			SetupPersonData();

			AssertIgnoredStatusFilterResult(StatusCodes.PermanentIgnore, DeduplicationHelper.IgnoredStatusConstants.PermanentIgnores, DeduplicationHelper.IgnoredStatusConstants.TemporaryIgnores);
			AssertIgnoredStatusFilterResult(StatusCodes.PermanentIgnore, DeduplicationHelper.IgnoredStatusConstants.PermanentIgnores, DeduplicationHelper.IgnoredStatusConstants.TemporaryIgnores, true);
		}

		public void TestIgnoredStatusFilterForNoIgnores()
		{
			SetupPersonData();

			AssertIgnoredStatusFilterResult(StatusCodes.PermanentIgnore, DeduplicationHelper.IgnoredStatusConstants.NoIgnores, DeduplicationHelper.IgnoredStatusConstants.PermanentIgnores);
			AssertIgnoredStatusFilterResult(StatusCodes.TemporaryIgnore, DeduplicationHelper.IgnoredStatusConstants.NoIgnores, DeduplicationHelper.IgnoredStatusConstants.TemporaryIgnores);

			AssertIgnoredStatusFilterResult(StatusCodes.PermanentIgnore, DeduplicationHelper.IgnoredStatusConstants.NoIgnores, DeduplicationHelper.IgnoredStatusConstants.PermanentIgnores, true);
			AssertIgnoredStatusFilterResult(StatusCodes.TemporaryIgnore, DeduplicationHelper.IgnoredStatusConstants.NoIgnores, DeduplicationHelper.IgnoredStatusConstants.TemporaryIgnores, true);
		}

		public void TestIgnoredStatusFilterForOtherInputs()
		{
			SetupPersonData();

			AssertIgnoredStatusFilterResult(StatusCodes.PermanentIgnore, "123456", DeduplicationHelper.IgnoredStatusConstants.PermanentIgnores);
			AssertIgnoredStatusFilterResult(StatusCodes.TemporaryIgnore, "123456", DeduplicationHelper.IgnoredStatusConstants.TemporaryIgnores);

			AssertIgnoredStatusFilterResult(StatusCodes.PermanentIgnore, "123456", DeduplicationHelper.IgnoredStatusConstants.PermanentIgnores, true);
			AssertIgnoredStatusFilterResult(StatusCodes.TemporaryIgnore, "123456", DeduplicationHelper.IgnoredStatusConstants.TemporaryIgnores, true);
		}

		#endregion

		#region Implementation

		DeduplicationPersonFilterBusinessObject FilterStripBizo { get; set; }

		DeduplicationPerson DeDupPerson { get; set; }

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new DeduplicationPersonFilterBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeDupPerson = Factory.New<DeduplicationPerson>();
			((IBusinessObjectInternals)DeDupPerson).Row.AcceptChanges();
			FilterStripBizo = (DeduplicationPersonFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		void AssertNumberRangeFilterMatches(string filterDescription, int value)
		{
			FilterStripBizo.ResetModuleFilters();
			var filter = FilterStripBizo[filterDescription] as ModuleNumberRangeFilter;
			AssertNotNull(filter);

			filter.IsActive = true;
			filter.Property1 = value;
			AssertCollectionContains($"Filter {filterDescription} matches", DeDupPerson, Factory.Load<DeduplicationPerson>(filter.Query));
		}

		void AssertIgnoredByFilterResult(string status)
		{
			var ignoredByFilter = FilterStripBizo["Ignored By"] as ModuleNkFilter;
			AssertNotNull("Ignored By filter exists", ignoredByFilter);
			AssertEquals(GlbStaffSchema.GS_Code.MaxLength, ignoredByFilter.MaxLength);

			ignoredByFilter.IsActive = true;
			ignoredByFilter.Property = GlbStaff.CurrentUser.GS_Code;

			var person = Factory.New<GlbPerson>();
			person.PER_FullName = "TEST PERSON 1";
			var patternMatchingResult = Factory.New<PatternMatchingResult>();
			patternMatchingResult.PMT_MasterPK = person.PK;
			patternMatchingResult.PMT_Status = status;
			patternMatchingResult.PMT_GS_NKExcludeBy = GlbStaff.CurrentUser.GS_Code;
			patternMatchingResult.PMT_FoundTimeUtc = ZDateTime.UtcNow;
			patternMatchingResult.PMT_MasterTableCode = GlbPersonSchema.Constants.Prefix;
			patternMatchingResult.PMT_TargetTableCode = GlbPersonSchema.Constants.Prefix;
			patternMatchingResult.PMT_TargetPK = ZGuid.NewZGuid();
			Factory.Save();

			AssertNotNull(Factory.LoadTop1<DeduplicationPerson>(ignoredByFilter.Query.AddToFilter(MDMAdminPanelPersonViewSchema.DPE_FullName, "TEST PERSON 1")));
			ignoredByFilter.Property = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, SQLComparisonOperator.NotEqual, GlbStaff.CurrentUser.GS_Code)).GS_Code;
			AssertNull(Factory.LoadTop1<DeduplicationPerson>(ignoredByFilter.Query.AddToFilter(MDMAdminPanelPersonViewSchema.DPE_FullName, "TEST PERSON 1")));
		}

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
			patternMatchingResult.PMT_MasterTableCode = GlbPersonSchema.Constants.Prefix;
			patternMatchingResult.PMT_TargetTableCode = GlbPersonSchema.Constants.Prefix;

			if (!shouldInverse)
			{
				patternMatchingResult.PMT_MasterPK = person1PK;
				patternMatchingResult.PMT_TargetPK = person2PK;
			}
			else
			{
				patternMatchingResult.PMT_MasterPK = person2PK;
				patternMatchingResult.PMT_TargetPK = person1PK;
			}

			Factory.Save();

			if (filterValue == DeduplicationHelper.IgnoredStatusConstants.AllIgnores || filterValue == DeduplicationHelper.IgnoredStatusConstants.PermanentIgnores || filterValue == DeduplicationHelper.IgnoredStatusConstants.TemporaryIgnores)
			{
				AssertNotNull(Factory.LoadTop1<DeduplicationPerson>(ignoredStatusFilter.Query.AddToFilter(MDMAdminPanelPersonViewSchema.DPE_FullName, "TEST PERSON 1")));
				ignoredStatusFilter.Property = filterNewValue;
				AssertNull(Factory.LoadTop1<DeduplicationPerson>(ignoredStatusFilter.Query.AddToFilter(MDMAdminPanelPersonViewSchema.DPE_FullName, "TEST PERSON 1")));
			}
			else
			{
				AssertNull(Factory.LoadTop1<DeduplicationPerson>(ignoredStatusFilter.Query.AddToFilter(MDMAdminPanelPersonViewSchema.DPE_FullName, "TEST PERSON 1")));
				ignoredStatusFilter.Property = filterNewValue;
				AssertNotNull(Factory.LoadTop1<DeduplicationPerson>(ignoredStatusFilter.Query.AddToFilter(MDMAdminPanelPersonViewSchema.DPE_FullName, "TEST PERSON 1")));
			}

			patternMatchingResult.Delete();
			Factory.Save();
		}

		void SetupPersonData()
		{
			var person1 = Factory.New<GlbPerson>();
			person1.PER_FullName = "TEST PERSON 1";
			var person2 = Factory.New<GlbPerson>();
			person2.PER_FullName = "TEST PERSON 2";

			Factory.Save();
			person1PK = person1.PK;
			person2PK = person2.PK;
		}

		ZGuid person1PK, person2PK;

		static PatternMatchingResult CreatePersonPatternMatchingResult(BusinessObjectFactory factory, GlbPerson master, GlbPerson target, ZByte scorePercent)
		{
			var patternMatchingResult = factory.New<PatternMatchingResult>();
			patternMatchingResult.PMT_MasterPK = master.PK;
			patternMatchingResult.PMT_ScorePercent = scorePercent;
			patternMatchingResult.PMT_TargetPK = target.PK;
			patternMatchingResult.PMT_FoundTimeUtc = DateTime.Now;
			patternMatchingResult.PMT_Status = StatusCodes.PotentialDuplicate;
			patternMatchingResult.PMT_TargetTableCode = GlbPersonSchema.Constants.Prefix;
			patternMatchingResult.PMT_MasterTableCode = GlbPersonSchema.Constants.Prefix;
			return patternMatchingResult;
		}

		#endregion
	}
}
