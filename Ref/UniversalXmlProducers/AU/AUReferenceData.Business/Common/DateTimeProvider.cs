using System;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class DateTimeProvider : IDateTimeProvider
	{
		public DateTime CurrentLocalDateTime => DateTime.Now;
	}
}
