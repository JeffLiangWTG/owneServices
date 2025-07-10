using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public class WhsAutoRateQueuedInvoicesProcessSynchronously : IWhsAutoRateQueuedInvoicesProcessSynchronously
	{
		#region ctor

		public WhsAutoRateQueuedInvoicesProcessSynchronously(IWhsInvoiceHelper invoiceHelper)
		{
			Argument.NotNull(invoiceHelper, nameof(invoiceHelper));
			InvoiceHelper = invoiceHelper;
		}
		readonly IWhsInvoiceHelper InvoiceHelper;

		#endregion

		#region CannotLockMessage

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule")]
		string CannotLockMessage(ZGuid pK)
		{
			var invoice = InvoiceHelper.GetByPK(pK);
			return string.Format((NoResString)"{0} - Another instance of the service task is processing the invoice for client {1} warehouse {2} or it cannot be locked.", // for logging only
				InvoiceHelper.GetInvoiceReferNumber(invoice), invoice.ClientName, invoice.WarehouseName);
		}

		#endregion

		#region CheckBeforePerformAction

		// Only load invoices when AutoCreateAndRatePeriodicInvoice is true (WI00477363)
		bool CheckBeforePerformAction(ZGuid pK) => InvoiceHelper.IsStillInQueueInDB(pK);

		#endregion

		#region Process

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule")]
		public void Process(IAutoRatingServiceLogger logger, CancellationToken token)
		{
			Argument.NotNull(logger, nameof(logger));
			var pKs = InvoiceHelper.GetQueuedInvoiceValidToProcessPKs();
			foreach (var pk in pKs)
			{
				token.ThrowIfCancellationRequested();
				using (var mutex = InvoiceHelper.GetInvoiceBillingAutomationMutex(pk))
				{
					if (!mutex.HasLock)
					{
						if (mutex.Lock())
						{
							using (logger.EnableAddingNoteWhileLogging(pk))
							{
								if (CheckBeforePerformAction(pk))
								{
									if (!PerformAction(logger, pk, 1))
									{
										PerformAction(logger, pk, 2);
									}
								}
							}
						}
						else
						{
							logger.Information(CannotLockMessage(pk));
						}
					}
				}
			}

			if (!pKs.Any())
			{
				logger.Information((NoResString)"There are no invoices in the queue to be processed."); // for logging only
			}
		}

		#endregion

		#region PerformAction

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule")]
		bool PerformAction(IAutoRatingServiceLogger logger, ZGuid pK, int attemptNumber)
		{
			var performed = true;
			var isValid = true;
			var invoiceSaveSafeResult = InvoiceSaveSafeResult.SaveSuccessful;
			var invoice = InvoiceHelper.LoadInNewFactory(pK);

			using (invoice.InvoiceBillingCheckLockSuspender())
			using (WhsInvoiceHelper.SetUserContextForInvoice(invoice))
			{
				logger.Information($"{InvoiceHelper.GetInvoiceReferNumber(invoice)} - Service task start processing billing invoice.");

				try
				{
					var billingAutomationStatus = invoice.BillingAutomationStatus;

					if (billingAutomationStatus == BillingAutomationStatusCodes.BillingAutomationDelivered)
					{
						logger.Information((NoResString)"The invoice has already been delivered. If you need to reprocess please do it manually.");
					}
					else
					{
						var posted = billingAutomationStatus == BillingAutomationStatusCodes.BillingAutomationPosted ||
							(RunAndSave(InvoiceHelper.AutoRateJobHeader) && RunAndSave(InvoiceHelper.PostInvoice));

						if (posted)
						{
							var invoiceClosed = RunAndSave(InvoiceHelper.CloseBillingAndRelatedJob);

							if (!invoiceClosed)
							{
								InvoiceHelper.DisposeLoadedJobHeaders(invoice.Factory);
								invoice = InvoiceHelper.LoadInNewFactory(pK);
								invoiceSaveSafeResult = InvoiceSaveSafeResult.SaveSuccessful;
							}

							isValid = InvoiceHelper.DeliverInvoice(logger, invoice);
						}
					}

					if (LastRunSucceed())
					{
						invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.NIQ;
						invoiceSaveSafeResult = InvoiceHelper.InvoiceSaveSafe(invoice, logger, attemptNumber);
					}

					performed = LastRunSucceed();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					logger.Error(ex.Message);
					isValid = false;
					throw;
				}
				finally
				{
					if (!isValid)
					{
						InvoiceHelper.DisposeLoadedJobHeaders(invoice.Factory);
					}
				}

				if (!performed)
				{
					var retry = invoiceSaveSafeResult == InvoiceSaveSafeResult.SaveFailedNoRetry ? 1 : 2;

					if (!isValid || attemptNumber == retry)
					{
						SaveInvoiceWithErrorStatus(logger, pK);
						performed = true;
					}
				}
			}

			return performed;

			bool RunAndSave(Func<IAutoRatingServiceLogger, WhsInvoice, bool> action)
			{
				isValid = action(logger, invoice);

				if (isValid)
				{
					invoiceSaveSafeResult = InvoiceHelper.InvoiceSaveSafe(invoice, logger, attemptNumber);
				}

				return LastRunSucceed();
			}

			bool LastRunSucceed() => isValid && invoiceSaveSafeResult == InvoiceSaveSafeResult.SaveSuccessful;
		}

		void SaveInvoiceWithErrorStatus(IAutoRatingServiceLogger logger, ZGuid pK)
		{
			var invoiceWithNoChanges = InvoiceHelper.LoadInNewFactory(pK);
			invoiceWithNoChanges.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.ERR;
			InvoiceHelper.InvoiceSaveSafe(invoiceWithNoChanges, logger, 3);
		}

		#endregion
	}
}
