using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ImportEntryCreationStrategy : EntryCreationStrategy
{
	public ImportEntryCreationStrategy(JobDeclaration declaration)
		: base(declaration)
	{
	}

	public override MergeKey GetKeyForLine(BaseJobComInvoiceLine invoiceLine)
	{
		var line = (JobComInvoiceLine)invoiceLine;
		var result = base.GetKeyForLine(invoiceLine);

		result.Add(line.ZG_CountryOfSupply);
		result.Add(line.JI_DateForDutyOverride);
		result.Add(line.JI_ValuationDateOverride);
		if (!line.JI_BrandName.IsEmpty)
		{
			result.Add(line.PK);
		}

		return result;
	}

	protected override string[] GetSupportingDocumentKeysCore() => new[]
	{
		SupportingDocument.Schema.CSI_Code,
		SupportingDocument.Schema.CSI_ReferenceNumber,
		SupportingDocument.Schema.CSI_Status,
		SupportingDocument.Schema.CSI_UnitOfQuantity,
		SupportingDocument.Schema.CSI_RN_NKCountryCode,
		SupportingDocument.Schema.CSI_ReferenceNumber2,
		SupportingDocument.Schema.CSI_Description,
	};

	protected override string[] GetPreviousDocumentKeysCore() => new[]
	{
		PreviousDocument.Schema.CSI_Code,
		PreviousDocument.Schema.CSI_SubType,
		PreviousDocument.Schema.CSI_ReferenceNumber,
		PreviousDocument.Schema.CSI_LineNo,
	};
}
