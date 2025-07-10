using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;

namespace Enterprise.Freight.Forwarding.Orders.Module.Testing
{
	public class OrderLineCRMSecurityProviderTest : CRMSecurityProviderTest<OrderLine>
	{
		protected override CRMSecurityProvider<OrderLine> GetNewProviderForTest() => new OrderLineCRMSecurityProvider();

		protected override IEnumerable<OrderLine> GetTestObjectWithoutStaffAssignment()
		{
			var obj = Factory.NewWithValidTestData<OrderLine>();
			obj.Order.Buyer.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			return new OrderLine[] { obj };
		}

		protected override IEnumerable<OrderLine> GetTestObjectWithOrgStaffAssignment()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			var staffAssignments = org1.StaffAssignments.AddNew();
			staffAssignments.O8_Role = "SAL";
			staffAssignments.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			var address = org1.Addresses.AddNewMainAddress();

			var order = Factory.NewWithValidTestData<Order>();
			order.Buyer.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			order.BuyerPK = org1.PK;

			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			orderLine.Order.Buyer.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			orderLine.JO_JD = order.PK;

			return new OrderLine[] { orderLine };
		}

		protected override IEnumerable<OrderLine> GetTestObjectWithBizObjStaffAssignment() => System.Array.Empty<OrderLine>();

		protected override IEnumerable<OrderLine> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.Buyer.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			order.BuyerPK = org.PK;

			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			orderLine.Order.Buyer.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			orderLine.JO_JD = order.PK;

			return new OrderLine[] { orderLine };
		}

		protected override void AddStaffAssignmentForCompany(OrderLine obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var assignment = obj.Order.Buyer.StaffAssignments.AddNew();
			assignment.O8_Role = role;
			assignment.O8_GS_NKPersonResponsible = staffCode;
			assignment.O8_GC = companyPk;
		}
	}
}
