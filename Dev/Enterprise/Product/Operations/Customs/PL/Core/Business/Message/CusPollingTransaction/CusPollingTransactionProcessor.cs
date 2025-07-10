using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;
using CusPollingTransactionStatus = Enterprise.Core.Constants.Customs.CusPollingTransactionStatus.Codes;
using CusPollingTransactionTypes = Enterprise.Core.Constants.Customs.CusPollingTransactionType.Codes;
using InterchangeStatus = Enterprise.Messaging.Business.EDIInterchange.Status;
using MessageStatus = Enterprise.Messaging.Business.EDIMessage.Status;
using ProcessingResult = Enterprise.Customs.PL.Business.CusPollingTransactionProcessingResult;

namespace Enterprise.Customs.PL.Business;

public sealed class CusPollingTransactionProcessor(LoggingInformation serviceLog, CancellationToken cancellationToken)
{
	readonly LoggingInformation serviceLog = Argument.NotNull(serviceLog, nameof(serviceLog));

	readonly TimeSpan oneHourTimeSpan = TimeSpan.FromHours(1);

	enum CommunicationFailureReason
	{
		InterchangeFailed = 0,
		ResponseWaitingTimeout = 1,
	}

	public ProcessingResult ProcessForBranch(BusinessObjectFactory factory)
	{
		int createdBacklogs = 0, updatedTransactions = 0, messagesSent = 0;
		var transactionsToSendMessage = new List<CusPollingTransaction>();
		var utcNow = ZDateTime.UtcNow.TrimSeconds();
		var timeoutCutoffUtc = utcNow.AddMinutes(-PLCustomsDataRegistry.Instance.PUESCRequestTimeOut.Value);

		var transactionsToProcess = GetCurrentBranchTransactionsToProcess(factory);
		foreach (var transaction in transactionsToProcess)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ProcessTransaction(transaction);
		}

		if (transactionsToSendMessage.Count > 0)
		{
			SendMessages();
		}

		return new ProcessingResult(createdBacklogs, updatedTransactions, messagesSent);

		void ProcessTransaction(CusPollingTransaction transaction)
		{
			var transmittedMessage = factory.FindLastTransmittedMessage(ApplicationCodes.PLCustoms, transaction);

			if (transmittedMessage?.Interchange is { } transmittedInterchange)
			{
				var interchange = factory.FindReceivedInterchangeBySessionGuid(transmittedInterchange.EI_ApplicationCode, transmittedInterchange.EI_SessionGUID) ?? transmittedInterchange;
				UpdateTransactionStatusFromInterchange(interchange);
			}

			if (transaction.CPT_Status == CusPollingTransactionStatus.AWR
				&& transaction.CPT_EarliestTimeOfNextAttemptUtc < timeoutCutoffUtc)
			{
				ProcessTransactionCommunicationFailure(CommunicationFailureReason.ResponseWaitingTimeout, transmittedMessage, transmittedMessage?.Interchange);
				return;
			}

			if (transaction.CPT_Status == CusPollingTransactionStatus.OPN
				&& transaction.CPT_Type == CusPollingTransactionTypes.PLC
				&& utcNow >= transaction.CPT_EarliestTimeOfNextAttemptUtc)
			{
				transactionsToSendMessage.Add(transaction);

				if (utcNow - transaction.CPT_EarliestTimeOfNextAttemptUtc > oneHourTimeSpan)
				{
					var newBacklogTransactions = transaction.CreateBacklogs(factory);
					transactionsToSendMessage.AddRange(newBacklogTransactions);
					createdBacklogs += newBacklogTransactions.Count;
				}
			}
			return;

			void UpdateTransactionStatusFromInterchange(EDIInterchange interchange)
			{
				switch (interchange.EI_Status)
				{
					case InterchangeStatus.Received:
						if (transaction.CPT_Type == CusPollingTransactionTypes.PLC)
						{
							transaction.ReopenPlcTransaction();
						}
						else
						{
							transaction.CPT_Status = CusPollingTransactionStatus.CLS;
						}
						updatedTransactions++;
						break;

					case InterchangeStatus.Cancelled:
					case InterchangeStatus.Discarded:
					case InterchangeStatus.Error:
					case InterchangeStatus.Failed:
					case InterchangeStatus.SyntaxRejected:
						ProcessTransactionCommunicationFailure(CommunicationFailureReason.InterchangeFailed, transmittedMessage, interchange);
						updatedTransactions++;
						break;
				}
			}

			void ProcessTransactionCommunicationFailure(CommunicationFailureReason failureReason, BaseEDIMessage lastTransmitMessage, EDIInterchange interchange)
			{
				var failureLogMessage = GetCommunicationFailureLogMessage(failureReason);
				switch (transaction.CPT_Type)
				{
					case CusPollingTransactionTypes.PLC:
						var createdBackLog = transaction.ProcessPlcFaultAndCreateBacklog(serviceLog, lastTransmitMessage, failureLogMessage);
						if (createdBackLog is not null)
						{
							createdBacklogs++;
							messagesSent++;
						}
						break;

					case CusPollingTransactionTypes.BLG:
						if (transaction.ProcessBacklogFault(serviceLog, lastTransmitMessage, failureLogMessage))
						{
							messagesSent++;
						}
						break;
				}

				if (lastTransmitMessage is not null)
				{
					MarkMessageAsFailed(lastTransmitMessage, interchange);
				}
			}

			static void MarkMessageAsFailed(BaseEDIMessage message, EDIInterchange interchange)
			{
				message.EM_Status = interchange?.EI_Status.ToString() switch
				{
					InterchangeStatus.Cancelled => MessageStatus.Cancelled,
					InterchangeStatus.Discarded => MessageStatus.Discarded,
					InterchangeStatus.Error => MessageStatus.Error,
					InterchangeStatus.SyntaxRejected => MessageStatus.Rejected,
					_ => MessageStatus.Failed,
				};
			}

			string GetCommunicationFailureLogMessage(CommunicationFailureReason failureReason) => failureReason switch
			{
				CommunicationFailureReason.InterchangeFailed => (NoResString)"Interchange sent failed",
				CommunicationFailureReason.ResponseWaitingTimeout => (NoResString)$"The response timeout has expired; the total waiting time was {NotEmptyPartsToString(utcNow - transaction.CPT_StatusTimeUtc)}.",
				_ => throw new InvalidOperationException($"{failureReason} is not supported!"),
			};

			static string NotEmptyPartsToString(TimeSpan timeToParse)
				=> string.Join(", ", new[]
				{
					timeToParse.Days > 0 ? (NoResString)$"{timeToParse.Days} days" : null,
					timeToParse.Hours > 0 ? (NoResString)$"{timeToParse.Hours} hours" : null,
					timeToParse.Minutes > 0 ? (NoResString)$"{timeToParse.Minutes} minutes" : null,
				}.WhereNotNull());
		}

