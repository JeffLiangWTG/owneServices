using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using TariffAttributeValues = Enterprise.Customs.US.Business.UniversalReferenceConstants.TariffAttributeTypes.Values;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class ACEImportAddInfoJobComInvoiceLineValidation : FormalImportAddInfoJobComInvoiceLineValidation
	{
		public ACEImportAddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine addInfo)
			: base(addInfo)
		{
		}

		readonly Func<TariffView, string, bool> tariffAttrTypeChecker = (tariff, type) => tariff != null && tariff.HasAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, type);

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateCustomsValueAgainstFWSUSDValues();
			}
		}

		protected override void CheckUS_SupTariff()
		{
			base.CheckUS_SupTariff();

			var invoiceLine = Parent;
			var supTariff = invoiceLine.US_SupTariff;
			var supTariffInfo = invoiceLine.US_SupTariffInfo;

			if (supTariff == CusEntryLine.Tariff99034529)
			{
				supTariffInfo.AddWarning(Res.GetString("7211D411-8FE0-4CCD-A61B-C4F471AF629E", "Per CSMS # 61152419 , when tariff number 9903.45.29 is used,  the appropriate importer certification documentation must be sent to ABI using DIS."));
			}
			else if(supTariff == CusEntryLine.Tariff99039109)
			{
				supTariffInfo.AddWarning(Res.GetString("40E9A6C7-E91C-466C-BE5E-22AEA4FB3FA3", "Per CSMS # 62284627 , when tariff number 9903.91.09 is used, the appropriate importer certification documentation must be sent to ABI using DIS."));
			}

			if (invoiceLine.IsEntrySummaryValidationMode)
			{
				new LicenceValidator().ValidateRequirementForACE(supTariffInfo, invoiceLine, invoiceLine.ImportSupTariff);
			}

			ValidateProvTariffOrProductExclusionForSteelProducts(supTariffInfo, (countryOfOrigin, propertyInfo) =>
			{
				if (countryOfOrigin == Core.Constants.CountryCodes.Argentina || countryOfOrigin == Core.Constants.CountryCodes.Brazil || countryOfOrigin == Core.Constants.CountryCodes.KoreaSouth)
				{
					if (supTariff.IsEmpty)
					{
						propertyInfo.AddMessageError(ChildLineTariffNotEntered);
					}
				}
			});

			if (CalculateDutyForSetsHelper.Is9903Tariff(supTariff) && invoiceLine.IsCombinedLine() && !invoiceLine.IsSetXLine && !invoiceLine.IsSetVLine)
			{
				var combinedLines = invoiceLine.GetCombinedLines();
				var supTariffs = combinedLines.Where(x => CalculateDutyForSetsHelper.Is9903Tariff(x.US_SupTariff) && x.SupTariff != null).Select(x => x.SupTariff).ToList();

				if (supTariffs.Count > 1)
				{
					var supTariffCodes = supTariffs.Select(x => x.ZZ1_TariffCode);

					if (supTariffCodes.Count(x => x == supTariff) > 1)
					{
						supTariffInfo.AddWarning(Res.GetString("528C6E1D-813A-4D5C-975D-187EDD612DF6", "Prov/Prog. Tariff should not repeat on the same line."));
					}

					var last301TariffIndex = supTariffs.LastIndexOf(supTariffs.LastOrDefault(x => x.HasAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, TariffAttributeValues._301)));
					var first201Or232TariffIndex = supTariffs.IndexOf(supTariffs.FirstOrDefault(x => tariffAttrTypeChecker(x, TariffAttributeValues._232) || tariffAttrTypeChecker(x, TariffAttributeValues._201)));

					if (first201Or232TariffIndex != -1 && last301TariffIndex > first201Or232TariffIndex)
					{
						supTariffInfo.AddWarning(Res.GetString("253F66C3-611D-49D1-9764-DF5C5DF9DCFC", "For multiple trade remedy tariff numbers, Section 301 tariff numbers should be listed first, then Section 232 or Section 201 tariff numbers."));
					}
				}
			}

			var tariffRuleMessageError = CustomsRuleHelper.ValidateWithTariffRule(supTariff, invoiceLine.Declaration);
			if (!tariffRuleMessageError.IsEmpty)
			{
				supTariffInfo.AddMessageError(tariffRuleMessageError);
			}

			TariffValidator.ValidateSupplementaryTariffIfEnteredOnChildLineForDerivedSets(invoiceLine, supTariffInfo);
		}

		protected override void CheckUS_ADD_NA()
		{
			base.CheckUS_ADD_NA();

			if (Parent.US_ADD_NA)
			{
				var ruleName = GetADCRuleDependsOnValue(CustomsRuleRuleADCEligibleCodeList.Codes.NotApplicableSelected);
				if (!ruleName.IsEmpty)
				{
					Parent.US_ADD_NAInfo.AddMessageError(string.Format(CaseNAFlaggedError, ruleName));
				}
			}
		}

		protected override void CheckUS_CVD_NA()
		{
			base.CheckUS_CVD_NA();

			if (Parent.US_CVD_NA)
			{
				var ruleName = GetADCRuleDependsOnValue(CustomsRuleRuleADCEligibleCodeList.Codes.NotApplicableSelected);
				if (!ruleName.IsEmpty)
				{
					Parent.US_CVD_NAInfo.AddMessageError(string.Format(CaseNAFlaggedError, ruleName));
				}
			}
		}

		internal const string CaseNAFlaggedError = "N/A flagged as error per Customs Rule: {0}.";

		protected override void CheckUS_ADDCaseNo()
		{
			base.CheckUS_ADDCaseNo();

			if (!Parent.US_ADDCaseNo.IsEmpty)
			{
				if (!Parent.Declaration.US_BondWaiverCode.IsEmpty)
				{
					Parent.US_ADDCaseNoInfo.AddMessageError(BondCannotBeWaivedWhenAD_CVDInvolved);
				}

				var ruleName = GetADCRuleDependsOnValue(CustomsRuleRuleADCEligibleCodeList.Codes.CaseNumberEntered);
				if (!ruleName.IsEmpty)
				{
					Parent.US_ADDCaseNoInfo.AddMessageError(string.Format(CaseNumberFlaggedError, ruleName));
				}
			}

			ValidateUS_ADCVDStat();
		}

		protected override void CheckUS_CVDCaseNo()
		{
			base.CheckUS_CVDCaseNo();

			if (!Parent.US_CVDCaseNo.IsEmpty)
			{
				if (!Parent.Declaration.US_BondWaiverCode.IsEmpty)
				{
					Parent.US_CVDCaseNoInfo.AddMessageError(BondCannotBeWaivedWhenAD_CVDInvolved);
				}

				var ruleName = GetADCRuleDependsOnValue(CustomsRuleRuleADCEligibleCodeList.Codes.CaseNumberEntered);
				if (!ruleName.IsEmpty)
				{
					Parent.US_CVDCaseNoInfo.AddMessageError(string.Format(CaseNumberFlaggedError, ruleName));
				}
			}

			ValidateUS_ADCVDStat();
		}

		internal const string BondCannotBeWaivedWhenAD_CVDInvolved = "Bond has been indicated as waived. However it cannot be waived when AD/CVD data is entered.";
		internal const string CaseNumberFlaggedError = "AD/CVD Case Information flagged as error per Customs Rule: {0}.";

		ZString GetADCRuleDependsOnValue(ZString value)
		{
			var ruleName = ZString.Empty;
			var customsRule = Parent.Declaration?.CustomsRule;
			if (customsRule != null)
			{
				var adcRules = customsRule.Rules.Where(x => x.CPR_RuleCode == CustomsRuleRuleCodeList.Codes.ADCEligible);
				if (adcRules != null && adcRules.Any(y => y.CPR_ValueFrom == value))
				{
					ruleName = customsRule.HumanReadableName;
				}
			}
			return ruleName;
		}

		protected override void CheckUS_ADCVDStat()
		{
			base.CheckUS_ADCVDStat();

			if (Parent.US_ADCVDStat.IsEmpty)
			{
				if (!Parent.US_ADDCaseNo.IsEmpty)
				{
					Parent.US_ADCVDStatInfo.AddMessageError(EmptyADDStatement);
				}
			}
			else if (Parent.US_ADDCaseNo.IsEmpty)
			{
				Parent.US_ADCVDStatInfo.AddMessageError(ADDStatementMadeOnALineWithoutADDCase);
			}

			if (Parent.IsEntrySummaryValidationMode && !Parent.US_ADCVDStat.IsEmpty && !HasValidEntryTypeForADD_CVD)
			{
				Parent.US_ADCVDStatInfo.AddMessageError(ADDStatementMadeOnALineWithEntryTypes);
			}

			ValidateUS_ADDDecID();
		}

		internal const string ADDStatementMadeOnALineWithEntryTypes = "An ADD Non-Reimbursement Statement is required only for entry types 03, 06, 07, 21, 22, 23, 34 and 38.";
		internal const string EmptyADDStatement = "You have not made an ADD Non-Reimbursement Statement.";
		internal const string ADDStatementMadeOnALineWithoutADDCase = "An ADD Non-Reimbursement statement is required only for lines with ADD cases.";

		protected override void CheckUS_ADDDepositRateIndicator()
		{
			base.CheckUS_ADDDepositRateIndicator();

			ValidateUS_IsBondedADD();
		}

		protected override void CheckUS_CVDDepositRateIndicator()
		{
			base.CheckUS_CVDDepositRateIndicator();

			ValidateUS_IsBondedCVD();
		}

		protected override void CheckUS_IsBondedADD()
		{
			base.CheckUS_IsBondedADD();

			if (Parent.US_IsBondedADD && IsContinuousBondDisallowed(Parent.AntidumpingDutyCase, Parent.US_ADDDepositRateIndicator))
			{
				if (HasContinuousBondOnly())
				{
					Parent.US_IsBondedADDInfo.AddMessageError(OneContinuousBondIsNotAllowedWhenSpecificOrMoreThan5PercentAdValoremRate);
				}
			}
		}

		protected override void CheckUS_IsBondedCVD()
		{
			base.CheckUS_IsBondedCVD();

			if (Parent.US_IsBondedCVD && IsContinuousBondDisallowed(Parent.CountervailingDutyCase, Parent.US_CVDDepositRateIndicator))
			{
				if (HasContinuousBondOnly())
				{
					Parent.US_IsBondedCVDInfo.AddMessageError(OneContinuousBondIsNotAllowedWhenSpecificOrMoreThan5PercentAdValoremRate);
				}
			}
		}

		protected override void CheckUS_ADDQty()
		{
			base.CheckUS_ADDQty();

			if (Parent.US_ADDQty == 0m && !Parent.US_ADDUQ.IsEmpty && Parent.US_ADDDepositRateIndicator == DepositRateIndicatorList.Codes.Specific)
			{
				Parent.US_ADDQtyInfo.AddMessageError(ADCVDQuantityShouldBeEntered);
			}
		}

		protected override void CheckUS_CVDQty()
		{
			base.CheckUS_CVDQty();
			if (Parent.US_CVDQty == 0m && !Parent.US_CVDUQ.IsEmpty && Parent.US_CVDDepositRateIndicator == DepositRateIndicatorList.Codes.Specific)
			{
				Parent.US_CVDQtyInfo.AddMessageError(ADCVDQuantityShouldBeEntered);
			}
		}

		internal const string ADCVDQuantityShouldBeEntered = "Quantity should be entered for a specific rate.";

		protected override void CheckUS_ADDDecID()
		{
			base.CheckUS_ADDDecID();

			if (Parent.US_ADCVDStat == ADDCVDNonReimbursementList.Codes.Declared &&
				!Parent.US_ADDCaseNo.IsEmpty &&
				Parent.US_ADDDecID.IsEmpty)
			{
				Parent.US_ADDDecIDInfo.AddMessageError(StatementMadeWithoutDeclarationID);
			}
		}
		internal const string StatementMadeWithoutDeclarationID = "ADD Non-Reimbursement Statement has been made without the ID.  ID is required.";

		bool IsContinuousBondDisallowed(USCACCase acCase, ZString indicator)
		{
			bool result = false;

			if (acCase != null)
			{
				if (indicator == DepositRateIndicatorList.Codes.AdValorem)
				{
					var caseRate = acCase.CaseRates.GetDepositRate(Parent.DateForADD_CVD);
					result = caseRate != null && caseRate.U6_AdValoremRate >= 0.05m;
				}
				else if (indicator == DepositRateIndicatorList.Codes.Specific)
				{
					result = true;
				}
			}

			return result;
		}

		bool HasContinuousBondOnly()
		{
			return (Parent.Declaration.US_BondType.IsEmpty || Parent.Declaration.US_BondType == BondTypeList.Codes.ContinuousBond);
		}

		internal const string OneContinuousBondIsNotAllowedWhenSpecificOrMoreThan5PercentAdValoremRate = "You have entered only Continuous bond. But this ADD/CVD case requires at least one STB.";

		protected override void CheckUS_TTBInd()
		{
			base.CheckUS_TTBInd();
			if (IsPGAValidationApplicable())
			{
				if (InvoiceLine.IsSetXLine)
				{
					ValidateDataNotNeededForXLine("TTB", InvoiceLine.US_TTBIndInfo);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_TTBIndInfo, Parent.AddInfoLookups.US_OGAIndicatorList);
					AgencyRequirementsValidator.ValidatePGA(Parent.US_TTBIndInfo, "TTB", true, InvoiceLine.PGARequirementIndicator.HasTTBRequirement);
					AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.US_TTBIndInfo, "TTB", Parent.TTBLines.Cast<IPGADataCorrection>());
					AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.US_TTBIndInfo, InvoiceLine.PGARequirementIndicator.RequireTTB);
					AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.TTB, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_TTBIndInfo);
				}
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_TTBIndInfo);
			}
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.TTB, InvoiceLine.OGAAgencyRequirements);
			ValidateUS_TTBDisclaimReason();
		}

		protected override void CheckUS_TTBDisclaimReason()
		{
			base.CheckUS_TTBDisclaimReason();

			AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.US_TTBDisclaimReasonInfo, Parent.US_TTBInd, Parent.AddInfoLookups.TTBDisclaimReasonList);
			AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.TTB, InvoiceLine.OGAAgencyRequirements);

			ValidateUS_TTBInd();
		}

		INotificationType DDTCNotificationType
		{
			get { return CargoWise.EntityFramework.NotificationType.MessageError; }
		}

		protected override void CheckUS_DDTCInd()
		{
			base.CheckUS_DDTCInd();
			ListValidation.IfInvalidCode(DDTCNotificationType, Parent.US_DDTCIndInfo, Parent.AddInfoLookups.US_OGAIndicatorWithoutDisclaimerList, ListValidation.GetNotificationMessage(Parent.US_DDTCIndInfo).ToString());

			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine("DDTC", InvoiceLine.US_DDTCIndInfo);
			}
			else if (IsPGAValidationApplicable())
			{
				ValidateUS_DDTCArrivalDate();
				ValidateUS_DDTCExemptionCode();
				ValidateUS_DDTCLicenseNo();
				ValidateUS_DDTCLicenseType();
				ValidateUS_DDTCRegistrationNo();
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_DDTCIndInfo);
			}

			AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.DDTC, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_DDTCIndInfo);
		}

		protected override void CheckUS_DDTCArrivalDate()
		{
			base.CheckUS_DDTCArrivalDate();
			if (OGAIndicatorList.IsToBeDeclared(InvoiceLine.US_DDTCInd) && Parent.US_DDTCArrivalDate.IsEmpty && IsPGAValidationApplicable())
			{
				Parent.US_DDTCArrivalDateInfo.AddNotification(DDTCNotificationType, MandatoryValidation.YouHaveNotEnteredMessage("DDTC Anticipated Arrival Date"));
			}
		}

		protected override void CheckUS_DDTCExemptionCode()
		{
			base.CheckUS_DDTCExemptionCode();
			if (OGAIndicatorList.IsToBeDeclared(InvoiceLine.US_DDTCInd) && IsPGAValidationApplicable())
			{
				ListValidation.IfInvalidCode(DDTCNotificationType, Parent.US_DDTCExemptionCodeInfo, Parent.AddInfoLookups.DDTCExemptionCodes, ListValidation.GetNotificationMessage(Parent.US_DDTCExemptionCodeInfo).ToString());
				ValidateUS_DDTCLicenseNo();
				ValidateUS_DDTCLicenseType();
				ValidateUS_DDTCRegistrationNo();
			}
		}

		protected override void CheckUS_DDTCLicenseNo()
		{
			base.CheckUS_DDTCLicenseNo();
			if (OGAIndicatorList.IsToBeDeclared(InvoiceLine.US_DDTCInd) && IsPGAValidationApplicable())
			{
				if (Parent.US_DDTCLicenseNo.IsEmpty)
				{
					if (!Parent.US_DDTCLicenseType.IsEmpty)
					{
						Parent.US_DDTCLicenseNoInfo.AddNotification(DDTCNotificationType, ValidationConstants.DDTC.DDTCLicenseNoIsRequired);
					}
					else if (Parent.US_DDTCExemptionCode.IsEmpty)
					{
						Parent.US_DDTCLicenseNoInfo.AddNotification(DDTCNotificationType, ValidationConstants.DDTC.EitherDDTCLicenseNoOrDDTCExemptionCodeIsRequired);
					}
				}
				else if (!Parent.US_DDTCExemptionCode.IsEmpty)
				{
					Parent.US_DDTCLicenseNoInfo.AddNotification(DDTCNotificationType, ValidationConstants.DDTC.EitherDDTCLicenseNoOrDDTCExemptionCodeIsRequired);
				}
				ValidateUS_DDTCLicenseType();
			}
		}

		protected override void CheckUS_DDTCLicenseType()
		{
			base.CheckUS_DDTCLicenseType();
			if (OGAIndicatorList.IsToBeDeclared(InvoiceLine.US_DDTCInd) && IsPGAValidationApplicable())
			{
				if (Parent.US_DDTCLicenseType.IsEmpty)
				{
					if (!Parent.US_DDTCLicenseNo.IsEmpty)
					{
						Parent.US_DDTCLicenseTypeInfo.AddNotification(DDTCNotificationType, ValidationConstants.DDTC.DDTCLicenseTypeIsRequired);
					}
					else if (Parent.US_DDTCExemptionCode.IsEmpty)
					{
						Parent.US_DDTCLicenseTypeInfo.AddNotification(DDTCNotificationType, ValidationConstants.DDTC.EitherDDTCLicenseTypeOrDDTCExemptionCodeIsRequired);
					}
				}
				else
				{
					ListValidation.IfInvalidCode(DDTCNotificationType, Parent.US_DDTCLicenseTypeInfo, Parent.AddInfoLookups.DDTCLicenseTypeCodes, ListValidation.GetNotificationMessage(Parent.US_DDTCLicenseTypeInfo).ToString());
					if (!Parent.US_DDTCExemptionCode.IsEmpty)
					{
						Parent.US_DDTCLicenseTypeInfo.AddNotification(DDTCNotificationType, ValidationConstants.DDTC.EitherDDTCLicenseTypeOrDDTCExemptionCodeIsRequired);
					}
				}
				ValidateUS_DDTCLicenseNo();
			}
		}

		protected override void CheckUS_DDTCRegistrationNo()
		{
			base.CheckUS_DDTCRegistrationNo();
			if (OGAIndicatorList.IsToBeDeclared(InvoiceLine.US_DDTCInd) && Parent.US_DDTCRegistrationNo.IsEmpty && IsPGAValidationApplicable())
			{
				var notificationType = DDTCNotificationType;
				if (!Parent.US_DDTCExemptionCode.IsEmpty)
				{
					notificationType = CargoWise.EntityFramework.NotificationType.Warning;
				}
				Parent.US_DDTCRegistrationNoInfo.AddNotification(notificationType, MandatoryValidation.YouHaveNotEnteredMessage("DDTC Registration No."));
			}
		}

		protected override void CheckUS_SetInd()
		{
			base.CheckUS_SetInd();

			if (InvoiceLine.IsEntrySummaryValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_SetIndInfo, Parent.AddInfoLookups.SetIndicatorList);

				ValidateSetXAndVAgainstRelationship(InvoiceLine.US_SetIndInfo);
			}
			ValidateUS_TTBInd();
			Parent.Validation.ValidateJI_ParentID();
		}

		public override void ValidateSetIndicator()
		{
			base.ValidateSetIndicator();
			ValidateUS_SetInd();
		}

		protected override void CheckUS_LaceyIndicator()
		{
			base.CheckUS_LaceyIndicator();

			ListValidation.MessageErrorIfInvalidCode(Parent.US_LaceyIndicatorInfo, Parent.AddInfoLookups.US_OGAIndicatorList);

			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine("Lacey Act", Parent.US_LaceyIndicatorInfo);
			}
			else if (IsPGAValidationApplicable())
			{
				if (!Parent.IsInformal)
				{
					AgencyRequirementsValidator.ValidatePGA(Parent.US_LaceyIndicatorInfo, "Lacey Act", true, InvoiceLine.PGARequirementIndicator.HasLaceyActRequirement);
					AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.US_LaceyIndicatorInfo, "Lacey Act", Parent.LaceyActLines.Cast<IPGADataCorrection>());
					AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.US_LaceyIndicatorInfo, Parent.PGARequirementIndicator.RequireACE_LaceyData);
				}
				AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.Lacey, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_LaceyIndicatorInfo);
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_LaceyIndicatorInfo);
			}
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.Lacey, InvoiceLine.OGAAgencyRequirements);
			ValidateUS_LaceyDisclaimReason();
		}

		protected override void CheckUS_LaceyDisclaimReason()
		{
			base.CheckUS_LaceyDisclaimReason();
			AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.US_LaceyDisclaimReasonInfo, Parent.US_LaceyIndicator, Parent.AddInfoLookups.LaceyDisclaimReasonList);
			AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.Lacey, InvoiceLine.OGAAgencyRequirements);

			ValidateUS_LaceyIndicator();
		}

		protected override void CheckUS_FWSInd()
		{
			base.CheckUS_FWSInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_FWSIndInfo, Parent.AddInfoLookups.US_OGAIndicatorList);

			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine("FWS", InvoiceLine.US_FWSIndInfo);
			}
			else if (IsPGAValidationApplicable())
			{
				var isFWSEffective = ZZCustomsFunctionality.IsFWSEffective;
				AgencyRequirementsValidator.ValidatePGA(Parent.US_FWSIndInfo, "FWS", isFWSEffective, InvoiceLine.PGARequirementIndicator.HasFWSRequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.US_FWSIndInfo, "FWS", Parent.FWSHeaders.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.US_FWSIndInfo, InvoiceLine.PGARequirementIndicator.RequireFWS);
				AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.FWS, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_FWSIndInfo);
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_FWSIndInfo);
			}

			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.FWS, InvoiceLine.OGAAgencyRequirements);
			ValidateUS_FWSDisclaimReason();
		}
		internal const string FWSNotAvailable = "FWS is currently in Pilot mode - only attempt to send FWS if you are an approved participant in the FWS Pilot program.";

		protected override void CheckUS_FWSDisclaimReason()
		{
			base.CheckUS_FWSDisclaimReason();
			if (ZZCustomsFunctionality.IsFWSEffective)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.US_FWSDisclaimReasonInfo, Parent.US_FWSInd, Parent.AddInfoLookups.FWSDisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.FWS, InvoiceLine.OGAAgencyRequirements);
			}

			ValidateUS_FWSInd();
		}

		protected override void CheckUS_FTZCurrentTariff()
		{
			base.CheckUS_FTZCurrentTariff();

			if (Parent.ImportEntryType == EntryTypeList.Codes.ConsumptionFTZ && IsEntrySummaryOrCargoReleaseValidationMode && Parent.IsACECargoCertificationMode)
			{
				if (Parent.US_FTZCurrentTariff.IsEmpty)
				{
					if (Parent.US_ZoneStatus == ZoneStatusList.Codes.PrivilegedForeign && !InvoiceLine.JI_Tariff.IsEmpty && InvoiceLine.FTZCurrentDutyDate.IsValid)
					{
						var tariff = new USCTariff.Loader(InvoiceLine.Factory).LoadBestMatch(InvoiceLine.JI_Tariff, InvoiceLine.FTZCurrentDutyDate);
						if (tariff == null)
						{
							Parent.US_FTZCurrentTariffInfo.AddWarning(string.Format(CultureInfo.InvariantCulture, FTZCurrentTariffShouldBeEntered, InvoiceLine.JI_FormattedTariff));
						}
					}
				}
				else
				{
					if (InvoiceLine.US_FTZCurrentTariff == InvoiceLine.JI_Tariff)
					{
						Parent.US_FTZCurrentTariffInfo.AddMessageError(FTZCurrentTariffShouldNotEqualToJI_Tariff);
					}
					else
					{
						var tariff = new USCTariff.Loader(InvoiceLine.Factory).LoadBestMatch(Parent.US_FTZCurrentTariff, InvoiceLine.FTZCurrentDutyDate);
						if (tariff == null)
						{
							Parent.US_FTZCurrentTariffInfo.AddMessageError(TariffValidator.GetTariffFoundButNotValid(Parent.US_FTZCurrentTariffInfo.HumanReadableName, InvoiceLine.FTZCurrentDutyDate.ToShortDateString()));
						}
						else
						{
							var invoiceTariff = new USCTariff.Loader(InvoiceLine.Factory).LoadBestMatch(InvoiceLine.JI_Tariff, InvoiceLine.FTZCurrentDutyDate);
							if (invoiceTariff != null)
							{
								Parent.US_FTZCurrentTariffInfo.AddWarning(FTZTariffUsedWhenTariffExpired);
							}
						}
					}
				}
			}
		}
		internal const string FTZCurrentTariffShouldBeEntered = "The tariff {0} is no longer valid. Please enter a FTZ Current Tariff.";
		internal const string FTZCurrentTariffShouldNotEqualToJI_Tariff = "Please enter a valid number which is different to Tariff.";
		internal const string FTZTariffUsedWhenTariffExpired = "FTZ Current Tariff should only be used when the Tariff is currently expired.";

		protected override void CheckUS_FDAContactPhoneNo()
		{
			base.CheckUS_FDAContactPhoneNo();

			if (ShouldCheckFDAContactInfo)
			{
				if (Parent.US_FDAContactPhoneNo.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_FDAContactPhoneNoInfo);
				}
			}
		}

		protected override void CheckUS_FDAContactEmail()
		{
			base.CheckUS_FDAContactEmail();
			if (ShouldCheckFDAContactInfo)
			{
				if (!Parent.US_FDAContactEmail.IsEmpty)
				{
					if (!EmailAddressValidation.IsEmailAddressValid(Parent.US_FDAContactEmail))
					{
						Parent.US_FDAContactEmailInfo.AddMessageError("Invalid email format");
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_FDAContactEmailInfo);
				}
			}
		}

		protected override void CheckUS_FDAContactName()
		{
			base.CheckUS_FDAContactName();
			if (ShouldCheckFDAContactInfo)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_FDAContactNameInfo);
			}
		}

		bool ShouldCheckFDAContactInfo
		{
			get { return (OGAIndicatorList.IsToBeDeclared(InvoiceLine.US_TSCAInd) || OGAIndicatorList.IsToBeDeclared(InvoiceLine.US_ODSInd)) && IsPGAValidationApplicable(); }
		}

		protected override void CheckUS_NMFSSIMPInd()
		{
			base.CheckUS_NMFSSIMPInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NMFSSIMPIndInfo, Parent.AddInfoLookups.US_OGAIndicatorWithoutDisclaimerList);
			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine("NMFS SIMP", InvoiceLine.US_NMFSSIMPIndInfo);
			}
			else if (IsPGAValidationApplicable())
			{
				if (Parent.US_NMFSSIMPInd != OGAIndicatorList.Codes.Disclaimed)
				{
					AgencyRequirementsValidator.ValidatePGA(Parent.US_NMFSSIMPIndInfo, GovernmentAgencyProgramCodeList.Codes.SIMP, true, InvoiceLine.PGARequirementIndicator.HasNMFSSIMRequirement);
					AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.US_NMFSSIMPIndInfo, GovernmentAgencyProgramCodeList.Codes.SIMP, Parent.NMFSSIMPLines.Cast<IPGADataCorrection>());
				}
				AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.SIMP, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_NMFSSIMPIndInfo);
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_NMFSSIMPIndInfo);
			}
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.SIMP, InvoiceLine.OGAAgencyRequirements);
		}

		protected override void CheckUS_ProductExclusion()
		{
			base.CheckUS_ProductExclusion();

			ValidateProvTariffOrProductExclusionForSteelProducts(Parent.US_ProductExclusionInfo, null);

			var exclusionCode = Parent.US_ProductExclusion;
			if (exclusionCode == AdditionalDeclarationTypeCodeList.Codes._02 || exclusionCode == AdditionalDeclarationTypeCodeList.Codes._03)
			{
				if (Parent.IsOnlySteelProductAvailable && exclusionCode != AdditionalDeclarationTypeCodeList.Codes._02)
				{
					Parent.US_ProductExclusionInfo.AddMessageError(SteelProductExclusionNotValid);
				}
				else if (Parent.IsOnlyAluminumProductAvailable && exclusionCode != AdditionalDeclarationTypeCodeList.Codes._03)
				{
					Parent.US_ProductExclusionInfo.AddMessageError(AluminumProductExclusionNotValid);
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_ProductExclusionInfo, Parent.AddInfoLookups.ProductExclusionList);
			}

			ValidateUS_ExclusionNumber();
		}
		internal const string SteelProductExclusionNotValid = "For tariffs in chapter 72 or 73, only Steel Product Exclusion can be made.";
		internal const string AluminumProductExclusionNotValid = "For tariffs in chapter 76, only Aluminum Product Exclusion can be made.";
		internal const string ProgTariffStartWith9903NotAllowed = "If Product Exclusion is claimed then no Section 232 9903 Program Tariff number should be used.";

		void ValidateProvTariffOrProductExclusionForSteelProducts(ZPropertyInfo propertyInfo, Action<ZString, ZPropertyInfo> extraValidationForSteelProductWithApplicableSupTariff)
		{
			var invoiceLine = InvoiceLine;
			var isSteel = invoiceLine.IsOnlySteelProductAvailable;
			if (isSteel)
			{
				var originFromEUN = invoiceLine.IsOriginFromEUN;
				var steelFromEUNForAnySupTariff = invoiceLine.IsSteelOriginFromEUNForAnySupTariff;
				var steelProductWithApplicableSupTariff = invoiceLine.IsSteelProductWithApplicableSupTariff;
				var isTariffWithGAEType = invoiceLine.IsTariffWithGAEType;
				var supTariff = invoiceLine.US_SupTariff;
				var exclusionCode = invoiceLine.US_ProductExclusion;

				if (originFromEUN && steelFromEUNForAnySupTariff && !isTariffWithGAEType)
				{
					if (supTariff.IsEmpty && exclusionCode.IsEmpty)
					{
						propertyInfo.AddMessageError(ZString.Format(SteelTariffRequiresEitherProvTariffNumOrProductExclusionNum, "the European Union"));
					}
					else if (!supTariff.IsEmpty && !exclusionCode.IsEmpty)
					{
						propertyInfo.AddMessageError(OnlyEitherProvTariffOrProductExclusionCodeAllowed);
					}
				}
				else if (steelProductWithApplicableSupTariff)
				{
					var countryOfOrigin = invoiceLine.US_UC_NKCountryOfOrigin;
					if (countryOfOrigin == Core.Constants.CountryCodes.Japan || countryOfOrigin == Core.Constants.CountryCodes.UnitedKingdom)
					{
						var country = countryOfOrigin == Core.Constants.CountryCodes.Japan ? "Japan" : "United Kingdom";
						var specificProvTariffWhenExclusionNumIsEntered = "99038180";
						if (supTariff.IsEmpty && exclusionCode.IsEmpty)
						{
							propertyInfo.AddMessageError(ChildLineTariffNotEntered);
						}
						else if (!isTariffWithGAEType && invoiceLine.ApplicableSupTariffs.Any(x => x.Tariff == specificProvTariffWhenExclusionNumIsEntered) &&
							(exclusionCode.IsEmpty ^ supTariff != specificProvTariffWhenExclusionNumIsEntered))
						{
							if (exclusionCode.IsEmpty && propertyInfo.Name == JobComInvoiceLine.Schema.US_ProductExclusion)
							{
								propertyInfo.AddMessageError(ZString.Format(SteelTariffRequiresProductExclusionNumWhen99038180IsEntered, country));
							}
							else if (!exclusionCode.IsEmpty && propertyInfo.Name == JobComInvoiceLine.Schema.US_SupTariff)
							{
								propertyInfo.AddMessageError(ZString.Format(SteelTariffRequires99038180WhenProductExclusionNumHasValue, country));
							}
						}
					}
					else if (extraValidationForSteelProductWithApplicableSupTariff != null)
					{
						extraValidationForSteelProductWithApplicableSupTariff(countryOfOrigin, propertyInfo);
					}
				}
			}
		}
		internal const string OnlyEitherProvTariffOrProductExclusionCodeAllowed = "Only one, Prov Tariff or Product Exclusion Code can be used.";
		internal const string SteelTariffRequiresEitherProvTariffNumOrProductExclusionNum = "Steel from {0} requires either a Prov/Prog Tariff Number or a Product Exclusion Number.";
		internal const string SteelTariffRequires99038180WhenProductExclusionNumHasValue = "For Steel from {0}, if Product Exclusion Number is entered, then the Prov/Prog Tariff Number (9903.81.80) is also required.";
		internal const string SteelTariffRequiresProductExclusionNumWhen99038180IsEntered = "For Steel from {0}, if Prov/Prog Tariff Number is 9903.81.80, then the Product Exclusion Number is also required.";

		protected override void CheckUS_ExclusionNumber()
		{
			base.CheckUS_ExclusionNumber();

			var productExclusion = Parent.US_ProductExclusion;
			if (productExclusion.IsEmpty)
			{
				if (!Parent.US_ExclusionNumber.IsEmpty)
				{
					Parent.US_ExclusionNumberInfo.AddMessageError(ProductExclustionNumberNotAllowed);
				}
			}
			else
			{
				var exclusionNumber = Parent.US_ExclusionNumber;
				UniversalReferenceDataHelper.ValidateNumberByRegexInZZ(
					Parent.Factory,
					Parent.US_ExclusionNumberInfo,
					exclusionNumber,
					productExclusion,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USProductExclusionTypes,
					Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USProductExclusionCodeMask,
					Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USProductExclusionCodeErrorText,
					InvalidProductExclusionNumberMessagePrefix);

				if (Parent.IsCombinedLine() && !Parent.IsNormalTariffLine())
				{
					Parent.US_ExclusionNumberInfo.AddMessageError(AluminumProductNumberInCombinedLines);
				}
			}
		}
		internal const string ProductExclustionNumberNotAllowed = "Product Exclusion Number is not required when exclusion code is empty.";
		internal const string InvalidProductExclusionNumberMessagePrefix = "Product Exclusion Number is empty or incorrect. ";
		internal const string AluminumProductNumberInCombinedLines = "For combined lines, Product Exclusion Number should be entered on invoice line where classification tariff number exists.";

		bool IsEntrySummaryOrCargoReleaseValidationMode
		{
			get { return Parent.Declaration != null && Parent.Declaration.IsEntrySummaryOrCargoReleaseValidationMode; }
		}

		protected override void CheckUS_ControlledGroupName()
		{
			base.CheckUS_ControlledGroupName();

			var invoiceLine = InvoiceLine;
			if (invoiceLine.IsCBMAProductClaimAndIsNotCBMA23Effective && invoiceLine.IsACEEntrySummaryValidationMode && invoiceLine.US_ControlledGroupName.IsEmpty)
			{
				invoiceLine.US_ControlledGroupNameInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(invoiceLine.US_ControlledGroupNameInfo.HumanReadableName));
			}
		}

		protected override void CheckUS_FPI()
		{
			base.CheckUS_FPI();

			var invoiceLine = InvoiceLine;
			if (invoiceLine.IsCBMAProductClaim && invoiceLine.IsACEEntrySummaryValidationMode)
			{
				if (invoiceLine.US_FPI.IsEmpty)
				{
					invoiceLine.US_FPIInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(InvoiceLine.US_FPIInfo.HumanReadableName));
				}
				else if (invoiceLine.IsCBMA23Effective)
				{
					if (EntryTypeList.IsExWarehouseTypeOrFTZ(invoiceLine.ImportEntryType))
					{
						if (!Regex.IsMatch(invoiceLine.US_FPI, @"(^TTB-FP-[a-zA-Z0-9]{7}$)|(^[BSW]\S{2,11}\d{2}$)"))
						{
							invoiceLine.US_FPIInfo.AddWarning(ForeignProducerIdentifierFormat);
						}
					}
					else
					{
						if (!Regex.IsMatch(invoiceLine.US_FPI, @"^TTB-FP-[a-zA-Z0-9]{7}$"))
						{
							invoiceLine.US_FPIInfo.AddWarning(FPI_ForeignProducerIdentifierFormat);
						}
					}
				}
				else if (!invoiceLine.IsCBMA23Effective && !Regex.IsMatch(invoiceLine.US_FPI, @"^[BSW]\S{2,11}\d{2}$"))
				{
					invoiceLine.US_FPIInfo.AddWarning(FPB_FPS_FPW_ForeignProducerIdentifierFormat);
				}
			}
		}
		internal const string FPI_ForeignProducerIdentifierFormat = "The FPI as assigned by TTB should be in the format TTB-FP-XXXXXXX where XXXXXXX is any combination of alphanumeric characters.";
		internal const string FPB_FPS_FPW_ForeignProducerIdentifierFormat = "The format of the Foreign Producer Identifier is:\r\n·The 1st character of the alcohol type (Beer=B, Wine or Cider=W, Spirits=S); followed by\r\n·Up to the 1st 6 characters of the Foreign Producer Name (eliminating any space value); followed by\r\n·Up to the 1st 5 characters of the Foreign Producer's international postal code; followed by\r\n·The 2-digit calendar year of the claim.";
		internal const string ForeignProducerIdentifierFormat = FPB_FPS_FPW_ForeignProducerIdentifierFormat + "\r\n·Or TTB-FP-XXXXXXX where XXXXXXX is any combination of alphanumeric characters.";

		protected override void CheckUS_AllocationQuantity()
		{
			base.CheckUS_AllocationQuantity();
			var invoiceLine = InvoiceLine;
			if (invoiceLine.IsCBMAProductClaimAndIsNotCBMA23Effective && invoiceLine.IsACEEntrySummaryValidationMode && invoiceLine.US_AllocationQuantity <= ZDecimal.Zero)
			{
				invoiceLine.US_AllocationQuantityInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(invoiceLine.US_AllocationQuantityInfo.HumanReadableName));
			}
		}

		protected override void CheckUS_CBMADefaultTaxRate()
		{
			base.CheckUS_CBMADefaultTaxRate();
			var invoiceLine = InvoiceLine;

			if (invoiceLine.IsCBMAProductClaimAndIsCBMA23Effective && invoiceLine.IsACEEntrySummaryValidationMode)
			{
				MandatoryValidation.MessageErrorIfIsNegative(invoiceLine.US_CBMADefaultTaxRateInfo);
			}
		}

		void ValidateCustomsValueAgainstFWSUSDValues()
		{
			var parent = Parent;

			if (!parent.IsChildLine
					&& (parent.IsFWSDeclared || (parent.IsParentLine && parent.ChildLines.Cast<JobComInvoiceLine>().Any(x => x.IsFWSDeclared)))
					&& parent.TotalCustomsValueIncludingChildLines < parent.TotalFWSUSDValueIncludingChildLines)
			{
				Parent.AddRowMessageError(ZString.Format(FWSTotalValue, parent.TotalFWSUSDValueIncludingChildLines, parent.TotalCustomsValueIncludingChildLines));
			}
		}
		internal const string FWSTotalValue = "The rounded total of all FWS Values ({0}) should be less than the total rounded Customs Value ({1}) (including child lines).";

		protected override void CheckUS_Prim_NA()
		{
			base.CheckUS_Prim_NA();

			var invoiceLine = InvoiceLine;
			if (invoiceLine.US_Prim_NA && !invoiceLine.US_RN_NKPrimCtry.IsEmpty)
			{
				invoiceLine.US_Prim_NAInfo.AddMessageError(PrimaryNACannotBeTicked);
			}

			ACEImportJobComInvoiceLineValidation.CheckSmeltAndCastInformationRequired(invoiceLine.US_Prim_NAInfo, invoiceLine, !invoiceLine.US_Prim_NA && invoiceLine.US_RN_NKPrimCtry.IsEmpty, invoiceLine.US_Prim_NA, () =>
			{
				ValidateUS_RN_NKPrimCtry();
				invoiceLine.Validation.ValidateJI_Tariff();
			});
		}
		internal const string PrimaryNACannotBeTicked = "Primary Country N/A cannot be ticked if Primary Country is entered.";

		protected override void CheckUS_RN_NKPrimCtry()
		{
			base.CheckUS_RN_NKPrimCtry();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_RN_NKPrimCtryInfo, Parent.AddInfoLookups.PrimCtries);
			ACEImportJobComInvoiceLineValidation.CheckSmeltAndCastInformationRequired(Parent.US_RN_NKPrimCtryInfo, Parent, !Parent.US_Prim_NA && Parent.US_RN_NKPrimCtry.IsEmpty, !Parent.US_RN_NKPrimCtry.IsEmpty, () =>
			{
				ValidateUS_Prim_NA();
				InvoiceLine.Validation.ValidateJI_Tariff();
			});
		}

		protected override void CheckUS_Sec_NA()
		{
			base.CheckUS_Sec_NA();

			var invoiceLine = InvoiceLine;
			if (invoiceLine.US_Sec_NA && !invoiceLine.US_RN_NKSecCtry.IsEmpty)
			{
				invoiceLine.US_Sec_NAInfo.AddMessageError(SecondaryNACannotBeTicked);
			}
			ACEImportJobComInvoiceLineValidation.CheckSmeltAndCastInformationRequired(invoiceLine.US_Sec_NAInfo, invoiceLine, !invoiceLine.US_Sec_NA && invoiceLine.US_RN_NKSecCtry.IsEmpty, invoiceLine.US_Sec_NA, () =>
			{
				ValidateUS_RN_NKSecCtry();
				invoiceLine.Validation.ValidateJI_Tariff();
			});
		}
		internal const string SecondaryNACannotBeTicked = "Secondary Country N/A cannot be ticked if Secondary Country is entered.";

		protected override void CheckUS_RN_NKSecCtry()
		{
			base.CheckUS_RN_NKSecCtry();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_RN_NKSecCtryInfo, Parent.AddInfoLookups.SecCtries);
			ACEImportJobComInvoiceLineValidation.CheckSmeltAndCastInformationRequired(Parent.US_RN_NKSecCtryInfo, Parent, !Parent.US_Sec_NA && Parent.US_RN_NKSecCtry.IsEmpty, !Parent.US_RN_NKSecCtry.IsEmpty, () =>
			{
				ValidateUS_Sec_NA();
				InvoiceLine.Validation.ValidateJI_Tariff();
			});
		}

		protected override void CheckUS_RN_NKCastCtry()
		{
			base.CheckUS_RN_NKCastCtry();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_RN_NKCastCtryInfo, Parent.AddInfoLookups.CastCtries);
			ACEImportJobComInvoiceLineValidation.CheckSmeltAndCastInformationRequired(Parent.US_RN_NKCastCtryInfo, Parent, Parent.US_RN_NKCastCtry.IsEmpty, !Parent.US_RN_NKCastCtry.IsEmpty, () =>
			{
				InvoiceLine.Validation.ValidateJI_Tariff();
			});
		}

		protected override void CheckUS_RN_NKCertOrigin()
		{
			base.CheckUS_RN_NKCertOrigin();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_RN_NKCertOriginInfo);

			if(!Parent.US_RN_NKCertOrigin.IsEmpty && Parent.IsCombinedLine() && !Parent.IsNormalTariffLine())
			{
				Parent.US_RN_NKCertOriginInfo.AddMessageError(CertificateOfOriginShouldBeEnteredOnNormalInvoiceLine);
			}
		}
		internal const string CertificateOfOriginShouldBeEnteredOnNormalInvoiceLine = "For combined lines, Certificate Of Origin should be entered on invoice line where classification tariff number exists.";
		internal const string MeltCtryShouldBeEnteredOnNormalInvoiceLine = "For combined lines, Melted Country should be entered on invoice line where classification tariff number exists.";

		internal const string MeltCtryExcluded = "The code you have selected is in the exclusion list.";
		protected override void CheckUS_RN_NKMeltCtry()
		{
			base.CheckUS_RN_NKMeltCtry();

			var meltCountry = Parent.US_RN_NKMeltCtry;
			if (!meltCountry.EqualsIgnoringCase(MeltCountryOther))
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_RN_NKMeltCtryInfo);
				var meltCondition = Parent.TariffMELTCondition;
				if (meltCondition != null && !Parent.US_RN_NKMeltCtryInfo.HasNotifications())
				{
					var notificationType = ConditionChecker.CheckConditionSeverity(meltCondition);
					if (meltCountry.IsEmpty)
					{
						Parent.US_RN_NKMeltCtryInfo.AddNotification(notificationType, MandatoryValidation.YouHaveNotEnteredMessage(Parent.US_RN_NKMeltCtryInfo.HumanReadableName));
					}
					else if (ConditionChecker.CheckConditionApplicabilitiesExcludeCountry(meltCondition, meltCountry, Parent.EffectiveDateForDutyRate))
					{
						Parent.US_RN_NKMeltCtryInfo.AddNotification(notificationType, MeltCtryExcluded);
					}
				}
			}

			if (!meltCountry.IsEmpty && Parent.IsCombinedLine() && !Parent.IsNormalTariffLine())
			{
				Parent.US_RN_NKMeltCtryInfo.AddMessageError(MeltCtryShouldBeEnteredOnNormalInvoiceLine);
			}
		}
		internal const string MeltCountryOther = "ZZ";
		
		protected override void CheckUS_DisclaimSanctions()
		{
			base.CheckUS_DisclaimSanctions();
			if (ZZCustomsFunctionality.IsSanctionsEffective && !Parent.US_DisclaimSanctions)
			{
				if (Parent.TariffMatchesFishingCondition && !Parent.HasFishingInformations)
				{
					Parent.US_DisclaimSanctionsInfo.AddMessageError(string.Format(SanctionsDataRequirementMessage, "Fishing"));
				}
				else if (Parent.TariffMatchesMiningCondition && !Parent.HasMiningInformations)
				{
					Parent.US_DisclaimSanctionsInfo.AddMessageError(string.Format(SanctionsDataRequirementMessage, "Mining"));
				}
			}
		}
		internal const string SanctionsDataRequirementMessage = "The article tariff for this line and the country of origin require Sanctions data to be submitted. Please either enter data in the {0} Information grid or tick the Disclaim box as appropriate.";
	}
}
