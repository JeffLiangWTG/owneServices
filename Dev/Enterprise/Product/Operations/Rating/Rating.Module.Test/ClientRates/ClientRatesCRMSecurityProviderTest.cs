using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.Module.Testing
{
	public class ClientRatesCRMSecurityProviderTest : CRMSecurityProviderTest<ClientRate>
	{
		protected override CRMSecurityProvider<ClientRate> GetNewProviderForTest() => new ClientRatesCRMSecurityProvider();

		protected override IEnumerable<ClientRate> GetTestObjectWithoutStaffAssignment()
		{
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			return new ClientRate[] { clientRate };
		}

		protected override IEnumerable<ClientRate> GetTestObjectWithOrgStaffAssignment()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			var staffAssignments = org1.StaffAssignments.AddNew();
			staffAssignments.O8_Role = "SAL";
			staffAssignments.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			_ = org1.Addresses.AddNewMainAddress();

			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			clientRate.TH_OH = org1.PK;

			return new ClientRate[] { clientRate };
		}

		protected override IEnumerable<ClientRate> GetTestObjectWithBizObjStaffAssignment() => System.Array.Empty<ClientRate>();

		protected override IEnumerable<ClientRate> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			clientRate.TH_OH = org.PK;

			return new ClientRate[] { clientRate };
		}

		protected override void AddStaffAssignmentForCompany(ClientRate obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var assignment = obj.Header.StaffAssignments.AddNew();
			assignment.O8_Role = role;
			assignment.O8_GS_NKPersonResponsible = staffCode;
			assignment.O8_GC = companyPk;
		}
	}
}
