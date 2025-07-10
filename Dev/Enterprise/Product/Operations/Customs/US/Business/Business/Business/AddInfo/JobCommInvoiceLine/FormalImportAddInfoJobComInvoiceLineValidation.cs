using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class FormalImportAddInfoJobComInvoiceLineValidation : CommonImportAddInfoJobComInvoiceLineValidation
	{
		#region Constants

		public new static class Constants
		{
			public const string NoDutyComputationFormulaAvailableCheckDutyAmount = "For the selected tariff number, no duty calculation formula is available. Please check the regulations and override duty if necessary.";

			public static class CountryOfExport
			{
				public const string SilkFromChina = "Silk articles exported from China do not require a visa document";
			}

			public static class VisaNumber
			{
				public const string VisaNumberNot9Characters = "The visa number should be 9 characters long";
				public const string FirstCharacterVisaNumberShouldStartWith = "The visa number should start with ";
				public const string SecondThirdLettersVisaNumbersShouldBeCountryOfOrigin = "The second and third characters should be the country of origin.";
				public const string FirstCharacterVisaNumberShouldBeANumber = "The first character should be a number indicating the last digit of the export date year.";
				public const string VisaRequiredForTariffOriginAndCategory = "Visa number is required for the entered tariff, country of origin and textile category number.";
				public const string MayNotBeEnteredWhereSPISecondaryM = "Visa number is not valid for entry if Secondary SPI is M.";
				public const string VisaNoRequired = "Visa number is required when Textile Export Date is entered.";
				public const string VisaNumberNotRequired = "Visa number should only be entered against quota entry types";
				public const string InvalidFormatForBrassiereTariff98201115 = ", the first character being numeric, representing the year of issuance (i.e. '2' for 2002), then two alphas 'CB' indicating the Caribbean Basin and then a six digit serial number starting with a '1' for a producer and '2' for a entity controlling production.";
				public const string Last6PositionsShouldBeNumbers = "Positions 4 through 9 of Visa Number are numbers assigned by the country of origin.";
				public const string VisaIsMandatoryForEnteredTariff = "Visa number is mandatory for the entered tariff.";
			}

			public static class FCC
			{
				public const string FCCRequiredButBlank = "FCC Indicator is required when tariff is an FCC tariff";
				public const string FCCRequiredButDisclaimed = "The selected tariff requires the declaration of FCC data. Indicator must be 'Declared'";
				public const string FCCLineRequired = "At least one FCC Line is required when FCC Indicator is 'Declared'";
				public const string FCCLineNotAllowed = "FCC Lines may not be entered if FCC Indicator is blank or 'Disclaimed'";
				public const string FCCLineNotAllowedForSecondaryTariffs = "FCC details should not be entered where the secondary tariff is entered under 98020040 or 98020050 as per CSMS #93-000514";
				public const string FCCNotRequired = "The tariff does not indicate that FCC reporting is required. If FCC Indicator is 'Declared', FCC data will be sent in the appropriate messages.";
				public const string FCCDisclaimedInvalid = "It is invalid to Disclaim FCC when the entered tariff does not require FCC reporting.";
				public const string FCCNotRequiredWhenOriginIsUS = "FCC Indicator is not required when the country of Origin is US";
				public const string FCCNoLongerBesentToCBP = "FCC data can no longer be sent to CBP.";
			}

			public static class DOT
			{
				public const string DOTRequiredButBlank = "DOT Indicator is required when tariff is an DOT tariff";
				public const string DOTRequiredButDisclaimed = "The selected tariff requires the declaration of DOT data. Indicator must be 'Declared'";
				public const string DOTLineRequired = "At least one DOT Line is required when DOT Indicator is 'Declared'";
				public const string DOTLineNotAllowed = "DOT Lines may not be entered if DOT Indicator is blank or 'Disclaimed'";
				public const string DOTNotRequired = "The tariff does not indicate that DOT reporting is required. If DOT Indicator is 'Declared', DOT data will be sent in the appropriate messages.";
				public const string DOTDisclaimedInvalid = "It is invalid to Disclaim DOT when the entered tariff does not require DOT reporting.";
			}

			public static class OGA
			{
				public const string SoftwoodLumber = "Softwood Lumber";
				public const string ADDCVDDetails = "ADD/CVD Details";
			}

			public static class PGA
			{
				public const string PGAWithOGAMustBeDeclared = "{0} data must be sent. If you are approved for the {0} pilot program you can send via ACE. If not, you must send under ACS Cargo Release and send {1} data.";
				public const string PGAWithOGADisclaimedInACS = "This includes sending a Disclaimer flag.";
			}
		}

		#endregion

		public FormalImportAddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine addInfo)
			: base(addInfo)
		{
		}

		protected bool IsPGAValidationApplicable()
		{
			var result = false;
			var declaration = InvoiceLine.Declaration;
			if (declaration != null)
			{
				result = declaration.IsPGAValidationOn();
			}
			return result;
		}

		protected override void CheckUS_ManifestQty()
		{
			base.CheckUS_ManifestQty();
			ValidateUS_ManifestUQ();
		}

		protected override string GetManifestQtyNotification()
		{
			return FTZPackQtyIsRequired;
		}
		internal const string FTZPackQtyIsRequired = "FTZ Pack Quantity is required.";

		protected override void CheckUS_ManifestUQ()
		{
			var declaration = Parent.Declaration;
			var isFTZIntegrationEnabled = declaration != null && declaration.IsFTZIntegrationEnabled;

			if (isFTZIntegrationEnabled)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_ManifestUQInfo, Parent.AddInfoLookups.ManifestUQList);
			}
			else if (declaration != null && declaration.IsConsumptionFTZ)
			{
				ListValidation.WarnIfInvalidCode(Parent.US_ManifestUQInfo, Parent.AddInfoLookups.ManifestUQList, ListValidation.InvalidCodeMessageError);
			}
		}

		protected override void CheckUS_WHSEntryNumber()
		{
			base.CheckUS_WHSEntryNumber();
			if (Parent.US_WHSEntryNumber.IsEmpty && Parent.JI_PartNo_CanBeSetByCustomer)
			{
				var declaration = Parent.Declaration;
				if (declaration != null && declaration.IsWHSUniversalXMLActive && declaration.SupportsBondedWarehousing
					&& (declaration.IsENSFormalImportAndConsumptionFTZ && declaration.US_EntryDateElectionCode != EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate))
				{
					Parent.US_WHSEntryNumberInfo.AddMessageError(WarehouseEntryNoIsRequired);
				}
			}
		}
		internal const string WarehouseEntryNoIsRequired = "A valid FTZ Admission Number is required for an FTZ withdrawal.";

		protected override void CheckUS_WHSEntryLineNo()
		{
			base.CheckUS_WHSEntryLineNo();
			if (Parent.US_WHSEntryLineNo <= 0 && Parent.JI_PartNo_CanBeSetByCustomer)
			{
				var declaration = Parent.Declaration;
				if (declaration != null && declaration.IsWHSUniversalXMLActive && declaration.SupportsBondedWarehousing
					&& (declaration.IsExWarehouseEntryType || declaration.IsImportByExternalBroker || (declaration.IsENSFormalImportAndConsumptionFTZ && declaration.US_EntryDateElectionCode != EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate)))
				{
					string term = declaration.IsENSFormalImportAndConsumptionFTZ ? "FTZ Admission" : "Whs";
					string action = declaration.IsENSFormalImportAndConsumptionFTZ ? "an FTZ withdrawal" : declaration.IsExWarehouseEntryType ? "a WHS withdrawal" : "Inventory Management integration";

					Parent.US_WHSEntryLineNoInfo.AddMessageError(WarehouseEntryLineNoIsRequired(term, action));
				}
			}
		}

		internal static string WarehouseEntryLineNoIsRequired(string term, string action)
		{
			return Res.GetString("8F1C4AE5-1B96-4DE1-A36F-747E3072C1BF", "A valid {0} Line Number is required for {1}.", term, action);
		}

		protected override void CheckUS_JI_ParentProduct()
		{
			base.CheckUS_JI_ParentProduct();
			var invoiceLineValidation = InvoiceLine.Validation;
			invoiceLineValidation.ValidateJI_PartNo();
			invoiceLineValidation.ValidateJI_InvoiceQuantity();
			invoiceLineValidation.ValidateJI_InvoiceUQ();
			ValidateUS_WHSEntryNumber();
		}

		protected override void CheckUS_DateOfExport()
		{
			base.CheckUS_DateOfExport();
			if (Parent.Declaration is JobDeclaration declaration)
			{
				if (declaration.US_EntryType == EntryTypeList.Codes.ConsumptionFTZ)
				{
					var tariff = Parent.ImportTariff;
					if (tariff != null && tariff.UE_QuotaIndicator)
					{
						MandatoryValidation.WarnIfNotEntered(Parent.US_DateOfExportInfo);
					}
				}

				Customs.Business.BaseJobDeclarationValidation.CheckDateOfArrivalIsNotBeforeDateOfExport(Parent.US_DateOfExportInfo, declaration.JE_DateOfArrival, Parent.US_DateOfExport);
			}
		}

		protected override void CheckUS_TaxApply()
		{
			base.CheckUS_TaxApply();

			var declaration = Parent.Declaration;
			if (declaration?.IsClearedInPR ?? false)
			{
				if (Parent.US_TaxApply == TaxApplyList.Codes.Yes || Parent.US_TaxApply == TaxApplyList.Codes.Override)
				{
					Parent.US_TaxApplyInfo.AddMessageError(PortIsExceptFromIRTaxes);
				}
			}
			else
			{
				CheckTaxApply(Parent.US_TaxApplyInfo, Parent.US_TaxCode, Parent.US_TaxRateS, Parent.ImportTariff);
			}

			if (declaration != null && declaration.IsBulkLiquorTaxDeferred &&
				!Parent.US_TaxApply.IsEmpty && Parent.US_TaxApply != TaxApplyList.Codes.No)
			{
				Parent.US_TaxApplyInfo.AddMessageError(BulkLiquorNoTaxApply);
			}
		}
		internal const string BulkLiquorNoTaxApply = "Tax Apply indicator should be 'N' for bulk liquor imports.";
		internal const string PortIsExceptFromIRTaxes = "Port is Excempt from IR Taxes.";

		protected override void CheckUS_TaxRate()
		{
			base.CheckUS_TaxRate();

			if (Parent.US_TaxRate <= 0m && Parent.IsTaxRateSpecifiedManually)
			{
				Parent.US_TaxRateInfo.AddMessageError(TaxRateShouldBeEntered);
			}
		}

		protected override void CheckUS_TaxCode()
		{
			base.CheckUS_TaxCode();
			if (Parent.IsTaxRateOverridden)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_TaxCodeInfo, "tax code");
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.US_TaxCodeInfo, Parent.AddInfoLookups.TaxCodeList);
		}

		protected override void CheckUS_OverrideDuty()
		{
			base.CheckUS_OverrideDuty();

			if (!Parent.US_OverrideDuty)
			{
				if (InvoiceLine.ImportTariff is USCTariff importTariff && importTariff.DutyMightBeOverridable(InvoiceLine.EffectiveDateForDutyRate))
				{
					Parent.US_OverrideDutyInfo.AddWarning(Constants.NoDutyComputationFormulaAvailableCheckDutyAmount);
				}
			}
		}

		protected override void CheckUS_OverrideSupDuty()
		{
			base.CheckUS_OverrideSupDuty();

			if (!Parent.US_OverrideSupDuty)
			{
				if (InvoiceLine.ImportSupTariff is USCTariff importSupTariff && importSupTariff.DutyMightBeOverridable(InvoiceLine.EffectiveDateForDutyRate))
				{
					Parent.US_OverrideSupDutyInfo.AddWarning(Constants.NoDutyComputationFormulaAvailableCheckDutyAmount);
				}
			}
		}

		protected override void CheckUS_TaxRateS()
		{
			base.CheckUS_TaxRateS();

			if (Parent.US_TaxRateS.IsEmpty)
			{
				if (Parent.IsTaxApplicable)
				{
					Parent.US_TaxRateSInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("tax rate"));
				}
			}
			else if (Parent.IsCBMAProductClaim && Parent.IsTaxRateReduced)
			{
				// do nothing
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_TaxRateSInfo, Parent.AddInfoLookups.TaxRateList);
			}

			ValidateUS_TaxQty();
		}

		protected override void CheckUS_TTBRateDesignationCode()
		{
			base.CheckUS_TTBRateDesignationCode();
			if (Parent.IsCBMAProductClaim)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_TTBRateDesignationCodeInfo);
			}
		}

		protected override void CheckUS_TaxQty()
		{
			base.CheckUS_TaxQty();
			CheckUnitOfOverriddenTaxRateAgainstCustomsUQs(Parent.US_TaxQtyInfo, Parent.JI_CustomsUnitQty, Parent.JI_CustomsSecondUnitQty, Parent.US_TaxRateS);
		}

		protected override void CheckUS_98GoodsValue()
		{
			base.CheckUS_98GoodsValue();

			if (Parent.US_98GoodsValue > 0 && !Parent.IsUSReturnedGoodsTransaction)
			{
				Parent.US_98GoodsValueInfo.AddMessageError(_98GoodsValueExpectedToBeEnteredOnlyFor9801And9802);
			}

			ImportLinePriceValidator priceValidator = Parent.Factory.GetCachedValue<ImportLinePriceValidator>();
			priceValidator.ValidateFor9802TransactionValues(Parent, Parent.US_98GoodsValueInfo, Parent.TotalOriginalGoodsValueInUSD);
			priceValidator.Validate98GoodsValueForSetXLine(Parent.US_98GoodsValueInfo, Parent);

			ValidateMinumCostFor9802Products(Parent.US_98GoodsValueInfo, true);

			ValidateUS_98ValueInvCurr();
		}

		internal void ValidateMinumCostFor9802Products(ZPropertyInfo info, bool supLine)
		{
			if (Parent.IsEntrySummaryValidationMode && Parent.US_SupTariff.StartsWith("9802") && !Parent.IsChildLine)
			{
				var cusEntryLine = Parent.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, supLine);
				if (cusEntryLine != null && cusEntryLine.CL_CustomsValue.IsEmpty)
				{
					info.AddMessageError(ValidationConstants.EntrySummary.MinimumAssemblyOperationCostFor9802Products);
				}
			}
		}

		public const string _98GoodsValueExpectedToBeEnteredOnlyFor9801And9802 = "You have entered a value which should be entered only for tariffs, 9801, 9802 or 9822.05.";

		protected override void CheckUS_98ValueInvCurr()
		{
			base.CheckUS_98ValueInvCurr();

			if (Parent.US_98ValueInvCurr > 0 && !Parent.IsUSReturnedGoodsTransaction)
			{
				Parent.US_98ValueInvCurrInfo.AddMessageError(_98GoodsValueExpectedToBeEnteredOnlyFor9801And9802);
			}

			ImportLinePriceValidator priceValidator = Parent.Factory.GetCachedValue<ImportLinePriceValidator>();
			priceValidator.ValidateFor9802TransactionValues(Parent, Parent.US_98ValueInvCurrInfo, Parent.TotalOriginalGoodsValueInUSD);
			priceValidator.Validate98GoodsValueForSetXLine(Parent.US_98ValueInvCurrInfo, Parent);

			ValidateMinumCostFor9802Products(Parent.US_98ValueInvCurrInfo, true);

			ValidateUS_98GoodsValue();
		}

		protected override void CheckUS_IsBondedADD()
		{
			base.CheckUS_IsBondedADD();
			if (Parent.US_IsBondedADD)
			{
				if (Parent.Declaration is JobDeclaration declaration && declaration.US_ADDCVDSuretyCode.IsEmpty)
				{
					Parent.US_IsBondedADDInfo.AddMessageError(BondedADD_CVDRequiresSuretyCode);
				}

				IACCase acCase = Parent.AntidumpingDutyCase;

				if (acCase != null && acCase.IsCashRequired(Parent.DateForADD_CVD))
				{
					Parent.US_IsBondedADDInfo.AddMessageError(CashRequirdADD_CVDCasesCannotBeBonded);
				}
			}
		}

		protected override void CheckUS_VisaUQ()
		{
			base.CheckUS_VisaUQ();

			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.TextileCLassificationDetails, Parent.US_VisaUQInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_VisaUQInfo, Parent.AddInfoLookups.US_UnitOfMeasureList, (NoResString)VisaUQShouldBeInList);
			}
		}

		internal const string VisaUQShouldBeInList = "Please enter a valid Visa Type. The code you have selected is not in the Visa Types List.";

		protected override void CheckUS_IsBondedCVD()
		{
			base.CheckUS_IsBondedCVD();
			if (Parent.US_IsBondedCVD)
			{
				if (Parent.Declaration is JobDeclaration declaration && declaration.US_ADDCVDSuretyCode.IsEmpty)
				{
					Parent.US_IsBondedCVDInfo.AddMessageError(BondedADD_CVDRequiresSuretyCode);
				}

				IACCase acCase = Parent.CountervailingDutyCase;

				if (acCase != null && acCase.IsCashRequired(Parent.DateForADD_CVD))
				{
					Parent.US_IsBondedCVDInfo.AddMessageError(CashRequirdADD_CVDCasesCannotBeBonded);
				}
			}
		}

		internal const string BondedADD_CVDRequiresSuretyCode = "Bonded ADD/CVD cases require ADD/CVD Surety Code to be entered. Please enter one in 'Misc Options'.";
		internal const string CashRequirdADD_CVDCasesCannotBeBonded = "Cash required. This ADD/CVD cases cannot be bonded.";

		#region AntiDumping

		protected override void CheckUS_CVDCaseNo()
		{
			base.CheckUS_CVDCaseNo();
			Parent.Validation.MatchToProductValidation(Parent.US_CVDCaseNoInfo, () => Parent.Pivot?.CD_CVDCaseNo ?? ZString.Empty);
			ValidateUS_CVDDepositValue();
		}

		protected override void CheckUS_ADDCaseNo()
		{
			base.CheckUS_ADDCaseNo();
			Parent.Validation.MatchToProductValidation(Parent.US_ADDCaseNoInfo, () => Parent.Pivot?.CD_ADDCaseNo ?? ZString.Empty);
			ValidateUS_ADDDepositValue();
		}

		protected override void CheckUS_ADD_NA()
		{
			base.CheckUS_ADD_NA();
			Parent.Validation.MatchToProductValidation(Parent.US_ADD_NAInfo, () => Parent.Pivot?.CD_ADDApplicable ?? ZBool.False);
		}

		protected override void CheckUS_CVD_NA()
		{
			base.CheckUS_CVD_NA();
			Parent.Validation.MatchToProductValidation(Parent.US_CVD_NAInfo, () => Parent.Pivot?.CD_CVDApplicable ?? ZBool.False);
		}

		protected override void CheckUS_ADDDepositValue()
		{
			base.CheckUS_ADDDepositValue();

			CheckIfSpecialDepositValueEnteredForDerivedDutyCalculation(Parent.US_ADDDepositValueInfo, Parent.US_ADDCaseNo);
			CheckIfADD_CVDDetailsEnteredWithoutCaseNumber(Parent.US_ADDDepositValueInfo, Parent.US_ADDCaseNo);
		}

		protected override void CheckUS_CVDDepositValue()
		{
			base.CheckUS_CVDDepositValue();

			CheckIfSpecialDepositValueEnteredForDerivedDutyCalculation(Parent.US_CVDDepositValueInfo, Parent.US_CVDCaseNo);
			CheckIfADD_CVDDetailsEnteredWithoutCaseNumber(Parent.US_CVDDepositValueInfo, Parent.US_CVDCaseNo);
		}

		void CheckIfSpecialDepositValueEnteredForDerivedDutyCalculation(ZPropertyInfo depositValueInfo, ZString caseNumber)
		{
			if (depositValueInfo.Value.IsEmpty && !caseNumber.IsEmpty && InvoiceLine.IsEntrySummaryValidationMode && InvoiceLine.ParentTariffLine != null)
			{
				CusEntryLine lineWithHighestDutyForDerived = GetEntryLineWithHighestDutyRateIfDerived();

				if (lineWithHighestDutyForDerived != null && lineWithHighestDutyForDerived.CL_AdValoremTariff == Parent.JI_Tariff)
				{
					depositValueInfo.AddMessageError(DepositValueShouldBeEnteredForDerivedDutyCalculation);
				}
			}
		}

		public const string DepositValueShouldBeEnteredForDerivedDutyCalculation = "The tariff indicates only the line with the highest duty rate should be sent in 7501 and the value of the component which is subject to ADD or CVD should be declared here.";

		void CheckIfADD_CVDDetailsEnteredWithoutCaseNumber(ZPropertyInfo info, ZString caseNumber)
		{
			if (!info.Value.IsEmpty && caseNumber.IsEmpty && InvoiceLine.IsEntrySummaryValidationMode)
			{
				info.AddMessageError(ADD_CVDDetailsEnteredWithoutCaseNumber);
			}
		}

		public const string ADD_CVDDetailsEnteredWithoutCaseNumber = "Antidumping/Countervailing details cannot be entered without Case Number.";
		#endregion

		protected override void CheckUS_TextileCategoryNo()
		{
			base.CheckUS_TextileCategoryNo();
			ValidateUS_VisaNo();
		}

		protected override void CheckUS_DestinationState()
		{
			base.CheckUS_DestinationState();

			if (InvoiceLine.IsEntrySummaryValidationMode)
			{
				if (Parent.US_DestinationState.IsEmpty && Parent.ImportEntryType != EntryTypeList.Codes.InformalFreeDutiable && !Parent.IsSecondaryTariffLine)
				{
					Parent.US_DestinationStateInfo.AddMessageError(DestinationStateIsRequired);
				}
			}
		}
		internal const string DestinationStateIsRequired = "You should enter the destination state. You can enter here or at the Declaration level.";

		protected override bool IsManifestQtyRequired
		{
			get { return InvoiceLine.IsACE && InvoiceLine.ImportEntryType == EntryTypeList.Codes.ConsumptionFTZ; }
		}

		protected override void CheckUS_UC_NKCountryOfExport()
		{
			base.CheckUS_UC_NKCountryOfExport();

			if (ShouldCheckUS_UC_NKCountryOfExport)
			{
				ValidateUS_VisaNo();
			}
		}

		protected override bool ShouldCheckUS_UC_NKCountryOfExport
		{
			get { return InvoiceLine.IsEntrySummaryValidationMode; }
		}

		protected override bool ShouldCheckUS_UC_NKCountryOfOrigin
		{
			get { return InvoiceLine.IsEntrySummaryOrCargoReleaseValidationMode; }
		}

		protected override void CheckUS_UC_NKCountryOfOrigin()
		{
			base.CheckUS_UC_NKCountryOfOrigin();
			InvoiceLine.Validation.MatchToProductValidation(Parent.US_UC_NKCountryOfOriginInfo, () => Parent.Pivot?.CD_UC_NKCountryOfOrigin ?? ZString.Empty);

			if (ShouldCheckUS_UC_NKCountryOfOrigin)
			{
				ValidateUS_VisaNo();
				ValidateUS_ADDCaseNo();
				ValidateUS_CVDCaseNo();
			}
		}

		bool IsACECargoReleaseValidationMode
		{
			get
			{
				var declaration = Parent.Declaration;
				return declaration != null && declaration.IsACECargoReleaseValidationMode;
			}
		}

		protected override void CheckUS_ZoneStatus()
		{
			base.CheckUS_ZoneStatus();

			if ((InvoiceLine.IsEntrySummaryValidationMode || (IsACECargoReleaseValidationMode)) && !Parent.IsChildLine)
			{
				CheckUS_ZoneStatusCommonForImport();
				if ((Parent.Declaration != null) && Parent.Declaration.IsConsumptionFTZ)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ZoneStatusInfo, ZoneStatusShouldBeEntered);
				}
			}

			ValidateUS_ADDCaseNo();
			ValidateUS_CVDCaseNo();
		}
		internal const string ZoneStatusShouldBeEntered = "Zone Status. Zone Status is mandatory when Entry Type is Consumption Foreign Trade Zone (FTZ)";

		protected override void CheckUS_PrivilegedStatusDate()
		{
			base.CheckUS_PrivilegedStatusDate();

			if (InvoiceLine.IsEntrySummaryValidationMode)
			{
				CheckUS_PrivilegedStatusDateCommonForImport();
				if (Parent.IsConsumptionFTZ && Parent.US_ZoneStatus == ZoneStatusList.Codes.PrivilegedForeign)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PrivilegedStatusDateInfo, PrivilegedStatusDateShouldBeEntered);
				}
			}
		}
		internal const string PrivilegedStatusDateShouldBeEntered = "Privileged Status Date. Privileged Status Date is mandatory when Zone status is Privileged Foreign Merchandise";

		protected override void CheckUS_SPI()
		{
			base.CheckUS_SPI();
			InvoiceLine.Validation.MatchToProductValidation(Parent.US_SPIInfo, () => Parent.Pivot?.CD_SPI ?? ZString.Empty);
		}

		protected override bool ShouldCheckUS_SPI
		{
			get { return InvoiceLine.IsEntrySummaryValidationMode && (!InvoiceLine.IsCombinedLine() || !InvoiceLine.IsChildLine); }
		}

		protected override void CheckUS_SecondarySPI()
		{
			base.CheckUS_SecondarySPI();
			if (ShouldCheckUS_SecondarySPI)
			{
				ValidateUS_CottonFeeExempt();
			}
		}

		protected override bool ShouldCheckUS_SecondarySPI
		{
			get { return InvoiceLine.IsEntrySummaryValidationMode; }
		}

		protected override void CheckUS_SWPMIndicator()
		{
			base.CheckUS_SWPMIndicator();

			if (InvoiceLine.IsEntrySummaryValidationMode)
			{
				if (Parent.IsSetXLine)
				{
					ValidateDataNotNeededForXLine(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit, Parent.US_SWPMIndicatorInfo);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_SWPMIndicatorInfo, Lookups.US_SWPMList);

					if (!Parent.US_SWPMIndicator.IsEmpty &&
						Parent.US_UC_NKCountryOfOrigin != Core.Constants.CountryCodes.China &&
						Parent.US_UC_NKCountryOfOrigin != Core.Constants.CountryCodes.HongKong)
					{
						Parent.US_SWPMIndicatorInfo.AddWarning(SWPMIndicatorForCN_HK);
					}
				}
			}
		}

		internal const string SWPMIndicatorForCN_HK = "SWPM Indicator will be ignored as country of origin is neither Hong Kong or China";

		protected override void CheckUS_SupTariff()
		{
			base.CheckUS_SupTariff();

			JobDeclaration declaration = Parent.Declaration;

			if (!Parent.US_SupTariff.IsEmpty)
			{
				USCTariff supTariff = Parent.ImportSupTariff;

				TariffValidator.Validate(Parent, supTariff, Parent.US_SupTariffInfo);
				TariffValidator.CheckSTNRule(Parent, supTariff, Parent.US_SupTariffInfo, true);
				TariffValidator.Ensure98_99IsEntered(Parent.US_SupTariffInfo);
				TariffValidator.ValidateBasedOnAdditionalTariffNumberIndicator(Parent, supTariff, Parent.US_SupTariffInfo, true);

				InvoiceLine.Validation.MatchToProductValidation(Parent.US_SupTariffInfo, () => InvoiceLine.Pivot?.CI_SupplementalTariff ?? ZString.Empty);

				if (supTariff != null && declaration != null)
				{
					if (supTariff.UE_Tariff.StartsWith("981800") && declaration.US_EntryType != EntryTypeList.Codes.VesselRepair)
					{
						Parent.US_SupTariffInfo.AddMessageError(EntryTypeMustBe05);
					}

					if (supTariff.IsTIBTariff)
					{
						if (declaration.IsTemporaryImportationBond)
						{
							if (Parent.ParentTariffLine != null && Parent.ParentTariffLine.US_SupTariff == Parent.US_SupTariff)
							{
								Parent.US_SupTariffInfo.AddMessageError(No9813TariffsOnSecondaryLines);
							}
						}
						else
						{
							Parent.US_SupTariffInfo.AddMessageError(EntryTypeShouldBeTIB);
						}
					}
					else if (declaration.IsTemporaryImportationBond)
					{
						if (!Parent.IsCombinedLine())
						{
							Parent.US_SupTariffInfo.AddMessageError(TIBEntryMustUseChapter9813Tariffs);
						}
					}

					if (EntryTypeList.IsInformal(declaration.US_EntryType))
					{
						TariffValidator.ValidateAgainstFormalEntryRequirement(declaration, supTariff, Parent.EffectiveDateForDutyRate, Parent.US_SupTariffInfo, Parent.JI_ParentID);
					}
				}
			}
			else if (declaration != null && declaration.IsTemporaryImportationBond && Parent.ParentTariffLine == null)
			{
				Parent.US_SupTariffInfo.AddMessageError(EmptyTIBTariff);
			}

			Parent.Validation.ValidateJI_LinePrice();

			ValidateUS_SPI();
			ValidateUS_VisaNo();
			ValidateUS_UC_NKCountryOfExport();
			ValidateUS_UC_NKCountryOfOrigin();
			ValidateUS_CottonFeeExempt();
			ValidateUS_FDAIndicator();
			ValidateUS_FCCIndicator();
			ValidateUS_DOTIndicator();
			ValidateUS_OverrideSupDuty();
			ValidateUS_ADDCaseNo();
			ValidateUS_CVDCaseNo();
			ValidateUS_98GoodsValue();
		}

		internal const string EntryTypeMustBe05 = "Tariff numbers starting with 9818.00 can only be used when Entry Type is '05' (Vessel Repair)";
		internal const string TIBEntryMustUseChapter9813Tariffs = "TIB entries require each primary tariff line to use a tariff number from Chapter 9813. No other tariff number can be used as primary tariff numbers for this entry type.";
		internal const string No9813TariffsOnSecondaryLines = "9813 numbers should not be entered on secondary tariff lines.";
		internal const string EntryTypeShouldBeTIB = "The tariff number, 9813 should be declared on an entry type, 23(TIB)";
		internal const string EmptyTIBTariff = "This is a TIB entry. Please enter a 9813 tariff number.";
		internal const string CBTPAGoodsBrassiereTariff98201115 = "98201115";

		#region MiscPermitNo

		internal const string MiscPermitNoIsNotRequired = "A Miscellaneous Permit/License Number is not required for this tariff no.";
		internal const string MiscPermitNoRequired = "Misc Permit Number is required for the entered tariff number.";

		#endregion

		protected override void CheckUS_VisaNo()
		{
			base.CheckUS_VisaNo();

			if (InvoiceLine.IsEntrySummaryValidationMode)
			{
				if (Parent.IsSetXLine)
				{
					ValidateDataNotNeededForXLine(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.TextileCLassificationDetails, Parent.US_VisaNoInfo);
				}
				else
				{
					USCTariff tariff = Parent.ImportSupTariff ?? Parent.ImportTariff;
					ZDateTime dutyDate = Parent.EffectiveDateForDutyRate;

					bool isHTHRuleAppliesToTariff = tariff != null && tariff.Applies(TariffRuleList.Codes.HaitiTariffHope, dutyDate);

					if (Parent.US_VisaNo.IsEmpty)
					{
						if (isHTHRuleAppliesToTariff)
						{
							Parent.US_VisaNoInfo.AddMessageError(Constants.VisaNumber.VisaIsMandatoryForEnteredTariff);
						}
					}
					else if (!EntryTypeList.IsQuotaVisa(Parent.Declaration.US_EntryType))
					{
						USCTariff tariffToLook = ((IDutyData)Parent).ParentTariffLine != null ? ((IDutyData)Parent).ParentTariffLine.ImportTariff : tariff;

						if (tariffToLook != null && !tariffToLook.IsEligibleForAGOATextileBenefits(dutyDate) && !isHTHRuleAppliesToTariff)
						{
							Parent.US_VisaNoInfo.AddMessageError(Constants.VisaNumber.VisaNumberNotRequired);
						}
					}

					if (!Parent.US_VisaNo.IsEmpty && Parent.US_SecondarySPI == SecondarySpecProgIndicatorList.Codes.M)
					{
						Parent.US_VisaNoInfo.AddMessageError(Constants.VisaNumber.MayNotBeEnteredWhereSPISecondaryM);
					}
					else
					{
						CheckVisaNumberFormat();

						if (tariff != null)
						{
							ZString effectiveVisaNo = Parent.US_VisaNo_Effective;

							if (!effectiveVisaNo.IsEmpty)
							{
								if (Parent.JI_Tariff.StartsWith("50")   /*Silk*/ && Parent.US_TextileCategoryNo.CompareTo("733") != -1 && Parent.US_TextileCategoryNo.CompareTo("759") != 1 && Parent.US_UC_NKCountryOfExport == Core.Constants.CountryCodes.China)
								{
									Parent.US_VisaNoInfo.AddMessageError(Constants.CountryOfExport.SilkFromChina);
								}
							}
							else
							{
								ZString textileCategoryNo = Parent.US_TextileCategoryNo_Effective;
								ZString countryOfOrigin = Parent.US_UC_NKCountryOfOrigin;
								ZDateTime exportDate = ZDateTime.Empty;
								if (!Parent.US_DateOfExportFromCountryOfOrigin.IsEmpty)
								{
									exportDate = Parent.US_DateOfExportFromCountryOfOrigin;
								}
								else if (Parent.InvoiceHeader != null && !Parent.InvoiceHeader.US_DateOfExport.IsEmpty)
								{
									exportDate = Parent.InvoiceHeader.US_DateOfExport;
								}
								else
								{
									exportDate = Parent.Declaration.JE_ExportDate;
								}

								if (!textileCategoryNo.IsEmpty && !countryOfOrigin.IsEmpty && exportDate.IsValid)
								{
									USCVisa visa = new USCVisa.Loader(Parent.Factory).Load(textileCategoryNo, countryOfOrigin, exportDate);
									if (visa != null && (visa.Tariffs.Count == 0 || visa.Tariffs.Find(Parent.JI_Tariff) != null ||
																				!Parent.HasEmptySupTariff && visa.Tariffs.Find(Parent.US_SupTariff) != null))
									{
										Parent.US_VisaNoInfo.AddMessageError(Constants.VisaNumber.VisaRequiredForTariffOriginAndCategory);
									}
								}
							}
						}

						if (!Parent.US_DateOfExportFromCountryOfOrigin.IsEmpty && Parent.US_VisaNo_Effective.IsEmpty)
						{
							Parent.US_VisaNoInfo.AddMessageError(Constants.VisaNumber.VisaNoRequired);
						}
					}

					ValidateUS_DateOfExportFromCountryOfOrigin();
				}
			}
		}

		void CheckVisaNumberFormat()
		{
			USCTariff tariff = Parent.ImportSupTariff ?? Parent.ImportTariff;
			ZDateTime dutyDate = Parent.EffectiveDateForDutyRate;

			if (!Parent.US_VisaNo.IsEmpty && tariff != null)
			{
				if (tariff.UE_Tariff.StartsWith(CBTPAGoodsBrassiereTariff98201115))
				{
					if (!Regex.IsMatch(Parent.US_VisaNo, @"^\d{1}CB[12]\d{5}$", RegexOptions.IgnoreCase))
					{
						Parent.US_VisaNoInfo.AddMessageError(Constants.VisaNumber.VisaNumberNot9Characters + Constants.VisaNumber.InvalidFormatForBrassiereTariff98201115);
					}
				}
				else
				{
					USCCountry originCountry = Parent.CountryOfOrigin_US;
					bool tariffIsEligibleForAGOATextileBenefitsOnDutyDate = tariff.IsEligibleForAGOATextileBenefits(dutyDate);

					if (tariffIsEligibleForAGOATextileBenefitsOnDutyDate || originCountry != null && originCountry.IssuesStandardisedVisas)
					{
						if (Parent.US_VisaNo.Length != 9)
						{
							Parent.US_VisaNoInfo.AddMessageError(Constants.VisaNumber.VisaNumberNot9Characters);
						}

						if (Parent.US_VisaNo.SubstringSafe(1, 2) != Parent.US_UC_NKCountryOfOrigin)
						{
							Parent.US_VisaNoInfo.AddMessageError(Constants.VisaNumber.SecondThirdLettersVisaNumbersShouldBeCountryOfOrigin);
						}

						if (!Parent.US_VisaNo.SubstringSafe(3).IsNumbersOnlyOrEmpty)
						{
							Parent.US_VisaNoInfo.AddMessageError(Constants.VisaNumber.Last6PositionsShouldBeNumbers);
						}

						if (tariffIsEligibleForAGOATextileBenefitsOnDutyDate)
						{
							string firstCharacter = "";

							switch (tariff.UE_Tariff)
							{
								case "9802008042":
									firstCharacter = "1";
									break;
								case "98191103":
									firstCharacter = "2";
									break;
								case "98191106":
									firstCharacter = "3";
									break;
								case "98191109":
									firstCharacter = "4";
									break;
								case "98191112":
									firstCharacter = "5";
									break;
								case "98191115":
									firstCharacter = "6";
									break;
								case "98191118":
									firstCharacter = "7";
									break;
								case "98191121":
								case "98191124":
									firstCharacter = "8";
									break;
								case "98191127":
									firstCharacter = "9";
									break;
							}

							if (!string.IsNullOrEmpty(firstCharacter))
							{
								if (!Parent.US_VisaNo.StartsWith(firstCharacter))
								{
									Parent.US_VisaNoInfo.AddMessageError(Constants.VisaNumber.FirstCharacterVisaNumberShouldStartWith + firstCharacter);
								}
							}
						}
						else
						{
							ZDateTime exportDate = ZDateTime.Empty;
							if (!Parent.US_DateOfExportFromCountryOfOrigin.IsEmpty)
							{
								exportDate = Parent.US_DateOfExportFromCountryOfOrigin;
							}
							else if (Parent.InvoiceHeader != null && !Parent.InvoiceHeader.US_DateOfExport.IsEmpty)
							{
								exportDate = Parent.InvoiceHeader.US_DateOfExport;
							}
							else if (Parent.Declaration != null)
							{
								exportDate = Parent.Declaration.JE_ExportDate;
							}

							ZString firstCharacter = Parent.US_VisaNo.Left(1);
							if (!firstCharacter.IsNumbersOnlyOrEmpty)
							{
								Parent.US_VisaNoInfo.AddMessageError(Constants.VisaNumber.FirstCharacterVisaNumberShouldBeANumber);
							}
							else if (exportDate.IsValid)
							{
								if (ZInt.ParseSafe(firstCharacter, -1) != exportDate.Date.Year % 10)
								{
									Parent.US_VisaNoInfo.AddMessageError(Constants.VisaNumber.FirstCharacterVisaNumberShouldBeANumber);
								}
							}
						}
					}
				}
			}
		}

		protected override void CheckUS_SupQty1()
		{
			base.CheckUS_SupQty1();

			if (Parent.US_SupQty1 < 0m)
			{
				Parent.US_SupQty1Info.AddMessageError(ValidationConstants.NegativeAmountNotAllowed);
			}
			else
			{
				if (InvoiceLine.IsEntrySummaryValidationMode)
				{
					if (Parent.ImportSupTariff != null)
					{
						IDutyData parentLine = ((IDutyData)Parent).ParentTariffLine;
						new ImportStatQtyValidator().Validate(parentLine, Parent.US_SupQty1Info, Parent.US_SupUQ1,
							delegate
							{ return Parent.ImportSupTariff.RequiresFirstQuantity(); }, QuantityCode.FirstQuantity);
					}
				}
			}

			Parent.Validation.ValidateJI_LinePrice();
			Parent.Validation.ValidateJI_Weight();
			ValidateUS_SupQty2();
			ValidateUS_SupQty3();
		}

		protected override void CheckUS_SupQty2()
		{
			base.CheckUS_SupQty2();

			JobComInvoiceLine invoiceLine = InvoiceLine;
			if (invoiceLine.US_SupQty2 < 0m)
			{
				invoiceLine.US_SupQty2Info.AddMessageError(ValidationConstants.NegativeAmountNotAllowed);
			}

			if (invoiceLine.IsEntrySummaryValidationMode)
			{
				if (Parent.ImportSupTariff != null)
				{
					IDutyData parentLine = ((IDutyData)Parent).ParentTariffLine;
					new ImportStatQtyValidator().Validate(parentLine, Parent.US_SupQty2Info, Parent.US_SupUQ2,
						delegate
						{ return parentLine.ImportTariff.RequiresSecondQuantity(); }, QuantityCode.SecondQuantity);
				}
			}
		}

		protected override void CheckUS_SupQty3()
		{
			base.CheckUS_SupQty3();

			JobComInvoiceLine invoiceLine = InvoiceLine;
			if (invoiceLine.US_SupQty3 < 0m)
			{
				invoiceLine.US_SupQty3Info.AddMessageError(ValidationConstants.NegativeAmountNotAllowed);
			}

			if (invoiceLine.IsEntrySummaryValidationMode)
			{
				if (Parent.ImportSupTariff != null)
				{
					IDutyData parentLine = ((IDutyData)Parent).ParentTariffLine;
					new ImportStatQtyValidator().Validate(parentLine, Parent.US_SupQty3Info, Parent.US_SupUQ3,
						delegate
						{ return parentLine.ImportTariff.RequiresThirdQuantity(); }, QuantityCode.ThirdQuantity);
				}
			}
		}

		protected override void CheckUS_PIRPRulingNo()
		{
			base.CheckUS_PIRPRulingNo();

			if (InvoiceLine.IsEntrySummaryValidationMode)
			{
				if (Parent.IsSetXLine)
				{
					ValidateDataNotNeededForXLine(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit, Parent.US_PIRPRulingNoInfo);
				}
				else
				{
					if (!Parent.US_PIRPRulingNo.IsEmpty && Parent.US_PIRPRulingType.IsEmpty)
					{
						Parent.US_PIRPRulingNoInfo.AddMessageError(PIRPRulingNoEnteredWithoutType);
					}

					if (Parent.Declaration != null
						&& Parent.Declaration.IsTruck
						&& Parent.US_PIRPRulingNo == "INVREQ")
					{
						Parent.US_PIRPRulingNoInfo.AddMessageError(PIRPRulingNumberCannotBeINVREQIfTruck);
					}

					if (!Parent.US_PIRPRulingNo.IsEmpty && Parent.US_PIRPRulingType == PIRPRulingTypeList.Codes.CommercialDescription)
					{
						Parent.US_PIRPRulingNoInfo.AddMessageError(PIRPRulingNumberShouldBeEmptyForCommercialDescr);
					}

					ValidateUS_PIRPRulingType();
				}
			}
		}

		protected override void CheckUS_CottonCertificateNo()
		{
			base.CheckUS_CottonCertificateNo();
			if (InvoiceLine.IsEntrySummaryValidationMode)
			{
				if (Parent.IsSetXLine)
				{
					ValidateDataNotNeededForXLine(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit, Parent.US_CottonCertificateNoInfo);
				}
				new LicenceValidator().ValidateCottonCertificate(Parent.US_CottonCertificateNoInfo, Parent);
			}
		}

		internal const string PIRPRulingNoEnteredWithoutType = "PIRP Ruling type should be accompanied by PIRP Ruling No.";
		internal const string PIRPRulingNumberCannotBeINVREQIfTruck = "'INVREQ' cannot be used for entries crossing the border into the US by truck.";
		internal const string PIRPRulingNumberShouldBeEmptyForCommercialDescr = "PIRP Ruling Number should be empty in the case that RulingType is 'D'.";

		protected override void CheckUS_PIRPRulingType()
		{
			base.CheckUS_PIRPRulingType();

			if (InvoiceLine.IsEntrySummaryValidationMode)
			{
				if (Parent.IsSetXLine)
				{
					ValidateDataNotNeededForXLine(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit, Parent.US_PIRPRulingTypeInfo);
				}
				else
				{
					if (!Parent.US_PIRPRulingType.IsEmpty && Parent.US_PIRPRulingType != PIRPRulingTypeList.Codes.CommercialDescription && Parent.US_PIRPRulingNo.IsEmpty)
					{
						Parent.US_PIRPRulingTypeInfo.AddWarning(PIRPRulingTypeEnteredWithoutNo);
					}

					ListValidation.MessageErrorIfInvalidCode(Parent.US_PIRPRulingTypeInfo, Parent.AddInfoLookups.US_PIRPRulingTypeList);

					ValidateUS_PIRPRulingNo();
				}
			}
		}

		internal const string PIRPRulingTypeEnteredWithoutNo = "You have entered PIRP Ruling type without the number. This type will not be sent in the entry summary message.";

		internal const string DeclarationDestination = "Declaration > Destination";
		internal const string DeclarationFirstArrivalETA = "Declaration > First Arrival ETA";
		internal const string InvoiceHeaderOrDeclarationLocationOfGoods = "Invoice Header > Location of Goods or Declaration > Location of Goods";
		internal const string DeclarationLocationsOfGoods = "Declaration > Location of Goods";

		internal const string TheFollowingMandatoryFieldsNotEntered = "The following mandatory fields have not been entered: ";

		#region Cotton Certificate and Exempt

		protected override void CheckUS_CottonFeeExempt()
		{
			base.CheckUS_CottonFeeExempt();

			if (InvoiceLine.IsEntrySummaryValidationMode)
			{
				if (Parent.IsSetXLine)
				{
					ValidateDataNotNeededForXLine(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit, Parent.US_CottonFeeExemptInfo);
				}
				else
				{
					if (Parent.US_CottonFeeExempt.IsEmpty && Parent.ShouldCottonFeeExemptBeIndicated())
					{
						Parent.US_CottonFeeExemptInfo.AddMessageError(CottonFeeApplicable);
					}

					if (!Parent.US_CottonFeeExempt.IsEmpty)
					{
						if (Parent.ImportTariff != null && !Parent.ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton))
						{
							Parent.US_CottonFeeExemptInfo.AddMessageError(CottonFeeNotApplicable);
						}
					}

					CheckENSMessageCannotBeSent(Parent.US_CottonFeeExemptInfo);
				}
			}
		}

		void CheckENSMessageCannotBeSent(ZPropertyInfo cottonFeeExemptInfo)
		{
			JobComInvoiceLine parentLineOrPassedLine = Parent.ParentTariffLine ?? Parent;
			if (parentLineOrPassedLine.CombinedCottonFeeExemptIndicators.Count > 1)
			{
				cottonFeeExemptInfo.AddMessageError(LineCannotBeSentWhenMixedExemptionIndicators);
			}
		}

		internal const string CottonFeeNotApplicable = "You do not have to indicate whether exempt or not, as cotton fee is not applicable.";
		internal const string CottonFeeApplicable = "You should indicate whether the cotton fee is exempt or not.";
		internal const string LineCannotBeSentWhenMixedExemptionIndicators = "Parent and the secondary lines have different Exemption Indicators. Due to Entry Summary message limitation, this cannot be sent electronically. Please contact Customs to declare this entry manually.";

		#endregion

		protected override void CheckUS_DateOfExportFromCountryOfOrigin()
		{
			base.CheckUS_DateOfExportFromCountryOfOrigin();

			if (InvoiceLine.IsEntrySummaryValidationMode)
			{
				if (Parent.IsSetXLine)
				{
					ValidateDataNotNeededForXLine(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.TextileCLassificationDetails, Parent.US_DateOfExportFromCountryOfOriginInfo);
				}
				else
				{
					if (Parent.US_TextileCategoryNo.IsEmpty && !Parent.US_DateOfExportFromCountryOfOrigin.IsEmpty)
					{
						MandatoryValidation.MessageErrorIfIsEntered(Parent.US_DateOfExportFromCountryOfOriginInfo, "Export Date from C/O if there is no Textile Category No");
					}
					else if (!Parent.US_DateOfExportFromCountryOfOrigin.IsEmpty)
					{
						if (Parent.US_UC_NKCountryOfOrigin == Parent.US_UC_NKCountryOfExport)
						{
							if (Parent.US_DateOfExport.Date != Parent.US_DateOfExportFromCountryOfOrigin.Date)
							{
								Parent.US_DateOfExportFromCountryOfOriginInfo.AddMessageError(OriginAndExportDateMustBeTheSame);
							}
						}
						else if (Parent.US_UC_NKCountryOfOrigin != Parent.US_UC_NKCountryOfExport)
						{
							if (Parent.US_DateOfExportFromCountryOfOrigin > Parent.US_DateOfExport)
							{
								Parent.US_DateOfExportFromCountryOfOriginInfo.AddMessageError(TextileExportDateNotLater);
							}
						}
					}

					ValidateUS_VisaNo();
				}
			}
		}
		internal const string OriginAndExportDateMustBeTheSame = "If Country of Origin and Export are the same, Textile Export Date from C/O must be the same as Date of Export.";
		internal const string TextileExportDateNotLater = "Textile Export Date from C/O cannot be later than Export Date.";

		protected override void CheckUS_ADDDepositRateIndicator()
		{
			base.CheckUS_ADDDepositRateIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ADDDepositRateIndicatorInfo, Parent.Lookups.AntidumpingDutyDepositRates);
			if (Parent.AntidumpingDutyCase != null && Parent.Lookups.AntidumpingDutyDepositRates.Count > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ADDDepositRateIndicatorInfo, "rate indicator");
			}
		}

		protected override void CheckUS_CVDDepositRateIndicator()
		{
			base.CheckUS_CVDDepositRateIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_CVDDepositRateIndicatorInfo, Parent.Lookups.CountervailingDutyDepositRates);
			if (Parent.CountervailingDutyCase != null && Parent.Lookups.CountervailingDutyDepositRates.Count > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CVDDepositRateIndicatorInfo, "rate indicator");
			}
		}

		protected override void CheckUS_LumberExportPrice()
		{
			base.CheckUS_LumberExportPrice();

			if (InvoiceLine.IsEntrySummaryValidationMode)
			{
				if (Parent.IsSetXLine)
				{
					ValidateDataNotNeededForXLine(Constants.OGA.SoftwoodLumber, Parent.US_LumberExportPriceInfo);
				}
				else
				{
					if (Parent.US_LumberExportPrice.IsEmpty && Parent.US_LumberImporterDeclaration == YesNoDefaultList.Codes.Yes)
					{
						Parent.US_LumberExportPriceInfo.AddMessageError(SoftWoodLumberExportPriceMandatory);
					}

					if (InvoiceLine.US_UC_NKCountryOfExport == Core.Constants.CountryCodes.Canada
				&& InvoiceLine.US_LumberImporterDeclaration == YesNoDefaultList.Codes.Yes
				&& Parent.US_LumberExportPrice.IsEmpty
				&& InvoiceLine.IsSoftwoodLumberSection804FarmBillRequirement)
					{
						Parent.US_LumberExportPriceInfo.AddMessageError(SoftwoodLumberExportPriceRequired);
					}
				}
			}
		}
		internal const string SoftwoodLumberExportPriceRequired = "Export Price is mandatory when goods exported from Canada are subject to the Softwood Lumber Act of 2008.";
		internal const string SoftWoodLumberExportPriceMandatory = "Export Price is mandatory when Declare for SLA 2008(Y) is entered.";

		protected override void CheckUS_LumberExportCharges()
		{
			base.CheckUS_LumberExportCharges();

			if (InvoiceLine.IsEntrySummaryValidationMode && Parent.IsSetXLine)
			{
				ValidateDataNotNeededForXLine(Constants.OGA.SoftwoodLumber, Parent.US_LumberExportChargesInfo);
			}
		}

		protected override void CheckUS_LumberImporterDeclaration()
		{
			base.CheckUS_LumberImporterDeclaration();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_LumberImporterDeclarationInfo, Lookups.US_YesNoList);
			if (InvoiceLine.IsEntrySummaryValidationMode)
			{
				if (Parent.US_LumberImporterDeclaration.IsEmpty && !Parent.US_LumberExportPrice.IsEmpty)
				{
					Parent.US_LumberImporterDeclarationInfo.AddMessageError(ExportPriceSoftwoodLumberRequired);
				}
				if (InvoiceLine.IsSoftwoodLumberSection804FarmBillRequirement)
				{
					if (Parent.IsSetXLine)
					{
						ValidateDataNotNeededForXLine(Constants.OGA.SoftwoodLumber, Parent.US_LumberImporterDeclarationInfo);
					}
					else
					{
						if (Parent.US_LumberImporterDeclaration.IsEmpty)
						{
							Parent.US_LumberImporterDeclarationInfo.AddMessageError(SoftwoodLumberImporterDeclarationRequired);
						}
					}
				}
			}
		}
		internal const string SoftwoodLumberImporterDeclarationRequired = "Please indicate whether the goods are subject to the Softwood Lumber Act of 2008.";
		internal const string ExportPriceSoftwoodLumberRequired = "Lumber Declaration is mandatory when the export price is entered.";

		#region OGA Indicators Validation

		protected override void CheckUS_FDAIndicator()
		{
			base.CheckUS_FDAIndicator();

			var declaration = Parent.Declaration;
			if (declaration != null && !declaration.US_DomesticCargo)
			{
				if (InvoiceLine.IsACEFDARelevant)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_FDAIndicatorInfo, Parent.AddInfoLookups.US_OGAIndicatorList);

					if (InvoiceLine.IsSetXLine)
					{
						ValidateDataNotNeededForXLine("FDA", InvoiceLine.US_FDAIndicatorInfo);
					}
					else if (IsPGAValidationApplicable())
					{
						AgencyRequirementsValidator.ValidatePGA(Parent.US_FDAIndicatorInfo, "FDA", true, InvoiceLine.PGARequirementIndicator.HasACEFDARequirement);
						AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.US_FDAIndicatorInfo, "FDA", Parent.ACE_FDALines.Cast<IPGADataCorrection>());
						AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.US_FDAIndicatorInfo, InvoiceLine.PGARequirementIndicator.RequireACEFDA);
						AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.FDA, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_FDAIndicatorInfo);
					}
				}

				ValidateUS_FDADisclaimReason();
			}
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.FDA, InvoiceLine.OGAAgencyRequirements);
		}

		protected override void CheckUS_FDADisclaimReason()
		{
			base.CheckUS_FDADisclaimReason();

			var declaration = Parent.Declaration;
			if (declaration != null && !declaration.US_DomesticCargo)
			{
				if (InvoiceLine.IsACEFDARelevant)
				{
					AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.US_FDADisclaimReasonInfo, Parent.US_FDAIndicator, Parent.AddInfoLookups.FDADisclaimReasonList);
				}

				ValidateUS_FDAIndicator();
			}
			AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.FDA, InvoiceLine.OGAAgencyRequirements);
		}

		protected override void CheckUS_DOTIndicator()
		{
			base.CheckUS_DOTIndicator();
			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine("DOT", InvoiceLine.US_DOTIndicatorInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_DOTIndicatorInfo, Parent.AddInfoLookups.US_OGAIndicatorList);

				if (!InvoiceLine.IsACECargoCertificationMode && InvoiceLine.IsEntrySummaryOrCargoReleaseValidationMode && !Parent.IsSecondaryTariffLine)
				{
					bool doesRequireDOT = InvoiceLine.PGARequirementIndicator.RequireDOT;
					bool mayRequireDOT = InvoiceLine.PGARequirementIndicator.MayRequireDOT;

					CheckDOTIndicatorForTariff(doesRequireDOT, mayRequireDOT);
				}
			}
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.DOT, InvoiceLine.OGAAgencyRequirements);
		}

		void CheckDOTIndicatorForTariff(bool doesRequireDOT, bool mayRequireDOT)
		{
			if (doesRequireDOT || mayRequireDOT)
			{
				if (Parent.US_DOTIndicator.IsEmpty)
				{
					Parent.US_DOTIndicatorInfo.AddMessageError(Constants.DOT.DOTRequiredButBlank);
				}
			}

			if (doesRequireDOT)
			{
				if (OGAIndicatorList.IsToBeDisclaimed(Parent.US_DOTIndicator))
				{
					Parent.US_DOTIndicatorInfo.AddMessageError(Constants.DOT.DOTRequiredButDisclaimed);
				}
			}

			if (!doesRequireDOT && !mayRequireDOT)
			{
				if (Parent.IsDOTDeclared)
				{
					Parent.US_DOTIndicatorInfo.AddWarning(Constants.DOT.DOTNotRequired);
				}
				else if (OGAIndicatorList.IsToBeDisclaimed(Parent.US_DOTIndicator))
				{
					Parent.US_DOTIndicatorInfo.AddMessageError(Constants.DOT.DOTDisclaimedInvalid);
				}
			}

			if (!Parent.US_DOTIndicatorInfo.HasMessageErrors())
			{
				if (Parent.IsDOTDeclared)
				{
					if (Parent.DOTs.Count == 0)
					{
						Parent.US_DOTIndicatorInfo.AddMessageError(Constants.DOT.DOTLineRequired);
					}
				}
				else
				{
					if (Parent.DOTs.Count > 0)
					{
						Parent.US_DOTIndicatorInfo.AddMessageError(Constants.DOT.DOTLineNotAllowed);
					}
				}
			}
		}

		protected override void CheckUS_FSISInd()
		{
			base.CheckUS_FSISInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_FSISIndInfo, Parent.AddInfoLookups.US_OGAIndicatorList);

			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine("FSIS", InvoiceLine.US_FSISIndInfo);
			}
			else if (InvoiceLine.IsACE && IsPGAValidationApplicable())
			{
				var isFSISEffective = ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.PGAFSIS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);
				var hasFSISRequirement = InvoiceLine.PGARequirementIndicator.HasFSISRequirement;
				AgencyRequirementsValidator.ValidatePGA(Parent.US_FSISIndInfo, "FSIS", isFSISEffective, hasFSISRequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.US_FSISIndInfo, "FSIS", Parent.FSISLines.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.US_FSISIndInfo, InvoiceLine.PGARequirementIndicator.RequireFSIS);
				AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.FSIS, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_FSISIndInfo);

				if (!isFSISEffective && Parent.US_FSISInd.IsEmpty && hasFSISRequirement)
				{
					Parent.US_FSISIndInfo.AddWarning(AgencyRequirementsValidator.RequirementConstants.PGA.PGASubmittedBy);
				}
			}

			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.FSIS, InvoiceLine.OGAAgencyRequirements);
			ValidateUS_FSISDisclaimReason();
		}

		protected override void CheckUS_OMCInd()
		{
			base.CheckUS_OMCInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_OMCIndInfo, Parent.AddInfoLookups.US_OGAIndicatorList);
			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine("OMC", InvoiceLine.US_OMCIndInfo);
			}
			else if (IsPGAValidationApplicable())
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.US_OMCIndInfo, "OMC", ZZCustomsFunctionality.IsOMCEffective, InvoiceLine.PGARequirementIndicator.HasOMCRequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.US_OMCIndInfo, "OMC", Parent.OMCHeaders.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.US_OMCIndInfo, InvoiceLine.PGARequirementIndicator.RequireOMC);
				AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.OMC, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_OMCIndInfo);
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_OMCIndInfo);
			}
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.OMC, InvoiceLine.OGAAgencyRequirements);
			ValidateUS_OMCDisclaimReason();
		}

		protected override void CheckUS_OMCDisclaimReason()
		{
			base.CheckUS_OMCDisclaimReason();
			if (ZZCustomsFunctionality.IsOMCEffective)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.US_OMCDisclaimReasonInfo, Parent.US_OMCInd, Parent.AddInfoLookups.OMCDisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.OMC, InvoiceLine.OGAAgencyRequirements);
			}
		}

		protected override void CheckUS_FSISDisclaimReason()
		{
			base.CheckUS_FSISDisclaimReason();
			if (ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.PGAFSIS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today))
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.US_FSISDisclaimReasonInfo, Parent.US_FSISInd, Parent.AddInfoLookups.FSISDisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.FSIS, InvoiceLine.OGAAgencyRequirements);
			}

			ValidateUS_FSISInd();
		}

		protected override void CheckUS_ODSInd()
		{
			base.CheckUS_ODSInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ODSIndInfo, Parent.AddInfoLookups.US_OGAIndicatorList);

			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine("ODS", InvoiceLine.US_ODSIndInfo);
			}
			else if (IsPGAValidationApplicable())
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.US_ODSIndInfo, "ODS", true, InvoiceLine.PGARequirementIndicator.HasODSRequirement);
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.US_ODSIndInfo, InvoiceLine.PGARequirementIndicator.RequireODS);
				AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.ODS, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_ODSIndInfo);
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_ODSIndInfo);
			}
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.ODS, InvoiceLine.OGAAgencyRequirements);
			ValidateUS_ODSDisclaimReason();
		}

		protected override void CheckUS_ODSDisclaimReason()
		{
			base.CheckUS_ODSDisclaimReason();
			AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.US_ODSDisclaimReasonInfo, Parent.US_ODSInd, Parent.AddInfoLookups.ODSDisclaimReasonList);
			AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.ODS, InvoiceLine.OGAAgencyRequirements);

			ValidateUS_ODSInd();
		}

		protected override void CheckUS_TSCAInd()
		{
			base.CheckUS_TSCAInd();

			ListValidation.MessageErrorIfInvalidCode(Parent.US_TSCAIndInfo, Parent.AddInfoLookups.US_OGAIndicatorList);
			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine("TSCA", InvoiceLine.US_TSCAIndInfo);
			}
			else if (IsPGAValidationApplicable())
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.US_TSCAIndInfo, "TSCA", true, InvoiceLine.PGARequirementIndicator.HasTSCARequirement);
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.US_TSCAIndInfo, InvoiceLine.PGARequirementIndicator.RequireTSCA);
				AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.TSCA, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_TSCAIndInfo);
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_TSCAIndInfo);
			}
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.TSCA, InvoiceLine.OGAAgencyRequirements);
		}

		protected override void CheckUS_TSCADisclaimReason()
		{
			base.CheckUS_TSCADisclaimReason();
			AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.US_TSCADisclaimReasonInfo, Parent.US_TSCAInd, Parent.AddInfoLookups.TSCADisclaimReasonList);
			AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.TSCA, InvoiceLine.OGAAgencyRequirements);

			ValidateUS_TSCAInd();
		}
		protected override void CheckUS_ATFInd()
		{
			base.CheckUS_ATFInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ATFIndInfo, Parent.AddInfoLookups.US_OGAIndicatorWithoutDisclaimerList);

			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine("ATF", InvoiceLine.US_ATFIndInfo);
			}
			else if (IsPGAValidationApplicable())
			{
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.US_ATFIndInfo, "ATF", Parent.ATFLines.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.ATF, InvoiceLine.OGAAgencyRequirements);
				AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.ATF, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_ATFIndInfo);
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_ATFIndInfo);
			}
		}

		protected override void CheckUS_CPSCInd()
		{
			base.CheckUS_CPSCInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_CPSCIndInfo, Parent.AddInfoLookups.US_OGAIndicatorList);
			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine("CPSC", InvoiceLine.US_CPSCIndInfo);
			}
			else if (IsPGAValidationApplicable())
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.US_CPSCIndInfo, "CPSC", ZZCustomsFunctionality.IsCPSCEffective, InvoiceLine.PGARequirementIndicator.HasCPSCRequirement);
				if (Parent.US_CPSCDisclaimReason != PGADisclaimReasonList.Codes.A)
				{
					AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.US_CPSCIndInfo, "CPSC", Parent.CPSCHeaders.Cast<IPGADataCorrection>());
				}
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.US_CPSCIndInfo, InvoiceLine.PGARequirementIndicator.RequireCPSC);
				AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.CPSC, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_CPSCIndInfo);
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_CPSCIndInfo);
			}
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.CPSC, InvoiceLine.OGAAgencyRequirements);
			ValidateUS_CPSCDisclaimReason();
		}

		protected override void CheckUS_CPSCDisclaimReason()
		{
			base.CheckUS_CPSCDisclaimReason();
			if (ZZCustomsFunctionality.IsCPSCEffective)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.US_CPSCDisclaimReasonInfo, Parent.US_CPSCInd, Parent.AddInfoLookups.CPSCDisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.CPSC, InvoiceLine.OGAAgencyRequirements);
			}

			ValidateUS_CPSCInd();
		}

		protected override void CheckUS_AMSInd()
		{
			base.CheckUS_AMSInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_AMSIndInfo, Parent.AddInfoLookups.US_OGAIndicatorList);
			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine("AMS", InvoiceLine.US_AMSIndInfo);
			}
			else if (IsPGAValidationApplicable())
			{
				var hasEGG = InvoiceLine.PGARequirementIndicator.HasAMSEGGRequirement;
				var hasORD = InvoiceLine.PGARequirementIndicator.HasAMSORDRequirement;
				var hasPNT = InvoiceLine.PGARequirementIndicator.HasAMSPNTRequirement;

				if (hasEGG)
				{
					AgencyRequirementsValidator.ValidatePGA(Parent.US_AMSIndInfo, "AMS", Parent.IsAMSEGGEffective, hasEGG);
				}
				else if (hasORD)
				{
					AgencyRequirementsValidator.ValidatePGA(Parent.US_AMSIndInfo, "AMS", true, hasORD);
				}
				else if (hasPNT)
				{
					AgencyRequirementsValidator.ValidatePGA(Parent.US_AMSIndInfo, "AMS", Parent.IsAMSPNTEffective, hasPNT);
				}
				else
				{
					AgencyRequirementsValidator.ValidatePGA(Parent.US_AMSIndInfo, "AMS", false, false);
				}

				if (!Parent.HasAMSNOP)
				{
					AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.US_AMSIndInfo, "AMS", Parent.AMSLines.Cast<IPGADataCorrection>());
				}
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.US_AMSIndInfo, InvoiceLine.PGARequirementIndicator.RequireAMS);
				AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.AMS, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_AMSIndInfo);
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_AMSIndInfo);
			}
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.AMS, InvoiceLine.OGAAgencyRequirements);
			ValidateUS_AMSDisclaimReason();
		}
		internal const string AMSDisclaimedNotRequired = "This tariff is not an exemption from AMS program under {0} disclaimer. Please go to Maintain -> Customs -> Rules and check '{1}' rule for tariffs allowed.";

		protected override void CheckUS_AMSDisclaimReason()
		{
			base.CheckUS_AMSDisclaimReason();
			AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.US_AMSDisclaimReasonInfo, Parent.US_AMSInd, Parent.AddInfoLookups.AMSDisclaimReasonList);
			AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.AMS, InvoiceLine.OGAAgencyRequirements);

			ValidateUS_AMSInd();
			ValidateUS_AMSDisclaimProgram();
		}

		protected override void CheckUS_NOPInd()
		{
			base.CheckUS_NOPInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NOPIndInfo, Parent.AddInfoLookups.US_OGAIndicatorList);

			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine("NOP", InvoiceLine.US_NOPIndInfo);
			}
			else if (IsPGAValidationApplicable())
			{
				var isNOPEffective = InvoiceLine.IsAMSNOPEffective;
				AgencyRequirementsValidator.ValidatePGA(Parent.US_NOPIndInfo, "NOP", isNOPEffective, InvoiceLine.PGARequirementIndicator.HasNOPRequirement);

				if (Parent.HasAMSNOP)
				{
					AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.US_NOPIndInfo, "NOP", Parent.AMSLines.Cast<IPGADataCorrection>());
				}

				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.US_NOPIndInfo, InvoiceLine.PGARequirementIndicator.RequireNOP);
				AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.NOP, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_NOPIndInfo);
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_NOPIndInfo);
			}
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.NOP, InvoiceLine.OGAAgencyRequirements);
			ValidateUS_NOPDisclaimReason();
		}

		protected override void CheckUS_NOPDisclaimReason()
		{
			base.CheckUS_NOPDisclaimReason();
			AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.US_NOPDisclaimReasonInfo, Parent.US_NOPInd, Parent.AddInfoLookups.NOPDisclaimReasonList);
			AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.NOP, InvoiceLine.OGAAgencyRequirements);

			ValidateUS_NOPInd();
			ValidateUS_AMSDisclaimProgram();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		protected override void CheckUS_AMSDisclaimProgram()
		{
			base.CheckUS_AMSDisclaimProgram();
			if (OGAIndicatorList.IsToBeDisclaimed(Parent.US_AMSInd) && Parent.US_AMSDisclaimProgram != AMSProgramList.Codes.EG1)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_AMSDisclaimProgramInfo, Parent.AddInfoLookups.AMSDisclaimProgramList);
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_AMSDisclaimProgramInfo);

				if (Parent.US_AMSDisclaimProgram == AMSProgramList.Codes.MO8)
				{
					var tariffs = new USCTariff[] { Parent.ImportTariffForPGA, Parent.ImportSupTariff };
					var programCode = Parent.US_AMSDisclaimProgram;
					var tariffRuleCode = TariffRuleList.Codes.HTSExemptFromAMSMO8ProgramRequirement;
					var dateForValidation = InvoiceLine.EffectiveDateForDutyRate;
					var tariffExempt = tariffs.FirstOrDefault(x => IsTariffExemptFromAMS(x, tariffRuleCode, dateForValidation));
					if (tariffExempt == null)
					{
						Parent.US_AMSDisclaimProgramInfo.AddMessageError(ZString.Format(AMSDisclaimedNotRequired, programCode, tariffRuleCode));
					}

					if (Parent.US_AMSDisclaimReason != PGADisclaimReasonList.Codes.B)
					{
						Parent.US_AMSDisclaimProgramInfo.AddMessageError(AMSDisclaimedReasonShouldBeB);
					}
				}
			}
		}
		public const string AMSDisclaimedReasonShouldBeB = "For the selected program code, 'B' is the only valid disclaim reason code.";

		ZBool IsTariffExemptFromAMS(USCTariff tariff, ZString tariffRuleCode, ZDateTime dateForDutyCalculation)
		{
			if (tariff != null && !tariffRuleCode.IsEmpty)
			{
				return tariff.Factory.GetCachedValue<ZBool>("IsTariffExemptFromAMS" + tariff.UE_Tariff + tariffRuleCode + dateForDutyCalculation.ToLongTimeString(), () =>
				{
					return tariff.Applies(tariffRuleCode, dateForDutyCalculation);
				});
			}

			return false;
		}

		protected override void CheckUS_VNEInd()
		{
			base.CheckUS_VNEInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_VNEIndInfo, Parent.AddInfoLookups.US_OGAIndicatorList);

			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine("VNE", InvoiceLine.US_VNEIndInfo);
			}
			else if (IsPGAValidationApplicable())
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.US_VNEIndInfo, "VNE", true, InvoiceLine.PGARequirementIndicator.HasVNERequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.US_VNEIndInfo, "VNE", Parent.VehicleLines.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.US_VNEIndInfo, InvoiceLine.PGARequirementIndicator.RequireVNE);
				AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.VNE, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_VNEIndInfo);
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_VNEIndInfo);
			}
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.VNE, InvoiceLine.OGAAgencyRequirements);
			ValidateUS_VNEDisclaimReason();
		}

		protected override void CheckUS_VNEDisclaimReason()
		{
			base.CheckUS_VNEDisclaimReason();
			AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.US_VNEDisclaimReasonInfo, Parent.US_VNEInd, Parent.AddInfoLookups.VNEDisclaimReasonList);
			AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.VNE, InvoiceLine.OGAAgencyRequirements);

			ValidateUS_VNEInd();
		}

		protected override void CheckUS_PSTIndicator()
		{
			base.CheckUS_PSTIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_PSTIndicatorInfo, Parent.AddInfoLookups.US_OGAIndicatorList);

			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine("PST", InvoiceLine.US_PSTIndicatorInfo);
			}
			else if (IsPGAValidationApplicable())
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.US_PSTIndicatorInfo, "PST", true, InvoiceLine.PGARequirementIndicator.HasPSTRequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.US_PSTIndicatorInfo, "PST", Parent.PSTLines.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.US_PSTIndicatorInfo, InvoiceLine.PGARequirementIndicator.RequirePST);
				AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.PST, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_PSTIndicatorInfo);
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_PSTIndicatorInfo);
			}
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.PST, InvoiceLine.OGAAgencyRequirements);
			ValidateUS_PSTDisclaimReason();
		}

		protected override void CheckUS_PSTDisclaimReason()
		{
			base.CheckUS_PSTDisclaimReason();
			AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.US_PSTDisclaimReasonInfo, Parent.US_PSTIndicator, Parent.AddInfoLookups.PSTDisclaimReasonList);
			AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.PST, InvoiceLine.OGAAgencyRequirements);

			ValidateUS_PSTIndicator();
		}

		protected override void CheckUS_PSTDisclaimProgram()
		{
			base.CheckUS_PSTDisclaimProgram();
			if (OGAIndicatorList.IsToBeDisclaimed(Parent.US_PSTIndicator))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_PSTDisclaimProgramInfo, Parent.AddInfoLookups.PSTDisclaimProgramList);
			}
		}

		protected override void CheckUS_HFCInd()
		{
			base.CheckUS_HFCInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_HFCIndInfo, Parent.AddInfoLookups.US_OGAIndicatorList);

			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine(GovernmentAgencyProgramCodeList.Codes.HFC, InvoiceLine.US_HFCIndInfo);
			}
			else if (IsPGAValidationApplicable())
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.US_HFCIndInfo, GovernmentAgencyProgramCodeList.Codes.HFC, true, InvoiceLine.PGARequirementIndicator.HasHFCRequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.US_HFCIndInfo, GovernmentAgencyProgramCodeList.Codes.HFC, Parent.USHFCHeaders.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.US_HFCIndInfo, InvoiceLine.PGARequirementIndicator.RequireHFC);
				AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.HFC, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_HFCIndInfo);
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_HFCIndInfo);
			}
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.HFC, InvoiceLine.OGAAgencyRequirements);
			ValidateUS_HFCDisclaimReason();
		}

		protected override void CheckUS_HFCDisclaimReason()
		{
			base.CheckUS_HFCDisclaimReason();
			AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.US_HFCDisclaimReasonInfo, Parent.US_HFCInd, Parent.AddInfoLookups.HFCDisclaimReasonList);
			AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.HFC, InvoiceLine.OGAAgencyRequirements);

			ValidateUS_HFCInd();
		}

		protected override void CheckUS_APHISInd()
		{
			base.CheckUS_APHISInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_APHISIndInfo, Parent.AddInfoLookups.US_OGAIndicatorList);

			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine("APHIS", InvoiceLine.US_APHISIndInfo);
			}
			else if (IsPGAValidationApplicable())
			{
				if (InvoiceLine.PGARequirementIndicator.MayRequireAPHISNoDisclaimRequired)
				{
					if (Parent.US_APHISInd.IsEmpty)
					{
						Parent.US_APHISIndInfo.AddWarning(APHISDataMayBeRequired);
					}
				}
				else
				{
					AgencyRequirementsValidator.ValidatePGA(Parent.US_APHISIndInfo, "APHIS", true, InvoiceLine.PGARequirementIndicator.HasAPHISRequirement);
				}
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.US_APHISIndInfo, "APHIS", Parent.APHISHeaders.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.US_APHISIndInfo, InvoiceLine.PGARequirementIndicator.RequireAPHIS);
				AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.APHIS, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_APHISIndInfo);
				ValidateUS_APHISDisclaimReason();
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_APHISIndInfo);
			}
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.APHIS, InvoiceLine.OGAAgencyRequirements);
		}
		internal const string APHISDataMayBeRequired = "APHIS Data May be required, no disclaim is required if APHIS does not apply";

		protected override void CheckUS_APHISDisclaimReason()
		{
			base.CheckUS_APHISDisclaimReason();
			if (IsACECargoReleaseValidationMode)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.US_APHISDisclaimReasonInfo, Parent.US_APHISInd, Parent.AddInfoLookups.APHISDisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.APHIS, InvoiceLine.OGAAgencyRequirements);
			}

			ValidateUS_APHISInd();
		}

		protected override void CheckUS_NMFSCOAInd()
		{
			base.CheckUS_NMFSCOAInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NMFSCOAIndInfo, Parent.AddInfoLookups.US_OGAIndicatorListWithDisclaimerList);
			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine("NMFS COA", InvoiceLine.US_NMFSCOAIndInfo);
			}
			else if (IsPGAValidationApplicable())
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.US_NMFSCOAIndInfo, "COA", ZZCustomsFunctionality.IsNMFSCOAACTIVEEffective, InvoiceLine.PGARequirementIndicator.RequireNMFSCOA);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.US_NMFSCOAIndInfo, GovernmentAgencyProgramCodeList.Codes.COA, Parent.NMFSCOALines.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.COA, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_NMFSCOAIndInfo);
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_NMFSCOAIndInfo);
			}
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.COA, InvoiceLine.OGAAgencyRequirements);
		}

		protected override void CheckUS_NMFS370Ind()
		{
			base.CheckUS_NMFS370Ind();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NMFS370IndInfo, Parent.AddInfoLookups.US_OGAIndicatorList);
			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine("NMFS 370", InvoiceLine.US_NMFS370IndInfo);
			}
			else if (IsPGAValidationApplicable())
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.US_NMFS370IndInfo, GovernmentAgencyProgramCodeList.Codes._370, true, InvoiceLine.PGARequirementIndicator.HasNMFS370Requirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.US_NMFS370IndInfo, GovernmentAgencyProgramCodeList.Codes._370, Parent.NMFS370Lines.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.US_NMFS370IndInfo, InvoiceLine.PGARequirementIndicator.RequireNMFS370);
				AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes._370, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_NMFS370IndInfo);
				ValidateUS_NMFS370DisclaimReason();
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_NMFS370IndInfo);
			}
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes._370, InvoiceLine.OGAAgencyRequirements);
		}

		protected override void CheckUS_NMFS370DisclaimReason()
		{
			base.CheckUS_NMFS370DisclaimReason();
			if (IsACECargoReleaseValidationMode)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.US_NMFS370DisclaimReasonInfo, Parent.US_NMFS370Ind, Parent.AddInfoLookups.NMFS370DisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes._370, InvoiceLine.OGAAgencyRequirements);
			}

			ValidateUS_NMFS370Ind();
		}

		protected override void CheckUS_NMFSAMRInd()
		{
			base.CheckUS_NMFSAMRInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NMFSAMRIndInfo, Parent.AddInfoLookups.US_OGAIndicatorList);

			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine("NMFS AMR", InvoiceLine.US_NMFSAMRIndInfo);
			}
			else if (IsPGAValidationApplicable())
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.US_NMFSAMRIndInfo, GovernmentAgencyProgramCodeList.Codes.AMR, true, InvoiceLine.PGARequirementIndicator.HasNMFSAMRRequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.US_NMFSAMRIndInfo, GovernmentAgencyProgramCodeList.Codes.AMR, Parent.NMFSAMRLines.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.US_NMFSAMRIndInfo, InvoiceLine.PGARequirementIndicator.RequireNMFSAMR);
				AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.AMR, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_NMFSAMRIndInfo);
				ValidateUS_NMFSAMRDisclaimReason();
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_NMFSAMRIndInfo);
			}
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.AMR, InvoiceLine.OGAAgencyRequirements);
		}

		protected override void CheckUS_NMFSAMRDisclaimReason()
		{
			base.CheckUS_NMFSAMRDisclaimReason();
			if (IsACECargoReleaseValidationMode)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.US_NMFSAMRDisclaimReasonInfo, Parent.US_NMFSAMRInd, Parent.AddInfoLookups.NMFSAMRDisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.AMR, InvoiceLine.OGAAgencyRequirements);
			}

			ValidateUS_NMFSAMRInd();
		}

		protected override void CheckUS_NMFSHMSInd()
		{
			base.CheckUS_NMFSHMSInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NMFSHMSIndInfo, Parent.AddInfoLookups.US_OGAIndicatorList);
			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine("NMFS HMS", InvoiceLine.US_NMFSHMSIndInfo);
			}
			else if (IsPGAValidationApplicable())
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.US_NMFSHMSIndInfo, GovernmentAgencyProgramCodeList.Codes.HMS, true, InvoiceLine.PGARequirementIndicator.HasNMFSHMSRequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.US_NMFSHMSIndInfo, GovernmentAgencyProgramCodeList.Codes.HMS, Parent.NMFSHMSLines.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.US_NMFSHMSIndInfo, InvoiceLine.PGARequirementIndicator.RequireNMFSHMS);
				AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.HMS, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_NMFSHMSIndInfo);
				ValidateUS_NMFSHMSDisclaimReason();
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_NMFSHMSIndInfo);
			}
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.HMS, InvoiceLine.OGAAgencyRequirements);
		}

		protected override void CheckUS_NMFSHMSDisclaimReason()
		{
			base.CheckUS_NMFSHMSDisclaimReason();
			if (IsACECargoReleaseValidationMode)
			{
				AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.US_NMFSHMSDisclaimReasonInfo, Parent.US_NMFSHMSInd, Parent.AddInfoLookups.NMFSHMSDisclaimReasonList);
				AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.HMS, InvoiceLine.OGAAgencyRequirements);
			}

			ValidateUS_NMFSHMSInd();
		}
		protected override void CheckUS_NHTSAIndicator()
		{
			base.CheckUS_NHTSAIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NHTSAIndicatorInfo, Parent.AddInfoLookups.US_OGAIndicatorList);

			if (InvoiceLine.IsNHTSARelevant)
			{
				if (InvoiceLine.IsSetXLine)
				{
					ValidateDataNotNeededForXLine("NHTSA", InvoiceLine.US_NHTSAIndicatorInfo);
				}
				else if (IsPGAValidationApplicable())
				{
					AgencyRequirementsValidator.ValidatePGA(Parent.US_NHTSAIndicatorInfo, "NHTSA", true, InvoiceLine.PGARequirementIndicator.HasNHTSARequirement);
					AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.US_NHTSAIndicatorInfo, "NHTSA", Parent.NHTSALines.Cast<IPGADataCorrection>());
					AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.US_NHTSAIndicatorInfo, InvoiceLine.PGARequirementIndicator.RequireNHTSA);
					AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.NHTSA, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_NHTSAIndicatorInfo);
				}
				AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.NHTSA, InvoiceLine.OGAAgencyRequirements);
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_NHTSAIndicatorInfo);
			}
			ValidateUS_NHTDisclaimReason();
		}

		protected override void CheckUS_NHTDisclaimReason()
		{
			base.CheckUS_NHTDisclaimReason();
			AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.US_NHTDisclaimReasonInfo, Parent.US_NHTSAIndicator, Parent.AddInfoLookups.NHTSADisclaimReasonList);
			AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.NHTSA, InvoiceLine.OGAAgencyRequirements);

			ValidateUS_NHTSAIndicator();
		}

		internal void ValidatePGANotApplicable(ZPropertyInfo propertyInfo)
		{
			var value = (ZString)propertyInfo.Value;
			var declaration = Parent.Declaration;

			if (declaration != null && declaration.IsACSCargoCertificationMode && OGAIndicatorList.IsToBeDeclaredOrDisclaimed(value))
			{
				propertyInfo.AddMessageError(string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode));
			}
		}

		protected override void CheckUS_TSCACertification()
		{
			base.CheckUS_TSCACertification();

			ListValidation.MessageErrorIfInvalidCode(Parent.US_TSCACertificationInfo, Parent.AddInfoLookups.US_TSCAIndicatorList);
			if ((OGAIndicatorList.IsToBeDeclared(Parent.US_TSCAInd) || OGAIndicatorList.IsToBeDeclared(Parent.US_ODSInd)) && Parent.US_TSCACertification.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_TSCACertificationInfo, "TSCA  Indicator");
			}
		}

		protected override void CheckUS_TSCAODSCertIndividual()
		{
			base.CheckUS_TSCAODSCertIndividual();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_TSCAODSCertIndividualInfo, Parent.AddInfoLookups.US_TSCAODSCertIndividualList);
			if ((OGAIndicatorList.IsToBeDeclared(Parent.US_TSCAInd) || OGAIndicatorList.IsToBeDeclared(Parent.US_ODSInd)) && Parent.US_TSCAODSCertIndividual.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_TSCAODSCertIndividualInfo);
			}
		}

		protected override void CheckUS_DEAInd()
		{
			base.CheckUS_DEAInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DEAIndInfo, Parent.AddInfoLookups.US_OGAIndicatorList);
			if (InvoiceLine.IsSetXLine)
			{
				ValidateDataNotNeededForXLine("DEA", InvoiceLine.US_DEAIndInfo);
			}
			else if (IsPGAValidationApplicable())
			{
				AgencyRequirementsValidator.ValidatePGA(Parent.US_DEAIndInfo, "DEA", true, InvoiceLine.PGARequirementIndicator.HasDEARequirement);
				AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.US_DEAIndInfo, "DEA", Parent.DEAHeaders.Cast<IPGADataCorrection>());
				AgencyRequirementsValidator.ValidatePGAIsDisallowed(GovernmentAgencyProgramCodeList.Codes.DEA, IsEntrySummary, IsCargoRelease, ImportEntryType, IsCertifyCargoRelease, IsPGAExpeditedRelease, IsWeeklyEstimateFiling, Parent.US_DEAIndInfo);
			}
			else if (!InvoiceLine.IsACECargoReleaseValidationMode)
			{
				ValidatePGANotApplicable(Parent.US_DEAIndInfo);
			}
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.DEA, InvoiceLine.OGAAgencyRequirements);
			ValidateUS_DEADisclaimReason();
		}

		protected override void CheckUS_DEADisclaimReason()
		{
			base.CheckUS_DEADisclaimReason();

			AgencyRequirementsValidator.ValidatePGADisclaimed(Parent.US_DEADisclaimReasonInfo, Parent.US_DEAInd, Parent.AddInfoLookups.DEADisclaimReasonList);
			AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(GovernmentAgencyProgramCodeList.Codes.DEA, InvoiceLine.OGAAgencyRequirements);

			ValidateUS_DEAInd();
		}

		#endregion

		#region Properties

		protected ZBool IsEntrySummary
		{
			get
			{
				var declaration = InvoiceLine.Declaration;
				return declaration?.US_EnableENS ?? false;
			}
		}

		protected ZBool IsCargoRelease
		{
			get
			{
				var declaration = InvoiceLine.Declaration;
				return declaration?.US_EnableCRL ?? false;
			}
		}

		protected ZString ImportEntryType
		{
			get { return InvoiceLine.ImportEntryType; }
		}

		protected ZBool IsPGAExpeditedRelease
		{
			get
			{
				var declaration = InvoiceLine.Declaration;
				return declaration?.US_PGAExpeditedRelease ?? false;
			}
		}

		protected ZBool IsWeeklyEstimateFiling
		{
			get
			{
				var declaration = InvoiceLine.Declaration;
				return declaration?.IsWeeklyEstimateConsumptionFTZ ?? false;
			}
		}

		protected ZBool IsCertifyCargoRelease
		{
			get { return InvoiceLine.Declaration?.US_CertifyCargoRelease ?? false; }
		}

		#endregion
	}
}
