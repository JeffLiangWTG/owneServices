using System;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public interface IKeyValues
	{
		string Code { get; }

		string SecondaryCode { get; }

		string Description { get; }

		bool IsStartDateSuccesfullyParsed { get; }

		DateTime StartDate { get; }

		string UniqueCode { get; }
	}
}
