using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Module.Testing
{
	public class BillOfLadingCRMSecurityProviderTest : CRMSecurityProviderTest<BillOfLading>
	{
		protected override CRMSecurityProvider<BillOfLading> GetNewProviderForTest() => new BillOfLadingCRMSecurityProvider();
		protected override IEnumerable<BillOfLading> GetTestObjectWithoutStaffAssignment()
		{
			var bill = Factory.NewWithValidTestData<BillOfLading>();
			return new BillOfLading[] { bill };
		}

		protected override IEnumerable<BillOfLading> GetTestObjectWithOrgStaffAssignment()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			var staffAssignments = org1.StaffAssignments.AddNew();
			staffAssignments.O8_Role = "SAL";
			staffAssignments.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			var address = org1.Addresses.AddNewMainAddress();
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_OA_LocalChargesAddr = address.PK;
			jobHeader1.JH_GS_NKRepSales = "U00";
			var bill = Factory.NewWithValidTestData<BillOfLading>();
			jobHeader1.JH_ParentID = bill.PK;
			jobHeader1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			return new BillOfLading[] { bill };
		}

		protected override IEnumerable<BillOfLading> GetTestObjectWithBizObjStaffAssignment() => Enumerable.Empty<BillOfLading>();
		protected override IEnumerable<BillOfLading> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_OA_LocalChargesAddr = org.Addresses[0].PK;
			jobHeader1.JH_GS_NKRepSales = "U00";
			var bill = Factory.NewWithValidTestData<BillOfLading>();
			jobHeader1.JH_ParentID = bill.PK;
			jobHeader1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			return new BillOfLading[] { bill };
		}

		protected override void AddStaffAssignmentForCompany(BillOfLading obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var job = obj.Factory.LoadTop1<JobHeader>(new CargoWise.EntityFramework.ZQuery(JobHeaderSchema.JH_ParentID, obj.PK));
			var assignment = job.LocalChargesAddr.Header.StaffAssignments.AddNew();
			assignment.O8_Role = role;
			assignment.O8_GS_NKPersonResponsible = staffCode;
			assignment.O8_GC = companyPk;
		}
	}
}
