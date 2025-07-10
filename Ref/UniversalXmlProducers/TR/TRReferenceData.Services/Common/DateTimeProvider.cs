using System;

namespace CargoWise.RefDbRepo.TRReferenceData.Services
{
	public class DateTimeProvider : IDateTimeProvider
	{
		public DateTime GetNow() => DateTime.Now;
	}
}
