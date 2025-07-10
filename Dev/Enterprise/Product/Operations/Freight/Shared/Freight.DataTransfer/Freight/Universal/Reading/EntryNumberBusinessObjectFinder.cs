using System.Collections.Generic;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class EntryNumberBusinessObjectFinder : MatchingBusinessObjectFinder<EntryNumber, CusEntryNumber>
	{
		public EntryNumberBusinessObjectFinder(EntryNumber dataObject)
			: base(dataObject)
		{
		}

		protected override CusEntryNumber FindCore(IEnumerable<CusEntryNumber> entryNumbers)
		{
			var entryNum = dataObject.Number;
			var type = dataObject.Type.GetCodeAsUpperCase();
			var countryCode = dataObject.CountryOfIssue.GetCodeAsUpperCase();

			if (!type.IsEmpty)
			{
				foreach (CusEntryNumber entryNumber in entryNumbers)
				{
					if (entryNumber.CE_EntryType == type
						&& entryNumber.CE_RN_NKCountryCode == countryCode)
					{
						if (entryNumber.CE_Category == CusEntryNumber.Categories.CustomsPermitClearanceNumber)
						{
							if (entryNumber.CE_EntryType == GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber)
							{
								return entryNumber;
							}
							else if (entryNumber.CE_EntryNum.Equals(entryNum))
							{
								entryNumber.CE_EntryIsSystemGenerated = true;
								return entryNumber;
							}
							else if (type == entryNumber.CE_EntryType && entryNumber.CE_EntryNum.IsEmpty)
							{
								return entryNumber;
							}
						}
					}
				}
			}

			return null;
		}
	}
}
