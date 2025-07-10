using Enterprise.Environment;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportConsignment.Business.Testing
{
	class DtbBookingConsignmentFountainUniqueIndexFailureHandlerTest : DtbTransportFountainUniqueIndexFailureHandlerTest<DtbBookingConsignment>
	{
		protected override INumberFountainProxy NumberFountainToTest
		{
			get { return Env.NumberFountains.DtbConsignmentID; }
		}

		protected override DtbTransportConsolidation GetNewConsolidation()
		{
			return Factory.New<DtbConsignmentConsolidation>();
		}
	}
}
