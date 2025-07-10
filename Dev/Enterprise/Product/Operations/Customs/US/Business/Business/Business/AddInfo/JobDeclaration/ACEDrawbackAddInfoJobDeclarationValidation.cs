using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ACEDrawbackAddInfoJobDeclarationValidation : CommonDrawbackAddInfoJobDeclarationValidation
	{
		public ACEDrawbackAddInfoJobDeclarationValidation(AddInfoJobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override ZBool IsBondTypeRequired
		{
			get { return Parent.US_AcceleratedClaimInd; }
		}

		protected override ZString BondTypeRequiredMessageError
		{
			get { return ACEBondTypeRequiredMessage; }
		}
		internal const string ACEBondTypeRequiredMessage = "A Bond Type is required if Accelerated Drawback is claimed.";

		protected override void CheckUS_BondType()
		{
			base.CheckUS_BondType();

			ValidateUS_BondSuperseding();
			ValidateUS_BondAmount();
			ValidateUS_BondProducerAccNo();
			ValidateUS_BondDesignationCode();
		}

		protected override ZBool BondTypeRequired
		{
			get { return Parent.US_AcceleratedClaimInd; }
		}

		protected override void CheckUS_AcceleratedClaimInd()
		{
			base.CheckUS_AcceleratedClaimInd();

			if (Parent.US_AcceleratedClaimInd)
			{
				Parent.US_AcceleratedClaimIndInfo.AddWarning(OtherFeeWillNotBeCalculatedOrClaimed);
			}

			ValidateUS_BondType();
		}
		internal const string OtherFeeWillNotBeCalculatedOrClaimed = "Other Fees will not calculate or claimed on the drawback if the Accelerated Payment Indicator is ticked, as they are not eligible for this program.";

		protected override void CheckUS_DRWOneTimeWaiverInd()
		{
			base.CheckUS_DRWOneTimeWaiverInd();
			var parent = Parent;
			if (parent.US_DRWOneTimeWaiverInd)
			{
				parent.US_DRWOneTimeWaiverIndInfo.AddWarning(BeSureProvideOTWDataToCBP);
			}
			if (ACEDrawbackProvisionsList.IsApplicableProvisionsForOneTimeWaiverInd(parent.US_EntryType) && !parent.US_DRWExamWitness && !parent.US_DRWOneTimeWaiverInd && !parent.US_WaiverNoticeInd)
			{
				parent.US_DRWOneTimeWaiverIndInfo.AddMessageError(WaiverIndMessageError);
			}
		}
		internal const string WaiverIndMessageError = "If claim is missing either One time Waiver indicator or Waiver Notice Indicator, claim is considered incomplete. At least one of the tick boxes must be ticked.";
		internal const string BeSureProvideOTWDataToCBP = "Be sure to provide the date of the OTW application submission to CBP on company letterhead and upload it to DIS.";

		protected override void CheckUS_WaiverNoticeInd()
		{
			base.CheckUS_WaiverNoticeInd();
			ValidateUS_DRWOneTimeWaiverInd();
		}

		protected override void CheckUS_BondWaiverCode()
		{
			base.CheckUS_BondWaiverCode();

			if (!Parent.US_BondWaiverCode.IsEmpty && !Parent.US_BondType.IsEmpty)
			{
				if (Parent.US_BondType != BondTypeList.Codes.NoBondRequired)
				{
					Parent.US_BondWaiverCodeInfo.AddMessageError(BondWaiverCodeNotForNoBondRequired);
				}
			}
		}
		internal const string BondWaiverCodeNotForNoBondRequired = "Bond is indicated as waived while bond type is not '0'(Bond Waived/No Bond Required).";

		protected override void CheckUS_BondAmount()
		{
			base.CheckUS_BondAmount();
			if (Declaration.US_BondType != BondTypeList.Codes.SingleTransactionBond && Declaration.US_BondAmount > 0)
			{
				Declaration.US_BondAmountInfo.AddMessageError(NotRelevantForContinuousBond);
			}
			else if (Declaration.US_BondType == BondTypeList.Codes.SingleTransactionBond && Declaration.US_BondAmount == 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.US_BondAmountInfo);
			}

			ValidateUS_BondType();
		}

		protected override void CheckUS_BondProducerAccNo()
		{
			base.CheckUS_BondProducerAccNo();
			if (Declaration.US_BondType == BondTypeList.Codes.SingleTransactionBond)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.US_BondProducerAccNoInfo);
			}

			ValidateUS_BondType();
		}
		internal const string NotRelevantForContinuousBond = "This is only relevant for a single transaction bond type.";

		protected override void CheckUS_BondDesignationCode()
		{
			base.CheckUS_BondDesignationCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_BondDesignationCodeInfo, Parent.Lookups.US_BondDesignationCodeList);

			if (Parent.US_BondType == BondTypeList.Codes.SingleTransactionBond)
			{
				if (Parent.US_BondDesignationCode.IsEmpty)
				{
					Parent.US_BondDesignationCodeInfo.AddMessageError(DesignationCodeRequired);
				}
			}
		}
		internal const string DesignationCodeRequired = "Please enter a designation code.";

		protected override void CheckUS_SuretyCode()
		{
			base.CheckUS_SuretyCode();
			if (!Parent.US_BondType.IsEmpty && Parent.US_BondType != BondTypeList.Codes.NoBondRequired)
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
			}
		}

		protected override void CheckUS_DRWExamName()
		{
			base.CheckUS_DRWExamName();

			var parent = Parent;
			if (IsExaminationWitnessWaived && !parent.US_DRWExamName.IsEmpty)
			{
				parent.US_DRWExamNameInfo.AddMessageError(ExaminationInformationShouldNotBeEntered);
			}
			else if (IsExaminationRequired && !IsExaminationWitnessWaived)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.US_DRWExamNameInfo);
			}

			ValidateUS_DRWExamBadge();
			ValidateUS_DRWExamPhone();
			ValidateUS_DRWExamDate();
		}
		internal const string FieldRequiredIfEitherNameOrBadgeOrPhoneOrDateEntered = "This is required when either Name or Badge No. or Phone No. or Date is entered.";
		internal const string ExaminationInformationShouldNotBeEntered = "Examination Information should not be entered when Exam is waived.";

		protected override void CheckUS_DRWExamBadge()
		{
			base.CheckUS_DRWExamBadge();

			var parent = Parent;
			if (IsExaminationWitnessWaived && !parent.US_DRWExamBadge.IsEmpty)
			{
				parent.US_DRWExamBadgeInfo.AddMessageError(ExaminationInformationShouldNotBeEntered);
			}
			else if (IsExaminationRequired && !IsExaminationWitnessWaived)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.US_DRWExamBadgeInfo);
			}
		}

		protected override void CheckUS_DRWExamPhone()
		{
			base.CheckUS_DRWExamPhone();

			var parent = Parent;
			if (IsExaminationWitnessWaived && !parent.US_DRWExamPhone.IsEmpty)
			{
				parent.US_DRWExamPhoneInfo.AddMessageError(ExaminationInformationShouldNotBeEntered);
			}
			else if (IsExaminationRequired && !IsExaminationWitnessWaived)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.US_DRWExamPhoneInfo);
			}
		}

		protected override void CheckUS_DRWExamDate()
		{
			base.CheckUS_DRWExamDate();

			var parent = Parent;
			if (IsExaminationWitnessWaived && !parent.US_DRWExamDate.IsEmpty)
			{
				parent.US_DRWExamDateInfo.AddMessageError(ExaminationInformationShouldNotBeEntered);
			}
			else if (IsExaminationRequired && !IsExaminationWitnessWaived)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.US_DRWExamDateInfo);
			}
		}

		protected override void CheckUS_DRWProcName()
		{
			base.CheckUS_DRWProcName();

			if (IsProcessorRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWProcNameInfo);
			}

			ValidateUS_DRWProcBadge();
			ValidateUS_DRWProcPhone();
			ValidateUS_DRWProcDate();
		}

		protected override void CheckUS_DRWProcBadge()
		{
			base.CheckUS_DRWProcBadge();

			if (IsProcessorRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWProcBadgeInfo);
			}
		}

		protected override void CheckUS_DRWProcPhone()
		{
			base.CheckUS_DRWProcPhone();

			if (IsProcessorRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWProcPhoneInfo);
			}
		}

		protected override void CheckUS_DRWProcDate()
		{
			base.CheckUS_DRWProcDate();

			if (IsProcessorRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWProcDateInfo);
			}
		}

		protected override void CheckUS_PreparerDistrictPort()
		{
			base.CheckUS_PreparerDistrictPort();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_PreparerDistrictPortInfo, Parent.Lookups.ACEDrawbackProcessingPortCodeList);
		}

		protected override void CheckUS_DRWLocatOfDest()
		{
			base.CheckUS_DRWLocatOfDest();

			ValidateUS_DRWProcName();
		}

		protected override void CheckUS_DRWIntendedPortOfExport()
		{
			base.CheckUS_DRWIntendedPortOfExport();

			ValidateUS_DRWProcName();
		}

		protected override void CheckUS_DRWExamWitness()
		{
			base.CheckUS_DRWExamWitness();
			ValidateUS_DRWOneTimeWaiverInd();
			ValidateUS_DRWProcName();
			ValidateUS_DRWExamName();
			ValidateUS_DRWDestructionResult();
		}

		protected override void CheckUS_DRWCommRuling()
		{
			base.CheckUS_DRWCommRuling();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DRWCommRulingInfo, Parent.Declaration.Lookups.CommercialRulingCodeList);

			if (!Parent.US_DRWCommRuling.IsEmpty && Parent.Declaration.IsDrawbackTFTEA)
			{
				Parent.US_DRWCommRulingInfo.AddMessageError(CommercialRulingNotAllowedForTFTEA);
			}
		}
		internal const string CommercialRulingNotAllowedForTFTEA = "Commercial Ruling is not allowed for Drawback Provision 51-77.";

		protected override void CheckUS_DRWUnUsedWine()
		{
			base.CheckUS_DRWUnUsedWine();

			if (!Parent.US_DRWUnUsedWine && Parent.US_EntryType == ACEDrawbackProvisionsList.Codes._74)
			{
				Parent.US_DRWUnUsedWineInfo.AddMessageError(SubstitutedUnusedWineCertificationRequired);
			}
		}
		internal const string SubstitutedUnusedWineCertificationRequired = "Substituted Unused Wine Certification is required when Drawback Provision is 74.";

		protected override void CheckUS_DRWBillOfFormula()
		{
			base.CheckUS_DRWBillOfFormula();

			if (!Parent.US_DRWBillOfFormula && new ZString("51,52,57,61,62,71,72,75").OccurrencesIgnoringCase(Parent.US_EntryType) == 1)
			{
				Parent.US_DRWBillOfFormulaInfo.AddMessageError(BillofMaterialsCertificationRequired);
			}
		}
		internal const string BillofMaterialsCertificationRequired = "Bill of Materials/Formula Certification is required when Drawback Provision is 51, 52, 57, 61, 62, 71, 72, 75.";

		protected override void CheckUS_DRWDestroyedValuation()
		{
			base.CheckUS_DRWDestroyedValuation();

			if (Parent.US_DRWDestroyedValuation && (Parent.US_EntryType.IsEmpty || new ZString("53,54,55,56,58,59,73,74,75,76,77").OccurrencesIgnoringCase(Parent.US_EntryType) == 0))
			{
				Parent.US_DRWDestroyedValuationInfo.AddMessageError(DestroyedValuationCertificationRequired);
			}
		}
		internal const string DestroyedValuationCertificationRequired = "Certification for Valuation of Destroyed Merchandise is conditional & only allowed when Drawback Provision is 53-56,58-59,73-77.";

		protected override void CheckUS_DRWDestructionResult()
		{
			base.CheckUS_DRWDestructionResult();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DRWDestructionResultInfo, Parent.Lookups.DestructionResultCodesList);

			if (Parent.US_DRWExamWitness && Parent.US_DRWDestructionResult.IsEmpty)
			{
				Parent.US_DRWDestructionResultInfo.AddMessageError(ExaminationResultRequired);
			}
			else if (!Parent.US_DRWExamWitness && !Parent.US_DRWDestructionResult.IsEmpty)
			{
				Parent.US_DRWDestructionResultInfo.AddMessageError(ExaminationResultNotRequired);
			}
		}
		internal const string ExaminationResultRequired = "Results Of Examination is required when Notice of Intent Indicator is ticked.";
		internal const string ExaminationResultNotRequired = "Result of Examination Indicator cannot be entered unless Notice of Intent Indicator is ticked.";

		bool IsExaminationRequired
		{
			get { return !Parent.US_DRWExamName.IsEmpty || !Parent.US_DRWExamBadge.IsEmpty || !Parent.US_DRWExamPhone.IsEmpty || !Parent.US_DRWExamDate.IsEmpty || Parent.Declaration.US_DRWExamWitness; }
		}

		bool IsProcessorRequired
		{
			get
			{
				return !Parent.US_DRWProcName.IsEmpty || !Parent.US_DRWProcBadge.IsEmpty || !Parent.US_DRWProcPhone.IsEmpty || !Parent.US_DRWProcDate.IsEmpty
						|| !Parent.Declaration.US_DRWIntendedPortOfExport.IsEmpty || Parent.Declaration.US_DRWExamWitness || !Parent.Declaration.US_DRWLocatOfDest.IsEmpty;
			}
		}

		bool IsExaminationWitnessWaived => Parent.US_DRWDestructionResult == DrawbackDestructionResultCodes.Codes.Waived;
	}
}
