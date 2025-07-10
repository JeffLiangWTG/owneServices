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
	public class DtbVehicleFilterBusinessObject : DtbChildFilterBusinessObject
	{
		public DtbVehicleFilterBusinessObject()
			: base()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "DtbRoutePlannerVehicle";
		}

		#region FilterConstants

		public static class FilterConstants
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filter name")]
			public const string Owner = "Vehicle Owner";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filter name")]
			public const string State = "Vehicle State";

			public static FilterCategory VehicleCategory
			{
				get { return FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("VehicleFilterBusinessObject|Vehicle", "Vehicle")); }
			}
		}

		#endregion

		#region ModuleFilters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var branchFilter = result.AddGuidFilter(FilterConstants.Owner, ModuleIDs.Organisation, RefEquipmentSchema.RQ_OH_Owner, new OrgHeaderCollection(Factory));
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("VehicleFilterBusinessObject|Owner", "Vehicle Owner");
			branchFilter.Category = FilterConstants.VehicleCategory;

			var stateFilter = result.AddTextFilter(FilterConstants.State, RefEquipmentSchema.RQ_RegState);
			stateFilter.MultilingualDescription = ResString.GetMultilingualString("VehicleFilterBusinessObject|State", "Vehicle State");
			stateFilter.Category = FilterConstants.VehicleCategory;

			return result;
		}

		#endregion

		#region Implementation

		public override SchemaColumn FieldOnRunsheet
		{
			get { return DtbConsignmentRunSheetSchema.KG_RQ_Truck; }
		}

		public override SchemaColumn ChildBizOPKOrNK
		{
			get { return RefEquipmentSchema.PK; }
		}

		public override IEnumerable<ModuleFilter> ActiveFilters
		{
			get { return routePlannerFilterBusinessObject != null ? routePlannerFilterBusinessObject.ActiveModuleFiltersForChildFilter.Where(c => c.Category == FilterConstants.VehicleCategory) : ActiveModuleFilters; }
		}

		public override Type TypeOfBusinessObjectToQuery
		{
			get { return typeof(RefEquipment); }
		}

		#endregion
	}
}
