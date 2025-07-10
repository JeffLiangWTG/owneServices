using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public class ReceiveCRMSecurityProviderTest : CRMSecurityProviderTest<WhsReceive>
	{
		protected override CRMSecurityProvider<WhsReceive> GetNewProviderForTest() => new ReceiveCRMSecurityProvider();

		protected override IEnumerable<WhsReceive> GetTestObjectWithoutStaffAssignment()
		{
			var org = Factory.NewWithValidTestData<WhsReceive>();
			return new WhsReceive[] { org };
		}

		protected override IEnumerable<WhsReceive> GetTestObjectWithOrgStaffAssignment()
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

			var whsReceive1 = Factory.NewWithValidTestData<WhsReceive>();
			whsReceive1.WD_OH_Client = org1.PK;

			var whsReceive2 = Factory.NewWithValidTestData<WhsReceive>();
			jobHeader1.JH_ParentID = whsReceive2.PK;
			jobHeader1.JH_ParentTableCode = WhsDocketSchema.Constants.Prefix;

			return new WhsReceive[] { whsReceive1, whsReceive2 };
		}

		protected override IEnumerable<WhsReceive> GetTestObjectWithBizObjStaffAssignment()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			_ = org1.Addresses.AddNewMainAddress();

			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;

			var whsReceive1 = Factory.NewWithValidTestData<WhsReceive>();
			jobHeader1.JH_ParentID = whsReceive1.PK;
			jobHeader1.JH_ParentTableCode = WhsDocketSchema.Constants.Prefix;

			return new WhsReceive[] { whsReceive1 };
		}

		protected override IEnumerable<WhsReceive> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_GS_NKRepSales = "U01";
			jobHeader1.JH_OA_LocalChargesAddr = org.Addresses[0].PK;

			var whsReceive1 = Factory.NewWithValidTestData<WhsReceive>();
			jobHeader1.JH_ParentID = whsReceive1.PK;
			jobHeader1.JH_ParentTableCode = WhsDocketSchema.Constants.Prefix;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.MiscServ.OM_GG_OrgSecurityGroup = Factory.NewWithValidTestData<GlbGroup>().PK;
			whsReceive1.WD_OH_Client = org2.PK;

			var whsReceive2 = Factory.NewWithValidTestData<WhsReceive>();
			whsReceive2.WD_OH_Client = org.PK;

			return new WhsReceive[] { whsReceive1, whsReceive2 };
		}

		protected override void AddStaffAssignmentForCompany(WhsReceive obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			if (obj.Client.StaffAssignments.Count > 0)
			{
				var assignment = obj.Client.StaffAssignments.AddNew();
				assignment.O8_Role = role;
				assignment.O8_GS_NKPersonResponsible = staffCode;
				assignment.O8_GC = companyPk;
			}
		}
	}
}
