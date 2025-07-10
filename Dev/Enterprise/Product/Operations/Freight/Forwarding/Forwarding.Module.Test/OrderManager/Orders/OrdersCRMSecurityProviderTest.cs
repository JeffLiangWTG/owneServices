using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;

namespace Enterprise.Freight.Forwarding.Orders.Module.Testing
{
	public class OrdersCRMSecurityProviderTest : CRMSecurityProviderTest<Order>
	{
		protected override CRMSecurityProvider<Order> GetNewProviderForTest() => new OrdersCRMSecurityProvider();

		protected override IEnumerable<Order> GetTestObjectWithoutStaffAssignment()
		{
			var obj = Factory.NewWithValidTestData<Order>();
			obj.Buyer.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			return new Order[] { obj };
		}

		protected override IEnumerable<Order> GetTestObjectWithOrgStaffAssignment()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			var staffAssignments = org1.StaffAssignments.AddNew();
			staffAssignments.O8_Role = "SAL";
			staffAssignments.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			_ = org1.Addresses.AddNewMainAddress();

			var order = Factory.NewWithValidTestData<Order>();
			order.Buyer.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			order.BuyerPK = org1.PK;

			return new Order[] { order };
		}

		protected override IEnumerable<Order> GetTestObjectWithBizObjStaffAssignment() => System.Array.Empty<Order>();

		protected override IEnumerable<Order> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.Buyer.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			order.BuyerPK = org.PK;

			return new Order[] { order };
		}

		protected override void AddStaffAssignmentForCompany(Order obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var assignment = obj.Buyer.StaffAssignments.AddNew();
			assignment.O8_Role = role;
			assignment.O8_GS_NKPersonResponsible = staffCode;
			assignment.O8_GC = companyPk;
		}
	}
}
