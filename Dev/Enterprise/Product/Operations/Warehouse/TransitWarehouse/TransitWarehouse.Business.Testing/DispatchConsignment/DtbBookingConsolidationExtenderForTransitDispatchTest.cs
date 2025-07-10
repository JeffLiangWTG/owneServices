using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class DtbBookingConsolidationExtenderForTransitDispatchTest : TestCaseWithFactory
	{
		public void TestGetExistDtbBookings()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var dcn = Helper.CreateDispatchConsignment("DC001", data.Whs1.PK);

			var bookingConsolidation = Factory.New<IDtbBookingConsolidation>();
			var booking = Factory.New<IDtbBooking>();
			booking.KM_KB_Booking = bookingConsolidation.PK;
			booking.KM_JobID = "TB001";
			var booking2 = Factory.New<IDtbBooking>();
			booking2.KM_KB_Booking = bookingConsolidation.PK;
			booking2.KM_JobID = "TB002";
			Factory.Save();
			var joblink = Factory.New<StmUniversalJobLink>();
			joblink.UCL_ParentID = booking.PK;
			joblink.UCL_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			joblink.UCL_SourceType = nameof(DataContextType.TransitDispatch);
			joblink.UCL_SourceKey = dcn.WDC_ConsignmentID;
			joblink.UCL_CompanyCode = "EDI";
			joblink.UCL_EnterpriseCode = "EDI";
			joblink.UCL_ServerCode = "AAA";

			Factory.Save();

			var extender = new DtbBookingConsolidationExtenderForTransitDispatch();
			var resultBookings = extender.GetExistingDtbBookings(bookingConsolidation, dcn.WDC_ConsignmentID, Factory.CreateNewFactory());

			AssertEquals("Should only return 1 booking", 1, resultBookings.Count());
			AssertEquals("The only booking should be TB001", "TB001", resultBookings.Single().KM_JobID);
		}

		public void TestIsCreateBookingByPassedPackage()
		{
			var extender = new DtbBookingConsolidationExtenderForTransitDispatch();
			AssertEquals("IsCreatingBookingByPassedPackages should be true", true, extender.IsCreatingBookingByPassedPackages);
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;
	}
}
