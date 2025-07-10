using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ReconAddInfoOriginalCusEntryHeaderValidation : AddInfoCusEntryHeaderValidation
	{
		public ReconAddInfoOriginalCusEntryHeaderValidation(AddInfoCusEntryHeader parent)
			: base(parent)
		{
		}

		new AddInfoCusEntryHeader Parent
		{
			get { return base.Parent; }
		}

		ReconDeclaration ReconciliationDecl
		{
			get { return Parent.EntryHeader.ReconOriginalEntry.ReconDeclaration; }
		}

		ReconOriginalEntryHeader OriginalEntryHeader
		{
			get { return Parent.EntryHeader.ReconOriginalEntry; }
		}

		bool ReconDeclHasBeenLodged
		{
			get { return ReconciliationDecl.CanSendWithdrawal; }
		}

		bool ReconIsClassification => ReconIssueCodeList.IsClassificationRecon(ReconciliationDecl.US_IssueCode);

		protected override void CheckUS_EntryType()
		{
			base.CheckUS_EntryType();

			if (!Parent.US_EntryType.IsEmpty && !EntryTypeList.IsValidForRecon(Parent.US_EntryType))
			{
				Parent.US_EntryTypeInfo.AddMessageError(EntryTypeInvalidForRecon);
			}
		}
		internal const string EntryTypeInvalidForRecon = "The selected entry type is not valid for reconciliation.";

		protected override void CheckUS_SchDEntry()
		{
			base.CheckUS_SchDEntry();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_SchDEntryInfo, "entry port");

			if (!Parent.US_SchDEntry.IsEmpty && !ReconciliationDecl.US_ImportEntrySource.IsEmpty)
			{
				string desc = Parent.Factory.GetCachedValue<ReconciliationImportEntrySourceList>().GetDescriptionFromCode(ReconciliationDecl.US_ImportEntrySource);

				ReconOriginalEntryHeader originalImportEntry = Parent.EntryHeader.ReconOriginalEntry;

				if (!string.IsNullOrEmpty(desc) && originalImportEntry.ImportSourceIndicator != ReconciliationDecl.US_ImportEntrySource)
				{
					Parent.US_SchDEntryInfo.AddMessageError(string.Format(SourceIndicatorWrong, ReconciliationDecl.US_ImportEntrySource, desc));
				}
			}
		}
		internal const string SourceIndicatorWrong = "You have indicated 'Import Entry Source' as {0}({1}) above, however this port is not one of them.";

		protected override void CheckUS_ImportDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckUS_ImportDate()
		{
			base.CheckUS_ImportDate();

			ReconDeclaration reconciliationDecl = ReconciliationDecl;
			if (reconciliationDecl.US_IssueCode == ReconIssueCodeList.Codes.FTA)
			{
				if (!Parent.US_ImportDate.IsValid)
				{
					Parent.US_ImportDateInfo.AddMessageError(ImportDateRequiredForNonFTARecon);
				}
				else if (!Parent.US_ImportDate.IsInThePastDatePartOnly)
				{
					Parent.US_ImportDateInfo.AddMessageError(ThisDateShouldBeAPastDate);
				}
				else if (Parent.US_ImportDate < ZDateTime.Today.AddYears(-1))
				{
					Parent.US_ImportDateInfo.AddMessageError(ImportDateCannotBeMore1YearForFTARecon);
				}
			}

			ValidateUS_R_ReleaseDate();
		}
		internal const string ImportDateRequiredForNonFTARecon = "Importation date is required for the selected issue code.";
		internal const string ThisDateShouldBeAPastDate = "This date should be a past date.";
		internal const string ImportDateCannotBeMore1YearForFTARecon = "A FTA claim cannot be made more than 1 year after the Import Date.";

		protected override void CheckUS_R_ReleaseDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckUS_R_ReleaseDate()
		{
			base.CheckUS_R_ReleaseDate();
			ReconDeclaration reconciliationDecl = ReconciliationDecl;
			if (reconciliationDecl.US_IssueCode != ReconIssueCodeList.Codes.FTA)
			{
				if (!Parent.US_R_ReleaseDate.IsValid)
				{
					Parent.US_R_ReleaseDateInfo.AddMessageError(EntrySummaryDateRequiredForNonFTARecon);
				}
				else if (!Parent.US_R_ReleaseDate.IsInThePastDatePartOnly)
				{
					Parent.US_R_ReleaseDateInfo.AddMessageError(ThisDateShouldBeAPastDate);
				}
			}

			if (Parent.US_R_ReleaseDate.IsValid)
			{
				if (Parent.US_ImportDate.IsValid && Parent.US_R_ReleaseDate < Parent.US_ImportDate.AddDays(-5))
				{
					Parent.US_R_ReleaseDateInfo.AddMessageError(EntryDateShouldNotBeEarlierThan5DaysFromImportDate);
				}
			}
			ValidateUS_PaymentDate();
			ValidateUS_R_DutyRateDate();
		}
		internal const string EntrySummaryDateRequiredForNonFTARecon = "Entry date is required for the selected issue code.";
		internal const string EntryDateShouldNotBeEarlierThan5DaysFromImportDate = "Entry Date should not be earlier than 5 days from Import Date.";

		protected override void CheckUS_R_DateForMPFCalcIsValidZDateTimeRange()
		{
		}

		protected override void CheckUS_R_DateForMPFCalc()
		{
			base.CheckUS_R_DateForMPFCalc();

			var reconciliationDecl = ReconciliationDecl;
			if (Parent.US_R_DateForMPFCalc.IsEmpty && !reconciliationDecl.US_R_IsNoChangeAgg)
			{
				Parent.US_R_DateForMPFCalcInfo.AddMessageError(MPFDutyRateDateIsRequired);
			}
		}
		internal const string MPFDutyRateDateIsRequired = "The MPF Duty Rate Date is required to calculate MPF duty based on that of the original import entry.";

		protected override void CheckUS_R_DutyRateDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckUS_R_DutyRateDate()
		{
			base.CheckUS_R_DutyRateDate();

			var reconciliationDecl = ReconciliationDecl;
			if (Parent.US_R_DutyRateDate.IsEmpty && !reconciliationDecl.US_R_IsNoChangeAgg)
			{
				Parent.US_R_DutyRateDateInfo.AddMessageError(DutyRateDateIsRequired);
			}

			if (Parent.US_R_DutyRateDate.IsValid && Parent.US_R_ReleaseDate.IsValid && Parent.US_R_DutyRateDate != Parent.US_R_ReleaseDate)
			{
				Parent.US_R_DutyRateDateInfo.AddWarning(DutyRateDateNotMatchEntryDate);
			}
		}
		internal const string DutyRateDateIsRequired = "The Duty Rate Date is required to calculate duties and fees based on that of the original import entry.";
		internal const string DutyRateDateNotMatchEntryDate = "Duty Rate Date does not match Entry Date.";

		protected override void CheckUS_R_IsHMFApplicable()
		{
			base.CheckUS_R_IsHMFApplicable();

			if (Parent.EntryHeader.ReconOriginalEntry != null && !Parent.EntryHeader.ReconOriginalEntry.US_R_NoLineDetails && Parent.EntryHeader.ReconOriginalEntry.Invoice.JobComInvoiceLines.Count > 0)
			{
				if (Parent.US_R_IsHMFApplicable.IsEmpty)
				{
					Parent.US_R_IsHMFApplicableInfo.AddMessageError(IsHMFApplicableRequired);
				}
				else if (!Parent.EntryHeader.AddInfoLookups.US_YesNoList.ContainsCode(Parent.US_R_IsHMFApplicable))
				{
					Parent.US_R_IsHMFApplicableInfo.AddMessageError(InvalidHMFApplicable);
				}
			}
		}
		internal const string IsHMFApplicableRequired = "HMF Applicable indicator is required so that the system can determine whether to calculate HMF or not for the entry";
		internal const string InvalidHMFApplicable = "HMF Applicable indicator is not in the list.";

		protected override void CheckUS_PaymentDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckUS_PaymentDate()
		{
			base.CheckUS_PaymentDate();

			ReconOriginalEntryHeader originalImportEntry = Parent.EntryHeader.ReconOriginalEntry;

			if (originalImportEntry != null)
			{
				if (Parent.US_R_ReleaseDate.IsValid && Parent.US_PaymentDate.IsValid && Parent.US_PaymentDate < Parent.US_R_ReleaseDate)
				{
					Parent.US_PaymentDateInfo.AddMessageError(PaymentDateShouldBeGreaterThanOrEqualToEntryDate);
				}

				var tradeAgreementIssueCode = originalImportEntry.ReconDeclaration.US_IssueCode == ReconIssueCodeList.Codes.FTA;
				if (Parent.US_PaymentDate.IsEmpty && !tradeAgreementIssueCode)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PaymentDateInfo);
				}
				else
				{
					if (Parent.US_PaymentDate.IsValid && !tradeAgreementIssueCode)
					{
						if (originalImportEntry.ReconDeclaration.ReconPaymentDate > Parent.US_PaymentDate.AddMonths(21))
						{
							Parent.US_PaymentDateInfo.AddMessageError(PaymentDateLate);
						}
					}

					if (originalImportEntry.ShouldCalculateInterestAmount && !HasInterestRatesFor(originalImportEntry.US_PaymentDate.Date, originalImportEntry.ReconDeclaration.ReconPaymentDate))
					{
						Parent.US_PaymentDateInfo.AddMessageError(string.Format(NotAllInterestRatesExistFromThisDate, originalImportEntry.US_PaymentDate.Date.ToShortDateString(), originalImportEntry.ReconDeclaration.ReconPaymentDate.ToShortDateString()));
					}
				}
			}

			if (!ReconciliationDecl.IsACE && Parent.US_PaymentDate.IsValid && ReconDeclHasBeenLodged && Parent.US_PaymentDate < ReconciliationDecl.US_R_EntrySumDateLodged)
			{
				Parent.US_PaymentDateInfo.AddMessageError(string.Format(PaymentDateEarlierEntrySumDateLodgedDate, ReconciliationDecl.US_R_EntrySumDateLodged));
			}
		}
		internal const string PaymentDateEarlierEntrySumDateLodgedDate = "The Earliest Payment Date sent in ADD was '{0}' and this cannot be changed to an earlier date than that.";

		internal const string PaymentDateShouldBeGreaterThanOrEqualToEntryDate = "Payment Date should be greater than or equal to Entry Date.";
		internal const string PaymentDateLate = "This payment date indicates that the Recon will incurr late file penalties. Payment date should be within 21 months of statement print date (or estimated recon date if no statement print date is specified).";
		internal const string NotAllInterestRatesExistFromThisDate = "System needs interest rates from '{0}' to '{1}' for interest calculation. Please make sure you have entered interest rates for all quarters in Registry > Customs > United States of America > Import > ABI > Recon. Interest Rates.";

		bool HasInterestRatesFor(ZDate originalPayDate, ZDate reconPayDate)
		{
			ZDate startDate = originalPayDate;

			while (startDate < reconPayDate)
			{
				ZDate endOfQuarter = ReconInterestCalculator.GetEndDateOfAQuater(startDate);

				if (ReconInterestRateRetriever.GetRate(startDate).IsEmpty)
				{
					return false;
				}

				ZDate endDate = endOfQuarter < reconPayDate ? endOfQuarter : reconPayDate;

				startDate = endDate.AddDays(1);
			}

			return true;
		}

		protected override void CheckUS_R_MsgMode()
		{
			base.CheckUS_R_MsgMode();

			if (Parent.US_R_MsgMode.IsEmpty)
			{
				Parent.US_R_MsgModeInfo.AddMessageError(PleaseEnterMessagingMode);
			}
			else
			{
				var originalImportEntry = Parent.EntryHeader.ReconOriginalEntry;

				ListValidation.MessageErrorIfInvalidCode(Parent.US_R_MsgModeInfo, originalImportEntry.Lookups.MessagingModeList);
			}
		}
		internal const string PleaseEnterMessagingMode = "Please enter ABI messaging mode.";

		protected override void CheckUS_ProtestID()
		{
			base.CheckUS_ProtestID();
			if (ReconciliationDecl != null && ReconciliationDecl.IsACE && OriginalEntryHeader.US_ProtestStat)
			{
				MandatoryValidation.MessageErrorIfNotEntered(OriginalEntryHeader.US_ProtestIDInfo);
			}
		}

		protected override void CheckUS_PendingActionIDType()
		{
			base.CheckUS_PendingActionIDType();
			CheckPendingActionID(OriginalEntryHeader.US_PendingActionIDTypeInfo);
			ListValidation.MessageErrorIfInvalidCode(OriginalEntryHeader.US_PendingActionIDTypeInfo);
		}

		protected override void CheckUS_PendingActionID()
		{
			base.CheckUS_PendingActionID();
			CheckPendingActionID(OriginalEntryHeader.US_PendingActionIDInfo);
		}

		void CheckPendingActionID(ZPropertyInfo pendingActionInfo)
		{
			if (ReconciliationDecl != null && ReconciliationDecl.IsACE)
			{
				if (!ReconciliationDecl.US_IssueCode.IsEmpty && !ReconIsClassification && !pendingActionInfo.Value.IsEmpty)
				{
					pendingActionInfo.AddMessageError(FieldIsNotAllowed("Pending Action ID"));
				}
				else if (ReconIsClassification)
				{
					MandatoryValidation.MessageErrorIfNotEntered(pendingActionInfo);
				}
			}
		}

		protected override void CheckUS_NAFTAClaimStat()
		{
			var reconDec = ReconciliationDecl;
			var originalEntryHeader = OriginalEntryHeader;
			base.CheckUS_NAFTAClaimStat();
			if (reconDec != null && reconDec.IsACE && reconDec.US_IssueCode != ReconIssueCodeList.Codes.FTA && originalEntryHeader != null && originalEntryHeader.US_NAFTAClaimStat)
			{
				OriginalEntryHeader.US_NAFTAClaimStatInfo.AddMessageError(FieldIsNotAllowed("NAFTA 303?"));
			}
		}

		protected override void CheckUS_R_OrigCV()
		{
			base.CheckUS_R_OrigCV();
			var originalEntryHeader = OriginalEntryHeader;
			if (!originalEntryHeader.US_R_OrigCV_ReadOnly && originalEntryHeader.US_R_OrigCV <= 0)
			{
				originalEntryHeader.US_R_OrigCVInfo.AddMessageError(EnterNumberGreaterThanZero);
			}
		}
		const string EnterNumberGreaterThanZero = "Please enter an original Customs value for HMF calculation. This is required as this entry has only changed lines entered.";

		protected override void CheckUS_ProtestStat()
		{
			var reconDec = ReconciliationDecl;
			var originalEntryHeader = OriginalEntryHeader;
			base.CheckUS_ProtestStat();
			if (reconDec != null && reconDec.IsACE && originalEntryHeader != null)
			{
				if (reconDec.US_IssueCode != ReconIssueCodeList.Codes.FTA && originalEntryHeader.US_ProtestStat)
				{
					OriginalEntryHeader.US_ProtestStatInfo.AddMessageError(FieldIsNotAllowed("Protest Filed?"));
				}
				if (!originalEntryHeader.US_ProtestID.IsEmpty && !originalEntryHeader.US_ProtestStat)
				{
					OriginalEntryHeader.US_ProtestStatInfo.AddMessageError(protestFiledMustBeTicked);
				}
			}
		}

		public const string protestFiledMustBeTicked = "Protest Filed? must be ticked when protest ID is entered.";

		static public ZString FieldIsNotAllowed(ZString fieldName)
		{
			return string.Format(CultureInfo.CurrentCulture, "{0} is not allowed for the selected reconciliation issue.", fieldName);
		}

		protected override void CheckUS_NAFTAReconIndicator()
		{
			var reconDec = ReconciliationDecl;
			var originalEntryHeader = OriginalEntryHeader;
			base.CheckUS_NAFTAReconIndicator();
			if (originalEntryHeader.US_NAFTAReconIndicator && reconDec.US_IssueCode == ReconIssueCodeList.Codes.FTA)
			{
				originalEntryHeader.US_NAFTAReconIndicatorInfo.AddMessageError(FTAReconFiledFlagCannotBeYes);
			}
		}
		internal const string FTAReconFiledFlagCannotBeYes = "FTA Recon Filed flag cannot be Yes when Recon Type is a Free Trade Recon";
	}
}
