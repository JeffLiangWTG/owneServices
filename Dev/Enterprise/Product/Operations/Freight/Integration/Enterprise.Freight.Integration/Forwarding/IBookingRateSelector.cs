using System.Collections.Generic;

namespace Enterprise.Freight.Integration
{
	public interface IBookingRateSelector
	{
		IBookingRate SelectRate(IReadOnlyCollection<IBookingRate> rates);
	}
}
