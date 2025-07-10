using System;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists
{
	public class BaseKeyValuesTSV : IKeyValues
	{
		public BaseKeyValuesTSV(string[] record)
		{
			this.record = record;
			SetDateTime(CodeListsConstants.TSVColumnIndexes.VALID_FROM, ref startDateDetails);
			SetDateTime(CodeListsConstants.TSVColumnIndexes.VALID_TO, ref endDateDetails);
		}
		readonly string[] record;

		public string Code => code ?? (code = record[CodeListsConstants.TSVColumnIndexes.CODE]);
		string code;

		public virtual string Description => description ?? (description = record[CodeListsConstants.TSVColumnIndexes.DESCRIPTION]);
		string description;

		public DateTime StartDate => startDateDetails.DateTime;

		public bool StartDateSuccessfullyParsed => startDateDetails.SuccessfullyParsed;

		public DateTime EndDate => endDateDetails.DateTime;

		public bool EndDateSuccessfullyParsed => endDateDetails.SuccessfullyParsed;

		public bool EndDateIsExpired => Helper.CheckExpiredDateTime(EndDate, Constants.GermanTimeZoneID);

		public string UniqueCode => Code;

		void SetDateTime(int typeOfDate, ref (bool SuccessfullyParsed, DateTime DateTime) propertyToSet)
		{
			var dateTimeAsString = record[typeOfDate];
			if (string.IsNullOrWhiteSpace(dateTimeAsString))
			{
				if (typeOfDate.Equals(CodeListsConstants.TSVColumnIndexes.VALID_FROM))
				{
					propertyToSet.DateTime = Constants.MinimumDateTime;
				}
				else if (typeOfDate.Equals(CodeListsConstants.TSVColumnIndexes.VALID_TO))
				{
					propertyToSet.DateTime = Constants.MaximumDateTime;
				}
				propertyToSet.SuccessfullyParsed = true;
			}
			else
			{
				propertyToSet = Helper.GetDateTime(dateTimeAsString, CodeListsConstants.SourceDateFormatTSV);
			}
		}

		public void SetStartDateDetails(bool successfullyParsed, DateTime dateTime) => startDateDetails = (successfullyParsed, dateTime);

		(bool SuccessfullyParsed, DateTime DateTime) startDateDetails;
		(bool SuccessfullyParsed, DateTime DateTime) endDateDetails;
	}
}
