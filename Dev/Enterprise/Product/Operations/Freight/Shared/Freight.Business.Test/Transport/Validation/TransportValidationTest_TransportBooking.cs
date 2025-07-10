using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TransportValidationTest_TransportBooking : TransportValidationTest
	{
		protected override Transport GetNewTransport()
		{
			var bookingConsolidation = Factory.New<IDtbBookingConsolidation>();
			var parentCore = (ITransportParentCore)bookingConsolidation;
			return new TransportCollection(parentCore).AddNew();
		}
	}
}
