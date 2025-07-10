using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	class RefOrgPartCategoryFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			filters.AddTextFilter("Code", OrgPartCategorySchema.OPC_CategoryCode).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefOrgPartCategoryFilter|Code", "Code");
			filters.AddFiltersForTranslatableText("Description", OrgPartCategorySchema.OPC_CategoryDescription, typeof(OrgPartCategory), ResString.GetMultilingualString("MasterFiles|RefOrgPartCategoryFilter|Description", "Description"));

			var childCategoriesFilter = filters.AddGuidFilter("Child Categories", ModuleIDs.RefOrgPartCategory, OrgPartCategorySchema.OPC_OPC_Parent, OrgPartCategories);
			childCategoriesFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefOrgPartCategoryFilter|ChildCategories", "Child Categories of");
			childCategoriesFilter.PropertyValidation = ChildCategoriesFilterValidation;
			childCategoriesFilter.SubGroup = new ChildCategoriesSubGroup();

			// category level filter
			var categoryLevelFilter = filters.AddTextFilter("Category Level", GetCategoryLevelFilter, GetCategoryLevelList());
			categoryLevelFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefOrgPartCategoryFilter|CategoryLevel", "Category Level");

			return filters;
		}

		#region Category Level Filter

		ZQuery GetCategoryLevelFilter(ZString categoryLevel)
		{
			ZQuery result = null;

			switch (categoryLevel)
			{
				case AllCategories:
					result = new ZDBOnlyQuery(typeof(OrgPartCategory));
					break;

				case TopLevelCategories:
					result = new ZDBOnlyQuery(typeof(OrgPartCategory));
					result.AddToFilter(OrgPartCategorySchema.OPC_OPC_Parent, SQLComparisonOperator.Equal, DBNull.Value);
					break;

				case ChildCategories:
					result = new ZDBOnlyQuery(typeof(OrgPartCategory));
					result.AddToFilter(OrgPartCategorySchema.OPC_OPC_Parent, SQLComparisonOperator.NotEqual, DBNull.Value);
					break;

				default:
					result = new ZDBOnlyQuery(typeof(OrgPartCategory));
					break;
			}

			return result;
		}

		public CodeDescriptionPairList GetCategoryLevelList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			result.AddPair(AllCategories, Res.GetString("250AC083-1BB1-4E27-81BD-2AF6541CD75D", "All Categories"));
			result.AddPair(TopLevelCategories, Res.GetString("287AB15D-8126-4CF4-9C55-93B189F92CAF", "Top-Level Categories"));
			result.AddPair(ChildCategories, Res.GetString("45EDD41B-F31F-44A1-A6B0-BE519C220E64", "Child Categories"));

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Drop List Code.")]
		const string AllCategories = "All Categories";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Drop List Code.")]
		const string TopLevelCategories = "Top-Level Categories";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Drop List Code.")]
		const string ChildCategories = "Child Categories";

		#endregion

		#region Child Categories Filter

		class ChildCategoriesSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(OrgPartCategory), OrgPartCategorySchema.PK);
				subQuery.AddToFilter(filter);

				var result = new ZDBOnlyQuery(typeof(OrgPartCategory));
				result.AddToFilter(filter);
				result.AddSubQuery(OrgPartCategorySchema.OPC_OPC_Parent, subQuery, JoinCondition.Or);

				return result;
			}
		}

		void ChildCategoriesFilterValidation(ZPropertyInfo info)
		{
			if (info.Value.IsEmpty)
			{
				info.AddError(Res.GetString("BFE48C0A-86D9-47BF-92CE-48811A22935C", "Please select a parent category to filter by"));
			}
		}

		#endregion

		#region OrgPartCategories

		public OrgPartCategoryCollection OrgPartCategories
		{
			get { return new OrgPartCategoryCollection(Factory); }
		}

		#endregion
	}
}
