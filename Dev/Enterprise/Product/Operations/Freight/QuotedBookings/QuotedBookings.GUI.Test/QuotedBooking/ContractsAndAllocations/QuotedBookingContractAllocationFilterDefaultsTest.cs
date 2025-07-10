using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	sealed class QuotedBookingContractAllocationFilterDefaultsTest : TestCaseWithFactory
	{
		public void TestFilterDefaultingFromQuotedBooking()
		{
			var today = ZDateTime.Today;

			var contract = Factory.New<IRatingContract>();
			contract.RCT_ContractNumber = "ACCIO";

			var shippingLine = Factory.New<OrgHeader>();

			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.Booking.JS_CarrierContractNumber = "LUMOS";
			quotedBooking.Booking.JS_RCA_BookingAllocationLine = ZGuid.NewZGuid();
			quotedBooking.Booking.JS_TransportMode = Core.Constants.TransportModes.Sea;
			quotedBooking.Booking.BookedShippingLinePK = shippingLine.PK;
			quotedBooking.ETD = today;

			var refCommodityCode = Factory.New<RefCommodityCode>();
			refCommodityCode.RH_IsHazardous = true;
			refCommodityCode.RH_Code = "NOX";

			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_ContainerType = Core.Constants.ContainerTypes.DryStorage;

			var container = quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_RCA_AllocationLine = ZGuid.NewZGuid();
			container.JC_RH_NKContainerCommodityCode = refCommodityCode.RH_Code;
			container.JC_RC = refContainer.PK;

			var filterDefaults = (new QuotedBookingContractAllocationFilterDefaults(quotedBooking)).GetFilterDefaultsForContract();
			AssertEquals(true, filterDefaults.ContainsDefaultFor($"{CarrierContractFilterConstants.ContractNumber}:Property"));
			AssertEquals("LUMOS", filterDefaults[$"{CarrierContractFilterConstants.ContractNumber}:Property"].Value);

			// might not need transportMode filter
			AssertEquals(true, filterDefaults.ContainsDefaultFor($"{CarrierContractFilterConstants.TransportMode}:Property"));
			AssertEquals(Core.Constants.TransportModes.Sea, filterDefaults[$"{CarrierContractFilterConstants.TransportMode}:Property"].Value);

			AssertEquals(true, filterDefaults.ContainsDefaultFor($"{CarrierContractFilterConstants.StartDate}:Property2"));
			AssertEquals(today, filterDefaults[$"{CarrierContractFilterConstants.StartDate}:Property2"].Value);

			AssertEquals(true, filterDefaults.ContainsDefaultFor($"{CarrierContractFilterConstants.ExpiryDate}:Property1:2"));
			AssertEquals(today, filterDefaults[$"{CarrierContractFilterConstants.ExpiryDate}:Property1:2"].Value);

			AssertEquals(true, filterDefaults.ContainsDefaultFor($"{CarrierContractFilterConstants.ServiceProvider}:Property"));
			AssertEquals(shippingLine.PK, filterDefaults[$"{CarrierContractFilterConstants.ServiceProvider}:Property"].Value);

			AssertEquals(true, filterDefaults.ContainsDefaultFor($"{CarrierContractFilterConstants.ContainerType}:Property:2"));
			AssertEquals(refContainer.RC_ContainerType, filterDefaults[$"{CarrierContractFilterConstants.ContainerType}:Property:2"].Value);

			AssertEquals(true, filterDefaults.ContainsDefaultFor($"{CarrierContractFilterConstants.AllowHazardousCommodities}:Property0"));
			AssertEquals(container.ContainerCommodityCode.RH_IsHazardous, filterDefaults[$"{CarrierContractFilterConstants.AllowHazardousCommodities}:Property0"].Value);
		}

		public void TestNoExceptionsWhenDefaultingForOneOffQuote()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			AssertNull(quotedBooking.Booking);

			var filterDefaults = new QuotedBookingContractAllocationFilterDefaults(quotedBooking);
			AssertNoExceptionThrown(() => filterDefaults.GetFilterDefaultsForContract());
		}

		public void TestFilterDefaultingFromQuotedBooking_AllocationRouteDefaults()
		{
			var allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.AllocationLinePK = allocationRoute.PK;

			var filterDefaults = (new QuotedBookingContractAllocationFilterDefaults(quotedBooking)).GetFilterDefaultsForContract();
			var routeChildDefaults = filterDefaults[$"{CarrierContractFilterConstants.AllocationRoutes}:Property:2"].ChildDefaults;

			AssertEquals(true, routeChildDefaults.ContainsDefaultFor($"{AllocationRouteFilterConstants.AllocationRouteID}:Property"));
			AssertEquals(allocationRoute.RCA_AllocationLineID, routeChildDefaults[$"{AllocationRouteFilterConstants.AllocationRouteID}:Property"].Value);
		}

		public void TestFilterDefaultingFromQuotedBooking_NotPopulateAllocationIDFilter_WhenEmpty()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);

			var filterDefaults = (new QuotedBookingContractAllocationFilterDefaults(quotedBooking)).GetFilterDefaultsForContract();
			var routeChildDefaults = filterDefaults[$"{CarrierContractFilterConstants.AllocationRoutes}:Property:2"].ChildDefaults;

			AssertEquals(false, routeChildDefaults.ContainsDefaultFor($"{AllocationRouteFilterConstants.AllocationRouteID}:Property"));
		}

		public void TestFilterDefaultingFromContainer_PopulatesAllocationIDFilter()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);

			var allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			var container = quotedBooking.QuotedBookingContainers.AddNew();

			container.JC_RCA_AllocationLine = allocationRoute.PK;

			var filterDefaults = (new QuotedBookingContractAllocationFilterDefaults(container)).GetFilterDefaultsForContract();
			var routeChildDefaults = filterDefaults[$"{CarrierContractFilterConstants.AllocationRoutes}:Property:2"].ChildDefaults;

			AssertEquals(true, routeChildDefaults.ContainsDefaultFor($"{AllocationRouteFilterConstants.AllocationRouteID}:Property"));
			AssertEquals(allocationRoute.RCA_AllocationLineID, routeChildDefaults[$"{AllocationRouteFilterConstants.AllocationRouteID}:Property"].Value);
		}

		public void TestFilterDefaultingFromContainer_NotPopulateAllocationIDFilter_WhenEmpty()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);

			var container = quotedBooking.QuotedBookingContainers.AddNew();

			var filterDefaults = (new QuotedBookingContractAllocationFilterDefaults(container)).GetFilterDefaultsForContract();
			var routeChildDefaults = filterDefaults[$"{CarrierContractFilterConstants.AllocationRoutes}:Property:2"].ChildDefaults;

			AssertEquals(false, routeChildDefaults.ContainsDefaultFor($"{AllocationRouteFilterConstants.AllocationRouteID}:Property"));
		}
	}
}
