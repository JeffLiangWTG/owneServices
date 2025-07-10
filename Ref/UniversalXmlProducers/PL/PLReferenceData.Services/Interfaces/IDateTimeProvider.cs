using System;

namespace CargoWise.RefDbRepo.PLReferenceData.Services.Interfaces
{
	public interface IDateTimeProvider
	{
		DateTime CurrentLocalDateTime { get; }
	}
}
