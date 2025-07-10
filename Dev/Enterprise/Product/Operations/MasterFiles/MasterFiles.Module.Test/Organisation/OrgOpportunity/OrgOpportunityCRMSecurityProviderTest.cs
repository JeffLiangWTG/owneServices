using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrgOpportunityCRMSecurityProviderTest : CRMSecurityProviderTest<OrgOpportunity>
	{
		protected override CRMSecurityProvider<OrgOpportunity> GetNewProviderForTest() => new OrgOpportunityCRMSecurityProvider();

		protected override IEnumerable<OrgOpportunity> GetTestObjectWithoutStaffAssignment()
		{
			var orgOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			orgOpportunity.P8_GS_NKPrimarySalesPerson = "U00";
			return new OrgOpportunity[] { orgOpportunity };
		}

		protected override IEnumerable<OrgOpportunity> GetTestObjectWithOrgStaffAssignment()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			var staffAssignments = org1.StaffAssignments.AddNew();
			staffAssignments.O8_Role = "SAL";
			staffAssignments.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			_ = org1.Addresses.AddNewMainAddress();

			var orgOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			orgOpportunity.P8_OH = org1.PK;
			orgOpportunity.P8_GS_NKPrimarySalesPerson = "U00";

			return new OrgOpportunity[] { orgOpportunity };
		}

		protected override IEnumerable<OrgOpportunity> GetTestObjectWithBizObjStaffAssignment()
		{
			var orgOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			orgOpportunity.P8_GS_NKPrimarySalesPerson = GlbStaff.CurrentUser.GS_Code;
			return new OrgOpportunity[] { orgOpportunity };
		}

		protected override IEnumerable<OrgOpportunity> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var orgOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			orgOpportunity.P8_OH = org.PK;
			orgOpportunity.P8_GS_NKPrimarySalesPerson = "U00";

			return new OrgOpportunity[] { orgOpportunity };
		}

		protected override void AddStaffAssignmentForCompany(OrgOpportunity obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var assignment = obj.Header.StaffAssignments.AddNew();
			assignment.O8_Role = role;
			assignment.O8_GS_NKPersonResponsible = staffCode;
			assignment.O8_GC = companyPk;
		}
	}
}
