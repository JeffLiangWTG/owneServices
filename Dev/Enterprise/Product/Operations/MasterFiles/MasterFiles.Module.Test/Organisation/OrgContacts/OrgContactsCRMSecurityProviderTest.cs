using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrgContactsCRMSecurityProviderTest : CRMSecurityProviderTest<OrgContact>
	{
		protected override CRMSecurityProvider<OrgContact> GetNewProviderForTest() => new OrgContactsCRMSecurityProvider();

		protected override IEnumerable<OrgContact> GetTestObjectWithoutStaffAssignment()
		{
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			return new OrgContact[] { orgContact };
		}

		protected override IEnumerable<OrgContact> GetTestObjectWithOrgStaffAssignment()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;

			var staffAssignments = org.StaffAssignments.AddNew();
			staffAssignments.O8_Role = "SAL";
			staffAssignments.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;

			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_OH = org.PK;

			return new OrgContact[] { orgContact };
		}

		protected override IEnumerable<OrgContact> GetTestObjectWithBizObjStaffAssignment()
		{
			return System.Array.Empty<OrgContact>();
		}

		protected override IEnumerable<OrgContact> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_OH = org.PK;

			return new OrgContact[] { orgContact };
		}

		protected override void AddStaffAssignmentForCompany(OrgContact obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var assignment = obj.Header.StaffAssignments.AddNew();
			assignment.O8_Role = role;
			assignment.O8_GS_NKPersonResponsible = staffCode;
			assignment.O8_GC = companyPk;
		}
	}
}
