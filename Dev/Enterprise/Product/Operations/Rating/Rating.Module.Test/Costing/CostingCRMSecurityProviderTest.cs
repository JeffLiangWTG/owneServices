using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.Module.Testing
{
	public class CostingCRMSecurityProviderTest : CRMSecurityProviderTest<Costing>
	{
		protected override CRMSecurityProvider<Costing> GetNewProviderForTest() => new CostingCRMSecurityProvider();

		protected override IEnumerable<Costing> GetTestObjectWithoutStaffAssignment()
		{
			var costing = Factory.NewWithValidTestData<Costing>();
			return new Costing[] { costing };
		}

		protected override IEnumerable<Costing> GetTestObjectWithOrgStaffAssignment()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			var staffAssignments = org1.StaffAssignments.AddNew();
			staffAssignments.O8_Role = "SAL";
			staffAssignments.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			_ = org1.Addresses.AddNewMainAddress();

			var costing = Factory.NewWithValidTestData<Costing>();
			costing.TH_OH = org1.PK;

			return new Costing[] { costing };
		}

		protected override IEnumerable<Costing> GetTestObjectWithBizObjStaffAssignment() => System.Array.Empty<Costing>();

		protected override IEnumerable<Costing> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var costing = Factory.NewWithValidTestData<Costing>();
			costing.TH_OH = org.PK;

			return new Costing[] { costing };
		}

		protected override void AddStaffAssignmentForCompany(Costing obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var assignment = obj.Header.StaffAssignments.AddNew();
			assignment.O8_Role = role;
			assignment.O8_GS_NKPersonResponsible = staffCode;
			assignment.O8_GC = companyPk;
		}
	}
}
