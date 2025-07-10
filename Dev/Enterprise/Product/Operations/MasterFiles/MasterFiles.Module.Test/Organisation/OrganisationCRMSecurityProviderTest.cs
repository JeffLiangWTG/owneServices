using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrganisationCRMSecurityProviderTest : CRMSecurityProviderTest<OrgHeader>
	{
		protected override CRMSecurityProvider<OrgHeader> GetNewProviderForTest() => new OrganisationCRMSecurityProvider();

		protected override IEnumerable<OrgHeader> GetTestObjectWithoutStaffAssignment()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			return new OrgHeader[] { org };
		}

		protected override IEnumerable<OrgHeader> GetTestObjectWithOrgStaffAssignment()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			var staffAssignments = org1.StaffAssignments.AddNew();
			staffAssignments.O8_Role = "SAL";
			staffAssignments.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;

			return new OrgHeader[] { org1 };
		}

		protected override void AddStaffAssignmentForCompany(OrgHeader obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var assignment = obj.StaffAssignments.AddNew();
			assignment.O8_Role = role;
			assignment.O8_GS_NKPersonResponsible = staffCode;
			assignment.O8_GC = companyPk;
		}

		protected override IEnumerable<OrgHeader> GetTestObjectWithBizObjStaffAssignment() => Enumerable.Empty<OrgHeader>();

		protected override IEnumerable<OrgHeader> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org) => new OrgHeader[] { Factory.Load<OrgHeader>(org.PK) };

		protected override void SetUp()
		{
			var nonOSMG = Factory.NewWithValidTestData<GlbGroup>();
			Factory.Save();
			TestConnection.ExecuteNonQuery($"UPDATE dbo.OrgMiscServ SET OM_GG_OrgSecurityGroup = '{nonOSMG.PK}'; ");
			Factory.ClearQueryCache();
			base.SetUp();
		}
	}
}
