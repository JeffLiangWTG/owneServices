using System;

namespace CargoWise.RefDbRepo.DEReferenceData.Services
{
	public interface IDateTimeProvider
	{
		DateTimeOffset CurrentLocalDateTimeOffset { get; }

		DateTime CurrentLocalDate { get; }
	}
}
