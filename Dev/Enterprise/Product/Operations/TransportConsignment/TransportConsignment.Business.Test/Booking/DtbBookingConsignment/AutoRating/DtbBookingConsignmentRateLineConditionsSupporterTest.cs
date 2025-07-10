using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Testing;

namespace Enterprise.TransportConsignment.Business.Testing
{
	public class DtbBookingConsignmentRateLineConditionsSupporterTest : TransportRateLineConditionsSupporterTest
	{
		protected override TransportRateLineConditionsSupporter GetConditionsSupporter()
		{
			return new DtbBookingConsignmentRateLineConditionsSupporter(Factory.New<DtbBookingConsignment>());
		}
	}
}
