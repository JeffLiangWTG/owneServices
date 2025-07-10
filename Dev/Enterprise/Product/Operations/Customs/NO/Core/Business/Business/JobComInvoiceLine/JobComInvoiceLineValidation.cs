using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business;

public abstract class JobComInvoiceLineValidation : AutoNOJobComInvoiceLineValidation
{
	protected JobComInvoiceLineValidation(JobComInvoiceLine parent)
	: base(parent) { }

	protected override INotificationType ValidRatesNotificationSeverity => CargoWise.EntityFramework.NotificationType.Warning;

	protected override IEnumerable<IZZRateSelectionCriteria> RateSelectionCriteriaLists => new List<IZZRateSelectionCriteria>() { Parent.DutyRateSelectionCriteria, Parent.NormalTariffDutyRateSelectionCriteria, Parent.ExciseRateSelectionCriteria };

	public override void ValidateAll()
	{
		Parent.ClearRowNotifications();

		base.ValidateAll();
		ValidateCustomsRateType();
		ValidateCustomsRateOverrideType();
		ValidateSupportingDocumentType();
	}

	public void ValidateCustomsRateType()
	{
		ValidateCalculatedProperty(Parent.CustomsRateTypeInfo);
	}

	public void ValidateCustomsRateOverrideType()
	{
		ValidateCalculatedProperty(Parent.JI_CustomsRateOverrideTypeInfo);
	}

	protected new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

	protected override bool IsJIProcedureMandatory => false;

	protected void CheckCustomsRateType()
	{
		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValue(Parent.CustomsRateTypeInfo, Parent.CustomsRateIsOverriddenInfo, ZBool.True);
		ListValidation.MessageErrorIfInvalidCode(Parent.CustomsRateTypeInfo);
	}

