using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgStaffAssignmentsLookupsImplementer : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OrgStaffAssignmentsLookupsImplementer(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		static OrgStaffAssignmentsLookupsImplementer New(BusinessObjectFactory factory)
		{
			var implementerType = TypeDecider.GetTypeForBinding(typeof(OrgStaffAssignmentsLookupsImplementer));

			var result = (OrgStaffAssignmentsLookupsImplementer)Activator.CreateInstance(implementerType, new[] { factory });

			return result;
		}

		public static OrgStaffAssignmentsLookupsImplementer Get(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");

			return factory.GetCachedValue("OrgStaffAssignmentsLookupsImplementer", () => New(factory));
		}

		public ReadOnlyCodeDescriptionPairList StaffRoles
		{
			get { return Factory.GetCachedValue("OrgStaffAssignmentsLookupsImplementer|StaffRoles", GetNewStaffRoles); }
		}

		protected virtual ReadOnlyCodeDescriptionPairList GetNewStaffRoles()
		{
			return DataRegistry.Instance.OrgStaffMemberAssignmentRoles;
		}

		public GlbStaffCollection PersonResponsibles
		{
			get { return Factory.GetCachedValue("OrgStaffAssignmentsLookupsImplementer|PersonResponsibles", () => new GlbStaffCollection(Factory)); }
		}

		public GlbBranchCollection ControllingBranches
		{
			get { return Factory.GetCachedValue("OrgStaffAssignmentsLookupsImplementer|ControllingBranches", () => new GlbBranchCollection(Factory)); }
		}

		#region DepartmentsList

		public const string AllServices = "ALL";
		public const string FreightServices = "FRT";
		public const string SeaFreightServices = "SEA";
		public const string AirFreightServices = "AIR";
		public const string RoadFreightServices = "ROD";
		public const string RailFreightServices = "RAI";
		public const string ClearanceServices = "CLR";
		public const string CFSServices = "CFS";
		public const string WarehouseServices = "WAR";
		public const string ContainerYardServices = "CON";
		public const string LocalCartageServices = "LOC";

		public CodeDescriptionPairList DepartmentCodes
		{
			get { return Factory.GetCachedValue("OrgStaffAssignmentsLookupsImplementer|DepartmentCodes", GetNewDepartmentCodes); }
		}

		CodeDescriptionPairList GetNewDepartmentCodes()
		{
			var departmentCodes = new CodeDescriptionPairList();

			var filter = new ZQuery(GlbDepartmentSchema.GE_IsActive, ZBool.True);
			var departments = new GlbDepartmentCollection(new BusinessObjectFactory(), filter);
			departments.ApplySort(GlbDepartmentSchema.GE_Code.Name, ListSortDirection.Ascending);

			departmentCodes.AddPair(AllServices, Res.GetString("a12f466b-41ab-469f-b393-84777ed11afb", "All Services"));
			departmentCodes.AddPair(FreightServices, Res.GetString("cbef5ccc-529d-4972-b231-2c1b6cad551f", "Freight Services"));
			departmentCodes.AddPair(SeaFreightServices, Res.GetString("04db493a-f09e-487f-850c-37f8a0497350", "Sea Freight Services"));
			departmentCodes.AddPair(AirFreightServices, Res.GetString("bafd01b3-f1ab-4299-be23-21ebbd00bb3c", "Air Freight Services"));
			departmentCodes.AddPair(RoadFreightServices, Res.GetString("a3825424-5be7-4994-bb33-039292c2ff4f", "Road Freight Services"));
			departmentCodes.AddPair(RailFreightServices, Res.GetString("9eb33f3a-8b1b-4b5b-af44-ce5d5a24592c", "Rail Freight Services"));
			departmentCodes.AddPair(ClearanceServices, Res.GetString("0ea1fc89-a1b0-4d63-9b3e-640166e645fe", "Clearance Services"));
			departmentCodes.AddPair(CFSServices, Res.GetString("fb2b3843-e6de-45e0-b129-c90c1ad2dcef", "CFS Services"));
			departmentCodes.AddPair(WarehouseServices, Res.GetString("09337bee-db09-44ca-aecc-4fc979fe7b27", "Contract Warehousing Services"));
			departmentCodes.AddPair(ContainerYardServices, Res.GetString("db2f951a-422f-447f-be44-c617665c1a05", "Container Yard Services"));
			departmentCodes.AddPair(LocalCartageServices, Res.GetString("ea79773e-f0d6-4dca-aa1e-5e94c37cfb17", "Port Transport Services"));

			foreach (GlbDepartment department in departments)
			{
				departmentCodes.AddPair(department.GE_Code, department.GE_DescMultilingual);
			}

			return departmentCodes;
		}

		#endregion
	}
}
