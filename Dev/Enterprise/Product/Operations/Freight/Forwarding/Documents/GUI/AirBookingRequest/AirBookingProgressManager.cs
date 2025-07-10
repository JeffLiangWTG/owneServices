using System;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.Documents.GUI
{
	public sealed class AirBookingProgressManager : IAirBookingProgressManager
	{
		public IDisposable Show(Action onUserRequestedCancel)
		{
			var manager = new AirBookingProgressFormManager(onUserRequestedCancel);
			manager.Start();
			return manager;
		}
	}
}
