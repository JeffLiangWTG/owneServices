using System;

namespace CargoWise.Billing.Kafka.API
{
	public interface IDateTimeProvider
	{
		DateTime DateTimeNow { get; }
	}

	public class DateTimeProvider : IDateTimeProvider
	{
		public DateTime DateTimeNow
		{
			get { return DateTime.Now; }
		}
	}
}
