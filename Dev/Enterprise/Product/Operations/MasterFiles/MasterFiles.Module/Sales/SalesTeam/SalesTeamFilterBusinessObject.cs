using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class SalesTeamFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			AddTextFilters(result);
			AddRelatedItemFilters(result);
			AddCompanyFilters(result);
			result.AddGuidFilter(nameof(SalesTeam.ParentTeamPk), ModuleIDs.SalesTeam, GlbGroupSchema.GG_GG_ParentGroup, () => new SalesTeamCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("55175B88-F8AD-4905-9521-6A4C99836C04", "Parent Team");
			return result;
		}

		#endregion

		#region System

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = base.Filter;
				query.AddToFilter(GlbGroupSchema.GG_IsSales, ZBool.True);
				return query;
			}
		}

		#endregion

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Code", GlbGroupSchema.GG_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|SalesTeamFilter|Code", "Code");
			filters.AddTextFilter("Sales Team Name", GlbGroupSchema.GG_Desc).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|SalesTeamFilter|SalesTeamName", "Sales Team Name");
		}

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddGuidFilter("Sales Rep", ModuleIDs.SalesRep, GetSalesRepQuery, Staff);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|SalesTeamFilter|SalesRep", "Sales Rep");
			filter.SubGroup = new SalesRepSubGroup();
		}

		class SalesRepSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GlbGroup));

				ZDBOnlySubQuery staffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.PK);
				staffSubQuery.AddToFilter(filter);

				ZDBOnlySubQuery staffLinkQuery = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GG);
				staffLinkQuery.AddSubQuery(GlbGroupLinkSchema.GK_GS, staffSubQuery, JoinCondition.And);

				query.AddSubQuery(staffLinkQuery, JoinCondition.And);
				return query;
			}
		}
		ZQuery GetSalesRepQuery(ZGuid staffPK)
		{
			var query = new ZQuery();
			if (!staffPK.IsEmpty)
			{
				query.AddToFilter(GlbStaffSchema.PK, staffPK);
			}
			return query;
		}

		#endregion

		#region Company

		void AddCompanyFilters(ModuleFilterCollection filters)
		{
			var companyFilter = new CompanyFilter("Company", new GlbCompanyCollection(Factory));
			companyFilter.MultilingualDescription = ResString.GetMultilingualString("07bbb993-e008-471f-b1c5-b41d4682effc", "Company");
			filters.AddFilter(companyFilter);

			var globalOrCurrentLoginCompanyFilter = new GlobalOrCurrentLoginCompanyFilter("GlobalOrCurrentLoginCompany");
			globalOrCurrentLoginCompanyFilter.MultilingualDescription = ResString.GetMultilingualString("1b15cac6-08b2-4a2b-b379-7f548ff56bbb", "Is Global Or Current Login Company");
			filters.AddFilter(globalOrCurrentLoginCompanyFilter);
		}

		static string SearchOutsideLoginCompanyErrorMessage
		{
			get
			{
				return Res.GetString("5a3aa243-45cf-4110-934d-b6ef86239f7c", @"You do not have the appropriate security rights to view or edit Sales Teams outside your current login company ({0}).

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: 

{1}", GlbCompany.CurrentCompany.GC_Code, Env.Security.SalesTeamsAllowSearchOutsideLoginCompany.DisplayTextPathToSecurityRight);
			}
		}

		internal class CompanyFilter : ModuleGuidFilter
		{
			public CompanyFilter(ZString description, IBusinessObjectCollection list)
				: base(description, ModuleIDs.GlbCompany, GlbGroupSchema.GG_GC, list)
			{
				if (!Env.Security.SalesTeamsAllowSearchOutsideLoginCompany.IsAllowed)
				{
					PropertyValidation = CurrentCompanyOnlyPropertyValidation;
				}
			}

			public override IReadOnlyList<string> AllowedComparisonOperators
			{
				get { return Env.Security.SalesTeamsAllowSearchOutsideLoginCompany.IsAllowed ? base.AllowedComparisonOperators : new[] { ModuleGuidFilter.ComparisonConstants.Exact, ModuleGuidFilter.ComparisonConstants.IsBlank }; }
			}

			static void CurrentCompanyOnlyPropertyValidation(ZPropertyInfo info)
			{
				var property = (ZGuid)info.Value;
				if (property.IsValid && property != GlbCompany.CurrentCompany.PK)
				{
					info.AddError(SearchOutsideLoginCompanyErrorMessage);
				}
			}
		}

#if DEBUG
		internal
#endif
		class GlobalOrCurrentLoginCompanyFilter : ModuleFlagsFilter
		{
			public GlobalOrCurrentLoginCompanyFilter(ZString description)
				: base(description, new string[] { FlagName }, new GetFlagsQuery[] { GetGlobalOrCurrentLoginCompanyQuery })
			{
				Visibility = FilterVisibility.AlwaysVisible;
				DefaultProperties[FlagName] = ZBool.True;
			}

			static string FlagName
			{
				get { return ResString.GetMultilingualString("161115d0-f6c5-4b36-b3d3-eaa0189c8953", "Show Global or for Current Login Company Only"); }
			}

			static ZQuery GetGlobalOrCurrentLoginCompanyQuery(ZBool value)
			{
				if (value)
				{
					var query = new ZQuery(GlbGroupSchema.GG_GC, null);
					query.AddToFilter(JoinCondition.Or, GlbGroupSchema.GG_GC, GlbCompany.CurrentCompany.PK);
					return query;
				}
				else
				{
					return new ZQuery();
				}
			}

			#region Validation

			protected override ModuleFilterValidation GetNewValidation()
			{
				return new GlobalOrCurrentLoginCompanyFilterValidation(this);
			}

			class GlobalOrCurrentLoginCompanyFilterValidation : ModuleFlagsFilterValidation
			{
				public GlobalOrCurrentLoginCompanyFilterValidation(ModuleFlagsFilter parent)
					: base(parent)
				{
				}

				protected override void CheckProperty0()
				{
					base.CheckProperty0();

					if (!ParentFilter.Property0 && !Env.Security.SalesTeamsAllowSearchOutsideLoginCompany.IsAllowed)
					{
						ParentFilter.Property0Info.AddError(SearchOutsideLoginCompanyErrorMessage);
					}
				}
			}

			#endregion
		}

		#endregion

		#region Lookups

		public GlbStaffCollection Staff
		{
			get
			{
				if (staff == null)
				{
					staff = new GlbStaffCollection(Factory);
					staff.AdditionalFilter = new ZQuery(GlbStaffSchema.GS_IsSalesRep, true);
				}
				return staff;
			}
		}

		GlbStaffCollection staff;

		#endregion
	}
}
