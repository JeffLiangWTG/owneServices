using System;

namespace CargoWise.eServices.Billing.WcfService
{
	public interface IDateTimeProvider
	{
		DateTime DateTimeNow { get; }
		DateTime UtcNow { get; }
	}

	public class DateTimeProvider : IDateTimeProvider
	{
		public DateTime DateTimeNow
		{
			get { return DateTime.Now; }
		}

		public DateTime UtcNow
		{
			get { return DateTime.UtcNow; }
		}
	}
}
