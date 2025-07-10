using System;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public class DateTimeProvider : IDateTimeProvider
	{
		public DateTime GetUTCNow()
		{
			return DateTime.UtcNow;
		}
	}
}
