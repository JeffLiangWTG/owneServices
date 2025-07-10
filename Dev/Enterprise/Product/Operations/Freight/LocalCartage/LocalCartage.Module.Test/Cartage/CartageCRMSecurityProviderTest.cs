using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	public class CartageCRMSecurityProviderTest : CRMSecurityProviderTest<CommonCartage>
	{
		protected override CRMSecurityProvider<CommonCartage> GetNewProviderForTest() => new CartageCRMSecurityProvider();
		protected override IEnumerable<CommonCartage> GetTestObjectWithoutStaffAssignment()
		{
			var obj = Factory.NewWithValidTestData<CommonCartage>();
			return new CommonCartage[] { obj };
		}

		protected override IEnumerable<CommonCartage> GetTestObjectWithOrgStaffAssignment()
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
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			jobHeader1.JH_ParentID = commonCartage.PK;
			jobHeader1.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
			return new CommonCartage[] { commonCartage };
		}

		protected override IEnumerable<CommonCartage> GetTestObjectWithBizObjStaffAssignment()
		{
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			jobHeader1.JH_ParentID = commonCartage.PK;
			jobHeader1.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
			return new CommonCartage[] { commonCartage };
		}

		protected override IEnumerable<CommonCartage> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_OA_LocalChargesAddr = org.Addresses[0].PK;
			jobHeader1.JH_GS_NKRepSales = "U00";
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			jobHeader1.JH_ParentID = commonCartage.PK;
			jobHeader1.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
			return new CommonCartage[] { commonCartage };
		}

		protected override void AddStaffAssignmentForCompany(CommonCartage obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var assignment = obj.Job.LocalChargesAddr.Header.StaffAssignments.AddNew();
			assignment.O8_Role = role;
			assignment.O8_GS_NKPersonResponsible = staffCode;
			assignment.O8_GC = companyPk;
		}
	}
}
