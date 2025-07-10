using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.Business.OperationalAction
{
	public class OperationalActionBulkAESTIRMessageSender : DeclarationOperationalActionBulkMessageSender
	{
		public OperationalActionBulkAESTIRMessageSender(JobDeclaration job)
			: base(job)
		{
		}

		protected override string MessageTypeCore => "AESTIR";

		protected override SaveResult OperationalActionSendMessageCore(bool sendWithMessageErrors, IOperationalActionSectionLog log)
		{
			var result = SaveResult.Fail;
			var messageManager = new AESMessageManager(job);
			var reasonsCannotSendToAESTIR = messageManager.GetAnyReasonsWeCantSendToAESTIR();
			if (reasonsCannotSendToAESTIR.Count > 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}:", new object[] { JobLink });
				log.Notify(OperationalActionLogErrorLevel.Warning, new StringCollectionX(reasonsCannotSendToAESTIR).ToString());
				return result;
			}

			string apportionmentErrorMessage;
			if (!job.Invoices.AreChargesBalancedForInvoices(out apportionmentErrorMessage))
			{
				log.Notify(OperationalActionLogErrorLevel.Warning, apportionmentErrorMessage);
				return result;
			}

			var notifier = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			job.MessageInitiator = notifier;

			messageManager.MergeAndCheck();
			var entriesToSendMessage = job.CustomsEntryHeaders.OfType<CusEntryHeader>().Where(e => e.US_ShouldBeReportToCustoms).ToList();
			if (entriesToSendMessage.Count > 0 && (!job.HasMessageErrors || sendWithMessageErrors))
			{
				if (job.LockSendCustomsMessageMutex)
				{
					try
					{
						var isSuccess = false;
						foreach (CusEntryHeader entry in job.CustomsEntryHeaders)
						{
							if (entry.IsWaitingForResponse)
							{
								log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: System cannot send message for entry {1}, please check whether it's waiting for response from customs.", JobLink, entry.CH_BGMReference);
								continue;
							}
							else
							{
								var actionCode = entry.MessageAction;
								var message = new AESTIRMessageBuilder(entry, actionCode).PopulateMessage();
								if (message != null)
								{
									var status = ZString.Empty;
									switch (actionCode)
									{
										case UpdateActionCode.Add:
											status = AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse;
											break;
										case UpdateActionCode.Replace:
											status = AESDirectCustomsEntryStatus.Codes.AwaitingReplacementResponse;
											break;
										default:
											status = AESDirectCustomsEntryStatus.Codes.AwaitingDeleteResponse;
											break;
									}

									entry.CH_Status = status;
									entry.CH_EntryStatus = (entry.CH_EntryStatus.IsEmpty) ? entry.CH_Status : entry.CH_EntryStatus;
									entry.PopulateEntrySubmittedDateIfRequired();
									isSuccess = true;
								}
							}
						}

						try
						{
							job.Factory.Save();
						}
						catch (ZSaveConcurrencyException)
						{
							return SaveResult.FailWithConcurrencyError;
						}
						catch (ZSaveException ex)
						{
							log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Cannot save job. An unexpected error occurred - {1}", JobLink, ex.FriendlyMessage);
							return SaveResult.Fail;
						}

						if (isSuccess)
						{
							result = SaveResult.Success;
						}
					}
					finally
					{
						job.UnlockSendCustomsMessageMutex();
					}
				}
				else
				{
					var mutexInfo = job.GetSendCustomsMessageMutexInfo();
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: {1} is in the process of sending message. Cannot send message until that process is finished.", JobLink, mutexInfo);
				}
			}
			else if (job.CustomsEntryHeaders.Count == 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: There is no entry to send the message.", JobLink);
			}
			else if (entriesToSendMessage.Count == 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: There is no entry to send the message, please check whether the entry is waiting for response or has been cleared by customs.", JobLink);
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: " + notifier.InvalidOperationText, JobLink);
			}

			return result;
		}
	}
}
