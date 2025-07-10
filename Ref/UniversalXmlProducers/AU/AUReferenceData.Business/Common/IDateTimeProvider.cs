using System;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public interface IDateTimeProvider
	{
		DateTime CurrentLocalDateTime { get; }
	}
}
