using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.TransportBookings.Module.Testing
{
	public class DtbBookingCRMSecurityProviderTest : CRMSecurityProviderTest<DtbBooking>
	{
		protected override CRMSecurityProvider<DtbBooking> GetNewProviderForTest() => new DtbBookingCRMSecurityProvider();

		protected override IEnumerable<DtbBooking> GetTestObjectWithTaskAssignment()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var capability = GlbStaff.CurrentUser.Capabilities.AddNew();
			capability.G4_Code = "CP1";
			capability.Factory.Save();

			var booking = Factory.NewWithValidTestData<DtbBooking>();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_OA_LocalChargesAddr = org.Addresses[0].PK;
			jobHeader.JH_GS_NKRepSales = "U00";
			jobHeader.JH_ParentID = booking.PK;
			jobHeader.JH_ParentTableCode = DtbBookingSchema.Constants.Prefix;

			var workflowItem1 = GetWorkflowProvider(booking).WorkflowItems.AddNew();
			workflowItem1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			var booking2 = Factory.NewWithValidTestData<DtbBooking>();
			var jobHeader2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader2.JH_OA_LocalChargesAddr = org.Addresses[0].PK;
			jobHeader2.JH_GS_NKRepSales = "U00";
			jobHeader2.JH_ParentID = booking2.PK;
			jobHeader2.JH_ParentTableCode = DtbBookingSchema.Constants.Prefix;

			var workflowItem2 = GetWorkflowProvider(booking2).WorkflowItems.AddNew();
			workflowItem2.P9_G4_RequiredCapability = capability.PK;

			return new DtbBooking[] { booking, booking2 };
		}

		protected override IEnumerable<DtbBooking> GetTestObjectWithoutStaffAssignment()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var booking = Factory.NewWithValidTestData<DtbBooking>();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_OA_LocalChargesAddr = org.Addresses[0].PK;
			jobHeader.JH_GS_NKRepSales = "U00";
			jobHeader.JH_ParentID = booking.PK;
			jobHeader.JH_ParentTableCode = DtbBookingSchema.Constants.Prefix;

			return new DtbBooking[] { booking };
		}

		protected override IEnumerable<DtbBooking> GetTestObjectWithOrgStaffAssignment()
		{
			var booking = Factory.NewWithValidTestData<DtbBooking>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			var staffAssignments = org1.StaffAssignments.AddNew();
			staffAssignments.O8_Role = "SAL";
			staffAssignments.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			var address = org1.Addresses.AddNewMainAddress();

			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_OA_LocalChargesAddr = address.PK;
			jobHeader1.JH_GS_NKRepSales = "U00";

			jobHeader1.JH_ParentID = booking.PK;
			jobHeader1.JH_ParentTableCode = DtbBookingSchema.Constants.Prefix;

			var shipment = Factory.New<IForwardingShipment>();
			var jobHeader2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader2.JH_OA_LocalChargesAddr = address.PK;
			jobHeader2.JH_GS_NKRepSales = "U00";

			jobHeader2.JH_ParentID = shipment.PK;
			jobHeader2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			var consol = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consol.KB_ParentID = shipment.PK;
			consol.KB_ParentTableCode = "JS";
			var booking2 = Factory.NewWithValidTestData<DtbBooking>();
			booking2.KM_KB_Booking = consol.PK;

			return new DtbBooking[] { booking };
		}

		protected override IEnumerable<DtbBooking> GetTestObjectWithBizObjStaffAssignment()
		{
			var booking = Factory.NewWithValidTestData<DtbBooking>();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			var address = org1.Addresses.AddNewMainAddress();

			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_OA_LocalChargesAddr = address.PK;
			jobHeader1.JH_GS_NKRepSales = "U00";
			jobHeader1.JH_ParentID = booking.PK;
			jobHeader1.JH_ParentTableCode = DtbBookingSchema.Constants.Prefix;

			var shipment = Factory.New<IForwardingShipment>();
			var jobHeader2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader2.JH_OA_LocalChargesAddr = address.PK;
			jobHeader2.JH_GS_NKRepSales = "U00";
			jobHeader2.JH_ParentID = shipment.PK;
			jobHeader2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var consol = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consol.KB_ParentID = shipment.PK;
			consol.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking2 = Factory.NewWithValidTestData<DtbBooking>();
			booking2.KM_KB_Booking = consol.PK;

			return new DtbBooking[] { booking, booking2 };
		}

		protected override IEnumerable<DtbBooking> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var booking1 = Factory.NewWithValidTestData<DtbBooking>();

			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_OA_LocalChargesAddr = org.Addresses[0].PK;
			jobHeader1.JH_GS_NKRepSales = "U00";

			jobHeader1.JH_ParentID = booking1.PK;
			jobHeader1.JH_ParentTableCode = DtbBookingSchema.Constants.Prefix;

			var shipment = Factory.New<IForwardingShipment>();
			var jobHeader2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader2.JH_OA_LocalChargesAddr = org.Addresses[0].PK;
			jobHeader2.JH_GS_NKRepSales = "U00";

			jobHeader2.JH_ParentID = shipment.PK;
			jobHeader2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			var consol = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consol.KB_ParentID = shipment.PK;
			consol.KB_ParentTableCode = "JS";
			var booking2 = Factory.NewWithValidTestData<DtbBooking>();
			booking2.KM_KB_Booking = consol.PK;

			return new DtbBooking[] { booking1, booking2 };
		}

		protected override void AddStaffAssignmentForCompany(DtbBooking obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var job = obj.Factory.LoadTop1<JobHeader>(new CargoWise.EntityFramework.ZQuery(JobHeaderSchema.JH_ParentID, obj.PK));
			var assignment = job.LocalChargesAddr.Header.StaffAssignments.AddNew();
			assignment.O8_Role = role;
			assignment.O8_GS_NKPersonResponsible = staffCode;
			assignment.O8_GC = companyPk;
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAppendAdditionalJobHeaderQuery()
		{
			var orgNoGroup = Factory.NewWithValidTestData<OrgHeader>();
			orgNoGroup.MiscServ.OM_GG_OrgSecurityGroup = ZGuid.Empty;

			var orgOsmg = Factory.NewWithValidTestData<OrgHeader>();
			orgOsmg.MiscServ.OM_GG_OrgSecurityGroup = OSMG.PK;

			var orgNonOsmg = Factory.NewWithValidTestData<OrgHeader>();
			orgNonOsmg.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;

			//TB00000001 - Stand-alone booking, no header => granted
			var tb01 = Factory.NewWithValidTestData<DtbBooking>();
			tb01.KM_JobID = "TB00000001";

			//TB00000002 - Stand-alone booking, header with no local client  => granted
			var tb02 = Factory.NewWithValidTestData<DtbBooking>();
			tb02.KM_JobID = "TB00000002";
			var jobHeaderTb02 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeaderTb02.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			jobHeaderTb02.JH_ParentID = tb02.PK;
			jobHeaderTb02.JH_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			jobHeaderTb02.JH_JobNum = tb02.KM_JobID;
			jobHeaderTb02.JH_OA_LocalChargesAddr = ZGuid.Empty;

			//TB00000003 - Stand-alone booking, header with local client with no security group => denied
			var tb03 = Factory.NewWithValidTestData<DtbBooking>();
			tb03.KM_JobID = "TB00000003";
			var jobHeaderTb03 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeaderTb03.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			jobHeaderTb03.JH_ParentID = tb03.PK;
			jobHeaderTb03.JH_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			jobHeaderTb03.JH_JobNum = tb03.KM_JobID;
			jobHeaderTb03.JH_OA_LocalChargesAddr = orgNoGroup.Addresses[0].PK;

			//TB00000004 - Stand-alone booking, header with local client with accessible security group => granted
			var tb04 = Factory.NewWithValidTestData<DtbBooking>();
			tb04.KM_JobID = "TB00000004";
			var jobHeaderTb04 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeaderTb04.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			jobHeaderTb04.JH_ParentID = tb04.PK;
			jobHeaderTb04.JH_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			jobHeaderTb04.JH_JobNum = tb04.KM_JobID;
			jobHeaderTb04.JH_OA_LocalChargesAddr = orgOsmg.Addresses[0].PK;

			//TB00000005 - Stand-alone booking, header with local client with inaccessible security group => denied
			var tb05 = Factory.NewWithValidTestData<DtbBooking>();
			tb05.KM_JobID = "TB00000005";
			var jobHeaderTb05 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeaderTb05.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			jobHeaderTb05.JH_ParentID = tb05.PK;
			jobHeaderTb05.JH_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			jobHeaderTb05.JH_JobNum = tb05.KM_JobID;
			jobHeaderTb05.JH_OA_LocalChargesAddr = orgNonOsmg.Addresses[0].PK;

			//TB00000006 (S00000006)- Booking linked to a shipment, no header => granted
			var tb06 = Factory.NewWithValidTestData<DtbBooking>();
			tb06.KM_JobID = "TB00000006";
			var shipmentTb06 = Factory.New<IForwardingShipment>();
			shipmentTb06.JS_UniqueConsignRef = "S00000006";
			var consolidationTb06 = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consolidationTb06.KB_JobID = "CM00000006";
			consolidationTb06.KB_ParentID = shipmentTb06.PK;
			consolidationTb06.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			tb06.KM_KB_Booking = consolidationTb06.PK;

			//TB00000007 (S00000007)- Booking linked to a shipment, header with no local client => granted
			var tb07 = Factory.NewWithValidTestData<DtbBooking>();
			tb07.KM_JobID = "TB00000007";
			var shipmentTb07 = Factory.New<IForwardingShipment>();
			shipmentTb07.JS_UniqueConsignRef = "S00000007";
			var consolidationTb07 = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consolidationTb07.KB_JobID = "CM00000007";
			consolidationTb07.KB_ParentID = shipmentTb07.PK;
			consolidationTb07.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			tb07.KM_KB_Booking = consolidationTb07.PK;

			var jobHeaderTb07 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeaderTb07.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			jobHeaderTb07.JH_ParentID = shipmentTb07.PK;
			jobHeaderTb07.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderTb07.JH_JobNum = shipmentTb07.JS_UniqueConsignRef;
			jobHeaderTb07.JH_OA_LocalChargesAddr = ZGuid.Empty;

			//TB00000008 (S00000008)- Booking linked to a shipment, header with local client with no security group => denied
			var tb08 = Factory.NewWithValidTestData<DtbBooking>();
			tb08.KM_JobID = "TB00000008";
			var shipmentTb08 = Factory.New<IForwardingShipment>();
			shipmentTb08.JS_UniqueConsignRef = "S00000008";
			var consolidationTb08 = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consolidationTb08.KB_JobID = "CM00000008";
			consolidationTb08.KB_ParentID = shipmentTb08.PK;
			consolidationTb08.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			tb08.KM_KB_Booking = consolidationTb08.PK;

			var jobHeaderTb08 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeaderTb08.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			jobHeaderTb08.JH_ParentID = shipmentTb08.PK;
			jobHeaderTb08.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderTb08.JH_JobNum = shipmentTb08.JS_UniqueConsignRef;
			jobHeaderTb08.JH_OA_LocalChargesAddr = orgNoGroup.Addresses[0].PK;

			//TB00000009 (S00000009)- Booking linked to a shipment, header with local client with accessible security group => granted
			var tb09 = Factory.NewWithValidTestData<DtbBooking>();
			tb09.KM_JobID = "TB00000009";
			var shipmentTb09 = Factory.New<IForwardingShipment>();
			shipmentTb09.JS_UniqueConsignRef = "S00000009";
			var consolidationTb09 = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consolidationTb09.KB_JobID = "CM00000009";
			consolidationTb09.KB_ParentID = shipmentTb09.PK;
			consolidationTb09.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			tb09.KM_KB_Booking = consolidationTb09.PK;

			var jobHeaderTb09 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeaderTb09.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			jobHeaderTb09.JH_ParentID = shipmentTb09.PK;
			jobHeaderTb09.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderTb09.JH_JobNum = shipmentTb09.JS_UniqueConsignRef;
			jobHeaderTb09.JH_OA_LocalChargesAddr = orgOsmg.Addresses[0].PK;

			//TB00000010 (S00000010)- Booking linked to a shipment, header with local client with inaccessible security group => denied
			var tb10 = Factory.NewWithValidTestData<DtbBooking>();
			tb10.KM_JobID = "TB00000010";
			var shipmentTb10 = Factory.New<IForwardingShipment>();
			shipmentTb10.JS_UniqueConsignRef = "S00000010";
			var consolidationTb10 = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consolidationTb10.KB_JobID = "CM00000010";
			consolidationTb10.KB_ParentID = shipmentTb10.PK;
			consolidationTb10.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			tb10.KM_KB_Booking = consolidationTb10.PK;

			var jobHeaderTb10 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeaderTb10.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			jobHeaderTb10.JH_ParentID = shipmentTb10.PK;
			jobHeaderTb10.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderTb10.JH_JobNum = shipmentTb10.JS_UniqueConsignRef;
			jobHeaderTb10.JH_OA_LocalChargesAddr = orgNonOsmg.Addresses[0].PK;

			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();

			//TB00000011 - Stand-alone booking, header for another company  => granted
			var tb11 = Factory.NewWithValidTestData<DtbBooking>();
			tb11.KM_JobID = "TB00000011";
			var jobHeaderTb11 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeaderTb11.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			jobHeaderTb11.JH_GC = anotherCompany.PK;
			jobHeaderTb11.JH_ParentID = tb11.PK;
			jobHeaderTb11.JH_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			jobHeaderTb11.JH_JobNum = tb11.KM_JobID;
			jobHeaderTb11.JH_OA_LocalChargesAddr = ZGuid.Empty;

			//TB00000012 - Booking linked to a shipment, header for another company => granted
			var tb12 = Factory.NewWithValidTestData<DtbBooking>();
			tb12.KM_JobID = "TB00000012";
			var shipmentTb12 = Factory.New<IForwardingShipment>();
			shipmentTb12.JS_UniqueConsignRef = "S00000012";
			var consolidationTb12 = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consolidationTb12.KB_JobID = "CM00000012";
			consolidationTb12.KB_ParentID = shipmentTb12.PK;
			consolidationTb12.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			tb12.KM_KB_Booking = consolidationTb12.PK;

			var jobHeaderTb12 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeaderTb12.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			jobHeaderTb12.JH_GC = anotherCompany.PK;
			jobHeaderTb12.JH_ParentID = shipmentTb12.PK;
			jobHeaderTb12.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderTb12.JH_JobNum = shipmentTb12.JS_UniqueConsignRef;
			jobHeaderTb12.JH_OA_LocalChargesAddr = ZGuid.Empty;

			//TB00000013 - Booking linked to a consolidation => granted
			var tb13 = Factory.NewWithValidTestData<DtbBooking>();
			tb13.KM_JobID = "TB00000013";

			var forwardingConsol = Factory.New<IForwardingConsol>();
			var consolidationTb13 = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consolidationTb13.KB_JobID = "CM00000013";
			consolidationTb13.KB_ParentID = forwardingConsol.PK;
			consolidationTb13.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			tb13.KM_KB_Booking = consolidationTb13.PK;
			Factory.Save();

			ProviderForTest.CRMSecurity.IgnoreOSMG.IsAllowed = false;
			ProviderForTest.CRMSecurity.IgnoreTaskAssignment.IsAllowed = true;
			ProviderForTest.CRMSecurity.ViewByStaffNotAssigned.IsAllowed = true;

			var filters = new ModuleFilterCollection();
			ProviderForTest.AddCRMSecurityFilterStrips(Factory, filters);

			var filter = filters["Org. Security Group Security"] as ModuleNkFilter;
			var bookings = Factory.Load<DtbBooking>(SetupCRMSecurityFilterStripsQuery(filter.Query));

			var granted = new List<DtbBooking> { tb01, tb02, tb04, tb06, tb07, tb09, tb11, tb12, tb13 };
			var denied = new List<DtbBooking> { tb03, tb05, tb08, tb10 };

			AssertContainsExactElementsInAnyOrder(tb => tb.KM_JobID, granted, bookings);
			AssertCollectionNotContains(denied, bookings);
		}
	}
}
