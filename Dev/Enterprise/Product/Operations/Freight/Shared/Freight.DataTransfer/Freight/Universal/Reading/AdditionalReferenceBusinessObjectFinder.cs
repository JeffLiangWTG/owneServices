using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class AdditionalReferenceBusinessObjectFinder : MatchingBusinessObjectFinder<AdditionalReference, CusEntryNumber>
	{
		public AdditionalReferenceBusinessObjectFinder(AdditionalReference dataObject)
			: base(dataObject)
		{
		}

		#region Implementation

		protected override CusEntryNumber FindCore(IEnumerable<CusEntryNumber> entryNumbers)
		{
			numberTypeCodeIsUnique = null;

			var type = dataObject.Type.GetCodeAsUpperCase();
			var referenceNumber = dataObject.ReferenceNumber.GetValueOrDefault();

			if (!type.IsEmpty)
			{
				foreach (CusEntryNumber entryNumber in entryNumbers)
				{
					if (entryNumber.CE_EntryType == type
						&& entryNumber.CE_Category == CusEntryNumber.Categories.AdditionalReferenceNumber)
					{
						if (IsNumberTypeUnique(entryNumber, type) || entryNumber.CE_EntryNum == referenceNumber || IsMatchedForSpecialBusinessCase(entryNumber))
						{
							return entryNumber;
						}
					}
				}
			}

			return null;
		}

		bool? numberTypeCodeIsUnique;

		bool IsNumberTypeUnique(CusEntryNumber entryNumber, string typeCode)
		{
			if (!numberTypeCodeIsUnique.HasValue)
			{
				var nonUniqueCodes = entryNumber.Lookups.AdditionalReferenceNumberTypes.NonUnique();
				numberTypeCodeIsUnique = !nonUniqueCodes.Contains(typeCode);
			}

			return numberTypeCodeIsUnique.Value;
		}

		bool IsMatchedForSpecialBusinessCase(CusEntryNumber entryNumber)
		{
			return entryNumber.CE_EntryType == GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber
					&& entryNumber.CE_EntryIsSystemGenerated;
		}

		#endregion
	}
}
