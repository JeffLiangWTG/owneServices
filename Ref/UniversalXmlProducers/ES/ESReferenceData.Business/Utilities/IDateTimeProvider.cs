using System;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public interface IDateTimeProvider
	{
		DateTime CurrentLocalDate { get; }
	}
}
