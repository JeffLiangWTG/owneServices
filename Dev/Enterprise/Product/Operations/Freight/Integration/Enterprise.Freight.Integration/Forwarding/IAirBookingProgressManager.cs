using System;

namespace Enterprise.Freight.Integration
{
	public interface IAirBookingProgressManager
	{
		IDisposable Show(Action onUserRequestedCancel);
	}
}
