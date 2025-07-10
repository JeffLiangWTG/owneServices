using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Module
{
	/// <summary>
	/// Common base class for the filters of Rating Header modules.
	/// </summary>
	public abstract class RatingFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddDateFilters(filters);
			AddOrganisationFilters(filters);
			AddStaffFilters(filters);
			AddTextFilters(filters);
			AddRateEntryFilters(filters);
			AddRateLineFilters(filters);
			AddGlobalRateFilter(filters);

			return filters;
		}

		#region Date Filters

		protected virtual void AddDateFilters(ModuleFilterCollection filters)
		{
		}

		#endregion

		#region Organisation Filters

		protected virtual void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			CreateOrganisationFilter(filters);
		}

		protected virtual ModuleGuidFilter CreateOrganisationFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddGuidFilter(RatingHeaderOrganisationName, ModuleIDs.Organisation, RatingHeaderSchema.TH_OH, TH_OH_List);
			filter.MultilingualDescription = RatingHeaderOrganisationCaption;
			return filter;
		}

		protected virtual string RatingHeaderOrganisationName
		{
			get { return RateFilterHelper.Constants.Client; }
		}

		protected virtual MultilingualString RatingHeaderOrganisationCaption
		{
			get { return ResString.GetMultilingualString("849a6867-eeec-45b7-8212-7ee0b8889f68", "Client"); }
		}

		#endregion

		#region Staff Filters

		void AddStaffFilters(ModuleFilterCollection filters)
		{
			if (ShouldIncludeSalesRepFilter)
			{
				var salesRepFilter = CreateSalesRepFilter(filters);
				salesRepFilter.Category = FilterCategories.Organisations;
			}
		}

		protected virtual ModuleNkFilter CreateSalesRepFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddNkFilter(RateFilterHelper.Constants.StaffFilterSalesRep, OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, ModuleIDs.GlbStaff, Staff);
			filter.SubGroup = new SalesRepSubGroup(RateFilterHelper);
			filter.MultilingualDescription = ResString.GetMultilingualString("c09ccfc5-19ca-401c-8ba8-747bc71ba962", "Sales Rep");
			return filter;
		}

		class SalesRepSubGroup : ModuleFilterSubGroup
		{
			public SalesRepSubGroup(RateFilterHelper helper)
			{
				this.helper = helper;
			}

			readonly RateFilterHelper helper;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return helper.GetSalesRepQuery(filter);
			}
		}

		protected virtual bool ShouldIncludeSalesRepFilter
		{
			get { return true; }
		}

		#endregion

		#region Text Filters

		protected virtual void AddTextFilters(ModuleFilterCollection filters)
		{
			var organizationNameFilter = filters.AddTextFilter(RateFilterHelper.Constants.OrganizationName, GetOrganizationNameQuery);
			organizationNameFilter.MaxLength = OrgHeaderSchema.OH_FullName.MaxLength;
			organizationNameFilter.MultilingualDescription = GetOrganizationNameString();
			organizationNameFilter.Category = FilterCategories.Organisations;
		}

		#region Organization Name

		protected virtual ZQuery GetOrganizationNameQuery(SQLComparisonOperator comparisonOperator, ZString organizationName)
		{
			var rateQueryHelper = new RateQueryHelper(Factory);

			var result = new ZDBOnlyQuery(typeof(RatingHeader));
			result.AddSubQuery(rateQueryHelper.GetOrganizationNameSubQuery(comparisonOperator, RatingHeaderSchema.TH_OH, organizationName), JoinCondition.And);
			return result;
		}

		protected virtual MultilingualString GetOrganizationNameString()
		{
			return ResString.GetMultilingualString("C9983878-E653-42F3-87CC-83255D83EE8A", "Client Name");
		}

		#endregion

		#endregion

		#region Global Rate Filter

		void AddGlobalRateFilter(ModuleFilterCollection filters)
		{
			if (ShouldIncludeGlobalRateFilter)
			{
				var descriptions = new[]
				{
					Res.GetString("8b626495-63a5-4f66-9521-6bbed9d5f9e1", "Globally"),
					Res.GetString("7478e1b9-4ec3-4e8a-986e-9de20933af9b", "Locally")
				};
				var flags = new GetFlagsQuery[]
				{
					GetShowGlobalQuery,
					GetShowLocalQuery
				};

				var globalRateFilter = filters.AddFlagsFilter(RateFilterHelper.Constants.GlobalRateFilter, descriptions, flags, JoinCondition.Or);
				globalRateFilter.MultilingualDescription = ResString.GetMultilingualString("7efd97e2-46ff-41ba-8ccf-042e80952bcd", "Published");
				globalRateFilter.Category = FilterCategories.ModesAndTypes;
			}
		}

		ZQuery GetShowGlobalQuery(ZBool showGlobal)
		{
			var result = new ZDBOnlyQuery(typeof(RatingHeader));

			if (showGlobal)
			{
				result.AddToFilter(RatingHeaderSchema.TH_GC, null);
			}

			return result;
		}

		ZQuery GetShowLocalQuery(ZBool showLocal)
		{
			var result = new ZDBOnlyQuery(typeof(RatingHeader));

			if (showLocal)
			{
				result.AddToFilter(RatingHeaderSchema.TH_GC, SQLComparisonOperator.NotEqual, null);
			}

			return result;
		}

		protected virtual bool ShouldIncludeGlobalRateFilter => true;

		#endregion

		#region Rate Filters

		void AddRateEntryFilters(ModuleFilterCollection filters)
		{
			var provider = new RateEntryFilterProvider(Factory);

			foreach (var filter in provider.GetRateEntryFilters(GetRateType(), filterCategory: string.Empty, ShouldIncludeAllStandardRateEntryFilters))
			{
				filter.SubGroup = RateFilterHelper.RateEntryFilterProcessor;
				filters.AddFilter(filter);
			}
		}

		protected virtual bool ShouldIncludeAllStandardRateEntryFilters => true;

		void AddRateLineFilters(ModuleFilterCollection filters)
		{
			new RateLineModuleFilters(Factory, GetRateType(), null, false)
				.AddForRatingHeaderFilter(filters);
		}

		#endregion

		#endregion

		#region Lookups

		#region Organisation

		protected OrgHeaderCollection TH_OH_List
		{
			get
			{
				if (fTH_OH_List == null)
				{
					fTH_OH_List = new OrgHeaderCollection(Factory);
				}
				return fTH_OH_List;
			}
		}

		OrgHeaderCollection fTH_OH_List;

		#endregion

		#region Staff

		protected GlbStaffCollection Staff
		{
			get
			{
				if (fStaff == null)
				{
					fStaff = new GlbStaffCollection(Factory);
				}
				return fStaff;
			}
		}

		GlbStaffCollection fStaff;

		#endregion

		#endregion

		#region Filter Helper

		protected RateFilterHelper RateFilterHelper
		{
			get
			{
				if (fRateFilterHelper == null)
				{
					fRateFilterHelper = new RateFilterHelper(Factory);
				}
				return fRateFilterHelper;
			}
		}

		RateFilterHelper fRateFilterHelper;

		#endregion

		protected abstract string GetRateType();
	}
}

