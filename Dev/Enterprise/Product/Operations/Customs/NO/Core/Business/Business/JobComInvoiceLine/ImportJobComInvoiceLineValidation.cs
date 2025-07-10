using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NO.Business;

public sealed class ImportJobComInvoiceLineValidation(JobComInvoiceLine parent) : JobComInvoiceLineValidation(parent)
{
	protected override void CheckJI_PrimaryPreference()
	{
		base.CheckJI_PrimaryPreference();

		const string EmptyFormula = "0";
		var parent = Parent;
		var targetInfo = parent.JI_PrimaryPreferenceInfo;

		var formula = parent.NormalTariffDutyRateFormula;
		if (!parent.PreferenceCodesExcludedFromCreatingDefaultSupportingDocument)
		{
			if (parent.JI_ReducedCustomsFlag == ReducedCustomsFlagList.Codes.S)
			{
				targetInfo.AddMessageError(Res.GetString("FAB36C4B-4330-489B-9183-A571FAEC09A2", "The selected reduced customs flag requires preference code to be N or J."));
			}
			else if (formula == EmptyFormula)
			{
				targetInfo.AddError(Res.GetString("3C31C7CC-DFD4-4F6D-9E6E-5EB48C891E24", "The preference code is not valid. On tariff numbers with general customs rate 0, the only valid preference codes are N or J"));
			}
		}

		if (formula != EmptyFormula)
		{
			ListValidation.MessageErrorIfInvalidCode(targetInfo, ResString.GetMultilingualString("6DE8F629-BD44-06BB-47C9-E15816C76FB7", "The preference code is not valid for this Country of origin"));
		}
	}

	protected override void CheckJI_ZZF_NKTaxType()
	{
		base.CheckJI_ZZF_NKTaxType();
		MandatoryValidation.CheckEntered(Parent.JI_ZZF_NKTaxTypeInfo);
		CheckJI_ZZF_NKTaxType_RequireSupportingDocumentOrMvaImporterIfTaxCodeMF();
	}

	void CheckJI_ZZF_NKTaxType_RequireSupportingDocumentOrMvaImporterIfTaxCodeMF()
	{
		var parent = Parent;
		if (parent != null &&
			parent.JI_ZZF_NKTaxType == UniversalReferenceConstants.RefCusTaxOrFee.MVF &&
			!parent.HasImporterWithMVARegistration &&
			!parent.SupportingDocuments.HasAnyWithCode("M2"))
		{
			parent.JI_ZZF_NKTaxTypeInfo.AddMessageError(Res.GetString("6428bf57-0bbd-4d82-b997-950b710dbb7a", "When VAT code MVF = MVA Free (full VAT exemption) is used a supporting document of type M2 must be entered (with the reference to correct paragraphs in Norwegian Duty Act). (Required only when Importer is NOT VAT/MVA registered)."));
		}
	}

	protected override void TaxTypeListValidationCore()
	{
		ListValidation.ErrorIfInvalidCode(Parent.JI_ZZF_NKTaxTypeInfo);
	}

	protected override ZString UniversalTariffWithAmbiguousRateCodeError(ZString rateTypeDescription, ZString rateCode, ZString tariff, IZZRateSelectionCriteria criteria)
	{
		return Res.GetString("B639AD21-18A2-3AB7-41A0-BDF713B2546B", "There is more than one applicable type of customs rate for the Tariff {0}. Please select customs rate override to select the type that applies.", tariff);
	}

	protected override ZBool ShouldValidateUniversalTariffWithAmbiguousRateCode => !Parent.CustomsRateIsOverridden;
}
