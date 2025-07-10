using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ImportJobComInvoiceHeaderValidation : JobComInvoiceHeaderValidation
{
	public ImportJobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
	{
	}

	protected override void CheckTranCircumstanceCode1()
	{
		base.CheckTranCircumstanceCode1();

		if (Parent.TranCircumstanceCode1.IsEmpty)
		{
			CheckRuleR278();
		}
	}

	void CheckRuleR278()
	{
		if (Parent != null
			&& Parent.CusEntryInstructions.Cast<CusEntryInstruction>()
				.Any(x => !IsProcedure71Or76(x))
			&& Parent.InvoiceLines.Cast<JobComInvoiceLine>()
				.Any(x => !HasConcessionCode2PL(x)))
		{
			var propertyInfo = Parent.TranCircumstanceCode1Info;
			propertyInfo.AddMessageError(Res.GetString("PLImportTranCircumstanceValidation|R278", "(R278) You have not entered a {0}.", propertyInfo.HumanReadableName));
		}

		bool IsProcedure71Or76(CusEntryInstruction entryInstruction) => entryInstruction != null
																		&& (entryInstruction.CEI_Procedure == Constants.ProcedureCodes._71
																			|| entryInstruction.CEI_Procedure == Constants.ProcedureCodes._76);

		bool HasConcessionCode2PL(JobComInvoiceLine invoiceLine) => invoiceLine.AdditionalProcedureCodes
			.Cast<AdditionalProcedureCode>()
			.Any(x => x.CY_Code == Constants.ConcessionCodes._2PL);
	}

	protected override void CheckJZ_IncoTermPlace()
	{
		base.CheckJZ_IncoTermPlace();
		if (Parent.CusEntryInstructions.Any(x => x.CEI_Procedure != Constants.ProcedureCodes._71
												&& x.CEI_Procedure != Constants.ProcedureCodes._76))
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_IncoTermPlaceInfo);
		}
	}

	protected override void CheckJZ_OH_Supplier()
	{
		base.CheckJZ_OH_Supplier();

		var parent = Parent;

		if (parent.Supplier is OrgHeader orgHeader)
		{
			PLOrgHeaderValidationHelper.ValidateOrganizationName(orgHeader, parent.JZ_OH_SupplierInfo);
		}
	}
}
