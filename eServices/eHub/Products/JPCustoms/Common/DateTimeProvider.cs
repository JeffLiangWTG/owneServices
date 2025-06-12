using System;

namespace CargoWise.eHub.Products.JPCustoms.Common
{
	public interface IDateTimeProvider
	{
		DateTime DateTimeNow { get; }
	}

	public class DateTimeProvider : IDateTimeProvider
	{
		public DateTime DateTimeNow
		{
			get
			{
				return DateTime.Now;
			}
		}
	}
}
