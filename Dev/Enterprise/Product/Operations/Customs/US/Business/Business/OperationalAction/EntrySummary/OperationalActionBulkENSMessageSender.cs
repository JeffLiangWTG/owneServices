using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.US.Business.OperationalAction
{
	public class OperationalActionBulkENSMessageSender : DeclarationOperationalActionBulkMessageSender
	{
		public OperationalActionBulkENSMessageSender(JobDeclaration dec)
			: this(dec, ZString.Empty, ZString.Empty)
		{
		}

		public OperationalActionBulkENSMessageSender(JobDeclaration dec, ZString contactName, ZString contactPhone)
			: base(dec)
		{
			this.contactName = contactName;
			this.contactPhone = contactPhone;
		}

		readonly ZString contactName;
		readonly ZString contactPhone;

		protected override SaveResult OperationalActionSendMessageCore(bool sendWithMessageErrors, IOperationalActionSectionLog log)
		{
			var result = SaveResult.Fail;

			job.ResumeApportionment();

			var entry = job.ActiveEntryHeaders.EntrySummaryEntry ?? GetEntryAfterMerge(log);

			if (entry != null)
			{
				if (entry.CanSendOriginal)
				{
					result = SendENSMessage(entry, sendWithMessageErrors, log);
				}
				else
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Cannot send the Entry Summary, please check whether the entry summary has already been submitted to Customs.", new object[] { JobLink });
				}
			}
			return result;
		}

		CusEntryHeader GetEntryAfterMerge(IOperationalActionSectionLog log)
		{
			CusEntryHeader result = null;

			var notifier = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			if (job.DoMerge(notifier))
			{
				result = job.ActiveEntryHeaders.EntrySummaryEntry;
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: " + notifier.InvalidOperationText + @"
There is no Entry Summary entry to send.", new object[] { JobLink });
			}
			return result;
		}

		SaveResult SendENSMessage(CusEntryHeader entry, bool sendWithMessageErrors, IOperationalActionSectionLog log)
		{
			var result = SaveResult.Fail;
			job.ValidationModes = ValidationModes.EntrySummary;
			job.RunPreSaveValidationWithFetchHints();
			if (job.HasErrors)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}:", new object[] { JobLink });
				log.Notify(OperationalActionLogErrorLevel.Warning, job.GetErrors().ToUniqueMessageListString());
				return result;
			}

			string apportionmentErrorMessage;
			if (!job.Invoices.AreChargesBalancedForInvoices(out apportionmentErrorMessage))
			{
				log.Notify(OperationalActionLogErrorLevel.Warning, apportionmentErrorMessage);
				return result;
			}

			if (!job.HasMessageErrors || sendWithMessageErrors)
			{
				var canSend = IsValidForSending(entry, log);
				if (canSend)
				{
					var needsToRestoreToPreMessagingState = false;
					Action restoreToPreMessagingState = null;
					try
					{
						Func<PublishToUniversalResult> preMessagingAction = null;
						var preMessagingActionResult = job.PreMessagingAction(ref preMessagingAction, ref restoreToPreMessagingState, ImportMessageSendingMessageType.Original);

						var errorMessages = preMessagingActionResult.GetErrorMessageForCheckFieldsForBondedWarehouse();
						if (!errorMessages.IsEmpty)
						{
							log.NotifyFormat(OperationalActionLogErrorLevel.Warning, errorMessages);
							return result;
						}

						var isBondedWarehouse = preMessagingActionResult.IsBondedWarehouse;
						if (isBondedWarehouse && preMessagingAction != null)
						{
							var universalResult = preMessagingAction();

							if (universalResult.ResultType == UniversalResult.HadErrors)
							{
								log.NotifyFormat(OperationalActionLogErrorLevel.Warning, universalResult.ErrorMessage);
								return result;
							}
							needsToRestoreToPreMessagingState = true;
							((BondedWarehousingHelper)job.BondedWarehousingHelper).UpdateWarehouseWithdrawal();
						}

						var shouldCertifyCRL = entry.ShouldCertifyCargoReleaseEnabledFromEntrySummary();
						MQEDIMessage message;
						if (job.IsACE)
						{
							var declaration = entry.Declaration;
							var expeditedRelease = declaration != null && declaration.US_PGAExpeditedRelease;
							var autoSendingAction = ACEEntrySummaryMessageSendingOption.New(shouldCertifyCRL, expeditedRelease, contactName, contactPhone);
							message = new ACEEntrySummaryMessageBuilder(entry, autoSendingAction, UpdateActionCode.Add).PopulateMessage();
						}
						else
						{
							var builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, shouldCertifyCRL);
							message = builder.PopulateMessage();
						}

						if (message != null)
						{
							message.CalculateRelatedPropertiesAfterENSSending(entry);
							job.LogCustomsCommencedIfNeeded();

							try
							{
								job.Factory.Save();
								needsToRestoreToPreMessagingState = false;
							}
							catch (ZSaveConcurrencyException)
							{
								result = SaveResult.FailWithConcurrencyError;
							}
							catch (ZSaveException ex)
							{
								result = SaveResult.Fail;
								log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Cannot save job. An unexpected error occurred - {1}", new object[] { JobLink, ex.FriendlyMessage });
							}

							if (message.IsInDatabase)
							{
								result = SaveResult.Success;
							}
						}
					}
					finally
					{
						if (needsToRestoreToPreMessagingState && restoreToPreMessagingState != null)
						{
							restoreToPreMessagingState();
						}
					}
				}
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Cannot send the Entry Summary because the declaration has message errors.", new object[] { JobLink });
			}
			return result;
		}

		bool IsValidForSending(CusEntryHeader entry, IOperationalActionSectionLog log)
		{
			var result = true;

			var messageErrors = FormalENSValidator.GetEntrySummaryMessageErrors(entry, ImportMessageSendingMessageType.Original);
			if (messageErrors.Length > 0)
			{
				result = false;

				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: has message errors that will result in an Entry Summary message rejection by Customs", new object[] { JobLink });
				foreach (var text in messageErrors)
				{
					log.Notify(OperationalActionLogErrorLevel.Warning, text);
				}
			}

			var securityError = FormalENSValidator.CheckSendRLFSecurity(entry, job.RegistryCompanyPK, "Entry Summary");
			if (!securityError.IsEmpty)
			{
				result = false;
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: has errors that will result in an Entry Summary message rejection by Customs", new object[] { JobLink });
				log.Notify(OperationalActionLogErrorLevel.Warning, securityError);
			}
			return result;
		}

		protected override string MessageTypeCore
		{
			get { return "Entry Summary"; }
		}
	}
}
