using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionHeaderCriticalValidation : CriticalValidation<AccTransactionHeader>
	{
		public AccTransactionHeaderCriticalValidation(AccTransactionHeader parent)
			: base(parent)
		{
		}

		#region Check Grouping Methods

		protected override IEnumerable<CriticalValidationResult> OnSavingOnlyCriticalChecks()
		{
			foreach (var result in base.OnSavingOnlyCriticalChecks())
			{
				yield return result;
			}

			if (Parent.HasContext(BusinessContext.SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged))
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.TransactionHeaderWasCriticallyChangedByDataRefreshBus_2,
													CriticalValidationMessageTemplate.TransactionHeaderWasCriticallyChangedByDataRefreshBus,
													(NoResString)"Standard validation must catch this case and doesn't not allow to save.",
													Parent.GetTransactionHeaderInfo());
			}

			yield return CheckIsAllowToCreateTransaction();
			yield return CheckTransactionNumber();
			yield return CheckTransactionHeaderAmountExceedMaximumAllowedAmount();
			yield return CheckIsEditedAfterBeingSaved();
			yield return CheckOutstandingAmountIsValid();
			yield return CheckOSOutstandingAmountIsValid();
			yield return CheckOutstandingAmountIsZeroOnMatchingMiscTransactions();
			yield return CheckClearedDateIsEmptyForReceiptsWithNoBatchNumber();
			yield return CheckDueDateSetOnAutoOrReverseGLJournal();
			yield return CheckLinesOfBankTranferBalanceToZero();
			yield return CheckCancelledTransactionHeaderWithNoLineLinkedToCharge();
			yield return CheckTransactionHeaderLedgerIsCompatibleWithTransactionHeaderType();
			yield return CheckAmountIsEditedAfterBeingSaved();
			yield return CheckChangePostDateOfAPostedInvoice();
			yield return CheckTransactionHasLines();
			yield return CheckINTransactionHasPostedLines();
			yield return CheckExchangeRate();
			yield return CheckIsAllowedToCreateCreditNotes();
			yield return CheckGeneralLedgerAccount();
			yield return CheckInvoiceDate();
			yield return CheckGLJournalEntriesNumberHasBeenAssigned();
		}

		static bool IsChequeDirectPayment(AccTransactionHeader parent)
		{
			return parent.AH_Ledger == LedgerTypes.CashBook && parent.AH_TransactionType == TransactionTypes.DirectPayment && parent.AH_ReceiptType == ReceiptTypes.Cheque;
		}

		protected CriticalValidationResult GetCriticalValidationResultForHeader(CriticalValidationErrorType errorType, ResourceString message, params string[] extraMessages)
		{
			StringBuilder msg = new StringBuilder();
			msg.Append(Parent.GetTransactionHeaderInfo());

			if (extraMessages.Length > 0)
			{
				foreach (var extraMessage in extraMessages)
				{
					msg.Append(extraMessage);
				}
			}

			return new CriticalValidationResult(errorType, message, msg.ToString());
		}

		#endregion

		#region OnSaving Check methods

		CriticalValidationResult CheckTransactionHeaderAmountExceedMaximumAllowedAmount()
		{
			if ((!Parent.IsInDatabase || Parent.AH_InvoiceAmountInfo.HasChanges || Parent.AH_GSTAmountInfo.HasChanges) && !Parent.IsCancelled)
			{
				var message = CriticalValidationHelpers.GetAmountGreaterThanMaximumAllowedAmountMessage(Parent.AH_GC, CriticalValidationHelpers.MaximumAmountLevel.Header, Parent.AH_InvoiceAmount, Parent.AH_GSTAmount);
				if (message != null)
				{
					return new CriticalValidationResult(CriticalValidationErrorType.TransactionHeaderAmountExceedMaximumAllowedAmount, message);
				}
			}

			return null;
		}

		CriticalValidationResult CheckIsAllowToCreateTransaction()
		{
			CriticalValidationResult result = null;

			if (!ObjectFactory.Get<ITransactionCreationRestrictionHelper>().AllowToCreateTransaction(Parent, out ResourceString errorMessage))
			{
				result = new CriticalValidationResult(CriticalValidationErrorType.CreateTransactionRestriction, errorMessage);
			}

			return result;
		}

		CriticalValidationResult CheckOSOutstandingAmountIsValid()
		{
			if (!AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.Value || IsOutstandingAmountUpdatedByCashAdvanceReceiptWithoutMatchLink())
			{
				return null;
			}

			var result = new OutstandingAmountValidationProvider(Parent).ValidateOverseas();
			if (!string.IsNullOrEmpty(result))
			{
				return GetCriticalValidationResultForHeader(CriticalValidationErrorType.TransactionHeaderIncorrectOSOutstandingAmount,
															CriticalValidationMessageTemplate.TransactionHeaderIncorrectOSOutstandingAmountErrorMessage,
															result);
			}

			return null;
		}

		CriticalValidationResult CheckOutstandingAmountIsValid()
		{
			if (IsOutstandingAmountUpdatedByCashAdvanceReceiptWithoutMatchLink())
			{
				return null;
			}

			var result = new OutstandingAmountValidationProvider(Parent).ValidateLocal();
			if (!string.IsNullOrEmpty(result))
			{
				var matchLinkDeleteFailedMessage = string.Empty;
				var outstandingAmountLastSetMessage = string.Empty;
				var additionLogForSignError = string.Empty;
				var additionLogForInvoiceBatch = string.Empty;
				var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
				if (collectorService != null)
				{
					matchLinkDeleteFailedMessage = collectorService.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.MatchLinkDeletionFailed);
					outstandingAmountLastSetMessage = collectorService.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.AH_OutstandingAmountLastSet);
					additionLogForSignError = collectorService.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionHeaderDifferentSignBetweenOutstandingAmountAndInvoiceAmount);
					additionLogForInvoiceBatch = collectorService.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.InvoiceBatchOnSavingDetailInfo);
				}

				return GetCriticalValidationResultForHeader(CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12,
															CriticalValidationMessageTemplate.TransactionHeaderIncorrectOutstandingAmountErrorMessage,
															result,
															matchLinkDeleteFailedMessage,
															outstandingAmountLastSetMessage,
															additionLogForInvoiceBatch,
															additionLogForSignError
															);
			}

			return null;
		}

		bool IsOutstandingAmountUpdatedByCashAdvanceReceiptWithoutMatchLink()
		{
			var result = false;
			if (Parent.AH_TransactionType == TransactionTypes.Invoice && !Parent.AH_IsCancelled)
			{
				var cashAdvanceFunctionalityChecker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
				result = Parent.AH_Ledger == LedgerTypes.AccountsReceivable &&
						 cashAdvanceFunctionalityChecker.IsReceivablesCashAdvanceFunctionalityEnabled &&
						 cashAdvanceFunctionalityChecker.IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed;

				result |= Parent.AH_Ledger == LedgerTypes.AccountsPayable &&
						 cashAdvanceFunctionalityChecker.IsPayablesCashAdvanceFunctionalityEnabled &&
						 cashAdvanceFunctionalityChecker.IsManualSettingOfPayablesCashAdvanceRequestStatusToPaidAllowed;

				if (result)
				{
					result = HasInvoicedCashAdvanceRequestLinesInLocalCache();
					if (!result && Parent.IsInDatabase)
					{
						return HasInvoicedCashAdvanceRequestLinesInDatabase();
					}
				}
			}
			return result;

			bool HasInvoicedCashAdvanceRequestLinesInLocalCache()
			{
				var lineQuery = new ZQuery(AccTransactionLinesSchema.AL_GC, Parent.AH_GC) { FetchOnlyFromLocalCache = true };
				lineQuery.AddToFilter(AccTransactionLinesSchema.AL_AH, Parent.PK);
				var lines = Parent.Factory.Load<AccTransactionLines>(lineQuery);
				if (lines.Any())
				{
					var linePKs = lines.Select(l => l.PK);
					var charges = new List<JobCharge>();
					foreach (var partialLinkPKs in linePKs.Chunk(1000))
					{
						if (Parent.AH_Ledger == LedgerTypes.AccountsReceivable)
						{
							var chargeQuery = new ZQuery(JobChargeSchema.JR_AL_ARLine, value: partialLinkPKs.ToArray());
							chargeQuery.AddToFilter(JobChargeSchema.JR_IsARCashAdvance, true);
							chargeQuery.AddToFilter(JobChargeSchema.JR_CAL_ARLine, SQLComparisonOperator.NotEqual, ZGuid.Empty);
							charges.AddRange(Parent.Factory.Load<JobCharge>(chargeQuery));
							if (charges.Any())
							{
								var calPKs = charges.Select(c => c.JR_CAL_ARLine);
								var calQuery = new ZQuery(AccCashAdvanceRequestLineSchema.PK, calPKs.ToArray());
								calQuery.AddToFilter(AccCashAdvanceRequestLineSchema.CAL_Status, CashAdvanceStatusCodes.RequestLine.Invoiced);
								var exists = Parent.Factory.Exists(typeof(AccCashAdvanceRequestLine), calQuery);
								if (exists)
								{
									return true;
								}
							}
						}

						if (Parent.AH_Ledger == LedgerTypes.AccountsPayable)
						{
							var chargeQuery = new ZQuery(JobChargeSchema.JR_AL_APLine, value: partialLinkPKs.ToArray());
							chargeQuery.AddToFilter(JobChargeSchema.JR_IsAPCashAdvance, true);
							chargeQuery.AddToFilter(JobChargeSchema.JR_CAL_APLine, SQLComparisonOperator.NotEqual, ZGuid.Empty);
							charges.AddRange(Parent.Factory.Load<JobCharge>(chargeQuery));
							if (charges.Any())
							{
								var calPKs = charges.Select(c => c.JR_CAL_APLine);
								var calQuery = new ZQuery(AccCashAdvanceRequestLineSchema.PK, calPKs.ToArray());
								calQuery.AddToFilter(AccCashAdvanceRequestLineSchema.CAL_Status, CashAdvanceStatusCodes.RequestLine.Invoiced);
								var exists = Parent.Factory.Exists(typeof(AccCashAdvanceRequestLine), calQuery);
								if (exists)
								{
									return true;
								}
							}
						}
					}
				}
				return false;
			}

			bool HasInvoicedCashAdvanceRequestLinesInDatabase()
			{
				var sql = FormattableString.Invariant($@"SELECt TOP 1 AL_PK 
FROM dbo.AccTransactionLines
	INNER JOIN dbo.JobCharge ON {(Parent.AH_Ledger == LedgerTypes.AccountsReceivable ? "JR_AL_ARLine" : "JR_AL_APLine")} = AL_PK
	INNER JOIN dbo.AccCashAdvanceRequestLine ON CAL_PK = JR_CAL_ARLine
WHERE AL_AH = @transactionPK AND CAL_Status = 'INV'");

				var parameters = new ZSqlParameterCollection();
				parameters.Add("@transactionPK", Parent.PK, AccTransactionHeaderSchema.PK);

				var headerPKs = new DynamicBusinessObjectCollection(Parent.Factory);
				headerPKs.Load(sql, parameters);

				return headerPKs.Count > 0;
			}
		}

		CriticalValidationResult CheckOutstandingAmountIsZeroOnMatchingMiscTransactions()
		{
			bool miscLedgerAndType = (Parent.AH_Ledger == LedgerTypes.AccountsReceivable || Parent.AH_Ledger == LedgerTypes.AccountsPayable) &&
				(Parent.AH_TransactionType == TransactionTypes.ExchangeDifference || Parent.AH_TransactionType == TransactionTypes.Discount || Parent.AH_TransactionType == TransactionTypes.Overpayment);

			if (miscLedgerAndType && Parent.AH_OutstandingAmount != 0m)
			{
				var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
				StringBuilder extraMessage = new StringBuilder();
				extraMessage.AppendLine(collectorService?.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.NonZeroOutstandingAmountOnMiscTransaction));
				extraMessage.AppendLine(GetMatchGroupInfo());
				extraMessage.AppendLine(Res.GetString("d24755cd-a734-4ecf-a908-aea55dc0118e", "Outstanding Amount Stack Trace:\r\n{0}", Parent.MiscTransactionAH_OutstandingAmountStackTrace));

				return GetCriticalValidationResultForHeader(CriticalValidationErrorType.NonZeroOutstandingAmountOnMiscellaneousTransaction_4,
															CriticalValidationMessageTemplate.NonZeroOutstandingAmountOnMiscellaneousTransactionErrorMessage,
															extraMessage.ToString());
			}

			return null;
		}

		CriticalValidationResult CheckClearedDateIsEmptyForReceiptsWithNoBatchNumber()
		{
			bool isNotValid = false;
			if (!Parent.IsInDatabase || Parent.AH_LedgerInfo.HasChanges || Parent.AH_TransactionTypeInfo.HasChanges || Parent.AH_IsCancelledInfo.HasChanges ||
				Parent.AH_ReceiptBatchNoInfo.HasChanges || Parent.AH_DateClearedInCashbookInfo.HasChanges)
			{
				bool isARAPReceipt = (Parent.AH_Ledger == LedgerTypes.AccountsReceivable || Parent.AH_Ledger == LedgerTypes.AccountsPayable) && Parent.AH_TransactionType == TransactionTypes.Receipt;
				isNotValid = isARAPReceipt && !Parent.IsCancelled && Parent.AH_ReceiptBatchNo.IsEmpty && !Parent.AH_DateClearedInCashbook.IsEmpty;
			}
			if (isNotValid)
			{
				var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
				var extraMessage = new StringBuilder();
				extraMessage.AppendLine(collectorService.GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.ClearedReceiptLinkedToNonClearedDepositBatch));

				return new CriticalValidationResult(CriticalValidationErrorType.ClearedReceiptWithoutBatchNumber_3,
													CriticalValidationMessageTemplate.ClearedReceiptWithoutBatchNumberErrorMessage,
													extraMessage.ToString(),
													Parent.GetAllPropertyValues());
			}

			return null;
		}

		CriticalValidationResult CheckDueDateSetOnAutoOrReverseGLJournal()
		{
			bool isValid = true;
			if (!Parent.IsInDatabase && Parent.AH_Ledger == LedgerTypes.General &&
				(Parent.AH_TransactionType == TransactionTypes.GLAutoJournal || Parent.AH_TransactionType == TransactionTypes.GLReversingJournal))
			{
				// AH_DueDate must be set via AgePeriod property for these types of GL Journal
				isValid = !Parent.AH_DueDate.IsEmpty;
			}

			if (!isValid)
			{
				return GetCriticalValidationResultForHeader(CriticalValidationErrorType.AutoGLJournalWithoutDueDate_2,
																CriticalValidationMessageTemplate.AutoGLJournalWithoutDueDateErrorMessage);
			}

			return null;
		}

		CriticalValidationResult CheckLinesOfBankTranferBalanceToZero()
		{
			var correspondingRow = null as AccTransactionHeader;
			var isValid = true;
			if (Parent.AH_Ledger == LedgerTypes.CashBook && Parent.AH_TransactionType == TransactionTypes.Transfer)
			{
				correspondingRow = GetCorrespondingRowForBankTransfer() as AccTransactionHeader;
				isValid = correspondingRow != null && Parent.AH_InvoiceAmount + correspondingRow.AH_InvoiceAmount == 0;
			}

			if (!isValid)
			{
				string devMessage = string.Format(CultureInfo.InvariantCulture,
	(NoResString)@"This transaction:
{0}

Corresponding transaction:
{1}",
	Parent.GetTransactionHeaderInfo2(),
	correspondingRow != null ? correspondingRow.GetTransactionHeaderInfo2() : Res.GetString("e4593de7-2432-4dd9-af94-226b8751763e", "Not Found")
	);
				return new CriticalValidationResult(CriticalValidationErrorType.LinesOfBankTranferShouldBalanceToZero_3,
													CriticalValidationMessageTemplate.LinesOfBankTranferShouldBalanceToZeroErrorMessage,
													devMessage);
			}

			return null;
		}

		CriticalValidationResult CheckCancelledTransactionHeaderWithNoLineLinkedToCharge()
		{
			var helper = ObjectFactory.Get<IAccTransactionHeaderCriticalValidationHelper>();
			var checkResult = helper.CheckCancelledTransactionHeaderWithNoLineLinkedToCharge(Parent);
			if (!checkResult.IsValid)
			{
				var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
				var extraMsg = collectorService?.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.ReversedTransactionsShouldNotHaveLinesLinkedToCharges);
				return new CriticalValidationResult(CriticalValidationErrorType.ReversedTransactionsShouldNotHaveLinesLinkedToCharges_7,
									CriticalValidationMessageTemplate.ReversedTransactionsShouldNotHaveLinesLinkedToChargesErrorMessage,
									checkResult.ErrorMessage, extraMsg);
			}

			return null;
		}

		CriticalValidationResult CheckTransactionHeaderLedgerIsCompatibleWithTransactionHeaderType()
		{
			if (!AccTransactionHeaderCompatibilityMatrix.IsLedgerCompatibleWithTransactionType(Parent.AH_Ledger, Parent.AH_TransactionType))
			{
				var errorMessage = Res.GetString("1f819a51-83ea-4f37-8b6f-ac41a90e2d82", "Transaction ledger {0} is not compatible with Transaction Type {1}", Parent.AH_Ledger, Parent.AH_TransactionType);
				return GetCriticalValidationResultForHeader(CriticalValidationErrorType.TransactionHeaderLedgerNotCompatibleWithTransactionHeaderType_2,
															CriticalValidationMessageTemplate.TransactionHeaderLedgerNotCompatibleWithTransactionHeaderTypeErrorMessage,
															errorMessage);
			}

			return null;
		}

		static bool IsAmountEditableWithLedger(ZString ledger)
		{
			return ledger == LedgerTypes.IncompleteTransactions || ledger == LedgerTypes.UnapprovedPayableTransactions || ledger == LedgerTypes.TransactionsPendingAllocation || ledger == LedgerTypes.General;
		}

		static bool IsInvoiceBatch(ZString ledger, ZString type)
		{
			return ledger == LedgerTypes.AccountsReceivable && type == TransactionTypes.InvoiceBatch;
		}

		static bool IsDepositBatch(ZString ledger, ZString transactionType)
		{
			return ledger == LedgerTypes.CashBook && (transactionType == TransactionTypes.ReceiptBatch || transactionType == TransactionTypes.DDRBatch);
		}

		CriticalValidationResult CheckAmountIsEditedAfterBeingSaved()
		{
			if (CheckAmount(Parent.AH_InvoiceAmountInfo))
			{
				var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
				var extraMsg = collectorService?.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionHeaderInvoiceAmountWasModifiedAfterBeingSaved);

				return GetCriticalValidationResultForHeader(CriticalValidationErrorType.TransactionHeaderInvoiceAmountWasModifiedAfterBeingSaved_7,
															CriticalValidationMessageTemplate.TransactionHeaderInvoiceAmountWasModifiedAfterBeingSaved, extraMsg);
			}

			if (CheckAmount(Parent.AH_OSTotalInfo))
			{
				return GetCriticalValidationResultForHeader(CriticalValidationErrorType.TransactionHeaderOSAmountWasModifiedAfterBeingSaved_2,
															CriticalValidationMessageTemplate.TransactionHeaderOSAmountWasModifiedAfterBeingSaved);
			}

			if (CheckAmount(Parent.AH_GSTAmountInfo))
			{
				return GetCriticalValidationResultForHeader(CriticalValidationErrorType.TransactionHeaderGstAmountWasModifiedAfterBeingSaved_2,
															CriticalValidationMessageTemplate.TransactionHeaderGstAmountWasModifiedAfterBeingSavedErrorMessage);
			}

			return null;

			bool CheckAmount(ZPropertyInfo propertyInfo)
			{
				return CheckIsEditedAfterBeingSaved(propertyInfo, () => !IsInvoiceBatch(Parent.AH_Ledger, Parent.AH_TransactionType) && !IsDepositBatch(Parent.AH_Ledger, Parent.AH_TransactionType) && !IsAmountEditableWithLedger(Parent.AH_Ledger) && !IsAmountEditableWithLedger(Parent.AH_LedgerInfo.OriginalValue.ToString()));
			}
		}

		CriticalValidationResult CheckIsEditedAfterBeingSaved()
		{
			var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);

			if (CheckIsEditedAfterBeingSaved(Parent.AH_ABInfo))
			{
				var transactionHeaderBankAccountWasModifiedAfterBeingSavedInfo = collectorService?.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionHeaderBankAccountWasModifiedAfterBeingSaved);
				return GetCriticalValidationResultForHeader(CriticalValidationErrorType.TransactionHeaderBankAccountWasModifiedAfterBeingSaved_2, CriticalValidationMessageTemplate.TransactionHeaderBankAccountWasModifiedAfterBeingSavedErrorMessage, transactionHeaderBankAccountWasModifiedAfterBeingSavedInfo);
			}

			return null;
		}

		bool CheckIsEditedAfterBeingSaved(ZPropertyInfo propertyInfo, Func<bool> isSatisfyingAdditionalChecks = null)
		{
			return Parent.IsInDatabase && propertyInfo.HasChanges && (isSatisfyingAdditionalChecks == null || isSatisfyingAdditionalChecks());
		}

		CriticalValidationResult CheckChangePostDateOfAPostedInvoice()
		{
#if DEBUG
			if (ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.PostDateCriticalValidationSuspenderService>.IsSuspended(Parent.Factory))
			{ return null; }
#endif
			// For PeriodManager.UpdateTransactionHeaderPostDates method.
			var isAllowChangePostDate =
				(Parent.AH_TransactionType == TransactionTypes.GLStandardJournal ||
				 Parent.AH_TransactionType == TransactionTypes.GLReversingJournal ||
				 Parent.AH_TransactionType == TransactionTypes.GLAutoJournal ||
				 Parent.AH_TransactionType == TransactionTypes.GLNoteJournal)
				&& Parent.AH_PostDate.IsValid;

			var isPosted = Parent.IsInDatabase &&
				Parent.AH_Ledger != LedgerTypes.UnapprovedPayableTransactions &&
				Parent.AH_Ledger != LedgerTypes.TransactionsPendingAllocation &&
				Parent.AH_Ledger != LedgerTypes.IncompleteTransactions &&
				!Parent.AH_LedgerInfo.HasChanges;

			if (!isAllowChangePostDate && isPosted && Parent.AH_PostDateInfo.HasChanges)
			{
				return GetCriticalValidationResultForHeader(CriticalValidationErrorType.TransactionHeaderPostDateChangesAfterPosted_3,
															CriticalValidationMessageTemplate.TransactionHeaderPostDateChangesAfterPostedMessage);
			}

			return null;
		}

		ITransactionHeader GetCorrespondingRowForBankTransfer()
		{
			byte transactionCountToFilterFor;
			switch (Parent.AH_TransactionCount)
			{
				case AccTransactionHeader.TransactionCountConstants.BankTransferFromRow:
					transactionCountToFilterFor = AccTransactionHeader.TransactionCountConstants.BankTransferToRow;
					break;

				case AccTransactionHeader.TransactionCountConstants.BankTransferFromRowWhenReversing:
					transactionCountToFilterFor = AccTransactionHeader.TransactionCountConstants.BankTransferToRowWhenReversing;
					break;

				case AccTransactionHeader.TransactionCountConstants.BankTransferToRow:
					transactionCountToFilterFor = AccTransactionHeader.TransactionCountConstants.BankTransferFromRow;
					break;

				case AccTransactionHeader.TransactionCountConstants.BankTransferToRowWhenReversing:
					transactionCountToFilterFor = AccTransactionHeader.TransactionCountConstants.BankTransferFromRowWhenReversing;
					break;

				default:
					throw new Exception("Transaction count not recognised");
			}

			var filter = new ZQuery();
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, Parent.AH_TransactionBelongsToGroup);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCount, transactionCountToFilterFor);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, Parent.AH_GC);
			return Parent.Factory.LoadTop1<ITransactionHeader>(filter);
		}

		CriticalValidationResult CheckTransactionHasLines()
		{
			if (IsChequeDirectPayment(Parent))
			{
				return null;
			}

			var linesExist = true;

#if DEBUG
			if (Globals.IsTest && !ServiceContainerSuspenderHelper.AccTransactionHeaderWithLinesCriticalValidationActivatorService.Instance.IsActivated)
			{ return null; }
#endif

			if ((!Parent.IsInDatabase || Parent.AH_LedgerInfo.HasChanges || Parent.AH_TransactionTypeInfo.HasChanges) &&
				TransactionWithLinesMap.Contains(new KeyValuePair<string, string>(Parent.AH_Ledger, Parent.AH_TransactionType)))
			{
				var query = new ZQuery(AccTransactionLinesSchema.AL_GC, Parent.AH_GC) { FetchOnlyFromLocalCache = true };
				query.AddToFilter(AccTransactionLinesSchema.AL_AH, Parent.PK);

				linesExist = Parent.Factory.LoadTop1<AccTransactionLines>(query) != null;

				if (!linesExist)
				{
					linesExist = Db.Connection.Exists(string.Format(CultureInfo.InvariantCulture, "FROM {0} WHERE {1}", AccTransactionLinesSchema.Constants.TableName, query.LiteralTextSqlFormatted));
				}
			}

			if (!linesExist)
			{
				return GetCriticalValidationResultForHeader(
						CriticalValidationErrorType.TransactionHeaderWithLinesShouldHaveLines_4,
						CriticalValidationMessageTemplate.TransactionHeaderWithLinesShouldHaveLinesMessage,
						CriticalValidationInfoCollectorService.GetOrCreateService(Parent.Factory).GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.INTransactionWithoutLines),
						CriticalValidationInfoCollectorService.GetOrCreateService(Parent.Factory).GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.INTransactionSerializedData),
						CriticalValidationInfoCollectorService.GetOrCreateService(Parent.Factory).GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionHeaderBranchChanged),
						(NoResString)@"Tips for developers: please check whether the Transaction is deserialized from the old dirty XML data without lines.
