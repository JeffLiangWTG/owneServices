using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.GUI
{
	public class DeduplicationPersonFilterBusinessObject : GlbPersonFilterBusinessObject
	{
		public DeduplicationPersonFilterBusinessObject()
		{
			QueryObjectType = typeof(DeduplicationPerson);
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "DeduplicationPerson";
		}

		public override ZQuery Filter
		{
			get
			{
				var query = new ZDBOnlyQuery(typeof(DeduplicationPerson));

				if (!base.Filter.IsEmpty)
				{
					var subQuery = new ZDBOnlySubQuery(typeof(GlbPerson), GlbPersonSchema.PK);
					subQuery.AddToFilter(base.Filter);
					query.AddSubQuery(subQuery, JoinCondition.And);
				}

				return query;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string for dev only")]
		const string TextSearchCategoryDescription = "Text Search";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string for dev only")]
		const string OtherCategoryDescription = "Other";

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			// TODO: Add other filter from GlbPersonFilterBusinessObject
			SetActiveStatusFilter(GlbPersonSchema.PER_IsActive, true);
			var collection = new ModuleFilterCollection();
			var filters = base.GetModuleFiltersCore();

			filters.ForEach(f =>
			{
				var category = f.Category.Description.GetUnresolvedString();
				if (category == TextSearchCategoryDescription ||
					category == OtherCategoryDescription)
				{
					collection.AddFilter(f);
				}
			});

			AddExcludedByFilter(collection);

			AddConfidenceFilters(collection);

			AddDeduplicationStatusFilter(collection);

			AddIgnoredByFilter(collection);

			AddIgnoredStatusFilter(collection);

			AddMaximumConfidenceScoreFilter(collection);

			return collection;
		}

		#region Excluded By Filter

		void AddExcludedByFilter(ModuleFilterCollection collection)
		{
			DeduplicationFilterUtils.AddExcludedByFilter(collection, GlbStaffList, MDMAdminPanelPersonViewSchema.DPE_Status, MDMAdminPanelPersonViewSchema.DPE_ExcludedBy, DeduplicationFilters);
		}

		GlbStaffCollection GlbStaffList => glbStaffList ?? (glbStaffList = new GlbStaffCollection(Factory));
		GlbStaffCollection glbStaffList;

		#endregion

		#region Confidence Filters

		void AddConfidenceFilters(ModuleFilterCollection collection)
		{
			DeduplicationFilterUtils.AddConfidenceFilters(collection,
				MDMAdminPanelPersonViewSchema.DPE_TotalDuplicates,
				MDMAdminPanelPersonViewSchema.DPE_HighDuplicates,
				MDMAdminPanelPersonViewSchema.DPE_MediumDuplicates,
				MDMAdminPanelPersonViewSchema.DPE_LowDuplicates,
				DeduplicationFilters);
		}

		#endregion

		#region Deduplication Status Filter

		void AddDeduplicationStatusFilter(ModuleFilterCollection collection)
		{
			DeduplicationFilterUtils.AddDeduplicationStatusFilter(collection, MDMAdminPanelPersonViewSchema.DPE_Status, DeduplicationFilters);
		}

		#endregion

		#region Ignored By Filter

		void AddIgnoredByFilter(ModuleFilterCollection collection)
		{
			DeduplicationFilterUtils.AddIgnoredByFilter(collection, GlbStaffList, typeof(DeduplicationPerson), GlbPersonSchema.Constants.Prefix, DeduplicationFilters);
		}

		#endregion

		#region Ignored Status Filter

		void AddIgnoredStatusFilter(ModuleFilterCollection collection)
		{
			DeduplicationFilterUtils.AddIgnoredStatusFilter(collection, typeof(DeduplicationPerson), GlbPersonSchema.Constants.Prefix, DeduplicationFilters);
		}

		#endregion

		#region Maximum Confidence Score Filter

		void AddMaximumConfidenceScoreFilter(ModuleFilterCollection collection)
		{
			DeduplicationFilterUtils.AddMaximumConfidenceScoreFilter(collection, DeduplicationFilters, MDMAdminPanelPersonViewSchema.DPE_MaxResult);
		}

		#endregion

		protected override bool ShouldAddUserDefinedFiltersCore => false;

		protected override bool ShouldAddCustomSqlFilter => true;

		protected override bool ShouldUseHelperFilter => false;

		List<ModuleFilter> DeduplicationFilters => deduplicationFilters ?? (deduplicationFilters = new List<ModuleFilter>());
		List<ModuleFilter> deduplicationFilters;
	}
}
