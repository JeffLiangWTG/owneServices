using System;
using System.Collections.Generic;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(JobSupplierBookingLineFilterBusinessObject))]
	class JobSupplierBookingLineFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new JobSupplierBookingLineFilterBusinessObject();

		public void TestOrderNumberFilter()
		{
			PrepareTestData();

			var filter = new JobSupplierBookingLineFilterBusinessObject();
			var packLineIDFilter = (ModuleTextFilter)filter["Order No"];
			packLineIDFilter.Property = "ORD003";
			packLineIDFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			packLineIDFilter.IsActive = true;

			var collection = new JobSupplierBookingLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("1 Booking Line Loaded", 1, collection.Count);
			AssertEquals("JSL003", collection[0].JSL_BookingLineId);
		}

		public void TestBookingIDFilter()
		{
			PrepareTestData();

			var filter = new JobSupplierBookingLineFilterBusinessObject();
			var packLineIDFilter = (ModuleTextFilter)filter["Booking #"];
			packLineIDFilter.Property = "JSB002";
			packLineIDFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			packLineIDFilter.IsActive = true;

			var collection = new JobSupplierBookingLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("1 Booking Line Loaded", 1, collection.Count);
			AssertEquals("JSL002", collection[0].JSL_BookingLineId);
		}

		public void TestOrderLineNoFilter()
		{
			PrepareTestData();

			var filter = new JobSupplierBookingLineFilterBusinessObject();
			var packLineIDFilter = (ModuleTextFilter)filter["Order Line No"];
			packLineIDFilter.Property = "3";
			packLineIDFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			packLineIDFilter.IsActive = true;

			var collection = new JobSupplierBookingLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("1 Booking Line Loaded", 1, collection.Count);
			AssertEquals("JSL003", collection[0].JSL_BookingLineId);
		}

		public void TestOrderLineProductFilter()
		{
			PrepareTestData();

			var filter = new JobSupplierBookingLineFilterBusinessObject();
			var packLineIDFilter = (ModuleTextFilter)filter["Product"];
			packLineIDFilter.Property = "P01";
			packLineIDFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			packLineIDFilter.IsActive = true;

			var collection = new JobSupplierBookingLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("1 Booking Line Loaded", 1, collection.Count);
			AssertEquals("JSL001", collection[0].JSL_BookingLineId);
		}

		public void TestOrderLineReferenceFilter()
		{
			PrepareTestData();

			var filter = new JobSupplierBookingLineFilterBusinessObject();
			var packLineIDFilter = (ModuleTextFilter)filter["Order Line Reference"];
			packLineIDFilter.Property = "R02";
			packLineIDFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			packLineIDFilter.IsActive = true;

			var collection = new JobSupplierBookingLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("1 Booking Line Loaded", 1, collection.Count);
			AssertEquals("JSL002", collection[0].JSL_BookingLineId);
		}

		public void TestBookingGoodsDescriptionFilter()
		{
			PrepareTestData();

			var filter = new JobSupplierBookingLineFilterBusinessObject();
			var packLineIDFilter = (ModuleTextFilter)filter["Goods Description"];
			packLineIDFilter.Property = "Desc003";
			packLineIDFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			packLineIDFilter.IsActive = true;

			var collection = new JobSupplierBookingLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("1 Booking Line Loaded", 1, collection.Count);
			AssertEquals("JSL003", collection[0].JSL_BookingLineId);
		}

		public void TestOrderLineSplitNoFilter()
		{
			PrepareTestData();

			var filter = new JobSupplierBookingLineFilterBusinessObject();
			var packLineIDFilter = (ModuleTextFilter)filter["Order Line Split No"];
			packLineIDFilter.Property = "1";
			packLineIDFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			packLineIDFilter.IsActive = true;

			var collection = new JobSupplierBookingLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("1 Booking Line Loaded", 1, collection.Count);
			AssertEquals("JSL001", collection[0].JSL_BookingLineId);
		}

		public void TestOrderLineSubLineNoFilter()
		{
			PrepareTestData();

			var filter = new JobSupplierBookingLineFilterBusinessObject();
			var packLineIDFilter = (ModuleTextFilter)filter["Order Line Sub Line No"];
			packLineIDFilter.Property = "2";
			packLineIDFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			packLineIDFilter.IsActive = true;

			var collection = new JobSupplierBookingLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("1 Booking Line Loaded", 1, collection.Count);
			AssertEquals("JSL002", collection[0].JSL_BookingLineId);
		}

		void PrepareTestData()
		{
			JobSupplierBookingLine CreateBookingLine(string orderNumber, string bookingID, string goodsDesc, string bookingLineId, int lineNo, string partNo, string lineReference, short splitNo, int subLineNo)
			{
				var order = Factory.NewWithValidTestData<Order>();
				order.JD_OrderNumber = orderNumber;
				var orderLine = order.OrderLines.AddNew();
				orderLine.JO_LineNo = lineNo;
				orderLine.JO_LineReference = lineReference;
				orderLine.JO_LineSplitNumber = splitNo;
				orderLine.JO_SubLineNo = subLineNo;
				orderLine.JO_Partno = partNo;

				var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
				booking.JSB_Status = Core.Constants.SupplierBookingStatus.Approved;
				booking.JSB_BookingId = bookingID;
				booking.JSB_GoodsDescription = goodsDesc;

				var bookingLine = booking.SupplierBookingLines.AddNew();
				bookingLine.JSL_BookingLineId = bookingLineId;
				bookingLine.JSL_JO_OrderLine = orderLine.PK;
				return bookingLine;
			}

			CreateBookingLine("ORD001", "JSB001", "Desc001", "JSL001", 1, "P01", "R01", 1, 1);
			CreateBookingLine("ORD002", "JSB002", "Desc002", "JSL002", 2, "P02", "R02", 2, 2);
			CreateBookingLine("ORD003", "JSB003", "Desc003", "JSL003", 3, "P03", "R03", 3, 3);

			Factory.Save();
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();
			result.Add(TableFilter(JobOrderLineSchema.Constants.TableName, "Order Line No"));
			result.Add(TableFilter(JobOrderLineSchema.Constants.TableName, "Product"));
			result.Add(TableFilter(JobOrderLineSchema.Constants.TableName, "Order Line Reference"));
			result.Add(TableFilter(JobOrderLineSchema.Constants.TableName, "Order Line Split No"));
			result.Add(TableFilter(JobOrderLineSchema.Constants.TableName, "Order Line Sub Line No"));

			result.Add(TableFilter(JobOrderHeaderSchema.Constants.TableName, "Order Number"));

			return result;
		}
	}
}
