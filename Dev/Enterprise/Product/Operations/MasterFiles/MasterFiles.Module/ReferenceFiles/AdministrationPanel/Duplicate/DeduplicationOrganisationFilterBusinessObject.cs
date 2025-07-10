using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class DeduplicationOrganisationFilterBusinessObject : OrganisationFilterBusinessObject
	{
		public DeduplicationOrganisationFilterBusinessObject()
		{
			this.QueryObjectType = typeof(DeduplicationOrganisation);
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "DeduplicationOrganisation";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string for dev only")]
		const string TextSearchCategoryDescription = "Text Search";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string for dev only")]
		const string OrganizationTypeCategoryDescription = "Organization Type";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string for dev only")]
		const string RegistrationNumbersCategoryDescription = "Registration Numbers";

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			SetActiveStatusFilter(OrgHeaderSchema.OH_IsActive, true);
			var collection = new ModuleFilterCollection();
			var filters = base.GetModuleFiltersCore();
			collection.AddFilter(filters["Organization – Account Type"]);
			collection.AddFilter(filters["Category"]);
			collection.AddFilter(filters["Language"]);
			collection.AddFilter(filters["Secondary Type"]);
			collection.AddFilter(filters["Created"]);
			collection.AddFilter(filters["Main UNLOCO"]);
			filters.ForEach(f =>
			{
				var category = f.Category.Description.GetUnresolvedString();
				if (f.Description != "SystemDefinedOrg" &&
				(category == TextSearchCategoryDescription ||
					category == OrganizationTypeCategoryDescription ||
					category == RegistrationNumbersCategoryDescription))
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
			DeduplicationFilterUtils.AddExcludedByFilter(collection, GlbStaffList, MDMAdminPanelOrganisationViewSchema.DOH_Status, MDMAdminPanelOrganisationViewSchema.DOH_ExcludedBy, DeduplicationFilters);
		}

		GlbStaffCollection GlbStaffList => glbStaffList ?? (glbStaffList = new GlbStaffCollection(Factory));
		GlbStaffCollection glbStaffList;

		#endregion

		#region Confidence Filters

		void AddConfidenceFilters(ModuleFilterCollection collection)
		{
			DeduplicationFilterUtils.AddConfidenceFilters(collection,
				MDMAdminPanelOrganisationViewSchema.DOH_TotalDuplicates,
				MDMAdminPanelOrganisationViewSchema.DOH_HighDuplicates,
				MDMAdminPanelOrganisationViewSchema.DOH_MediumDuplicates,
				MDMAdminPanelOrganisationViewSchema.DOH_LowDuplicates,
				DeduplicationFilters);
		}

		#endregion

		#region Deduplication Status Filter

		void AddDeduplicationStatusFilter(ModuleFilterCollection collection)
		{
			DeduplicationFilterUtils.AddDeduplicationStatusFilter(collection, MDMAdminPanelOrganisationViewSchema.DOH_Status, DeduplicationFilters);
		}

		#endregion

		#region Ignored By Filter

		void AddIgnoredByFilter(ModuleFilterCollection collection)
		{
			DeduplicationFilterUtils.AddIgnoredByFilter(collection, GlbStaffList, typeof(DeduplicationOrganisation), OrgHeaderSchema.Constants.Prefix, DeduplicationFilters);
		}

		#endregion

		#region Ignored Status Filter

		void AddIgnoredStatusFilter(ModuleFilterCollection collection)
		{
			DeduplicationFilterUtils.AddIgnoredStatusFilter(collection, typeof(DeduplicationOrganisation), OrgHeaderSchema.Constants.Prefix, DeduplicationFilters);
		}

		#endregion

		#region  Maximum Confidence Score Filter

		void AddMaximumConfidenceScoreFilter(ModuleFilterCollection collection)
		{
			DeduplicationFilterUtils.AddMaximumConfidenceScoreFilter(collection, DeduplicationFilters, MDMAdminPanelOrganisationViewSchema.DOH_MaxResult);
		}

		#endregion

		protected override bool ShouldAddWorkflowCustomFieldsFilters => false;

		protected override bool ShouldAddUserDefinedFiltersCore => false;

		protected override bool ShouldAddCustomSqlFilter => true;

		protected override bool ShouldUseHelperFilter => false;

		public override ZQuery Filter
		{
			get
			{
				var query = new ZDBOnlyQuery(typeof(DeduplicationOrganisation));

				if (!base.Filter.IsEmpty)
				{
					var subQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
					subQuery.AddToFilter(base.Filter);
					query.AddSubQuery(subQuery, JoinCondition.And);
				}

				query.AddToFilter(ExcludeSystemDefinedOrgQuery);
				return query;
			}
		}

		ZQuery ExcludeSystemDefinedOrgQuery => new ZQuery().
			AddToFilter(MDMAdminPanelOrganisationViewSchema.PK, SQLComparisonOperator.NotEqual, OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation).
			AddToFilter(JoinCondition.And, MDMAdminPanelOrganisationViewSchema.PK, SQLComparisonOperator.NotEqual, OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation);

		List<ModuleFilter> DeduplicationFilters => deduplicationFilters ?? (deduplicationFilters = new List<ModuleFilter>());
		List<ModuleFilter> deduplicationFilters;
	}
}
