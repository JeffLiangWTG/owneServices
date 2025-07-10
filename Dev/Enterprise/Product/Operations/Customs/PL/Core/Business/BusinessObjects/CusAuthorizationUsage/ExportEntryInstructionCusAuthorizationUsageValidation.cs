using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business;

public class ExportEntryInstructionCusAuthorizationUsageValidation : CusAuthorizationUsageValidation
{
	public ExportEntryInstructionCusAuthorizationUsageValidation(EU.Business.AutoCusAuthorizationUsage parent) : base(parent)
	{
	}

	protected new CusAuthorizationUsage Parent => (CusAuthorizationUsage)base.Parent;

	protected override void CheckAGC_Code()
	{
		base.CheckAGC_Code();

		CheckRuleR0031E();
		CheckRuleR0026E();
		CheckRuleR0027E();
		if (Instruction?.IsIE515BMessage ?? false)
		{
			CheckRuleG0066();
		}
		else
		{
			CheckRuleR0006E();
		}
	}

	void CheckRuleR0006E()
	{
		if (IsCentralizedCustomsDeclaration && (Instruction?.JobDeclaration?.CustomsOfficeOfPresentationReferenceNumber().IsEmpty ?? false))
		{
			Parent.AGC_CodeInfo.AddMessageError(Res.GetString("PLExportCusAuthorizationUsageValidation|CheckRuleR0006E"
				, "(R0006E) Office of Presentation is required for Centralized Clearance."));
		}
	}

	void CheckRuleG0066()
	{
		if (IsCentralizedCustomsDeclaration)
		{
			Parent.AGC_CodeInfo.AddMessageError(Res.GetString("PLExportCusAuthorizationUsageValidation|CheckRuleR338"
				, "(G0066) Centralized Clearance not allowed for Sub-style B and E."));
		}
	}

	bool IsCentralizedCustomsDeclaration => Parent.CustomsCode == CusAuthorizationUsageType.C513;

	void CheckRuleR0026E()
	{
		var code = Parent.CustomsCode;
		if (CodeIsFromR0026ECodeList()
			&& IsDuplicatedCode())
		{
			Parent.AGC_CodeInfo.AddMessageError(Res.GetString("PLExportCusAuthorizationUsageValidation|CheckRuleR0026E"
				, "(R0026E) For Authorization codes C512,C513,C514,C515 only one unique authorization number is allowed"));
		}

		bool IsDuplicatedCode() => Parent?.Instruction?.CusAuthorizationUsages
										.Cast<CusAuthorizationUsage>()
										.Any(auth => Parent != auth && auth.CustomsCode == code)
									?? false;

		bool CodeIsFromR0026ECodeList() => code == CusAuthorizationUsageType.C515
											|| code == CusAuthorizationUsageType.C514
											|| code == CusAuthorizationUsageType.C513
											|| code == CusAuthorizationUsageType.C512;
	}

	void CheckRuleR0027E()
	{
		var code = Parent.CustomsCode;
		if (CodeIsC512OrC514(code)
			&& CodesC512AndC514Exists())
		{
			Parent.AGC_CodeInfo.AddMessageError(Res.GetString("PLExportCusAuthorizationUsageValidation|CheckRuleR0027E"
				, "(R0027E) Authorization codes C512,C514 cannot appear together in one customs declaration"));
		}

		bool CodesC512AndC514Exists() => Parent?.Instruction?.CusAuthorizationUsages
											.Cast<CusAuthorizationUsage>()
											.Any(auth => Parent != auth
														&& auth.CustomsCode != code
														&& CodeIsC512OrC514(auth.CustomsCode))
										?? false;

		bool CodeIsC512OrC514(ZString agcCode) => agcCode == CusAuthorizationUsageType.C514
												|| agcCode == CusAuthorizationUsageType.C512;
	}

	bool HasSupportingDocumentCode(IReadOnlyCollection<ZString> supportingDocumentCodes) => Parent.Instruction is CusEntryInstruction instruction && (instruction.JobDeclaration.HasSupportingDocumentCode(supportingDocumentCodes)
																																				|| instruction.Invoices.Any() && instruction.Invoices.All(invoice => invoice.HasSupportingDocumentCode(supportingDocumentCodes))
																																				|| instruction.InvoiceLines.Any() && instruction.InvoiceLines.Cast<JobComInvoiceLine>().All(invoiceLine => invoiceLine.HasSupportingDocumentCode(supportingDocumentCodes))
		);

	bool HasEntryInstructionProcedureCode(ZString instructionProcedureCode) => Parent.Instruction is CusEntryInstruction instruction && instruction.CEI_Procedure == instructionProcedureCode;

	void CheckRuleR0031E()
	{
		var code = Parent.CustomsCode;
		if (((code == Constants.CusAuthorizationUsageType.C601 && HasEntryInstructionProcedureCode(Constants.ProcedureCodes._11))
			|| (code == Constants.CusAuthorizationUsageType.C019 && HasEntryInstructionProcedureCode(Constants.ProcedureCodes._21)))
			&& !HasSupportingDocumentCode(ValidationLists.R0031ESupportingDocuments(Parent.Factory)))
		{
			Parent.AGC_CodeInfo.AddMessageError(Res.GetString("PLExportCusAuthorizationUsageValidation|CheckRuleR0031E",
				"(R0031E) Supporting Document C710 or 4DK3 is required for each Entry Line"));
		}
	}

	protected override void CheckAGC_OH_Owner()
	{
		base.CheckAGC_OH_Owner();

		CheckRuleC0848();
	}

	void CheckRuleC0848()
	{
		var code = Parent.CustomsCode;
		if (code == CusAuthorizationUsageType.C626
			|| code == CusAuthorizationUsageType.C627)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AGC_OH_OwnerInfo, messagePrefix: "(C0848)");
		}
	}
}
