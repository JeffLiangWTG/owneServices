using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module
{
	public class SalesDashboardOneOffQuoteCRMSecurityProviderTest : CRMSecurityProviderTest<RateOneOffShipment>
	{
		protected override CRMSecurityProvider<RateOneOffShipment> GetNewProviderForTest() => new SalesDashboardOneOffQuoteCRMSecurityProvider();

		protected override IEnumerable<RateOneOffShipment> GetTestObjectWithoutStaffAssignment()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var result = CreateRateOneOffShipment(quote);
			return new RateOneOffShipment[] { result };
		}

		protected override IEnumerable<RateOneOffShipment> GetTestObjectWithOrgStaffAssignment()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			var staffAssignments = org1.StaffAssignments.AddNew();
			staffAssignments.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			staffAssignments.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			var address = org1.Addresses.AddNewMainAddress();

			var quote1 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var result1 = CreateRateOneOffShipment(quote1);
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_OA_LocalChargesAddr = address.PK;
			jobHeader1.JH_GS_NKRepSales = "U00";
			jobHeader1.JH_ParentID = quote1.PK;
			jobHeader1.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;

			var quote2 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var result2 = CreateRateOneOffShipment(quote2);
			quote2.QuotationClientAddress.E2_OA_Address = address.PK;

			return new RateOneOffShipment[] { result1, result2 };
		}

		protected override IEnumerable<RateOneOffShipment> GetTestObjectWithBizObjStaffAssignment()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var result = CreateRateOneOffShipment(quote);
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			jobHeader.JH_ParentID = quote.PK;
			jobHeader.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;

			return new RateOneOffShipment[] { result };
		}

		protected override IEnumerable<RateOneOffShipment> GetTestObjectWithTaskAssignment()
		{
			var capability = GlbStaff.CurrentUser.Capabilities.AddNew();
			capability.G4_Code = "CP1";
			capability.Factory.Save();

			var quote1 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var result1 = CreateRateOneOffShipment(quote1);
			var workflowItem1 = (result1.Factory.Load<ViewQuotedBooking>(result1.TT_TH) as IWorkflowProvider).WorkflowItems.AddNew();
			workflowItem1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			var quote2 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var result2 = CreateRateOneOffShipment(quote2);
			var workflowItem2 = (result2.Factory.Load<ViewQuotedBooking>(result2.TT_TH) as IWorkflowProvider).WorkflowItems.AddNew();
			workflowItem2.P9_G4_RequiredCapability = capability.PK;

			return new RateOneOffShipment[] { result1, result2 };
		}

		protected override IEnumerable<RateOneOffShipment> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_OA_LocalChargesAddr = org.Addresses[0].PK;
			jobHeader1.JH_GS_NKRepSales = "U00";

			var quote1 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var result1 = CreateRateOneOffShipment(quote1);
			jobHeader1.JH_ParentID = quote1.PK;
			jobHeader1.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;

			var quote2 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var result2 = CreateRateOneOffShipment(quote2);
			quote2.QuotationClientAddress.E2_OA_Address = org.Addresses[0].PK;

			return new RateOneOffShipment[] { result1, result2 };
		}

		protected override void AddStaffAssignmentForCompany(RateOneOffShipment obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var jobQuery = new ZQuery(JobHeaderSchema.JH_ParentID, (obj.Factory.Load<ViewQuotedBooking>(obj.TT_TH)).QuotedBooking.PK);
			jobQuery.AddToFilter(JobHeaderSchema.JH_GC, Env.CurrentCompanyPK);
			var job = obj.Factory.LoadTop1<JobHeader>(jobQuery);
			if (job != null)
			{
				var assignment = job.LocalChargesAddr.Header.StaffAssignments.AddNew();
				assignment.O8_Role = role;
				assignment.O8_GS_NKPersonResponsible = staffCode;
				assignment.O8_GC = companyPk;
			}
		}

		RateOneOffShipment CreateRateOneOffShipment(Quote quote)
		{
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var result = Factory.LoadTop1<RateOneOffShipment>(new ZQuery(RateOneOffShipmentSchema.TT_TH, quotedBooking.ViewPK));
			return result;
		}
	}
}
