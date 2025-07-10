using System;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public interface IDateProvider
	{
		DateTime StartDate { get; }
		DateTime EndDate { get; }
	}
}
