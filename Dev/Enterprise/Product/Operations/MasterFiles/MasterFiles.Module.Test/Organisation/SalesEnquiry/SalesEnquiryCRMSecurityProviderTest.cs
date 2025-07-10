using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class SalesEnquiryCRMSecurityProviderTest : CRMSecurityProviderTest<SalesEnquiry>
	{
		protected override CRMSecurityProvider<SalesEnquiry> GetNewProviderForTest() => new SalesEnquiryCRMSecurityProvider();

		protected override IEnumerable<SalesEnquiry> GetTestObjectWithoutStaffAssignment()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNewMainAddress();

			var salesEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			salesEnquiry.O1_GS_NKRepAssigned = "U00";
			salesEnquiry.O1_OA_LinkedAddress = address.PK;
			return new SalesEnquiry[] { salesEnquiry };
		}

		protected override IEnumerable<SalesEnquiry> GetTestObjectWithOrgStaffAssignment()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			var staffAssignments = org1.StaffAssignments.AddNew();
			staffAssignments.O8_Role = "SAL";
			staffAssignments.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			var address = org1.Addresses.AddNewMainAddress();

			var salesEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			salesEnquiry.O1_OA_LinkedAddress = address.PK;
			salesEnquiry.O1_GS_NKRepAssigned = "U00";

			return new SalesEnquiry[] { salesEnquiry };
		}

		protected override IEnumerable<SalesEnquiry> GetTestObjectWithBizObjStaffAssignment()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNewMainAddress();

			var salesEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			salesEnquiry.O1_GS_NKRepAssigned = GlbStaff.CurrentUser.GS_Code;
			salesEnquiry.O1_OA_LinkedAddress = address.PK;
			return new SalesEnquiry[] { salesEnquiry };
		}

		protected override IEnumerable<SalesEnquiry> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var salesEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			salesEnquiry.O1_OA_LinkedAddress = org.Addresses[0].PK;
			salesEnquiry.O1_GS_NKRepAssigned = "U00";

			return new SalesEnquiry[] { salesEnquiry };
		}

		protected override IEnumerable<SalesEnquiry> GetTestObjectWithTaskAssignment()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNewMainAddress();

			var objs = base.GetTestObjectWithTaskAssignment();
			foreach (var obj in objs)
			{
				obj.O1_OA_LinkedAddress = address.PK;
			}

			return objs;
		}

		protected override void AddStaffAssignmentForCompany(SalesEnquiry obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var assignment = obj.LinkedAddress.Header.StaffAssignments.AddNew();
			assignment.O8_Role = role;
			assignment.O8_GS_NKPersonResponsible = staffCode;
			assignment.O8_GC = companyPk;
		}

		public void TestSalesEnquiryWithoutLinkedAddress()
		{
			var salesEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			Factory.Save();

			ProviderForTest.CRMSecurity.IgnoreOSMG.IsAllowed = false;
			ProviderForTest.CRMSecurity.IgnoreTaskAssignment.IsAllowed = false;
			ProviderForTest.CRMSecurity.ViewByStaffNotAssigned.IsAllowed = false;

			var filters = new ModuleFilterCollection();
			ProviderForTest.AddCRMSecurityFilterStrips(Factory, filters);

			var filter = filters["Org. Security Group Security"] as ModuleNkFilter;
			var objects = Factory.Load<SalesEnquiry>(SetupCRMSecurityFilterStripsQuery(filter.Query));
			AssertCollectionNotContains(salesEnquiry, objects);
		}
	}
}
