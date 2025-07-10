using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class FormalImportJobComInvoiceLineValidation : CommonImportJobComInvoiceLineValidation
	{
		public FormalImportJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override bool IsTariffMandatory
		{
			get { return IsEntrySummaryOrCargoReleaseValidationMode; }
		}

		protected override bool IsDescriptionRequired
		{
			get
			{
				return Parent.IsACECargoCertificationMode && new PGAIndicatorsInvoiceLineWrapper(Parent).HasAnyPGADataToBeDeclaredOrDisclaimed();
			}
		}

		protected override void CheckJI_HazMatCodeQualifier()
		{
			base.CheckJI_HazMatCodeQualifier();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_HazMatCodeQualifierInfo, Parent.AddInfoLookups.US_HazMatQualifierList, (NoResString)HazMatCodeQualifierShouldBeInList);
		}

		internal const string HazMatCodeQualifierShouldBeInList = "Please enter a valid Dangerous Goods Type Code. The code you have selected is not in the Dangerous Goods Type Codes List.";

		protected override void CheckJI_ParentID()
		{
			base.CheckJI_ParentID();

			JobComInvoiceLine topLevelInvoiceLine = Parent.ParentTariffLine;

			if (topLevelInvoiceLine != null)
			{
				JobComInvoiceLineValidation validationObject = topLevelInvoiceLine.Validation;

				validationObject.ValidateJI_Tariff();
				validationObject.ValidateJI_LinePrice();
			}

			ValidateJI_Weight();
			ValidateJI_WeightUQ();
			ValidateJI_InvoiceQuantity();
			ValidateJI_InvoiceUQ();
			ValidateJI_BondedWhsQuantity();
			var addInfoValidation = Parent.AddInfoValidation;
			addInfoValidation.ValidateUS_WHSEntryLineNo();
			addInfoValidation.ValidateUS_WHSEntryNumber();
		}

		protected override void CheckJI_PartNo()
		{
			base.CheckJI_PartNo();
			ValidateJI_InvoiceQuantity();
			ValidateJI_BondedWhsQuantity();
		}

		protected override void CheckJI_TariffIsValidWhenItIsNotEmpty()
		{
			TariffValidator.Validate(Parent, Tariff, Parent.JI_TariffInfo);
			TariffValidator.ValidateFishNotFromRussia(Parent, Parent.JI_TariffInfo);

			// TIB entries
			if (!USCTariff.IsTIB(Parent.US_SupTariff))
			{
				TariffValidator.Ensure98_99IsNotEntered(Parent.JI_TariffInfo, Tariff);
			}
			TariffValidator.ValidateBasedOnAdditionalTariffNumberIndicator(Parent, Tariff, Parent.JI_TariffInfo, Parent.HasSecondaryTariffLines);

			USCTariff importTariff = Parent.ParentTariffLine != null ? Parent.ParentTariffLine.ImportTariff : Tariff;
			TariffValidator.CheckSTNRule(Parent, importTariff, Parent.JI_TariffInfo, false);

			if (Tariff != null)
			{
				if (Tariff.UE_QuotaIndicator)
				{
					CheckQuotaOnTariff();
				}

				CheckDutyFreeGoods();

				CheckMIDAgainstCountryOfOrigin();

				if (!Parent.IsACE && InvoiceLine.PGARequirementIndicator.MayRequireACSLacey && !Parent.HasLaceyActData && !Parent.IsSetXLine)
				{
					Parent.JI_TariffInfo.AddWarning(LaceyActDataMayBeRequired);
				}

				var tariff = Tariff.UE_Tariff;
				if ((tariff.StartsWith("98") || tariff.StartsWith("99")) && Tariff.UE_AdditionalTariffNumberIndicator)
				{
					Parent.JI_TariffInfo.AddWarning(Chapter98or99ShouldBeEnteredInProvTariff);
				}
			}

			ValidateForTariffAgainstFormalEntryRequirement();
		}
		internal const string LaceyActDataMayBeRequired = "This Tariff may require reporting of Lacey Act data.";
		internal const string Chapter98or99ShouldBeEnteredInProvTariff = "This Tariff number may require an additional Tariff number. If so, this number should be entered in the Prov/Prog Tariff field, with the classification Tariff number placed in the Tariff number field.";

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected USCTariff Tariff
		{
			get { return tariff ?? (tariff = Parent.ImportTariff); }
		}
		USCTariff tariff;

		void CheckQuotaOnTariff()
		{
			ZString warningMessage = "";

			if (!EntryTypeList.IsQuotaVisa(Declaration.US_EntryType))
			{
				warningMessage = QuotaMayBeApplicable;
			}

			ZString tariffOne, tariffTwo;
			if (Parent.HasSupplementary)
			{
				tariffOne = Parent.US_SupTariff;
				tariffTwo = Parent.JI_Tariff;
			}
			else
			{
				tariffOne = Parent.JI_Tariff;
				var theOnlySecondaryTariff = Parent.SecondaryTariffLines.IsCountEqualTo(1) ? Parent.SecondaryTariffLines.ElementAt(0) : null;
				tariffTwo = theOnlySecondaryTariff != null ? theOnlySecondaryTariff.JI_Tariff : ZString.Empty;
			}

			var quota = new USCQuota.Loader(Parent.Factory).LoadBestMatchFor(tariffOne, tariffTwo, Parent.US_UC_NKCountryOfOrigin, ZDateTime.Today, Parent.Declaration.JE_ExportDate);
			if (quota != null)
			{
				warningMessage += "\r\n\r\n" + string.Format(LastQueryDetails, quota.UT_LastUpdateDate.ToLongTimeString(), quota.UT_QuotaLimit + " " + quota.UT_QuotaLimitType, quota.UT_QtyToDate, quota.QuotaStatusDesc);
			}

			if (!warningMessage.IsEmpty)
			{
				Parent.JI_TariffInfo.AddWarning(warningMessage);
			}

			if (Parent.IsQuota && quota != null && quota.UT_QuotaStatus == QuotaStatusList.Codes.Banned)
			{
				Parent.JI_TariffInfo.AddMessageError(BannedImportMessage + quota.UT_LastUpdateDate.ToLongTimeString());
			}
		}

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();

			ValidateJI_LinePrice();

			AddInfoJobComInvoiceLineValidation addInfoValidation = Parent.AddInfoValidation;

			addInfoValidation.ValidateUS_SPI();
			addInfoValidation.ValidateUS_VisaNo();
			addInfoValidation.ValidateUS_UC_NKCountryOfExport();
			addInfoValidation.ValidateUS_UC_NKCountryOfOrigin();
			addInfoValidation.ValidateUS_CottonFeeExempt();
			addInfoValidation.ValidateUS_FDAIndicator();
			addInfoValidation.ValidateUS_FCCIndicator();
			addInfoValidation.ValidateUS_DOTIndicator();
			addInfoValidation.ValidateUS_OverrideDuty();
			addInfoValidation.ValidateUS_ADDCaseNo();
			addInfoValidation.ValidateUS_CVDCaseNo();
		}

		internal const string BannedImportMessage = "This quota's status is banned as of ";
		internal const string QuotaMayBeApplicable = "According to CBP reference files, this tariff may be subject to quota.";
		internal const string LastQueryDetails =
