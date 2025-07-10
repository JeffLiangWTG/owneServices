using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class CommunicationCRMSecurityProviderTest : CRMSecurityProviderTest<OrgSalesCall>
	{
		protected override CRMSecurityProvider<OrgSalesCall> GetNewProviderForTest() => new CommunicationCRMSecurityProvider();

		protected override IEnumerable<OrgSalesCall> GetTestObjectWithoutStaffAssignment()
		{
			var call1 = Factory.NewWithValidTestData<OrgSalesCall>();
			call1.OQ_GS_NKSalesRep = "U00";
			return new OrgSalesCall[] { call1 };
		}

		protected override IEnumerable<OrgSalesCall> GetTestObjectWithOrgStaffAssignment()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			var staffAssignments = org1.StaffAssignments.AddNew();
			staffAssignments.O8_Role = "SAL";
			staffAssignments.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;

			var call1 = Factory.NewWithValidTestData<OrgSalesCall>();
			call1.OQ_OH = org1.PK;
			call1.OQ_GS_NKSalesRep = "U00";

			return new OrgSalesCall[] { call1 };
		}

		protected override IEnumerable<OrgSalesCall> GetTestObjectWithBizObjStaffAssignment()
		{
			var call1 = Factory.NewWithValidTestData<OrgSalesCall>();
			call1.OQ_GS_NKSalesRep = GlbStaff.CurrentUser.GS_Code;

			var call2 = Factory.NewWithValidTestData<OrgSalesCall>();
			call2.OQ_GS_NKSalesRep = "U00";
			var attendee = call2.AdditionalAttendeesStaff.AddNew();
			attendee.O6_AttendeeTableCode = "GS";
			attendee.O6_AttendeeID = GlbStaff.CurrentUser.PK;

			return new OrgSalesCall[] { call1, call2 };
		}

		protected override IEnumerable<OrgSalesCall> GetTestObjectWithTaskAssignment()
		{
			var result = base.GetTestObjectWithTaskAssignment();
			foreach (var call in result)
			{
				call.OQ_GS_NKSalesRep = "U00";
			}
			return result;
		}

		protected override IEnumerable<OrgSalesCall> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var call1 = Factory.NewWithValidTestData<OrgSalesCall>();
			call1.OQ_OH = org.PK;
			call1.OQ_GS_NKSalesRep = "U00";

			return new OrgSalesCall[] { call1 };
		}

		protected override void AddStaffAssignmentForCompany(OrgSalesCall obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var assignment = obj.Header.StaffAssignments.AddNew();
			assignment.O8_Role = role;
			assignment.O8_GS_NKPersonResponsible = staffCode;
			assignment.O8_GC = companyPk;
		}
	}
}
