using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public static class DeduplicationFilterUtils
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string")]
		public static class FilterDescriptionConstants
		{
			public const string TotalResults = "Total Results";
			public const string HighConfidenceResults = "High Confidence Results";
			public const string MediumConfidenceResults = "Medium Confidence Results";
			public const string LowConfidenceResults = "Low Confidence Results";
			public const string Status = "Status";
			public const string ExcludedBy = "Excluded By";
			public const string IgnoredBy = "Ignored By";
			public const string IgnoredStatus = "Ignored Status";
			public const string MaximumConfidenceScore = "Maximum Confidence Score";
		}

		public static void AddExcludedByFilter(ModuleFilterCollection collection, GlbStaffCollection glbStaffList, SchemaStringColumn statusColumn, SchemaStringColumn excludedByColumn, List<ModuleFilter> deduplicationFilters)
		{
			var excludedByFilter = collection.AddNkFilter(FilterDescriptionConstants.ExcludedBy, GetExcludedByFilterQuery, ModuleIDs.GlbStaff, glbStaffList);
			excludedByFilter.MultilingualDescription = ResString.GetMultilingualString("42981ee2-7e1f-4dfb-ad4d-d87a968e7811", "Excluded By");
			excludedByFilter.MaxLength = GlbStaffSchema.GS_Code.MaxLength;
			deduplicationFilters.Add(excludedByFilter);

			ZQuery GetExcludedByFilterQuery(SQLComparisonOperator @operator, ZString userCode)
			{
				return new ZQuery(statusColumn, DeduplicationHelper.StatusConstants.Excluded)
					.AddToFilter(excludedByColumn, @operator, userCode);
			}
		}

		public static void AddConfidenceFilters(ModuleFilterCollection collection, SchemaIntColumn totalDuplicates, SchemaIntColumn highDuplicates, SchemaIntColumn mediumDuplicates, SchemaIntColumn lowDuplicates, List<ModuleFilter> deduplicationFilters)
		{
			var totalFilter = collection.AddNumberRangeFilter(FilterDescriptionConstants.TotalResults, totalDuplicates);
			totalFilter.MultilingualDescription = ResString.GetMultilingualString("e33d6306-1ee1-46ce-8433-cb6a35a6454a", "Total Results");
			totalFilter.DefaultPropertySearch = new ZString(ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo.GetUnresolvedString());
			totalFilter.GreaterThanOrEqualToDefaultProperty = 1;
			totalFilter.PropertySearch = totalFilter.DefaultPropertySearch;

			var highConfidenceFilter = collection.AddNumberRangeFilter(FilterDescriptionConstants.HighConfidenceResults, highDuplicates);
			highConfidenceFilter.MultilingualDescription = ResString.GetMultilingualString("9400696f-a74d-4353-bd4b-fd15f08447f9", "High Confidence Results");
			var mediumConfidenceFilter = collection.AddNumberRangeFilter(FilterDescriptionConstants.MediumConfidenceResults, mediumDuplicates);
			mediumConfidenceFilter.MultilingualDescription = ResString.GetMultilingualString("448fc5eb-a7b0-4f97-8fe3-e2d5b8324a85", "Medium Confidence Results");
			var lowConfidenceFilter = collection.AddNumberRangeFilter(FilterDescriptionConstants.LowConfidenceResults, lowDuplicates);
			lowConfidenceFilter.MultilingualDescription = ResString.GetMultilingualString("a7d468e6-cbbe-4de5-9652-64f8dd4945a1", "Low Confidence Results");

			deduplicationFilters.Add(totalFilter);
			deduplicationFilters.Add(highConfidenceFilter);
			deduplicationFilters.Add(mediumConfidenceFilter);
			deduplicationFilters.Add(lowConfidenceFilter);
		}

		public static void AddDeduplicationStatusFilter(ModuleFilterCollection collection, SchemaStringColumn statusColumn, List<ModuleFilter> deduplicationFilters)
		{
			var statusFilter = new ModuleTextFilter(FilterDescriptionConstants.Status, (status) => new ZQuery(statusColumn, status), StatusList);
			statusFilter.Category = FilterCategories.StatusAndFlags;
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("459f0baf-cf03-4252-9544-2b088f2d52ab", "Status");
			collection.AddCustomFilter(statusFilter);
			deduplicationFilters.Add(statusFilter);
		}

		public static void AddIgnoredByFilter(ModuleFilterCollection collection, GlbStaffCollection glbStaffList, Type duplicationType, string tablePrefix, List<ModuleFilter> deduplicationFilters)
		{
			var ignoredByFilter = collection.AddNkFilter(FilterDescriptionConstants.IgnoredBy, GetIgnoredByFilterQuery, ModuleIDs.GlbStaff, glbStaffList);
			ignoredByFilter.MultilingualDescription = ResString.GetMultilingualString("e32c0912-3d85-4b3d-b359-476ec8ff3891", "Ignored By");
			ignoredByFilter.MaxLength = GlbStaffSchema.GS_Code.MaxLength;
			deduplicationFilters.Add(ignoredByFilter);

			ZQuery GetIgnoredByFilterQuery(SQLComparisonOperator @operator, ZString userCode)
			{
				var query1 = new ZDBOnlyQuery(typeof(PatternMatchingResult)).AddToFilter(PatternMatchingResultSchema.PMT_GS_NKExcludeBy, @operator, userCode);
				var query2 = new ZDBOnlyQuery(typeof(PatternMatchingResult)).AddToFilter(PatternMatchingResultSchema.PMT_Status, PatternMatchingResult.StatusCodes.PermanentIgnore);
				query2.AddToFilter(JoinCondition.Or, PatternMatchingResultSchema.PMT_Status, PatternMatchingResult.StatusCodes.TemporaryIgnore);
				query1.AddToFilter(query2);

				var subQueryAsMaster = new ZDBOnlySubQuery(typeof(PatternMatchingResult), PatternMatchingResultSchema.PMT_MasterPK);
				subQueryAsMaster.AddToFilter(PatternMatchingResultSchema.PMT_MasterTableCode, tablePrefix);
				subQueryAsMaster.AddToFilter(PatternMatchingResultSchema.PMT_TargetTableCode, tablePrefix);
				subQueryAsMaster.AddToFilter(query1);

				var subQueryAsTarget = new ZDBOnlySubQuery(typeof(PatternMatchingResult), PatternMatchingResultSchema.PMT_TargetPK);
				subQueryAsTarget.AddToFilter(PatternMatchingResultSchema.PMT_MasterTableCode, tablePrefix);
				subQueryAsTarget.AddToFilter(PatternMatchingResultSchema.PMT_TargetTableCode, tablePrefix);
				subQueryAsTarget.AddToFilter(query1);

				var queryResult = new ZDBOnlyQuery(duplicationType);
				queryResult.AddSubQuery(subQueryAsMaster, JoinCondition.Or);
				queryResult.AddSubQuery(subQueryAsTarget, JoinCondition.Or);

				return queryResult;
			}
		}

		public static void AddIgnoredStatusFilter(ModuleFilterCollection collection, Type duplicationType, string tablePrefix, List<ModuleFilter> deduplicationFilters)
		{
			var statusFilter = new ModuleTextFilter(FilterDescriptionConstants.IgnoredStatus, GetIgnoredStatusQuery, IgnoredStatusList);
			statusFilter.Category = FilterCategories.StatusAndFlags;
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("d3f72d96-9057-485c-8c9a-0ccc17282160", "Ignored Status");
			collection.AddCustomFilter(statusFilter);
			deduplicationFilters.Add(statusFilter);

			ZQuery GetIgnoredStatusQuery(ZString value)
			{
				var queryResult = new ZDBOnlyQuery(duplicationType);
				InitSubQuery(value, out var subQueryAsMaster, out var subQueryAsTarget);

				switch (value)
				{
					case DeduplicationHelper.IgnoredStatusConstants.AllIgnores:

						var query1 = new ZDBOnlyQuery(typeof(PatternMatchingResult)).AddToFilter(PatternMatchingResultSchema.PMT_Status, PatternMatchingResult.StatusCodes.PermanentIgnore);
						query1.AddToFilter(JoinCondition.Or, PatternMatchingResultSchema.PMT_Status, PatternMatchingResult.StatusCodes.TemporaryIgnore);

						subQueryAsMaster.AddToFilter(query1);
						subQueryAsTarget.AddToFilter(query1);

						queryResult.AddSubQuery(subQueryAsMaster, JoinCondition.Or);
						queryResult.AddSubQuery(subQueryAsTarget, JoinCondition.Or);

						break;
					case DeduplicationHelper.IgnoredStatusConstants.TemporaryIgnores:

						var query2 = new ZDBOnlyQuery(typeof(PatternMatchingResult)).AddToFilter(PatternMatchingResultSchema.PMT_Status, PatternMatchingResult.StatusCodes.TemporaryIgnore);

						subQueryAsMaster.AddToFilter(query2);
						subQueryAsTarget.AddToFilter(query2);

						queryResult.AddSubQuery(subQueryAsMaster, JoinCondition.Or);
						queryResult.AddSubQuery(subQueryAsTarget, JoinCondition.Or);

						break;
					case DeduplicationHelper.IgnoredStatusConstants.PermanentIgnores:

						var query3 = new ZDBOnlyQuery(typeof(PatternMatchingResult)).AddToFilter(PatternMatchingResultSchema.PMT_Status, PatternMatchingResult.StatusCodes.PermanentIgnore);

						subQueryAsMaster.AddToFilter(query3);
						subQueryAsTarget.AddToFilter(query3);

						queryResult.AddSubQuery(subQueryAsMaster, JoinCondition.Or);
						queryResult.AddSubQuery(subQueryAsTarget, JoinCondition.Or);

						break;
					case DeduplicationHelper.IgnoredStatusConstants.NoIgnores:

						var query4 = new ZDBOnlyQuery(typeof(PatternMatchingResult)).AddToFilter(PatternMatchingResultSchema.PMT_Status, PatternMatchingResult.StatusCodes.PermanentIgnore);
						query4.AddToFilter(JoinCondition.Or, PatternMatchingResultSchema.PMT_Status, PatternMatchingResult.StatusCodes.TemporaryIgnore);

						subQueryAsMaster.AddToFilter(query4);
						subQueryAsTarget.AddToFilter(query4);

						queryResult.AddSubQuery(subQueryAsMaster, JoinCondition.And);
						queryResult.AddSubQuery(subQueryAsTarget, JoinCondition.And);

						break;
					default:

						queryResult.IsNoResultQuery = true;

						break;
				}

				return queryResult;
			}

			void InitSubQuery(ZString value, out ZDBOnlySubQuery subQueryAsMaster, out ZDBOnlySubQuery subQueryAsTarget)
			{
				if (value == DeduplicationHelper.IgnoredStatusConstants.NoIgnores)
				{
					subQueryAsMaster = new ZDBOnlySubQuery(typeof(PatternMatchingResult), PatternMatchingResultSchema.PMT_MasterPK, true);
					subQueryAsTarget = new ZDBOnlySubQuery(typeof(PatternMatchingResult), PatternMatchingResultSchema.PMT_TargetPK, true);
				}
				else
				{
					subQueryAsMaster = new ZDBOnlySubQuery(typeof(PatternMatchingResult), PatternMatchingResultSchema.PMT_MasterPK);
					subQueryAsTarget = new ZDBOnlySubQuery(typeof(PatternMatchingResult), PatternMatchingResultSchema.PMT_TargetPK);
				}

				subQueryAsMaster.AddToFilter(PatternMatchingResultSchema.PMT_MasterTableCode, tablePrefix);
				subQueryAsMaster.AddToFilter(PatternMatchingResultSchema.PMT_TargetTableCode, tablePrefix);
				subQueryAsTarget.AddToFilter(PatternMatchingResultSchema.PMT_MasterTableCode, tablePrefix);
				subQueryAsTarget.AddToFilter(PatternMatchingResultSchema.PMT_TargetTableCode, tablePrefix);
			}
		}

		public static void AddMaximumConfidenceScoreFilter(ModuleFilterCollection collection, List<ModuleFilter> deduplicationFilters, SchemaIntColumn maximumScoreColumn)
		{
			var maximumConfidenceScoreFilter = collection.AddNumberRangeFilter(FilterDescriptionConstants.MaximumConfidenceScore, maximumScoreColumn);
			maximumConfidenceScoreFilter.MultilingualDescription = ResString.GetMultilingualString("11709407-F692-4D0F-9811-523D0729C5D0", "Maximum Confidence Score");
			maximumConfidenceScoreFilter.DefaultPropertySearch = ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo.GetUnresolvedString();
			maximumConfidenceScoreFilter.GreaterThanOrEqualToDefaultProperty = 80;
			maximumConfidenceScoreFilter.PropertySearch = maximumConfidenceScoreFilter.DefaultPropertySearch;
			deduplicationFilters.Add(maximumConfidenceScoreFilter);
		}

		static CodeDescriptionPairList StatusList
		{
			get
			{
				var statusList = new CodeDescriptionPairList(OLookUpEditType.CustomType);
				statusList.AddPair(DeduplicationHelper.StatusConstants.ToBeProcessed, Res.GetString("0d748057-48e4-4bf5-9459-a093480a975d", "To Be Processed"));
				statusList.AddPair(DeduplicationHelper.StatusConstants.Processed, Res.GetString("96ca551f-e719-4581-bf38-97bb5dc1e154", "Processed"));
				statusList.AddPair(DeduplicationHelper.StatusConstants.Excluded, Res.GetString("9bf30380-909f-47a2-9e7a-f28814ec857d", "Excluded"));
				statusList.AddPair(DeduplicationHelper.StatusConstants.Error, Res.GetString("cc624188-f58e-4176-a7eb-feb1490c4b36", "Error"));

				return statusList;
			}
		}

		static CodeDescriptionPairList IgnoredStatusList
		{
			get
			{
				var ignoredStatusList = new CodeDescriptionPairList();
				ignoredStatusList.AddPair(DeduplicationHelper.IgnoredStatusConstants.AllIgnores, Res.GetString("649ae79b-8d86-45c0-ba10-7a4065f1260d", "All Ignores"));
				ignoredStatusList.AddPair(DeduplicationHelper.IgnoredStatusConstants.TemporaryIgnores, Res.GetString("26b3a6d4-ccd4-44ae-9e3f-ffc892e64842", "Temporary Ignores"));
				ignoredStatusList.AddPair(DeduplicationHelper.IgnoredStatusConstants.PermanentIgnores, Res.GetString("b4ab5433-0ed5-4109-8c12-49d0dfe70a4f", "Permanent Ignores"));
				ignoredStatusList.AddPair(DeduplicationHelper.IgnoredStatusConstants.NoIgnores, Res.GetString("5a3c1fba-00fe-42f7-ac69-a49a35ad59fd", "No Ignores"));

				return ignoredStatusList;
			}
		}
	}
}