Transactions can be saved as incomplete invoice without lines before [WI00545480 - Critical Validation: This transaction should have lines].
If there is no line in the XML data and
•	it's old data (created before this WI), users should enter lines and fix it by themselves.
•	or it's new data (created after this WI), please find out how this data is generated.");
			}

			return null;
		}

		CriticalValidationResult CheckExchangeRate()
		{
			var helper = ObjectFactory.Get<IAccTransactionHeaderCriticalValidationHelper>();
			if (!Parent.IsInDatabase && Parent.AH_ExchangeRate <= 0 && !helper.IsReversalOfOriginalTransaction(Parent) && !ShouldNotEnforceTransactionNumberAndExchangeRate)
			{
				var extraMessage = new ZStringBuilder();
				var query = new ZQuery(AccTransactionLinesSchema.AL_GC, Parent.AH_GC);
				query.AddToFilter(AccTransactionLinesSchema.AL_AH, Parent.PK);
				var lines = Parent.Factory.Load<AccTransactionLines>(query);
				if (lines.Any())
				{
					extraMessage.AppendLine().Append((NoResString)"Posted Lines: ");

					foreach (var line in lines)
					{
						extraMessage.Append(line.GetTransactionLineInfo());
					}

					extraMessage.AppendLine().Append((NoResString)"Related Job Charges: ");
					foreach (var line in lines)
					{
						var charge = line.LoadRelatedJobCharge();
						if (charge != null)
						{
							extraMessage.Append(charge.GetJobChargeInfo());
							extraMessage.AppendLine(CriticalValidationInfoCollectorService.GetOrCreateService(Parent.Factory).GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellExRateChangeWhenPostingReceivableCharges));
						}
					}
				}

				switch (Parent.AH_Ledger)
				{
					case LedgerTypes.AccountsReceivable:
						extraMessage.AppendLine().Append(FormattableString.Invariant($"AR Invoice Posting Exchange Rate Option: {ObjectFactory.Get<IAccounting>().Registry.GetInvoicePostingExchangeRateOptionAR(Parent.AH_GC, Parent.AH_RX_NKTransactionCurrency == Parent.Company.GC_RX_NKLocalCurrency)}"));
						break;
					case LedgerTypes.AccountsPayable:
					case LedgerTypes.IncompleteTransactions:
					case LedgerTypes.UnapprovedPayableTransactions:
					case LedgerTypes.TransactionsPendingAllocation:
						extraMessage.AppendLine().Append(FormattableString.Invariant($"AP Invoice Posting Exchange Rate Option: {ObjectFactory.Get<IAccounting>().Registry.GetInvoicePostingExchangeRateOptionAP(Parent.AH_GC, Parent.AH_RX_NKTransactionCurrency == Parent.Company.GC_RX_NKLocalCurrency)}"));
						break;
				}

				extraMessage.AppendLine().Append(FormattableString.Invariant($"Actual type:{Parent.GetType().Name}"));

				if (Parent.HasContext(BusinessContext.ShouldTraceExchangeRateError) || Parent.HasContext(BusinessContext.CreateTransactionsBeforePostingForFactoryLevel))
				{
					extraMessage.AppendLine().Append(FormattableString.Invariant($@"Exchange rate was changed after property info validation:{CriticalValidationInfoCollectorService.GetOrCreateService(Parent.Factory).GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.EvaluateTransactionHeaderWithZeroExchangeRate)}"));
				}

				return GetCriticalValidationResultForHeader(CriticalValidationErrorType.TransactionHeaderExchangeRateShouldBeGreaterThanZero_10,
													CriticalValidationMessageTemplate.TransactionHeaderExchangeRateIsNotGreaterThanZeroErrorMessage,
													extraMessage.ToStringWithNewLineBetweenAppends());
			}
			return null;
		}

		CriticalValidationResult CheckIsAllowedToCreateCreditNotes()
		{
			if (!Parent.IsInDatabase && (
				(new ZString[] { TransactionTypes.CreditNote, TransactionTypes.CreditNotePendingAllocation, TransactionTypes.UACreditNote, TransactionTypes.IncompleteCreditNote }.Contains(Parent.AH_TransactionType)
				&& !Parent.AH_IsCancelled && AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(Parent.AH_Ledger, Parent.AH_GC)) ||
				(Parent.AH_TransactionType == TransactionTypes.CreditNote
				&& Parent.AH_IsCancelled && AccountingMasterFilesUtils.ShouldPreventCreateReversalTransactions(Parent.AH_Ledger, Parent.AH_GC))))
			{
				//POSSIBLE CAUSES OF THIS CV:
				//Can be caused by profit share posting when AccountingConfigurationRegistry.Instance.ProfitSharePostProfitShareOnCreation registry is activated.
				//If so, this is user configuration issue, which we decided to address when we will have such occurrences.
				//To define if this is the case, we may need to collect this registry value and collect call stack where transaction is created.
				return GetCriticalValidationResultForHeader(CriticalValidationErrorType.NotAllowedToPostCreditNoteDueToRegistryConfiguration, CriticalValidationMessageTemplate.NotAllowedToPostCreditNoteDueToRegistryConfigurationErrorMessage);
			}
			return null;
		}

		CriticalValidationResult CheckGeneralLedgerAccount()
		{
			var isEmptyAG = !Parent.AH_AG.IsValid
				&& !Parent.IsInDatabase
				&& !Parent.AH_IsCancelled;
			if (isEmptyAG)
			{
				if (Parent.AH_Ledger == LedgerTypes.CashBook && Parent.AH_TransactionType == TransactionTypes.ExchangeDifference)
				{
					var devInfo = $@"{Parent.GetAllPropertyValues()}
----Registries Info----
CurrencyAdjustmentExchangeGainAccount:{ObjectFactory.Get<IAccounting>().Registry.CurrencyAdjustmentExchangeGainAccount.Value}
CurrencyAdjustmentExchangeLossAccount:{ObjectFactory.Get<IAccounting>().Registry.CurrencyAdjustmentExchangeLossAccount.Value}
";
					return new CriticalValidationResult(
						CriticalValidationErrorType.CashBookExchangeTransactionNeedGeneralLedgerAccount
						, CriticalValidationMessageTemplate.CashBookExchangeTransactionNeedGeneralLedgerAccountErrorMessage
						, devInfo);
				}
			}
			return null;
		}

		CriticalValidationResult CheckInvoiceDate()
		{
			var provider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IInvoiceDateValidation>;
			var error = provider?.Get()?.ValidateInvoiceDate(Parent);
			if (error != null)
			{
				return new CriticalValidationResult(CriticalValidationErrorType.InvalidInvoiceDate, error);
			}

			return null;
		}

		CriticalValidationResult CheckINTransactionHasPostedLines()
		{
#if DEBUG
			if (Globals.IsTest && Testing.SuspendINTransactionHeaderHasPostedLinesCriticalValidationAttribute.IsActive)
			{
				return null;
			}
#endif
			if (!Parent.IsCancelled && (Parent.IsIncompleteTransaction || Parent.IsAPTransactionConvertedFromIncompleteTransaction))
			{
				var query = new ZQuery(AccTransactionLinesSchema.AL_GC, Parent.AH_GC);
				query.AddToFilter(AccTransactionLinesSchema.AL_AH, Parent.PK);
				var lines = new BusinessObjectFactory().Load<AccTransactionLines>(query);
				if (lines.Any())
				{
					var lineInfoBuilder = new ZStringBuilder();
					lineInfoBuilder.AppendLine();
					lineInfoBuilder.Append((NoResString)"Posted Lines: ");

					foreach (var line in lines)
					{
						lineInfoBuilder.Append(line.GetTransactionLineInfo());
					}

					return GetCriticalValidationResultForHeader(CriticalValidationErrorType.INTransactionHeaderHasPostedLines_4, CriticalValidationMessageTemplate.INTransactionHeaderHasPostedLinesMessage, lineInfoBuilder.ToStringWithNewLineBetweenAppends());
				}
			}
			return null;
		}

		protected virtual bool ShouldNotEnforceTransactionNumberAndExchangeRate
		{
			get
			{
				return ((Parent.AH_Ledger == LedgerTypes.IncompleteTransactions || Parent.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions) && Parent.IsSelfBillingInvoice) ||
								Parent.AH_Ledger == LedgerTypes.TransactionsPendingAllocation;
			}
		}

		CriticalValidationResult CheckTransactionNumber()
		{
			if (!AccountingMasterFilesRegistry.Instance.EnableTransactionNumberCriticalValidation.Value)
			{
				return null;
			}

			if ((!Parent.IsInDatabase || Parent.AH_TransactionNumInfo.HasChanges) && Parent.AH_TransactionNum == ZString.Empty && !ShouldNotEnforceTransactionNumberAndExchangeRate)
			{
				var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
				var onSavingInfo = collectorService?.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.InvoicingBaseDidNotCreateTransactionNumberOnSaving);
				var setToEmptyInfo = collectorService?.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionHeaderTransactionNumberSetToEmpty);
				var valdiationProviderInfo = FormattableString.Invariant($@"CriticalValidation Class concrete type: {GetType().FullName}
Parent Object constructor call stack: {(Parent as IHaveConstructorStackTrace)?.ConstructorStackTrace}");
				return GetCriticalValidationResultForHeader(CriticalValidationErrorType.TransactionHeaderMustHaveTransactionNumber_8,
					CriticalValidationMessageTemplate.TransactionHeaderMustHaveTransactionNumber,
					new ZStringBuilder().AppendLine().AppendLine(onSavingInfo).AppendLine(setToEmptyInfo).AppendLine().AppendLine(valdiationProviderInfo).ToString());
			}

			if (CheckIsEditedAfterBeingSaved(Parent.AH_TransactionNumInfo, ShouldValidateTransactionNumberChanged))
			{
				var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
				var numberChangedInfo = collectorService?.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionHeaderTransactionNumberChanged);
				return GetCriticalValidationResultForHeader(CriticalValidationErrorType.TransactionNumberOfHeaderMustNotChangeOnceSaved_5,
					CriticalValidationMessageTemplate.TransactionNumberOfHeaderMustNotChangeOnceSaved,
					numberChangedInfo);
			}

			return null;
		}

		bool ShouldValidateTransactionNumberChanged()
		{
			return !Parent.CanUpdateTransactionNumber();
		}

		HashSet<KeyValuePair<string, string>> transactionWithLinesMap;
		HashSet<KeyValuePair<string, string>> TransactionWithLinesMap
		{
			get
			{
				if (transactionWithLinesMap == null)
				{
					transactionWithLinesMap = new HashSet<KeyValuePair<string, string>>();

					transactionWithLinesMap.Add(new KeyValuePair<string, string>(LedgerTypes.AccountsPayable, TransactionTypes.Invoice));
					transactionWithLinesMap.Add(new KeyValuePair<string, string>(LedgerTypes.AccountsPayable, TransactionTypes.AdjustmentNote));
					transactionWithLinesMap.Add(new KeyValuePair<string, string>(LedgerTypes.AccountsPayable, TransactionTypes.CreditNote));

					transactionWithLinesMap.Add(new KeyValuePair<string, string>(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice));
					transactionWithLinesMap.Add(new KeyValuePair<string, string>(LedgerTypes.AccountsReceivable, TransactionTypes.AdjustmentNote));
					transactionWithLinesMap.Add(new KeyValuePair<string, string>(LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote));

					transactionWithLinesMap.Add(new KeyValuePair<string, string>(LedgerTypes.CashBook, TransactionTypes.DirectPayment));
					transactionWithLinesMap.Add(new KeyValuePair<string, string>(LedgerTypes.CashBook, TransactionTypes.DirectReceipt));

					transactionWithLinesMap.Add(new KeyValuePair<string, string>(LedgerTypes.General, TransactionTypes.GLStandardJournal));
					transactionWithLinesMap.Add(new KeyValuePair<string, string>(LedgerTypes.General, TransactionTypes.GLAutoJournal));
					transactionWithLinesMap.Add(new KeyValuePair<string, string>(LedgerTypes.General, TransactionTypes.GLReversingJournal));

					transactionWithLinesMap.Add(new KeyValuePair<string, string>(LedgerTypes.JobCosting, TransactionTypes.Journal));
					transactionWithLinesMap.Add(new KeyValuePair<string, string>(LedgerTypes.JobCosting, TransactionTypes.JobRevenueJournal));

					transactionWithLinesMap.Add(new KeyValuePair<string, string>(LedgerTypes.UnapprovedPayableTransactions, TransactionTypes.UAInvoice));
					transactionWithLinesMap.Add(new KeyValuePair<string, string>(LedgerTypes.UnapprovedPayableTransactions, TransactionTypes.UACreditNote));
				}

				return transactionWithLinesMap;
			}
		}

		CriticalValidationResult CheckGLJournalEntriesNumberHasBeenAssigned()
		{
			if (CriticalValidationHelpers.CheckGLJournalEntriesNumberHasBeenAssigned(Parent))
			{
				return new CriticalValidationResult(CriticalValidationErrorType.GLJournalEntriesNumberHasBeenAssigned,
													CriticalValidationMessageTemplate.GLJournalEntriesNumberHasBeenAssignedErrorMessage());
			}
			return null;
		}

		#endregion

		#region AfterSaving Check Methods

		protected override IEnumerable<CriticalValidationResult> AfterSavingCriticalChecks()
		{
			foreach (var result in base.AfterSavingCriticalChecks())
			{
				yield return result;
			}

			if (ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().HasSkippedDataRefreshBusUpdate(Parent))
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.TransactionHeaderSkippedDataRefreshBusUpdateButWasSavedSuccessfully_2,
										CriticalValidationMessageTemplate.TransactionHeaderSkippedDataRefreshBusUpdateButWasSavedSuccessfully,
										(NoResString)"Transaction header skipped data refresh bus update, but it was saved successfully.",
										Parent.IsDeleted ? Parent.GetTransactionHeaderOriginalInfo() : Parent.GetTransactionHeaderInfo(),
										Parent.GetSkipDataRefreshBusInfo());
			}
		}

		#endregion

		string GetMatchGroupInfo()
		{
			string result;
			var msg = new StringBuilder();

			var matchGroupNums = Parent.Factory.Load<AccTransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, Parent.PK))
				.Select(x => x.AP_MatchGroupNum).Distinct().ToArray();

			var matchLinks = Parent.Factory.Load<AccTransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_MatchGroupNum, matchGroupNums));

			foreach (var link in matchLinks)
			{
				msg.AppendLine(link.GetMatchLinkInfo());
			}

			var headerPKs = matchLinks.Select(x => x.AP_AH).Distinct().ToArray();

			var headers = Parent.Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, headerPKs));
			foreach (var header in headers)
			{
				msg.AppendLine(header.GetTransactionHeaderInfo());
			}

			if (msg.Length > 0)
			{
				result = (NoResString)"Match group info:" + System.Environment.NewLine + msg.ToString();
			}
			else
			{
				result = (NoResString)"Match group info: There is no data collected.";
			}

			return result;
		}
	}
}
