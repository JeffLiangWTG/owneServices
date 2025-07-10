using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefComplianceListFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddFlagsFilters(filters);
			AddTextFilters(filters);

			return filters;
		}

		#region Text Filters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var listCodeFilter = filters.AddTextFilter("Code", RefComplianceListSchema.RCL_ListCode);
			listCodeFilter.MultilingualDescription = ResString.GetMultilingualString("E565237A-5497-4A3B-B299-FF6695B78338", "Code");

			var listNameFilter = filters.AddTextFilter("Name", RefComplianceListSchema.RCL_ListName);
			listNameFilter.MultilingualDescription = ResString.GetMultilingualString("BF82FEF1-3A6C-4554-9175-26D8C726DAA2", "Name");

			var listPublisherFilter = filters.AddTextFilter("Publisher", RefComplianceListSchema.RCL_ListPublisher);
			listPublisherFilter.MultilingualDescription = ResString.GetMultilingualString("85963163-6779-407C-A176-3DE218C08938", "Publisher");

			var publisherJurisdictionFilter = filters.AddTextFilter("Publisher Jurisdiction", RefComplianceListSchema.RCL_PublisherJurisdiction);
			publisherJurisdictionFilter.MultilingualDescription = ResString.GetMultilingualString("C0BAFF15-A02D-4F26-B561-2BDE56A4EEB5", "Publisher Jurisdiction");

			var listTypeFilter = filters.AddTextFilter("Type", RefComplianceListSchema.RCL_ListType);
			listTypeFilter.MultilingualDescription = ResString.GetMultilingualString("3FA448C6-FC8B-4EA0-AEE3-E69B52217E43", "Type");
		}

		#endregion

		#region Flags Filters

		void AddFlagsFilters(ModuleFilterCollection filters)
		{
			var exclusionStatusFilter = filters.AddTextFilter("Exclusion Status", GetExclusionStatusFilter, GetExclusionStatuses());
			exclusionStatusFilter.Category = FilterCategories.StatusAndFlags;
			exclusionStatusFilter.MultilingualDescription = ResString.GetMultilingualString("B0C59EAF-5476-4DED-8CEB-7CB4855433D8", "Exclusion Status");
		}

		#endregion

		#region ExclusionStatus

		ZQuery GetExclusionStatusFilter(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(RefComplianceList));
			if (!string.IsNullOrWhiteSpace(value) && value != RefComplianceListExclusionStatusFilterCodes.All)
			{
				ZDBOnlySubQuery subQuery;
				if (value == RefComplianceListExclusionStatusFilterCodes.Excluded)
				{
					subQuery = new ZDBOnlySubQuery(typeof(RefComplianceList), RefComplianceListSchema.PK);
				}
				else
				{
					subQuery = new ZDBOnlySubQuery(typeof(RefComplianceList), RefComplianceListSchema.PK, true); //using NOT IN
				}
				subQuery.AddToFilter(RefComplianceListSchema.RCL_IsExcluded, 1);

				query.AddSubQuery(RefComplianceListSchema.PK, subQuery, JoinCondition.And);
			}

			return query;
		}

		CodeDescriptionPairList GetExclusionStatuses()
		{
			var list = new CodeDescriptionPairList();

			list.AddPair(RefComplianceListExclusionStatusFilterCodes.All, ResString.GetMultilingualString("32D8D8FF-8779-47EE-9597-B99459A17814", "All"));
			list.AddPair(RefComplianceListExclusionStatusFilterCodes.Excluded, ResString.GetMultilingualString("56C0A3C8-1C5D-4B2A-9698-36A58341B178", "Excluded"));
			list.AddPair(RefComplianceListExclusionStatusFilterCodes.Included, ResString.GetMultilingualString("551FA6A2-AD9B-458C-AC3F-10E85AD5D24A", "Included"));

			return list;
		}

		public static class RefComplianceListExclusionStatusFilterCodes
		{
			public const string All = "ALL";
			public const string Excluded = "EXC";
			public const string Included = "INC";
		}

		#endregion
	}
}
