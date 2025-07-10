using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public class OrderCRMSecurityProviderTest : CRMSecurityProviderTest<WhsOrder>
	{
		protected override CRMSecurityProvider<WhsOrder> GetNewProviderForTest() => new OrderCRMSecurityProvider();

		protected override IEnumerable<WhsOrder> GetTestObjectWithoutStaffAssignment()
		{
			var org = Factory.NewWithValidTestData<WhsOrder>();
			return new WhsOrder[] { org };
		}

		protected override IEnumerable<WhsOrder> GetTestObjectWithOrgStaffAssignment()
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

			var whsOrder1 = Factory.NewWithValidTestData<WhsOrder>();
			whsOrder1.WD_OH_Client = org1.PK;

			var whsOrder2 = Factory.NewWithValidTestData<WhsOrder>();
			jobHeader1.JH_ParentID = whsOrder2.PK;
			jobHeader1.JH_ParentTableCode = WhsDocketSchema.Constants.Prefix;

			return new WhsOrder[] { whsOrder1, whsOrder2 };
		}

		protected override IEnumerable<WhsOrder> GetTestObjectWithBizObjStaffAssignment()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			org1.Addresses.AddNewMainAddress();

			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;

			var whsOrder1 = Factory.NewWithValidTestData<WhsOrder>();
			jobHeader1.JH_ParentID = whsOrder1.PK;
			jobHeader1.JH_ParentTableCode = WhsDocketSchema.Constants.Prefix;

			return new WhsOrder[] { whsOrder1 };
		}

		protected override IEnumerable<WhsOrder> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_GS_NKRepSales = "U01";
			jobHeader1.JH_OA_LocalChargesAddr = org.Addresses[0].PK;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.MiscServ.OM_GG_OrgSecurityGroup = Factory.NewWithValidTestData<GlbGroup>().PK;
			var whsOrder1 = Factory.NewWithValidTestData<WhsOrder>();
			whsOrder1.WD_OH_Client = org2.PK;

			jobHeader1.JH_ParentID = whsOrder1.PK;
			jobHeader1.JH_ParentTableCode = WhsDocketSchema.Constants.Prefix;

			var whsOrder2 = Factory.NewWithValidTestData<WhsOrder>();
			whsOrder2.WD_OH_Client = org.PK;

			return new WhsOrder[] { whsOrder1, whsOrder2 };
		}

		protected override void AddStaffAssignmentForCompany(WhsOrder obj, ZString staffCode, ZString role, ZGuid companyPk)
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
