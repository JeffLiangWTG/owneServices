using System;

namespace Enterprise.Freight.Forwarding.Business
{
	public enum DeliveryDueTimeSource
	{
		ServiceLevel,
		ZoneItem,
		TransportProvider
	}

	public class DeliveryDueTime
	{
		public DeliveryDueTime(TimeSpan time, DeliveryDueTimeSource source)
		{
			this.Time = time;
			this.Source = source;
		}

		public TimeSpan Time { get; }

		public DeliveryDueTimeSource Source { get; }
	}
}
