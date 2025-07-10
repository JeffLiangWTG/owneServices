using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.Module.Testing
{
	public class QuotationsCRMSecurityProviderTest : CRMSecurityProviderTest<Quote>
	{
		protected override CRMSecurityProvider<Quote> GetNewProviderForTest() => new QuotationsCRMSecurityProvider();

		protected override IEnumerable<Quote> GetTestObjectWithoutStaffAssignment()
		{
			var quote = Factory.NewWithValidTestData<Quote>();
			return new Quote[] { quote };
		}

		protected override IEnumerable<Quote> GetTestObjectWithOrgStaffAssignment()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			var staffAssignments = org1.StaffAssignments.AddNew();
			staffAssignments.O8_Role = "SAL";
			staffAssignments.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			_ = org1.Addresses.AddNewMainAddress();

			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OH = org1.PK;

			return new Quote[] { quote };
		}

		protected override IEnumerable<Quote> GetTestObjectWithBizObjStaffAssignment() => System.Array.Empty<Quote>();

		protected override IEnumerable<Quote> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OH = org.PK;

			return new Quote[] { quote };
		}

		protected override void AddStaffAssignmentForCompany(Quote obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var assignment = obj.Header.StaffAssignments.AddNew();
			assignment.O8_Role = role;
			assignment.O8_GS_NKPersonResponsible = staffCode;
			assignment.O8_GC = companyPk;
		}
	}
}
