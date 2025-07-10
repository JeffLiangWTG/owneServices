using System;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists
{
	public interface IKeyValues
	{
		string Code { get; }
		string Description { get; }
		DateTime StartDate { get; }
		bool StartDateSuccessfullyParsed { get; }
		DateTime EndDate { get; }
		bool EndDateSuccessfullyParsed { get; }
		bool EndDateIsExpired { get; }
		string UniqueCode { get; }
		void SetStartDateDetails(bool successfullyParsed, DateTime dateTime);
	}
}
