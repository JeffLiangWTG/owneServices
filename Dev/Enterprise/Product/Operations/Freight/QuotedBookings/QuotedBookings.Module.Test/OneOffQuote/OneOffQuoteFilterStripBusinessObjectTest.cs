using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Module;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Module.Test
{
	[TestedType(typeof(OneOffQuoteFilterStripBusinessObject))]
	public class OneOffQuoteFilterStripBusinessObjectTest : BaseQuotedBookingFilterStripBusinessObjectTest
	{
		protected override string[] GetExpectedFilterDescriptions() => new[]
		{
			"Quote #",
			"Booking #",
			"Active Status",
			"One Off Quote Approval Status",
			"Origin / Destination",
			"Client Name",
			"Sales Representative",
			"Client Related Parties",
			"Consignor Related Parties",
			"Consignee Related Parties",
			"Carrier",
			"Creditor",
			"Potential Creditor",
			"Potential Carrier",
			"Client",
			"Consignor / Consignee",
			"Creating User",
			"Last Edit User",
			"Created Time",
			"Last Edit Time",
			"Created On Web/Internal",
			"Mode",
			"Service Level",
			"DG Class / DG Substance",
			"AP Invoice #",
			"Charges with Debtor",
			"Charges with Creditor",
			"AR Transaction #",
			"Supplier Cost Reference",
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
			"Client Accepted Date",
			"Status",
			"One Off Quote KPI",
			"One Off Quote Source",
			"One Off Quote Revision Reason",
			"Transport Mode",
			"Container Mode",
			"Custom SQL Filter",
			"Company Tariff Level Override",
			"Commodity Code",
			"FMC Tariff ID",
			"HBL Delivery Mode",
			"CO2e (kg)",
			"Start Date",
			"End Date",
			"Used",
			"Final Print",
			"Branch",
			"Operations Representative",
			"Consignor Contact",
			"Consignee Contact",
			"Client Contact",
		};

		#region Dates

		#region TestClientAcceptedDate

		[TestDate(2000, 07, 15)]
		public void TestClientAcceptedDate()
		{
			var quotedBooking1 = CreateOneOffQuote(quoteNumber: "01", quoteDate: new ZDate(2000, 07, 01), quoteEndDate: null);
			quotedBooking1.Quote.TH_ClientAccepted = new ZDateTime(2000, 07, 13);
			var quotedBooking2 = CreateOneOffQuote(quoteNumber: "02", quoteDate: new ZDate(2000, 07, 01), quoteEndDate: null);
			quotedBooking2.Quote.TH_ClientAccepted = new ZDateTime(2000, 07, 15);
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleDateFilter)filter["Client Accepted Date"]).IsActive = true;
			((ModuleDateFilter)filter["Client Accepted Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			var collection = new ViewQuotedBookingCollection(Factory);

			AssertDateFilter(filter, "Client Accepted Date", fromDateTimeFilter: new ZDateTime(2000, 07, 12), toDateTimeFilter: new ZDateTime(2000, 07, 16), collection, expected: new [] { quotedBooking1, quotedBooking2 });
			AssertDateFilter(filter, "Client Accepted Date", fromDateTimeFilter: new ZDateTime(2000, 07, 12), toDateTimeFilter: new ZDateTime(2000, 07, 14), collection, expected: new [] { quotedBooking1 });
			AssertDateFilter(filter, "Client Accepted Date", fromDateTimeFilter: new ZDateTime(2000, 07, 14), toDateTimeFilter: new ZDateTime(2000, 07, 16), collection, expected: new [] { quotedBooking2 });
			AssertDateFilter(filter, "Client Accepted Date", fromDateTimeFilter: new ZDateTime(2000, 07, 10), toDateTimeFilter: new ZDateTime(2000, 07, 12), collection, expected: Array.Empty<QuotedBooking>());
		}

		[TestDate(2000, 07, 15)]
		public void TestClientAcceptedDate_EmptyFromDateTime()
		{
			var quotedBooking1 = CreateOneOffQuote(quoteNumber: "01", quoteDate: new ZDate(2000, 07, 01), quoteEndDate: null);
			quotedBooking1.Quote.TH_ClientAccepted = new ZDateTime(2000, 07, 13);
			var quotedBooking2 = CreateOneOffQuote(quoteNumber: "02", quoteDate: new ZDate(2000, 07, 01), quoteEndDate: null);
			quotedBooking2.Quote.TH_ClientAccepted = new ZDateTime(2000, 07, 15);
			var quotedBooking3 = CreateOneOffQuote(quoteNumber: "03", quoteDate: new ZDate(2000, 07, 01), quoteEndDate: null);
			quotedBooking3.Quote.TH_ClientAccepted = new ZDateTime(2000, 07, 10);
			var quotedBooking4 = CreateOneOffQuote(quoteNumber: "04", quoteDate: new ZDate(2000, 07, 01), quoteEndDate: null);
			quotedBooking4.Quote.TH_ClientAccepted = new ZDateTime(2000, 07, 20);
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleDateFilter)filter["Client Accepted Date"]).IsActive = true;
			((ModuleDateFilter)filter["Client Accepted Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			var collection = new ViewQuotedBookingCollection(Factory);

			AssertDateFilter(filter, "Client Accepted Date", fromDateTimeFilter: null, toDateTimeFilter: new ZDateTime(2000, 07, 16), collection, expected: new [] { quotedBooking1, quotedBooking2, quotedBooking3 });
		}

		[TestDate(2000, 07, 15)]
		public void TestClientAcceptedDate_EmptyToDateTime()
		{
			var quotedBooking1 = CreateOneOffQuote(quoteNumber: "01", quoteDate: new ZDate(2000, 07, 01), quoteEndDate: null);
			quotedBooking1.Quote.TH_ClientAccepted = new ZDateTime(2000, 07, 13);
			var quotedBooking2 = CreateOneOffQuote(quoteNumber: "02", quoteDate: new ZDate(2000, 07, 01), quoteEndDate: null);
			quotedBooking2.Quote.TH_ClientAccepted = new ZDateTime(2000, 07, 15);
			var quotedBooking3 = CreateOneOffQuote(quoteNumber: "03", quoteDate: new ZDate(2000, 07, 01), quoteEndDate: null);
			quotedBooking3.Quote.TH_ClientAccepted = new ZDateTime(2000, 07, 10);
			var quotedBooking4 = CreateOneOffQuote(quoteNumber: "04", quoteDate: new ZDate(2000, 07, 01), quoteEndDate: null);
			quotedBooking4.Quote.TH_ClientAccepted = new ZDateTime(2000, 07, 20);
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleDateFilter)filter["Client Accepted Date"]).IsActive = true;
			((ModuleDateFilter)filter["Client Accepted Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			var collection = new ViewQuotedBookingCollection(Factory);

			AssertDateFilter(filter, "Client Accepted Date", fromDateTimeFilter: new ZDateTime(2000, 07, 12), toDateTimeFilter: null, collection, expected: new [] { quotedBooking1, quotedBooking2, quotedBooking4 });
		}

		#endregion

		#region TestStartDate

		[TestDate(2000, 07, 15)]
		public void TestStartDate()
		{
			var quotedBooking1 = CreateOneOffQuote(quoteNumber: "01", quoteDate: new ZDate(2000, 07, 13), quoteEndDate: null);
			var quotedBooking2 = CreateOneOffQuote(quoteNumber: "02", quoteDate: new ZDate(2000, 07, 15), quoteEndDate: null);
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleDateFilter)filter["Start Date"]).IsActive = true;
			((ModuleDateFilter)filter["Start Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			var collection = new ViewQuotedBookingCollection(Factory);

			AssertDateFilter(filter, "Start Date", fromDateTimeFilter: new ZDateTime(2000, 07, 12), toDateTimeFilter: new ZDateTime(2000, 07, 16), collection, expected: new [] { quotedBooking1, quotedBooking2 });
			AssertDateFilter(filter, "Start Date", fromDateTimeFilter: new ZDateTime(2000, 07, 12), toDateTimeFilter: new ZDateTime(2000, 07, 14), collection, expected: new [] { quotedBooking1 });
			AssertDateFilter(filter, "Start Date", fromDateTimeFilter: new ZDateTime(2000, 07, 14), toDateTimeFilter: new ZDateTime(2000, 07, 16), collection, expected: new [] { quotedBooking2 });
			AssertDateFilter(filter, "Start Date", fromDateTimeFilter: new ZDateTime(2000, 07, 10), toDateTimeFilter: new ZDateTime(2000, 07, 12), collection, expected: Array.Empty<QuotedBooking>());
		}

		[TestDate(2000, 07, 15)]
		public void TestStartDate_EmptyFromDateTime()
		{
			var quotedBooking1 = CreateOneOffQuote(quoteNumber: "01", quoteDate: new ZDate(2000, 07, 10), quoteEndDate: null);
			var quotedBooking2 = CreateOneOffQuote(quoteNumber: "02", quoteDate: new ZDate(2000, 07, 13), quoteEndDate: null);
			var quotedBooking3 = CreateOneOffQuote(quoteNumber: "03", quoteDate: new ZDate(2000, 07, 15), quoteEndDate: null);
			var quotedBooking4 = CreateOneOffQuote(quoteNumber: "04", quoteDate: new ZDate(2000, 07, 20), quoteEndDate: null);
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleDateFilter)filter["Start Date"]).IsActive = true;
			((ModuleDateFilter)filter["Start Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			var collection = new ViewQuotedBookingCollection(Factory);

			AssertDateFilter(filter, "Start Date", fromDateTimeFilter: null, toDateTimeFilter: new ZDateTime(2000, 07, 16), collection, expected: new [] { quotedBooking1, quotedBooking2, quotedBooking3 });
		}

		[TestDate(2000, 07, 15)]
		public void TestStartDate_EmptyToDateTime()
		{
			var quotedBooking1 = CreateOneOffQuote(quoteNumber: "01", quoteDate: new ZDate(2000, 07, 10), quoteEndDate: null);
			var quotedBooking2 = CreateOneOffQuote(quoteNumber: "02", quoteDate: new ZDate(2000, 07, 13), quoteEndDate: null);
			var quotedBooking3 = CreateOneOffQuote(quoteNumber: "03", quoteDate: new ZDate(2000, 07, 15), quoteEndDate: null);
			var quotedBooking4 = CreateOneOffQuote(quoteNumber: "04", quoteDate: new ZDate(2000, 07, 20), quoteEndDate: null);
			quotedBooking4.StartDate = new ZDateTime(2000, 07, 20);
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleDateFilter)filter["Start Date"]).IsActive = true;
			((ModuleDateFilter)filter["Start Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			var collection = new ViewQuotedBookingCollection(Factory);

			AssertDateFilter(filter, "Start Date", fromDateTimeFilter: new ZDateTime(2000, 07, 12), toDateTimeFilter: null, collection, expected: new [] { quotedBooking2, quotedBooking3, quotedBooking4 });
		}

		#endregion

		#region TestEndDate

		[TestDate(2000, 07, 15)]
		public void TestEndDate()
		{
			var quotedBooking1 = CreateOneOffQuote(quoteNumber: "01", quoteDate: new ZDate(2000, 06, 13), quoteEndDate: new ZDate(2000, 07, 13));
			var quotedBooking2 = CreateOneOffQuote(quoteNumber: "02", quoteDate: new ZDate(2000, 06, 15), quoteEndDate: new ZDate(2000, 07, 15));
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleDateFilter)filter["End Date"]).IsActive = true;
			((ModuleDateFilter)filter["End Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			var collection = new ViewQuotedBookingCollection(Factory);

			AssertDateFilter(filter, "End Date", fromDateTimeFilter: new ZDateTime(2000, 07, 12), toDateTimeFilter: new ZDateTime(2000, 07, 16), collection, expected: new [] { quotedBooking1, quotedBooking2 });
			AssertDateFilter(filter, "End Date", fromDateTimeFilter: new ZDateTime(2000, 07, 12), toDateTimeFilter: new ZDateTime(2000, 07, 14), collection, expected: new [] { quotedBooking1 });
			AssertDateFilter(filter, "End Date", fromDateTimeFilter: new ZDateTime(2000, 07, 14), toDateTimeFilter: new ZDateTime(2000, 07, 16), collection, expected: new [] { quotedBooking2 });
			AssertDateFilter(filter, "End Date", fromDateTimeFilter: new ZDateTime(2000, 07, 10), toDateTimeFilter: new ZDateTime(2000, 07, 12), collection, expected: Array.Empty<QuotedBooking>());
		}

		[TestDate(2000, 07, 15)]
		public void TestEndDate_EmptyFromDateTime()
		{
			var quotedBooking1 = CreateOneOffQuote(quoteNumber: "01", quoteDate: new ZDate(2000, 06, 10), quoteEndDate: new ZDate(2000, 07, 10));
			var quotedBooking2 = CreateOneOffQuote(quoteNumber: "02", quoteDate: new ZDate(2000, 06, 13), quoteEndDate: new ZDate(2000, 07, 13));
			var quotedBooking3 = CreateOneOffQuote(quoteNumber: "03", quoteDate: new ZDate(2000, 06, 15), quoteEndDate: new ZDate(2000, 07, 15));
			var quotedBooking4 = CreateOneOffQuote(quoteNumber: "04", quoteDate: new ZDate(2000, 06, 20), quoteEndDate: new ZDate(2000, 07, 20));
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleDateFilter)filter["End Date"]).IsActive = true;
			((ModuleDateFilter)filter["End Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			var collection = new ViewQuotedBookingCollection(Factory);

			AssertDateFilter(filter, "End Date", fromDateTimeFilter: null, toDateTimeFilter: new ZDateTime(2000, 07, 16), collection, expected: new [] { quotedBooking1, quotedBooking2, quotedBooking3 });
		}

		[TestDate(2000, 07, 15)]
		public void TestEndDate_EmptyToDateTime()
		{
			var quotedBooking1 = CreateOneOffQuote(quoteNumber: "01", quoteDate: new ZDate(2000, 06, 10), quoteEndDate: new ZDate(2000, 07, 10));
			var quotedBooking2 = CreateOneOffQuote(quoteNumber: "02", quoteDate: new ZDate(2000, 06, 13), quoteEndDate: new ZDate(2000, 07, 13));
			var quotedBooking3 = CreateOneOffQuote(quoteNumber: "03", quoteDate: new ZDate(2000, 06, 15), quoteEndDate: new ZDate(2000, 07, 15));
			var quotedBooking4 = CreateOneOffQuote(quoteNumber: "04", quoteDate: new ZDate(2000, 06, 20), quoteEndDate: new ZDate(2000, 07, 20));
			quotedBooking4.StartDate = new ZDateTime(2000, 07, 20);
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleDateFilter)filter["End Date"]).IsActive = true;
			((ModuleDateFilter)filter["End Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			var collection = new ViewQuotedBookingCollection(Factory);

			AssertDateFilter(filter, "End Date", fromDateTimeFilter: new ZDateTime(2000, 07, 12), toDateTimeFilter: null, collection, expected: new [] { quotedBooking2, quotedBooking3, quotedBooking4 });
		}

		#endregion

		#region AssertDateFilter

		static void AssertDateFilter(FilterStripBusinessObject filterStripBusinessObject, ZString filterName, ZDateTime? fromDateTimeFilter, ZDateTime? toDateTimeFilter, ViewQuotedBookingCollection collection, QuotedBooking[] expected)
		{
			if (fromDateTimeFilter != null)
			{
				((ModuleDateFilter)filterStripBusinessObject[filterName]).Property1 = fromDateTimeFilter.Value;
			}

			if (toDateTimeFilter != null)
			{
				((ModuleDateFilter)filterStripBusinessObject[filterName]).Property2 = toDateTimeFilter.Value;
			}

			collection.Load(filterStripBusinessObject.Filter);
			AssertContainsExactElementsInAnyOrder
			(
				$"Filter: {filterName} {fromDateTimeFilter} > {toDateTimeFilter}",
				expected.Select(x => x.Quote.TH_QuoteNumber),
				collection.Select(x => x.QuotedBooking.Quote.TH_QuoteNumber)
			);
		}

		#endregion

		#endregion

		#region Numbers

		#region TestQuoteNo

		public void TestQuoteNo()
		{
			QuotedBooking quoteOnly1 = CreateQuoteOnly();
			QuotedBooking quoteOnly2 = CreateQuoteOnly();
			QuotedBooking quoteOnly3 = CreateQuoteOnly();
			QuotedBooking quotedBooking = CreateQuotedBooking();
			QuotedBooking bookingOnly = CreateBookingOnly();

			quoteOnly1.Quote.TH_QuoteNumber = "1010No";
			quoteOnly2.Quote.TH_QuoteNumber = "101010";
			quoteOnly3.Quote.TH_QuoteNumber = "101Nup";
			quotedBooking.Quote.TH_QuoteNumber = "101011";

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleNumberFilter)filter["Quote #"]).Property = "101010/A";
			((ModuleNumberFilter)filter["Quote #"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(new BusinessObjectFactory());
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View One Off Quote", 1, collection.Count);
			AssertEquals("Should have Quote 2 Quote", quoteOnly2.PK, collection[0].QuotedBooking.PK);

			((ModuleNumberFilter)filter["Quote #"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			((ModuleNumberFilter)filter["Quote #"]).Property = "";
			collection.Load(filter.Filter);

			var expectedPKs = new ZGuid[] { quoteOnly1.PK, quoteOnly2.PK, quoteOnly3.PK, quotedBooking.PK };
			AssertContainsExactElementsInAnyOrder(expectedPKs, collection.Select(element => element.PK));
		}

		public void TestQuoteNoForWeb()
		{
			//Web case
			Globals.IsWeb = true;
			try
			{
				QuotedBooking quoteOnly1 = CreateQuoteOnly();
				QuotedBooking quoteOnly2 = CreateQuoteOnly();
				QuotedBooking quoteOnly3 = CreateQuoteOnly();
				QuotedBooking quotedBooking = CreateQuotedBooking();
				QuotedBooking bookingOnly = CreateBookingOnly();
				QuotedBooking quotedBooking2 = CreateQuotedBooking();
				Factory.Save();

				var filter = new OneOffQuoteFilterStripBusinessObject
				{
					IsInWebQuoteMode = true
				};

				QuoteCollection webCollection = new QuoteCollection(Factory);
				webCollection.Load(filter.Filter);

				AssertEquals("Precondition: quote only", 3, webCollection.Count);

				Quote quote1 = CreateQuote();
				Quote quote2 = CreateQuote();
				Quote quote3 = CreateQuote();

				quote1.TH_QuoteNumber = "2020No";
				quote2.TH_QuoteNumber = "202020";
				quote3.TH_QuoteNumber = "202Nup";

				Factory.Save();

				((ModuleNumberFilter)filter["Quote #"]).Property = "202020/A";
				filter["Quote #"].IsActive = true;

				webCollection = new QuoteCollection(Factory);
				webCollection.Load(filter.Filter);

				// The query should filter quote number by using a column ViewQuotedBooking.VB_QuoteNumber instead of RatingHeader.TH_QuoteNumber
				AssertContains("VB_QuoteNumber", filter.Filter.FilterString);
				AssertNotContains("TH_QuoteNumber", filter.Filter.FilterString);

				AssertEquals("Should have loaded 1 Quote", 1, webCollection.Count);
				AssertEquals("Filtered quote should be the second one", quote2, webCollection[0]);
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		#endregion

		#region TestBookingNo

		public void TestBookingNo()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			QuotedBooking quotedBooking3 = CreateQuotedBooking();

			quotedBooking1.Booking.JS_UniqueConsignRef = "S1010No";
			quotedBooking2.Booking.JS_UniqueConsignRef = "S10101010";
			quotedBooking3.Booking.JS_UniqueConsignRef = "S1010Nup";

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleNumberFilter)filter["Booking #"]).Property = "S10101010";
			((ModuleNumberFilter)filter["Booking #"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 QuotedBooking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking2's Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking2's Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleNumberFilter)filter["Booking #"]).Property = "S10101011";
			collection.Load(filter.Filter);

			AssertEquals("Should not load any item", 0, collection.Count);

			QuotedBooking bookingOnly = CreateBookingOnly();
			QuotedBooking quoteOnly = CreateQuoteOnly();
			QuotedBooking quotedBooking4 = CreateQuotedBooking();
			Factory.Save();

			((ModuleNumberFilter)filter["Booking #"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			((ModuleNumberFilter)filter["Booking #"]).Property = "";
			collection.Load(filter.Filter);

			ZGuid[] expectedPKs = new ZGuid[] { quotedBooking1.PK, quotedBooking2.PK, quotedBooking3.PK, quotedBooking4.PK };
			AssertContainsExactElementsInAnyOrder(expectedPKs, collection.Select(element => element.PK));

			((ModuleNumberFilter)filter["Booking #"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 Quote", 1, collection.Count);
			AssertEquals("Should have quoteOnly's Quote", quoteOnly.Quote.PK, collection[0].QuotedBooking.Quote.PK);
		}

		#endregion

		#endregion

		#region Status And Flags

		#region TestStatus

		public void TestStatusFilterOptions()
		{
			var filter = GetNewFilterStripBusinessObject();
			var options = ((ModuleTextFilter)filter["Status"]).List.Cast<CodeDescriptionPair>().Select(pair => pair.Code);
			var expectedOptions = new[] { "Accepted", "Client Accepted", "Active", "Approved", "Finalized", "Canceled", "Expired", "Used" };
			AssertContainsExactElementsInAnyOrder(expectedOptions, options);
		}

		public void TestStatusFilter()
		{
			var today = ZDate.Today;
			var monthAgo = today.AddMonths(-1);
			var nextMonth = today.AddMonths(1);

			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var activeOOQ = CreateOneOffQuote(quoteNumber: "01", quoteDate: monthAgo, quoteEndDate: nextMonth);

			var activeOOQWithEmptyEndDate = CreateOneOffQuote(quoteNumber: "02", quoteDate: monthAgo, quoteEndDate: ZDate.Empty);

			var activeFinalisedOOQ = CreateOneOffQuote(quoteNumber: "03", quoteDate: monthAgo, quoteEndDate: nextMonth);
			activeFinalisedOOQ.Quote.TH_IsLocked = true;

			var expiredOOQ = CreateOneOffQuote(quoteNumber: "04", quoteDate: monthAgo, quoteEndDate: today.AddDays(-5));

			var acceptedOOQ = CreateOneOffQuote(quoteNumber: "05", quoteDate: monthAgo, quoteEndDate: null);
			acceptedOOQ.Quote.TH_Accepted = today;

			var clientAcceptedOOQ = CreateOneOffQuote(quoteNumber: "06", quoteDate: monthAgo, quoteEndDate: null);
			clientAcceptedOOQ.Quote.TH_ClientAccepted = today;

			var acceptedAndClientAcceptedOOQ = CreateOneOffQuote(quoteNumber: "07", quoteDate: monthAgo, quoteEndDate: null);
			acceptedAndClientAcceptedOOQ.Quote.TH_Accepted = today;
			acceptedAndClientAcceptedOOQ.Quote.TH_ClientAccepted = today.AddDays(-1);

			var cancelledOOQ = CreateOneOffQuote(quoteNumber: "08", quoteDate: monthAgo, quoteEndDate: null);
			cancelledOOQ.Quote.TH_IsCancelled = true;

			var acceptedCancelledOOQ = CreateOneOffQuote(quoteNumber: "09", quoteDate: monthAgo, quoteEndDate: null);
			acceptedCancelledOOQ.Quote.TH_IsCancelled = true;
			acceptedCancelledOOQ.Quote.TH_Accepted = today;

			var expiredCancelledOOQ = CreateOneOffQuote(quoteNumber: "10", quoteDate: monthAgo, quoteEndDate: today.AddDays(-5));
			expiredCancelledOOQ.Quote.TH_IsCancelled = true;

			var approvedOOQ = CreateOneOffQuote(quoteNumber: "11", quoteDate: monthAgo, quoteEndDate: nextMonth, approve: true);

			var consumedOOQ = CreateOneOffQuote(quoteNumber: "12", quoteDate: monthAgo, quoteEndDate: nextMonth, approve: true);
			consumedOOQ.Quote.TH_IsOneOffQuoteConsumed = true;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			var collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			((ModuleTextFilter)filter["Status"]).IsActive = true;

			AssertStatusFilter(filter, Quote.QuoteStatusOptions.Active, collection, expected: new[] { activeOOQ, activeFinalisedOOQ, activeOOQWithEmptyEndDate, approvedOOQ });
			AssertStatusFilter(filter, Quote.QuoteStatusOptions.Finalized, collection, expected: new[] { activeFinalisedOOQ });
			AssertStatusFilter(filter, Quote.QuoteStatusOptions.Accepted, collection, expected: new[] { acceptedOOQ, acceptedAndClientAcceptedOOQ });
			AssertStatusFilter(filter, Quote.QuoteStatusOptions.Expired, collection, expected: new[] { expiredOOQ });
			AssertStatusFilter(filter, Quote.QuoteStatusOptions.Approved, collection, expected: new[] { approvedOOQ });
			AssertStatusFilter(filter, Quote.QuoteStatusOptions.ClientAccepted, collection, expected: new[] { clientAcceptedOOQ });
			AssertStatusFilter(filter, Quote.QuoteStatusOptions.Used, collection, expected: new[] { consumedOOQ });

			((ModuleTextFilter)filter["Active Status"]).Property = OrgConstants.FilterControl.ActiveStatus.Code.InactiveClients; // this would set VB_IsCanceled = 1
			AssertStatusFilter(filter, Quote.QuoteStatusOptions.Cancelled, collection, expected: new[] { cancelledOOQ, expiredCancelledOOQ, acceptedCancelledOOQ });
		}

		QuotedBooking CreateOneOffQuote(ZString quoteNumber, ZDate quoteDate, ZDate? quoteEndDate, bool approve = false)
		{
			var oneOffQuote = CreateQuoteOnly(QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			oneOffQuote.Quote.TH_OneTimeQuote = true;
			oneOffQuote.Quote.TH_QuoteDate = quoteDate;

			if (quoteEndDate != null)
			{
				oneOffQuote.Quote.TH_QuoteEndDate = quoteEndDate.Value;
			}

			if (approve)
			{
				oneOffQuote.Quote.ShowApprovalDialog += ApprovedDelegate;
			}
			else
			{
				oneOffQuote.Quote.ShowApprovalDialog += NotApprovedDelegate;
			}

			oneOffQuote.Quote.TH_QuoteNumber = quoteNumber;

			return oneOffQuote;
		}

		static void AssertStatusFilter(FilterStripBusinessObject filterStripBusinessObject, string filterValue, ViewQuotedBookingCollection collection, QuotedBooking[] expected)
		{
			((ModuleTextFilter)filterStripBusinessObject["Status"]).Property = filterValue;
			collection.Load(filterStripBusinessObject.Filter);
			AssertContainsExactElementsInAnyOrder
			(
				$"Filter: {filterValue}",
				expected.Select(x => $"{x.Quote.TH_QuoteNumber} => {x.Quote.QuoteStatus}"),
				collection.Select(x => $"{x.QuotedBooking.Quote.TH_QuoteNumber} => {x.QuotedBooking.Quote.QuoteStatus}")
			);
		}

		void NotApprovedDelegate(object sender, Quote.ApprovalDialogEventArgs e)
		{
			e.Cancel = true;
		}

		void ApprovedDelegate(object sender, Quote.ApprovalDialogEventArgs e)
		{
		}

		#endregion

		#region TestActiveStatus

		public void TestActiveStatus()
		{
			QuotedBooking quoteOnly1 = CreateQuoteOnly();
			QuotedBooking quoteOnly2 = CreateQuoteOnly();
			QuotedBooking quoteOnly3 = CreateQuoteOnly();
			QuotedBooking quoteOnly4 = CreateQuoteOnly();
			QuotedBooking quotedBooking = CreateQuotedBooking();

			quoteOnly1.Quote.TH_IsCancelled = false;
			quoteOnly2.Quote.TH_IsCancelled = true;
			quoteOnly3.Quote.TH_IsCancelled = false;
			quoteOnly4.Quote.TH_IsCancelled = true;
			quotedBooking.Quote.TH_IsCancelled = true;

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleTextFilter)filter["Active Status"]).Property = OrgConstants.FilterControl.ActiveStatus.Code.ActiveClients;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(otherFactory);
			collection.Load(filter.Filter);

			var expected = new[] { quoteOnly1.PK, quoteOnly3.PK };

			AssertEquals("Should have loaded 2 View Quoted Bookings", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(expected, collection.Select(x => x.PK));

			((ModuleTextFilter)filter["Active Status"]).Property = OrgConstants.FilterControl.ActiveStatus.Code.InactiveClients;
			collection.Load(filter.Filter);

			expected = new[] { quoteOnly2.PK, quoteOnly4.PK, quotedBooking.PK };

			AssertEquals("Should have loaded View Quoted Booking", 3, collection.Count);
			AssertContainsExactElementsInAnyOrder(expected, collection.Select(x => x.PK));

			((ModuleTextFilter)filter["Active Status"]).Property = OrgConstants.FilterControl.ActiveStatus.Code.AllClients;
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded all View Quoted Booking", 5, collection.Count);

			((ModuleTextFilter)filter["Active Status"]).Property = ZString.Empty;
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded all View Quoted Booking", 5, collection.Count);
		}

		#endregion

		#region TestUsed

		public void TestUsedFilter()
		{
			var today = ZDate.Today;
			var monthAgo = today.AddMonths(-1);
			var nextMonth = today.AddMonths(1);

			var unconsumedOOQ = CreateOneOffQuote(quoteNumber: "1", quoteDate: monthAgo, quoteEndDate: nextMonth, approve: true);

			var consumedOOQ = CreateOneOffQuote(quoteNumber: "2", quoteDate: monthAgo, quoteEndDate: nextMonth, approve: true);
			consumedOOQ.Quote.TH_IsOneOffQuoteConsumed = true;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleTextFilter)filter["Used"]).IsActive = true;

			var collection = new ViewQuotedBookingCollection(Factory);

			AssertUsedFilter(filter, "Yes", collection, expected: new[] { consumedOOQ });
			AssertUsedFilter(filter, "No", collection, expected: new[] { unconsumedOOQ });
			AssertUsedFilter(filter, "Both", collection, expected: new[] { consumedOOQ, unconsumedOOQ });
		}

		static void AssertUsedFilter(FilterStripBusinessObject filterStripBusinessObject, string property, ViewQuotedBookingCollection collection, QuotedBooking[] expected)
		{
			((ModuleTextFilter)filterStripBusinessObject["Used"]).Property = property;
			collection.Load(filterStripBusinessObject.Filter);
			AssertContainsExactElementsInAnyOrder
			(
				$"Filter: {property}",
				expected.Select(x => $"{x.Quote.TH_QuoteNumber} => {x.Quote.TH_IsOneOffQuoteConsumed}"),
				collection.Select(x => $"{x.QuotedBooking.Quote.TH_QuoteNumber} => {x.QuotedBooking.Quote.TH_IsOneOffQuoteConsumed}")
			);
		}

		#endregion

		#region TestFinalPrint

		public void TestFinalPrint()
		{
			var lockedOOQ = CreateQuoteOnly();
			lockedOOQ.Quote.TH_QuoteNumber = "1";
			lockedOOQ.Quote.TH_IsLocked = true;

			var unlockedOOQ = CreateQuoteOnly();
			unlockedOOQ.Quote.TH_QuoteNumber = "2";

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleTextFilter)filter["Final Print"]).IsActive = true;

			var collection = new ViewQuotedBookingCollection(Factory);

			AssertFinalPrintFilter(filter, "Yes", collection, expected: new[] { lockedOOQ });
			AssertFinalPrintFilter(filter, "No", collection, expected: new[] { unlockedOOQ });
			AssertFinalPrintFilter(filter, "Both", collection, expected: new[] { lockedOOQ, unlockedOOQ });
		}

		static void AssertFinalPrintFilter(FilterStripBusinessObject filterStripBusinessObject, string value, ViewQuotedBookingCollection collection, QuotedBooking[] expected)
		{
			((ModuleTextFilter)filterStripBusinessObject["Final Print"]).Property = value;
			collection.Load(filterStripBusinessObject.Filter);
			AssertContainsExactElementsInAnyOrder
			(
				$"Filter: {value}",
				expected.Select(x => $"{x.Quote.TH_QuoteNumber} => {x.Quote.TH_IsLocked}"),
				collection.Select(x => $"{x.QuotedBooking.Quote.TH_QuoteNumber} => {x.QuotedBooking.Quote.TH_IsLocked}")
			);
		}

		#endregion

		#endregion

		#region Locations

		#region TestOriginDestination

		public void TestOriginDestinationForSpotQuote()
		{
			QuotedBooking qb1 = CreateQuoteOnly();
			QuotedBooking qb2 = CreateQuoteOnly();
			QuotedBooking qb3 = CreateQuoteOnly();

			qb1.Origin = "AUBNE";
			qb2.Destination = "AUSYD";
			qb3.Origin = "AUBNE";
			qb3.Destination = "AUSYD";

			Factory.Save();

			ViewQuotedBooking view1 = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, qb1.Quote.PK));
			ViewQuotedBooking view2 = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, qb2.Quote.PK));
			ViewQuotedBooking view3 = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, qb3.Quote.PK));

			var filter = GetNewFilterStripBusinessObject();
			((ModuleLocationFilter)filter["Origin / Destination"]).Property1 = "AUSYD";
			((ModuleLocationFilter)filter["Origin / Destination"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 0 View Quoted Booking", 0, collection.Count);

			((ModuleLocationFilter)filter["Origin / Destination"]).Property1 = "";
			((ModuleLocationFilter)filter["Origin / Destination"]).Property2 = "AUSYD";
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 2 View Quoted Booking", 2, collection.Count);
			AssertCollectionContains("Should have QuotedBooking 2 Quote", view2, collection);
			AssertCollectionContains("Should have QuotedBooking 2 Quote", view3, collection);

			((ModuleLocationFilter)filter["Origin / Destination"]).Property1 = "AUBNE";
			((ModuleLocationFilter)filter["Origin / Destination"]).Property2 = "";
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 2 View Quoted Booking", 2, collection.Count);
			AssertCollectionContains("Should have QuotedBooking 2 Quote", view1, collection);
			AssertCollectionContains("Should have QuotedBooking 2 Quote", view3, collection);

			((ModuleLocationFilter)filter["Origin / Destination"]).Property1 = "AU";
			((ModuleLocationFilter)filter["Origin / Destination"]).Property2 = "AU";
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertCollectionContains("Should have QuotedBooking 3 Quote", view3, collection);
		}

		#endregion

		#endregion

		#region Billing Filters

		public void TestAPInvoiceNumberFilter()
		{
			AssertNotNull(FilterStripBizO["AP Invoice #"]);

			QuotedBooking oneoffQuote1 = CreateQuoteOnly();
			QuotedBooking oneoffQuote2 = CreateQuoteOnly();
			QuotedBooking oneoffQuote3 = CreateQuoteOnly();
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateBookingOnly();

			AddTransactionLineToBusinessObject(oneoffQuote1.Job, LedgerTypes.AccountsPayable, "00001001");
			AddTransactionLineToBusinessObject(oneoffQuote2.Job, LedgerTypes.AccountsPayable, "00001002");
			AddTransactionLineToBusinessObject(oneoffQuote3.Job, LedgerTypes.AccountsPayable, "QUOTE001");

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStripBizO["AP Invoice #"];
			filter.IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(FilterStripBizO.Filter);

			ZGuid[] expected = new[] { oneoffQuote1.PK, oneoffQuote2.PK, oneoffQuote3.PK, quotedBooking1.PK };
			ZGuid[] actual = collection.Select(x => x.PK).ToArray();
			AssertContainsExactElementsInAnyOrder("Expected to return only One Off Quotes by default", expected, actual);

			filter.Property = "00001001";
			collection.Load(FilterStripBizO.Filter);

			Assert(collection.Contains(oneoffQuote1.PK));

			filter.Property = "00001002";
			collection.Load(FilterStripBizO.Filter);

			Assert(collection.Contains(oneoffQuote2.PK));

			filter.Property = "QUOTE001";
			collection.Load(FilterStripBizO.Filter);

			Assert(collection.Contains(oneoffQuote3.PK));

			filter.Property = "00001005";
			collection.Load(FilterStripBizO.Filter);

			Assert("There should not be any matching one off quotes", !collection.Any());

			filter.Property = "00001";
			collection.Load(FilterStripBizO.Filter);

			expected = new[] { oneoffQuote1.PK, oneoffQuote2.PK };
			actual = collection.Select(x => x.PK).ToArray();

			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestARTransactionNumberFilter()
		{
			AssertNotNull(FilterStripBizO["AR Transaction #"]);

			QuotedBooking oneOffQuote1 = CreateQuoteOnly();
			QuotedBooking oneOffQuote2 = CreateQuoteOnly();
			QuotedBooking oneOffQuote3 = CreateQuoteOnly();
			QuotedBooking quotedBooking4 = CreateQuotedBooking();
			QuotedBooking quotedBooking5 = CreateBookingOnly();

			AddTransactionLineToBusinessObject(oneOffQuote1.Job, LedgerTypes.AccountsReceivable, "00001003");
			AddTransactionLineToBusinessObject(oneOffQuote2.Job, LedgerTypes.AccountsReceivable, "00001004");
			AddTransactionLineToBusinessObject(oneOffQuote3.Job, LedgerTypes.AccountsReceivable, "QUOTE002");

			Factory.Save();

			ModuleFountainFilter filter = (ModuleFountainFilter)FilterStripBizO["AR Transaction #"];
			filter.IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(FilterStripBizO.Filter);

			ZGuid[] expected = new[] { oneOffQuote1.PK, oneOffQuote2.PK, oneOffQuote3.PK, quotedBooking4.PK };
			ZGuid[] actual = collection.Select(x => x.PK).ToArray();
			AssertContainsExactElementsInAnyOrder("Expected to return all quoted bookings by default", expected, actual);

			filter.Property = "00001003";
			collection.Load(FilterStripBizO.Filter);

			Assert(collection.Contains(oneOffQuote1.PK));

			filter.Property = "00001004";
			collection.Load(FilterStripBizO.Filter);

			Assert(collection.Contains(oneOffQuote2.PK));

			filter.Property = "QUOTE002";
			collection.Load(FilterStripBizO.Filter);

			Assert(collection.Contains(oneOffQuote3.PK));

			filter.Property = "00001005";
			collection.Load(FilterStripBizO.Filter);

			Assert("There should not be any matching quoted bookings", !collection.Any());
		}

		public void TestSupplierCostReferenceFilter()
		{
			AssertNotNull(FilterStripBizO["Supplier Cost Reference"]);

			QuotedBooking quotedBooking1 = CreateBookingOnly();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			QuotedBooking oneOffQuote1 = CreateQuoteOnly();
			QuotedBooking oneOffQuote2 = CreateQuoteOnly();
			QuotedBooking oneOffQuote3 = CreateQuoteOnly();

			JobHeader quotedBooking1Job = AddJobForBusinessObject(quotedBooking1.Booking);
			AddTransactionLineToBusinessObject(quotedBooking1Job, LedgerTypes.AccountsPayable, "00001001", "Pending");
			AddTransactionLineToBusinessObject(oneOffQuote1.Job, LedgerTypes.AccountsPayable, "00001002", "Pending tax");
			AddTransactionLineToBusinessObject(oneOffQuote3.Job, LedgerTypes.AccountsPayable, "00001004", "784-152");
			AddTransactionLineToBusinessObject(quotedBooking1Job, LedgerTypes.AccountsReceivable, "00002001", "BORK");
			AddTransactionLineToBusinessObject(oneOffQuote2.Job, LedgerTypes.AccountsReceivable, "00002003", "Pending");
			AddTransactionLineToBusinessObject(oneOffQuote3.Job, LedgerTypes.AccountsPayable, "00002004", "TEST");

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStripBizO["Supplier Cost Reference"];
			filter.IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(FilterStripBizO.Filter);

			ZGuid[] expected = new[] { oneOffQuote1.PK, oneOffQuote2.PK, oneOffQuote3.PK, quotedBooking2.PK };
			ZGuid[] actual = collection.Select(x => x.PK).ToArray();
			AssertContainsExactElementsInAnyOrder("Expected to return all one off quotes by default", expected, actual);

			filter.Property = "Pending";
			collection.Load(FilterStripBizO.Filter);

			expected = new[] { oneOffQuote1.PK, oneOffQuote2.PK };
			actual = collection.Select(x => x.PK).ToArray();
			AssertContainsExactElementsInAnyOrder(expected, actual);

			filter.Property = "Pending tax";
			collection.Load(FilterStripBizO.Filter);

			Assert(collection.Contains(oneOffQuote1.PK));

			filter.Property = "TEST";
			collection.Load(FilterStripBizO.Filter);

			Assert(collection.Contains(oneOffQuote3.PK));

			filter.Property = "invalid";
			collection.Load(FilterStripBizO.Filter);

			Assert("There should not be any matching quoted bookings", !collection.Any());
		}

		#endregion

		#region Organisations Staff

		#region TestSalesRep

		public void TestSalesRep()
		{
			QuotedBooking oneOffQuote1 = CreateQuoteOnly();
			QuotedBooking oneOffQuote2 = CreateQuoteOnly();

			oneOffQuote1.Job.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			oneOffQuote2.Job.JH_GS_NKRepSales = ZString.Empty;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			var expectedCollection = new ViewQuotedBookingCollection(Factory);
			expectedCollection.Add(ViewQuotedBooking.LoadOrCreate(oneOffQuote1));
			expectedCollection.Add(ViewQuotedBooking.LoadOrCreate(oneOffQuote2));

			AssertContainsExactElementsInAnyOrder("Should have loaded 2 View Quoted Booking", expectedCollection, collection);

			((ModuleNkFilter)filter["Sales Representative"]).Property = GlbStaff.CurrentUser.GS_Code;
			((ModuleNkFilter)filter["Sales Representative"]).IsActive = true;
			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);
			AssertEquals("Should only have one offquote1", oneOffQuote1.PK, collection[0].PK);
		}

		#endregion

		#region TestCarrier

		public void TestCarrier()
		{
			QuotedBooking quote1 = CreateQuoteOnly();
			QuotedBooking quote2 = CreateQuoteOnly();
			QuotedBooking quote3 = CreateQuoteOnly();

			quote1.Quote.CurrentOneOffQuote.TT_OH_Carrier = TestOrg.PK;
			quote2.Quote.CurrentOneOffQuote.TT_OH_Carrier = Factory.NewWithValidTestData<OrgHeader>().PK;
			quote3.Quote.CurrentOneOffQuote.TT_OH_Carrier = TestOrg.PK;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleGuidFilter)filter["Carrier"]).Property = TestOrg.PK;
			((ModuleGuidFilter)filter["Carrier"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			ZGuid[] expectedPKs = new ZGuid[]
			{
				quote1.PK,
				quote3.PK
			};
			ZGuid[] actualPKs = new ZGuid[]
			{
				collection[0].PK,
				collection[1].PK
			};

			AssertContainsExactElementsInAnyOrder("Should only contain quote1 and quote3", expectedPKs, actualPKs);
		}

		#endregion

		#region TestCreditor

		public void TestCreditor()
		{
			var quotedBooking1 = CreateQuoteOnly(TestOrg.PK);
			var quotedBooking2 = CreateQuoteOnly(TestOrg2.PK);

			quotedBooking1.Quote.CurrentOneOffQuote.TT_OH_Creditor = TestOrg.PK;
			quotedBooking2.Quote.CurrentOneOffQuote.TT_OH_Creditor = TestOrg2.PK;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleGuidFilter)filter["Creditor"]).Property = TestOrg.PK;
			((ModuleGuidFilter)filter["Creditor"]).IsActive = true;
			var collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.PK, collection[0].PK);

			filter = GetNewFilterStripBusinessObject();
			((ModuleGuidFilter)filter["Creditor"]).Property = TestOrg2.PK;
			((ModuleGuidFilter)filter["Creditor"]).IsActive = true;
			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 2 Quote", quotedBooking2.PK, collection[0].PK);

			filter = GetNewFilterStripBusinessObject();
			((ModuleGuidFilter)filter["Creditor"]).Property = TestOrg3.PK;
			((ModuleGuidFilter)filter["Creditor"]).IsActive = true;
			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 0 View Quoted Bookings", 0, collection.Count);
		}

		#endregion

		#region TestPotentialCarrier

		public void TestPotentialCarrier()
		{
			var quotedBooking1 = CreateQuoteOnlyWithPossibleCarrier(TestOrg.PK, new CarrierCreditorPair[]
			{
				new () { Carrier = TestOrg2.PK }
			});
			var quotedBooking2 = CreateQuoteOnlyWithPossibleCarrier(TestOrg2.PK, new CarrierCreditorPair[]
			{
				new () { Carrier = TestOrg.PK },
				new () { Carrier = TestOrg2.PK }
			});

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleGuidInSubCollectionFilter)filter["Potential Carrier"]).Property = TestOrg2.PK;
			((ModuleGuidInSubCollectionFilter)filter["Potential Carrier"]).IsActive = true;
			var collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 2 View Quoted Booking", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder("Should have QuotedBooking 1 and 2",
				new ZGuid[] { quotedBooking1.PK, quotedBooking2.PK },
				collection.Select((qb) => qb.PK));

			filter = GetNewFilterStripBusinessObject();
			((ModuleGuidInSubCollectionFilter)filter["Potential Carrier"]).Property = TestOrg.PK;
			((ModuleGuidInSubCollectionFilter)filter["Potential Carrier"]).IsActive = true;
			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 2 Quote", quotedBooking2.PK, collection[0].PK);

			filter = GetNewFilterStripBusinessObject();
			((ModuleGuidInSubCollectionFilter)filter["Potential Carrier"]).Property = TestOrg3.PK;
			((ModuleGuidInSubCollectionFilter)filter["Potential Carrier"]).IsActive = true;
			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 0 View Quoted Booking", 0, collection.Count);

			filter = GetNewFilterStripBusinessObject();
			((ModuleGuidInSubCollectionFilter)filter["Potential Carrier"]).Property = TestOrg.PK;
			((ModuleGuidInSubCollectionFilter)filter["Potential Carrier"]).IsActive = true;
			((ModuleGuidInSubCollectionFilter)filter["Potential Carrier"]).ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotContain;
			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.PK, collection[0].PK);
		}

		public void TestPotentialCreditor()
		{
			var quotedBooking1 = CreateQuoteOnlyWithPossibleCarrier(TestOrg.PK, new CarrierCreditorPair[]
			{
				new () { Carrier = TestOrg2.PK, Creditor = TestOrg2.PK }
			});
			var quotedBooking2 = CreateQuoteOnlyWithPossibleCarrier(TestOrg2.PK, new CarrierCreditorPair[]
			{
				new () { Carrier = TestOrg.PK, Creditor = TestOrg.PK },
				new () { Carrier = TestOrg2.PK, Creditor = TestOrg2.PK },
			});

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleGuidInSubCollectionFilter)filter["Potential Creditor"]).Property = TestOrg2.PK;
			((ModuleGuidInSubCollectionFilter)filter["Potential Creditor"]).IsActive = true;
			var collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 2 View Quoted Booking", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder("Should have QuotedBooking 1 and 2",
				new ZGuid[] { quotedBooking1.PK, quotedBooking2.PK },
				collection.Select((qb) => qb.PK));

			filter = GetNewFilterStripBusinessObject();
			((ModuleGuidInSubCollectionFilter)filter["Potential Creditor"]).Property = TestOrg.PK;
			((ModuleGuidInSubCollectionFilter)filter["Potential Creditor"]).IsActive = true;
			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 2 Quote", quotedBooking2.PK, collection[0].PK);

			filter = GetNewFilterStripBusinessObject();
			((ModuleGuidInSubCollectionFilter)filter["Potential Creditor"]).Property = TestOrg3.PK;
			((ModuleGuidInSubCollectionFilter)filter["Potential Creditor"]).IsActive = true;
			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 0 View Quoted Booking", 0, collection.Count);

			filter = GetNewFilterStripBusinessObject();
			((ModuleGuidInSubCollectionFilter)filter["Potential Creditor"]).Property = TestOrg.PK;
			((ModuleGuidInSubCollectionFilter)filter["Potential Creditor"]).IsActive = true;
			((ModuleGuidInSubCollectionFilter)filter["Potential Creditor"]).ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotContain;
			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.PK, collection[0].PK);
		}

		#endregion

		#region TestClient

		public void TestClient()
		{
			QuotedBooking oneOffQuote1 = CreateQuoteOnly(TestOrg.PK);
			QuotedBooking oneOffQuote2 = CreateQuoteOnly(TestOrg2.PK);

			oneOffQuote1.Job.LocalChargesPK = TestOrg.PK;
			oneOffQuote2.Quote.TH_OH = TestOrg2.PK;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleGuidFilter)filter["Client"]).Property = TestOrg.PK;
			((ModuleGuidFilter)filter["Client"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", oneOffQuote1.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			((ModuleGuidFilter)filter["Client"]).Property = TestOrg2.PK;

			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 2 Quote", oneOffQuote2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
		}

		#endregion

		#region TestClientName

		public void TestClientNameUseEqual()
		{
			var data = GetClientNameData();

			var property = "Test Client #1";
			var message = "Should have loaded 1 View Quoted Booking";
			var expected = data["Client1"].Where(x => x.Quote != null);

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.Equal, message, expected);

			property = "Test Client #2";
			expected = data["Client2"].Where(x => x.Quote != null);
			message = "Should have loaded 2 View Quoted Bookings";

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.Equal, message, expected);

			property = "Test Client";
			message = "Should have loaded nothing.";
			expected = Array.Empty<QuotedBooking>();

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.Equal, message, expected);

			property = string.Empty;
			message = "Should have loaded 2 View Quoted Bookings";
			expected = data.Values.SelectMany(vale => vale).Where(x => x.Quote != null).ToArray();

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.Equal, message, expected);
		}

		public void TestClientNameUseNotEqual()
		{
			var data = GetClientNameData();

			var property = "Test Client #1";
			var message = "Should have loaded 1 View Quoted Bookings";
			var expected = data["Client2"].Union(data["Empty"]).Where(x => x.Quote != null);

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.NotEqual, message, expected);

			property = "Test Client #2";
			expected = data["Client1"].Union(data["Empty"]).Where(x => x.Quote != null);

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.NotEqual, message, expected);

			property = "Test Client";
			message = "Should have loaded 2 View Quoted Bookings";
			expected = data.Values.SelectMany(vale => vale).Where(x => x.Quote != null).ToArray();

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.NotEqual, message, expected);

			property = string.Empty;
			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.NotEqual, message, expected);
		}

		public void TestClientNameUseContainsAndStartWith()
		{
			var data = GetClientNameData();

			var property = "Test Client #1";
			var message = "Should have loaded 1 View Quoted Booking";
			var expected = data["Client1"].Where(x => x.Quote != null);

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.Contains, message, expected);
			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.StartsWith, message, expected);

			property = "Test Client #2";
			expected = data["Client2"].Where(x => x.Quote != null);

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.Contains, message, expected);
			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.StartsWith, message, expected);

			property = "Test Client";
			message = "Should have loaded 2 View Quoted Bookings";
			expected = data["Client1"].Union(data["Client2"]).Where(x => x.Quote != null);

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.Contains, message, expected);
			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.StartsWith, message, expected);

			property = "AnyAddress";
			message = "Should have loaded nothing.";
			expected = Array.Empty<QuotedBooking>();

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.Contains, message, expected);
			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.StartsWith, message, expected);
		}

		public void TestClientNameUseNotContainsAndDoesNotStartWith()
		{
			var data = GetClientNameData();

			var property = "Test Client #1";
			var message = "Should have loaded 1 View Quoted Booking";
			var expected = data["Client2"].Union(data["Empty"]).Where(x => x.Quote != null);

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.NotContains, message, expected);
			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.DoesNotStartWith, message, expected);

			property = "Test Client #2";
			expected = data["Client1"].Union(data["Empty"]).Where(x => x.Quote != null);

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.NotContains, message, expected);
			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.DoesNotStartWith, message, expected);

			property = "Test Client";
			expected = data["Empty"];

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.NotContains, message, expected);
			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.DoesNotStartWith, message, expected);

			property = "AnyAddress";
			expected = data.Values.SelectMany(vale => vale).Where(x => x.Quote != null);
			message = "Should have loaded 2 View Quoted Bookings";

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.NotContains, message, expected);
			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.DoesNotStartWith, message, expected);
		}

		public void TestClientNameUseIsBlankAndIsNotBlank()
		{
			var data = GetClientNameData();

			var message = "Should have loaded 0 View Quoted Bookings";
			var expected = data["Empty"].Where(x => x.Quote != null);

			CheckCollectionForUseClientNameFilter(string.Empty, SpecialComparisonOperator.IsBlank, message, expected);

			message = "Should have loaded 2 View Quoted Bookings";
			expected = data["Client1"].Union(data["Client2"]).Where(x => x.Quote != null);

			CheckCollectionForUseClientNameFilter(string.Empty, SpecialComparisonOperator.IsNotBlank, message, expected);
		}

		#endregion

		#region TestConsignorConsignee

		public void TestConsignorConsignee()
		{
			var originalActiveStatus = TestOrg3.OH_IsActive;

			QuotedBooking oneOffQuote1 = CreateQuoteOnly();
			QuotedBooking oneOffQuote2 = CreateQuoteOnly();
			var oneOffQuote3 = CreateQuoteOnly();
			oneOffQuote1.Quote.CurrentOneOffQuote.DeliveryDocAddress.E2_OA_Address = TestOrg.MainAddress.PK;
			oneOffQuote2.Quote.CurrentOneOffQuote.PickUpDocAddress.Address.OA_OH = TestOrg2.PK;
			TestOrg3.OH_IsActive = false;
			oneOffQuote3.Quote.CurrentOneOffQuote.DeliveryDocAddress.E2_OA_Address = TestOrg3.MainAddress.PK;
			oneOffQuote3.Quote.CurrentOneOffQuote.PickUpDocAddress.Address.OA_OH = TestOrg3.PK;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleGuidsFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.OrganisationsStaff.ConsignorConsignee]).Property2 = TestOrg.PK;
			((ModuleGuidsFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.OrganisationsStaff.ConsignorConsignee]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", oneOffQuote1.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			((ModuleGuidsFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.OrganisationsStaff.ConsignorConsignee]).Property1 = TestOrg2.PK;
			((ModuleGuidsFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.OrganisationsStaff.ConsignorConsignee]).Property2 = ZGuid.Empty;
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", oneOffQuote2.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			AssertNoWarning(((ModuleGuidsFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.OrganisationsStaff.ConsignorConsignee]).Property1Info, "Organization is in-active.");
			AssertNoWarning(((ModuleGuidsFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.OrganisationsStaff.ConsignorConsignee]).Property2Info, "Organization is in-active.");
			((ModuleGuidsFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.OrganisationsStaff.ConsignorConsignee]).Property1 = TestOrg3.PK;
			((ModuleGuidsFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.OrganisationsStaff.ConsignorConsignee]).Property2 = TestOrg3.PK;
			collection.Load(filter.Filter);
			AssertHasWarning(((ModuleGuidsFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.OrganisationsStaff.ConsignorConsignee]).Property1Info, "Organization is in-active.");
			AssertHasWarning(((ModuleGuidsFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.OrganisationsStaff.ConsignorConsignee]).Property2Info, "Organization is in-active.");
			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 3 Quote", oneOffQuote3.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			filter = new OneOffQuoteFilterStripBusinessObject();
			AssertEquals("Consignor / Consignee", ((ModuleGuidsFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.OrganisationsStaff.ConsignorConsignee]).MultilingualDescription);

			FreightDataRegistry.Instance.ConsignorShipperTerminology.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (NoResString)"Sender");
			filter = new OneOffQuoteFilterStripBusinessObject();
			AssertEquals("Sender / Consignee", ((ModuleGuidsFilter)filter[QuotedBookingFilterStripBusinessObject.Descriptions.OrganisationsStaff.ConsignorConsignee]).MultilingualDescription);

			TestOrg3.OH_IsActive = originalActiveStatus;
		}

		#endregion

		#region Test Related Parties Filters

		public void TestConsignorRelatedPartiesFilter()
		{
			OrgHeader con1 = GetOrgHeader("con1");
			OrgHeader con2 = GetOrgHeader("con2");
			OrgHeader empty = GetOrgHeader("empty");

			var oneOffQuote1 = GetOneOffQuoteWithConsignor(con1);
			var oneOffQuote2 = GetOneOffQuoteWithConsignor(con2);
			var emptyQuote = GetOneOffQuoteWithConsignor(empty);

			OrgHeader party1 = GetOrgHeader("party1");
			OrgHeader party2 = GetOrgHeader("party2");
			OrgHeader party3 = GetOrgHeader("party3");
			OrgHeader emptyParty = GetOrgHeader("empty");

			OrgRelatedParty relatedParty1 = GetOrgRelatedParty(con1, party1);
			OrgRelatedParty relatedParty2 = GetOrgRelatedParty(con1, party2);
			OrgRelatedParty relatedParty3 = GetOrgRelatedParty(con2, party2);
			OrgRelatedParty relatedParty4 = GetOrgRelatedParty(con2, party3);

			OrgHeader consignor1 = GetOrgHeader("consignor1");
			OrgHeader consignor2 = GetOrgHeader("consignor2");
			OrgHeader consignor3 = GetOrgHeader("consignor3");
			OrgHeader consignor4 = GetOrgHeader("consignor4");
			OrgHeader consignor5 = GetOrgHeader("consignor5");
			OrgHeader consignor6 = GetOrgHeader("consignor6");

			var newOneOffQuote1 = GetOneOffQuoteWithConsignor(consignor1);
			var newOneOffQuote2 = GetOneOffQuoteWithConsignor(consignor2);
			var newOneOffQuote3 = GetOneOffQuoteWithConsignor(consignor3);
			var newOneOffQuote4 = GetOneOffQuoteWithConsignor(consignor4);
			var newOneOffQuote5 = GetOneOffQuoteWithConsignor(consignor5);
			var newOneOffQuote6 = GetOneOffQuoteWithConsignor(consignor6);

			Asserter.AddToScope(oneOffQuote1);
			Asserter.AddToScope(oneOffQuote2);
			Asserter.AddToScope(emptyQuote);
			Asserter.AddToScope(newOneOffQuote1);
			Asserter.AddToScope(newOneOffQuote2);
			Asserter.AddToScope(newOneOffQuote3);
			Asserter.AddToScope(newOneOffQuote4);
			Asserter.AddToScope(newOneOffQuote5);
			Asserter.AddToScope(newOneOffQuote6);

			OrgHeader newParty = GetOrgHeader("newParty");

			OrgRelatedParty newRelatedParty1 = GetOrgRelatedParty(consignor1, newParty);
			OrgRelatedParty newRelatedParty2 = GetOrgRelatedParty(consignor2, newParty);
			OrgRelatedParty newRelatedParty3 = GetOrgRelatedParty(consignor3, newParty);
			OrgRelatedParty newRelatedParty4 = GetOrgRelatedParty(consignor4, newParty);
			OrgRelatedParty newRelatedParty5 = GetOrgRelatedParty(consignor5, newParty);
			OrgRelatedParty newRelatedParty6 = GetOrgRelatedParty(consignor6, newParty);

			newRelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;

			newRelatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;

			newRelatedParty3.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty3.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;

			newRelatedParty4.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty4.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;

			newRelatedParty5.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty5.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty5.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			newRelatedParty5.PR_FreightContainerMode = Core.Constants.ContainerModes.FCL;

			newRelatedParty6.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty6.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty6.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			newRelatedParty6.PR_FreightContainerMode = Core.Constants.ContainerModes.LCL;

			Factory.Save();

			OrgRelatedPartiesModuleFilter filter = (OrgRelatedPartiesModuleFilter)FilterStripBizO["Consignor Related Parties"];

			Asserter.AssertMatches("Empty Filter", filter, oneOffQuote1, oneOffQuote2, emptyQuote, newOneOffQuote1, newOneOffQuote2, newOneOffQuote3, newOneOffQuote4, newOneOffQuote5, newOneOffQuote6);

			filter.RelatedParty = party1.PK;
			Asserter.AssertMatches("party1", filter, oneOffQuote1);

			filter.RelatedParty = party2.PK;
			Asserter.AssertMatches("party2", filter, oneOffQuote1, oneOffQuote2);

			filter.RelatedParty = party3.PK;
			Asserter.AssertMatches("party3", filter, oneOffQuote2);

			filter.RelatedParty = emptyParty.PK;
			Asserter.AssertMatches("empty", filter);

			filter.RelatedParty = newParty.PK;
			Asserter.AssertMatches("newParty", filter, newOneOffQuote1, newOneOffQuote2, newOneOffQuote3, newOneOffQuote4, newOneOffQuote5, newOneOffQuote6);

			filter.PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			Asserter.AssertMatches("PartyType = APNettingGroup", filter, newOneOffQuote1);

			filter.PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			Asserter.AssertMatches("PartyType = APSettlementGroup", filter, newOneOffQuote2, newOneOffQuote3, newOneOffQuote4, newOneOffQuote5, newOneOffQuote6);

			filter.Direction = RelatedPartyDirectionList.Codes.Delivery;
			Asserter.AssertMatches("Direction = Delivery", filter, newOneOffQuote3);

			filter.Direction = RelatedPartyDirectionList.Codes.Pickup;
			Asserter.AssertMatches("Direction = Pickup", filter, newOneOffQuote4, newOneOffQuote5, newOneOffQuote6);

			filter.TransportMode = Core.Constants.TransportModes.Sea;
			filter.ContainerMode = Core.Constants.ContainerModes.FCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = FCL", filter, newOneOffQuote5);

			filter.TransportMode = Core.Constants.TransportModes.Sea;
			filter.ContainerMode = Core.Constants.ContainerModes.LCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = LCL", filter, newOneOffQuote6);
		}

		public void TestConsigneeRelatedPartiesFilter()
		{
			OrgHeader con1 = GetOrgHeader("con1");
			OrgHeader con2 = GetOrgHeader("con2");
			OrgHeader empty = GetOrgHeader("empty");

			var oneOffQuote1 = GetOneOffQuoteWithConsignee(con1);
			var oneOffQuote2 = GetOneOffQuoteWithConsignee(con2);
			var emptyQuote = GetOneOffQuoteWithConsignee(empty);

			OrgHeader party1 = GetOrgHeader("party1");
			OrgHeader party2 = GetOrgHeader("party2");
			OrgHeader party3 = GetOrgHeader("party3");
			OrgHeader emptyParty = GetOrgHeader("empty");

			OrgRelatedParty relatedParty1 = GetOrgRelatedParty(con1, party1);
			OrgRelatedParty relatedParty2 = GetOrgRelatedParty(con1, party2);
			OrgRelatedParty relatedParty3 = GetOrgRelatedParty(con2, party2);
			OrgRelatedParty relatedParty4 = GetOrgRelatedParty(con2, party3);

			OrgHeader consignee1 = GetOrgHeader("consignee1");
			OrgHeader consignee2 = GetOrgHeader("consignee2");
			OrgHeader consignee3 = GetOrgHeader("consignee3");
			OrgHeader consignee4 = GetOrgHeader("consignee4");
			OrgHeader consignee5 = GetOrgHeader("consignee5");
			OrgHeader consignee6 = GetOrgHeader("consignee6");

			var newOneOffQuote1 = GetOneOffQuoteWithConsignee(consignee1);
			var newOneOffQuote2 = GetOneOffQuoteWithConsignee(consignee2);
			var newOneOffQuote3 = GetOneOffQuoteWithConsignee(consignee3);
			var newOneOffQuote4 = GetOneOffQuoteWithConsignee(consignee4);
			var newOneOffQuote5 = GetOneOffQuoteWithConsignee(consignee5);
			var newOneOffQuote6 = GetOneOffQuoteWithConsignee(consignee6);

			Asserter.AddToScope(oneOffQuote1);
			Asserter.AddToScope(oneOffQuote2);
			Asserter.AddToScope(emptyQuote);
			Asserter.AddToScope(newOneOffQuote1);
			Asserter.AddToScope(newOneOffQuote2);
			Asserter.AddToScope(newOneOffQuote3);
			Asserter.AddToScope(newOneOffQuote4);
			Asserter.AddToScope(newOneOffQuote5);
			Asserter.AddToScope(newOneOffQuote6);

			OrgHeader newParty = GetOrgHeader("newParty");

			OrgRelatedParty newRelatedParty1 = GetOrgRelatedParty(consignee1, newParty);
			OrgRelatedParty newRelatedParty2 = GetOrgRelatedParty(consignee2, newParty);
			OrgRelatedParty newRelatedParty3 = GetOrgRelatedParty(consignee3, newParty);
			OrgRelatedParty newRelatedParty4 = GetOrgRelatedParty(consignee4, newParty);
			OrgRelatedParty newRelatedParty5 = GetOrgRelatedParty(consignee5, newParty);
			OrgRelatedParty newRelatedParty6 = GetOrgRelatedParty(consignee6, newParty);

			newRelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;

			newRelatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;

			newRelatedParty3.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty3.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;

			newRelatedParty4.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty4.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;

			newRelatedParty5.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty5.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty5.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			newRelatedParty5.PR_FreightContainerMode = Core.Constants.ContainerModes.FCL;

			newRelatedParty6.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty6.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty6.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			newRelatedParty6.PR_FreightContainerMode = Core.Constants.ContainerModes.LCL;

			Factory.Save();

			OrgRelatedPartiesModuleFilter filter = (OrgRelatedPartiesModuleFilter)FilterStripBizO["Consignee Related Parties"];

			Asserter.AssertMatches("Empty Filter", filter, oneOffQuote1, oneOffQuote2, emptyQuote, newOneOffQuote1, newOneOffQuote2, newOneOffQuote3, newOneOffQuote4, newOneOffQuote5, newOneOffQuote6);

			filter.RelatedParty = party1.PK;
			Asserter.AssertMatches("party1", filter, oneOffQuote1);

			filter.RelatedParty = party2.PK;
			Asserter.AssertMatches("party2", filter, oneOffQuote1, oneOffQuote2);

			filter.RelatedParty = party3.PK;
			Asserter.AssertMatches("party3", filter, oneOffQuote2);

			filter.RelatedParty = emptyParty.PK;
			Asserter.AssertMatches("empty", filter);

			filter.RelatedParty = newParty.PK;
			Asserter.AssertMatches("newParty", filter, newOneOffQuote1, newOneOffQuote2, newOneOffQuote3, newOneOffQuote4, newOneOffQuote5, newOneOffQuote6);

			filter.PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			Asserter.AssertMatches("PartyType = APNettingGroup", filter, newOneOffQuote1);

			filter.PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			Asserter.AssertMatches("PartyType = APSettlementGroup", filter, newOneOffQuote2, newOneOffQuote3, newOneOffQuote4, newOneOffQuote5, newOneOffQuote6);

			filter.Direction = RelatedPartyDirectionList.Codes.Delivery;
			Asserter.AssertMatches("Direction = Delivery", filter, newOneOffQuote3);

			filter.Direction = RelatedPartyDirectionList.Codes.Pickup;
			Asserter.AssertMatches("Direction = Pickup", filter, newOneOffQuote4, newOneOffQuote5, newOneOffQuote6);

			filter.TransportMode = Core.Constants.TransportModes.Sea;
			filter.ContainerMode = Core.Constants.ContainerModes.FCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = FCL", filter, newOneOffQuote5);

			filter.TransportMode = Core.Constants.TransportModes.Sea;
			filter.ContainerMode = Core.Constants.ContainerModes.LCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = LCL", filter, newOneOffQuote6);
		}

		public void TestLocalClientRelatedPartiesFilter()
		{
			var quotedBooking1 = GetQuotedBookingWithClient("client1");
			var quotedBooking2 = GetQuotedBookingWithClient("client2");
			var emptyQuotedBooking = GetQuotedBooking("Empty");

			OrgHeader party1 = GetOrgHeader("party1");
			OrgHeader party2 = GetOrgHeader("party2");
			OrgHeader party3 = GetOrgHeader("party3");
			OrgHeader emptyParty = GetOrgHeader("empty");

			OrgRelatedParty relatedParty1 = GetOrgRelatedParty(quotedBooking1.QuotedBooking.Job.LocalCharges, party1);
			OrgRelatedParty relatedParty2 = GetOrgRelatedParty(quotedBooking1.QuotedBooking.Job.LocalCharges, party2);
			OrgRelatedParty relatedParty3 = GetOrgRelatedParty(quotedBooking2.QuotedBooking.Job.LocalCharges, party2);
			OrgRelatedParty relatedParty4 = GetOrgRelatedParty(quotedBooking2.QuotedBooking.Job.LocalCharges, party3);

			var newQuotedBooking1 = GetQuotedBookingWithClient("newClient1");
			var newQuotedBooking2 = GetQuotedBookingWithClient("newClient2");
			var newQuotedBooking3 = GetQuotedBookingWithClient("newClient3");
			var newQuotedBooking4 = GetQuotedBookingWithClient("newClient4");
			var newQuotedBooking5 = GetQuotedBookingWithClient("newClient5");
			var newQuotedBooking6 = GetQuotedBookingWithClient("newClient6");

			OrgHeader newParty = GetOrgHeader("newParty");

			OrgRelatedParty newRelatedParty1 = GetOrgRelatedParty(newQuotedBooking1.QuotedBooking.Job.LocalCharges, newParty);
			OrgRelatedParty newRelatedParty2 = GetOrgRelatedParty(newQuotedBooking2.QuotedBooking.Job.LocalCharges, newParty);
			OrgRelatedParty newRelatedParty3 = GetOrgRelatedParty(newQuotedBooking3.QuotedBooking.Job.LocalCharges, newParty);
			OrgRelatedParty newRelatedParty4 = GetOrgRelatedParty(newQuotedBooking4.QuotedBooking.Job.LocalCharges, newParty);
			OrgRelatedParty newRelatedParty5 = GetOrgRelatedParty(newQuotedBooking5.QuotedBooking.Job.LocalCharges, newParty);
			OrgRelatedParty newRelatedParty6 = GetOrgRelatedParty(newQuotedBooking6.QuotedBooking.Job.LocalCharges, newParty);

			newRelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;

			newRelatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;

			newRelatedParty3.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty3.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;

			newRelatedParty4.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty4.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;

			newRelatedParty5.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty5.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty5.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			newRelatedParty5.PR_FreightContainerMode = Core.Constants.ContainerModes.FCL;

			newRelatedParty6.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty6.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty6.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			newRelatedParty6.PR_FreightContainerMode = Core.Constants.ContainerModes.LCL;

			Factory.Save();

			OrgRelatedPartiesModuleFilter filter = (OrgRelatedPartiesModuleFilter)FilterStripBizO["Client Related Parties"];

			Asserter.AssertMatches("Empty Filter", filter, quotedBooking1, quotedBooking2, emptyQuotedBooking, newQuotedBooking1, newQuotedBooking2, newQuotedBooking3, newQuotedBooking4, newQuotedBooking5, newQuotedBooking6);

			filter.RelatedParty = party1.PK;
			Asserter.AssertMatches("party1", filter, quotedBooking1);

			filter.RelatedParty = party2.PK;
			Asserter.AssertMatches("party2", filter, quotedBooking1, quotedBooking2);

			filter.RelatedParty = party3.PK;
			Asserter.AssertMatches("party3", filter, quotedBooking2);

			filter.RelatedParty = emptyParty.PK;
			Asserter.AssertMatches("empty", filter);

			filter.RelatedParty = newParty.PK;
			Asserter.AssertMatches("newParty", filter, newQuotedBooking1, newQuotedBooking2, newQuotedBooking3, newQuotedBooking4, newQuotedBooking5, newQuotedBooking6);

			filter.PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			Asserter.AssertMatches("PartyType = APNettingGroup", filter, newQuotedBooking1);

			filter.PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			Asserter.AssertMatches("PartyType = APSettlementGroup", filter, newQuotedBooking2, newQuotedBooking3, newQuotedBooking4, newQuotedBooking5, newQuotedBooking6);

			filter.Direction = RelatedPartyDirectionList.Codes.Delivery;
			Asserter.AssertMatches("Direction = Delivery", filter, newQuotedBooking3);

			filter.Direction = RelatedPartyDirectionList.Codes.Pickup;
			Asserter.AssertMatches("Direction = Pickup", filter, newQuotedBooking4, newQuotedBooking5, newQuotedBooking6);

			filter.TransportMode = Core.Constants.TransportModes.Sea;
			filter.ContainerMode = Core.Constants.ContainerModes.FCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = FCL", filter, newQuotedBooking5);

			filter.TransportMode = Core.Constants.TransportModes.Sea;
			filter.ContainerMode = Core.Constants.ContainerModes.LCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = LCL", filter, newQuotedBooking6);
		}

		#endregion

		#region TestBranch

		public void TestBranch()
		{
			var oneOffQuote1 = CreateQuoteOnly();
			var oneOffQuote2 = CreateQuoteOnly();

			var gb1 = Factory.NewWithValidTestData<GlbBranch>();
			gb1.GB_IsActive = true;
			var gb2 = Factory.NewWithValidTestData<GlbBranch>();
			gb2.GB_IsActive = true;
			var gb3 = Factory.NewWithValidTestData<GlbBranch>();
			gb3.GB_IsActive = true;

			oneOffQuote1.Job.JH_GB = gb1.PK;
			oneOffQuote2.Job.JH_GB = gb2.PK;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();

			((ModuleGuidFilter)filter["Branch"]).IsActive = true;
			((ModuleGuidFilter)filter["Branch"]).Property = ZGuid.Empty;
			var collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded all 2 View Quoted Bookings",2, collection.Count);

			((ModuleGuidFilter)filter["Branch"]).Property = gb1.PK;
			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", oneOffQuote1.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			((ModuleGuidFilter)filter["Branch"]).Property = gb2.PK;
			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 2 Quote", oneOffQuote2.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			((ModuleGuidFilter)filter["Branch"]).Property = gb3.PK;
			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("Should not have loaded any View Quoted Booking", 0, collection.Count);
		}

		#endregion

		#region TestOperationsRep

		public void TestOperationsRep()
		{
			var oneOffQuote1 = CreateQuoteOnly();
			var oneOffQuote2 = CreateQuoteOnly();

			var newStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			newStaff1.GS_Code = "ST1";
			newStaff1.GS_IsActive = true;
			var newStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			newStaff2.GS_Code = "ST2";
			newStaff2.GS_IsActive = true;
			var newStaff3 = Factory.NewWithValidTestData<GlbStaff>();
			newStaff3.GS_Code = "ST3";
			newStaff3.GS_IsActive = true;

			oneOffQuote1.Job.JH_GS_NKRepOps = newStaff1.GS_Code;
			oneOffQuote2.Job.JH_GS_NKRepOps = newStaff2.GS_Code;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();

			((ModuleNkFilter)filter["Operations Representative"]).IsActive = true;
			((ModuleNkFilter)filter["Operations Representative"]).Property = ZString.Empty;
			var collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded all 2 View Quoted Bookings", 2, collection.Count);

			((ModuleNkFilter)filter["Operations Representative"]).Property = newStaff1.GS_Code;
			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", oneOffQuote1.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			((ModuleNkFilter)filter["Operations Representative"]).Property = newStaff2.GS_Code;
			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 2 Quote", oneOffQuote2.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			((ModuleNkFilter)filter["Operations Representative"]).Property = newStaff3.GS_Code;
			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("Should not have loaded any View Quoted Booking", 0, collection.Count);
		}

		#endregion

		#region TestContact

		public void TestConsignorContact()
		{
			TestContactFilter(ContactType.ConsignorContact);
		}

		public void TestConsigneeContact()
		{
			TestContactFilter(ContactType.ConsigneeContact);
		}

		public void TestClientContact()
		{
			TestContactFilter(ContactType.ClientContact);
		}

		enum ContactType
		{
			ConsignorContact,
			ConsigneeContact,
			ClientContact,
		}

		void TestContactFilter(ContactType contactType)
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "test1";
			org1.OH_RL_NKClosestPort = "AUSYD";
			var contact = org1.Contacts.AddNew();
			contact.OC_ContactName = "test1contact";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "test2";
			org2.OH_RL_NKClosestPort = "AUSYD";

			var oneOffQuote1 = CreateQuoteOnly();
			oneOffQuote1.Quote.TH_QuoteNumber = "1";

			var oneOffQuote2 = CreateQuoteOnly();
			oneOffQuote2.Quote.TH_QuoteNumber = "2";

			var filterName = "";

			switch (contactType)
			{
				case ContactType.ConsignorContact:
					SetupContactAddress(oneOffQuote1, org1, contactType, "test1contact", false);
					SetupContactAddress(oneOffQuote2, org2, contactType, "test2contact", true);
					filterName = "Consignor Contact";
					break;
				case ContactType.ConsigneeContact:
					SetupContactAddress(oneOffQuote1, org1, contactType, "test1contact", false);
					SetupContactAddress(oneOffQuote2, org2, contactType, "test2contact", true);
					filterName = "Consignee Contact";
					break;
				case ContactType.ClientContact:
					SetupContactAddress(oneOffQuote1, org1, contactType, "test1contact", false);
					SetupContactAddress(oneOffQuote2, org2, contactType, "test2contact", true);
					filterName = "Client Contact";
					break;
				default:
					throw new NotSupportedException($"contactType '{contactType}' is not supported.");
			}

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleTextFilter)filter[filterName]).IsActive = true;

			var collection = new ViewQuotedBookingCollection(Factory);

			AssertContactFilter(filter, filterName: filterName, property: "test1contact", comparisonOperator: SQLComparisonOperator.Equal, collection, expected: new[] { oneOffQuote1 });
			AssertContactFilter(filter, filterName: filterName, property: "test2contact", comparisonOperator: SQLComparisonOperator.Equal, collection, expected: new[] { oneOffQuote2 });
			AssertContactFilter(filter, filterName: filterName, property: "test3contact", comparisonOperator: SQLComparisonOperator.Equal, collection, expected: Array.Empty<QuotedBooking>());

			AssertContactFilter(filter, filterName: filterName, property: "test1contact", comparisonOperator: SQLComparisonOperator.NotEqual, collection, expected: new[] { oneOffQuote2 });
			AssertContactFilter(filter, filterName: filterName, property: "test2contact", comparisonOperator: SQLComparisonOperator.NotEqual, collection, expected: new[] { oneOffQuote1 });
			AssertContactFilter(filter, filterName: filterName, property: "test3contact", comparisonOperator: SQLComparisonOperator.NotEqual, collection, expected: new[] { oneOffQuote1, oneOffQuote2 });

			AssertContactFilter(filter, filterName: filterName, property: "test", comparisonOperator: SQLComparisonOperator.StartsWith, collection, expected: new[] { oneOffQuote1, oneOffQuote2 });
			AssertContactFilter(filter, filterName: filterName, property: "test1", comparisonOperator: SQLComparisonOperator.StartsWith, collection, expected: new[] { oneOffQuote1 });
			AssertContactFilter(filter, filterName: filterName, property: "test2", comparisonOperator: SQLComparisonOperator.StartsWith, collection, expected: new[] { oneOffQuote2 });
			AssertContactFilter(filter, filterName: filterName, property: "test3", comparisonOperator: SQLComparisonOperator.StartsWith, collection, expected: Array.Empty<QuotedBooking>());

			AssertContactFilter(filter, filterName: filterName, property: "test", comparisonOperator: SQLComparisonOperator.DoesNotStartWith, collection, expected: Array.Empty<QuotedBooking>());
			AssertContactFilter(filter, filterName: filterName, property: "test1", comparisonOperator: SQLComparisonOperator.DoesNotStartWith, collection, expected: new[] { oneOffQuote2 });
			AssertContactFilter(filter, filterName: filterName, property: "test2", comparisonOperator: SQLComparisonOperator.DoesNotStartWith, collection, expected: new[] { oneOffQuote1 });
			AssertContactFilter(filter, filterName: filterName, property: "test3", comparisonOperator: SQLComparisonOperator.DoesNotStartWith, collection, expected: new[] { oneOffQuote1, oneOffQuote2 });

			AssertContactFilter(filter, filterName: filterName, property: "test", comparisonOperator: SQLComparisonOperator.Contains, collection, expected: new[] { oneOffQuote1, oneOffQuote2 });
			AssertContactFilter(filter, filterName: filterName, property: "test1", comparisonOperator: SQLComparisonOperator.Contains, collection, expected: new[] { oneOffQuote1 });
			AssertContactFilter(filter, filterName: filterName, property: "test2", comparisonOperator: SQLComparisonOperator.Contains, collection, expected: new[] { oneOffQuote2 });
			AssertContactFilter(filter, filterName: filterName, property: "test3", comparisonOperator: SQLComparisonOperator.Contains, collection, expected: Array.Empty<QuotedBooking>());

			AssertContactFilter(filter, filterName: filterName, property: "test", comparisonOperator: SQLComparisonOperator.NotContains, collection, expected: Array.Empty<QuotedBooking>());
			AssertContactFilter(filter, filterName: filterName, property: "test1", comparisonOperator: SQLComparisonOperator.NotContains, collection, expected: new[] { oneOffQuote2 });
			AssertContactFilter(filter, filterName: filterName, property: "test2", comparisonOperator: SQLComparisonOperator.NotContains, collection, expected: new[] { oneOffQuote1 });
			AssertContactFilter(filter, filterName: filterName, property: "test3", comparisonOperator: SQLComparisonOperator.NotContains, collection, expected: new[] { oneOffQuote1, oneOffQuote2 });

			AssertContactFilter(filter, filterName: filterName, property: "", comparisonOperator: SQLComparisonOperator.IsBlank, collection, expected: Array.Empty<QuotedBooking>());

			AssertContactFilter(filter, filterName: filterName, property: "", comparisonOperator: SQLComparisonOperator.IsNotBlank, collection, expected: new[] { oneOffQuote1, oneOffQuote2 });
		}

		void SetupContactAddress(QuotedBooking quote, OrgHeader org, ContactType type, string contactName, bool isOverride)
		{
			switch (type)
			{
				case ContactType.ConsignorContact:
					quote.ConsignorDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
					quote.ConsignorDocumentaryAddress.E2_Contact = contactName;
					quote.ConsignorDocumentaryAddress.E2_AddressOverride = isOverride;
					break;
				case ContactType.ConsigneeContact:
					quote.ConsigneeDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
					quote.ConsigneeDocumentaryAddress.E2_Contact = contactName;
					quote.ConsigneeDocumentaryAddress.E2_AddressOverride = isOverride;
					break;
				case ContactType.ClientContact:
					quote.ClientDocAddress.E2_OA_Address = org.MainAddress.PK;
					quote.ClientDocAddress.E2_Contact = contactName;
					quote.ClientDocAddress.E2_AddressOverride = isOverride;
					break;
				default:
					return;
			}
		}

		static void AssertContactFilter(FilterStripBusinessObject filterStripBusinessObject, ZString filterName, ZString property, SQLComparisonOperator comparisonOperator, ViewQuotedBookingCollection collection, QuotedBooking[] expected)
		{
			((ModuleTextFilter)filterStripBusinessObject[filterName]).SqlComparisonOperator = comparisonOperator;
			((ModuleTextFilter)filterStripBusinessObject[filterName]).Property = property;

			collection.Load(filterStripBusinessObject.Filter);
			AssertContainsExactElementsInAnyOrder
			(
				$"Filter: {comparisonOperator} {property}",
				expected.Select(x => x.Quote.TH_QuoteNumber),
				collection.Select(x => x.QuotedBooking.Quote.TH_QuoteNumber)
			);
		}

		#endregion

		#endregion

		#region Audit Information

		#region TestCreatingUser

		public void TestCreatingUser()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_SystemCreateUser = "ZA";
			quotedBooking1.Quote.TH_SystemCreateUser = "ZB";

			QuotedBooking bookingOnly = CreateBookingOnly();
			bookingOnly.Booking.JS_SystemCreateUser = "AA";

			QuotedBooking quoteOnly = CreateQuoteOnly();
			quoteOnly.Quote.TH_SystemCreateUser = "BB";

			Factory.Save();

			Action<string, string, ZGuid> assertFiltering = (message, userNK, expectedQuotedBooking) =>
			{
				var filter = GetNewFilterStripBusinessObject();
				((ModuleNkFilter)filter["Creating User"]).Property = userNK;
				((ModuleNkFilter)filter["Creating User"]).IsActive = true;

				var collection = new ViewQuotedBookingCollection(Factory);
				collection.Load(filter.Filter);

				if (expectedQuotedBooking.IsEmpty)
				{
					AssertEquals(message, 0, collection.Count);
				}
				else
				{
					AssertEquals(message, 1, collection.Count);
					AssertEquals(message, expectedQuotedBooking, collection[0].QuotedBooking.PK);
				}
			};

			assertFiltering("Quotedbooking not found via booking field", "ZA", ZGuid.Empty);
			assertFiltering("Quotedbooking not found via quote field", "ZB", quotedBooking1.PK);

			assertFiltering("QuickBooking not found via booking field", "AA", ZGuid.Empty);
			assertFiltering("Spot quote found via quote field", "BB", quoteOnly.PK);

			assertFiltering("No matching results", "ZE", ZGuid.Empty);
		}

		public void TestCreatingUser_WorksWithCommaInCode()
		{
			using (RawDataRegistry.Instance.MultiSearchSeparator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ","))
			{
				var user = Factory.NewWithValidTestData<GlbStaff>();
				user.GS_Code = "A,B";

				var quotedBooking = CreateQuotedBooking();
				quotedBooking.Quote.TH_SystemCreateUser = "A,B";

				var quotedBooking2 = CreateQuotedBooking();
				quotedBooking2.Quote.TH_SystemCreateUser = "C,D";

				Factory.Save();

				var filter = GetNewFilterStripBusinessObject();
				((ModuleNkFilter)filter["Creating User"]).Property = "A,B";
				((ModuleNkFilter)filter["Creating User"]).IsActive = true;

				var collection = new ViewQuotedBookingCollection(Factory);
				collection.Load(filter.Filter);

				AssertEquals("Found staff with comma separated name", 1, collection.Count);
				AssertEquals("booking PK", quotedBooking.PK, collection[0].QuotedBooking.PK);
			}
		}

		#endregion

		#region TestLastEditUser

		public void TestLastEditUser_SpotQuote()
		{
			CreateTestUser("ZA", "userZA");
			CreateTestUser("ZB", "userZB");
			Factory.Save();

			QuotedBooking booking1;

			using (CurrentUserChanger.SwitchToNewUserTemporarily("userZA"))
			{
				booking1 = CreateQuoteOnly();
				booking1.Mode = "FRO";
				Factory.Save();
				AssertEquals("1 result for ZA", 1, FetchLastEditUserFilterResults("ZA").Count);
				AssertEquals("0 results for ZB", 0, FetchLastEditUserFilterResults("ZB").Count);
			}

			using (CurrentUserChanger.SwitchToNewUserTemporarily("userZB"))
			{
				booking1.Mode = "LSE";
				Factory.Save();
				AssertEquals("1 result for ZB", 1, FetchLastEditUserFilterResults("ZB").Count);
				AssertEquals("0 results for ZA", 0, FetchLastEditUserFilterResults("ZA").Count);
			}
		}

		public void TestLastEditUser_WorksWithCommaInCode()
		{
			using (RawDataRegistry.Instance.MultiSearchSeparator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ","))
			{
				var user = Factory.NewWithValidTestData<GlbStaff>();
				user.GS_Code = "A,B";
				user.GS_LoginName = "A,B";
				Factory.Save();

				QuotedBooking quotedBooking = null;
				using (CurrentUserChanger.SwitchToNewUserTemporarily("A,B"))
				{
					quotedBooking = CreateQuotedBooking();
					Factory.Save();
				}

				var quotedBooking2 = CreateQuotedBooking();
				Factory.Save();

				var filter = GetNewFilterStripBusinessObject();
				((ModuleNkFilter)filter["Last Edit User"]).Property = "A,B";
				((ModuleNkFilter)filter["Last Edit User"]).IsActive = true;

				var collection = new ViewQuotedBookingCollection(Factory);
				collection.Load(filter.Filter);

				AssertEquals("Found staff with comma separated name", 1, collection.Count);
				AssertEquals("booking PK", quotedBooking.PK, collection[0].QuotedBooking.PK);
			}
		}

		public void TestLastEditUser_QuoteOnly_Addresses()
		{
			#region Setup

			CreateTestUser("ZA", "userZA");
			CreateTestUser("ZB", "userZB");
			CreateTestUser("ZC", "userZC");
			CreateTestUser("ZD", "userZD");
			CreateTestUser("ZX", "userZX");
			Factory.Save();

			var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			var orgAddress3 = Factory.NewWithValidTestData<OrgAddress>();

			// Initial Save
			QuotedBooking booking1;

			using (CurrentUserChanger.SwitchToNewUserTemporarily("userZA"))
			{
				booking1 = CreateQuoteOnly();
				booking1.ClientDocAddress.E2_OA_Address = orgAddress1.PK;
				booking1.ConsigneeDocumentaryAddress.E2_OA_Address = orgAddress1.PK;
				booking1.ConsignorDocumentaryAddress.E2_OA_Address = orgAddress1.PK;
				Factory.Save();
			}

			AssertEquals("Precondition - 1 result for ZA", 1, FetchLastEditUserFilterResults("ZA").Count);
			AssertEquals("Precondition - 0 results for ZB", 0, FetchLastEditUserFilterResults("ZB").Count);
			AssertEquals("Precondition - 0 results for ZC", 0, FetchLastEditUserFilterResults("ZC").Count);
			AssertEquals("Precondition - 0 results for ZD", 0, FetchLastEditUserFilterResults("ZD").Count);
			AssertEquals("Precondition - 0 results for ZX", 0, FetchLastEditUserFilterResults("ZX").Count);

			#endregion

			ViewQuotedBookingCollection results;

			using (CurrentUserChanger.SwitchToNewUserTemporarily("userZB"))
			{
				booking1.ClientDocAddress.E2_OA_Address = orgAddress2.PK;
				Factory.Save();

				results = FetchLastEditUserFilterResults("ZB");
				AssertEquals("Should be 1 result for ZB.", 1, results.Count);
				AssertEquals("Should have found Booking 1 Quote", booking1.Quote.PK, results[0].QuotedBooking.Quote.PK);
			}

			using (CurrentUserChanger.SwitchToNewUserTemporarily("userZC"))
			{
				booking1.ConsigneeDocumentaryAddress.E2_OA_Address = orgAddress2.PK;
				Factory.Save();
				results = FetchLastEditUserFilterResults("ZC");
				AssertEquals("Should be 1 result for ZC.", 1, results.Count);
				AssertEquals("Should have found Booking 1 Quote", booking1.Quote.PK, results[0].QuotedBooking.Quote.PK);
			}

			using (CurrentUserChanger.SwitchToNewUserTemporarily("userZD"))
			{
				booking1.ConsignorDocumentaryAddress.E2_OA_Address = orgAddress3.PK;
				Factory.Save();
				results = FetchLastEditUserFilterResults("ZD");
				AssertEquals("Should be 1 result for ZD.", 1, results.Count);
				AssertEquals("Should have found Booking 1 Quote", booking1.Quote.PK, results[0].QuotedBooking.Quote.PK);
			}

			results = FetchLastEditUserFilterResults("ZX");
			AssertEquals("Should be 0 results.", 0, results.Count);
		}

		public void TestLastEditUser_QuoteOnly_Containers()
		{
			CreateTestUser("ZA", "userZA");
			CreateTestUser("ZB", "userZB");
			Factory.Save();

			QuotedBooking quoteOnly;

			using (CurrentUserChanger.SwitchToNewUserTemporarily("userZA"))
			{
				quoteOnly = CreateQuoteOnly();
				Factory.Save();
			}
			AssertEquals("Precondition - Expected 1 result for ZA user.", 1, FetchLastEditUserFilterResults("ZA").Count);
			AssertEquals("Precondition - Expected 0 results for ZB user.", 0, FetchLastEditUserFilterResults("ZB").Count);

			using (CurrentUserChanger.SwitchToNewUserTemporarily("userZB"))
			{
				var container1 = quoteOnly.Quote.CurrentOneOffQuote.Containers.AddNew();
				var containerRef1 = Factory.New<RefContainer>();
				containerRef1.RC_Code = "MWH1234567";
				container1.TC_RC = containerRef1.PK;
				container1.TC_ContainerCount = 1;
				Factory.Save();
			}
			var results = FetchLastEditUserFilterResults("ZB");
			AssertEquals("Expected one matching ZB filter result.", 1, results.Count);
			AssertEquals("Should have matched quoteOnly.", quoteOnly.Quote.PK, results[0].QuotedBooking.Quote.PK);
		}

		public void TestLastEditUser_QuoteOnly_LooseCargo()
		{
			CreateTestUser("ZA", "userZA");
			CreateTestUser("ZB", "userZB");
			Factory.Save();

			QuotedBooking quoteOnly;

			using (CurrentUserChanger.SwitchToNewUserTemporarily("userZA"))
			{
				quoteOnly = CreateQuoteOnly();
				Factory.Save();
			}
			AssertEquals("Precondition - Expected 1 result for ZA user.", 1, FetchLastEditUserFilterResults("ZA").Count);
			AssertEquals("Precondition - Expected 0 results for ZB user.", 0, FetchLastEditUserFilterResults("ZB").Count);

			using (CurrentUserChanger.SwitchToNewUserTemporarily("userZB"))
			{
				var loose1 = quoteOnly.Quote.CurrentOneOffQuote.LooseCargo.AddNew();
				loose1.TPL_PackLineCount = 1;
				loose1.TPL_F3_NKPackType = "BOX";
				loose1.TPL_Weight = 555m;
				Factory.Save();
			}
			var results = FetchLastEditUserFilterResults("ZB");
			AssertEquals("Expected one matching ZB filter result.", 1, results.Count);
			AssertEquals("Should have matched quoteOnly.", quoteOnly.Quote.PK, results[0].QuotedBooking.Quote.PK);
		}

		#endregion

		#region TestCreateTime

		[TestDate(2014, 12, 10)]
		[TestUtcOffset(0, 0, 0)]
		public void TestCreatedTime()
		{
			var now = ZDateTime.Now;

			var quotedBooking = CreateQuotedBooking();
			quotedBooking.Booking.JS_SystemCreateTimeUtc = now;
			quotedBooking.Quote.TH_SystemCreateTimeUtc = now.AddDays(-2);

			var quoteOnly1 = CreateQuoteOnly();
			quoteOnly1.Quote.TH_SystemCreateTimeUtc = now.AddDays(-1);

			var quoteOnly2 = CreateQuoteOnly();
			quoteOnly2.Quote.TH_SystemCreateTimeUtc = now;

			var quoteOnly3 = CreateQuoteOnly();
			quoteOnly3.Quote.TH_SystemCreateTimeUtc = now.AddHours(-16);

			var quoteOnly4 = CreateQuoteOnly();
			quoteOnly4.Quote.TH_SystemCreateTimeUtc = now.AddDays(1);

			var bookingOnly = CreateBookingOnly();
			bookingOnly.Booking.JS_SystemCreateTimeUtc = now.AddDays(-10);

			Factory.Save();

			var filter = (ModuleDateFilter)FilterStripBizO["Created Time"];
			filter.IsActive = true;

			AssertBookingCollectionIsFiltered("No dates entered should return all results with quotes", FilterStripBizO, quotedBooking, quoteOnly1, quoteOnly2, quoteOnly3, quoteOnly4);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = now.AddDays(-11);
			filter.Property2 = now.AddDays(-4);

			AssertBookingCollectionIsFiltered("bookingOnly is not found one off quote modules is looking only at quotes", FilterStripBizO);

			filter.Property1 = now.AddDays(-2);
			filter.Property2 = now.AddDays(-2);

			AssertBookingCollectionIsFiltered("Quotedbooking is found via quote field.", FilterStripBizO, quotedBooking);

			filter.Property1 = now;
			filter.Property2 = now.AddDays(1);

			AssertBookingCollectionIsFiltered("Spot quote found via quote field. Quotedbooking is not found by booking time", FilterStripBizO, quoteOnly2, quoteOnly4);

			filter.Property1 = now.AddDays(-1);
			filter.Property2 = now.AddDays(-1);

			AssertBookingCollectionIsFiltered("Spot quote found via quote field", FilterStripBizO, quoteOnly1, quoteOnly3);
		}

		#endregion

		#region TestLastEditTime

		[TestDate(2014, 12, 10)]
		[TestUtcOffset(0, 0, 0)]
		public void TestLastEditTime()
		{
			var oneOffQuote = CreateQuoteOnly();
			var timeNow = ZDateTime.UtcNow;

			Factory.Save();

			var lastEditFilter = (ModuleDateFilter)FilterStripBizO["Last Edit Time"];
			lastEditFilter.Property1 = timeNow.AddDays(+1);
			lastEditFilter.Property2 = timeNow.AddDays(+1);
			lastEditFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			lastEditFilter.IsActive = true;

			AssertBookingCollectionIsFiltered("Should have loaded 0 View Quoted Booking", FilterStripBizO);

			lastEditFilter.Property1 = timeNow;
			lastEditFilter.Property2 = timeNow;

			AssertBookingCollectionIsFiltered("Should have 1 Quote as the last edit time is added by default", FilterStripBizO, oneOffQuote);

			lastEditFilter.Property1 = timeNow.AddDays(-3);
			lastEditFilter.Property2 = timeNow.AddDays(-1);

			AssertBookingCollectionIsFiltered("Should have loaded 0 View Quoted Booking", FilterStripBizO);
		}

		#endregion

		#region TestCreatedOn

		public void TestCreatedOn()
		{
			QuotedBooking oneOffQuote1 = CreateQuoteOnly();
			QuotedBooking oneOffQuote2 = CreateQuoteOnly();

			oneOffQuote1.Quote.TH_SystemCreateUser = "ZZ";
			oneOffQuote2.Quote.TH_SystemCreateUser = "ZD";

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleTextFilter)filter["Created On Web/Internal"]).Property = "WEB";
			((ModuleTextFilter)filter["Created On Web/Internal"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", oneOffQuote1.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			((ModuleTextFilter)filter["Created On Web/Internal"]).Property = "ENT";
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 2 Quote", oneOffQuote2.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			((ModuleTextFilter)filter["Created On Web/Internal"]).Property = "ALL";

			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 2 View Quoted Bookings", 2, collection.Count);
		}

		#endregion

		#endregion

		#region Modes And Types

		#region TestModes

		public void TestModes()
		{
			List<QuotedBooking> oneOffQuoteList = new List<QuotedBooking>();
			ZString[] rateModeList = { Core.Constants.RateMode.LCL, Core.Constants.RateMode.RAI, Core.Constants.RateMode.FCL, Core.Constants.RateMode.SEA
										, Core.Constants.RateMode.FWL, Core.Constants.RateMode.LSE, Core.Constants.RateMode.ULD, Core.Constants.RateMode.ROA
										, Core.Constants.RateMode.LRO, Core.Constants.RateMode.FRO, Core.Constants.RateMode.FTL, Core.Constants.RateMode.COU
										, Core.Constants.RateMode.LRA, Core.Constants.RateMode.FRA };
			for (int i = 0; i < 13; i++)
			{
				var oneOffQuote = CreateQuoteOnly();
				oneOffQuote.Mode = rateModeList[i];
				oneOffQuoteList.Add(oneOffQuote);
			}

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleTextFilter)filter["Mode"]).IsActive = true;
			var collection = new ViewQuotedBookingCollection(Factory);
			foreach (var qb in oneOffQuoteList)
			{
				AssertMode(qb.Mode, qb);
			}

			void AssertMode(ZString mode, QuotedBooking qb)
			{
				((ModuleTextFilter)filter["Mode"]).Property = mode;
				collection.Load(filter.Filter);

				AssertEquals($"{mode} Should have loaded 1 View Quoted Booking", 1, collection.Count);
				Assert($"Should have QuotedBooking {mode} Quote and Booking", collection.Cast<ViewQuotedBooking>().Any(x => x.QuotedBooking.Quote.PK == qb.Quote.PK));
			}
		}

		#endregion

		#region TestServiceLevel

		public void TestServiceLevel()
		{
			QuotedBooking oneOffQuote1 = CreateQuoteOnly();
			QuotedBooking oneOffQuote2 = CreateQuoteOnly();

			oneOffQuote1.Quote.CurrentOneOffQuote.TT_RS_NKServiceLevel = "AAA";
			oneOffQuote2.Quote.CurrentOneOffQuote.TT_RS_NKServiceLevel = "BBB";

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleNkFilter)filter["Service Level"]).Property = "AAA";
			((ModuleNkFilter)filter["Service Level"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", oneOffQuote1.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			((ModuleNkFilter)filter["Service Level"]).Property = "BBB";
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", oneOffQuote2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
		}

		#endregion

		#endregion

		#region TestWebStatusFilter

		public void TestWebStatusFilter()
		{
			Globals.IsWeb = true;
			try
			{
				Quote quote1 = CreateQuote();
				Quote quote2 = CreateQuote();

				quote1.TH_IsLocked = true;
				quote2.TH_IsLocked = false;

				Factory.Save();

				QuoteCollection collection = new QuoteCollection(Factory);

				var filter = (OneOffQuoteFilterStripBusinessObject)FilterStripBizO;
				filter.IsInWebQuoteMode = true;
				filter["Status"].IsActive = true;

				((ModuleTextFilter)filter["Status"]).Property = "ALL";
				collection.Load(filter.Filter);

				AssertEquals(2, collection.Count);

				((ModuleTextFilter)filter["Status"]).Property = "FIN";
				collection.Load(filter.Filter);

				AssertEquals(1, collection.Count);
				AssertEquals(quote1, collection[0]);

				((ModuleTextFilter)filter["Status"]).Property = "ACT";
				collection.Load(filter.Filter);

				AssertEquals(1, collection.Count);
				AssertEquals(quote2, collection[0]);
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		#endregion

		#region WebDates

		public void TestWebDateFilters()
		{
			Globals.IsWeb = true;
			try
			{
				AssertWebDateFilter("Quotation Date", RatingHeaderSchema.TH_QuoteDate);
				AssertWebDateFilter("Expiry Date", RatingHeaderSchema.TH_QuoteEndDate);
				AssertWebDateFilter("Acceptance Date", RatingHeaderSchema.TH_Accepted);
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		#endregion

		#region WorkFlow

		public void TestCompletedMilestoneFilter()
		{
			QuotedBooking quotedBooking = CreateQuotedBooking();
			QuotedBooking oneOffQuote = CreateQuoteOnly();

			ProcessTask milestone = quotedBooking.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = "DSN";

			ProcessTask milestone2 = oneOffQuote.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = "DSN";

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();

			WorkflowModuleTextFilter milestoneCompletedFilter = (WorkflowModuleTextFilter)filter["Milestone Completed"];
			milestoneCompletedFilter.MilestoneEvent = "DSN";
			milestoneCompletedFilter.Property = "Not Completed";
			milestoneCompletedFilter.IsActive = true;

			var quotedBookings = Factory.Load<ViewQuotedBooking>(filter.Filter).Select(x => x.PK).ToArray();
			var expected = new[] { quotedBooking.PK, oneOffQuote.PK };

			AssertEquals(2, quotedBookings.Length);
			AssertContainsExactElementsInAnyOrder(expected, quotedBookings);
		}

		#endregion

		#region CRM Security

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<ViewQuotedBooking>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.OneOffQuoteCRMSecurity);
		}

		#endregion

		static void AssertExpectedQuotesAreSeen(string errorMessage, FilterStripBusinessObject filter, ModuleTextBaseFilter moduleTextFilter, string filterProperty, params QuotedBooking[] expectedQuotes)
		{
			var factory = new BusinessObjectFactory();
			var expectedQuoteNumbers = expectedQuotes.Select(x => x.QuotedBookingNumber).ToArray();
			moduleTextFilter.Property = filterProperty;
			var actualQuoteNumbers = factory.Load<ViewQuotedBooking>(filter.Filter).Select(x => x.QuotedBooking.QuotedBookingNumber).ToArray();

			AssertContainsExactElementsInAnyOrder(errorMessage, expectedQuoteNumbers, actualQuoteNumbers);
			AssertEquals(1, factory.GetTableHitCount(ViewQuotedBookingSchema.Constants.TableName));
		}

		static void AssertExpectedQuotesAreSeen(string errorMessage, string comparisonOperator, FilterStripBusinessObject filter, ModuleTextFilter moduleTextFilter, string filterProperty, params QuotedBooking[] expectedQuotes)
		{
			var factory = new BusinessObjectFactory();
			var expectedQuoteNumbers = expectedQuotes.Select(x => x.QuotedBookingNumber).ToArray();
			moduleTextFilter.Property = filterProperty;
			moduleTextFilter.ComparisonOperator = comparisonOperator;
			var actualQuoteNumbers = factory.Load<ViewQuotedBooking>(filter.Filter).Select(x => x.QuotedBooking.QuotedBookingNumber).ToArray();

			AssertContainsExactElementsInAnyOrder(errorMessage, expectedQuoteNumbers, actualQuoteNumbers);
			AssertEquals(1, factory.GetTableHitCount(ViewQuotedBookingSchema.Constants.TableName));
		}

		#region TestOneOffQuoteStatusApprovalFilter

		public void TestOneOffQuoteStatusApprovalFilter()
		{
			var collection = new ViewQuotedBookingCollection(Factory);
			var filter = GetNewFilterStripBusinessObject();
			var filterOneOffQuoteApprovalStatus = ((ModuleTextFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.StatusAndFlags.OneOffQuoteApprovalStatus]);
			filterOneOffQuoteApprovalStatus.IsActive = true;

			var oneOffQuoteApprovedByManager = CreateQuoteOnly();
			var oneOffQuote1NotApprovedByManager = CreateQuoteOnly();
			var oneOffQuote2NotApprovedByManager = CreateQuoteOnly();
			var notOneOffQuote = CreateBookingOnly();

			oneOffQuoteApprovedByManager.Quote.TH_QuoteNumber = "001000_Approved";
			oneOffQuote1NotApprovedByManager.Quote.TH_QuoteNumber = "001001_NotApproved";
			oneOffQuote2NotApprovedByManager.Quote.TH_QuoteNumber = "001002_NotApproved";
			notOneOffQuote.Booking.JS_IsBooking = false;
			Factory.Save();

			oneOffQuoteApprovedByManager.Quote.CurrentOneOffQuote.TT_QuoteApprovedByManager = true;
			oneOffQuote1NotApprovedByManager.Quote.CurrentOneOffQuote.TT_QuoteApprovedByManager = false;
			oneOffQuote2NotApprovedByManager.Quote.CurrentOneOffQuote.TT_QuoteApprovedByManager = false;
			Factory.Save();

			AssertExpectedQuotesAreSeen("Expect only the approved quotes", filter, filterOneOffQuoteApprovalStatus, OrgConstants.FilterControl.ActiveStatus.Code.ActiveClients, oneOffQuoteApprovedByManager);
			AssertExpectedQuotesAreSeen("Expect only the unapproved quotes", filter, filterOneOffQuoteApprovalStatus, OrgConstants.FilterControl.ActiveStatus.Code.InactiveClients, oneOffQuote1NotApprovedByManager, oneOffQuote2NotApprovedByManager);
			AssertExpectedQuotesAreSeen("Expect all quotes", filter, filterOneOffQuoteApprovalStatus, OrgConstants.FilterControl.ActiveStatus.Code.AllClients, oneOffQuoteApprovedByManager, oneOffQuote1NotApprovedByManager, oneOffQuote2NotApprovedByManager);
		}

		#endregion

		public void TestOneOffQuoteKPIFilter()
		{
			var collection = new ViewQuotedBookingCollection(Factory);
			var filter = GetNewFilterStripBusinessObject();
			var filterOneOffQuoteKPI = ((ModuleTextFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.StatusAndFlags.OneOffQuoteKPI]);
			filterOneOffQuoteKPI.IsActive = true;

			var oneOffQuote1 = CreateQuoteOnly();
			var oneOffQuote2 = CreateQuoteOnly();
			var oneOffQuote3 = CreateQuoteOnly();
			var kpiList = new CodeDescriptionPairList();
			kpiList.AddPair("K01", "KPI 001");
			kpiList.AddPair("K02", "KPI 002");
			kpiList.AddPair("K03", "KPI 003");

			using (DataRegistryRating.Instance.OneOffQuoteKPISettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, kpiList))
			{
				oneOffQuote1.OneOffQuoteStatistics.OneOffQuoteKPI = "K01";
				oneOffQuote2.OneOffQuoteStatistics.OneOffQuoteKPI = "K02";
				oneOffQuote3.OneOffQuoteStatistics.OneOffQuoteKPI = "K03";
				Factory.Save();
				AssertExpectedQuotesAreSeen("Expect only the approved quotes", filter, filterOneOffQuoteKPI, "K01", oneOffQuote1);
				AssertExpectedQuotesAreSeen("Expect only the approved quotes", filter, filterOneOffQuoteKPI, "K02", oneOffQuote2);
				AssertExpectedQuotesAreSeen("Expect only the approved quotes", filter, filterOneOffQuoteKPI, "K03", oneOffQuote3);
			}
		}

		public void TestOneOffQuoteSourceFilter()
		{
			var collection = new ViewQuotedBookingCollection(Factory);
			var filter = GetNewFilterStripBusinessObject();
			var filterOneOffQuoteSource = ((ModuleTextFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.StatusAndFlags.OneOffQuoteSource]);
			filterOneOffQuoteSource.IsActive = true;

			var oneOffQuote1 = CreateQuoteOnly();
			var oneOffQuote2 = CreateQuoteOnly();
			var oneOffQuote3 = CreateQuoteOnly();
			var sourceList = new CodeDescriptionPairList();
			sourceList.AddPair("S01", "Source 001");
			sourceList.AddPair("S02", "Source 002");
			sourceList.AddPair("S03", "Source 003");

			using (DataRegistryRating.Instance.OneOffQuoteSourceSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, sourceList))
			{
				oneOffQuote1.OneOffQuoteStatistics.OneOffQuoteSource = "S01";
				oneOffQuote2.OneOffQuoteStatistics.OneOffQuoteSource = "S02";
				oneOffQuote3.OneOffQuoteStatistics.OneOffQuoteSource = "S03";
				Factory.Save();
				AssertExpectedQuotesAreSeen("Expect only the approved quotes", filter, filterOneOffQuoteSource, "S01", oneOffQuote1);
				AssertExpectedQuotesAreSeen("Expect only the approved quotes", filter, filterOneOffQuoteSource, "S02", oneOffQuote2);
				AssertExpectedQuotesAreSeen("Expect only the approved quotes", filter, filterOneOffQuoteSource, "S03", oneOffQuote3);
			}
		}

		public void TestOneOffQuoteRevisionReasonFilter()
		{
			var collection = new ViewQuotedBookingCollection(Factory);
			var filter = GetNewFilterStripBusinessObject();
			var filterOneOffQuoteRevisionReason = ((ModuleTextFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.StatusAndFlags.OneOffQuoteRevisionReason]);
			filterOneOffQuoteRevisionReason.IsActive = true;

			var oneOffQuote1 = CreateQuoteOnly();
			var oneOffQuote2 = CreateQuoteOnly();
			var oneOffQuote3 = CreateQuoteOnly();
			var revisionReasonList = new CodeDescriptionPairList();
			revisionReasonList.AddPair("R01", "Revision Reason 001");
			revisionReasonList.AddPair("R02", "Revision Reason 002");
			revisionReasonList.AddPair("R03", "Revision Reason 003");

			using (DataRegistryRating.Instance.OneOffQuoteRevisionReasonSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, revisionReasonList))
			{
				oneOffQuote1.OneOffQuoteStatistics.OneOffQuoteRevisionReason = "R01";
				oneOffQuote2.OneOffQuoteStatistics.OneOffQuoteRevisionReason = "R02";
				oneOffQuote3.OneOffQuoteStatistics.OneOffQuoteRevisionReason = "R03";
				Factory.Save();
				AssertExpectedQuotesAreSeen("Expect only the approved quotes", filter, filterOneOffQuoteRevisionReason, "R01", oneOffQuote1);
				AssertExpectedQuotesAreSeen("Expect only the approved quotes", filter, filterOneOffQuoteRevisionReason, "R02", oneOffQuote2);
				AssertExpectedQuotesAreSeen("Expect only the approved quotes", filter, filterOneOffQuoteRevisionReason, "R03", oneOffQuote3);
			}
		}

		public void TestOneOffQuoteTransportModeAndContainerModeFilter()
		{
			var collection = new ViewQuotedBookingCollection(Factory);
			var filter = GetNewFilterStripBusinessObject();
			var filterOneOffQuoteTransportMode = ((ModuleTextFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.ModesAndTypes.TransportMode]);
			var filterOneOffQuoteContainerMode = ((ModuleTextFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.ModesAndTypes.ContainerMode]);
			filterOneOffQuoteTransportMode.IsActive = true;
			filterOneOffQuoteContainerMode.IsActive = false;
			var containerModeList = QuotedBooking.GetContainerModes(string.Empty, bookingOnly: false);
			AssertContainsExactElementsInAnyOrder("Container modes do not match", filterOneOffQuoteContainerMode.List, containerModeList);
			var transportModeList = QuotedBooking.GetNewTransportModes();
			AssertContainsExactElementsInAnyOrder("Trasport modes do not match", filterOneOffQuoteTransportMode.List, transportModeList);
			var oneOffQuote1 = CreateQuoteOnly();
			var oneOffQuote2 = CreateQuoteOnly();
			var oneOffQuote3 = CreateQuoteOnly();

			oneOffQuote1.TransportMode = "AIR";
			oneOffQuote1.ContainerMode = "LSE";
			oneOffQuote2.TransportMode = "AIR";
			oneOffQuote2.ContainerMode = "ULD";
			oneOffQuote3.TransportMode = "RAI";
			oneOffQuote3.ContainerMode = "LCL";
			Factory.Save();
			AssertExpectedQuotesAreSeen("Expect One Off Quote1 and One Off Quote2", filter, filterOneOffQuoteTransportMode, "AIR", oneOffQuote1, oneOffQuote2);
			AssertExpectedQuotesAreSeen("Expect One Off Quote3", filter, filterOneOffQuoteTransportMode, "RAI", oneOffQuote3);

			filterOneOffQuoteTransportMode.IsActive = false;
			filterOneOffQuoteContainerMode.IsActive = true;
			AssertExpectedQuotesAreSeen("Expect One Off Quote3", filter, filterOneOffQuoteContainerMode, "LCL", oneOffQuote3);
			AssertExpectedQuotesAreSeen("Expect One Off Quote2", filter, filterOneOffQuoteContainerMode, "ULD", oneOffQuote2);
			AssertExpectedQuotesAreSeen("Expect One Off Quote1", filter, filterOneOffQuoteContainerMode, "LSE", oneOffQuote1);
		}

		public void TestCompanyTariffLevelOverrideFilter()
		{
			var collection = new ViewQuotedBookingCollection(Factory);
			var filter = GetNewFilterStripBusinessObject();
			var companyTariffLevelOverrideFilter = ((ModuleTextFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.NumbersAndReferences.CompanyTariffLevelOverride]);
			companyTariffLevelOverrideFilter.IsActive = true;
			var glbTariff = Factory.New<GlobalTariff>();
			var glbTariff2 = Factory.New<GlobalTariff>();

			var oneOffQuote1 = CreateQuoteOnly();
			var oneOffQuote2 = CreateQuoteOnly();
			var oneOffQuote3 = CreateQuoteOnly();
			var unacceptedBWQ = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
			var acceptedBWQ = CreateQuotedBooking();
			var booking = CreateBookingOnly();
			oneOffQuote1.CompanyTariffLevel = "";
			oneOffQuote2.CompanyTariffLevel = "1";
			oneOffQuote3.CompanyTariffLevel = "2";
			unacceptedBWQ.CompanyTariffLevel = "2";
			acceptedBWQ.CompanyTariffLevel = "2";
			booking.CompanyTariffLevel = "2";
			Factory.Save();
			AssertExpectedQuotesAreSeen("Searching for QuotedBooking whose Quote.CompanyTariffLevelOverride is 2", filter, companyTariffLevelOverrideFilter, "2", oneOffQuote3, acceptedBWQ);
			AssertExpectedQuotesAreSeen("Searching for QuotedBooking whose Quote.CompanyTariffLevelOverride is 1", filter, companyTariffLevelOverrideFilter, "1", oneOffQuote2);
			AssertExpectedQuotesAreSeen("Searching for QuotedBooking whose Quote.CompanyTariffLevelOverride is 0", filter, companyTariffLevelOverrideFilter, "0", oneOffQuote1, unacceptedBWQ);
		}

		public void TestFMCTariffIDFilter()
		{
			var collection = new ViewQuotedBookingCollection(Factory);
			var filter = GetNewFilterStripBusinessObject();
			var fmcTariffIDFilter = ((ModuleTextFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.NumbersAndReferences.FMCTariffID]);
			fmcTariffIDFilter.IsActive = true;

			var oneOffQuote1 = CreateQuoteOnly();
			var oneOffQuote2 = CreateQuoteOnly();
			var oneOffQuote3 = CreateQuoteOnly();
			var unacceptedBWQ = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
			var acceptedBWQ = CreateQuotedBooking();
			var booking = CreateBookingOnly();
			oneOffQuote2.FMCTariffID = "EACD";
			oneOffQuote3.FMCTariffID = "ABCD";
			unacceptedBWQ.FMCTariffID = "CEAD";
			acceptedBWQ.FMCTariffID = "ABCD";
			booking.FMCTariffID = "ACD";
			Factory.Save();

			AssertEquals("FMCTariffID MaxLength", 4, fmcTariffIDFilter.MaxLength);
			AssertExpectedQuotesAreSeen("Searching for QuotedBooking whose Quote.FMCTariffId start with A", ModuleTextFilter.ComparisonConstants.StartsWith, filter, fmcTariffIDFilter, "A", oneOffQuote3, acceptedBWQ);
			AssertExpectedQuotesAreSeen("Searching for QuotedBooking whose Quote.FMCTariffId contains E", ModuleTextFilter.ComparisonConstants.Contains, filter, fmcTariffIDFilter, "E", oneOffQuote2);
			AssertExpectedQuotesAreSeen("Searching for QuotedBooking whose Quote.FMCTariffId is blank", ModuleTextFilter.ComparisonConstants.IsBlank, filter, fmcTariffIDFilter, "", oneOffQuote1, unacceptedBWQ);
		}

		public void TestGivenOneOffQuoteSearch_WhenUsingHBLDeliveryModeFilter_ThenSpecificItemShouldBeListed()
		{
			var collection = new ViewQuotedBookingCollection(Factory);
			var filter = GetNewFilterStripBusinessObject();
			var hblDeliveryModeFilter = ((ModuleTextFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.ModesAndTypes.HBLDeliveryMode]);
			hblDeliveryModeFilter.IsActive = true;

			var oneOffQuote1 = CreateQuoteOnly(QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var oneOffQuote2 = CreateQuoteOnly(QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var oneOffQuote3 = CreateQuoteOnly(QuotedBooking.QuoteState.ApprovedAndAccepted);
			oneOffQuote1.ContainerPackModeOverride = Enterprise.Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			oneOffQuote2.ContainerPackModeOverride = Enterprise.Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR;
			oneOffQuote3.ContainerPackModeOverride = Enterprise.Core.Constants.HBLDeliveryModes.Codes.CFS_CFS;
			Factory.Save();
			AssertExpectedQuotesAreSeen("Searching for QuotedBooking whose Quote.HBLDeliveryMode is empty", filter, hblDeliveryModeFilter, ZString.Empty, oneOffQuote1, oneOffQuote2, oneOffQuote3);
			AssertExpectedQuotesAreSeen("Searching for QuotedBooking whose Quote.HBLDeliveryMode is DOOR/DOOR", filter, hblDeliveryModeFilter, Enterprise.Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR, oneOffQuote1);
			AssertExpectedQuotesAreSeen("Searching for QuotedBooking whose Quote.HBLDeliveryMode is CFS/DOOR", filter, hblDeliveryModeFilter, Enterprise.Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR, oneOffQuote2);
			AssertExpectedQuotesAreSeen("Searching for QuotedBooking whose Quote.HBLDeliveryMode is CFS/CFS", filter, hblDeliveryModeFilter, Enterprise.Core.Constants.HBLDeliveryModes.Codes.CFS_CFS, oneOffQuote3);
		}

		public void TestCommodityCodeFilter()
		{
			var commodityCode1 = Factory.New<RefCommodityCode>();
			commodityCode1.RH_Code = "COM1";
			var commodityCode2 = Factory.New<RefCommodityCode>();
			commodityCode2.RH_Code = "COM2";
			var commodityCode3 = Factory.New<RefCommodityCode>();
			commodityCode3.RH_Code = "COM3";

			var collection = new ViewQuotedBookingCollection(Factory);
			var filter = GetNewFilterStripBusinessObject();
			var commodityCodeFilter = (ModuleNkFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.NumbersAndReferences.CommodityCode];
			commodityCodeFilter.IsActive = true;

			var oneOffQuote1 = CreateQuoteOnly();
			var oneOffQuote2 = CreateQuoteOnly();
			var oneOffQuote3 = CreateQuoteOnly();
			var unacceptedBWQ = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
			var acceptedBWQ = CreateQuotedBooking();
			var booking = CreateBookingOnly();
			oneOffQuote1.Commodity = commodityCode1.RH_Code;
			oneOffQuote2.Commodity = commodityCode2.RH_Code;
			oneOffQuote3.Commodity = commodityCode3.RH_Code;
			unacceptedBWQ.Commodity = commodityCode1.RH_Code;
			acceptedBWQ.Commodity = commodityCode2.RH_Code;
			booking.Commodity = commodityCode3.RH_Code;
			Factory.Save();

			AssertExpectedQuotesAreSeen("Searching for QuotedBooking whose Quote.Commodity is COM1", filter, commodityCodeFilter, "COM1", oneOffQuote1);
			AssertExpectedQuotesAreSeen("Searching for QuotedBooking whose Quote.Commodity is COM2", filter, commodityCodeFilter, "COM2", oneOffQuote2, acceptedBWQ);
			AssertExpectedQuotesAreSeen("Searching for QuotedBooking whose Quote.Commodity is COM3", filter, commodityCodeFilter, "COM3", oneOffQuote3);
		}

		#region CO2 Emission

		public void TestOneOffQuoteCO2Filter()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				var quoteOnly1 = CreateQuoteOnly();

				quoteOnly1.Mode = Core.Constants.ContainerModes.Loose;
				quoteOnly1.Weight = 10;
				quoteOnly1.WeightUnit = Core.Constants.Weight.Tonnes;
				quoteOnly1.SetCO2ePerTonneInKg(12m);
				quoteOnly1.SetCO2eStatus(CO2eStatusList.Codes.Pending);
				quoteOnly1.SetTotalCO2e(120m);

				Factory.Save();

				var filter = GetNewFilterStripBusinessObject();
				var filterOneOffQuoteCO2Total = ((CO2eStatusAndCO2eKgRangeNumberFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.NumbersAndReferences.CO2e]);
				filterOneOffQuoteCO2Total.Property1 = 50.0;
				filterOneOffQuoteCO2Total.Property2 = 130.0;
				filterOneOffQuoteCO2Total.CO2eStatus = CO2eStatusList.Codes.Current;
				filterOneOffQuoteCO2Total.IsActive = true;

				var collection = new ViewQuotedBookingCollection(new BusinessObjectFactory());
				collection.Load(filter.Filter);

				AssertEquals("Should have loaded 0 View One Off Quote because status is NOT CURRENT", 0, collection.Count);

				quoteOnly1.SetCO2eStatus(CO2eStatusList.Codes.Current);
				Factory.Save();

				collection.Load(filter.Filter);

				AssertEquals("Should have loaded 1 View One Off Quote", 1, collection.Count);
				AssertEquals("Should have Quote 1 Quote", quoteOnly1.PK, collection[0].QuotedBooking.PK);
				AssertEquals("Total CO2 should be between 50 & 130", 120m, collection[0].QuotedBooking.GetTotalCO2e());

				quoteOnly1.SetTotalCO2e(12m);
				Factory.Save();

				collection.Load(filter.Filter);

				AssertEquals("Should have loaded 0 View One Off Quote because Total CO2 is less than 50", 0, collection.Count);
				AssertEquals("Total CO2 should be less than 50", 12m, quoteOnly1.GetTotalCO2e());

				quoteOnly1.SetTotalCO2e(140m);
				Factory.Save();

				collection.Load(filter.Filter);

				AssertEquals("Should have loaded 0 View One Off Quote because Total CO2 is more than 130", 0, collection.Count);
				AssertEquals("Total CO2 should be more than 130", 140m, quoteOnly1.GetTotalCO2e());

				quoteOnly1.SetTotalCO2e(130m);
				Factory.Save();

				collection.Load(filter.Filter);

				AssertEquals("Should have loaded 1 View One Off Quote because Total CO2 is between 50 and 130", 1, collection.Count);
				AssertEquals("Total CO2 should be 130", 130m, quoteOnly1.GetTotalCO2e());

				quoteOnly1.SetCO2eStatus(CO2eStatusList.Codes.Pending);
				Factory.Save();
				filterOneOffQuoteCO2Total.CO2eStatus = CO2eStatusList.Codes.Pending;
				collection.Load(filter.Filter);

				AssertEquals("Should have loaded 1 View One Off Quote", 1, collection.Count);
			}
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			var filterStripBO = new OneOffQuoteFilterStripBusinessObject();
			filterStripBO.QueryObjectType = typeof(ViewQuotedBooking);
			return filterStripBO;
		}

		protected void AssertWebDateFilter(string name, SchemaDateTimeColumn column)
		{
			var quote1 = CreateQuote();
			quote1.TH_QuoteDate = ZDate.Today.AddMonths(-6);

			var quote2 = CreateQuote();
			quote2.TH_QuoteDate = ZDate.Today.AddMonths(-6);

			quote1[column] = ZDateTime.Now.AddDays(-15);
			quote2[column] = ZDateTime.Now.AddDays(-10);

			Factory.Save();
			var filter = GetNewFilterStripBusinessObject();
			((OneOffQuoteFilterStripBusinessObject)filter).IsInWebQuoteMode = true;

			ModuleDateFilter dateFilter = (ModuleDateFilter)filter[name];
			dateFilter.Property1 = ZDateTime.Now.AddDays(-17);
			dateFilter.Property2 = ZDateTime.Now.AddDays(-13);
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;

			QuoteCollection collection = new QuoteCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Testing '" + name + "' filter. Filtered elements count.", 1, collection.Count);
			AssertEquals("Testing '" + name + "' filter. Filtered element.", quote1, collection[0]);
		}

		#endregion
	}
}