		void SendMessages()
		{
			cancellationToken.ThrowIfCancellationRequested();
			var cusPollingTransactionMessageSender = new CusPollingTransactionMessageSender(factory);
			foreach (var transaction in transactionsToSendMessage)
			{
				cancellationToken.ThrowIfCancellationRequested();

				cusPollingTransactionMessageSender.SendInContext(transaction);
				transaction.CPT_Status = CusPollingTransactionStatus.AWR;
				messagesSent++;
			}
		}
	}

	static IReadOnlyCollection<CusPollingTransaction> GetCurrentBranchTransactionsToProcess(BusinessObjectFactory factory)
	{
		var cusPollingTransactionsQuery = new ZDBOnlyQuery(typeof(CusPollingTransaction));
		cusPollingTransactionsQuery.AddToFilter(CusPollingTransactionSchema.CPT_ApplicationCode, ApplicationCodes.PLCustoms);
		cusPollingTransactionsQuery.AddToFilter(CusPollingTransactionSchema.CPT_Type, new[] { CusPollingTransactionTypes.PLC, CusPollingTransactionTypes.BLG });
		cusPollingTransactionsQuery.AddToFilter(CusPollingTransactionSchema.CPT_Status, new[] { CusPollingTransactionStatus.OPN, CusPollingTransactionStatus.AWR });
		var glbPasswordQuery = new ZDBOnlySubQuery(typeof(GlbExternalPassword), GlbExternalPasswordSchema.PK);
		{
			glbPasswordQuery.AddToFilter(GlbExternalPasswordSchema.GP_GB, GlbBranch.CurrentBranch.PK);
			glbPasswordQuery.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.PLB);
			glbPasswordQuery.AddToFilter(GlbExternalPasswordSchema.GP_CurrentPassword, SQLComparisonOperator.NotEqual, ZString.Empty);
			glbPasswordQuery.AddToFilter(GlbExternalPasswordSchema.GP_MailBoxID, SQLComparisonOperator.NotEqual, ZString.Empty);
		}
		cusPollingTransactionsQuery.AddSubQuery(CusPollingTransactionSchema.CPT_ParentID, glbPasswordQuery, JoinCondition.And);

		return factory.Load<CusPollingTransaction>(cusPollingTransactionsQuery);
	}
}
