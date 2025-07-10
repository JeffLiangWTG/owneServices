using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Module.Test
{
	public class QuotedBookingCRMSecurityProviderTest : CRMSecurityProviderTest<ViewQuotedBooking>
	{
		[ExpectNoExceptions]
		public void TestOSMGFilter_AccessToOrgWithoutOSMG_ButEmptyLocalClient()
		{
			if (ProviderForTest.CRMSecurity.IgnoreOSMG != null)
			{
				OrgOSMG.MiscServ.OM_GG_OrgSecurityGroup = ZGuid.Empty;
				var orgWithSecurity = Factory.NewWithValidTestData<OrgHeader>();
				orgWithSecurity.MiscServ.OM_GG_OrgSecurityGroup = OSMG.PK;

				var objNoLocalCient = GetTestObjectWithBizObjStaffAssignment();
				var objWithLocalClient = GetTestObjectWithOrgAssignedForOSMG(orgWithSecurity);

				Factory.Save();

				ProviderForTest.CRMSecurity.IgnoreOSMG.IsAllowed = false;
				var filters = new ModuleFilterCollection();
				ProviderForTest.AddCRMSecurityFilterStrips(Factory, filters);
				var filter = filters["Org. Security Group Security"] as ModuleNkFilter;
				var query = SetupCRMSecurityFilterStripsQuery(filter.Query);

				var resultJobs = Factory.Load<ViewQuotedBooking>(query);
				AssertContainsExactElementsInAnyOrder(objNoLocalCient.Union(objWithLocalClient), resultJobs);
			}
		}

		protected override void AssertStaffAssignmentFilterResult(IEnumerable<ViewQuotedBooking> results)
		{
			AssertEquals("Only the local client is empty should be found", 1, results.Count());
			AssertEquals(ZGuid.Empty, results.First().QuotedBooking.Job.JH_OA_LocalChargesAddr);
		}

		protected override void AssertOSMGFilterWithOsmgOrgsResults(IEnumerable<ViewQuotedBooking> orgObjectsForOSMG, IEnumerable<ViewQuotedBooking> results)
		{
			AssertEquals("One of the results must have the empty local address",
				1,
				results.Count(x => x.QuotedBooking.Job.JH_OA_LocalChargesAddr == ZGuid.Empty));

			// +1 for the empty local client from GetTestObjectWithBizObjStaffAssignment
			AssertEquals("Business objects with org assigned for OSMG should be loaded", orgObjectsForOSMG.Count() + 1, results.Count());
		}

		protected override void AssertOSMGFilterWithAnotherUserResults(IEnumerable<ViewQuotedBooking> results)
		{
			AssertEquals("Only the local client is empty should be found", 1, results.Count());
			AssertEquals(ZGuid.Empty, results.First().QuotedBooking.Job.JH_OA_LocalChargesAddr);
		}

		protected override CRMSecurityProvider<ViewQuotedBooking> GetNewProviderForTest() => new QuotedBookingCRMSecurityProvider();

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
			var address = org1.Addresses.AddNewMainAddress();

			var staffAssignments = org1.StaffAssignments.AddNew();
			staffAssignments.O8_Role = "SAL";
			staffAssignments.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;

			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_OA_LocalChargesAddr = address.PK;
			jobHeader1.JH_GS_NKRepSales = "U00";

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var testQuotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var viewQuotedBooking = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, testQuotedBooking.Quote.PK));

			jobHeader1.JH_ParentID = viewQuotedBooking.PK;
			jobHeader1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			return new ViewQuotedBooking[] { viewQuotedBooking };
		}

		protected override IEnumerable<ViewQuotedBooking> GetTestObjectWithBizObjStaffAssignment()
		{
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			jobHeader1.JH_OA_LocalChargesAddr = ZGuid.Empty;

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var testQuotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var viewQuotedBooking = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, testQuotedBooking.Quote.PK));

			jobHeader1.JH_ParentID = viewQuotedBooking.PK;
			jobHeader1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

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

			var workflowItem1 = (testQuotedBooking1 as IWorkflowProvider).WorkflowItems.AddNew();
			workflowItem1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			var quote2 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var testQuotedBooking2 = QuotedBooking.New(quote2.PK, ZGuid.Empty, Factory);
			var viewQuotedBooking2 = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, testQuotedBooking2.Quote.PK));

			var workflowItem2 = (testQuotedBooking2 as IWorkflowProvider).WorkflowItems.AddNew();
			workflowItem2.P9_G4_RequiredCapability = capability.PK;

			return new ViewQuotedBooking[] { viewQuotedBooking1, viewQuotedBooking2 };
		}

		protected override IEnumerable<ViewQuotedBooking> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			jobHeader1.JH_GS_NKRepSales = "U01";
			jobHeader1.JH_OA_LocalChargesAddr = org.Addresses[0].PK;

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var testQuotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var viewQuotedBooking = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, testQuotedBooking.Quote.PK));

			jobHeader1.JH_ParentID = viewQuotedBooking.PK;
			jobHeader1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			return new ViewQuotedBooking[] { viewQuotedBooking };
		}

		protected override void AddStaffAssignmentForCompany(ViewQuotedBooking obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var job = obj.Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, obj.PK));
			var assignment = job.LocalChargesAddr.Header.StaffAssignments.AddNew();
			assignment.O8_Role = role;
			assignment.O8_GS_NKPersonResponsible = staffCode;
			assignment.O8_GC = companyPk;
		}
	}
}
