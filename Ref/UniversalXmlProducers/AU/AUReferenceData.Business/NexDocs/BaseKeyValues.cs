using System;
using CargoWise.RefDbRepo.AUReferenceData.Services;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class BaseKeyValues : IKeyValues
	{
		public BaseKeyValues(IItemCodeSet[] listItemCodeSet)
		{
			ListItemCodeSet = listItemCodeSet;
			StartDateDetails = ListItemCodeSet.GetDateTimeCodeValue(NexDocConstants.ItemCodeSetKeys.StartDate);
		}
		readonly IItemCodeSet[] ListItemCodeSet;
		readonly (bool SuccessfullyParsed, DateTime DateTime) StartDateDetails;

		public string Code => code ?? (code = ListItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.Code));
		string code;

		public string SecondaryCode => null;

		public string Description => description ?? (description = ListItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.Description).ToUpperInvariant());
		string description;

		public bool IsStartDateSuccesfullyParsed => StartDateDetails.SuccessfullyParsed;

		public DateTime StartDate => StartDateDetails.DateTime;

		public string UniqueCode => Code;
	}
}
