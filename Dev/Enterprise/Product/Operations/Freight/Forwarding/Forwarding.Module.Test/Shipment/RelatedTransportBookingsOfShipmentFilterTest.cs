using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(RelatedTransportBookingsOfShipmentFilter))]
	class RelatedTransportBookingsOfShipmentFilterTest : ModuleFilterTestCase<RelatedTransportBookingsOfShipmentFilter>
	{
		public void TestFilter()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();
			var shipment3 = Factory.New<ForwardingShipment>();

			CreateTransportBooking(shipment1, "AVL");
			CreateTransportBooking(shipment1, "AVL");

			CreateTransportBooking(shipment2, "AVL");
			CreateTransportBooking(shipment2, "DLV");

			CreateTransportBooking(shipment3, "AVL");

			Factory.Save();

			var filterStripBizO = new JobShipmentFilterBusinessObject();
			var relatedTransportBookingsOfShipmentFilter = (RelatedTransportBookingsOfShipmentFilter)filterStripBizO["Related Transport Booking"];
			relatedTransportBookingsOfShipmentFilter.IsActive = true;
			relatedTransportBookingsOfShipmentFilter.SelectedFilters.AddTextFilterStrip("Booking Status", "AVL");

			var shipments = new ForwardingShipmentCollection(Factory);

			relatedTransportBookingsOfShipmentFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			shipments.Load(filterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2, shipment3 }, shipments);

			relatedTransportBookingsOfShipmentFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			shipments.Load(filterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment3 }, shipments);

			relatedTransportBookingsOfShipmentFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			shipments.Load(filterStripBizO.Filter);
			AssertEquals(0, shipments.Count);

			((ModuleTextFilter)relatedTransportBookingsOfShipmentFilter.SelectedFilters["Booking Status"]).Property = "DLV";

			relatedTransportBookingsOfShipmentFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			shipments.Load(filterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment2 }, shipments);

			relatedTransportBookingsOfShipmentFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			shipments.Load(filterStripBizO.Filter);
			AssertEquals(0, shipments.Count);

			relatedTransportBookingsOfShipmentFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			shipments.Load(filterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment3 }, shipments);
		}

		public void TestAllMatch_ShouldUseParameterisedTextForSubQuery()
		{
			var filterBizo = new JobShipmentFilterBusinessObject();
			var relatedTransportBookingsOfShipmentFilter = filterBizo.AddFilterStrip<RelatedTransportBookingsOfShipmentFilter>("Related Transport Booking");
			relatedTransportBookingsOfShipmentFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			relatedTransportBookingsOfShipmentFilter.SelectedFilters.AddTextFilterStrip("Booking Status", "AVL");

			var sql = filterBizo.Filter.ParameterisedText.ParameterisedQueryText;
			AssertContains("The sub query should use parameters rather than string literals. SAD!" + System.Environment.NewLine + sql, "@FOREIGN", sql);

			var parameters = filterBizo.Filter.Params;
			AssertEquals("The renamed parameters must actually be declared, too, or the query obviously won't work. SAD!", true, parameters.Any(x => x.ParameterName.StartsWith("@FOREIGN")));
		}

		void CreateTransportBooking(BusinessObject parent, string status)
		{
			var booking = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbBooking>(), TestBusinessObjectKind.MinimumRequiredToSave);
			booking[DtbBookingSchema.KM_Status] = status;
			var bookingConsolidation = (BusinessObject)Factory.LoadTop1<IDtbBookingConsolidation>(new ZQuery(DtbBookingConsolidationSchema.PK, booking[DtbBookingSchema.KM_KB_Booking]));
			using (bookingConsolidation.GetValidationSuspender())
			{
				bookingConsolidation[DtbBookingConsolidationSchema.KB_IsOverridden] = 1;
				bookingConsolidation[DtbBookingConsolidationSchema.KB_ParentID] = parent.PK;
				bookingConsolidation[DtbBookingConsolidationSchema.KB_ParentTableCode] = parent.TablePrefix;
			}
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override RelatedTransportBookingsOfShipmentFilter GetNewModuleFilter()
		{
			return new RelatedTransportBookingsOfShipmentFilter("moo", () => new ArrayList());
		}

		IBusinessObjectCollection CreateRelatedEntityCollection() => ObjectFactory.Get<IDtbBookingCollection>(nameof(IDtbBookingCollection), Factory);

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;
	}
}
