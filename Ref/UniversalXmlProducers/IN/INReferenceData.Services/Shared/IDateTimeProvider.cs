using System;

namespace CargoWise.RefDbRepo.INReferenceData.Services
{
	public interface IDateTimeProvider
	{
		DateTime GetDateTimeNow();
		DateTime GetIndiaTime();
		DateTime GetIndiaToday();
	}
}
