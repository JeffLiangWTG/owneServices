using System;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode
{
	public interface IRawAdditionalCode
	{
		string Code { get; }

		string Description { get; }

		DateTime? StartDate { get; }

		DateTime? EndDate { get; }
	}
}
