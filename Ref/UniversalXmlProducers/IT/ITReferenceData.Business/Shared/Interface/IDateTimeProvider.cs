using System;

namespace CargoWise.RefDbRepo.ITReferenceData.Business
{
	public interface IDateTimeProvider
	{
		DateTime Now { get; }
	}
}
