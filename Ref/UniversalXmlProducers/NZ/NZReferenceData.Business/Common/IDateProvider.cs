using System;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	public interface IDateProvider
	{
		DateTime Today { get; }
		DateTime ActiveDate { get; }
	}
}
