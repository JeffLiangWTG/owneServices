using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Module.Test
{
	public class OneOffQuoteCRMSecurityProviderTest : CRMSecurityProviderTest<ViewQuotedBooking>
	{
		protected override CRMSecurityProvider<ViewQuotedBooking> GetNewProviderForTest() => new OneOffQuoteCRMSecurityProvider();

		protected override IEnumerable<ViewQuotedBooking> GetTestObjectWithoutStaffAssignment()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var testQuotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var viewQuotedBooking = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, testQuotedBooking.Quote.PK));

			return new ViewQuotedBooking[] { viewQuotedBooking };
		}

		protected override IEnumerable<ViewQuotedBooking> GetTestObjectWithOrgStaffAssignment()
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

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var testQuotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var viewQuotedBooking = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, testQuotedBooking.Quote.PK));

			jobHeader1.JH_ParentID = quote.PK;
			jobHeader1.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;

			var quote2 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var testQuotedBooking2 = QuotedBooking.New(quote2.PK, ZGuid.Empty, Factory);
			quote2.QuotationClientAddress.E2_OA_Address = address.PK;
			var viewQuotedBooking2 = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, testQuotedBooking2.Quote.PK));

			return new ViewQuotedBooking[] { viewQuotedBooking, viewQuotedBooking2 };
		}

		protected override IEnumerable<ViewQuotedBooking> GetTestObjectWithBizObjStaffAssignment()
		{
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var testQuotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var viewQuotedBooking = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, testQuotedBooking.Quote.PK));

			jobHeader1.JH_ParentID = quote.PK;
			jobHeader1.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;

			return new ViewQuotedBooking[] { viewQuotedBooking };
		}

		protected override IEnumerable<ViewQuotedBooking> GetTestObjectWithTaskAssignment()
		{
			var capability = GlbStaff.CurrentUser.Capabilities.AddNew();
			capability.G4_Code = "CP1";
			capability.Factory.Save();

			var quote1 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var testQuotedBooking1 = QuotedBooking.New(quote1.PK, ZGuid.Empty, Factory);
			var viewQuotedBooking1 = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, testQuotedBooking1.Quote.PK));

			var workflowItem1 = (viewQuotedBooking1 as IWorkflowProvider).WorkflowItems.AddNew();
			workflowItem1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			var quote2 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var testQuotedBooking2 = QuotedBooking.New(quote2.PK, ZGuid.Empty, Factory);
			var viewQuotedBooking2 = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, testQuotedBooking2.Quote.PK));

			var workflowItem2 = (viewQuotedBooking2 as IWorkflowProvider).WorkflowItems.AddNew();
			workflowItem2.P9_G4_RequiredCapability = capability.PK;

			return new ViewQuotedBooking[] { viewQuotedBooking1, viewQuotedBooking2 };
		}

		protected override IEnumerable<ViewQuotedBooking> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_OA_LocalChargesAddr = org.Addresses[0].PK;
			jobHeader1.JH_GS_NKRepSales = "U00";

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var testQuotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var viewQuotedBooking = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, testQuotedBooking.Quote.PK));

			jobHeader1.JH_ParentID = quote.PK;
			jobHeader1.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;

			var quote2 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var testQuotedBooking2 = QuotedBooking.New(quote2.PK, ZGuid.Empty, Factory);
			quote2.QuotationClientAddress.E2_OA_Address = org.Addresses[0].PK;
			var viewQuotedBooking2 = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, testQuotedBooking2.Quote.PK));

			return new ViewQuotedBooking[] { viewQuotedBooking, viewQuotedBooking2 };
		}

		protected override void AddStaffAssignmentForCompany(ViewQuotedBooking obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var jobQuery = new ZQuery(JobHeaderSchema.JH_ParentID, obj.QuotedBooking.PK);
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
	}
}
