using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Rating.Module.Test.Quotations;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(QuotationsFilterBusinessObject))]
	public class QuotationsFilterBusinessObjectTest : RatingFilterBusinessObjectTestCase<QuotationsFilterBusinessObject>
	{
		#region Filter

		public void TestSalesRepSecurityFilter()
		{
			var salesOne = Factory.NewWithValidTestData<GlbStaff>();
			salesOne.GS_GB_HomeBranch = Env.CurrentBranch.PK;
			salesOne.GS_LoginName = "chuck norris";
			salesOne.GS_Code = "CN";

			var salesTwo = Factory.NewWithValidTestData<GlbStaff>();
			salesTwo.GS_GB_HomeBranch = Env.CurrentBranch.PK;
			salesTwo.GS_LoginName = "bruce lee";
			salesTwo.GS_Code = "BL";

			var salesThree = Factory.NewWithValidTestData<GlbStaff>();
			salesThree.GS_GB_HomeBranch = Env.CurrentBranch.PK;
			salesThree.GS_LoginName = "good guy";
			salesThree.GS_Code = "GG";

			var salesFour = Factory.NewWithValidTestData<GlbStaff>();
			salesFour.GS_GB_HomeBranch = Factory.NewWithValidTestData<GlbBranch>().PK;
			salesFour.GS_LoginName = "bad guy";
			salesFour.GS_Code = "BG";

			var quoteOne = Factory.NewWithValidTestData<Quote>();
			quoteOne.TH_GS_NKFirstSignatory = salesOne.GS_Code;

			var quoteTwo = Factory.NewWithValidTestData<Quote>();
			quoteTwo.TH_GS_NKFirstSignatory = salesTwo.GS_Code;
			quoteTwo.TH_GS_NKSecondSignatory = salesFour.GS_Code;

			var quoteThree = Factory.NewWithValidTestData<Quote>();
			quoteThree.TH_GS_NKSecondSignatory = salesThree.GS_Code;

			var quoteFour = Factory.NewWithValidTestData<Quote>();
			quoteFour.TH_GS_NKFirstSignatory = salesFour.GS_Code;
			Factory.Save();

			using (Env.SetTemporaryUserContext(salesOne.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				QuotationsFilterBusinessObject filterBizo = new QuotationsFilterBusinessObject();

				QuoteCollection quotes = new QuoteCollection(Factory);
				Env.Security.QuotationShowAllQuotes.IsAllowed = true;
				quotes.Load(filterBizo.Filter);
				AssertEquals("Show all quotes", 4, quotes.Count);

				Env.Security.QuotationShowAllQuotes.IsAllowed = false;
				Env.Security.QuotationShowBranchQuotes.IsAllowed = true;
				quotes.Load(filterBizo.Filter);
				AssertEquals("Show only branch quotes", 3, quotes.Count);

				Env.Security.QuotationShowBranchQuotes.IsAllowed = false;
				quotes.Load(filterBizo.Filter);
				AssertEquals("Show only personal quotes", 1, quotes.Count);

				var staffLoaded = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, quotes[0].TH_GS_NKFirstSignatory);
				AssertEquals("Correct quote filtered for current user", "chuck norris", staffLoaded.GS_LoginName);
			}
		}

		#endregion

		#region Staff Filter

		public virtual void TestStaffFilter()
		{
			var salesRep = Factory.New<GlbStaff>();
			salesRep.GS_Code = "S1";
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.StaffAssignments.OverallSalesRep = salesRep.GS_Code;

			var testQuote = Factory.New<Quote>();
			testQuote.QuotationClientAddress.OrganisationPK = org.PK;

			Factory.Save();

			RatingFilterBusinessObject filterBizo = (RatingFilterBusinessObject)GetNewFilterStripBusinessObject();
			RatingHeaderCollection quotes = new RatingHeaderCollection(Factory);

			quotes.Load(filterBizo.Filter);
			AssertEquals("1 quotation exists", 1, quotes.Count);

			((ModuleNkFilter)filterBizo[RateFilterHelper.Constants.StaffFilterSalesRep]).IsActive = true;
			((ModuleNkFilter)filterBizo[RateFilterHelper.Constants.StaffFilterSalesRep]).Property = GlbStaff.CurrentUser.GS_Code;
			quotes.Load(filterBizo.Filter);
			AssertEquals("No quotes where sales rep is current user", 0, quotes.Count);

			((ModuleNkFilter)filterBizo[RateFilterHelper.Constants.StaffFilterSalesRep]).Property = salesRep.GS_Code;
			quotes.Load(filterBizo.Filter);
			AssertEquals("1 quote where sales rep is the sales rep", 1, quotes.Count);
		}

		public void TestSignatoryFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var quote1 = Factory.New<Quote>();
			quote1.QuotationClientAddress.OrganisationPK = org.PK;
			quote1.TH_GS_NKFirstSignatory = staff1.GS_Code;
			quote1.TH_GS_NKSecondSignatory = ZString.Empty;

			var quote2 = Factory.New<Quote>();
			quote2.QuotationClientAddress.OrganisationPK = org.PK;
			quote2.TH_GS_NKFirstSignatory = ZString.Empty;
			quote2.TH_GS_NKSecondSignatory = staff2.GS_Code;

			Factory.Save();

			var filterBizo = (RatingFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleNkFilter)filterBizo[RateFilterHelper.Constants.Signatory];
			var quotes = new RatingHeaderCollection(Factory);

			quotes.Load(filterBizo.Filter);
			AssertEquals("2 quotations exist", 2, quotes.Count);

			filter.IsActive = true;
			filter.Property = GlbStaff.CurrentUser.GS_Code;
			quotes.Load(filterBizo.Filter);
			AssertEquals("No quotes where Signatory is current user", 0, quotes.Count);

			filter.Property = staff1.GS_Code;
			quotes.Load(filterBizo.Filter);
			AssertEquals("1 quote where First Signatory is the staff 1", 1, quotes.Count);
			AssertEquals(quote1, quotes[0]);

			filter.Property = staff2.GS_Code;
			quotes.Load(filterBizo.Filter);
			AssertEquals("1 quote where Second Signatory is the staff 2", 1, quotes.Count);
			AssertEquals(quote2, quotes[0]);
		}

		#endregion

		#region TestContractNumberFilter

		public void TestContractNumberFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var quote1 = Factory.NewWithValidTestData<Quote>();
			quote1.TH_OH = org1.PK;
			RateEntry entry1 = quote1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "");
			entry1.TI_ContractNumber = "ABC123";

			var quote2 = Factory.NewWithValidTestData<Quote>();
			quote2.TH_OH = org2.PK;
			RateEntry entry2 = quote2.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "");
			entry2.TI_ContractNumber = "";

			Factory.Save();

			QuotationsFilterBusinessObject filter = new QuotationsFilterBusinessObject();
			QuoteCollection quotes = new QuoteCollection(Factory);
			quotes.Load(filter.Filter);
			AssertEquals(2, quotes.Count);

			((ModuleTextFilter)filter["Client Contract Number"]).IsActive = true;
			((ModuleTextFilter)filter["Client Contract Number"]).Property = "ABC123";

			quotes.Load(filter.Filter);
			AssertEquals(1, quotes.Count);
		}

		#endregion

		#region TestCommodityCodeFilter

		public void TestCommodityCodeFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var quote1 = Factory.NewWithValidTestData<Quote>();
			quote1.TH_QuoteNumber = "111";
			quote1.TH_OH = org1.PK;
			var entry1 = quote1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "");
			entry1.TI_RH_NKCommodityCode = "BANA";

			var quote2 = Factory.NewWithValidTestData<Quote>();
			quote2.TH_QuoteNumber = "222";
			quote2.TH_OH = org2.PK;
			var entry2 = quote2.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "");
			entry2.TI_RH_NKCommodityCode = "PAIN";

			Factory.Save();

			var filter = new QuotationsFilterBusinessObject();
			var quotes = new QuoteCollection(Factory);
			quotes.Load(filter.Filter);

			var actual1 = quotes.Select(s => s.TH_QuoteNumber).ToArray();
			var expected1 = new ZString[] { "111/A", "222/A" };
			AssertContainsExactElementsInAnyOrder(
				"quote1 and quote2 should both be in the collection",
				expected1,
				actual1
			);

			((ModuleNkFilter)filter["Commodity Code"]).IsActive = true;
			((ModuleNkFilter)filter["Commodity Code"]).Property = "BANA";
			quotes.Load(filter.Filter);

			var actual2 = quotes.Select(s => s.TH_QuoteNumber).ToArray();
			var expected2 = new ZString[] { "111/A" };
			AssertContainsExactElementsInAnyOrder(
				"Only quote1's commodity code is BANA",
				expected2,
				actual2
			);
		}

		#endregion

		#region Status Filter

		public void TestStatusFilterOptions()
		{
			var filter = new QuotationsFilterBusinessObject();
			var options = ((ModuleTextFilter)filter["Status"]).List.Cast<CodeDescriptionPair>().Select(pair => pair.Code);
			var expectedOptions = new[] { "Accepted", "Client Accepted", "Active", "Approved", "Finalized", "Canceled", "Expired" };
			AssertContainsExactElementsInAnyOrder(expectedOptions, options);
		}

		public void TestStatusFilter()
		{
			var today = ZDate.Today;
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var activeQuote = Factory.New<Quote>();
			activeQuote.TH_QuoteDate = ZDate.Today.AddMonths(-6);
			activeQuote.TH_QuoteEndDate = today.AddDays(5);
			activeQuote.ShowApprovalDialog += NotApprovedDelegate;

			var activeQuoteEmptyEndDate = Factory.New<Quote>();
			activeQuoteEmptyEndDate.TH_QuoteDate = ZDate.Today.AddMonths(-6);
			activeQuoteEmptyEndDate.TH_QuoteEndDate = ZDate.Empty;
			activeQuoteEmptyEndDate.ShowApprovalDialog += NotApprovedDelegate;

			var activeFinalisedQuote = Factory.New<Quote>();
			activeFinalisedQuote.TH_QuoteDate = ZDate.Today.AddMonths(-6);
			activeFinalisedQuote.TH_QuoteEndDate = today.AddDays(5);
			activeFinalisedQuote.TH_IsLocked = true;
			activeFinalisedQuote.ShowApprovalDialog += NotApprovedDelegate;

			var expiredQuote = Factory.New<Quote>();
			expiredQuote.TH_QuoteDate = ZDate.Today.AddMonths(-6);
			expiredQuote.TH_QuoteEndDate = today.AddDays(-5);
			expiredQuote.ShowApprovalDialog += NotApprovedDelegate;

			var acceptedQuote = Factory.New<Quote>();
			acceptedQuote.TH_QuoteDate = ZDate.Today.AddMonths(-6);
			acceptedQuote.TH_Accepted = today;
			acceptedQuote.ShowApprovalDialog += NotApprovedDelegate;

			var clientAcceptedQuote = Factory.New<Quote>();
			clientAcceptedQuote.TH_QuoteDate = ZDate.Today.AddMonths(-6);
			clientAcceptedQuote.TH_ClientAccepted = today;
			clientAcceptedQuote.ShowApprovalDialog += NotApprovedDelegate;

			var acceptedAndClientAcceptedQuote = Factory.New<Quote>();
			acceptedAndClientAcceptedQuote.TH_QuoteDate = ZDate.Today.AddMonths(-6);
			acceptedAndClientAcceptedQuote.TH_ClientAccepted = today.AddDays(-1);
			acceptedAndClientAcceptedQuote.TH_Accepted = today;
			acceptedAndClientAcceptedQuote.ShowApprovalDialog += NotApprovedDelegate;

			var cancelledQuote = Factory.New<Quote>();
			cancelledQuote.TH_QuoteDate = ZDate.Today.AddMonths(-6);
			cancelledQuote.TH_IsCancelled = true;
			cancelledQuote.ShowApprovalDialog += NotApprovedDelegate;

			var oneOffQuote = Factory.New<Quote>();
			oneOffQuote.TH_QuoteDate = ZDate.Today.AddMonths(-6);
			oneOffQuote.TH_OneTimeQuote = true;
			oneOffQuote.ShowApprovalDialog += NotApprovedDelegate;

			var acceptedCancelledQuote = Factory.New<Quote>();
			acceptedCancelledQuote.TH_QuoteDate = ZDate.Today.AddMonths(-6);
			acceptedCancelledQuote.TH_IsCancelled = true;
			acceptedCancelledQuote.TH_Accepted = today;
			acceptedCancelledQuote.ShowApprovalDialog += NotApprovedDelegate;

			var expiredCancelledQuote = Factory.New<Quote>();
			expiredCancelledQuote.TH_IsCancelled = true;
			expiredCancelledQuote.TH_QuoteDate = ZDate.Today.AddMonths(-6);
			expiredCancelledQuote.TH_QuoteEndDate = today.AddDays(-5);
			expiredCancelledQuote.ShowApprovalDialog += NotApprovedDelegate;

			var approvedQuote = Factory.New<Quote>();
			approvedQuote.TH_QuoteDate = ZDate.Today.AddMonths(-6);
			approvedQuote.TH_QuoteEndDate = today.AddDays(5);
			approvedQuote.ShowApprovalDialog += ApprovedDelegate;

			Factory.Save();

			var filterBizo = new QuotationsFilterBusinessObject();
			var quotes = new QuoteCollection(Factory);

			((ModuleTextFilter)filterBizo["Status"]).IsActive = true;
			((ModuleTextFilter)filterBizo["Status"]).Property = Quote.QuoteStatusOptions.Active.ToString();
			quotes.Load(filterBizo.Filter);
			AssertEquals(4, quotes.Count);
			Assert(quotes.Contains(activeQuote));
			Assert(quotes.Contains(activeFinalisedQuote));
			Assert(quotes.Contains(activeQuoteEmptyEndDate));
			Assert(quotes.Contains(approvedQuote));

			((ModuleTextFilter)filterBizo["Status"]).Property = Quote.QuoteStatusOptions.Finalized.ToString();
			quotes.Load(filterBizo.Filter);
			AssertEquals(1, quotes.Count);
			Assert(quotes.Contains(activeFinalisedQuote));

			((ModuleTextFilter)filterBizo["Status"]).Property = Quote.QuoteStatusOptions.Accepted.ToString();
			quotes.Load(filterBizo.Filter);
			AssertEquals(2, quotes.Count);
			Assert(quotes.Contains(acceptedQuote));
			Assert(quotes.Contains(acceptedAndClientAcceptedQuote));

			((ModuleTextFilter)filterBizo["Status"]).Property = Quote.QuoteStatusOptions.Cancelled.ToString();
			quotes.Load(filterBizo.Filter);
			AssertEquals(3, quotes.Count);
			Assert(quotes.Contains(cancelledQuote));
			Assert(quotes.Contains(expiredCancelledQuote));
			Assert(quotes.Contains(acceptedCancelledQuote));

			((ModuleTextFilter)filterBizo["Status"]).Property = Quote.QuoteStatusOptions.Expired.ToString();
			quotes.Load(filterBizo.Filter);
			AssertEquals(1, quotes.Count);
			AssertEquals(expiredQuote, quotes[0]);

			((ModuleTextFilter)filterBizo["Status"]).Property = Quote.QuoteStatusOptions.Approved.ToString();
			quotes.Load(filterBizo.Filter);
			AssertEquals(1, quotes.Count);
			AssertEquals(approvedQuote, quotes[0]);

			((ModuleTextFilter)filterBizo["Status"]).Property = Quote.QuoteStatusOptions.ClientAccepted.ToString();
			quotes.Load(filterBizo.Filter);
			AssertEquals(1, quotes.Count);
			AssertEquals(clientAcceptedQuote, quotes[0]);
		}

		#endregion

		#region FollowUpDate Filter

		public void TestFollowUpDateFilter()
		{
			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_QuoteDate = new ZDate(2013, 04, 05);
			quote.TH_QuoteEndDate = new ZDate(2013, 05, 08);
			quote.TH_FollowUpDate = new ZDate(2013, 04, 06);

			var quote2 = Factory.NewWithValidTestData<Quote>();
			quote2.TH_QuoteDate = new ZDate(2020, 06, 05);
			quote2.TH_QuoteEndDate = new ZDate(2020, 07, 08);
			quote2.TH_FollowUpDate = new ZDate(2020, 08, 06);

			Factory.Save();

			var filterBizo = new QuotationsFilterBusinessObject();
			var quotes = new QuoteCollection(Factory);

			((ModuleDateFilter)filterBizo["Follow Up Date"]).IsActive = true;
			((ModuleDateFilter)filterBizo["Follow Up Date"]).Property1 = new ZDate(2013, 04, 06);
			((ModuleDateFilter)filterBizo["Follow Up Date"]).Property2 = new ZDate(2013, 05, 06);
			((ModuleDateFilter)filterBizo["Follow Up Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			quotes.Load(filterBizo.Filter);
			AssertCollectionContains("Follow Up Dates within the specified range", quote, quotes);
			AssertCollectionNotContains(quote2, quotes);

			((ModuleDateFilter)filterBizo["Follow Up Date"]).Property1 = new ZDate(2013, 01, 01);
			((ModuleDateFilter)filterBizo["Follow Up Date"]).Property2 = new ZDate(2013, 01, 31);
			quotes.Load(filterBizo.Filter);
			Assert("Follow Up Dates are outside the specified range", quotes.IsNullOrEmpty());

			((ModuleDateFilter)filterBizo["Follow Up Date"]).Property1 = new ZDate(2013, 04, 06);
			((ModuleDateFilter)filterBizo["Follow Up Date"]).Property2 = new ZDate(2013, 04, 06);
			quotes.Load(filterBizo.Filter);
			AssertCollectionContains("Follow Up Dates with a single-day range", quote, quotes);
			AssertCollectionNotContains(quote2, quotes);

			((ModuleDateFilter)filterBizo["Follow Up Date"]).Property1 = new ZDate(2013, 04, 06);
			((ModuleDateFilter)filterBizo["Follow Up Date"]).Property2 = new ZDate(2013, 04, 07);
			quotes.Load(filterBizo.Filter);
			AssertCollectionContains("Follow Up Dates with a range of exactly one day", quote, quotes);
			AssertCollectionNotContains(quote2, quotes);

			((ModuleDateFilter)filterBizo["Follow Up Date"]).Property1 = new ZDate(2013, 04, 05);
			((ModuleDateFilter)filterBizo["Follow Up Date"]).Property2 = new ZDate(2013, 05, 08);
			quotes.Load(filterBizo.Filter);
			AssertCollectionContains("Follow Up Dates within the entire date range of the quote", quote, quotes);
			AssertCollectionNotContains(quote2, quotes);

			((ModuleDateFilter)filterBizo["Follow Up Date"]).Property1 = new ZDate(2013, 06, 01);
			((ModuleDateFilter)filterBizo["Follow Up Date"]).Property2 = new ZDate(2013, 06, 30);
			quotes.Load(filterBizo.Filter);
			Assert("Range that covers the future", quotes.IsNullOrEmpty());

			((ModuleDateFilter)filterBizo["Follow Up Date"]).Property1 = new ZDate(2013, 01, 01);
			((ModuleDateFilter)filterBizo["Follow Up Date"]).Property2 = new ZDate(2013, 01, 31);
			quotes.Load(filterBizo.Filter);
			Assert("Range that covers the past", quotes.IsNullOrEmpty());

			((ModuleDateFilter)filterBizo["Follow Up Date"]).Property1 = new ZDate(2013, 01, 01);
			((ModuleDateFilter)filterBizo["Follow Up Date"]).Property2 = new ZDate(2021, 12, 31);
			quotes.Load(filterBizo.Filter);
			AssertCollectionContains("Range covers 2 Test Quotes", quote, quotes);
			AssertCollectionContains("Range covers 2 Test Quotes", quote2, quotes);
		}

		#endregion

		#region Cancellation Reason Filter

		public void TestCancellationReasonFilter()
		{
			var list = new CodeDescriptionBoolCollection();
			list.Add("AAA", (NoResString)"Test A", true);
			list.Add("BBB", (NoResString)"Test B", true);
			RatingDataRegistry.Instance.QuoteCancellationReasonCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var quote1 = Factory.New<Quote>();
			quote1.TH_IsCancelled = true;

			var quote2 = Factory.New<Quote>();
			quote2.TH_IsCancelled = true;
			quote2.TH_QuoteCancellationReason = "AAA";

			var quote3 = Factory.New<Quote>();
			quote3.TH_IsCancelled = true;
			quote3.TH_QuoteCancellationReason = "BBB";

			var quote4 = Factory.New<Quote>();
			quote4.TH_IsCancelled = true;
			quote4.TH_QuoteCancellationReason = "AAA";

			Factory.Save();

			QuotationsFilterBusinessObject filterBizO = new QuotationsFilterBusinessObject();
			QuoteCollection collection = new QuoteCollection(Factory);

			ModuleTextFilter filter = ((ModuleTextFilter)filterBizO["Cancellation Reason"]);

			filter.IsActive = true;
			filter.Property = "AAA";
			collection.Load(filterBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertEquals(false, collection.Contains(quote1));
			AssertEquals(true, collection.Contains(quote2));
			AssertEquals(false, collection.Contains(quote3));
			AssertEquals(true, collection.Contains(quote4));

			filter.Property = "BBB";
			collection.Load(filterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertEquals(false, collection.Contains(quote1));
			AssertEquals(false, collection.Contains(quote2));
			AssertEquals(true, collection.Contains(quote3));
			AssertEquals(false, collection.Contains(quote4));

			filter.Property = "CCC";
			collection.Load(filterBizO.Filter);
			AssertEquals(0, collection.Count);
		}

		#endregion

		#region Standard Spot Quote Filter

		public void TestStandardSpotQuoteFilter()
		{
			var normalQuote = Factory.NewWithValidTestData<Quote>();

			var spotQuote = Factory.NewWithValidTestData<Quote>();
			spotQuote.TH_OneTimeQuote = true;

			Factory.Save();

			QuotationsFilterBusinessObject filterBizo = new QuotationsFilterBusinessObject();

			QuoteCollection quotes = new QuoteCollection(Factory);
			quotes.Load(filterBizo.Filter);
			AssertEquals(1, quotes.Count);
		}

		#endregion

		#region Workflow Milestones Filters

		public void TestWorkflowFiltersPresent()
		{
			QuotationsFilterBusinessObject milestoneFilter = (QuotationsFilterBusinessObject)GetNewFilterStripBusinessObject();
			milestoneFilter.QueryObjectType = typeof(Quote);
			AssertNotNull("You must use WorkflowFilterStripsHelper to add Workflow filter strips", milestoneFilter["Milestone Date"]);
		}

		#endregion

		#region Sales Rep Filter

		public override void TestSalesRepFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "TS1";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "TS2";

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgStaffAssignments assignment1 = org.StaffAssignments.AddNew();
			assignment1.O8_GS_NKPersonResponsible = "TS1";
			assignment1.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			assignment1.O8_Department = "ALL";
			assignment1.O8_GC = GlbCompany.CurrentCompany.PK;
			OrgStaffAssignments assignment2 = org.StaffAssignments.AddNew();
			assignment2.O8_GS_NKPersonResponsible = "TS2";
			assignment2.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			assignment2.O8_Department = "ALL";
			assignment2.O8_GC = ZGuid.Empty;

			Factory.Save();

			Quote quote1 = Helper.NewQuote(org);
			Quote quote2 = Helper.NewQuote(Helper.NewOrgHeader());
			Factory.Save();

			RatingFilterBusinessObject filterBizO = (RatingFilterBusinessObject)GetNewFilterStripBusinessObject();
			ModuleNkFilter salesRepFilter = (ModuleNkFilter)filterBizO[RateFilterHelper.Constants.StaffFilterSalesRep];
			RatingHeaderCollection collection = new RatingHeaderCollection(Factory);

			collection.Load(filterBizO.Filter);
			AssertEquals(2, collection.Count);

			salesRepFilter.IsActive = true;
			salesRepFilter.Property = "TS1";
			collection.Load(filterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(quote1, collection);

			salesRepFilter.Property = "TS2";
			collection.Load(filterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(quote1, collection);
		}

		#endregion

		#region CRM Security

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<Quote>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.QuotationCRMSecurity);
		}

		#endregion

		#region FMC Tariff ID Filter

		public void TestFMCTariffIDFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var quote1 = Factory.NewWithValidTestData<Quote>();
			quote1.TH_OH = org1.PK;
			var entry1 = quote1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "");
			entry1.TI_FMCTariffID = "ABC";

			var quote2 = Factory.NewWithValidTestData<Quote>();
			quote2.TH_OH = org2.PK;
			var entry2 = quote2.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "");
			entry2.TI_FMCTariffID = "";

			Factory.Save();

			var filter = new QuotationsFilterBusinessObject();
			var quotes = new QuoteCollection(Factory);
			quotes.Load(filter.Filter);

			var actual1 = quotes.Select(s => s.PK).ToArray();
			var expected1 = new[] { quote1.PK, quote2.PK };
			AssertContainsExactElementsInAnyOrder(
				"If FMC Tariff ID is not filtered, we should load all quotes.",
				expected1,
				actual1
			);

			((ModuleTextFilter)filter["FMC Tariff ID"]).IsActive = true;
			((ModuleTextFilter)filter["FMC Tariff ID"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

			quotes.Load(filter.Filter);
			var actual2 = quotes.Select(s => s.PK).ToArray();
			var expected2 = new[] { quote2.PK };
			AssertContainsExactElementsInAnyOrder(
				"When FMC Tariff ID is blank, only the second quote should appear.",
				expected2,
				actual2
			);

			((ModuleTextFilter)filter["FMC Tariff ID"]).Property = "A";
			((ModuleTextFilter)filter["FMC Tariff ID"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;

			quotes.Load(filter.Filter);
			var actual3 = quotes.Select(s => s.PK).ToArray();
			var expected3 = new[] { quote1.PK };
			AssertContainsExactElementsInAnyOrder(
				"Only quote1 has an FMC Tariff ID starting with 'A'.",
				expected3,
				actual3
			);

			AssertEquals("FMCTariffID MaxLength", 4, ((ModuleTextFilter)filter["FMC Tariff ID"]).MaxLength);
		}

		#endregion

		#region Implementation

		protected override void AssertLastUpdatedFilter(FilterStripBusinessObject filterBizo)
		{
		}

		void NotApprovedDelegate(object sender, Quote.ApprovalDialogEventArgs e)
		{
			e.Cancel = true;
		}

		void ApprovedDelegate(object sender, Quote.ApprovalDialogEventArgs e)
		{
		}

		protected override List<RatingHeader> GetGlobalAndLocalRatingHeaders()
		{
			return null;
		}

		protected override RatingHeaderCollection GetRatingHeaderCollection() =>
			new QuoteCollection(Factory);

		protected override RatingHeader NewRatingHeader(OrgHeader client) =>
			Helper.NewQuote(client);

		public override void TestRatingHeaderOrganisationCaption()
		{
			var filter = new ClientRatesFilterBusinessObject();
			AssertNotNull(filter[RateFilterHelper.Constants.Client]);
			AssertNotNull(filter[RateFilterHelper.Constants.ServiceProvider]); // refer to TI_OH_Supplier
		}

		protected override string[] GetExpectedFilterDescriptions() => new[]
		{
			"Quote Date",
			"Expiry Date",
			"Follow Up Date",
			"Accepted Date",
			"Client Accepted Date",
			"Cancellation Reason",
			"Status",
			"Signatory",
			"Milestone Date",
			"Milestone Completed",
			"Next Milestone",
			"Last Completed Milestone",
			"Any Open Task Assigned To",
			"Next Task Assigned To",
			"Tasks",
			"Exceptions",
			"Milestones",
			"Triggers",
			RateEntryFilterUtility.Constants.Codes.StartDate,
			RateEntryFilterUtility.Constants.Codes.EndDate,
			RateEntryFilterUtility.Constants.Codes.EffectiveOn,
			RateFilterHelper.Constants.Client,
			RateFilterHelper.Constants.StaffFilterSalesRep,
			RateFilterHelper.Constants.OrganizationName,
			RateEntryFilterUtility.Constants.Codes.ServiceProvider,
			RateEntryFilterUtility.Constants.Codes.ControllingCustomer,
			RateEntryFilterUtility.Constants.Codes.Consignee,
			RateEntryFilterUtility.Constants.Codes.Consignor,
			RateEntryFilterUtility.Constants.Codes.CarrierServiceLevel,
			RateEntryFilterUtility.Constants.Codes.ProductWarehouse,
			RateEntryFilterUtility.Constants.Codes.TransitWarehouse,
			RateEntryFilterUtility.Constants.Codes.OriginDestination,
			RateEntryFilterUtility.Constants.Codes.Via,
			RateEntryFilterUtility.Constants.Codes.FirstLoad,
			RateEntryFilterUtility.Constants.Codes.LastDischarge,
			RateEntryFilterUtility.Constants.Codes.FirstRouteSetLoad,
			RateEntryFilterUtility.Constants.Codes.LastRouteSetDischarge,
			RateEntryFilterUtility.Constants.Codes.CrossTrade,
			RateEntryFilterUtility.Constants.Codes.FromToOrganization,
			RateEntryFilterUtility.Constants.Codes.FromToSuburb,
			RateEntryFilterUtility.Constants.Codes.FromPostcode,
			RateEntryFilterUtility.Constants.Codes.ToPostcode,
			RateEntryFilterUtility.Constants.Codes.FromToZone,
			RateEntryFilterUtility.Constants.Codes.FromLocationDescription,
			RateEntryFilterUtility.Constants.Codes.ToLocationDescription,
			RateEntryFilterUtility.Constants.Codes.TransportMode,
			RateEntryFilterUtility.Constants.Codes.ContainerType,
			RateEntryFilterUtility.Constants.Codes.ServiceLevel,
			RateEntryFilterUtility.Constants.Codes.AircraftType,
			RateFilterHelper.Constants.GlobalRateFilter,
			RateFilterHelper.Constants.QuoteNumber,
			RateEntryFilterUtility.Constants.Codes.CommodityCode,
			RateEntryFilterUtility.Constants.Codes.TransitTime,
			RateEntryFilterUtility.Constants.Codes.ClientContractNumber,
			RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider,
			RateEntryFilterUtility.Constants.Codes.Currency,
			RateEntryFilterUtility.Constants.Codes.IsNonOperatingReefer,
			RateEntryFilterUtility.Constants.Codes.FMCTariffID,
			RateLineModuleFilters.Constants.Codes.ActualPercentage,
			RateLineModuleFilters.Constants.Codes.UseOnlyActualWeightMeasure,
			RateLineModuleFilters.Constants.Codes.Condition,
			RateLineModuleFilters.Constants.Codes.ContainerOwnership,
			RateLineModuleFilters.Constants.Codes.ConversionFactor,
			RateLineModuleFilters.Constants.Codes.Currency,
			RateLineModuleFilters.Constants.Codes.ChargeCode,
			RateLineModuleFilters.Constants.Codes.FeeChargeType,
			RateLineModuleFilters.Constants.Codes.FeeChargeLevel,
			RateLineModuleFilters.Constants.Codes.Rounding,
			RateLineModuleFilters.Constants.Codes.HasOverrideChargeDescription,
			RateLineModuleFilters.Constants.Codes.IsJobLevelCharge,
			RateLineModuleFilters.Constants.Codes.UnitFactor,
			RateLineModuleFilters.Constants.Codes.UnitMultiple,
			RateLineModuleFilters.Constants.Codes.Units,
			RateLineModuleFilters.Constants.Codes.StartDate,
			RateLineModuleFilters.Constants.Codes.EndDate,
			RateLineModuleFilters.Constants.Codes.EffectiveOn,
			RateLineModuleFilters.Constants.Codes.ShowExpired,
			RateEntryFilterUtility.Constants.Codes.HBLDeliveryMode
		};

		#endregion

		#region TestQuotesWithDifferentStatus

		[TestDate(2013, 10, 21)]
		public void TestQuotesWithDifferentStatus()
		{
			var accepted1 = Factory.NewWithValidTestData<Quote>();
			accepted1.TH_QuoteDate = new ZDate(2013, 04, 05);
			accepted1.TH_QuoteEndDate = new ZDate(2013, 05, 08);
			accepted1.TH_Accepted = new ZDateTime(2013, 05, 04);
			Asserter.AddToScope(accepted1);

			var accepted2 = Factory.NewWithValidTestData<Quote>();
			accepted2.TH_QuoteDate = new ZDate(2013, 06, 14);
			accepted2.TH_QuoteEndDate = new ZDate(2013, 06, 24);
			accepted2.TH_Accepted = new ZDateTime(2013, 06, 14);
			Asserter.AddToScope(accepted2);

			var accepted3 = Factory.NewWithValidTestData<Quote>();
			accepted3.TH_QuoteDate = new ZDate(2013, 10, 10);
			accepted3.TH_QuoteEndDate = new ZDate(2013, 11, 25);
			accepted3.TH_Accepted = new ZDateTime(2013, 10, 21);
			Asserter.AddToScope(accepted3);

			var expired1 = Factory.NewWithValidTestData<Quote>();
			expired1.TH_QuoteDate = new ZDate(2013, 03, 06);
			expired1.TH_QuoteEndDate = new ZDate(2013, 05, 10);
			Asserter.AddToScope(expired1);

			var expired2 = Factory.NewWithValidTestData<Quote>();
			expired2.TH_QuoteDate = new ZDate(2013, 06, 01);
			expired2.TH_QuoteEndDate = new ZDate(2013, 09, 10);
			Asserter.AddToScope(expired2);

			var approved = Factory.NewWithValidTestData<Quote>();
			approved.TH_QuoteDate = new ZDate(2013, 10, 10);
			approved.TH_QuoteEndDate = new ZDate(2013, 12, 25);
			Asserter.AddToScope(approved);

			var cancelled1 = Factory.NewWithValidTestData<Quote>();
			cancelled1.TH_QuoteDate = new ZDate(2013, 08, 10);
			cancelled1.TH_QuoteEndDate = new ZDate(2013, 09, 22);
			cancelled1.IsCancelled = true;
			Asserter.AddToScope(cancelled1);

			var cancelled2 = Factory.NewWithValidTestData<Quote>();
			cancelled2.TH_QuoteDate = new ZDate(2013, 05, 05);
			cancelled2.TH_QuoteEndDate = new ZDate(2013, 08, 25);
			cancelled2.IsCancelled = true;
			Asserter.AddToScope(cancelled2);

			var finalized = Factory.NewWithValidTestData<Quote>();
			finalized.TH_QuoteDate = new ZDate(2013, 09, 10);
			finalized.TH_QuoteEndDate = new ZDate(2013, 10, 25);
			finalized.TH_IsLocked = true;
			Asserter.AddToScope(finalized);

			Factory.Save();

			AssertEquals(Quote.QuoteStatusOptions.Accepted, accepted1.QuoteStatus); //modify these to compare to a string rather than enum value
			AssertEquals(Quote.QuoteStatusOptions.Accepted, accepted2.QuoteStatus);
			AssertEquals(Quote.QuoteStatusOptions.Accepted, accepted3.QuoteStatus);

			AssertEquals(Quote.QuoteStatusOptions.Expired, expired1.QuoteStatus);
			AssertEquals(Quote.QuoteStatusOptions.Expired, expired2.QuoteStatus);

			AssertEquals(Quote.QuoteStatusOptions.Approved, approved.QuoteStatus);

			AssertEquals(Quote.QuoteStatusOptions.Cancelled, cancelled1.QuoteStatus);
			AssertEquals(Quote.QuoteStatusOptions.Cancelled, cancelled2.QuoteStatus);

			AssertEquals(Quote.QuoteStatusOptions.Finalized, finalized.QuoteStatus);

			var filters = new QuotationsFilterBusinessObject();
			var filter = (ModuleTextFilter)filters["Status"];

			filter.Property = Quote.QuoteStatusOptions.Accepted.ToString();
			Asserter.AssertMatches("Status is Accepted", filter, accepted1, accepted2, accepted3);

			filter.Property = Quote.QuoteStatusOptions.Expired.ToString();
			Asserter.AssertMatches("Status is Expired", filter, expired1, expired2);

			filter.Property = Quote.QuoteStatusOptions.Approved.ToString();
			Asserter.AssertMatches("Status is Approved", filter, approved);

			filter.Property = Quote.QuoteStatusOptions.Cancelled.ToString();
			Asserter.AssertMatches("Status is Cancelled", filter, cancelled1, cancelled2);

			filter.Property = Quote.QuoteStatusOptions.Finalized.ToString();
			Asserter.AssertMatches("Status is Finalized", filter, finalized);
		}

		FilterStripAsserter<Quote> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<Quote>(Factory, quote => quote.QuoteStatus)); }
		}
		FilterStripAsserter<Quote> asserter;

		#endregion

		#region HBL Delivery Mode Filter

		public void TestHBLDeliveryModeFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var quote1 = Factory.NewWithValidTestData<Quote>();
			quote1.TH_OH = org1.PK;
			var entry1 = quote1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "");
			entry1.TI_HBLDeliveryMode = "DOOR/DOOR";

			var quote2 = Factory.NewWithValidTestData<Quote>();
			quote2.TH_OH = org2.PK;
			var entry2 = quote2.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "");
			entry2.TI_HBLDeliveryMode = "";

			Factory.Save();

			var filter = new QuotationsFilterBusinessObject();
			var quotes = new QuoteCollection(Factory);
			quotes.Load(filter.Filter);
			AssertEquals(2, quotes.Count);

			((ModuleTextFilter)filter["HBL Delivery Mode"]).IsActive = true;
			((ModuleTextFilter)filter["HBL Delivery Mode"]).Property = "DOOR/DOOR";

			quotes.Load(filter.Filter);
			AssertEquals(1, quotes.Count);
		}

		#endregion

		#region CustomFieldFilter

		public void TestWorkflowCustomFieldsFilters()
		{
			var filter = GetNewFilterStripBusinessObject();
			AssertNull(filter["custom text"]);
			AssertNull(filter["custom int"]);
			AssertNull(filter["custom decimal"]);
			AssertNull(filter["custom datetime"]);
			AssertNull(filter["custom shipment string"]);
			QuotationTestHelper.CreateQuotationWorkflowWithCustomFields(Factory);
			filter = GetNewFilterStripBusinessObject();
			AssertNotNull(filter["custom text"]);
			AssertNotNull(filter["custom int"]);
			AssertNotNull(filter["custom decimal"]);
			AssertNotNull(filter["custom datetime"]);
			AssertNull(filter["custom shipment string"]);
			var workflowCustomFieldsFilter = (ModuleTextFilter)filter["custom text"];
			workflowCustomFieldsFilter.IsActive = true;
			AssertNoExceptionThrown(() => Factory.Load<Quote>(filter.Filter));
		}

		#endregion
	}
}
