using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgStaffAssignmentsCollection : DependentBusinessObjectCollection<OrgStaffAssignments, BusinessObject>
	{
		public OrgStaffAssignmentsCollection(OrgHeader organisation)
			: base(organisation)
		{
		}

		public OrgStaffAssignmentsCollection(OrgHeader organisation, GlbCompany company)
			: base(organisation)
		{
			fCompany = company;
		}

		#region Staff Roles

		#region Overall Account Manager

		public ZString OverallAccountManager
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.AccountManager, OrgStaffAssignmentsLookups.AllServices); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.AccountManager, value, OrgStaffAssignmentsLookups.AllServices); }
		}

		public GlbStaff OverallAccountManagerStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, OverallAccountManager); }
		}

		#endregion

		#region OverallRepSales

		public ZString OverallSalesRep
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, OrgStaffAssignmentsLookups.AllServices); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, value, OrgStaffAssignmentsLookups.AllServices); }
		}

		public GlbStaff OverallSalesRepStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, OverallSalesRep); }
		}

		#endregion

		#region OverallCartageCoordinator

		public ZString OverallCartageCoordinator
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.CartageCoordinator, OrgStaffAssignmentsLookups.AllServices); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.CartageCoordinator, value, OrgStaffAssignmentsLookups.AllServices); }
		}

		public GlbStaff OverallCartageCoordinatorStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, OverallCartageCoordinator); }
		}

		#endregion

		#region OverallCustomerServiceRep

		public ZString OverallCustomerServiceRep
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.CustomerServiceRep, OrgStaffAssignmentsLookups.AllServices); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.CustomerServiceRep, value, OrgStaffAssignmentsLookups.AllServices); }
		}

		public GlbStaff OverallCustomerServiceRepStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, OverallCustomerServiceRep); }
		}

		#endregion

		#region OverallController

		public ZString OverallController
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.Controller, OrgStaffAssignmentsLookups.AllServices); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.Controller, value, OrgStaffAssignmentsLookups.AllServices); }
		}

		public GlbStaff OverallControllerStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, OverallController); }
		}

		#endregion

		#region ImportSeaRep

		public ZString ImportSeaRep
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, Direction.Import, AirSea.Sea); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, value, Direction.Import, AirSea.Sea); }
		}

		public GlbStaff ImportSeaRepStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ImportSeaRep); }
		}

		#endregion

		#region ImportAirRep

		public ZString ImportAirRep
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, Direction.Import, AirSea.Air); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, value, Direction.Import, AirSea.Air); }
		}

		public GlbStaff ImportAirRepStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ImportAirRep); }
		}

		#endregion

		#region ImportRoadRep

		public ZString ImportRoadRep
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, Direction.Import, AirSea.Road); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, value, Direction.Import, AirSea.Road); }
		}

		public GlbStaff ImportRoadRepStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ImportRoadRep); }
		}

		#endregion

		#region ImportRailRep

		public ZString ImportRailRep
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, Direction.Import, AirSea.Rail); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, value, Direction.Import, AirSea.Rail); }
		}

		public GlbStaff ImportRailRepStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ImportRailRep); }
		}

		#endregion

		#region ExportSeaRep

		public ZString ExportSeaRep
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, Direction.Export, AirSea.Sea); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, value, Direction.Export, AirSea.Sea); }
		}

		public GlbStaff ExportSeaRepStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ExportSeaRep); }
		}

		#endregion

		#region ExportAirRep

		public ZString ExportAirRep
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, Direction.Export, AirSea.Air); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, value, Direction.Export, AirSea.Air); }
		}

		public GlbStaff ExportAirRepStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ExportAirRep); }
		}

		#endregion

		#region ExportRoadRep

		public ZString ExportRoadRep
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, Direction.Export, AirSea.Road); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, value, Direction.Export, AirSea.Road); }
		}

		public GlbStaff ExportRoadRepStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ExportRoadRep); }
		}

		#endregion

		#region ExportRailRep

		public ZString ExportRailRep
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, Direction.Export, AirSea.Rail); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, value, Direction.Export, AirSea.Rail); }
		}

		public GlbStaff ExportRailRepStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ExportRailRep); }
		}

		#endregion

		#region DomesticSeaRep

		public ZString DomesticSeaRep
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, Direction.Domestic, AirSea.Sea); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, value, Direction.Domestic, AirSea.Sea); }
		}

		public GlbStaff DomesticSeaRepStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, DomesticSeaRep); }
		}

		#endregion

		#region DomesticAirRep

		public ZString DomesticAirRep
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, Direction.Domestic, AirSea.Air); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, value, Direction.Domestic, AirSea.Air); }
		}

		public GlbStaff DomesticAirRepStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, DomesticAirRep); }
		}

		#endregion

		#region DomesticRoadRep

		public ZString DomesticRoadRep
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, Direction.Domestic, AirSea.Road); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, value, Direction.Domestic, AirSea.Road); }
		}

		public GlbStaff DomesticRoadRepStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, DomesticRoadRep); }
		}

		#endregion

		#region DomesticRailRep

		public ZString DomesticRailRep
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, Direction.Domestic, AirSea.Rail); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, value, Direction.Domestic, AirSea.Rail); }
		}

		public GlbStaff DomesticRailRepStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, DomesticRailRep); }
		}

		#endregion

		#region WarehousingRep

		public ZString WarehousingRep
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, OrgStaffAssignmentsLookups.WarehouseServices); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.SalesRep, value, OrgStaffAssignmentsLookups.WarehouseServices); }
		}

		public GlbStaff WarehousingRepStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, WarehousingRep); }
		}

		#endregion

		#region ExportSeaCartageCordinator

		public ZString ExportSeaCartageCordinator
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.CartageCoordinator, Direction.Export, AirSea.Sea); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.CartageCoordinator, value, Direction.Export, AirSea.Sea); }
		}

		public GlbStaff ExportSeaCartageCordinatorStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ExportSeaCartageCordinator); }
		}

		#endregion

		#region ExportAirCartageCordinator

		public ZString ExportAirCartageCordinator
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.CartageCoordinator, Direction.Export, AirSea.Air); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.CartageCoordinator, value, Direction.Export, AirSea.Air); }
		}

		public GlbStaff ExportAirCartageCordinatorStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ExportAirCartageCordinator); }
		}

		#endregion

		#region ImportSeaCartageCordinator

		public ZString ImportSeaCartageCordinator
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.CartageCoordinator, Direction.Import, AirSea.Sea); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.CartageCoordinator, value, Direction.Import, AirSea.Sea); }
		}

		public GlbStaff ImportSeaCartageCordinatorStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ImportSeaCartageCordinator); }
		}

		#endregion

		#region ImportAirCartageCordinator

		public ZString ImportAirCartageCordinator
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.CartageCoordinator, Direction.Import, AirSea.Air); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.CartageCoordinator, value, Direction.Import, AirSea.Air); }
		}

		public GlbStaff ImportAirCartageCordinatorStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ImportAirCartageCordinator); }
		}

		#endregion

		#region ExportSeaCustomerServiceRep

		public ZString ExportSeaCustomerServiceRep
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.CustomerServiceRep, Direction.Export, AirSea.Sea); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.CustomerServiceRep, value, Direction.Export, AirSea.Sea); }
		}

		public GlbStaff ExportSeaCustomerServiceRepStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ExportSeaCustomerServiceRep); }
		}

		#endregion

		#region ExportAirCustomerServiceRep

		public ZString ExportAirCustomerServiceRep
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.CustomerServiceRep, Direction.Export, AirSea.Air); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.CustomerServiceRep, value, Direction.Export, AirSea.Air); }
		}

		public GlbStaff ExportAirCustomerServiceRepStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ExportAirCustomerServiceRep); }
		}

		#endregion

		#region ImportSeaCustomerServiceRep

		public ZString ImportSeaCustomerServiceRep
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.CustomerServiceRep, Direction.Import, AirSea.Sea); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.CustomerServiceRep, value, Direction.Import, AirSea.Sea); }
		}

		public GlbStaff ImportSeaCustomerServiceRepStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ImportSeaCustomerServiceRep); }
		}

		#endregion

		#region ImportAirCustomerServiceRep

		public ZString ImportAirCustomerServiceRep
		{
			get { return GetStaffAssignment(StaffAssignmentRoles.Codes.CustomerServiceRep, Direction.Import, AirSea.Air); }
			set { SetStaffAssignment(StaffAssignmentRoles.Codes.CustomerServiceRep, value, Direction.Import, AirSea.Air); }
		}

		public GlbStaff ImportAirCustomerServiceRepStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ImportAirCustomerServiceRep); }
		}

		#endregion

		#region Storage and Retrieval

		public enum AirSea
		{
			None,
			Air,
			Sea,
			Road,
			Rail,
			Post
		}

		public enum Direction
		{
			None,
			Import,
			Export,
			Domestic
		}

		public ZString GetStaffAssignment(ZString role, Direction direction, AirSea mode)
		{
			ZString result = GetStaffAssignment(role, GetDepartmentCode(direction, mode));

			if (result.IsEmpty && direction != Direction.None)
			{
				result = GetStaffAssignment(role, GetDepartmentCode(Direction.None, mode));
			}

			if (result.IsEmpty && mode != AirSea.None)
			{
				result = GetStaffAssignment(role, OrgStaffAssignmentsLookups.AllServices);
			}

			return result;
		}

		public ZString GetStaffAssignment(ZString role, ZString departmentCode)
		{
			foreach (OrgStaffAssignments assignment in this)
			{
				if (assignment.O8_Role == role && assignment.O8_Department == departmentCode && assignment.O8_GC == Company.PK && assignment.O8_Product.IsEmpty)
				{
					return assignment.O8_GS_NKPersonResponsible;
				}
			}

			foreach (OrgStaffAssignments assignment in this)
			{
				if (assignment.O8_Role == role && assignment.O8_Department == departmentCode && assignment.O8_GC.IsEmpty && assignment.O8_Product.IsEmpty)
				{
					return assignment.O8_GS_NKPersonResponsible;
				}
			}

			return ZString.Empty;
		}

		public ZString GetStaffAssignment(ZString role, ZString departmentCode, ZString productCode)
		{
			foreach (OrgStaffAssignments assignment in this)
			{
				if (assignment.O8_Role == role && assignment.O8_Department == departmentCode && assignment.O8_GC == Company.PK && assignment.O8_Product == productCode)
				{
					return assignment.O8_GS_NKPersonResponsible;
				}
			}

			foreach (OrgStaffAssignments assignment in this)
			{
				if (assignment.O8_Role == role && assignment.O8_Department == departmentCode && assignment.O8_GC.IsEmpty && assignment.O8_Product == productCode)
				{
					return assignment.O8_GS_NKPersonResponsible;
				}
			}

			return GetStaffAssignment(role, departmentCode);
		}

		public void SetStaffAssignment(ZString role, ZString staffCode, Direction direction, AirSea mode)
		{
			SetStaffAssignment(role, staffCode, GetDepartmentCode(direction, mode));
		}

		public void SetStaffAssignment(ZString role, ZString staffCode, ZString departmentCode)
		{
			if (staffCode.IsEmpty)
			{
				DeleteStaffAssignment(role, departmentCode);
			}
			else
			{
				ZQuery filter = new ZQuery(OrgStaffAssignmentsSchema.O8_Role, role);
				filter.AddToFilter(OrgStaffAssignmentsSchema.O8_Department, departmentCode);
				BusinessObject[] results = Find(filter);

				OrgStaffAssignments assignment = null;
				if (results.Length > 0)
				{
					assignment = (OrgStaffAssignments)results[0];
				}
				else
				{
					assignment = AddNew();
					assignment.O8_Role = role;
					assignment.O8_Department = departmentCode;
				}

				assignment.O8_GS_NKPersonResponsible = staffCode;
			}
		}

		void DeleteStaffAssignment(ZString role, ZString department)
		{
			for (int i = Count - 1; i >= 0; i--)
			{
				if (this[i].O8_Role == role && this[i].O8_Department == department)
				{
					this[i].Delete();
					break;
				}
			}
		}

		ZString GetDepartmentCode(Direction direction, AirSea mode)
		{
			var result = ZString.Empty;

			if (direction == Direction.None)
			{
				if (mode == AirSea.Air)
				{
					result = OrgStaffAssignmentsLookups.AirFreightServices;
				}
				else if (mode == AirSea.Sea)
				{
					result = OrgStaffAssignmentsLookups.SeaFreightServices;
				}
				else if (mode == AirSea.Road)
				{
					result = OrgStaffAssignmentsLookups.RoadFreightServices;
				}
				else if (mode == AirSea.Rail)
				{
					result = OrgStaffAssignmentsLookups.RailFreightServices;
				}
			}
			else
			{
				var departmentFilter = new ZQuery();
				departmentFilter.AddToFilter(GlbDepartmentSchema.GE_IsActive, ZBool.True);

				if (mode == AirSea.Air)
				{
					departmentFilter.AddToFilter(GlbDepartmentSchema.GE_Air, ZBool.True);
				}
				else if (mode == AirSea.Sea)
				{
					departmentFilter.AddToFilter(GlbDepartmentSchema.GE_Sea, ZBool.True);
				}
				else if (mode == AirSea.Road)
				{
					departmentFilter.AddToFilter(GlbDepartmentSchema.GE_Road, ZBool.True);
				}
				else if (mode == AirSea.Rail)
				{
					departmentFilter.AddToFilter(GlbDepartmentSchema.GE_Rail, ZBool.True);
				}
				else if (mode == AirSea.Post)
				{
					departmentFilter.AddToFilter(GlbDepartmentSchema.GE_Post, ZBool.True);
				}

				if (direction == Direction.Import)
				{
					departmentFilter.AddToFilter(GlbDepartmentSchema.GE_Import, ZBool.True);
					departmentFilter.AddToFilter(GlbDepartmentSchema.GE_InternationalFreight, ZBool.True);
				}
				else if (direction == Direction.Export)
				{
					departmentFilter.AddToFilter(GlbDepartmentSchema.GE_Export, ZBool.True);
					departmentFilter.AddToFilter(GlbDepartmentSchema.GE_InternationalFreight, ZBool.True);
				}
				else if (direction == Direction.Domestic)
				{
					departmentFilter.AddToFilter(GlbDepartmentSchema.GE_Domestic, ZBool.True);
					departmentFilter.AddToFilter(GlbDepartmentSchema.GE_InternationalFreight, ZBool.False);
				}

				GlbDepartment dept = null;
				var departments = Factory.Load<GlbDepartment>(departmentFilter);
				foreach (GlbDepartment department in departments)
				{
					if (!department.IsGatewayDepartment && !department.IsCartageDepartment)
					{
						dept = department;
						break;
					}
				}

				if (dept != null)
				{
					result = dept.GE_Code;
				}
			}

			return result;
		}

		#endregion

		#endregion

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var assignment = (OrgStaffAssignments)child;

			if (Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.IsAllowed && OrganisationsDataRegistry.Instance.NewStaffAssignmentsAsGlobal.Value)
			{
				assignment.O8_GC = ZGuid.Empty;
			}
			else
			{
				assignment.O8_GC = Company.PK;
			}
		}

		#endregion

		#region Filtering

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			if (CompanySpecific)
			{
				ZQuery companyFilter = new ZQuery(OrgStaffAssignmentsSchema.O8_GC, Company.PK);
				companyFilter.AddToFilter(JoinCondition.Or, OrgStaffAssignmentsSchema.O8_GC, null);
				query.AddToFilter(companyFilter);
			}
			return query;
		}

		public bool CompanySpecific
		{
			get { return companySpecific; }
			set
			{
				companySpecific = value;
				Load();
			}
		}
		bool companySpecific = true;

		#endregion

		#region Overall Rep Security

		public bool AllowDeleteOfOverallRepRegardlessOfSecurity
		{
			get { return fAllowDeleteOfOverallRepRegardlessOfSecurity; }
			set { fAllowDeleteOfOverallRepRegardlessOfSecurity = value; }
		}

		bool fAllowDeleteOfOverallRepRegardlessOfSecurity;

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			var toDelete = (OrgStaffAssignments)elementToDelete;

			if (!toDelete.IsOverallRepWithDeniedModifySecurity || AllowDeleteOfOverallRepRegardlessOfSecurity)
			{
				if (!Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.IsAllowed && (toDelete.O8_GC.IsEmpty || toDelete.O8_GC != GlbCompany.CurrentCompany.PK))
				{
					Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.ShowError();
				}
				else
				{
					base.RemoveAndDelete(elementToDelete);
				}
			}
		}

		#endregion

		#region Implementation

		GlbCompany Company
		{
			get
			{
				if (fCompany == null)
				{
					fCompany = GlbCompany.CurrentCompany;
				}
				return fCompany;
			}
		}

		GlbCompany fCompany;

		#endregion
	}
}
