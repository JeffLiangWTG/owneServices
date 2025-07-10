using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;

namespace Enterprise.Freight.Confirmations.Module.Testing
{
	public class ConsolidatedTransportBookingCRMSecurityProviderTest : CRMSecurityProviderTest<CommonConsolidatedTransportBooking>
	{
		protected override CRMSecurityProvider<CommonConsolidatedTransportBooking> GetNewProviderForTest() => new ConsolidatedTransportBookingCRMSecurityProvider();

		protected override IEnumerable<CommonConsolidatedTransportBooking> GetTestObjectWithoutStaffAssignment()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNewMainAddress();

			var booking = Factory.NewWithValidTestData<CommonConsolidatedTransportBooking>();
			booking.D1_OA_Customer = address.PK;
			return new CommonConsolidatedTransportBooking[] { booking };
		}

		protected override IEnumerable<CommonConsolidatedTransportBooking> GetTestObjectWithOrgStaffAssignment()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			var staffAssignments = org1.StaffAssignments.AddNew();
			staffAssignments.O8_Role = "SAL";
			staffAssignments.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			var address = org1.Addresses.AddNewMainAddress();

			var booking = Factory.NewWithValidTestData<CommonConsolidatedTransportBooking>();
			booking.D1_OA_Customer = address.PK;

			return new CommonConsolidatedTransportBooking[] { booking };
		}

		protected override IEnumerable<CommonConsolidatedTransportBooking> GetTestObjectWithBizObjStaffAssignment() => Enumerable.Empty<CommonConsolidatedTransportBooking>();

		protected override IEnumerable<CommonConsolidatedTransportBooking> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var booking = Factory.NewWithValidTestData<CommonConsolidatedTransportBooking>();
			booking.D1_OA_Customer = org.Addresses[0].PK;

			return new CommonConsolidatedTransportBooking[] { booking };
		}

		protected override IEnumerable<CommonConsolidatedTransportBooking> GetTestObjectsWithoutOrg()
		{
			var booking = Factory.NewWithValidTestData<CommonConsolidatedTransportBooking>();
			return new CommonConsolidatedTransportBooking[] { booking };
		}

		protected override void AddStaffAssignmentForCompany(CommonConsolidatedTransportBooking obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var assignment = obj.Customer.Header.StaffAssignments.AddNew();
			assignment.O8_Role = role;
			assignment.O8_GS_NKPersonResponsible = staffCode;
			assignment.O8_GC = companyPk;
		}
	}
}
