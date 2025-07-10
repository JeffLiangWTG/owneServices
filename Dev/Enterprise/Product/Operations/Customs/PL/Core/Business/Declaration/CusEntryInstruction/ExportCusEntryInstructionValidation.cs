using System.Linq;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.PL.Business.Constants;
using EntrySubStyleListCode = Enterprise.Customs.EU.Business.EntrySubStyleList.Codes;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ExportCusEntryInstructionValidation : CusEntryInstructionValidation
{
	public ExportCusEntryInstructionValidation(CusEntryInstruction parent)
		: base(parent)
	{
	}

	public override void ValidateAll()
	{
		base.ValidateAll();

		CheckSameIncotermPlaceCodeUsed();
		CheckSameTransportChargesMoPUsed();

		CheckRuleR0224();
	}

	void CheckSameIncotermPlaceCodeUsed()
	{
		if (!Parent.Invoices.AllSame(x => x.ZG_AgreedPlaceCode))
		{
			Parent.AddRowMessageError(Res.GetString("PLExportCusEntryInstructionValidation|CheckSameIncotermPlaceCodeUsed"
				, "All Invoices on an Entry Instruction must have this same Incoterm Place code"));
		}
	}

	void CheckSameTransportChargesMoPUsed()
	{
		if (!Parent.Invoices.AllSame(x => x.ZG_TransportChargesMethodOfPayment))
		{
			Parent.AddRowMessageError(Res.GetString("PLExportCusEntryInstructionValidation|CheckSameTransportChargesMoPUsed"
				, "All Invoices on an Entry Instruction must have this same Transp.Charges MoP"));
		}
	}

	protected override void CheckCEI_SubStyle()
	{
		base.CheckCEI_SubStyle();

		CheckAllowedSubStyleCodes();
		CheckRuleR0677();
		CheckRuleR0678();
	}

	void CheckAllowedSubStyleCodes()
	{
		var parent = Parent;
		var entryInstructionsSubStyles = parent.JobDeclaration.CustomsEntryInstructions.Select(x => x.CEI_SubStyle.ToString()).ToArray();
		if (entryInstructionsSubStyles.Any(x => x is EntrySubStyleListCode.SimplifiedDeclaration or EntrySubStyleListCode.PreliminaryDeclarationUnderCodeC) &&
			parent.CEI_SubStyle.ToString() is not EntrySubStyleListCode.SimplifiedDeclaration and not EntrySubStyleListCode.PreliminaryDeclarationUnderCodeC)
		{
			parent.CEI_SubStyleInfo.AddError(Res.GetString("PLExportCusEntryInstructionValidation|CheckAllowedSubStyleCodes|SimplifiedDeclaration",
				"For simplified declaration with Sub Style (Additional Declaration Type) 'C' or 'F' can exist only Entries with the “Sub Style” codes: 'C' or 'F'."));
		}
		if (entryInstructionsSubStyles.Any(x => x is EntrySubStyleListCode.IncompleteDeclaration or EntrySubStyleListCode.PreliminaryDeclarationUnderCodeB) &&
			parent.CEI_SubStyle.ToString() is not EntrySubStyleListCode.IncompleteDeclaration and not EntrySubStyleListCode.PreliminaryDeclarationUnderCodeB)
		{
			parent.CEI_SubStyleInfo.AddError(Res.GetString("PLExportCusEntryInstructionValidation|CheckAllowedSubStyleCodes|IncompleteDeclaration",
				"For simplified declaration with Sub Style (Additional Declaration Type) 'B' or 'E' can exist only Entries with the “Sub Style” codes: 'B' or 'E'."));
		}
		if (entryInstructionsSubStyles.Any(x => x is EntrySubStyleListCode.SupplementaryDeclarationForCodeBOrCodeE) &&
			parent.CEI_SubStyle.ToString() is not EntrySubStyleListCode.SupplementaryDeclarationForCodeBOrCodeE)
		{
			parent.CEI_SubStyleInfo.AddError(Res.GetString("PLExportCusEntryInstructionValidation|CheckAllowedSubStyleCodes|SupplementaryDeclarationForCodeBOrCodeE",
				"For Supplementary Declaration with Sub Style (Additional Declaration Type) 'X' can be used only Entries with the “Sub Style” codes: 'X'."));
		}
		if (entryInstructionsSubStyles.Any(x => x is EntrySubStyleListCode.SupplementaryDeclarationForCodeCOrCodeF) &&
			parent.CEI_SubStyle.ToString() is not EntrySubStyleListCode.SupplementaryDeclarationForCodeCOrCodeF)
		{
			parent.CEI_SubStyleInfo.AddError(Res.GetString("PLExportCusEntryInstructionValidation|CheckAllowedSubStyleCodes|SupplementaryDeclarationForCodeCOrCodeF",
				"For Supplementary Declaration with Sub Style (Additional Declaration Type) 'Y' can be used only Entries with the “Sub Style” codes: 'Y'."));
		}
	}

	void CheckRuleR0677()
	{
		var parent = Parent;
		var subStyle = parent.CEI_SubStyle;
		if ((subStyle == Constants.SubStyleCodes.C
			|| subStyle == Constants.SubStyleCodes.F)
			&& !parent.HasAuthorisationUsageCode(Constants.CusAuthorizationUsageType.C512))
		{
			Parent.CEI_SubStyleInfo.AddMessageError(Res.GetString("PLExportCusEntryInstructionValidation|CheckRuleR0677"
				, "(R0677) An Authorization code C512 is required for a declaration Sub Style C or F"));
		}
	}

	void CheckRuleR0678()
	{
		var parent = Parent;
		var subStyle = parent.CEI_SubStyle;
		if ((subStyle != Constants.SubStyleCodes.C
			&& subStyle != Constants.SubStyleCodes.F)
			&& parent.HasAuthorisationUsageCode(Constants.CusAuthorizationUsageType.C512))
		{
			Parent.CEI_SubStyleInfo.AddMessageError(Res.GetString("PLExportCusEntryInstructionValidation|CheckRuleR0678"
				, "(R0678) Only declaration Sub Style C or F is allowed for an Authorization code C512"));
		}
	}

	protected override void CheckCEI_Procedure()
	{
		base.CheckCEI_Procedure();
		CheckRuleR0030E();
		CheckRuleR0033E();
	}

	void CheckRuleR0033E()
	{
		if (Parent.CEI_Procedure == Constants.ProcedureCodes._11
			&& !Parent.HasAuthorisationUsageCode(Constants.CusAuthorizationUsageType.C601))
		{
			Parent.CEI_ProcedureInfo.AddMessageError(Res.GetString("PLExportCusEntryInstructionValidation|CheckRuleR0033E",
				"(R0033E) an authorization code C601 is required for requested procedure code 11"));
		}
	}

	void CheckRuleR0030E()
	{
		if (Parent.CEI_Procedure == ProcedureCodes._21
			&& HasAdditionalInformationCode(AdditionalInfoTypes.INF, AdditionalInfoCodes._00100)
			&& !HasSupportingDocumentCodes(ValidationLists.R0030ESupportingDocuments(Parent.Factory)))
		{
			Parent.CEI_ProcedureInfo.AddMessageError(Res.GetString("PLExportCusEntryInstructionValidation|CheckRuleR0030E"
				, "(R0030E) A supporting document C710 or 4DK3 is required for each Entry Line"));
		}
	}

	void CheckRuleR0224()
	{
		var sumOfCustomsQuantity = Parent.InvoiceLines.Sum(x => x.CustomsWeight.InKilogramsSafe);
		var sumOfJI_Weight = Parent.InvoiceLines.Sum(x => x.EffectiveGrossWeight.InKilogramsSafe);
		if (sumOfJI_Weight < sumOfCustomsQuantity)
		{
			Parent.AddRowMessageError(Res.GetString("PLExportCusEntryInstructionValidation|CheckRuleR0224"
				, "(R0224) The sum of Gross Weight < sum of Net Weight (Customs Quantity)"));
		}
	}
}
