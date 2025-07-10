using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgStaffAssignmentsLookups : AutoOrgStaffAssignmentsLookups
	{
		public OrgStaffAssignmentsLookups(AutoOrgStaffAssignments parent)
			: base(parent)
		{
		}

		#region Staff Roles

		public virtual ReadOnlyCodeDescriptionPairList StaffRoles
		{
			get { return Implementer.StaffRoles; }
		}

		public override GlbStaffCollection PersonResponsibles
		{
			get { return Implementer.PersonResponsibles; }
		}

		#endregion

		#region Departments

		public const string AllServices = OrgStaffAssignmentsLookupsImplementer.AllServices;
		public const string FreightServices = OrgStaffAssignmentsLookupsImplementer.FreightServices;
		public const string SeaFreightServices = OrgStaffAssignmentsLookupsImplementer.SeaFreightServices;
		public const string AirFreightServices = OrgStaffAssignmentsLookupsImplementer.AirFreightServices;
		public const string RoadFreightServices = OrgStaffAssignmentsLookupsImplementer.RoadFreightServices;
		public const string RailFreightServices = OrgStaffAssignmentsLookupsImplementer.RailFreightServices;
		public const string ClearanceServices = OrgStaffAssignmentsLookupsImplementer.ClearanceServices;
		public const string CFSServices = OrgStaffAssignmentsLookupsImplementer.CFSServices;
		public const string WarehouseServices = OrgStaffAssignmentsLookupsImplementer.WarehouseServices;
		public const string ContainerYardServices = OrgStaffAssignmentsLookupsImplementer.ContainerYardServices;
		public const string LocalCartageServices = OrgStaffAssignmentsLookupsImplementer.LocalCartageServices;

		public CodeDescriptionPairList DepartmentCodes
		{
			get
			{
				return Implementer.DepartmentCodes;
			}
		}

		#endregion

		#region Helper

		OrgStaffAssignmentsLookupsImplementer Implementer
		{
			get { return OrgStaffAssignmentsLookupsImplementer.Get(Factory); }
		}

		#endregion
	}
}
