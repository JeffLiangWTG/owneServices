using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefEquipmentFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddTextFilters(result);
			AddFlagsFilters(result);

			var providers = ObjectFactory.Get<IEnumerable>("EquipmentModuleColumnsAndFiltersProviders");
			foreach (IRefEquipmentModuleColumnsAndFiltersProvider provider in providers)
			{
				provider.AddFilters(result);
			}
			return result;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Short Code", RefEquipmentSchema.RQ_ShortCode).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefEquipmentFilter|ShortCode", "Short Code");
			filters.AddFiltersForTranslatableText("Description", RefEquipmentSchema.RQ_Description, typeof(RefEquipment), ResString.GetMultilingualString("MasterFiles|RefEquipmentFilter|Description", "Description"));

			var equipmentTypeFilter = filters.AddGuidFilter("Equipment Type", ModuleIDs.RefContainer, RefEquipmentSchema.RQ_RC_RoadContainerType, EquipmentType_List);
			equipmentTypeFilter.Category = FilterCategories.TextSearch;
			equipmentTypeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefEquipmentFilter|EquipmentType", "Equipment Type");

			var equipmentGroupFilter = filters.AddTextFilter("Equipment Group", RefEquipmentSchema.RQ_EquipmentGroup, RQ_EquipmentGroup_List);
			equipmentGroupFilter.Category = FilterCategories.TextSearch;
			equipmentGroupFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefEquipmentFilter|EquipmentGroup", "Equipment Group");

			var driverFilter = filters.AddTextFilter("Driver", RefEquipmentSchema.RQ_GS_NKPreferredDriver);
			driverFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefEquipmentFilter|Driver", "Driver");
			driverFilter.Category = FilterCategories.TextSearch;
		}

		#endregion

		#region Flags

		void AddFlagsFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter vehicleFilter = filters.AddTextFilter("Vehicle Status", IsVehicleStatusFilter, VehicleStatusList);
			vehicleFilter.Category = FilterCategories.StatusAndFlags;
			vehicleFilter.DefaultProperty = IsVehicleFilterTypes.IsVehicle;
			vehicleFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefEquipmentFilter|VehicleStatus", "Vehicle Status");
		}

		ZQuery IsVehicleStatusFilter(ZString value)
		{
			ZQuery query = new ZQuery();

			if (value == IsVehicleFilterTypes.IsVehicle)
			{
				query.AddToFilter(RefEquipmentSchema.RQ_IsVehicle, ZBool.True);
			}
			else if (value == IsVehicleFilterTypes.IsNotVehicle)
			{
				query.AddToFilter(RefEquipmentSchema.RQ_IsVehicle, ZBool.False);
			}

			return query;
		}

		#endregion

		#endregion

		#region Lookups

		CodeDescriptionPairList VehicleStatusList
		{
			get
			{
				if (RQVehicleList == null)
				{
					RQVehicleList = new CodeDescriptionPairList();
					RQVehicleList.AddPair(IsVehicleFilterTypes.All, Res.GetString("MasterFiles|RefEquipmentFilter|All", "All"));
					RQVehicleList.AddPair(IsVehicleFilterTypes.IsVehicle, Res.GetString("MasterFiles|RefEquipmentFilter|IsAVehicle", "Is a Vehicle"));
					RQVehicleList.AddPair(IsVehicleFilterTypes.IsNotVehicle, Res.GetString("MasterFiles|RefEquipmentFilter|IsNotAVehicle", "Is not a Vehicle"));
				}
				return RQVehicleList;
			}
		}

		CodeDescriptionPairList RQVehicleList;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related, constants")]
		public static class IsVehicleFilterTypes
		{
			public const string All = "All";
			public const string IsVehicle = "Is a Vehicle";
			public const string IsNotVehicle = "Is not a Vehicle";
		}

		#region EquipmentType_List

		RefContainerCollection fEquipmentType_List;

		RefContainerCollection EquipmentType_List
		{
			get
			{
				if (fEquipmentType_List == null)
				{
					fEquipmentType_List = new RefContainerCollection(Factory, RefContainerLookups.ShippingModes.Road);
				}
				return fEquipmentType_List;
			}
		}

		#endregion

		#region RQ_EquipmentGroup_List

		CodeDescriptionPairList RQ_EquipmentGroup_List
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.EquipmentGroup); }
		}

		#endregion

		#endregion
	}
}
