using System;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class NoShowAirBookingProgressManager : IAirBookingProgressManager
	{
		public IDisposable Show(Action onUserRequestedCancel)
		{
			return new DummyDisposable();
		}

		sealed class DummyDisposable : IDisposable
		{
			public void Dispose()
			{
			}
		}
	}
}
