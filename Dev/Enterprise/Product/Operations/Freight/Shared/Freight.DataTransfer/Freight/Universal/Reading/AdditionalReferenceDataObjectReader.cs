using System;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class AdditionalReferenceDataObjectReader : DataObjectReader<AdditionalReference, CusEntryNumber>
	{
		public AdditionalReferenceDataObjectReader(AdditionalReference dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IEnumerable<CusEntryNumber> additionalReferences)
			: base(dataObject, logger, factory)
		{
			Argument.NotNull(additionalReferences, "IEnumerable<CusEntryNumber> additionalReferences");
			additionalReferenceBizObjProvider = dataObj => new AdditionalReferenceBusinessObjectFinder(dataObj).Find(additionalReferences);
		}

		public AdditionalReferenceDataObjectReader(AdditionalReference dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, Func<AdditionalReference, CusEntryNumber> additionalReferenceBizObjProvider)
			: base(dataObject, logger, factory)
		{
			this.additionalReferenceBizObjProvider = Argument.NotNull(additionalReferenceBizObjProvider, "additionalReferenceBizObjProvider");
		}

		readonly Func<AdditionalReference, CusEntryNumber> additionalReferenceBizObjProvider;

		#region Implementation

		protected override CusEntryNumber GetExistingBusinessObject()
		{
			return additionalReferenceBizObjProvider(dataObject);
		}

		protected override void PopulateBusinessObject(CusEntryNumber additionalReference)
		{
			var isOutdatedSZBEntryNumber = dataObject.Type.GetCodeAsUpperCase() == GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber
				&& dataObject.IssueDate != null && dataObject.IssueDate < additionalReference.CE_IssueDate;

			if (!isOutdatedSZBEntryNumber)
			{
				SetValue(additionalReference, CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.AdditionalReferenceNumber);
				SetValue(additionalReference, CusEntryNumSchema.CE_EntryLineReference, dataObject.ContextInformation);
				SetValue(additionalReference, CusEntryNumSchema.CE_EntryNum, dataObject.ReferenceNumber);
				SetValue(additionalReference, CusEntryNumSchema.CE_EntryType, dataObject.Type);
				SetValue(additionalReference, CusEntryNumSchema.CE_IssueDate, dataObject.IssueDate);
			}

			if ((dataObject.Type?.Code ?? string.Empty) == GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber)
			{
				SetValue(additionalReference, CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Germany);
			}
		}

		#endregion
	}
}
