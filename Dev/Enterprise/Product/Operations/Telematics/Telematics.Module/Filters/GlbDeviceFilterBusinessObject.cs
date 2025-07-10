using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Telematics.Module.Filters
{
	public class GlbDeviceFilterBusinessObject : FilterStripBusinessObject
	{
		#region FilterStripBusinessObject

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var humanReadableIDFilter = filters.AddTextFilter("Serial Number", GlbDeviceSchema.V3_HumanReadableIdentifier);
			humanReadableIDFilter.MultilingualDescription = ResString.GetMultilingualString("Telematics|GlbDeviceFilter|SerialNumber", "Serial Number");
			humanReadableIDFilter.Category = FilterCategories.NumbersAndReferences;

			var modelFilter = filters.AddTextFilter("Model", GlbDeviceSchema.V3_Model);
			modelFilter.MultilingualDescription = ResString.GetMultilingualString("Telematics|GlbDeviceFilter|Model", "Model");
			modelFilter.Category = FilterCategories.TextSearch;

			var assignedStaffFilter = filters.AddGuidFilter("Assigned Staff", ModuleIDs.GlbStaff, GetQueryForAssignedStaff, new GlbStaffCollection(Factory));
			assignedStaffFilter.MultilingualDescription = ResString.GetMultilingualString("Telematics|GlbDeviceFilter|AssignedStaff", "Assigned Staff");
			assignedStaffFilter.Category = FilterCategories.RelationshipOrgAndStaff;

			var assignedEquipmentFilter = filters.AddGuidFilter("Assigned Equipment", ModuleIDs.GlbStaff, GetQueryForAssignedEquipment, new RefEquipmentCollection(Factory));
			assignedEquipmentFilter.MultilingualDescription = ResString.GetMultilingualString("Telematics|GlbDeviceFilter|AssignedEquipment", "Assigned Equipment");
			assignedEquipmentFilter.Category = FilterCategories.RelationshipOrgAndStaff;

			return filters;
		}

		#endregion

		#region Custom Queries

		ZQuery GetQueryForAssignedStaff(ZGuid parentID)
		{
			return GetQueryForAssignedParent(GlbStaffSchema.Constants.Prefix, parentID);
		}

		ZQuery GetQueryForAssignedEquipment(ZGuid parentID)
		{
			return GetQueryForAssignedParent(RefEquipmentSchema.Constants.Prefix, parentID);
		}

		ZQuery GetQueryForAssignedParent(string parentTableCode, ZGuid parentID)
		{
			var query = new ZDBOnlyQuery(typeof(GlbDevice));
			var subQuery = new ZDBOnlySubQuery(typeof(GlbDeviceAssignmentDivot), GlbDeviceAssignmentDivotSchema.V7_V3_Device);
			subQuery.AddToFilter(GlbDeviceAssignmentDivotSchema.V7_ParentID, SQLComparisonOperator.Equal, parentID);
			subQuery.AddToFilter(GlbDeviceAssignmentDivotSchema.V7_ParentTableCode, SQLComparisonOperator.Equal, parentTableCode);
			subQuery.AddToFilter(GlbDeviceAssignmentDivotSchema.V7_EndTimeUtc, SQLComparisonOperator.Equal, null);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		#endregion
	}
}
