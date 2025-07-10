using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal class AgencyBookingCRMSecurityProviderTest : CRMSecurityProviderTest<AgencyBooking>
	{
		protected override CRMSecurityProvider<AgencyBooking> GetNewProviderForTest() => new AgencyBookingCRMSecurityProvider();
		protected override IEnumerable<AgencyBooking> GetTestObjectWithoutStaffAssignment()
		{
			var booking = Factory.NewWithValidTestData<AgencyBooking>();
			return new AgencyBooking[] { booking };
		}

		protected override IEnumerable<AgencyBooking> GetTestObjectWithOrgStaffAssignment()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			var staffAssignments = org1.StaffAssignments.AddNew();
			staffAssignments.O8_Role = "SAL";
			staffAssignments.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			_ = org1.Addresses.AddNewMainAddress();
			var booking = Factory.NewWithValidTestData<AgencyBooking>();
			booking.BookingPartyDocumentaryAddress.E2_OA_Address = org1.Addresses[0].PK;
			return new AgencyBooking[] { booking };
		}

		protected override IEnumerable<AgencyBooking> GetTestObjectWithBizObjStaffAssignment() => Enumerable.Empty<AgencyBooking>();
		protected override IEnumerable<AgencyBooking> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var booking = Factory.NewWithValidTestData<AgencyBooking>();
			booking.BookingPartyDocumentaryAddress.E2_OA_Address = org.Addresses[0].PK;
			return new AgencyBooking[] { booking };
		}

		protected override void AddStaffAssignmentForCompany(AgencyBooking obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var assignment = obj.BookingPartyDocumentaryAddress.Organisation.StaffAssignments.AddNew();
			assignment.O8_Role = role;
			assignment.O8_GS_NKPersonResponsible = staffCode;
			assignment.O8_GC = companyPk;
		}
	}
}
