using System;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists
{
	public class BaseKeyValuesXML : IKeyValues
	{
		public BaseKeyValuesXML(XElement entry, string codeName)
		{
			this.entry = entry;
			SetDateTime(CodeListsConstants.XMLEntryElementNames.START_DATE, ref startDateDetails);
			SetDateTime(CodeListsConstants.XMLEntryElementNames.END_DATE, ref endDateDetails);
			this.codeName = codeName;
		}
		readonly XElement entry;
		readonly string codeName;

		public virtual string Code => code ?? (code = entry.Element(codeName)?.Value);
		string code;

		public virtual string Description => description ?? (description = entry.Element(CodeListsConstants.XMLEntryElementNames.DESCRIPTION)?.Value);
		string description;

		public DateTime StartDate => startDateDetails.DateTime;

		public bool StartDateSuccessfullyParsed => startDateDetails.SuccessfullyParsed;

		public DateTime EndDate => endDateDetails.DateTime;

		public bool EndDateSuccessfullyParsed => endDateDetails.SuccessfullyParsed;

		public bool EndDateIsExpired => Helper.CheckExpiredDateTime(EndDate, Constants.GermanTimeZoneID);

		public virtual string UniqueCode => Code;

		void SetDateTime(string elementName, ref (bool SuccessfullyParsed, DateTime DateTime) propertyToSet)
		{
			var dateString = entry.Element(elementName)?.Value;
			if (string.IsNullOrWhiteSpace(dateString))
			{
				if (elementName.Equals(CodeListsConstants.XMLEntryElementNames.START_DATE, StringComparison.Ordinal))
				{
					propertyToSet.DateTime = Constants.MinimumDateTime;
				}
				else if (elementName.Equals(CodeListsConstants.XMLEntryElementNames.END_DATE, StringComparison.Ordinal))
				{
					propertyToSet.DateTime = Constants.MaximumDateTime;
				}
				propertyToSet.SuccessfullyParsed = true;
			}
			else
			{
				propertyToSet = Helper.GetDateTime(dateString, CodeListsConstants.SourceDateFormatXML);
			}
		}

		public void SetStartDateDetails(bool successfullyParsed, DateTime dateTime) => startDateDetails = (successfullyParsed, dateTime);

		(bool SuccessfullyParsed, DateTime DateTime) startDateDetails;
		(bool SuccessfullyParsed, DateTime DateTime) endDateDetails;


		protected XElement Entry => entry;

		protected string CodeName => codeName;
	}
}
