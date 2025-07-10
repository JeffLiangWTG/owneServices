using System;

namespace CargoWise.RefDbRepo.SEReferenceData.Business
{
	public interface IDateTimeProvider
	{
		DateTime CurrentLocalDate { get; }
	}
}
