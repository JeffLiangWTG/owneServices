using System;

namespace CargoWise.RefDbRepo.TRReferenceData.Services
{
	public interface IDateTimeProvider
	{
		DateTime GetNow();
	}
}
