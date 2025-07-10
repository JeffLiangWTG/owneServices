using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class ACEImportAddInfoJobDeclarationValidation : FormalImportAddInfoJobDeclarationValidation
	{
		public ACEImportAddInfoJobDeclarationValidation(AddInfoJobDeclaration addInfoJobDeclaration)
			: base(addInfoJobDeclaration)
		{
		}

		protected override void CheckUS_CertifyCargoRelease()
		{
			base.CheckUS_CertifyCargoRelease();

			if (Parent.US_CertifyCargoRelease)
			{
				if (Parent.US_ConsolACE)
				{
					Parent.US_CertifyCargoReleaseInfo.AddMessageError(CargoReleaseShouldNotBeTicked);
				}

				if (Parent.US_EnableCRL)
				{
					Parent.US_CertifyCargoReleaseInfo.AddMessageError(ShouldNotTickCertifyCargoRelease);
				}
			}

			var declaration = Declaration;
			if (declaration.US_CertifyCargoRelease && Parent.US_EntryDateElectionCode == EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate)
			{
				Parent.US_CertifyCargoReleaseInfo.AddMessageError(CertifyForCargoReleaseCannotBeUsedForWeeklyEstimate);
			}

			ValidateUS_UI_NKCarrierSCAC();
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
		}

		protected override void CheckUS_CargoReleaseType()
		{
			base.CheckUS_CargoReleaseType();

			ValidateUS_EnableCRL();
			ValidateUS_CertifyCargoRelease();

			if (Parent.US_CargoReleaseType == CargoReleaseTypeList.Codes.ACS && Parent.US_EntryType == EntryTypeList.Codes.ConsumptionFTZ)
			{
				Parent.US_CargoReleaseTypeInfo.AddMessageError(FTZNumberWillNotBeAcceptedinACS);
			}
		}
		internal const string FTZNumberWillNotBeAcceptedinACS = "You have indicated that cargo certification is to be done in ACS, but FTZ number will not be accepted. Please change Message Mode to ACS or certify cargo release in ACE.";

		protected override void CheckUS_SESplitRel()
		{
			base.CheckUS_SESplitRel();
			if (Declaration.IsACECargoReleaseValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_SESplitRelInfo, Parent.Lookups.US_SplitShipmentReleaseCodeList);
			}
		}

		protected override void CheckUS_NAFTAReconIndicator()
		{
			base.CheckUS_NAFTAReconIndicator();

			CheckAgainstLatestNonPSCEntryDataIfPossible(Parent.US_NAFTAReconIndicatorInfo, x => x.FTARecon != Parent.US_NAFTAReconIndicator, ValidationConstants.PSC.NotAllowedToChangeReconIndicator, true);
		}

		protected override void CheckUS_OtherReconIndicator()
		{
			base.CheckUS_OtherReconIndicator();

			var otherIssueCodeConvertedToENS = ReconIssueCodeList.ConvertToENSOtherIssueCode(Parent.US_OtherReconIndicator);
			CheckAgainstLatestNonPSCEntryDataIfPossible(Parent.US_OtherReconIndicatorInfo, x => x.ReconType != otherIssueCodeConvertedToENS, ValidationConstants.PSC.NotAllowedToChangeReconIndicator, true);
		}

		protected override void CheckUS_EntryType()
		{
			base.CheckUS_EntryType();

			CheckAgainstLatestNonPSCEntryDataIfPossible(Parent.US_EntryTypeInfo, x => !EntryTypeList.IsADD_CVDInvolved(Parent.US_EntryType) && EntryTypeList.IsADD_CVDInvolved(x.EntryType), ValidationConstants.PSC.CannotChangeEntryTypeToADD_CVDTypeForPSC, false);

			if (Parent.US_PSC)
			{
				if (EntryTypeList.IsInformal(Parent.US_EntryType))
				{
					Parent.US_EntryTypeInfo.AddMessageError(ValidationConstants.PSC.NotAllowedForInformalEntry);
				}
				var lastAcceptedEntryType = Declaration.LastAcceptedEntryType;
				if (!lastAcceptedEntryType.IsEmpty)
				{
					if (!Declaration.IsTemporaryImportationBond && EntryTypeList.IsTIB(lastAcceptedEntryType))
					{
						Parent.US_EntryTypeInfo.AddMessageError(ValidationConstants.PSC.EntryTypeCannotBeChangedFromTIBForPSC);
					}
					else if (Declaration.IsTemporaryImportationBond && !EntryTypeList.IsTIB(lastAcceptedEntryType))
					{
						Parent.US_EntryTypeInfo.AddMessageError(ValidationConstants.PSC.EntryTypeCannotBeChangedToTIBForPSC);
					}
				}
			}

			CheckAgainstLatestNonPSCEntryDataIfPossible(Parent.US_EntryTypeInfo, x => EntryTypeList.IsInformal(x.EntryType), ValidationConstants.PSC.EntrySummaryWasFiledAsInformalEntry, false);
			Declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();

			if (Declaration.IsMergeDone && (EntryTypeList.IsInformalImportWarningOrError(Declaration.US_EntryType) || Declaration.IsLowValue))
			{
				var customsValue = Declaration.CustomsValue;
				if (customsValue > 0)
				{
					if (EntryTypeList.IsInformalImportWarningOrError(Declaration.US_EntryType))
					{
						if (IsInformalImportWarningOrErrorRequired())
						{
							string errorOrWarning = USCustomsDataRegistry.Instance.InformalImportWarningOrError.GetValueWithoutFallback(Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty);
							if (errorOrWarning == ErrorWarningInformalImport.Codes.WAR)
							{
								Parent.US_EntryTypeInfo.AddWarning(PossibleInformalEntry);
							}
							else if (errorOrWarning == ErrorWarningInformalImport.Codes.ERR)
							{
								Parent.US_EntryTypeInfo.AddMessageError(PossibleInformalEntry);
							}
						}
					}
					else if (Declaration.IsLowValue)
					{
						var deminimus = FeeCalculationHelper.GetDeminimus(Parent.Factory);
						if (customsValue > deminimus)
						{
							Parent.US_EntryTypeInfo.AddMessageError(ZString.Format(CustomsValueExceedMaxminLowValue, deminimus));
						}
					}
				}
			}

			ValidateEntryTypeForLowValue();
			ValidateUS_EnableENS();
		}
		internal const string PossibleInformalEntry = "This entry is eligible to be filed as an Informal Entry";
		internal const string CustomsValueExceedMaxminLowValue = "The maximum customs value allowed for entry type 86 is {0}.";

		bool IsInformalImportWarningOrErrorRequired()
		{
			var customsValue = Declaration.CustomsValue;
			return customsValue > 0
				&& (customsValue < InformalFreeDutiableMaxValueAfter2013Jan7 || (customsValue < AmericanGoodsReturnedFreeDutiableMaxValue && Declaration.PGAFlags.AllInvoiceLinesAreReturnedGoods))
				&& !Declaration.PGAFlags.HasInvoiceLinesWithADCVDCaseReported
				&& !Declaration.PGAFlags.HasInvoiceLinesWithSection301Or232;
		}

		void ValidateEntryTypeForLowValue()
		{
			if (Declaration.IsLowValue)
			{
				var billValidator = new BillValidator();
				var hasMultipleMasterBills = billValidator.CheckNumberOfBillsForLowValueEntries(Declaration, Parent.US_EntryTypeInfo, Enterprise.Customs.Business.BillTypeList.Codes.MasterBill);
				if (!hasMultipleMasterBills)
				{
					billValidator.CheckNumberOfBillsForLowValueEntries(Declaration, Parent.US_EntryTypeInfo, Enterprise.Customs.Business.BillTypeList.Codes.HouseBill);
				}
			}
		}

		protected override void CheckUS_SchDEntry()
		{
			base.CheckUS_SchDEntry();

			CheckAgainstLatestNonPSCEntryDataIfPossible(Parent.US_SchDEntryInfo, x => x.EntryPort != Parent.US_SchDEntry, ValidationConstants.PSC.NotAllowedToChangeEntryPort, false);
		}

		protected override void CheckUS_AccLiqReq()
		{
			base.CheckUS_AccLiqReq();

			if (Parent.US_PSC && Parent.US_AccLiqReq && EntryTypeList.IsADD_CVDInvolved(Parent.US_EntryType))
			{
				Parent.US_AccLiqReqInfo.AddMessageError(ValidationConstants.PSC.AccLiqRequestNotAllowedForAD_CVDEntry);
			}
		}

		protected override void ValidateEntryDate()
		{
			Declaration.US_EntryDateInfo.AddWarning(DateAtEntryPortPastLimitWarning);
		}

		internal const string DateAtEntryPortPastLimitWarning = "The Arrival Date is older than 90 days and no GO Number is entered. Please Verify";

		protected override void CheckUS_PSC()
		{
			base.CheckUS_PSC();

			if (Parent.US_PSC)
			{
				if (Declaration.IsEntryFiledByCurrentCompanyForPSC)
				{
					if (Declaration.ActiveEntryHeaders.EntrySummaryEntry == null || !Declaration.ActiveEntryHeaders.EntrySummaryEntry.HasBeenLodgedAtCustoms)
					{
						Parent.US_PSCInfo.AddMessageError(ValidationConstants.PSC.PSCMayBeFiledOnAcceptedEntry);
					}

					if (Declaration.JE_EntryAuthorisationDate.IsValid && Declaration.JE_EntryAuthorisationDate.AddDays(ValidationConstants.PSC.MaxDaysFromEntryDateToFilePSC).IsInThePastDatePartOnly)
					{
						Parent.US_PSCInfo.AddMessageError(string.Format(CultureInfo.CurrentCulture, ValidationConstants.PSC.MayFilePSCUpTo300DaysFromEntryDate, ValidationConstants.PSC.MaxDaysFromEntryDateToFilePSC));
					}

					if (Declaration.RelatedStatement == null && Parent.US_PaymentType != PaymentTypeList.Codes.IndividualBasis)
					{
						Parent.US_PSCInfo.AddMessageError(ValidationConstants.PSC.MayFilePSCOnlyWhenEntryOnStatement);
					}

					if (Declaration.RelatedStatement != null && PaymentTypeList.IsPeriodicPayment(Declaration.RelatedStatement.B2_PaymentType) && (Declaration.RelatedStatement.MonthlyStatementHeader == null || !Declaration.RelatedStatement.MonthlyStatementHeader.IsFinal)
						&& Declaration.RelatedStatement.TotalAmountDue != ZDecimal.Zero)
					{
						Parent.US_PSCInfo.AddMessageError(ValidationConstants.PSC.MayFilePSCOnlyWhenPeriodicStatementIsFinalised);
					}
				}
			}

			ValidateUS_EntryType();
		}

		protected override void CheckUS_PaymentType()
		{
			base.CheckUS_PaymentType();

			CheckAgainstLatestNonPSCEntryDataIfPossible(Parent.US_PaymentTypeInfo, x => x.PaymentType != Parent.US_PaymentType, ValidationConstants.PSC.NotAllowedToChangePaymentType, true);
		}

		internal const string IndividualPaymentTypeNotAllowedUntilStatementIsIssued = "Payment Type '1' is not allowed until a preliminary statement is issued.";

		protected override void CheckUS_PreliminaryStatementPrintDate()
		{
			base.CheckUS_PreliminaryStatementPrintDate();

			CheckAgainstLatestNonPSCEntryDataIfPossible(Parent.US_PreliminaryStatementPrintDateInfo, x => x.PSD != Parent.US_PreliminaryStatementPrintDate, ValidationConstants.PSC.NotAllowedToChangePSD, true);
		}

		protected override void CheckUS_PeriodicStatementMM()
		{
			base.CheckUS_PeriodicStatementMM();

			CheckAgainstLatestNonPSCEntryDataIfPossible(Parent.US_PeriodicStatementMMInfo, x => x.PSCMonth != Parent.US_PeriodicStatementMM, ValidationConstants.PSC.NotAllowedToChangeStatementMonth, true);
		}

		protected override void CheckUS_ClientBranchDesignation()
		{
			base.CheckUS_ClientBranchDesignation();

			CheckAgainstLatestNonPSCEntryDataIfPossible(Parent.US_ClientBranchDesignationInfo, x => x.ClientBranchDesig != Parent.US_ClientBranchDesignation, ValidationConstants.PSC.NotAllowedToChangeClientBranchDesig, true);
		}

		protected override bool ShouldValidateWHSDetails
		{
			get { return EntryTypeList.IsExWarehouseOrReWarehouseType(Parent.US_EntryType); }
		}

		protected override void CheckUS_US_NKLocationOfGoods()
		{
			base.CheckUS_US_NKLocationOfGoods();

			CheckAgainstLatestNonPSCEntryDataIfPossible(Parent.US_US_NKLocationOfGoodsInfo, x => x.GoodsLocation != Parent.US_US_NKLocationOfGoods, ValidationConstants.PSC.NotAllowedToChangeLocationOfGoods, true);
		}

		protected override void CheckUS_LiveEntryIndicator()
		{
			base.CheckUS_LiveEntryIndicator();

			CheckAgainstLatestNonPSCEntryDataIfPossible(Parent.US_LiveEntryIndicatorInfo, x => x.LiveEntry ^ Parent.US_LiveEntryIndicator == YesNoDefaultList.Codes.Yes, ValidationConstants.PSC.NotAllowedToChangeLiveIndicator, true);
		}

		protected override void CheckUS_ConsolACE()
		{
			base.CheckUS_ConsolACE();
			CheckAgainstLatestNonPSCEntryDataIfPossible(Parent.US_ConsolACEInfo, x => x.Consolidated ^ Parent.US_ConsolACE, ValidationConstants.PSC.NotAllowedToChangeConsolidated, true);
		}

		public void CheckAgainstLatestNonPSCEntryDataIfPossible(ZPropertyInfo info, Func<PSCEntrySummaryData, bool> isError, string message, bool appendInvisibleToCustomsMessage)
		{
			if (Parent.US_PSC && Declaration.IsEntryFiledByCurrentCompanyForPSC)
			{
				var entryData = Declaration.EntryStatusesAndErrors.LatestNonPSCEntryData;
				if (entryData.IsValid && isError(entryData))
				{
					info.AddMessageError(message + (appendInvisibleToCustomsMessage ? " " + ValidationConstants.PSC.CustomsWontSeeThisChange : ""));
				}
			}
		}

		protected override void CheckUS_BondType()
		{
			base.CheckUS_BondType();

			if (Declaration.IsACECargoCertificationMode)
			{
				var entryType = Declaration.US_EntryType;
				var bondType = Declaration.US_BondType;

				if (entryType == EntryTypeList.Codes.InformalQuotaVisa && bondType == BondTypeList.Codes.SingleTransactionBond)
				{
					Declaration.US_BondTypeInfo.AddMessageError(SingleBondInvalid);
				}

				if (bondType == BondTypeList.Codes.NoBondRequired)
				{
					var specificMessageError = ZString.Empty;
					if (EntryTypeList.IsADD_CVDInvolved(entryType))
					{
						specificMessageError = ZString.Format("Entry Type '{0}'", entryType);
					}
					else if (EntryTypeList.IsInvalidEntryTypeForWaivedBondTypeWhenADCVDReported(entryType) && Declaration.PGAFlags.HasInvoiceLinesWithADCVDCaseReported)
					{
						specificMessageError = BondCannotBeWaivedDuetoADCVD;
					}

					if (!specificMessageError.IsEmpty)
					{
						Declaration.US_BondTypeInfo.AddMessageError(ZString.Format(BondCannotBeWaived, specificMessageError));
					}
				}

				if (Declaration.IsTemporaryImportationBond && bondType == BondTypeList.Codes.NoBondRequired)
				{
					if (Declaration.PGAFlags.HasInvoiceLinesWithADCVDCaseReported || !Declaration.PGAFlags.AllInvoiceLinesAreFromCA)
					{
						Declaration.US_BondTypeInfo.AddMessageError(BondTypeCanOnlyBeWaived);
					}
				}
			}

			ValidateUS_BondWaiverCode();
			ValidateUS_BondSuperseding();
		}

		internal const string BondCannotBeWaivedDuetoADCVD = "entries with AD/CVD cases";
		internal const string BondCannotBeWaived = "Bond cannot be waived for {0}.";
		internal const string SingleBondInvalid = "The selected bond type is invalid for Entry Type '12'.";
		internal const string BondTypeCanOnlyBeWaived = "Bond can be waived for Entry Type is '23 - TIB' if no AD/CVD cases are reported and all invoice lines are from CA.";

		protected override void CheckUS_BondType2()
		{
			base.CheckUS_BondType2();

			if (IsEntrySummaryOrCargoReleaseValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_BondType2Info, Parent.Lookups.AdditionBondTypesList);
			}

			ValidateUS_BondAmount2();
			ValidateUS_BondProducerAccNo2();
		}

		protected override void CheckUS_BondDispositionCode()
		{
			base.CheckUS_BondDispositionCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_BondDispositionCodeInfo, Parent.Lookups.US_BondDispositionCodeList);
		}

		protected override void CheckUS_BondDispositionCode2()
		{
			base.CheckUS_BondDispositionCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_BondDispositionCode2Info, Parent.Lookups.US_BondDispositionCodeList);
		}

		protected override void CheckUS_BondProducerAccNo()
		{
			base.CheckUS_BondProducerAccNo();
			if (IsEntrySummaryValidationMode)
			{
				if (Declaration.US_BondType == BondTypeList.Codes.SingleTransactionBond)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Declaration.US_BondProducerAccNoInfo);
				}
			}

			ValidateUS_BondType();
		}

		protected override void CheckUS_FDAADTA()
		{
			base.CheckUS_FDAADTA();
			if (Declaration.US_FDAADTA.IsEmpty)
			{
				if (Declaration.PGAFlags.HasInvoiceLinesWithATF)
				{
					Declaration.US_FDAADTAInfo.AddMessageError(ZString.Format(RequiredFDAADTA, "ATF"));
				}

				if (Declaration.PGAFlags.HasInvoiceLinesWithDEA)
				{
					Declaration.US_FDAADTAInfo.AddMessageError(ZString.Format(RequiredFDAADTA, "DEA"));
				}
			}

			if (Declaration.PGAFlags.HasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2)
			{
				if (Declaration.US_FDAADTA.IsEmpty)
				{
					Declaration.US_FDAADTAInfo.AddMessageError(DateTimeOfArrivalMandatoryForAMS);
				}
				else if (Declaration.US_FDAADTA.IsValid)
				{
					ValidateFDADateTime(Declaration.US_FDAADTAInfo);
				}
			}
		}
		internal const string DateTimeOfArrivalMandatoryForAMS = "Date of Arrival is mandatory for PGA AMS reporting.";
		internal const string RequiredFDAADTA = "You have not entered an Arrival Date/Time, it is required when {0} PGA information is reported.";

		protected override void CheckUS_BondAmount()
		{
			base.CheckUS_BondAmount();
			var declaration = Declaration;
			if (IsEntrySummaryValidationMode)
			{
				if (declaration.IsSingleTransactionBond)
				{
					var customsRule = declaration.CustomsRule;
					var stbRules = customsRule?.Rules.Where(x => x.CPR_RuleCode == CustomsRuleRuleCodeList.Codes.SingleTransactionBondAmount);
					if (stbRules != null && stbRules.Any(x => ZDecimal.TryParse(x.CPR_ValueTo, out var stbRuleAmount) && declaration.US_BondAmount > stbRuleAmount))
					{
						declaration.US_BondAmountInfo.AddMessageError(string.Format(BondAmountExceedsSTBRuleAmount, declaration.US_BondAmount, customsRule.HumanReadableName));
					}
				}
				else if (declaration.US_BondAmount > 0)
				{
					declaration.US_BondAmountInfo.AddMessageError(NotRelevantForContinuousBond);
				}
			}

			ValidateUS_BondType();
		}

		internal const string NotRelevantForContinuousBond = "This is only relevant for a single transaction bond type.";
		internal const string BondAmountExceedsSTBRuleAmount = "Bond Amount {0} exceeds maximum amount defined by Customs Rule: {1}.";

		protected override void CheckUS_BondWaiverCode()
		{
			base.CheckUS_BondWaiverCode();

			if (IsEntrySummaryValidationMode && !Parent.US_BondWaiverCode.IsEmpty && !Parent.US_BondType.IsEmpty)
			{
				if (Parent.US_BondType != BondTypeList.Codes.NoBondRequired)
				{
					Parent.US_BondWaiverCodeInfo.AddMessageError(BondWaiverCodeNotForNoBondRequired);
				}
				else if (EntryTypeList.IsADD_CVDInvolved(Parent.US_EntryType))
				{
					Parent.US_BondWaiverCodeInfo.AddMessageError(BondWaivedForAD_CVDEntryType);
				}
			}
		}

		protected override void CheckUS_BondProducerAccNo2()
		{
			base.CheckUS_BondProducerAccNo2();
			if (IsEntrySummaryValidationMode)
			{
				if (Declaration.US_BondType2 == BondTypeList.Codes.SingleTransactionBond)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Declaration.US_BondProducerAccNo2Info);
				}
				else if (!Declaration.US_BondProducerAccNo2.IsEmpty)
				{
					Declaration.US_BondProducerAccNo2Info.AddMessageError(NotRelevantForContinuousBond);
				}
			}
			ValidateUS_BondType2();
		}

		protected override void CheckUS_BondAmount2()
		{
			base.CheckUS_BondAmount2();
			if (IsEntrySummaryValidationMode)
			{
				if (Declaration.US_BondType2 == BondTypeList.Codes.SingleTransactionBond)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Declaration.US_BondAmount2Info);
				}
				else if (Declaration.US_BondAmount2 > 0)
				{
					Declaration.US_BondAmount2Info.AddMessageError(NotRelevantForContinuousBond);
				}
			}

			ValidateUS_BondType();
		}

		internal const string BondWaiverCodeNotForNoBondRequired = "Bond is indicated as waived while bond type is not '0'(Bond Waived/No Bond Required).";
		internal const string BondWaivedForAD_CVDEntryType = "Bond Waiving Reason is not allowed when entry type is 03, 07, 34 or 38";

		protected override void CheckUS_BondSuperseding()
		{
			base.CheckUS_BondSuperseding();
			CheckContinuousBondSuperceding(Parent.US_BondSupersedingInfo, Parent.US_BondType);
		}

		void CheckContinuousBondSuperceding(ZPropertyInfo supercedingInfo, ZString bondType)
		{
			var superceding = (ZBool)supercedingInfo.Value;

			if (superceding && bondType != BondTypeList.Codes.ContinuousBond)
			{
				supercedingInfo.AddMessageError(ContinuousBondCoverageSupersedingWhileNotContinuousBondType);
			}
		}

		internal const string ContinuousBondCoverageSupersedingWhileNotContinuousBondType = "This should be indicated as 'Y' only when bond type is continous.";

		protected override void ValidateRequirementsForPresentationDate(ZPropertyInfo propertyInfo)
		{
			var declaration = Declaration;
			if (declaration.IsConsumptionFTZ && declaration.IsACECargoCertificationMode)
			{
				if (declaration.IsCargoReleaseValidationMode)
				{
					var presentationDate = (ZDateTime)propertyInfo.Value;
					if (Parent.US_EntryDateElectionCode == EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate)
					{
						if (!presentationDate.IsValid)
						{
							propertyInfo.AddMessageError(DateRequiredForElectionCode);
						}
						else if (presentationDate.Date > ZDate.Today.AddDays(7))
						{
							var crlEntry = declaration.ActiveEntryHeaders.CargoReleaseEntry;
							if (crlEntry == null || !crlEntry.IsClearedEntry)
							{
								propertyInfo.AddMessageError(DateForWeeklyCannotBeMoreThan7DaysInTheFuture);
							}
						}
					}
					else if (!presentationDate.IsEmpty)
					{
						propertyInfo.AddMessageError(DateNotRequiredForElectionCode);
					}
				}
			}
			else
			{
				base.ValidateRequirementsForPresentationDate(propertyInfo);
			}
		}

		internal const string DateNotRequiredForElectionCode = "This date is not required unless Entry Date Election Code is 'W'.";
		internal const string DateForWeeklyCannotBeMoreThan7DaysInTheFuture = "This date cannot be more than 7 days in the future when Entry Date Election Code is 'W'.";
		internal const string DateRequiredForElectionCode = "This date is required if Entry Date Election Code is 'W' and Entry Type is '06'.";

		protected override bool EntryDateValidationRequired
		{
			get { return Declaration.US_EntryType != EntryTypeList.Codes.ReWarehouse && Parent.US_GeneralOrderNo.IsEmpty; }
		}

		protected override bool ShouldCheckFDAADTA
		{
			get { return base.ShouldCheckFDAADTA || (!Declaration.US_PSC && Declaration.IsACEFDARelevant && Declaration.PGAFlags.HasInvoiceLinesWithPGAFDA); }
		}

		protected override void CheckUS_EnableCRL()
		{
			base.CheckUS_EnableCRL();

			if (!Declaration.US_EnableCRL && Declaration.IsACECargoCertificationMode)
			{
				if (Declaration.IsConsumptionFTZ && Declaration.US_CargoReleaseType == CargoReleaseTypeList.Codes.ACE && Declaration.PGAFlags.HasInvoiceLinesWithDomesticStatus)
				{
					Parent.US_EnableCRLInfo.AddMessageError(CargoReleaseShouldBeTickedForDomesticStatus);
				}
				else if (Declaration.US_ImmediateDelivery)
				{
					Parent.US_EnableCRLInfo.AddMessageError(CargoReleaseShouldBeTickedForImmediateDelivery);
				}
			}

			if (Declaration.US_EnableCRL && Declaration.US_ConsolACE)
			{
				Parent.US_EnableCRLInfo.AddMessageError(CargoReleaseShouldNotBeTicked);
			}

			Declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
			ValidateUS_UI_NKCarrierSCAC();
			Declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
		}
		internal const string CargoReleaseShouldBeTickedForDomesticStatus = "Please enable cargo release and send cargo release messages to perform cargo certification. It is known that Customs does not handle lines with Zone Status D when creating cargo release records from entry summary.";
		internal const string CargoReleaseShouldBeTickedForImmediateDelivery = "You have indicated it is an immediate delivery. In this case, you cannot certify cargo from entry summary. Please enable cargo release.";
		internal const string CargoReleaseShouldNotBeTicked = "Consolidated Entry is indicated. No cargo release should be sent in this entry.";
		internal const string ShouldNotTickCertifyCargoRelease = "Certify Cargo Rel from Sum should not be selected if sending separate Cargo Release Message.";

		protected override void CheckUS_PipelineName()
		{
			base.CheckUS_PipelineName();

			if (Declaration.IsACECargoCertificationAndFixedTransportRelevant)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PipelineNameInfo);
			}
		}

		protected override void CheckUS_PGAExpeditedRelease()
		{
			base.CheckUS_PGAExpeditedRelease();
			var declaration = Declaration;
			if (Parent.US_PGAExpeditedRelease)
			{
				if (declaration.US_EnableCRL || declaration.US_CertifyCargoRelease)
				{
					Parent.US_PGAExpeditedReleaseInfo.AddMessageError(PGAReleaseIndicator);
				}
				else if (declaration.IsWeeklyEstimateConsumptionFTZ)
				{
					Parent.US_PGAExpeditedReleaseInfo.AddWarning(PGAFor06FTZ);
				}
				else if (declaration.IsExWarehouseEntryType)
				{
					Parent.US_PGAExpeditedReleaseInfo.AddWarning(PGAForWarehouseWithdraw);
				}
			}
		}

		internal const string PGAFor06FTZ = "This is a weekly estimate 06 entry and PGA will be sent based on a rule of each PGA.";
		internal const string PGAForWarehouseWithdraw = "This is a warehouse withdrawal entry and PGA will be sent based on a rule of each PGA.";
		internal const string PGAReleaseIndicator = "If PGA Expedited Release is ticked, a job does not require a normal Cargo Release process. Please untick both Enable Cargo Rel and Certify Cargo Rel from Sum.";

		protected override void CheckUS_TIBMotorVehicles()
		{
			base.CheckUS_TIBMotorVehicles();
			if (Declaration.IsTemporaryImportationBond && Declaration.HasAutoCondition && Declaration.US_TIBMotorVehicles == ZString.Empty)
			{
				Parent.US_TIBMotorVehiclesInfo.AddMessageError(TIBMotorVehicles);
			}
		}
		internal const string TIBMotorVehicles = "Invoice Details indicate that this TIB may contain a Motor Vehicle. Please select Y or N to confirm.";

		protected override void CheckUS_NonAMS()
		{
			base.CheckUS_NonAMS();

			if (Declaration.IsSplitShipment && Declaration.US_NonAMS)
			{
				Parent.US_NonAMSInfo.AddMessageError(FormalImportAddInfoBillValidation.NonAMSBillCannotBeSplit);
			}

			if (Declaration.US_NonAMS && Declaration.IsNonTransportDeclarationType)
			{
				Parent.US_NonAMSInfo.AddMessageError(FormalImportAddInfoBillValidation.NonTransportNeverBeNonAMS);
			}

			if (!Declaration.US_NonAMS && !Declaration.US_GeneralOrderNo.IsEmpty)
			{
				Parent.US_NonAMSInfo.AddWarning(NonAMSRequiredForGONumber);
			}

			ValidateUS_ITDate();
			Declaration.Validation.ValidateJE_PrimaryITNumber();
		}
		internal const string NonAMSRequiredForGONumber = "When the GO Number is entered, the entry should be flagged as Non-AMS.";

		protected override bool IsCarrierSCACRequired
		{
			get { return Declaration.IsACECargoCertificationAndFixedTransportRelevant || base.IsCarrierSCACRequired; }
		}

		protected override void CheckUS_ITDate()
		{
			if (Declaration.US_NonAMS && !Parent.US_ITDate.IsEmpty)
			{
				Parent.US_ITDateInfo.AddWarning(ITDateNotRequiredForNonAMSJob);
			}
			else
			{
				base.CheckUS_ITDate();
			}
		}
		internal const string ITDateNotRequiredForNonAMSJob = "In-Bond Date will not be sent in message since the Non-AMS flag is ticked. ";

		protected override void CheckUS_EntryDateElectionCode()
		{
			base.CheckUS_EntryDateElectionCode();

			var declaration = Declaration;

			if (declaration.IsConsumptionFTZ)
			{
				JobDeclarationValidation.CheckWHSTransactionExists(declaration, Parent.US_EntryDateElectionCodeInfo, () =>
				{
					var currentValue = Parent.US_EntryDateElectionCode;
					var result = currentValue == EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
					if (result)
					{
						var originalValue = (ZString)Parent.GetOriginalValue(USAddInfoSchema.US_EntryDateElectionCode);
						result = originalValue != currentValue;
					}
					return result;
				});
			}

			if (declaration.US_CertifyCargoRelease && Parent.US_EntryDateElectionCode == EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate)
			{
				Parent.US_EntryDateElectionCodeInfo.AddMessageError(CertifyForCargoReleaseCannotBeUsedForWeeklyEstimate);
			}
		}
		internal const string CertifyForCargoReleaseCannotBeUsedForWeeklyEstimate = "Certify for Cargo Release cannot be used for a Weekly Estimate";

		protected override void CheckUS_SPNIDType()
		{
			base.CheckUS_SPNIDType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_SPNIDTypeInfo, Declaration.Lookups.SPNIDTypeList);

			if (Declaration.IsStandAlonePriorNoticeMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_SPNIDTypeInfo);

				if (Declaration.IsACEENTStandAlonePriorNotice && Declaration.DecEntryNumber.IsEmpty)
				{
					Parent.US_SPNIDTypeInfo.AddMessageError(ValidationConstants.PriorNotice.EntryNumberRequiredForENTStandAlonePriorNotice);
				}
				else if (Declaration.IsACEBLNStandAlonePriorNotice && Declaration.Bills.NumberOfMasterBill == 0)
				{
					Parent.US_SPNIDTypeInfo.AddMessageError(ValidationConstants.PriorNotice.MasterBillNumberRequiredForBLNStandAlonePriorNotice);
				}
			}
		}

		protected override void CheckUS_GoodsFromFTZ()
		{
			base.CheckUS_GoodsFromFTZ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_GoodsFromFTZInfo, Parent.Declaration.AddInfoLookups.FIRMSList);
		}

		protected override void CheckUS_UI_NKCarrierSCAC()
		{
			base.CheckUS_UI_NKCarrierSCAC();
			if (Parent.US_UI_NKCarrierSCAC.IsEmpty)
			{
				if (Declaration.IsBorderWaterBorne)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_UI_NKCarrierSCACInfo, Parent.US_UI_NKCarrierSCACInfo.HumanReadableName);
				}
			}
			else
			{
				ACEImportJobDeclarationValidation.ValidateUnknownCarrierSCAC(Declaration, Parent.US_UI_NKCarrierSCACInfo);
			}
		}

		protected override void CheckUS_BRDRefNo()
		{
			base.CheckUS_BRDRefNo();
			if (Parent.US_PSC && Parent.US_BRDRefNo.IsEmpty && USCustomsDataRegistry.Instance.EntryFiler.Value.EntryFilerCode != Parent.US_EntryFilerCode)
			{
				Parent.US_BRDRefNoInfo.AddWarning(BrokerReferenceWillBeSentEmptyInPSCMessage);
			}
		}
		public const string BrokerReferenceWillBeSentEmptyInPSCMessage = "Broker Reference Number is blank.  If left blank, nothing will be send in the Broker Reference Number Field";

		#region CheckUS_InsuranceAgent

		protected override void CheckUS_InsuranceAgent()
		{
			base.CheckUS_InsuranceAgent();

			if (Declaration?.IsInsuranceFunctionEnable ?? false)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_InsuranceAgentInfo, Parent.Lookups.InsuranceAgentList);
			}
		}

		#endregion

		#region CheckUS_InsuranceDisposition

		protected override void CheckUS_InsuranceDisposition()
		{
			base.CheckUS_InsuranceDisposition();

			if (Declaration?.IsInsuranceFunctionEnable ?? false)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_InsuranceDispositionInfo, Parent.Lookups.InsuranceDispositionList);
			}
		}

		#endregion

		#region CheckUS_EnableENS

		protected override void CheckUS_EnableENS()
		{
			base.CheckUS_EnableENS();

			if (Declaration.IsLowValue && Parent.US_EnableENS)
			{
				Parent.US_EnableENSInfo.AddMessageError(EntrySummaryNotRequired);
			}
		}
		internal const string EntrySummaryNotRequired = "Entry Summary may not be filed for Entry Type 86.";

		#endregion

		#region CheckUS_FTZNo

		protected override ZString GetImportFTZNumberMessageEorrIfInvalid(ZString ftzNumber)
		{
			return FTZJobDeclarationValidation.GetFTZZoneIDFormatMessageErrorIfInvalid(FTZJobDeclarationValidation.FieldType.FTZNumber, ftzNumber);
		}

		#endregion
	}
}
