using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using ICusEntryNumber = Enterprise.Integration.Customs.ICusEntryNumber;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class AdditionalReferenceDataObjectWriter : DataObjectWriter<BusinessObject, AdditionalReference>
	{
		public AdditionalReferenceDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{ }

		protected override AdditionalReference PopulateDataObject(BusinessObject additionalReferenceBO)
		{
			var cusEntryNumberBO = (ICusEntryNumber)additionalReferenceBO;

			Country countryOfIssue = null;

			if (!cusEntryNumberBO.CE_RN_NKCountryCode.IsEmpty)
			{
				countryOfIssue = new Country()
				{
					Code = cusEntryNumberBO.CE_RN_NKCountryCode,
					Name = cusEntryNumberBO.Lookups.CountriesAsCodeDescriptionList.GetDescriptionFromCode(cusEntryNumberBO.CE_RN_NKCountryCode)
				};
			}

			return new AdditionalReference()
			{
				ContextInformation = cusEntryNumberBO.CE_EntryLineReference,
				IssueDate = cusEntryNumberBO.CE_IssueDate,
				ReferenceNumber = cusEntryNumberBO.CE_EntryNum,
				Type = ListHelper.GetWithDescription<EntryType>(cusEntryNumberBO.CE_EntryType, cusEntryNumberBO.Lookups.AdditionalReferenceNumberTypes),
				CountryOfIssue = countryOfIssue
			};
		}
	}
}
