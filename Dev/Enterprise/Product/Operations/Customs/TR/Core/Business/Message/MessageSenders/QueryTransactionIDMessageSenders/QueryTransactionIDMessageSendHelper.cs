using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.TR.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.TR.Business
{
	public class QueryTransactionIDMessageSendHelper
	{
		public QueryTransactionIDMessageSendHelper(LoggingInformation logger)
		{
			this.logger = logger;
		}

		public void SendQueryMessage(CancellationToken token)
		{
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;
			var query = GetPollingTransactionFilter(Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN, SendMessageTypes);
			query.AddToFilter(CusPollingTransactionSchema.CPT_EarliestTimeOfNextAttemptUtc, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);
			var pollingTransactions = factory.Load<CusPollingTransaction>(query);

			if (pollingTransactions.Length == 0)
			{
				logger.Log("No polling transaction record needs to be processed", Integration.LogType.Information);
			}
			else
			{
				foreach (var pollingTransaction in pollingTransactions)
				{
					SendQueryTransactionIDMessage(pollingTransaction);
				}
			}
		}

		void SendQueryTransactionIDMessage(CusPollingTransaction pollingTransaction)
		{
			var factory = pollingTransaction.Factory;
			EDIMessage originalRequestMessage;
			var originalResponseMessage = factory.Load<EDIMessage>(pollingTransaction.CPT_ParentID);
			if (originalResponseMessage != null)
			{
				switch (pollingTransaction.CPT_Type)
				{
					case TRMessageTypes.Codes.T2O:
						originalRequestMessage = TRInterchangeHelper.GetMainMessageByType(originalResponseMessage, TRMessageTypes.Codes.TRO);
						break;
					case TRMessageTypes.Codes.DT2:
						originalRequestMessage = TRInterchangeHelper.GetMainMessageByType(originalResponseMessage, TRMessageTypes.Codes.DKO);
						break;
					case TRMessageTypes.Codes.T1N:
						originalRequestMessage = TRInterchangeHelper.GetMainMessageByType(originalResponseMessage, TRMessageTypes.Codes.TRN);
						break;
					default:
						originalRequestMessage = TRInterchangeHelper.GetOriginalMessageByTrackingId(originalResponseMessage);
						break;
				}

				if (originalResponseMessage.EM_LinkedObject is IMessageAttachee && originalRequestMessage != null)
				{
					var messageType = pollingTransaction.CPT_Type;
					switch (messageType)
					{
						case TRMessageTypes.Codes.TRO:
						case TRMessageTypes.Codes.T2O:
							if (originalResponseMessage.EM_LinkedObject is Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader manifestHeader)
							{
								if (messageType == TRMessageTypes.Codes.T2O)
								{
									TRMessageSendingHelper.SendManifestAutoReceiveResponseMessageForT3O(manifestHeader, pollingTransaction.CPT_TransactionID, originalRequestMessage, UpdateCusPollingTransaction);
								}
								else
								{
									TRMessageSendingHelper.SendManifestAutoReceiveResponseMessage(manifestHeader, pollingTransaction.CPT_TransactionID, originalRequestMessage, UpdateCusPollingTransaction);
								}
							}
							break;
						case TRMessageTypes.Codes.TRE:
							if (originalResponseMessage.EM_LinkedObject is Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader eTradeHeader)
							{
								TRMessageSendingHelper.SendETradeAutoReceiveResponseMessage(eTradeHeader, pollingTransaction.CPT_TransactionID, originalRequestMessage, UpdateCusPollingTransaction);
							}
							break;
						case TRMessageTypes.Codes.TRN:
						case TRMessageTypes.Codes.T1N:
							if (originalResponseMessage.EM_LinkedObject is Integration.Customs.TR.ICusInBondHeader nctsHeader)
							{
								TRMessageSendingHelper.SendNCTSAutoReceiveResponseMessage(messageType, nctsHeader, pollingTransaction.CPT_TransactionID, originalRequestMessage, UpdateCusPollingTransaction);
							}
							break;
						case TRMessageTypes.Codes.DKO:
						case TRMessageTypes.Codes.DT2:
						case TRMessageTypes.Codes.DTE:
							if (originalResponseMessage.EM_LinkedObject is Integration.Customs.TR.ICusEntryHeader cusEntryHeader)
							{
								if (messageType == TRMessageTypes.Codes.DT2)
								{
									TRMessageSendingHelper.SendImportExportAutoReceiveResponseMessageDT3(cusEntryHeader as CusEntryHeader, pollingTransaction.CPT_TransactionID, originalRequestMessage, UpdateCusPollingTransaction);
								}
								else
								{
									TRMessageSendingHelper.SendImportExportAutoReceiveResponseMessage(cusEntryHeader as CusEntryHeader, pollingTransaction.CPT_TransactionID, originalRequestMessage, messageType, UpdateCusPollingTransaction);
								}
							}
							break;
					}
				}
			}
			else
			{
				logger.Log(pollingTransaction.CPT_TransactionID + "|" + pollingTransaction.CPT_Type + "|" + pollingTransaction.CPT_SystemLastEditTimeUtc + " Original Response Message not found!", Integration.LogType.Information);
			}

			void UpdateCusPollingTransaction(ActionResult result)
			{
				if (result.Success)
				{
					pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND;
					pollingTransaction.CPT_StatusReason = (NoResString)"Pending";
					logger.Log($"Polling transaction processed, TransactionID: {pollingTransaction.CPT_TransactionID}. {result.Notifications.NotificationsAsString()}", Integration.LogType.Information);
				}
				else
				{
					pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.ERR;
					pollingTransaction.CPT_StatusReason = (NoResString)"Error";
					var messageTrackingID = originalResponseMessage?.Interchange?.eHubID ?? (NoResString)"Unknown";
					logger.LogError($"Polling transaction processed, TransactionID: {pollingTransaction.CPT_TransactionID}, original response message eHub tracking ID: {messageTrackingID}. Message sent failed. {result.Notifications.NotificationsAsString()}");
				}

				pollingTransaction.CPT_StatusTimeUtc = ZDateTime.UtcNow;
			}
		}

		public static ZQuery GetPollingTransactionFilter(string status, params string[] types)
		{
			var query = new ZQuery();
			query.AddToFilter(CusPollingTransactionSchema.CPT_ApplicationCode, EDIMessage.ApplicationCodes.TRCustoms);
			query.AddToFilter(CusPollingTransactionSchema.CPT_Status, status);
			if (types.Length > 0)
			{
				query.AddToFilter(CusPollingTransactionSchema.CPT_Type, types);
			}

			return query;
		}

		public string[] SendMessageTypes => new[]
		{
			TRMessageTypes.Codes.TRO,
			TRMessageTypes.Codes.TRE,
			TRMessageTypes.Codes.TRN,
			TRMessageTypes.Codes.T1N,
			TRMessageTypes.Codes.T2O,
			TRMessageTypes.Codes.DKO,
			TRMessageTypes.Codes.DT2,
			TRMessageTypes.Codes.DTE
		};

		protected LoggingInformation logger;
	}
}
