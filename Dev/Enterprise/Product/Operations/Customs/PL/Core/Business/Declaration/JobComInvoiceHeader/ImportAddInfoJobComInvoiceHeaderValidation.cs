using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ImportAddInfoJobComInvoiceHeaderValidation : AddInfoJobComInvoiceHeaderValidation
{
	public ImportAddInfoJobComInvoiceHeaderValidation(AddInfoJobComInvoiceHeader parent)
		: base(parent)
	{
	}

	protected override void CheckZG_ValuationMethod()
	{
		base.CheckZG_ValuationMethod();

		if (Parent.ZG_ValuationMethod.IsEmpty)
		{
			CheckRuleR284();
		}
		else
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_ValuationMethodInfo);
		}
	}

	void CheckRuleR284()
	{
		if (InvoiceHeader is JobComInvoiceHeader invoiceHeader
			&& invoiceHeader.CusEntryInstructions.Cast<CusEntryInstruction>()
				.Any(x => !IsProcedure71Or76(x))
			&& invoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>()
				.Any(x => !HasConcessionCode2PL(x)))
		{
			Parent.ZG_ValuationMethodInfo.AddMessageError(Res.GetString("PLImportTranCircumstanceValidation|R284", "(R284) You have not entered a Valuation Method."));
		}

		bool IsProcedure71Or76(CusEntryInstruction entryInstruction) => entryInstruction != null
																		&& (entryInstruction.CEI_Procedure == Constants.ProcedureCodes._71
																			|| entryInstruction.CEI_Procedure == Constants.ProcedureCodes._76);

		bool HasConcessionCode2PL(JobComInvoiceLine invoiceLine) => invoiceLine.AdditionalProcedureCodes
			.Cast<AdditionalProcedureCode>()
			.Any(x => x.CY_Code == Constants.ConcessionCodes._2PL);
	}
}
