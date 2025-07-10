using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Module
{
	public class DtbDriverFilterBusinessObject : DtbChildFilterBusinessObject
	{
		public DtbDriverFilterBusinessObject()
			: base()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "DtbRoutePlannerDriver";
		}

		#region FilterConstants

		public static class FilterConstants
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filter name")]
			public const string Branch = "Driver Branch";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filter name")]
			public const string Department = "Driver Department";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filter name")]
			public const string State = "Driver State";

			public static FilterCategory DriverCategory
			{
				get { return FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("DriverFilterBusinessObject|Driver", "Driver")); }
			}
		}

		#endregion

		#region ModuleFilters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var branchFilter = result.AddGuidFilter(FilterConstants.Branch, ModuleIDs.GlbBranch, GlbStaffSchema.GS_GB_HomeBranch, new GlbBranchCollection(Factory));
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("DriverFilterBusinessObject|Branch", "Driver Branch");
			branchFilter.Category = FilterConstants.DriverCategory;

			var departmentFilter = result.AddGuidFilter(FilterConstants.Department, ModuleIDs.GlbDepartment, GlbStaffSchema.GS_GE_HomeDepartment, new GlbDepartmentCollection(Factory));
			departmentFilter.MultilingualDescription = ResString.GetMultilingualString("DriverFilterBusinessObject|Department", "Driver Department");
			departmentFilter.Category = FilterConstants.DriverCategory;

			var stateFilter = result.AddTextFilter(FilterConstants.State, GlbStaffSchema.GS_State);
			stateFilter.MultilingualDescription = ResString.GetMultilingualString("DriverFilterBusinessObject|State", "Driver State");
			stateFilter.Category = FilterConstants.DriverCategory;

			return result;
		}

		#endregion

		#region Implementation

		public override SchemaColumn FieldOnRunsheet
		{
			get { return DtbConsignmentRunSheetSchema.KG_GS_NKTruckDriver; }
		}

		public override SchemaColumn ChildBizOPKOrNK
		{
			get { return GlbStaffSchema.GS_Code; }
		}

		public override IEnumerable<ModuleFilter> ActiveFilters
		{
			get { return routePlannerFilterBusinessObject != null ? routePlannerFilterBusinessObject.ActiveModuleFiltersForChildFilter.Where(c => c.Category == FilterConstants.DriverCategory) : ActiveModuleFilters; }
		}

		public override Type TypeOfBusinessObjectToQuery
		{
			get { return typeof(GlbStaff); }
		}

		#endregion
	}
}
