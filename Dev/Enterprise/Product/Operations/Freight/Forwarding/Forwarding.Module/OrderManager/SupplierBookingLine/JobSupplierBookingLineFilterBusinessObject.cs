using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module
{
	internal class JobSupplierBookingLineFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			filters.AddTextFilter("Booking #", JobSupplierBookingSchema.JSB_BookingId).With(
				filter =>
				{
					filter.SubGroup = BookingSubGroupProcessor;
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingLineFilter|BookingID", "Booking #");
					filter.MaxLength = JobSupplierBookingSchema.JSB_BookingId.MaxLength;
				});

			filters.AddTextFilter("Order No", JobOrderHeaderSchema.JD_OrderNumber).With(
				filter =>
				{
					filter.SubGroup = OrderSubGroupProcessor;
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingLineFilter|OrderNumber", "Order Number");
					filter.MaxLength = JobOrderHeaderSchema.JD_OrderNumber.MaxLength;
				});

			filters.AddNumberFilter("Order Line No", GetOrderLineNumberQuery).With(
					filter =>
					{
						filter.SubGroup = OrderLineSubGroupProcessor;
						filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingLineFilter|OrderLineNo", "Order Line No");
					});

			filters.AddTextFilter("Product", JobOrderLineSchema.JO_Partno).With(
				filter =>
				{
					filter.SubGroup = OrderLineSubGroupProcessor;
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingLineFilter|OrderLinePartNo", "Product");
					filter.MaxLength = JobOrderLineSchema.JO_Partno.MaxLength;
				});

			filters.AddTextFilter("Order Line Reference", JobOrderLineSchema.JO_LineReference).With(
				filter =>
				{
					filter.SubGroup = OrderLineSubGroupProcessor;
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingLineFilter|OrderLineReference", "Order Line Reference");
					filter.MaxLength = JobOrderLineSchema.JO_LineReference.MaxLength;
				});

			filters.AddTextFilter("Goods Description", JobSupplierBookingSchema.JSB_GoodsDescription).With(
				filter =>
				{
					filter.SubGroup = BookingSubGroupProcessor;
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingLineFilter|BookingGoodsDescription", "Goods Description");
					filter.MaxLength = JobSupplierBookingSchema.JSB_GoodsDescription.MaxLength;
				});

			filters.AddNumberFilter("Order Line Split No", GetOrderLineSplitNumberQuery).With(
				filter =>
				{
					filter.SubGroup = OrderLineSubGroupProcessor;
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingLineFilter|OrderLineSplitNo", "Order Line Split No");
				});

			filters.AddNumberFilter("Order Line Sub Line No", GetOrderLineSubLineNumberQuery).With(
				filter =>
				{
					filter.SubGroup = OrderLineSubGroupProcessor;
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingLineFilter|OrderLineSubLineNo", "Order Line Sub Line No");
				});

			return filters;
		}

		ZQuery GetOrderLineNumberQuery(SQLComparisonOperator comparisonOperator, ZString value) =>
			GetOrderLineIntFieldQuery(JobOrderLineSchema.JO_LineNo, value);

		ZQuery GetOrderLineSplitNumberQuery(SQLComparisonOperator comparisonOperator, ZString value) =>
			ZShort.TryParse(value, out var shortValue) ? new ZQuery().AddToFilter_PossiblyCommaSeparated(JobOrderLineSchema.JO_LineSplitNumber, SQLComparisonOperator.Equal, shortValue) : ZQuery.NoResultQuery;

		ZQuery GetOrderLineSubLineNumberQuery(SQLComparisonOperator comparisonOperator, ZString value) =>
			GetOrderLineIntFieldQuery(JobOrderLineSchema.JO_SubLineNo, value);

		ZQuery GetOrderLineIntFieldQuery(SchemaNumericColumn column, ZString value) => ZInt.TryParse(value, out var intValue) ? new ZQuery().AddToFilter_PossiblyCommaSeparated(column, SQLComparisonOperator.Equal, intValue) : ZQuery.NoResultQuery;

		public ModuleFilterSubGroup OrderSubGroupProcessor => orderSubGroup ?? (orderSubGroup = new OrderSubGroup());
		ModuleFilterSubGroup orderSubGroup;

		class OrderSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var orderSubQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.PK);
				orderSubQuery.AddToFilter(filter);

				var orderLineSubQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.PK);
				orderLineSubQuery.AddSubQuery(JobOrderLineSchema.JO_JD, orderSubQuery, JoinCondition.And);

				var result = new ZDBOnlyQuery(typeof(JobSupplierBookingLine));
				result.AddSubQuery(JobSupplierBookingLineSchema.JSL_JO_OrderLine, orderLineSubQuery, JoinCondition.And);

				return result;
			}
		}

		public ModuleFilterSubGroup OrderLineSubGroupProcessor => orderLineSubGroup ?? (orderLineSubGroup = new OrderLineSubGroup());
		ModuleFilterSubGroup orderLineSubGroup;

		class OrderLineSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var orderLineSubQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.PK);
				orderLineSubQuery.AddToFilter(filter);

				var result = new ZDBOnlyQuery(typeof(JobSupplierBookingLine));
				result.AddSubQuery(JobSupplierBookingLineSchema.JSL_JO_OrderLine, orderLineSubQuery, JoinCondition.And);

				return result;
			}
		}

		public ModuleFilterSubGroup BookingSubGroupProcessor => bookingSubGroup ?? (bookingSubGroup = new BookingSubGroup());
		ModuleFilterSubGroup bookingSubGroup;

		class BookingSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var bookingSubQuery = new ZDBOnlySubQuery(typeof(JobSupplierBooking), JobSupplierBookingSchema.PK);
				bookingSubQuery.AddToFilter(filter);

				var result = new ZDBOnlyQuery(typeof(JobSupplierBookingLine));
				result.AddSubQuery(JobSupplierBookingLineSchema.JSL_JSB_Booking, bookingSubQuery, JoinCondition.And);
				return result;
			}
		}
	}
}
