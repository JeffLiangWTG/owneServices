using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ImportSupportingDocumentValidation : SupportingDocumentValidation
{
	public ImportSupportingDocumentValidation(SupportingDocument parent)
		: base(parent)
	{
	}

	#region CSI_Code

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();
		CheckRuleR956();
		CheckRuleR626();
		CheckRuleR629();
		CheckRuleR632();
		CheckRuleR258();
	}

	void CheckRuleR956()
	{
		if (Parent.CSI_Code == SupportingDocumentCodes.C651)
		{
			if (AnyRelatedInstruction(SubStyleCodes.C) && AdditionalInfosCode4PL12Exists())
			{
				Parent.CSI_CodeInfo.AddMessageError(Res.GetString("ImportSupportingDocumentValidation|CheckRuleR956|C4PL12"
					, "(R956) Supporting document C651 cannot be presented. - Additional Info Code '4PL12' & Sub Style 'C' exists."));
			}
			if (!ConcessionF06ExistsForProcedureCode45Or68())
			{
				Parent.CSI_CodeInfo.AddMessageError(Res.GetString("ImportSupportingDocumentValidation|CheckRuleR956|F06"
					, "(R956) Supporting document C651 cannot be presented. - Procedure Code '45' or '68' with Concession 'F06' does not exist."));
			}
		}
	}

	bool ConcessionF06ExistsForProcedureCode45Or68()
	{
		switch (Parent.Parent)
		{
			case JobComInvoiceLine invoiceLine when InvoiceLineContainsProcedureCode45Or68AndConcessionF06Exists(invoiceLine):
			case JobComInvoiceHeader invHeader when invHeader.InvoiceLines.Cast<JobComInvoiceLine>()
				.Any(inv => InvoiceLineContainsProcedureCode45Or68AndConcessionF06Exists(inv)):
			case JobDeclaration declaration when declaration.InvoiceLines.Cast<JobComInvoiceLine>()
				.Any(inv => InvoiceLineContainsProcedureCode45Or68AndConcessionF06Exists(inv)):
				return true;
			default:
				return false;
		}
	}

	bool InvoiceLineContainsProcedureCode45Or68AndConcessionF06Exists(JobComInvoiceLine invoiceLine) => ProcedureCode45Or68Exists(invoiceLine.ProcedureCodeBase)
																										&& invoiceLine.AdditionalProcedureCodes.Cast<AdditionalProcedureCode>()
																											.Any(x => x.CY_Code == ConcessionCodes.F06);

	bool ProcedureCode45Or68Exists(ZString procedureCode) => (procedureCode == ProcedureCodes._45 || procedureCode == ProcedureCodes._68);

	bool AdditionalInfosCode4PL12Exists() => Parent.HasAdditionalDocumentCode(AdditionalInfoCodes._4PL12);

	void CheckRuleR626()
	{
		if (Parent.CSI_Code == Constants.SupportingDocumentCodes.C601
			&& Parent.HasEntryInstructionProcedureCode(Constants.ProcedureCodes._51)
			&& Parent.HasAdditionalDocumentCode(Constants.AdditionalInfoCodes._00100))
		{
			Parent.CSI_CodeInfo.AddMessageError(Res.GetString("ImportSupportingDocumentValidation|CheckRuleR626"
				, "(R626) Additional Information code 00100 and Supporting Document/Authorization code C601 cannot be used together."));
		}
	}

	void CheckRuleR629()
	{
		if (Parent.CSI_Code == Constants.SupportingDocumentCodes.C516
			&& Parent.HasEntryInstructionProcedureCode(Constants.ProcedureCodes._53)
			&& Parent.HasAdditionalDocumentCode(Constants.AdditionalInfoCodes._00100))
		{
			Parent.CSI_CodeInfo.AddMessageError(Res.GetString("ImportSupportingDocumentValidation|CheckRuleR629",
				"(R629) Additional Information code 00100 and Supporting Document/Authorization code C516 cannot be used together."));
		}
	}

	void CheckRuleR632()
	{
		if (Parent.CSI_Code == Constants.SupportingDocumentCodes.C019
			&& Parent.HasEntryInstructionProcedureCode(Constants.ValidationLists.R632Procedures(Parent.Factory))
			&& Parent.HasAdditionalDocumentCode(Constants.AdditionalInfoCodes._00100)
			&& !Parent.HasAdditionalDocumentCode(Constants.AdditionalInfoCodes._4PL09))
		{
			Parent.CSI_CodeInfo.AddMessageError(Res.GetString("ImportSupportingDocumentValidation|CheckRuleR632",
				"(R632) Additional Information code 00100 and Supporting Document/Authorization code C019 can coexist only with Additional Information code 4PL09."));
		}
	}

	void CheckRuleR258()
	{
		var parent = Parent;

		if (parent.CSI_Code == Constants.SupportingDocumentCodes.N018
			&& parent.Declaration is JobDeclaration declaration
			&& declaration.JE_EntryStyle == EntryStyleListExport.Codes.ExportToEFTAMember
			&& HasR258InvoiceLine())
		{
			parent.CSI_CodeInfo.AddMessageError(Res.GetString("ImportSupportingDocumentValidation|CheckRuleR258",
				"(R258) 'N018' supporting document is not allowed for requested procedure code 40 with preference code 400/420 and procedure details F15/F16."));
		}
	}

	bool HasR258InvoiceLine()
	{
		var result = false;
		switch (Parent.Parent)
		{
			case JobComInvoiceLine invoiceLine when IsR258(invoiceLine):
			case JobComInvoiceHeader invoiceHeader when invoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>()
				.Any(invoiceHeaderLine => IsR258(invoiceHeaderLine)):
			case JobDeclaration declaration when declaration.InvoiceLines.Cast<JobComInvoiceLine>()
				.Any(declarationLine => IsR258(declarationLine)):
				result = true;
				break;
		}
		return result;
	}

	bool IsR258(JobComInvoiceLine invoiceLine)
	{
		return invoiceLine != null
				&& invoiceLine.EntryInstruction.HasEntryInstructionProcedureCode(ProcedureCodes._40)
				&& IsPrefernceCode400Or420()
				&& invoiceLine.AdditionalProcedureCodes.Cast<AdditionalProcedureCode>().Any(IsProcedureDeatilsF15OrF16);

		bool IsPrefernceCode400Or420()
		{
			var primaryPreference = invoiceLine.JI_PrimaryPreference;
			return primaryPreference == PrimaryPreferenceCodes._400 || primaryPreference == PrimaryPreferenceCodes._420;
		}

		bool IsProcedureDeatilsF15OrF16(AdditionalProcedureCode additionalProcedureCode)
		{
			var concession = additionalProcedureCode?.CY_Code ?? ZString.Empty;
			return concession == ConcessionCodes.F15 || concession == ConcessionCodes.F16;
		}
	}

	#endregion

	#region CSI_ReferenceNumber

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		CheckRuleR900();
		CheckRuleR922();
	}

	void CheckRuleR900()
	{
		if (Parent.CSI_Code == SupportingDocumentCodes.C513
			&& !Parent.CSI_ReferenceNumber.Contains(ReferenceNumberSpecialNumbers.CCL)
			&& AnyRelatedInstruction(new ZString[] { SubStyleCodes.A, SubStyleCodes.B, SubStyleCodes.C, SubStyleCodes.D, SubStyleCodes.E, SubStyleCodes.F, SubStyleCodes.X, SubStyleCodes.Y, SubStyleCodes.Z }))
		{
			Parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("ImportSupportingDocumentValidation|CheckRuleR900", "(R900) Supporting document number (for C513 code) must contain CCL text."));
		}
	}

	void CheckRuleR922()
	{
		switch (Parent.CSI_Code)
		{
			case SupportingDocumentCodes.C512 when AnyRelatedInstruction(new ZString[] { SubStyleCodes.C, SubStyleCodes.F, SubStyleCodes.Y }) && !ReferenceNumberContainsAny(new ZString[] { ReferenceNumberSpecialNumbers.SDE, ReferenceNumberSpecialNumbers.ZW, ReferenceNumberSpecialNumbers.ZS }):
				Parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("ImportSupportingDocumentValidation|CheckRuleR922C512", "(R922) Supporting document number (for C512 code) must contain SDE, ZW or ZS text."));
				break;
			case SupportingDocumentCodes.C514 when AnyRelatedInstruction(SubStyleCodes.Z) && !ReferenceNumberContainsAny(new ZString[] { ReferenceNumberSpecialNumbers.EIR, ReferenceNumberSpecialNumbers.RW, ReferenceNumberSpecialNumbers.ZS }):
				Parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("ImportSupportingDocumentValidation|CheckRuleR922C514", "(R922) Supporting document number (for C514 code) must contain EIR, RW or ZS text."));
				break;
		}
	}

	bool AnyRelatedInstruction(ZString subStyle)
	{
		return GetRelatedEntryInstructions().Any(x => x.CEI_SubStyle == subStyle);
	}

	bool AnyRelatedInstruction(ZString[] subStyleList)
	{
		return GetRelatedEntryInstructions().Any(x => subStyleList.Contains(x.CEI_SubStyle));
	}

	bool ReferenceNumberContainsAny(IEnumerable<ZString> candidates)
	{
		var referenceNumber = Parent.CSI_ReferenceNumber;
		return candidates.Any(x => referenceNumber.Contains(x));
	}

	#endregion

	protected override void CheckCSI_UnitOfQuantity()
	{
		if (Parent.IsParentInvoiceLine)
		{
			base.CheckCSI_UnitOfQuantity();
		}
	}

	protected override void CheckCSI_Quantity()
	{
		if (Parent.IsParentInvoiceLine)
		{
			base.CheckCSI_Quantity();
		}
	}
}
