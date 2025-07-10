using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ExportJobComInvoiceLineValidation(JobComInvoiceLine parent) : JobComInvoiceLineValidation(parent)
{
	public override void ValidateAll()
	{
		base.ValidateAll();
		CheckRuleR0031E();
		CheckRuleR0219();
	}

	protected override void CheckJI_CountryOfOrigin()
	{
		base.CheckJI_CountryOfOrigin();
		if (Parent.JI_CountryOfOrigin.IsEmpty)
		{
			var procedure = Parent.ProcedureCodeBase;
			var specificCircumstanceIndicator = Parent.Declaration?.ZG_SpecificCircumstanceIndicator ?? ZString.Empty;
			if (procedure.StartsWith(ProcedureCodes._76) || procedure.StartsWith(ProcedureCodes._77) || specificCircumstanceIndicator == SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CountryOfOriginInfo);
			}
			CheckRuleC0871();
		}
	}

	void CheckRuleC0871()
	{
		if (Parent.AdditionalProcedureCodes.Cast<AdditionalProcedureCode>().Any(x => x.CY_Code.StartsWith("E")))
		{
			Parent.JI_CountryOfOriginInfo.AddMessageError(Res.GetString("PLExportJobComInvoiceLineValidation|CheckRuleRC0871"
				, "(C0871) A Country of Origin is required for a procedure details code starting with letter 'E'"));
		}
	}

	bool HasAuthorisationUsageCode(ZString authorisationUsageCode) => Parent.EntryInstruction?.HasAuthorisationUsageCode(authorisationUsageCode) ?? false;

	bool HasEntryInstructionProcedureCode(ZString instructionProcedureCode) => Parent.EntryInstruction?.HasEntryInstructionProcedureCode(instructionProcedureCode) ?? false;

	void CheckRuleR0031E()
	{
		if (((HasAuthorisationUsageCode(CusAuthorizationUsageType.C601) && HasEntryInstructionProcedureCode(ProcedureCodes._11))
			|| (HasAuthorisationUsageCode(CusAuthorizationUsageType.C019) && HasEntryInstructionProcedureCode(ProcedureCodes._21)))
			&& !HasSupportingDocumentCodes(ValidationLists.R0031ESupportingDocuments(Parent.Factory)))
		{
			Parent.AddRowMessageError(R0031EMessageError);
		}
	}

	public string R0031EMessageError => Res.GetString("PLExportJobComInvoiceLineValidation|R0031E", "(R0031E) Supporting Document C710 or 4DK3 is required");
	protected override void CheckProcedureCodeBase()
	{
		base.CheckProcedureCodeBase();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.ProcedureCodeBaseInfo);
		CheckRuleR0025E();
	}

	protected override void CheckPreviousProcedureCode()
	{
		base.CheckPreviousProcedureCode();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.PreviousProcedureCodeInfo);
	}

	void CheckRuleR0025E()
	{
		var procedure = Parent.ProcedureCodeBase + Parent.PreviousProcedureCode;
		if ((R0025EProcedureCheck1()
			|| R0025EProcedureCheck2())
			&& !HasPreviousDocumentCodes(ValidationLists.SpecialProcedureCodesForPreviousDocuments(Parent.Factory)))
		{
			Parent.ProcedureCodeBaseInfo.AddMessageError(Res.GetString("PLExportJobComInvoiceLineValidation|CheckRuleR0025E", "(R0025E) For the requested Procedure Code, a Previous document with one of the codes 'MRN','CLE','SDE','OGL','ZZZ' is required."));
		}

		bool R0025EProcedureCheck1()
		{
			return procedure == ProcedureCodes._21 + ProcedureCodes._41
					|| procedure == ProcedureCodes._21 + ProcedureCodes._48
					|| procedure == ProcedureCodes._21 + ProcedureCodes._46
					|| procedure == ProcedureCodes._21 + ProcedureCodes._51
					|| procedure == ProcedureCodes._21 + ProcedureCodes._54
					|| procedure == ProcedureCodes._21 + ProcedureCodes._91
					|| procedure == ProcedureCodes._31 + ProcedureCodes._78
					|| procedure == ProcedureCodes._31 + ProcedureCodes._51
					|| procedure == ProcedureCodes._31 + ProcedureCodes._54
					|| procedure == ProcedureCodes._31 + ProcedureCodes._53
					|| procedure == ProcedureCodes._31 + ProcedureCodes._71
					|| procedure == ProcedureCodes._31 + ProcedureCodes._91
					|| procedure == ProcedureCodes._10 + ProcedureCodes._41
					|| procedure == ProcedureCodes._10 + ProcedureCodes._44;
		}

		bool R0025EProcedureCheck2()
		{
			return (procedure == ProcedureCodes._10 + ProcedureCodes._40
					|| procedure == ProcedureCodes._21 + ProcedureCodes._40
					|| procedure == ProcedureCodes._21 + ProcedureCodes._44)
					&& (HasAuthorisationUsageCode(CusAuthorizationUsageType.N990)
						|| HasAuthorisationUsageCode(CusAuthorizationUsageType.D019));
		}
	}

	bool HasPreviousDocumentCodes(IReadOnlyCollection<ZString> previousDocumentCsiCodes) =>
		Parent.HasPreviousDocumentCodes(previousDocumentCsiCodes)
		|| (Parent.InvoiceHeader?.HasPreviousDocumentCodes(previousDocumentCsiCodes) ?? false)
		|| (Parent.Declaration?.HasPreviousDocumentCodes(previousDocumentCsiCodes) ?? false);

	public void CheckRuleR0219()
	{
		var parent = Parent;
		if (!UniversalValidationHelper.IsInAESTransitionPeriod
			&& parent.PackagesPivot.Count == 0)
		{
			parent.AddRowMessageError(Res.GetString("PLExportJobComInvoiceLineValidation|CheckRuleR0219"
				, "(R0219) You have not selected any packing information."));
		}
	}
}
