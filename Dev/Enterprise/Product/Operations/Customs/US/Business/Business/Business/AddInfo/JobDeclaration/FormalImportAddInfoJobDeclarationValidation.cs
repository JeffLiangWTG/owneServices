using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class FormalImportAddInfoJobDeclarationValidation : CommonImportAddInfoJobDeclarationValidation
	{
		public FormalImportAddInfoJobDeclarationValidation(AddInfoJobDeclaration addInfoJobDeclaration)
			: base(addInfoJobDeclaration)
		{
		}

		protected override void CheckUS_EntryMode()
		{
			base.CheckUS_EntryMode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_EntryModeInfo, Parent.Lookups.EntryModes);

			if (Declaration.IsPairedPortProgram && Parent.US_EntryDate >= USConstants.PairedPortProgramEndDate)
			{
				Parent.US_EntryModeInfo.AddMessageError(ValidationConstants.PAIIsInvalidAfter_28_01_2011);
			}
		}

		#region CheckUS_EntryType

		protected override void CheckUS_EntryType()
		{
			base.CheckUS_EntryType();
			if (WarehouseTransactionTypeHasChanged)
			{
				if (WarehouseTransactionStatusList.IsInwardCode(Declaration.WarehouseTransactionStatus))
				{
					Parent.US_EntryTypeInfo.AddError(InvalidEntryTypeForInward);
				}
				else
				{
					Parent.US_EntryTypeInfo.AddError(InvalidEntryTypeForOutward);
				}
			}
			if (IsEntrySummaryValidationMode)
			{
				if (Parent.US_EntryType.IsEmpty)
				{
					Parent.US_EntryTypeInfo.AddMessageError(ValidationConstants.EntrySummary.EntryType);
				}
				else
				{
					bool entryTypeIsInformalFreeDutiable = Parent.US_EntryType == EntryTypeList.Codes.InformalFreeDutiable;
					bool consolidatedInformalIndicatorIsNotPersonal = Parent.US_ConsolidatedInformalIndicator != ConsolidatedInformalList.Codes.Personal;

					if (entryTypeIsInformalFreeDutiable && consolidatedInformalIndicatorIsNotPersonal)
					{
						if (EntryForAmericanGoodsReturned)
						{
							if (Declaration.CustomsValue > AmericanGoodsReturnedFreeDutiableMaxValue)
							{
								Parent.US_EntryTypeInfo.AddMessageError(string.Format(CustomsValueExceedsAmericanGoodsReturnedFreeDutiableMaxValue, AmericanGoodsReturnedFreeDutiableMaxValue));
							}
						}
						else
						{
							var maxValue = Declaration.DateForMPFCalculation < ValidationConstants.InformalEntryLimit2500StartDate ? InformalFreeDutiableMaxValue : InformalFreeDutiableMaxValueAfter2013Jan7;

							if (Declaration.CustomsValue > maxValue)
							{
								Parent.US_EntryTypeInfo.AddMessageError(string.Format(CustomsValueExceedsInformalFreeDutiableMaxValue, maxValue));
							}
						}
					}
				}

				CheckEntryNumberSettings();
			}

			if (IsCargoReleaseValidationMode)
			{
				if (Parent.US_EntryType.IsEmpty)
				{
					Parent.US_EntryTypeInfo.AddMessageError(ValidationConstants.CargoRelease.EntryType);
				}

				ValidateUS_CargoReleaseType();
			}

			if (Declaration.IsStandAlonePriorNoticeMode && Parent.US_EntryType.IsEmpty)
			{
				Parent.US_EntryTypeInfo.AddMessageError(ValidationConstants.PriorNotice.EntryType);
			}

			if (AllLinesWithoutAddOrCvd())
			{
				Parent.US_EntryTypeInfo.AddMessageError(EntryTypeNotAllowedWithoutAddOrCvd);
			}

			Declaration.Validation.ValidateJE_TransportMode();
			ValidateUS_EnableAII();
			ValidateUS_IsInvoiceByRequest();
			Declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
			Declaration.Validation.ValidateJE_OH_Importer();
			ValidateUS_EntryDateElectionCode();
		}

		internal const int InformalFreeDutiableMaxValue = 2000;
		internal const int InformalFreeDutiableMaxValueAfter2013Jan7 = 2500;

		internal const string CustomsValueExceedsInformalFreeDutiableMaxValue = "Customs value exceeds ${0} - unable to send an informal entry";
		internal const int AmericanGoodsReturnedFreeDutiableMaxValue = 10000;
		internal const string CustomsValueExceedsAmericanGoodsReturnedFreeDutiableMaxValue = "Customs value for American Goods Returned exceeds ${0} - unable to send an informal entry";
		internal const string InvalidEntryTypeForInward = "Invalid Entry Type for Inward Warehouse transaction; only type '21' or '22' or '26' is allowed.";
		internal const string InvalidEntryTypeForOutward = "Invalid Entry Type for Outward Warehouse transaction; only type '31' or '32' or '34' or '38' is allowed.";
		internal const string EntryTypeNotAllowedWithoutAddOrCvd = "Entry Type not allowed when no lines have ADD or CVD information";

		ZBool AllLinesWithoutAddOrCvd()
		{
			var result = ZBool.False;
			switch (Parent.US_EntryType)
			{
				case EntryTypeList.Codes.ConsumptionADDCVD:
				case EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa:
				case EntryTypeList.Codes.WarehouseWithdrawalADDCVD:
				case EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa:
					result = true;
					break;
				default:
					break;
			}
			return result && (Parent?.Declaration?.InvoiceLines?.Cast<JobComInvoiceLine>()?.All(x => x.US_ADDCaseNo.IsEmpty && x.US_CVDCaseNo.IsEmpty) ?? ZBool.False);
		}

		bool WarehouseTransactionTypeHasChanged
		{
			get
			{
				var result = false;
				if (Parent.IsInDatabase && Declaration.HasWHSTransaction)
				{
					var originalValue = (ZString)Parent.GetOriginalValue(USAddInfoSchema.US_EntryType);
					var currentValue = Parent.US_EntryType;
					result = originalValue != currentValue &&
						(
							(EntryTypeList.IsWarehouseType(originalValue) != EntryTypeList.IsWarehouseType(currentValue)) ||
							(EntryTypeList.IsExWarehouseType(originalValue) != EntryTypeList.IsExWarehouseType(currentValue))
						);
				}
				return result;
			}
		}

		bool EntryForAmericanGoodsReturned
		{
			get
			{
				bool uSGoodsReturned = false;

				foreach (JobComInvoiceLine invoiceLine in Declaration.InvoiceLines)
				{
					if (invoiceLine.JI_Tariff.StartsWith(USGoodsReturnedTariffPrefix))
					{
						uSGoodsReturned = true;
						break;
					}
				}

				return uSGoodsReturned;
			}
		}
		internal const string USGoodsReturnedTariffPrefix = "98010010";

		protected override void CheckUS_EnableENS()
		{
			base.CheckUS_EnableENS();
			if (!Parent.US_EnableENS)
			{
				CheckIfCustomsTransactionsExist(Parent.US_EnableENSInfo, CusEntryHeaderMessageTypeList.Codes.EntrySummary, "Entry Summary" + CustomsTransactionsExist);
			}
			ValidateUS_CertifyCargoRelease();
			ValidateUS_EnableCRL();
			Declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
			Declaration.Validation.ValidateJE_OH_Importer();
			ValidateUS_PaymentType();
		}

		#endregion

		protected override void CheckUS_EntryDate()
		{
			base.CheckUS_EntryDate();

			if (IsLegacyCargoRelease || (Declaration.IsACECargoReleaseValidationMode && Declaration.IsSplitShipment))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.US_EntryDateInfo, ValidationConstants.Declaration.DateAtEntryPortRequired);
			}

			if (IsCargoReleaseValidationMode)
			{
				if (Declaration.US_EntryDate.Date < Declaration.JE_DateOfArrival.Date)
				{
					Declaration.US_EntryDateInfo.AddMessageError(DateAtEntryMustGreaterThanDateArrival);
				}

				if (EntryDateValidationRequired && Declaration.US_EntryDate < (ZDateTime.Now.AddDays(-90)))
				{
					ValidateEntryDate();
				}
				else if (Declaration.US_EntryDate > (ZDateTime.Now.AddDays(60)))
				{
					Declaration.US_EntryDateInfo.AddMessageError(DateAtEntryPortFutureLimit);
				}

				Declaration.AddInfoValidation.ValidateUS_EntryMode();
			}
		}

		protected virtual void ValidateEntryDate()
		{
			Declaration.US_EntryDateInfo.AddMessageError(DateAtEntryPortPastLimit);
		}

		protected virtual bool EntryDateValidationRequired
		{
			get { return true; }
		}

		internal const string DateAtEntryPortPastLimit = "If the Arrival Date is older than 90 days measured from the submission date, Cargo Certification is not permitted";
		internal const string DateAtEntryPortFutureLimit = "If the Arrival Date is more than 60 days in the future, measured from the submission date, Cargo Certification is not permitted";
		internal const string DateAtEntryMustGreaterThanDateArrival = "Date at Entry Port must not be earlier than Date at Discharge Port";

		protected override void CheckUS_EstEnteredValue()
		{
			base.CheckUS_EstEnteredValue();

			if (Declaration.IsEstimatedEnteredValueRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_EstEnteredValueInfo, "Estimated Entered Value");
			}
		}

		protected override void CheckUS_PayableMPF()
		{
			base.CheckUS_PayableMPF();

			if (Declaration.IsConsolidatedMonthlyFilingOverPipeline)
			{
				if (Parent.US_PayableMPF == 0)
				{
					Parent.US_PayableMPFInfo.AddMessageError(MPFMayNotBeZero);
				}
				else if (Parent.US_PayableMPF < 0)
				{
					Parent.US_PayableMPFInfo.AddMessageError(MPFMayNotBeNegative);
				}
			}
		}

		internal const string MPFMayNotBeZero = "Total MPF is required when Consolidated Monthly Filing is indicated.";
		internal const string MPFMayNotBeNegative = "Total MPF may not be negative.";

		protected override void CheckUS_BRDRefNo()
		{
			base.CheckUS_BRDRefNo();

			if (Declaration.IsFormalImport && !Parent.US_BRDRefNo.IsEmpty)
			{
				ZDBOnlyQuery query = new GenAddOnColumnQueryHelper(typeof(JobDeclaration)).GetQueryOnGenAddOnColumn(JobDeclaration.Schema.US_BRDRefNo, Parent.US_BRDRefNo);
				query.AddToFilter(JobDeclarationSchema.PK, SQLComparisonOperator.NotEqual, Declaration.PK);

				JobDeclaration duplicateDec = Parent.Factory.LoadTop1<JobDeclaration>(query);
				if (duplicateDec != null)
				{
					Parent.US_BRDRefNoInfo.AddError(string.Format(DuplicateDeclarationWithSameRefNo, duplicateDec.JE_DeclarationReference));
				}

				if (IsEntrySummaryValidationMode && Parent.US_BRDRefNo.Length > 9)
				{
					Parent.US_BRDRefNoInfo.AddWarning(LastNineLettersOfBrokerReferenceWillBeSent);
				}
			}
		}

		public const string DuplicateDeclarationWithSameRefNo = "The declaration, '{0}' has the same BIRD Ref No.";
		public const string LastNineLettersOfBrokerReferenceWillBeSent = "7501 allows only nine characters in the message and therefore last nine characters will be sent.";

		protected override void CheckUS_TaxDeferIndicator()
		{
			base.CheckUS_TaxDeferIndicator();
			if (IsEntrySummaryValidationMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_TaxDeferIndicatorInfo, "deferred tax indicator");
				ListValidation.MessageErrorIfInvalidCode(Parent.US_TaxDeferIndicatorInfo, Parent.Lookups.US_TaxDeferIndicatorList);

				if (!Parent.US_TaxDeferIndicator.IsEmpty && Parent.US_TaxDeferIndicator != TaxDeferIndicatorList.Codes.NotApplicableOrNoDeferredTax)
				{
					foreach (CusEntryHeader entry in Declaration.CustomsEntryHeaders)
					{
						if (entry.IsFormalEntry && EntryTypeList.IsInformal(entry.EntryType))
						{
							Parent.US_TaxDeferIndicatorInfo.AddMessageError(TaxIsToBeDeferredOnlyOnFormalEntries);
							break;
						}
					}
				}
			}
		}

		internal const string TaxIsToBeDeferredOnlyOnFormalEntries = "Tax should not be deferred on informal entry types. (11, 12)";

		protected override void CheckUS_EntryFilerCode()
		{
			base.CheckUS_EntryFilerCode();

			JobDeclarationValidation validation = Declaration.Validation;

			validation.ValidateJE_MessageType();
			validation.ValidateDecEntryNumber();

			if (Declaration.IsImportByExternalBroker && !Declaration.DecEntryNumber.IsEmpty)
			{
				if (Parent.US_EntryFilerCode.IsEmpty)
				{
					Parent.US_EntryFilerCodeInfo.AddError(EntryNumberEnteredWithoutEntryFilerCode);
				}
				else
				{
					var branch = Declaration.Branch;
					if (branch != null)
					{
						ZString currentCompanyEntryFilerCode = USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty).EntryFilerCode;

						if (currentCompanyEntryFilerCode == Parent.US_EntryFilerCode)
						{
							Parent.US_EntryFilerCodeInfo.AddError(EntryFilerCodeSameAsCurrentCompany);
						}
					}
				}
			}
		}

		public const string EntryNumberEnteredWithoutEntryFilerCode = "You have entered an entry number without an entry filer code.";
		public const string EntryFilerCodeSameAsCurrentCompany = "This job is to be done by an outport broker, however the entry filer code entered is the same as this company's entry filer code.";

		protected override void CheckUS_GeneralOrderNo()
		{
			base.CheckUS_GeneralOrderNo();

			if (Parent.US_GeneralOrderNo.IsEmpty)
			{
				if (Declaration.JE_EntryAuthorisationDate.IsEmpty /*not released*/ && Parent.US_EntryDate.IsValid)
				{
					ZDateTime etaPlus15Days = Parent.US_EntryDate.AddDays(15);
					if (ZDateTime.Now > etaPlus15Days)
					{
						Parent.US_GeneralOrderNoInfo.AddWarning(GONoRequired);
					}
				}
			}
			else
			{
				if (!Regex.IsMatch(Parent.US_GeneralOrderNo, @"^[0-9]{12}$") &&
					!Regex.IsMatch(Parent.US_GeneralOrderNo, @"^[0-9]{13}$"))
				{
					Parent.US_GeneralOrderNoInfo.AddWarning(GONoInvalidFormat);
				}
			}
		}
		internal const string GONoRequired = "General Order Number is required on documentation if the entry is made more than 15 days after the ETA";
		internal const string GONoInvalidFormat = "General Order Number should be 12 numeric digits or 13 numeric digits.\r\nThis value will be printed in Block 21 of the 7501 document.\r\nFormatted as 'G.O. NNNNNNNNNNNN' or 'GO-NNNN-NNNN-NNNNN'.";

		protected override void CheckUS_US_NKLocationOfGoods()
		{
			base.CheckUS_US_NKLocationOfGoods();

			if (!Declaration.IsExWarehouse)
			{
				var firms = Declaration.LocationOfGoods;
				if (firms == null)
				{
					if (!Declaration.IsACECargoCertificationMode && IsCargoReleaseValidationMode && !Declaration.IsBorderMovement)
					{
						Parent.US_US_NKLocationOfGoodsInfo.AddMessageError(LocationOfGoodsRequiredForCargoRelease);
					}
					else if (EntryTypeList.IsWarehouseLocationNeededFor(Parent.US_EntryType))
					{
						Parent.US_US_NKLocationOfGoodsInfo.AddMessageError(string.Format(LocationOfGoodsRequiredForEntrySummaryWithEntryType, Parent.US_EntryType));
					}
				}
				else
				{
					if (IsEntrySummaryValidationMode)
					{
						if (firms.HasAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.FacilityType, FacilityTypeList.Codes.DataProcessingSite_07)
							|| firms.HasAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.FacilityType, FacilityTypeList.Codes.CBPAdministrativeSite_08))
						{
							Parent.US_US_NKLocationOfGoodsInfo.AddMessageError(FirmsCodeFacilityType07or08);
						}
					}

					if (!Declaration.IsPairedPortProgram && !firms.HasAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DistrictPortCode, Parent.US_SchDEntry))
					{
						Parent.US_US_NKLocationOfGoodsInfo.AddWarning(ValidationConstants.Declaration.FirmsNotOnTheSameDistrict);
					}
				}
			}
		}

		internal const string LocationOfGoodsRequiredForCargoRelease = "Location of Goods (FIRMS code) is required for Cargo Release";
		internal const string LocationOfGoodsRequiredForEntrySummaryWithEntryType = "Location of Goods (FIRMS code) is required for Entry Summary (where Entry Type is '{0}')";
		internal const string FirmsCodeFacilityType07or08 = "Facilities with Type Code 07 or 08 should not be used for entry purposes.";

		void CheckIfCustomsTransactionsExist(ZPropertyInfo info, ZString cH_MessageType, string errorMessageToAdd)
		{
			if (DoesEntryHaveActiveTransactionsWithCustoms(cH_MessageType))
			{
				info.AddError(errorMessageToAdd);
			}
		}

		internal ZBool DoesEntryHaveActiveTransactionsWithCustoms(ZString cH_MessageType)
		{
			ZBool result = false;
			CusEntryHeader[] entries = (CusEntryHeader[])Declaration.ActiveEntryHeaders.Find(new ZQuery(CusEntryHeaderSchema.CH_MessageType, cH_MessageType));

			foreach (CusEntryHeader entry in entries)
			{
				if (!entry.IsOKToBeDeactivated)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public const string CustomsTransactionsExist = " cannot be disabled because customs transactions exist.";

		protected override void CheckUS_ConsolidatedInformalIndicator()
		{
			base.CheckUS_ConsolidatedInformalIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ConsolidatedInformalIndicatorInfo, Parent.Lookups.US_ConsolidatedInformalList, (NoResString)ConsolidatedInformalIndicatorShouldBeInList);

			if (Parent.US_ConsolidatedInformalIndicator == ConsolidatedInformalList.Codes.Consolidated)
			{
				if (!Declaration.IsACE)
				{
					Parent.US_ConsolidatedInformalIndicatorInfo.AddMessageError(ConsolidatedNotSupported);
				}
			}
			else if (Parent.US_ConsolidatedInformalIndicator == ConsolidatedInformalList.Codes.Samples)
			{
				if (Parent.US_EntryType != EntryTypeList.Codes.InformalFreeDutiable && Parent.US_EntryType != EntryTypeList.Codes.InformalQuotaVisa)
				{
					Parent.US_ConsolidatedInformalIndicatorInfo.AddMessageError(MustBeInformalEntryForSamples);
				}
			}
		}

		internal const string MustBeInformalEntryForSamples = "Use of Sample Indicator (X) is only allowed for Informal Entry Types, (11 or 12).";
		internal const string ConsolidatedNotSupported = "Consolidated declarations are not supported.";
		internal const string ConsolidatedInformalIndicatorShouldBeInList = "Please enter a valid Consolidated Informal Indicator code. The code you have selected is not in the Consolidated Informal Indicator codes List.";

		void CheckEntryNumberSettings()
		{
			if (Parent.US_EnableENS)
			{
				var stmNums = ACEEntryStmNumsSetting.GetFirstAvailableOrLastSequenceStmNums(Declaration.Branch, Parent.US_EntryFilerCode);
				if (stmNums == null)
				{
					Parent.US_EntryTypeInfo.AddWarning(BranchEntryNumberNotSetup);
				}
				else if (stmNums.HasReachedLimit)
				{
					Parent.US_EntryTypeInfo.AddWarning(string.Format((stmNums.Owner is GlbCompany ? CompanyEntryNumberLimitReachWarning : BranchEntryNumberLimitReachWarning), stmNums.TotalAvailableNumbers));
				}
			}
		}
		internal const string BranchEntryNumberNotSetup = "The entry numbers for this branch has not been setup.\nPlease setup the entry range for this branch.";
		internal const string BranchEntryNumberLimitReachWarning = "The entry numbers for this branch are running out.\nThere are only {0} numbers left.\nPlease update your entry range in or alternative contact Customs for a new Entry Filer Code.";
		internal const string CompanyEntryNumberLimitReachWarning = "The entry numbers for this company are running out.\nThere are only {0} numbers left.\nPlease contact Customs for a new Entry Filer Code.";

		protected override void CheckUS_CertifyCargoRelease()
		{
			base.CheckUS_CertifyCargoRelease();

			if (Declaration.IsRemoteLocationFiling && !Declaration.IsACECargoCertificationMode)
			{
				if (!Parent.US_CertifyCargoRelease && !Parent.US_EnableCRL)
				{
					Parent.US_CertifyCargoReleaseInfo.AddMessageError(MustCertifyForRLF);
				}
			}

			CheckCertifyCargoReleaseForExWarehouse(Parent.US_CertifyCargoReleaseInfo, IsExWarehouse);
		}
		internal const string MustCertifyForRLF = "You have enabled a 'Remote Location Filling', which indicates this is a remote entry. \nWhen remote location filing, you must Certify Cargo Rel from Sum.";
		internal const string ShouldNotCertifyForExWarehouse = "You should not certify for Cargo Release for Warehouse Withdrawal entry types (31, 32, 34, 38).";

		internal static void CheckCertifyCargoReleaseForExWarehouse(ZPropertyInfo propertyInfo, ZBool isExWarehouse)
		{
			var certifyCargoRelease = (ZBool)propertyInfo.Value;
			if (isExWarehouse && certifyCargoRelease)
			{
				propertyInfo.AddMessageError(ShouldNotCertifyForExWarehouse);
			}
		}

		protected override void CheckUS_DES()
		{
			base.CheckUS_DES();

			ListValidation.MessageErrorIfInvalidCode(Parent.US_DESInfo, Parent.Lookups.FIRMSList);

			if (!Parent.US_SchDExam.IsEmpty)
			{
				var firmsCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(
								Parent.Factory,
								Declaration.US_DES,
								Core.Constants.CountryCodes.UnitedStates,
								Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode,
								ZDateTime.Today,
								null,
								new[] {
									new RefCusCodeListAttributeFilter(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DistrictPortCode, SQLComparisonOperator.Equal, new ZString[] { Parent.US_SchDExam })
									});
				if (firmsCode == null)
				{
					Parent.US_DESInfo.AddWarning(DesExamSiteNotOnTheSameDistrict);
				}
			}

			ValidateUS_SchDExam();
			ValidateUS_EnableAII();
		}
		internal const string DesExamSiteNotOnTheSameDistrict = "Designated Exam Site location is not in the same district as Designated Exam Port.";

		protected sealed override void CheckUS_EnableINB()
		{
			base.CheckUS_EnableINB();
			Declaration.Validation.ValidateJE_MessageType();
			ValidateUS_InbondType();
		}

		protected override void CheckUS_EnableCRL()
		{
			base.CheckUS_EnableCRL();

			if (!Parent.US_EnableCRL)
			{
				CheckIfCustomsTransactionsExist(Parent.US_EnableCRLInfo, CusEntryHeaderMessageTypeList.Codes.CargoRelease, "Cargo Release" + CustomsTransactionsExist);
				CheckIfCustomsTransactionsExist(Parent.US_EnableCRLInfo, CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease, "Cargo Release" + CustomsTransactionsExist);
			}

			if (!Parent.US_EnableCRL && Declaration.US_CargoReleaseType != CargoReleaseTypeList.Codes.ACE)
			{
				CheckIfCustomsTransactionsExist(Parent.US_EnableCRLInfo, CusEntryHeaderMessageTypeList.Codes.ACECargoRelease, "ACE Cargo Release" + CustomsTransactionsExist);
			}
		}

		protected override void CheckUS_BondType()
		{
			base.CheckUS_BondType();

			if (IsEntrySummaryOrCargoReleaseValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_BondTypeInfo, Parent.Lookups.US_BondTypeList);

				if (!Declaration.US_EntryType.IsEmpty &&
					(EntryTypeList.IsBondTypeCodeRequired(Declaration.US_EntryType) || Declaration.IsACECargoReleaseValidationMode && Declaration.US_EntryType == EntryTypeList.Codes.InformalQuotaVisa)
					&& Declaration.US_BondType.IsEmpty)
				{
					Declaration.US_BondTypeInfo.AddMessageError(BondTypeRequired);
				}

				if (Declaration.IORWrapper != null)
				{
					List<ZString> activityCodes = new List<ZString>(new ZString[] { ((IBondDetailsDefault)Declaration).ActivityCode, ActivityCodeList.Codes._1a1 });
					CusBondDetailCollection bondDetails = Declaration.IORWrapper.BondDetails;
					bool continousBondExistsButNotActive = bondDetails.HasContinuousBond &&
													bondDetails.GetActiveBondDetailDataFor(activityCodes, ImporterBondTypeList.Codes.ContinuousBond, Declaration.EffectiveDateForBond) == null;

					if (continousBondExistsButNotActive)
					{
						Declaration.US_BondTypeInfo.AddWarning(ContinousBondExistsButNotActive);
					}
				}

				ValidateUS_BondAmount();
				ValidateUS_BondProducerAccNo();
				ValidateUS_BondCalcCode();
			}
			ValidateUS_EnableAII();
			ValidateUS_IsInvoiceByRequest();
		}
		internal const string BondTypeRequired = "Bond Type Code is required for selected Entry Type.";
		internal const string ContinousBondExistsButNotActive = "The continous bond details on the Importer of Record organization have expired. An importer bond query can be sent, from the Imported of Record organization, in order to retrieve the latest bond information from customs.";

		protected override void CheckUS_BondAmount()
		{
			base.CheckUS_BondAmount();
			var declaration = Declaration;
			if (IsEntrySummaryValidationMode && declaration.IsSingleTransactionBond && declaration.US_BondCalcCode == SEBCalculationList.Codes.MAN)
			{
				MandatoryValidation.MessageErrorIfIsZero(Declaration.US_BondAmountInfo);
				MandatoryValidation.MessageErrorIfIsNegative(Declaration.US_BondAmountInfo);

				if (declaration.US_BondAmount > 0)
				{
					var enteredValue = declaration.ActiveEntryHeaders.EntySummaryEnteredValue;
					if (enteredValue > 0 && declaration.US_BondAmount < 100)
					{
						declaration.US_BondAmountInfo.AddWarning(BelowMinimumWithManualValue);
					}

					var totalPayable = declaration.ActiveEntryHeaders.EntySummaryTotalAmountPayable;
					if (totalPayable > declaration.US_BondAmount)
					{
						declaration.US_BondAmountInfo.AddMessageError(TotalPayableGreaterThanBondAmount);
					}
				}
			}
		}
		internal const string TotalPayableGreaterThanBondAmount = "The current total payable is greater than the bond amount.";
		internal const string BelowMinimumWithManualValue = "Manual calculation of bond is less than Customs specified Minimum Bond amount of $100";

		protected override void CheckUS_BondCalcCode()
		{
			base.CheckUS_BondCalcCode();

			if (Declaration.US_BondType == BondTypeList.Codes.SingleTransactionBond)
			{
				if (Declaration.US_BondCalcCode.IsEmpty)
				{
					Declaration.US_BondCalcCodeInfo.AddMessageError(CalcCodeRequired);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_BondCalcCodeInfo, Parent.Lookups.SEBCalcCodes);

					if (Declaration.US_BondCalcCode != SEBCalculationList.Codes.MAN)
					{
						if (Declaration.US_EntryType == EntryTypeList.Codes.TradeFair)
						{
							Declaration.US_BondCalcCodeInfo.AddMessageError(TradFairBond);
						}

						if (Declaration.US_EntryType == EntryTypeList.Codes.PermanentExhibition && Declaration.US_BondCalcCode != SEBCalculationList.Codes.EXH)
						{
							Declaration.US_BondCalcCodeInfo.AddMessageError(ExhibitionBond);
						}

						if (Declaration.US_EntryType == EntryTypeList.Codes.TemporaryImportationBond)
						{
							if (!Declaration.US_TIBMVNonConforming && Declaration.US_BondCalcCode != SEBCalculationList.Codes.TIB)
							{
								Declaration.US_BondCalcCodeInfo.AddMessageError(TemporaryImportationBond);
							}
						}
						else if (Declaration.US_BondCalcCode == SEBCalculationList.Codes.TIB)
						{
							Declaration.US_BondCalcCodeInfo.AddMessageError(NotTIBEntry);
						}
					}

					ValidateUS_BondAmount();
				}
			}
		}
		internal const string CalcCodeRequired = "A calculation code must be chosen for Single Transaction Bond";
		internal const string TradFairBond = "The bond required for trade fair entry is determined by the district director. Please use Bond Calculation Code 'MAN' and enter the appropriate value";
		internal const string TemporaryImportationBond = "For a Temporary Import Bond (TIB) entry, please select Bond Calculation Code 'TIB' or 'MAN'.";
		internal const string NotTIBEntry = "This entry is not a Temporary Import Bond entry. Bond Calculation Codes 'TIB' should only be used for Temporary Import Bond entries.";
		internal const string ExhibitionBond = "For an exhibition entry, Bond Calculation Code 'EXH' should generally be used";

		protected override void CheckUS_BondProducerAccNo()
		{
			base.CheckUS_BondProducerAccNo();
			if (IsEntrySummaryValidationMode)
			{
				if (Declaration.US_BondType == BondTypeList.Codes.SingleTransactionBond)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Declaration.US_BondProducerAccNoInfo);

					CusBondDetail bondData = new BondDetailsDefaulter().GetBondDetailsForAccountNo(Declaration, Declaration.US_BondProducerAccNo);
					if (bondData != null)
					{
						ZDate declarationBondEffectiveDate = Declaration.EffectiveDateForBond;
						if (bondData.PW_BondExpiryDate < declarationBondEffectiveDate)
						{
							Declaration.US_BondProducerAccNoInfo.AddMessageError(BondExpired);
						}

						if (bondData.PW_BondEffectiveDate > declarationBondEffectiveDate)
						{
							Declaration.US_BondProducerAccNoInfo.AddMessageError(string.Format(BondIsNotEffective, declarationBondEffectiveDate.ToShortDateString()));
						}
					}
				}
			}
		}
		internal const string BondExpired = "Bond has expired. Please refer to Importer Of Record > Details > Config > US Defaults.";
		internal const string BondIsNotEffective = "The Bond is not effective as it is later than the declaration bond date, {0}. In this case, Cargo Selectivity will not be issued via ABI until the Bond Effective Date equals the calendar date.";

		protected override void CheckUS_OtherReconIndicator()
		{
			base.CheckUS_OtherReconIndicator();
			if (IsEntrySummaryValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_OtherReconIndicatorInfo, Parent.Lookups.OtherReconIssueList);

				if (Parent.US_OtherReconIndicator.IsEmpty)
				{
					if (EntryTypeList.IsValidForRecon(Parent.US_EntryType))
					{
						Parent.US_OtherReconIndicatorInfo.AddMessageError(SelectReconFor7501Entry);
					}
				}
				else
				{
					if (!EntryTypeList.IsValidForRecon(Parent.US_EntryType) && Parent.US_OtherReconIndicator != ReconIssueCodeList.Codes.NotApplicable)
					{
						Parent.US_OtherReconIndicatorInfo.AddMessageError(JobNotFlaggedForReconciliation);
					}

					if (Parent.US_EntryType == EntryTypeList.Codes.ConsumptionQuotaVisa)
					{
						if (!ReconIssueCodeList.IsValidForConsumptionQuotaVisaEntryType(Parent.US_OtherReconIndicator))
						{
							Parent.US_OtherReconIndicatorInfo.AddMessageError(ReconIssueInvalidForConsumptionQuotaVisa);
						}
					}

					if (Parent.US_OtherReconIndicator == ReconIssueCodeList.Codes._9802Recon ||
						Parent.US_OtherReconIndicator == ReconIssueCodeList.Codes.Class9802Recon ||
						Parent.US_OtherReconIndicator == ReconIssueCodeList.Codes.Value9802Recon ||
						Parent.US_OtherReconIndicator == ReconIssueCodeList.Codes.ValueClass9802Recon)
					{
						if (!Declaration.Has9802Tariff)
						{
							Parent.US_OtherReconIndicatorInfo.AddWarning(No9802Tariff);
						}
					}
				}
			}
		}
		internal const string JobNotFlaggedForReconciliation = "The selected Entry Type is not valid for reconciliation. Only Entry Types 01, 02 or 06 are valid for reconciliation.";
		internal const string ReconIssueInvalidForConsumptionQuotaVisa = "A reconciliation Reason Code that is valid for the selected entry type, (Consumption Quota Visa), is either 'Value Recon', '9802 Recon' or 'Value/9802 Recon'.";
		internal const string SelectReconFor7501Entry = "Recon Indicator must be selected on a 7501 entry requiring reconciliation, (Entry Types 01, 02 or 06).";
		internal const string No9802Tariff = "There are no '9802' tariffs (Prov/Prog. Tariffs) on any of the invoice lines to warrant this reconciliation reason.";
		internal const string NoMatchingLinesForFTARecon = "There are no invoice lines with a FTA SPI that warrant FTA Recon.";

		protected override void CheckUS_NAFTAReconIndicator()
		{
			base.CheckUS_NAFTAReconIndicator();

			if (Parent.US_NAFTAReconIndicator)
			{
				if (!EntryTypeList.IsValidForRecon(Parent.US_EntryType))
				{
					Parent.US_NAFTAReconIndicatorInfo.AddMessageError(JobNotFlaggedForReconciliation);
				}

				if (AllEligibleSPIInvoiceLinesClaimedSPI(Declaration.InvoiceLines))
				{
					Parent.US_NAFTAReconIndicatorInfo.AddWarning(NoMatchingLinesForFTARecon);
				}
			}
		}

		ZBool AllEligibleSPIInvoiceLinesClaimedSPI(InvoiceLineCompleteCollection invoiceLineCompleteCollection)
		{
			var eligibleInvoiceLines = invoiceLineCompleteCollection?.Cast<JobComInvoiceLine>()?.Where((x) =>
			{
				var spiList = x.AddInfoLookups?.SPIList;
				return spiList != null && spiList.Count > 0 && !(spiList.Count == 1 && spiList[0].Code == SPICompleteList.MoreCodes.NotApplicable);
			});
			return eligibleInvoiceLines?.All(x => !x.US_SPI.IsEmpty && x.US_SPI != SPICompleteList.MoreCodes.NotApplicable) ?? ZBool.False;
		}

		protected override void CheckUS_SchDEntry()
		{
			base.CheckUS_SchDEntry();

			if (IsEntrySummaryOrCargoReleaseValidationMode)
			{
				if (Parent.US_SchDEntry.IsEmpty)
				{
					var isRequiredForACECargoRelease = Declaration.IsACECargoReleaseValidationMode && (!Declaration.JE_PrimaryITNumber.IsEmpty || EntryTypeList.IsEntryPortRequired(Declaration.US_EntryType) || Declaration.PGAFlags.HasInvoiceLinesWithPGA || Declaration.US_NonAMS);

					if (IsEntrySummaryValidationMode || IsLegacyCargoRelease || isRequiredForACECargoRelease)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.US_SchDEntryInfo, "Port of Entry");
					}
					else if (IsCargoReleaseValidationMode)
					{
						Parent.US_SchDEntryInfo.AddWarning(SubmitPortOfEntryIsRecommend);
					}
				}
				else
				{
					CheckPortOfEntryAndPortOfDischargeWhenITPresents();

					if (Parent.US_SchDEntry == Parent.US_SchDArrival)
					{
						if (Parent.US_ITDate.IsEmpty)
						{
							if (Declaration.IsPairedPortProgram
								&& (Parent.US_EntryDate.IsEmpty || Parent.US_EntryDate < USConstants.PairedPortProgramEndDate))
							{
								Parent.US_SchDEntryInfo.AddMessageError(PairedPortOfEntrySameAsPortOfDischarge);
							}
						}
					}
					else
					{
						if (!IsExWarehouse && !Declaration.IsPairedPortProgram)
						{
							ValidateUS_CargoReleaseType();
						}
					}
				}

				if (Declaration.IsRemoteLocationFiling && Parent.US_SchDEntry == Parent.US_PreparerDistrictPort)
				{
					Parent.US_SchDEntryInfo.AddWarning(RLFPortOfEntrySameAsPreparerPort);
				}
			}
		}

		public const string PairedPortOfEntrySameAsPortOfDischarge = "Port of Entry and Port of Discharge should differ where Paired Port Program is indicated.";
		public const string RLFPortOfEntrySameAsPreparerPort = "Port of Entry and Preparer Port should normally differ on an RLF entry.";
		public const string SubmitPortOfEntryIsRecommend = "CBP recommends that the Planned Port of Entry be submitted on the entry as doing so will assist with timely and accurate processing.";

		protected override void CheckUS_SchDArrival()
		{
			base.CheckUS_SchDArrival();

			if (Parent.US_SchDArrival.IsEmpty && !Declaration.IsConsumptionFTZ)
			{
				var airOrSeaEntryNotExWHS = !IsExWarehouse && IsEntrySummaryValidationMode && (Declaration.IsAir || Declaration.IsSea);
				var isRequiredForACECargoRelease = Declaration.IsACECargoReleaseValidationMode && (!Declaration.JE_PrimaryITNumber.IsEmpty || Declaration.PGAFlags.HasInvoiceLinesWithPGA || TransportTypeList.IsPortOfDischargeMandatory(Declaration.JE_TransportMode));

				if (airOrSeaEntryNotExWHS || IsLegacyCargoRelease || isRequiredForACECargoRelease)
				{
					Parent.US_SchDArrivalInfo.AddMessageError(PortOfDischargeRequired);
				}
			}

			if (Declaration.IsFDAValidationMode)
			{
				CheckUS_SchDArrivalRequiredForPriorNotice();
			}

			if (!IsExWarehouse && IsEntrySummaryValidationMode && !Declaration.IsPairedPortProgram)
			{
				CheckPortMatchesTransportMode(Parent.US_SchDArrivalInfo);
			}
		}
		internal const string PortOfDischargeRequired = "Port Of Discharge is required.";

		protected override void CheckUS_SchDExam()
		{
			base.CheckUS_SchDExam();

			ListValidation.MessageErrorIfInvalidCode(Parent.US_SchDExamInfo, Lookups.DischargeSchDList);
			if (IsEntrySummaryValidationMode)
			{
				if (!Parent.US_SchDExam.IsEmpty && !Declaration.IsRemoteLocationFiling)
				{
					Parent.US_SchDExamInfo.AddMessageError(ExamPortForRLFOnly);
				}

				if (Parent.US_SchDExam.IsEmpty && !Parent.US_DES.IsEmpty)
				{
					Parent.US_SchDExamInfo.AddMessageError(ExamPortRequired);
				}
			}
		}
		internal const string ExamPortForRLFOnly = "Designated exam port may only be entered for Remote Location Filing entries. Please check 'RLF' checkbox to indicate this is a Remote Location entry.";
		internal const string ExamPortRequired = "Designated Exam Port is required when Designated Exam Site is entered.";

		protected override void CheckUS_PreparerDistrictPort()
		{
			base.CheckUS_PreparerDistrictPort();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_PreparerDistrictPortInfo, Parent.Lookups.RegionDistrictPorts, (NoResString)PreparerDistrictPortShouldBeInList);
			if (IsEntrySummaryValidationMode || IsCargoReleaseValidationMode)
			{
				if (Declaration.IsRemoteLocationFiling)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PreparerDistrictPortInfo, PreparerDistrictPortIsMandatory);
				}
				else
				{
					if (!Parent.US_PreparerDistrictPort.IsEmpty)
					{
						Parent.US_PreparerDistrictPortInfo.AddMessageError(PreparerDistrictPortRLFOnly);
					}
				}
			}
		}
		internal const string PreparerDistrictPortIsMandatory = "Preparer District Port. Preparer District Port is mandatory when Remote Location Filling is set";
		internal const string PreparerDistrictPortShouldBeInList = "Please enter a valid Preparer District Port Code. The code you have selected is not in the Preparer District Port codes List.";
		internal const string PreparerDistrictPortRLFOnly = "Preparer District Port  may only be entered for Remote Location Filing entries. Please check 'RLF' checkbox to indicate this is a Remote Location entry.";

		protected override void CheckUS_PreparerOfficeCode()
		{
			base.CheckUS_PreparerOfficeCode();

			if (IsEntrySummaryValidationMode)
			{
				if (!Parent.US_PreparerOfficeCode.IsEmpty && !Declaration.IsRemoteLocationFiling)
				{
					Parent.US_PreparerOfficeCodeInfo.AddMessageError(PreparerOfficeCodeRLFOnly);
				}
			}
		}
		internal const string PreparerOfficeCodeRLFOnly = "Preparer Office Code may only be entered for Remote Location Filing entries. Please check 'RLF' checkbox to indicate this is a Remote Location entry.";

		protected override void CheckUS_PeriodicStatementMM()
		{
			base.CheckUS_PeriodicStatementMM();
			if (IsEntrySummaryValidationMode && !Parent.US_PSC)
			{
				var validator = new PeriodicStatementMMValidator();

				validator.Validate(Parent.US_PeriodicStatementMMInfo, Parent.US_PaymentType, Parent.Lookups.US_MonthList);

				if (Declaration.RelatedStatement == null || !Declaration.RelatedStatement.IsPaid)
				{
					validator.ValidateAgainstCurrentDate(Parent.US_PeriodicStatementMMInfo);
				}
				validator.ValidateAgainstReleaseDate(Parent.US_PeriodicStatementMMInfo, Declaration.JE_EntryAuthorisationDate);
			}
		}

		protected override void CheckUS_PaymentType()
		{
			base.CheckUS_PaymentType();

			if (IsEntrySummaryValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_PaymentTypeInfo, Parent.Lookups.US_PaymentTypeList);

				if (!Parent.US_PSC)
				{
					if (Parent.US_PaymentType.IsEmpty)
					{
						Parent.US_PaymentTypeInfo.AddMessageError(PaymentTypeRequiredWarning);
					}

					if (PaymentTypeList.IsPeriodicPayment(Parent.US_PaymentType))
					{
						var entrySummaryEntry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
						if (entrySummaryEntry != null && entrySummaryEntry.TotalEstimatedTax > 0m && !entrySummaryEntry.IsTaxDeferred)
						{
							Parent.US_PaymentTypeInfo.AddMessageError(IRTaxNotAllowedToBePaidOnAMonthlyStatement);
						}
					}

					ValidateUS_PreliminaryStatementPrintDate();
					ValidateUS_PeriodicStatementMM();
				}

				if (IsIndividualBasisPaymentInvalidForRLFEntry)
				{
					Parent.US_PaymentTypeInfo.AddMessageError(IndividualBasisPaymentInvalidForRLFEntry);
				}
			}

			ValidateUS_EnableAII();
			ValidateUS_IsInvoiceByRequest();
		}

		internal const string PaymentTypeRequiredWarning = "You have not entered a value, Payment Type is required by Customs.";
		internal const string IRTaxNotAllowedToBePaidOnAMonthlyStatement = "IR Tax is not allowed to be paid on a monthly statement.";
		internal const string IndividualBasisPaymentInvalidForRLFEntry = "Payment Type cannot be '1' for RLF entries on initial filing.";

		protected override void CheckUS_PreliminaryStatementPrintDate()
		{
			base.CheckUS_PreliminaryStatementPrintDate();

			if (IsEntrySummaryValidationMode && !Parent.US_PSC)
			{
				new DeclarationPrelimStatementPrintDateValidator().ValidatePreliminaryStatementPrintDate(Parent.US_PreliminaryStatementPrintDateInfo, Declaration);

				if (!Declaration.IsManualPayment && Parent.US_PreliminaryStatementPrintDate.IsValid)
				{
					ValidateUS_EstimatedEntryDate();
				}
			}

			ValidateUS_FixPSD();
		}

		protected override void CheckUS_FixPSD()
		{
			base.CheckUS_FixPSD();

			if (Parent.US_FixPSD && Parent.US_PreliminaryStatementPrintDate.IsEmpty)
			{
				Parent.US_FixPSDInfo.AddError("Preliminary Statement Print Date can only be locked when it has a value.");
			}
		}

		protected override void CheckUS_FixDefTaxDueDate()
		{
			base.CheckUS_FixDefTaxDueDate();

			if (Parent.US_FixDefTaxDueDate && Parent.US_DeferredTaxDueDate.IsEmpty)
			{
				Parent.US_FixDefTaxDueDateInfo.AddError(FixDefTaxDueDate);
			}
		}
		internal const string FixDefTaxDueDate = "Deferred Tax Due Date can only be locked when it has a value.";

		WeekendsAndHolidaysValidator WeekendsAndHolidaysValidator
		{
			get { return weekendsAndHolidaysValidator ?? (weekendsAndHolidaysValidator = new WeekendsAndHolidaysValidator()); }
		}
		WeekendsAndHolidaysValidator weekendsAndHolidaysValidator;

		protected override void CheckUS_ClientBranchDesignation()
		{
			base.CheckUS_ClientBranchDesignation();

			if (!Parent.US_ClientBranchDesignation.IsEmpty)
			{
				if (Parent.US_PaymentType == PaymentTypeList.Codes.IndividualBasis || Parent.US_PaymentType == "")
				{
					Parent.US_ClientBranchDesignationInfo.AddMessageError("Client Branch Designation is not valid in the case that Payment Type is '1'.");
				}

				string clientRegistryBranchDesignation = USCustomsDataRegistry.Instance.ClientBranchDesignation.GetFallBackValueAtAllLevels(Guid.Empty, Declaration.RegistryBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid());
				if ((!string.IsNullOrEmpty(clientRegistryBranchDesignation)) && (Parent.US_ClientBranchDesignation != clientRegistryBranchDesignation))
				{
					Parent.US_ClientBranchDesignationInfo.AddMessageError(string.Format(ValidationConstants.ClientBranchDesignationNotMatchRegistrySetting, clientRegistryBranchDesignation));
				}
			}
		}

		protected override void CheckUS_ADDCVDSuretyCode()
		{
			base.CheckUS_ADDCVDSuretyCode();
			if (IsEntrySummaryOrCargoReleaseValidationMode && !Parent.US_ADDCVDSuretyCode.IsEmpty)
			{
				ZString messageError = SuretyCodeValidator.Validate(Parent.US_ADDCVDSuretyCode);
				if (!messageError.IsEmpty)
				{
					Parent.US_ADDCVDSuretyCodeInfo.AddMessageError(messageError);
				}
			}
		}

		protected override void CheckUS_BondDesignationCode()
		{
			base.CheckUS_BondDesignationCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_BondDesignationCodeInfo, Parent.Lookups.US_BondDesignationCodeList);
			if (Declaration.IsACE && Declaration.US_BondType == BondTypeList.Codes.SingleTransactionBond)
			{
				if (Declaration.US_BondDesignationCode.IsEmpty)
				{
					Parent.US_BondDesignationCodeInfo.AddMessageError(ShouldEntryDesignationCode);
				}
			}
		}

		protected override void CheckUS_SuretyCode()
		{
			base.CheckUS_SuretyCode();
			if (IsEntrySummaryOrCargoReleaseValidationMode)
			{
				if (!Declaration.US_BondType.IsEmpty && Declaration.US_BondType != BondTypeList.Codes.NoBondRequired)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_SuretyCodeInfo);
				}

				if (!Parent.US_SuretyCode.IsEmpty)
				{
					ZString messageError = SuretyCodeValidator.Validate(Parent.US_SuretyCode);
					if (!messageError.IsEmpty)
					{
						Parent.US_SuretyCodeInfo.AddMessageError(messageError);
					}
					else if (Declaration.US_BondType == BondTypeList.Codes.ContinuousBond)
					{
						var iORWrapper = Declaration.IORWrapper;
						if (iORWrapper != null)
						{
							var activityCode = ((IBondDetailsDefault)Declaration).ActivityCode;
							List<ZString> activityCodes = new List<ZString>(new ZString[] { activityCode, ActivityCodeList.Codes._1a1 });

							var bondDetail = iORWrapper.BondDetails.GetActiveBondDetailDataFor(activityCodes, BondTypeList.Codes.ContinuousBond, Declaration.EffectiveDateForBond);
							if (bondDetail != null && !bondDetail.HasSufficientFund)
							{
								Parent.US_SuretyCodeInfo.AddMessageError(BondIsInsufficient);
							}
						}
					}
				}

				if (EntryTypeList.IsInformal(Declaration.US_EntryType) && Declaration.IsWithoutBondType && !Declaration.US_SuretyCode.IsEmpty)
				{
					Parent.US_SuretyCodeInfo.AddMessageError(InformalEntriesWithoutBondTypeDoesntRequireSuretyCode);
				}
			}
		}

		internal const string BondIsInsufficient = "Bond is insufficient per last bond query. Single Transaction Bond must be filed.";
		internal const string InformalEntriesWithoutBondTypeDoesntRequireSuretyCode = "Surety Code is not permitted for Informal Entry Types when no Bond Type.";
		internal const string ShouldEntryDesignationCode = "Please enter a designation code.";
		internal const string NoElectronicMessageForAirIT = "Electronic messaging is not valid, for IT, when transport mode is AIR";

		protected override void CheckUS_SchDLoading()
		{
			base.CheckUS_SchDLoading();

			if (!IsExWarehouse && (IsEntrySummaryValidationMode))
			{
				if (Declaration.IsSchDPortOfLoadingRequired)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_SchDLoadingInfo, PortOfLoadingRequiredForWaterBorne);
				}
			}
		}
		internal const string PortOfLoadingRequiredForWaterBorne = "Port of Loading for transport modes water borne";

		protected override bool ShouldValidateUS_EntryType
		{
			get { return IsEntrySummaryOrCargoReleaseValidationMode; }
		}

		internal const string FinalDestinationRequiredForEntrySummary = "A destination port is required for an entry summary to send its destination state in the message for non-informal entries.";
		internal const string FinalDestinationShouldBeForeignForInBond = "For InBond entries, this should be a foreign port.";

		protected override void CheckUS_EstimatedEntryDate()
		{
			base.CheckUS_EstimatedEntryDate();
			if (IsExWarehouse || Declaration.IsReWarehouse)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.US_EstimatedEntryDateInfo);
			}
			else
			{
				if (Parent.US_EstimatedEntryDate.IsValid)
				{
					if (IsEntrySummaryValidationMode && !Parent.US_PreliminaryStatementPrintDate.IsEmpty)
					{
						if (Parent.US_EstimatedEntryDate > Parent.US_PreliminaryStatementPrintDate)
						{
							Parent.US_EstimatedEntryDateInfo.AddMessageError(EstEntryDateAfterPaymentDate);
						}
					}

					ValidateUS_PreliminaryStatementPrintDate();
				}
			}

			if (Parent.US_EstimatedEntryDate.IsValid && Parent.US_ITDate.IsValid && Parent.US_EstimatedEntryDate < Parent.US_ITDate)
			{
				Parent.US_EstimatedEntryDateInfo.AddMessageError(EstEntryDateBeforeITDate);
			}
		}
		internal const string EstEntryDateAfterPaymentDate = "Estimated Entry Date cannot be later than Payment Due Date (Preliminary Statement Print Date).";
		internal const string EstEntryDateBeforeITDate = "Estimated Entry Date cannot be earlier than IT Date.";

		protected override void CheckUS_LiveEntryIndicator()
		{
			base.CheckUS_LiveEntryIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_LiveEntryIndicatorInfo, Parent.Lookups.US_YesNoList, (NoResString)LiveEntryIndicShouldBeInList);

			if (Parent.US_EntryType != US.Business.EntryTypeList.Codes.ConsumptionFTZ && EntryTypeList.IsQuotaVisa(Parent.US_EntryType))
			{
				if (Parent.US_LiveEntryIndicator.IsEmpty)
				{
					Parent.US_LiveEntryIndicatorInfo.AddMessageError(LiveEntryIndicatorShouldSelectedWhenEntryTypeIsQuotaVisa);
				}
			}

			ValidateUS_PresentationDate();
		}
		internal const string LiveEntryIndicShouldBeInList = "Please enter a valid Live Entry Indicator.";
		internal const string LiveEntryIndicatorShouldSelectedWhenEntryTypeIsQuotaVisa = "Please enter a valid Live Entry Indicator. A selection is required when entry type is Quota Visa";

		protected override void CheckUS_PresentationDate()
		{
			base.CheckUS_PresentationDate();
			if (Parent.US_EntryDateElectionCode == EntryDateElectionCodeList.Codes.PresentationDate && Parent.US_PresentationDate.IsValid)
			{
				WeekendsAndHolidaysValidator.CheckWeekendsAndHolidays(Parent.US_PresentationDateInfo);
			}

			if (IsCargoReleaseValidationMode || IsEntrySummaryValidationMode)
			{
				if (Parent.US_EntryDateElectionCode == EntryDateElectionCodeList.Codes.PresentationDate && Parent.US_LiveEntryIndicator == YesNoDefaultList.Codes.Yes)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PresentationDateInfo, "Presentation Date - " + PresentationDateRequiredPriorETA);
				}
				else
				{
					ValidateRequirementsForPresentationDate(Parent.US_PresentationDateInfo);
				}

				if (Parent.US_EntryDateElectionCode == EntryDateElectionCodeList.Codes.PresentationDate && Parent.US_PresentationDate.IsValid && Parent.US_EntryDate.IsValid)
				{
					if (Parent.US_PresentationDate.Date < Declaration.US_EntryDate.Date)
					{
						Parent.US_PresentationDateInfo.AddMessageError(PresentationDateRequiredPriorETA);
					}
				}

				if (Declaration.JE_EntryAuthorisationDate.IsValid && Parent.US_PresentationDate.IsValid && Declaration.JE_EntryAuthorisationDate.IsValid && Parent.US_PresentationDate.Date > Declaration.JE_EntryAuthorisationDate.Date)
				{
					Parent.US_PresentationDateInfo.AddMessageError(PresentationDateNoLaterThanReleaseDate + Declaration.JE_EntryAuthorisationDate.ToString("MM-dd-yy") + ".");// This is a US format.
				}
			}
		}
		internal const string PresentationDateRequiredPriorETA = "For Cargo Release Entry Date Code P, the Presentation Date is required and cannot be prior to the Estimated Arrival Date.";
		internal const string PresentationDateNoLaterThanReleaseDate = "The Presentation Date cannot be later than the Release Date, ";

		protected virtual void ValidateRequirementsForPresentationDate(ZPropertyInfo propertyInfo)
		{
			if (!Parent.US_PresentationDate.IsEmpty && Parent.US_EntryDateElectionCode != EntryDateElectionCodeList.Codes.PresentationDate)
			{
				propertyInfo.AddMessageError(PresentationDateNotRequired);
			}
		}
		internal const string PresentationDateNotRequired = "Date is only allowed when Entry Date Election Code is equal to 'P' and the entry is a live entry or if certify for ACE Cargo Release and Entry Date Election Code is 'W' and Entry Type is '06'.";

		protected override void CheckUS_ExpConsign()
		{
			base.CheckUS_ExpConsign();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ExpConsignInfo, Parent.Lookups.US_YesNoList, (NoResString)ExpressConsignmentIndicatorShouldBeInList);
		}
		internal const string ExpressConsignmentIndicatorShouldBeInList = "Please enter a valid Express Consignment Indicator code. The code you have selected is not in the Express Consignment codes List.";

		protected override void CheckUS_OGALineReleaseIndicator()
		{
			base.CheckUS_OGALineReleaseIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_OGALineReleaseIndicatorInfo, Parent.Lookups.US_YesNoList, (NoResString)OGALineReleaseIndicatorShouldBeInList);
		}
		internal const string OGALineReleaseIndicatorShouldBeInList = "Please enter a valid OGA Line Release Indicator code. The code you have selected is not in the OGA Line Release Indicator codes List.";

		protected override void CheckUS_EntryDateElectionCode()
		{
			base.CheckUS_EntryDateElectionCode();
			if (Declaration.IsACECargoCertificationMode && Declaration.IsConsumptionFTZ)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_EntryDateElectionCodeInfo, Parent.Lookups.EntryDateElectionCodeList);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_EntryDateElectionCodeInfo, Parent.Lookups.EntryDateElectionCodeList, (NoResString)EntryDateElectionCodeShouldBeInList);
			}
			ValidateUS_PresentationDate();
		}
		internal const string EntryDateElectionCodeShouldBeInList = "Please enter a valid Entry Date Election code. The code you have selected is not in the Entry Date Election codes List.";

		protected override void CheckUS_IsHMFApplicable()
		{
			base.CheckUS_IsHMFApplicable();
			if (ShouldCheckUS_IsHMFApplicable)
			{
				if (Declaration.IsSea && Declaration.IsHMFApplicable)
				{
					if (EntryTypeList.IsHMFNotApplicable(Parent.US_EntryType))
					{
						Parent.US_IsHMFApplicableInfo.AddMessageError(FeeApplicableForSeaOnlyWhenNotCertainEntryType + Lookups.EntryTypeCodeList.GetDescriptionFromCode(Parent.US_EntryType));
					}
				}
			}
		}

		protected override bool ShouldCheckUS_IsHMFApplicable
		{
			get { return IsEntrySummaryValidationMode; }
		}

		internal const string FeeApplicableForSeaOnlyWhenNotCertainEntryType = "Harbor Maintenance Fee is not applicable for Mode Of Transport 'Sea' when Entry Type is ";

		protected override bool IsCarrierSCACRequired
		{
			get
			{
				bool isRequired = false;
				if (!Declaration.IsExWarehouse && IsEntrySummaryOrCargoReleaseValidationMode)
				{
					isRequired = base.IsCarrierSCACRequired;

					isRequired |= Declaration.IsBorderMovement && IsCargoReleaseValidationMode;
				}

				isRequired |= Declaration.IsBorderMovement && IsCargoReleaseValidationMode;

				return isRequired;
			}
		}

		protected override void CheckUS_WHSEntryNumber()
		{
			base.CheckUS_WHSEntryNumber();

			if (ShouldValidateWHSDetails)
			{
				if (Parent.US_WHSEntryNumber.IsEmpty)
				{
					Parent.US_WHSEntryNumberInfo.AddMessageError(WarehouseEntryNumberRequired);
				}
				else
				{
					if (Parent.US_WHSEntryNumber.KeepNumericCharacters() != Parent.US_WHSEntryNumber)
					{
						Parent.US_WHSEntryNumberInfo.AddMessageError(WarehouseEntryNumberFormat);
					}
				}
			}
		}
		internal const string WarehouseEntryNumberRequired = "Warehouse Entry Number is required for an Ex-Warehouse Job (Entry types: 31, 32, 34 & 38) or for ACE Cargo Release with Entry Type 22.";
		internal const string WarehouseEntryNumberFormat = "Warehouse Entry Number should be eight (8) digits in length.";

		protected virtual bool ShouldValidateWHSDetails
		{
			get { return Declaration.IsExWarehouse; }
		}

		protected override void CheckUS_WHSDistrictPortCode()
		{
			base.CheckUS_WHSDistrictPortCode();
			if (IsEntrySummaryValidationMode && Declaration.IsExWarehouse)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_WHSDistrictPortCodeInfo, Parent.Lookups.RegionDistrictPorts);
			}
		}

		protected override void CheckUS_WHSEntryFilerCode()
		{
			base.CheckUS_WHSEntryFilerCode();
			if (IsEntrySummaryValidationMode && ShouldValidateWHSDetails)
			{
				if (Parent.US_WHSEntryFilerCode.Length != 3)
				{
					Parent.US_WHSEntryFilerCodeInfo.AddMessageError(WHSFilerCode);
				}
				ValidateUS_WHSEntryNumber();
			}
		}
		internal const string WHSFilerCode = "Warehouse Entry Filer Code should be at least three (3) digits in length.";

		protected override void CheckUS_IsFinalWHS()
		{
			base.CheckUS_IsFinalWHS();
			if (Declaration.IsExWarehouse)
			{
				if (!Parent.US_IsFinalWHS && Parent.US_QtyInWHAfterWithdrawal.IsEmpty)
				{
					Parent.US_IsFinalWHSInfo.AddWarning(FinalWithdrawalRequired);
				}

				ValidateUS_QtyInWHAfterWithdrawal();
			}
		}
		internal const string FinalWithdrawalRequired = "If Balance remaining in Warehouse is zero, Final Withdrawal should be ticked.";

		protected override void CheckUS_QtyBeingWithdrawn()
		{
			base.CheckUS_QtyBeingWithdrawn();
			if (Declaration.IsExWarehouse)
			{
				if (Parent.US_QtyBeingWithdrawn > Parent.US_QtyInWHBeforeWithdrawal)
				{
					Parent.US_QtyBeingWithdrawnInfo.AddWarning(WithdrawalConflict);
				}
			}
		}
		internal const string WithdrawalConflict = "Balance being withdrawn cannot be greater than the quantity in the warehouse before withdrawal.";

		protected override void CheckUS_QtyInWHAfterWithdrawal()
		{
			base.CheckUS_QtyInWHAfterWithdrawal();
			if (Declaration.IsExWarehouse)
			{
				if (Parent.US_IsFinalWHS && !Parent.US_QtyInWHAfterWithdrawal.IsEmpty)
				{
					Parent.US_QtyInWHAfterWithdrawalInfo.AddWarning(FinalWithdrawalConflict);
				}
				else
				{
					ZDecimal balanceCheck = Parent.US_QtyInWHBeforeWithdrawal - Parent.US_QtyBeingWithdrawn;
					if (balanceCheck != Parent.US_QtyInWHAfterWithdrawal)
					{
						Parent.US_QtyInWHAfterWithdrawalInfo.AddWarning(BalanceError);
					}
				}

				ValidateUS_IsFinalWHS();
			}
		}
		internal const string FinalWithdrawalConflict = "If Final Withdrawal is ticked, Balance remaining in Warehouse should be zero.";
		internal const string BalanceError = "Balance remaining in Warehouse does not equate correctly.";

		protected override void CheckUS_FDACANType()
		{
			base.CheckUS_FDACANType();

			if (!Declaration.IsStandAlonePriorNoticeMode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_FDACANTypeInfo, Parent.Lookups.FDACANTypes);

				if (Declaration.IsFDAValidationMode)
				{
					if (Declaration.RequiresPriorNoticeReporting)
					{
						if (Parent.US_UI_NKCarrierSCAC.IsEmpty && Parent.US_FDACANType.IsEmpty && !Declaration.IsConsumptionFTZ)
						{
							Parent.US_FDACANTypeInfo.AddMessageError(ValidationConstants.PriorNotice.CarrierType);
						}

						ListValidation.MessageErrorIfInvalidCode(Parent.US_FDACANTypeInfo, Lookups.FDACANTypes);
					}
				}
			}
			ValidateUS_FDACAN();
			ValidateUS_FDACCN();
		}

		protected override void CheckUS_FDACAN()
		{
			base.CheckUS_FDACAN();

			if (!Declaration.IsStandAlonePriorNoticeMode && Declaration.IsFDAValidationMode && Declaration.RequiresPriorNoticeReporting)
			{
				string message = "";

				if (Parent.US_FDACANType == FDACarrierTypeList.Codes.Carrier)
				{
					message = ValidationConstants.PriorNotice.CarrierName;
				}
				else if (Parent.US_FDACANType == FDACarrierTypeList.Codes.PrivatelyOwnedUSVehicle)
				{
					message = ValidationConstants.PriorNotice.PrivatelyOwnedUSVehicleLicense;
				}
				else if (Parent.US_FDACANType == FDACarrierTypeList.Codes.PrivatelyOwnedFNVehicle)
				{
					message = ValidationConstants.PriorNotice.PrivatelyOwnedForeignVehicleStateProvinceName;
				}

				if (!string.IsNullOrEmpty(message) && Parent.US_FDACAN.IsEmpty)
				{
					Parent.US_FDACANInfo.AddMessageError(message);
				}
			}
		}

		protected override void CheckUS_FDACCN()
		{
			base.CheckUS_FDACCN();

			if (!Declaration.IsStandAlonePriorNoticeMode)
			{
				if (Declaration.IsFDAValidationMode && Declaration.RequiresPriorNoticeReporting)
				{
					string message = "";

					if (Parent.US_FDACANType == FDACarrierTypeList.Codes.Carrier)
					{
						message = ValidationConstants.PriorNotice.CarrierCountry;
					}
					else if (Parent.US_FDACANType == FDACarrierTypeList.Codes.PrivatelyOwnedUSVehicle)
					{
						message = ValidationConstants.PriorNotice.PrivatelyOwnedUSVehicleStateCode;
					}

					if (!string.IsNullOrEmpty(message) && Parent.US_FDACCN.IsEmpty)
					{
						Parent.US_FDACCNInfo.AddMessageError(message);
					}
				}
			}
		}

		protected override void CheckUS_ITDate()
		{
			base.CheckUS_ITDate();

			if (Declaration.IsEntrySummaryValidationMode || Declaration.IsCargoReleaseValidationMode)
			{
				if (Declaration.US_ITDate.IsEmpty && Declaration.US_EntryType != EntryTypeList.Codes.ReWarehouse && Declaration.IsBillDetailRequired())
				{
					if (Declaration.IsBorderMovement)
					{
						if (!Declaration.US_SchDArrival.IsEmpty && Declaration.US_SchDArrival != Declaration.US_SchDEntry)
						{
							Declaration.US_ITDateInfo.AddMessageError(ITDateIsRequiredWhenEntryAndDischargePortDifferBCR);
						}
					}
					else if (!FormalImportJobDeclarationValidation.IsInSameDistrict(Declaration.US_SchDArrival, Declaration.US_SchDEntry))
					{
						Declaration.US_ITDateInfo.AddMessageError(ITDateIsRequiredWhenEntryAndDischargeDistrictDiffer);
					}
				}

				ValidateUS_CargoReleaseType();
			}

			if (Declaration.ITNumbersFromBills.Count > 0 && Declaration.US_ITDate.IsEmpty)
			{
				Declaration.US_ITDateInfo.AddMessageError(ITDateIsRequiredWhenITNumberIsEntered);
			}

			if (Declaration.US_ITDate.IsValid)
			{
				if (Declaration.JE_DateOfArrival.IsValid && Declaration.US_ITDate.Date < Declaration.JE_DateOfArrival.Date)
				{
					Declaration.US_ITDateInfo.AddMessageError(ITDateCannotBeforeArrivalDate);
				}

				if (Declaration.US_ITDate.Date < ZDateTime.Today.AddYears(-2))
				{
					Declaration.US_ITDateInfo.AddMessageError(ITDateCannotBeMoreThan2YearsInThePast);
				}
			}

			ValidateUS_IsInvoiceByRequest();
			Declaration.Validation.ValidateJE_ExportDate();
		}
		internal const string ITDateIsRequiredWhenITNumberIsEntered = "An IT date must be entered when an IT number is supplied.";
		internal const string ITDateIsRequiredWhenEntryAndDischargeDistrictDiffer = "An IT Date must be entered when Port of Discharge is not in the the same district as Port of Entry.";
		internal const string ITDateIsRequiredWhenEntryAndDischargePortDifferBCR = "An IT Date must be entered when Port of Discharge is different to Port of Entry for Border Crossing.";
		internal const string ITDateCannotBeforeArrivalDate = "IT Date may not be before Date of Arrival.";
		internal const string ITDateCannotBeMoreThan2YearsInThePast = "The IT Date entered is more than 2 years old.";

		static string GetStandardCargoReleaseType(string value)
		{
			var result = value;

			//ACE & SE are identical to Customs
			if (value == CargoReleaseTypeList.Codes.ACE || value == CargoReleaseTypeList.Codes.SE)
			{
				result = CargoReleaseTypeList.Codes.SE;
			}

			return result;
		}

		void CheckCargoReleaseTransactionExist()
		{
			if (Parent.IsInDatabase)
			{
				var oldValue = GetStandardCargoReleaseType(Parent.GetOriginalValue(USAddInfoSchema.US_CargoReleaseType).ToString());
				var currentValue = GetStandardCargoReleaseType(Parent.US_CargoReleaseType);

				if (oldValue != currentValue)
				{
					if (oldValue == CargoReleaseTypeList.Codes.BCR || oldValue == CargoReleaseTypeList.Codes.CR || oldValue == CargoReleaseTypeList.Codes.SE)
					{
						var entryMessageType = "";

						switch (oldValue)
						{
							case CargoReleaseTypeList.Codes.BCR:
								entryMessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;
								break;
							case CargoReleaseTypeList.Codes.CR:
								entryMessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
								break;
							case CargoReleaseTypeList.Codes.SE:
								entryMessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
								break;
							default:
								break;
						}

						CheckIfCustomsTransactionsExist(Parent.US_CargoReleaseTypeInfo, entryMessageType, CargoReleaseTypeCannotBeChanged);
					}
					else if (oldValue == CargoReleaseTypeList.Codes.ACS && (Declaration.HasCargoReleaseBeenCertified || Declaration.IsCargoReleaseBeingCertified))
					{
						Parent.US_CargoReleaseTypeInfo.AddMessageError(CargoReleaseTypeCannotBeChangedAfterCertified);
					}
				}
			}
		}

		protected override void CheckUS_CargoReleaseType()
		{
			base.CheckUS_CargoReleaseType();

			if (!Declaration.US_CargoReleaseType_ReadOnly && Parent.US_CargoReleaseType.IsEmpty && Declaration.JE_MessageType == JobMessageTypeList.Codes.Import)
			{
				Parent.US_CargoReleaseTypeInfo.AddError(CRLTypeIsMandatory);
				return;
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.US_CargoReleaseTypeInfo, Parent.Lookups.CargoReleaseTypeList);

			CheckCargoReleaseTransactionExist();

			var isBorderMovement = TransportTypeList.IsBorderTransportType(Declaration.JE_TransportMode);
			var bCRPorts = Declaration.BCRPortsFromRegistry;
			var entryPortIsInTheRegistry = bCRPorts.ContainsPortCode(Parent.US_SchDEntry);

			if (Parent.US_CargoReleaseType == CargoReleaseTypeList.Codes.BCR)
			{
				if (!Parent.US_ITDate.IsEmpty || Declaration.IsConsumptionFTZ || !isBorderMovement)
				{
					Parent.US_CargoReleaseTypeInfo.AddMessageError(BRCIsInvalid);
				}

				if (bCRPorts.Count > 0 && !entryPortIsInTheRegistry)
				{
					Parent.US_CargoReleaseTypeInfo.AddMessageError(BRCIsInvalidForPortOfEntry);
				}
			}
			else if (Parent.US_CargoReleaseType == CargoReleaseTypeList.Codes.CR)
			{
				if (isBorderMovement && bCRPorts.Count > 0 && entryPortIsInTheRegistry)
				{
					Parent.US_CargoReleaseTypeInfo.AddWarning(BCRMode);
				}
			}

			if (Parent.US_CargoReleaseType == CargoReleaseTypeList.Codes.ACS && Declaration.ShouldForceFilingInACECargoRelease())
			{
				Parent.US_CargoReleaseTypeInfo.AddMessageError(UnacceptableCargoReleaseType);
			}
		}

		internal const string ACECertificationModeMessageError = "ACE Cargo Release certification method does not accept OGAs or RLF entries. Please choose ACS.";
		internal const string CargoReleaseTypeCannotBeChangedAfterCertified = "Cargo Release Type cannot be changed after it has been certified in ACS or while it is being certified in ACS.";
		internal const string CargoReleaseTypeCannotBeChanged = "Cargo Release Type cannot be changed because Cargo Release transaction exists. Please send delete message first.";
		internal const string BRCIsInvalid = "Border Cargo Release mode is invalid if IT Date is entered or Entry Type is Consumption Foreign Trade Zone (FTZ) or transport mode is not 'Border Movement'.";
		internal const string BRCIsInvalidForPortOfEntry = "Border Cargo Release mode is invalid because Port Of Entry is not in the registry Border Cargo Release ports list.";
		internal const string CRLTypeIsMandatory = "Cargo Release Type is mandatory.";
		internal const string BCRMode = "According to the 'Border Cargo Release Ports' registry item, it should be BCR (Border Cargo Release) for the selected transport mode and entry port. Please review it.";
		internal const string UnacceptableCargoReleaseType = "ACE Cargo Release is required for the selected entry type.";

		protected override void CheckUS_TIBPurpose()
		{
			base.CheckUS_TIBPurpose();

			if (Declaration.IsTemporaryImportationBond && Parent.US_TIBPurpose.IsEmpty)
			{
				Parent.US_TIBPurposeInfo.AddWarning(TIBPurposeRequired);
			}
		}
		internal const string TIBPurposeRequired = "You have not entered a value, Purpose may be required by Customs.";

		protected override void CheckUS_DeferredTaxDueDate()
		{
			base.CheckUS_DeferredTaxDueDate();

			if (!Parent.US_DeferredTaxDueDate.IsEmpty)
			{
				new WeekendsAndHolidaysValidator().CheckWeekendsAndHolidays(Parent.US_DeferredTaxDueDateInfo);

				var generatedDate = new AddInfoJobDeclarationWorkingDate().GenerateDeferredTaxDueDate(Declaration);
				if (generatedDate != Parent.US_DeferredTaxDueDate)
				{
					Parent.US_DeferredTaxDueDateInfo.AddWarning(string.Format(DeferredTaxDueDateNotDefaulted, generatedDate.ToShortDateString()));
				}
			}

			ValidateUS_FixDefTaxDueDate();
		}
		internal const string DeferredTaxDueDateNotDefaulted = "The Deferred Tax Due Date differs from the calculated default value, ({0}). The Deferred Tax Due Date default value is calculated based on Registry and Importer settings and normally due the 14th and the 29th of the month with exceptions.";

		protected override void CheckUS_EnableSPN()
		{
			base.CheckUS_EnableSPN();

			if (!Declaration.IsACE && Parent.US_EnableSPN)
			{
				Parent.US_EnableSPNInfo.AddMessageError(ValidationConstants.PriorNotice.ACSPriorNoticeTurnedOff);
			}
		}

		protected override void CheckUS_UC_NKCountryOfExport()
		{
			base.CheckUS_UC_NKCountryOfExport();
			ListValidation.WarnIfInvalidCode(Parent.US_UC_NKCountryOfExportInfo, Declaration.AddInfoLookups.USCountryList);
		}

		protected override void CheckUS_FTZNo()
		{
			base.CheckUS_FTZNo();
			var dec = Declaration;
			if (IsEntrySummaryOrCargoReleaseValidationMode)
			{
				if (dec.IsConsumptionFTZ && !dec.IsACECargoCertificationAndFixedTransportRelevant)
				{
					if (dec.US_FTZNo.IsEmpty)
					{
						MandatoryValidation.MessageErrorIfNotEntered(dec.US_FTZNoInfo, ImportFTZNumberIsMandatory);
					}
					else
					{
						var ftzNumberIncorrectMessageError = GetImportFTZNumberMessageEorrIfInvalid(dec.US_FTZNo);
						if (!ftzNumberIncorrectMessageError.IsEmpty)
						{
							dec.US_FTZNoInfo.AddMessageError(ftzNumberIncorrectMessageError);
						}
					}
				}
			}
		}
		internal const string ImportFTZNumberIsMandatory = "FTZ Number. FTZ Number is mandatory when Entry Type is Consumption Foreign Trade Zone";

		protected virtual ZString GetImportFTZNumberMessageEorrIfInvalid(ZString ftzNumber)
		{
			var ftzNumberIncorrectMessageError = ZString.Empty;

			//Regular expression explanation: the first three characters must be â€œFTZâ€, followed by any number in the range of â€œ001â€ through â€œ300â€, inclusive.  No number outside this range will be accepted.  The last character can be any alpha (A-Z) or numeric (0-9) character. (from catair_all\EntrySummary.doc)
			var regex = new Regex(@"(^FTZ(([0-2][0-9][1-9])|([0-2][1-9][0-9])|([1-2][0-9]{2})|(300))\w$)", RegexOptions.IgnoreCase);
			if (!regex.IsMatch(ftzNumber))
			{
				ftzNumberIncorrectMessageError = ImportFTZNumberHasWrongFormat;
			}

			return ftzNumberIncorrectMessageError;
		}
		internal const string ImportFTZNumberHasWrongFormat = "Invalid FTZ Number format. The FTZ Number must be 7 characters long. The first three characters must be “FTZ”, followed by any three-digit number in the range of “001” through “300”, inclusive.  No number outside this range will be accepted.  The last character can be any alpha (A-Z) or numeric (0-9) character.";

		#region Boolean Flags

		protected bool IsExWarehouse
		{
			get { return Declaration.IsExWarehouse; }
		}

		protected internal bool IsEntrySummaryValidationMode
		{
			get { return Declaration.IsEntrySummaryValidationMode; }
		}

		protected internal bool IsEntrySummaryOrCargoReleaseValidationMode
		{
			get { return IsEntrySummaryValidationMode || IsCargoReleaseValidationMode; }
		}

		protected internal bool IsCargoReleaseValidationMode
		{
			get { return Declaration.IsCargoReleaseValidationMode; }
		}

		bool IsLegacyCargoRelease
		{
			get { return IsCargoReleaseValidationMode && !Declaration.IsACECargoCertificationMode; }
		}

		bool IsIndividualBasisPaymentInvalidForRLFEntry
		{
			get
			{
				var declaration = Declaration;
				return declaration.IsRemoteLocationFiling && declaration.US_EnableENS && (declaration.FormalEntry == null || !declaration.FormalEntry.HasBeenLodgedAtCustoms) && declaration.IsIndividualBasisPayment;
			}
		}

		#endregion
	}
}
