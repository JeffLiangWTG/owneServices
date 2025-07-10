using System;
using CargoWise.RefDbRepo.PLReferenceData.Services.Interfaces;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Providers
{
	public class DateTimeProvider : IDateTimeProvider
	{
		public DateTime CurrentLocalDateTime => DateTime.Now;
	}
}