	protected override void CheckJI_Description()
	{
		base.CheckJI_Description();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_DescriptionInfo);
	}

	protected override void CheckJI_Weight()
	{
		base.CheckJI_Weight();
		CompareValidation.MessageErrorIfLessThanOrEqualToZero(Parent.JI_WeightInfo);
	}

	protected override void CheckJI_WeightUQ()
	{
		base.CheckJI_WeightUQ();
		ListValidation.MessageErrorIfInvalidCode(Parent.JI_WeightUQInfo);
	}

	protected override void CheckJI_NetWeight()
	{
		base.CheckJI_NetWeight();
		CompareValidation.MessageErrorIfLessThanOrEqualToZero(Parent.JI_NetWeightInfo);
	}

	protected override void CheckJI_NetWeightUQ()
	{
		base.CheckJI_NetWeightUQ();
		ListValidation.MessageErrorIfInvalidCode(Parent.JI_NetWeightUQInfo);
	}

	protected override void CheckJI_PrimaryPreference()
	{
		base.CheckJI_PrimaryPreference();
		CheckJI_PrimaryPreferenceIfMandatorySupportingDocumentsExists(Parent);
	}

	protected void CheckJI_PrimaryPreferenceIfMandatorySupportingDocumentsExists(JobComInvoiceLine parent)
	{
		var supportingDocuments = parent.SupportingDocuments.Cast<SupportingDocument>().ToArray();
		if (!parent.PreferenceCodesExcludedFromCreatingDefaultSupportingDocument && !supportingDocuments.Any(x => x.CSI_Code == SupportingDocumentCodeList.CertificateForOrigin))
		{
			parent.JI_PrimaryPreferenceInfo.AddMessageError(Res.GetString("FCA1BB2B-3174-44DC-BEB1-1864DD850771", "Preference code '{0}' requires a Supporting Document of type 'SER'.", parent.JI_PrimaryPreference));
		}
		else if (!parent.PreferenceCodesExcludedFromCreatingDefaultSupportingDocument && supportingDocuments.Any(x => x.CSI_Code == SupportingDocumentCodeList.CertificateForOrigin && x.CSI_ReferenceNumber == SupportingDocumentCodeList.CertificateForOrigin_DefaultText))
		{
			parent.JI_PrimaryPreferenceInfo.AddWarning(Res.GetString("2F2EC528-AD88-46F5-9B92-356E1AFE078A", "Preference code is set to '{0}', and SER/FAKTURAERKLÆRING  is added to box [44] Supporting documents.", parent.JI_PrimaryPreference));
		}
	}

	protected override void CheckJI_Procedure()
	{
		base.CheckJI_Procedure();
		ListValidation.MessageErrorIfInvalidCode(Parent.JI_ProcedureInfo);

		if (Parent is { EntryInstruction.CEI_Procedure: var ceiProcedure, JI_Procedure: { IsEmpty: false } jiProcedure }
			&& ceiProcedure == UniversalReferenceConstants.ChargeTypes.VGE_ProcedureCode
			&& jiProcedure != UniversalReferenceConstants.ChargeTypes.VGE_ProcedureCode)
		{
			var errorMessage = Res.GetString("274EFB45-285B-5186-41C6-0682AAD409B0", "Procedure 6021 cannot be mixed with other procedures on same entry.");
			Parent.JI_ProcedureInfo.AddMessageError(errorMessage);
		}
	}

	protected override void CheckJI_ValuationCode()
	{
		base.CheckJI_ValuationCode();
		ListValidation.MessageErrorIfInvalidCode(Parent.JI_ValuationCodeInfo);
	}

	protected override void CheckJI_CountryOfOrigin()
	{
		base.CheckJI_CountryOfOrigin();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_CountryOfOriginInfo);
	}

	protected override void CheckJI_CustomsSecondUnitQty()
	{
		base.CheckJI_CustomsSecondUnitQty();
		ListValidation.MessageErrorIfInvalidCode(Parent.JI_CustomsSecondUnitQtyInfo);
		AddFieldRequiredMessageErrorIfAnyQuantityIsNotSpecified(
			Parent.JI_CustomsSecondUnitQtyInfo,
			[Parent.JI_CustomsThirdUnitQtyInfo, Parent.JI_CustomsFourthUnitQtyInfo, Parent.JI_CustomsFifthUnitQtyInfo],
			Parent.Lookups.CustomsSecondUnitQtyList);
	}

	protected override void CheckJI_CustomsSecondQuantity()
	{
		base.CheckJI_CustomsSecondQuantity();
		var targetInfo = Parent.JI_CustomsSecondQuantityInfo;

		if (Parent.JI_CustomsSecondUnitQty.IsEmpty)
		{
			if (Parent.JI_CustomsSecondQuantity > 0m)
			{
				targetInfo.AddMessageError(Res.GetString("6A9A2E3D-CD8D-87AF-48B3-3085F559B2DA", "Other unit cannot be given when type is blank."));
			}
		}
		else
		{
			if (Parent.JI_CustomsSecondQuantity <= 0m)
			{
				targetInfo.AddMessageError(Res.GetString("B1CEDF7F-544F-6989-4EF4-CF894A9E5596", "This tariff number requires other unit (see type) to be greater than zero."));
			}
		}
	}

	protected override void CheckJI_CustomsRateOverrideType()
	{
		base.CheckJI_CustomsRateOverrideType();
		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValue(Parent.JI_CustomsRateOverrideTypeInfo, Parent.CustomsRateIsOverriddenInfo, ZBool.True);
		ListValidation.MessageErrorIfInvalidCode(Parent.JI_CustomsRateOverrideTypeInfo);
	}

	protected override void CheckJI_PackageType()
	{
		base.CheckJI_PackageType();
		if (Parent.NO_PackageTypeVisible)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_PackageTypeInfo);
		}
	}

	protected override void CheckJI_CustomsThirdUnitQty()
	{
		base.CheckJI_CustomsThirdUnitQty();
		ListValidation.MessageErrorIfInvalidCode(Parent.JI_CustomsThirdUnitQtyInfo);
		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.JI_CustomsThirdUnitQtyInfo, Parent.JI_CustomsThirdQuantityInfo);
		AddFieldRequiredMessageErrorIfAnyQuantityIsNotSpecified(
			Parent.JI_CustomsThirdUnitQtyInfo,
			[Parent.JI_CustomsSecondUnitQtyInfo, Parent.JI_CustomsFourthUnitQtyInfo, Parent.JI_CustomsFifthUnitQtyInfo],
			Parent.Lookups.CustomsUQList);
	}

	protected override void CheckJI_CustomsFourthUnitQty()
	{
		base.CheckJI_CustomsFourthUnitQty();
		ListValidation.MessageErrorIfInvalidCode(Parent.JI_CustomsFourthUnitQtyInfo);
		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.JI_CustomsFourthUnitQtyInfo, Parent.JI_CustomsFourthQuantityInfo);
		AddFieldRequiredMessageErrorIfAnyQuantityIsNotSpecified(
			Parent.JI_CustomsFourthUnitQtyInfo,
			[Parent.JI_CustomsSecondUnitQtyInfo, Parent.JI_CustomsThirdUnitQtyInfo, Parent.JI_CustomsFifthUnitQtyInfo],
			Parent.Lookups.CustomsUQList);
	}

	protected override void CheckJI_CustomsFifthUnitQty()
	{
		base.CheckJI_CustomsFifthUnitQty();
		ListValidation.MessageErrorIfInvalidCode(Parent.JI_CustomsFifthUnitQtyInfo);
		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.JI_CustomsFifthUnitQtyInfo, Parent.JI_CustomsFifthQuantityInfo);
		AddFieldRequiredMessageErrorIfAnyQuantityIsNotSpecified(
			Parent.JI_CustomsFifthUnitQtyInfo,
			[Parent.JI_CustomsSecondUnitQtyInfo, Parent.JI_CustomsThirdUnitQtyInfo, Parent.JI_CustomsFourthUnitQtyInfo],
			Parent.Lookups.CustomsUQList);
	}

	static void AddFieldRequiredMessageErrorIfAnyQuantityIsNotSpecified(ZPropertyInfo customsQuantityInfo, ZPropertyInfo[] otherQuantities, CodeDescriptionPairList codeDescriptionPairList)
	{
		var requiredUOMs = codeDescriptionPairList.GetAllCodes();
		var specifiedUOMs = otherQuantities.Select(q => q.Value)
			.Prepend(customsQuantityInfo.Value)
			.Where(v => !v.IsEmpty)
			.Select(v => v.ToString());

		if (requiredUOMs.Except(specifiedUOMs).Any())
		{
			MandatoryValidation.MessageErrorIfNotEntered(customsQuantityInfo);
		}
	}

	protected override void CheckJI_ReducedCustomsFlag()
	{
		base.CheckJI_ReducedCustomsFlag();
		ListValidation.ErrorIfInvalidCode(Parent.JI_ReducedCustomsFlagInfo);
	}

	void ValidateSupportingDocumentType()
	{
		var invoiceLine = Parent;
		if (invoiceLine.EntryInstruction?.CEI_SubStyle.ToString() == ImportDeclarationSubTypes.Codes.P &&
			!invoiceLine.SupportingDocuments.Any(x => x.CSI_Code == SupportingDocumentCodeFOR || x.CSI_Code == SupportingDocumentCodeTXT))
		{
			invoiceLine.AddRowMessageError(Res.GetString("5F6C895A-2011-4B3A-8415-656BBEC0962A", "ALL invoice lines must have a supporting document of type FOR or TXT when Entry Instruction Decl. Sub Type is Preliminary"));
		}
	}

	const string SupportingDocumentCodeFOR = "FOR";
	const string SupportingDocumentCodeTXT = "TXT";
}