@"There is a query record which indicates that the quota for this tariff was updated at Customs on {0},
its quota limit was {1}, quantities to date was {2} and its status was '{3}'.
You can view more details in Main Menu > Maintain > Customs > Quotas.";

		void ValidateForTariffAgainstFormalEntryRequirement()
		{
			if (Declaration != null && EntryTypeList.IsInformal(Declaration.US_EntryType))
			{
				TariffValidator.ValidateAgainstFormalEntryRequirement(Declaration, Tariff, Parent.EffectiveDateForDutyRate, Parent.JI_TariffInfo, Parent.JI_ParentID);
			}
		}

		void CheckDutyFreeGoods()
		{
			if (IsEntrySummaryOrCargoReleaseValidationMode)
			{
				var entryType = Parent.ImportEntryType;
				if (EntryTypeList.IsWarehouseType(entryType) && Parent.IsDutyFree && !Parent.IsTaxOrFeePayable && Parent.US_TaxRate <= 0 && !Tariff.UE_QuotaIndicator)
				{
					Parent.JI_TariffInfo.AddWarning(DutyFreeGoodsShouldNotBeBonded);
				}

				if (Declaration != null && Declaration.IsConsolidatedMonthlyFilingOverPipeline)
				{
					if (Tariff != null && !Tariff.IsDutyFree)
					{
						Parent.JI_TariffInfo.AddMessageError(ConsolidatedMonthlyFilingOnlyForUnconditionallyDutyFreeGoods);
					}
				}
			}
		}
		internal const string DutyFreeGoodsShouldNotBeBonded = "Duty Free goods should not be bonded.";
		internal const string ConsolidatedMonthlyFilingOnlyForUnconditionallyDutyFreeGoods = "Consolidated Monthly Filing can be used for commodities that are unconditionally free only, pursuant to 19 C.F.R § 24.23(d).";

		protected override void CheckJI_OA_ExporterAddress()
		{
			base.CheckJI_OA_ExporterAddress();
			Parent.Validation.MatchToProductValidation(Parent.JI_OA_ExporterAddressInfo, () => Parent.Pivot?.CD_OA_Exporter ?? ZGuid.Empty);
		}

		protected override void CheckJI_InvoiceQuantity()
		{
			base.CheckJI_InvoiceQuantity();

			if (Parent.JI_InvoiceQuantity > ZDecimal.Zero && Parent.JI_PartNo_CanBeSetByCustomer && Parent.Part != null)
			{
				if (Declaration != null &&
					Declaration.IsInwardBondedWarehousingEnabled &&
					!Parent.JI_InvoiceQuantity.IsEmpty &&
					Parent.JI_BondedWhsQuantity.IsEmpty &&
					Parent.WHSPackLines.Count > 0 &&
					Parent.JI_InvoiceQuantity != Parent.JI_Calc_AllocatedQty)
				{
					Parent.JI_InvoiceQuantityInfo.AddMessageError(ValidationConstants.InvoiceLine.InvoiceQtyShouldBeEqualToSumOfWHSPackedQty(Declaration.TermNameForBondedWarehouse));
				}
			}
		}

		protected override void CheckJI_InvoiceUQ()
		{
			base.CheckJI_InvoiceUQ();

			ListValidation.WarnIfInvalidCode(Parent.JI_InvoiceUQInfo, Parent.Lookups.InvoiceUQList);
		}

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();

			if (Parent.JI_CustomsQuantity >= 0m)
			{
				if (IsEntrySummaryValidationMode)
				{
					var notificationType = Parent.IsChildLine || Parent.TariffMatchesSmeltCondition || Parent.TariffMatchesMeltCondition ? NotificationType.Warning : NotificationType.MessageError;
					CheckJI_CustomsQuantityBoundary(notificationType);
				}
			}
		}

		protected override void CheckJI_NetWeight()
		{
			base.CheckJI_NetWeight();

			MandatoryValidation.CheckNotNegative(Parent.JI_NetWeightInfo);
		}

		protected override bool ShouldValidateGrossWeight
		{
			get { return IsEntrySummaryValidationMode; }
		}

		protected override void CheckJI_WeightUQ()
		{
			base.CheckJI_WeightUQ();

			ValidateJI_Weight();
		}

		protected override void CheckJI_NetWeightUQ()
		{
			base.CheckJI_NetWeightUQ();

			ListValidation.MessageErrorIfInvalidCode(Parent.JI_NetWeightUQInfo, Parent.Lookups.WeightUQList);

			ValidateJI_Weight();
		}

		protected override void CheckJI_LinePrice()
		{
			base.CheckJI_LinePrice();

			ImportLinePriceValidator priceValidator = new ImportLinePriceValidator();
			if (Parent.IsSetXLine)
			{
				priceValidator.ValidateForSetXLine(Parent);
			}
			else
			{
				CheckJI_LinePriceBoundary();

				priceValidator.ValidateForValueToBeDeclaredAtSecondary(Parent);
				priceValidator.ValidateFor9802TransactionValues(Parent, Parent.JI_LinePriceInfo, Parent.JI_LinePrice);

				((FormalImportAddInfoJobComInvoiceLineValidation)Parent.AddInfoValidation).ValidateMinumCostFor9802Products(Parent.JI_LinePriceInfo, false);
				Parent.AddInfoValidation.ValidateUS_98GoodsValue();
				Parent.AddInfoValidation.ValidateUS_98ValueInvCurr();
			}
			ValidateJI_Weight();
		}

		protected override void CheckJI_Volume()
		{
			base.CheckJI_Volume();

			MandatoryValidation.CheckNotNegative(Parent.JI_VolumeInfo);
		}

		protected override void CheckBondedWhsQuantityForGUI()
		{
			base.CheckBondedWhsQuantityForGUI();
			ValidateJI_BondedWhsQuantity();
			Parent.BondedWhsQuantityForGUIInfo.AddAllNotificationsFrom(Parent.JI_BondedWhsQuantityInfo);
		}

		protected override void CheckJI_BondedWhsQuantity()
		{
			base.CheckJI_BondedWhsQuantity();
			if (Declaration != null && Declaration.IsInwardBondedWarehousingEnabled && Parent.JI_PartNo_CanBeSetByCustomer && Parent.Part != null && Parent.JI_Calc_BondedWhsQuantity.IsEmpty)
			{
				Parent.JI_BondedWhsQuantityInfo.AddMessageError(ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired(Declaration.TermNameForBondedWarehouse));
			}
			ValidateBondedWhsQuantityForGUI();
		}

		#region Boolean Flags

		protected override bool IsBondedWarehouseValidationMode
		{
			get { return IsEntrySummaryValidationMode; }
		}

		internal bool IsCargoReleaseValidationMode
		{
			get { return Declaration.IsCargoReleaseValidationMode; }
		}

		internal bool IsEntrySummaryValidationMode
		{
			get { return Declaration.IsEntrySummaryValidationMode; }
		}

		bool IsEntrySummaryOrCargoReleaseValidationMode
		{
			get { return IsEntrySummaryValidationMode || IsCargoReleaseValidationMode; }
		}

		#endregion

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateAllFeesWithSpecificSpecificRateCalculationHaveBeenAdded();
		}

		protected override bool ShouldCheckJI_OA_ManufacturerAddress
		{
			get { return InvoiceLine.IsEntrySummaryOrCargoReleaseValidationMode; }
		}

		protected void ValidateAllFeesWithSpecificSpecificRateCalculationHaveBeenAdded()
		{
			ValidateAllFeesWithSpecificSpecificRateCalculationHaveBeenAdded(Parent.ImportTariff, Parent.ImportSupTariff);
		}

		internal const string FeesWithSpecificSpecificRateRequired = "The following fee(s) need to be added as they have a Specific/Specific tax calculation: ";

		protected void ValidateAllFeesWithSpecificSpecificRateCalculationHaveBeenAdded(params USCTariff[] tariffs)
		{
			if (!(Declaration?.IsClearedInPR ?? false))
			{
				foreach (USCTariff tariff in tariffs)
				{
					if (tariff != null)
					{
						FeeCusCodeDataCollection fees = Parent.FeeCusCodes;
						ZStringBuilder messageBuilder = new ZStringBuilder();
						foreach (USCTariffDutyRate rate in tariff.DutyRates)
						{
							if (rate.UD_TaxFeeComputationCode == ComputationCodeList.Codes.SpecificSpecific &&
								!rate.UD_TaxFeeClassCode.IsEmpty &&
								rate.IsFeeRequired &&
								!fees.ContainsCode(rate.UD_TaxFeeClassCode))
							{
								messageBuilder.Append(rate.UD_TaxFeeClassCode);
							}
						}
						ZString message = messageBuilder.ToStringWithDelimiterBetweenAppends(",");
						if (!message.IsEmpty)
						{
							Parent.AddRowMessageError(FeesWithSpecificSpecificRateRequired + message);
							break;
						}
					}
				}
			}
		}

		protected override void CheckJI_OA_ConsigneeAddress()
		{
			base.CheckJI_OA_ConsigneeAddress();

			if (Parent.IsCargoReleaseValidationMode && !Parent.IsACE)
			{
				CusEntryLine entryLine = Parent.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.CargoRelease, !Parent.HasEmptySupTariff)
					?? Parent.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease, !Parent.HasEmptySupTariff);

				if (entryLine == null || !entryLine.US_IJAccepted)//therefore needs some number
				{
					OrganisationValidation.ValidateMatchedCustomsRegoNoForOrganisation(Parent.JI_OA_ConsigneeAddressInfo, OrgMatchedCustomsRegNoType.ECN, ZString.Format(OrganisationValidation.EIN_SSN_CBN_EncryptedCodeRequired, "Ultimate Consignee"), true, false);
				}
			}

			if (Parent.IsACEEntrySummaryValidationMode)
			{
				if (Parent.InvoiceHeader != null && Parent.InvoiceHeader.JZ_OA_ConsigneeAddress != Parent.JI_OA_ConsigneeAddress)
				{
					OrganisationValidation.ValidateMatchedCustomsRegoNoForOrganisation(Parent.JI_OA_ConsigneeAddressInfo, OrgMatchedCustomsRegNoType.EIN, ZString.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Ultimate Consignee"), true, false);
				}
			}

			if (Parent.IsStandAlonePriorNoticeMode && !Parent.IsACE)
			{
				FDAOrganisationValidator.Validate(Parent.ConsigneeOrgAddress, Parent.JI_OA_ConsigneeAddressInfo);
			}
		}

		protected override void CheckJI_CustomsSecondQuantity()
		{
			base.CheckJI_CustomsSecondQuantity();

			var invoiceLine = InvoiceLine;

			if (invoiceLine.IsEntrySummaryValidationMode)
			{
				CheckJI_CustomsSecondQuantityAgainstTariff();
			}

			invoiceLine.AddInfoValidation.ValidateUS_CottonFeeExempt();
		}

		protected override void CheckJI_CustomsThirdQuantity()
		{
			base.CheckJI_CustomsThirdQuantity();

			JobComInvoiceLine invoiceLine = InvoiceLine;
			if (invoiceLine.JI_CustomsThirdQuantity < 0m)
			{
				invoiceLine.JI_CustomsThirdQuantityInfo.AddMessageError(ValidationConstants.NegativeAmountNotAllowed);
			}

			if (invoiceLine.IsEntrySummaryValidationMode)
			{
				if (Parent.ImportTariff != null)
				{
					var notificationType = Parent.IsChildLine ? NotificationType.Warning : NotificationType.MessageError;
					new ImportStatQtyValidator().Validate(Parent, Parent.JI_CustomsThirdQuantityInfo, Parent.JI_CustomsThirdUnitQty,
						() => Parent.ImportTariff.RequiresThirdQuantity(), QuantityCode.ThirdQuantity, notificationType);
				}
			}

			invoiceLine.AddInfoValidation.ValidateUS_CottonFeeExempt();
			ValidateJI_LinePrice();
			ValidateJI_CustomsQuantity();
		}
	}
}
