using System;

namespace CargoWise.RefDbRepo.IEReferenceData.Services
{
	public interface IDateTimeProvider
	{
		DateTime CurrentLocalDate { get; }
	}
}
