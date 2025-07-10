using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ImportJobDeclarationOfficeCodeValidation : OfficeCodeValidation
{
	public ImportJobDeclarationOfficeCodeValidation(OfficeCode parent) : base(parent)
	{
	}

	JobDeclaration Declaration => (JobDeclaration)Parent.Parent;

	protected override void CheckCY_Data()
	{
		base.CheckCY_Data();
		CheckRuleR1586();
	}

	protected override void CheckIfOfficeTypeEmptyOrInvalid()
	{
	}

	void CheckRuleR1586()
	{
		var customsOffice = Declaration?.JE_CustomsOffice ?? ZString.Empty;
		if ((Parent.CY_Data.Left(6) != customsOffice.Left(6) || Parent.CY_Data.IsEmpty)
			&& Parent.CY_Code == EuOfficeCodesTypes.Codes.AuthorityControlCode
			&& HasAdditionalDocumentCode(Constants.AdditionalInfoCodes._00100))
		{
			Parent.CY_DataInfo.AddMessageError(Res.GetString("ImportPLOfficeCode|CheckRuleR1586"
				, "(R1586) – If there is an Additional Information code 00100, the Code of the Supervising Customs Office (SCO) is required and the first 6 characters must be the same as the first 6 characters of the Decl.Customs Office."));
		}
	}

	bool HasAdditionalDocumentCode(ZString documentCode)
	{
		return Declaration.HasAdditionalInfoCode(documentCode)
				|| Declaration.Invoices.Cast<JobComInvoiceHeader>().Any(x => x.HasAdditionalInfoCode(documentCode))
				|| Declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.HasAdditionalInfoCode(documentCode));
	}
}
