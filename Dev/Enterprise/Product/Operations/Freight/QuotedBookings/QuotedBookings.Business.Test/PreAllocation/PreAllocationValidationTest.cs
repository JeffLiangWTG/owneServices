using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	public class PreAllocationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEmptyHouseBIllRange()
		{
			ForwardingShipment booking = Factory.NewWithValidTestData<ForwardingShipment>();
			booking.JS_IsBooking = true;
			QuotedBooking quotedbooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			PreAllocation allocation = new PreAllocation(quotedbooking, PreAllocation.PreAllocationState.New);
			allocation.HouseBillFrom = "";
			allocation.HouseBillTo = "";
			allocation.Validation.ValidateAll();
			AssertHasError(allocation.HouseBillToInfo, "House bill number range 'to' cannot be empty.");
			AssertHasError(allocation.HouseBillFromInfo, "House bill number range 'from' cannot be empty.");
		}

		public void TestHousebillCOunt()
		{
			ForwardingShipment booking = Factory.NewWithValidTestData<ForwardingShipment>();
			booking.JS_IsBooking = true;
			QuotedBooking quotedbooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			PreAllocation allocation = new PreAllocation(quotedbooking, PreAllocation.PreAllocationState.New);
			allocation.HouseBillCount = 10002;
			allocation.Validation.ValidateAll();
			AssertHasError(allocation.HouseBillCountInfo, "Count needs to be less than or equal to 10000.");
		}

		public void TestValidateBranchOriginDestination()
		{
			ForwardingShipment booking = Factory.NewWithValidTestData<ForwardingShipment>();
			booking.JS_IsBooking = true;
			QuotedBooking quotedbooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			PreAllocation allocation = new PreAllocation(quotedbooking, PreAllocation.PreAllocationState.New);
			try
			{
				allocation.QuotedBooking.TryLoadOrCreateJob();
				allocation.QuotedBooking.ClientAddrPK = Factory.NewWithValidTestData<OrgAddress>().PK;
				allocation.Validation.ValidateAll();
				AssertHasRowError(allocation, "A Department cannot be determined. Please ensure you're logged into an operations branch with 'Default Departments > Default to Current Login Dept.' registry item enabled, or enter a Mode, Origin and Destination.");
				allocation.QuotedBooking.Mode = Core.Constants.RateMode.FCL;
				allocation.QuotedBooking.Origin = "AUSYD";
				allocation.QuotedBooking.Destination = "AUSYD";
				allocation.Validation.ValidateAll();
				Assert(!allocation.HasRowErrors);
			}
			finally
			{
				if (allocation.QuotedBooking.Job != null)
				{
					allocation.QuotedBooking.Job.Dispose();
				}
			}
		}
	}
}
