using System.Linq;
using CargoWise.Types;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ImportCusEntryInstructionValidation : CusEntryInstructionValidation
{
	public ImportCusEntryInstructionValidation(CusEntryInstruction parent)
		: base(parent)
	{
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		CheckAllEntryInstructionValuationDatesAreTheSame();
		CheckRuleR625();
		CheckRuleR628();
		CheckRuleR631();
		CheckRuleR1628();
	}

	#region CEI_SubStyle

	protected override void CheckCEI_SubStyle()
	{
		base.CheckCEI_SubStyle();
		CheckRuleR986();
	}

	void CheckRuleR986()
	{
		if (IsR986SubStyle(Parent.CEI_SubStyle) &&
			!HasR986InvoiceHeaderDocument &&
			!HasR986InvoiceLineDocument)
		{
			Parent.CEI_SubStyleInfo.AddMessageError(Res.GetString("PLImportCusEntryInstructionValidation|CheckRuleR986", "(R986) One of C512, C513 Supporting document is required."));
		}
	}

	static bool IsR986SubStyle(ZString subStyle) => subStyle == SubStyleCodes.C || subStyle == SubStyleCodes.F || subStyle == SubStyleCodes.Y;

	bool HasR986InvoiceHeaderDocument => Parent.Invoices.Any(hd => hd.SupportingDocuments.Cast<SupportingDocument>().Any(IsR986DocumentCode));

	bool HasR986InvoiceLineDocument => Parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(inv => inv.SupportingDocuments.Cast<SupportingDocument>().Any(IsR986DocumentCode));

	static bool IsR986DocumentCode(SupportingDocument supportingDocument)
	{
		var code = supportingDocument?.CSI_Code ?? ZString.Empty;
		return code == SupportingDocumentCodes.C512 || code == SupportingDocumentCodes.C513;
	}

	#endregion

	#region CEI_Procedure

	protected override void CheckCEI_Procedure()
	{
		base.CheckCEI_Procedure();

		CheckForRuleR267();
	}

	void CheckForRuleR267()
	{
		if (IsR267Procedure(Parent.CEI_Procedure) && (Parent.JobDeclaration?.JE_GoodsDestination.Left(2) ?? ZString.Empty) == CountryCodes.Poland)
		{
			Parent.CEI_ProcedureInfo.AddMessageError(Res.GetString("PLCusEntryInstructionValidation|CheckForRuleR267", "(R267) – For the requested procedure code 42 or 63 the destination country code cannot be PL."));
		}
	}

	static bool IsR267Procedure(ZString procedure) => procedure == ProcedureCodes._42 || procedure == ProcedureCodes._63;

	#endregion

	void CheckAllEntryInstructionValuationDatesAreTheSame()
	{
		var firstValuationDate = Parent.InvoiceLines.FirstOrDefault()?.JI_ValuationDateOverride ?? ZDateTime.Empty;
		if (firstValuationDate.IsValid)
		{
			var yearMonthOfFirstValuationDate = GetYearMonth(firstValuationDate);
			if (Parent.InvoiceLines.Skip(0).Any(line =>
				{
					var currentValuationDateOverride = line.JI_ValuationDateOverride;
					return currentValuationDateOverride.IsValid && GetYearMonth(currentValuationDateOverride) != yearMonthOfFirstValuationDate;
				}))
			{
				Parent.AddRowMessageError(ResString.GetMultilingualString("PLJobComInvoiceLineValidation|DifferentValuationDate",
					"All valuation dates for the Entry must be from the same month and year."));
			}
		}
	}

	static (int Year, int Month) GetYearMonth(ZDateTime datetime) => (datetime.Year, datetime.Month);

	void CheckRuleR625()
	{
		if (Parent.CEI_Procedure == Constants.ProcedureCodes._51
			&& DoesNotHaveAdditionalInformationCode(ZString.Empty, Constants.AdditionalInfoCodes._00100)
			&& DoesNotHaveSupportingDocumentCode(Constants.SupportingDocumentCodes.C601))
		{
			Parent.AddRowMessageError(ResString.GetMultilingualString("PLJobComInvoiceLineValidation|CheckRuleR625",
				"(R625) Additional Information code 00100 or Supporting Document/Authorization code C601 is required for requested procedure."));
		}
	}

	void CheckRuleR628()
	{
		if (Parent.CEI_Procedure == Constants.ProcedureCodes._53
			&& DoesNotHaveAdditionalInformationCode(ZString.Empty, Constants.AdditionalInfoCodes._00100)
			&& DoesNotHaveSupportingDocumentCode(Constants.SupportingDocumentCodes.C516))
		{
			Parent.AddRowMessageError(ResString.GetMultilingualString("PLJobComInvoiceLineValidation|CheckRuleR628",
				"(R628) Additional Information code 00100 or Supporting Document/Authorization code C516 is required for requested procedure."));
		}
	}

	void CheckRuleR631()
	{
		if (Parent.CEI_Procedure == Constants.ProcedureCodes._48
			&& DoesNotHaveAdditionalInformationCode(ZString.Empty, Constants.AdditionalInfoCodes._00100)
			&& DoesNotHaveSupportingDocumentCode(Constants.SupportingDocumentCodes.C019))
		{
			Parent.AddRowMessageError(ResString.GetMultilingualString("PLJobComInvoiceLineValidation|CheckRuleR631",
				"(R631) Additional Information code 00100 or Supporting Document/Authorization code C019 is required for requested procedure."));
		}
	}

	void CheckRuleR1628()
	{
		if (Parent.CusAuthorizationUsages.Count == 0
			&& ExistsOneOfR1628SupportingDocuments())
		{
			Parent.AddRowMessageError(ResString.GetMultilingualString("PLJobComInvoiceLineValidation|CheckRuleR1628",
				"[R1628] Authorization owner information is required."));
		}

		bool ExistsOneOfR1628SupportingDocuments() => Parent.AllInstructionSupportingDocuments.Any(x => IsR1628SupportingDocument(x.CSI_Code));

		bool IsR1628SupportingDocument(ZString csiCode) => csiCode == Constants.SupportingDocumentCodes.C019
															|| csiCode == Constants.SupportingDocumentCodes.C504
															|| csiCode == Constants.SupportingDocumentCodes.C512
															|| csiCode == Constants.SupportingDocumentCodes.C513
															|| csiCode == Constants.SupportingDocumentCodes.C514
															|| csiCode == Constants.SupportingDocumentCodes.C515
															|| csiCode == Constants.SupportingDocumentCodes.C516
															|| csiCode == Constants.SupportingDocumentCodes.C601
															|| csiCode == Constants.SupportingDocumentCodes.C626
															|| csiCode == Constants.SupportingDocumentCodes.C627
															|| csiCode == Constants.SupportingDocumentCodes.N990;
	}

	protected override void CheckCEI_DateForDuty()
	{
		base.CheckCEI_DateForDuty();

		var dateForDuty = Parent.CEI_DateForDuty;
		if (!DutyCalculationHelper.HasCUDExchangeRateOrSpecifiedDateIsInvalid(Parent.Factory, dateForDuty))
		{
			Parent.CEI_DateForDutyInfo.AddMessageError(Res.GetString("PLImportCusEntryInstructionValidation|CheckCUDExchangeRatesArePresent"
				, "The EUR (CUD type) exchange rate for {0} is missing. No duty / taxes calculation can be done.", dateForDuty.ToShortDateString()));
		}
	}
}
