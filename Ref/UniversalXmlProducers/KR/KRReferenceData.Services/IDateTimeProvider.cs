using System;

namespace CargoWise.RefDbRepo.KRReferenceData.Services
{
	public interface IDateTimeProvider
	{
		DateTimeOffset CurrentLocalDateTimeOffset { get; }

		DateTime CurrentLocalDate { get; }
	}
}
