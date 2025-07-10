using CargoWise.Integration;
using Enterprise.Customs.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using CodeDescriptionPair = Enterprise.ZArchitecture.Core.CodeDescriptionPair;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class EntryNumberDataObjectWriter : DataObjectWriter<CusEntryNumber, EntryNumber>
	{
		public EntryNumberDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override EntryNumber PopulateDataObject(CusEntryNumber entryNumberBO)
		{
			var entryNumberDataObject = new EntryNumber();

			entryNumberDataObject.CountryOfIssue = ListHelper.GetWithName<Country>(entryNumberBO.CE_RN_NKCountryCode, entryNumberBO.Lookups.Countries);
			entryNumberDataObject.EntryIsSystemGenerated = entryNumberBO.CE_EntryIsSystemGenerated;
			entryNumberDataObject.EntryLineReference = entryNumberBO.CE_EntryLineReference;

			var entryStatus = entryNumberBO.CE_EntryStatus;
			entryNumberDataObject.EntryStatus = entryStatus.IsEmpty ? null : new EntryStatus { Code = entryStatus };
			entryNumberDataObject.ExpiryDate = entryNumberBO.CE_ExpiryDate;
			entryNumberDataObject.IssueDate = entryNumberBO.CE_IssueDate;
			entryNumberDataObject.Number = entryNumberBO.CE_EntryNum;

			var entryNumberTypeList = new CodeDescriptionPairList();

			var entryNumberType = GetFromEntryNumberTypeList(entryNumberBO, false)
				?? GetFromEntryNumberTypeList(entryNumberBO, true);

			if (entryNumberType != null)
			{
				entryNumberTypeList.Add(entryNumberType);
			}

			entryNumberDataObject.Type = ListHelper.GetWithDescription<EntryType>(entryNumberBO.CE_EntryType, entryNumberTypeList);

			return entryNumberDataObject;
		}

		ICodeDescription GetFromEntryNumberTypeList(CusEntryNumber entryNumberBO, bool isImport)
		{
			var codeDescriptionList = CusEntryNumberTypes.CountrySpecificCustomsEntryNumberTypeList(entryNumberBO.Factory, entryNumberBO.CE_RN_NKCountryCode, isImport);
			var mappedCode = CusEntryNumber.GetEntryNumberTypeForDisplay(entryNumberBO.CE_EntryType, entryNumberBO.CE_RN_NKCountryCode, isImport);

			return codeDescriptionList.ContainsCode(mappedCode)
				? new CodeDescriptionPair((string)entryNumberBO.CE_EntryType, codeDescriptionList.GetDescriptionFromCode(mappedCode))
				: null;
		}
	}
}
