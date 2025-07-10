using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.TR.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.TR.Business
{
	public static class TRMessageSendingHelper
	{
		public static void SendManifestAutoReceiveResponseMessage(Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader manifestHeader, ZString queryGuid, EDIMessage originalMessage, Action<ActionResult> afterSendMessage = null)
		{
			TRCustomsAutoSendProviderFactory.SendMessage<TRManifestAutoReceiveResponseMessageProvider>(manifestHeader, (sender) => new TRManifestAutoReceiveResponseMessageGenerator(sender, queryGuid, originalMessage), afterSendMessage);
		}

		public static void SendManifestAutoReceiveResponseMessageForT2O(Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader manifestHeader, EDIMessage originalMessage)
		{
			var message = TRInterchangeHelper.GetMainMessageByType(originalMessage, TRMessageTypes.Codes.T1O);
			var processStartDate = message?.EM_SystemCreateTimeUtc ?? ZDateTime.Invalid;

			TRCustomsAutoSendProviderFactory.SendMessage<TRManifestAutoReceiveResponseMessageProvider>(manifestHeader, (sender) => new TRManifestAutoReceiveResponseMessageGeneratorT2O(sender, originalMessage, processStartDate));
		}

		public static void SendManifestAutoReceiveResponseMessageForT3O(Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader manifestHeader, ZString queryGuid, EDIMessage originalMessage, Action<ActionResult> afterSendMessage = null)
		{
			TRCustomsAutoSendProviderFactory.SendMessage<TRManifestAutoReceiveResponseMessageProvider>(manifestHeader, (sender) => new TRManifestAutoReceiveResponseMessageGeneratorT3O(sender, queryGuid, originalMessage), afterSendMessage);
		}

		public static void SendManifestAutoReceiveResponseMessageForTRM(Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader manifestHeader, EDIMessage originalMessage)
		{
			var customsOffice = manifestHeader.AMA_CustomsOffice;
			var registrationNumber = manifestHeader.RegistrationNumber;

			TRCustomsAutoSendProviderFactory.SendMessage<TRManifestAutoReceiveResponseMessageProvider>(manifestHeader, (sender) => new TRManifestAutoReceiveResponseMessageGeneratorTRM(sender, originalMessage, customsOffice, registrationNumber));
		}

		public static void SendETradeAutoReceiveResponseMessage(Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader header, ZString queryGuid, EDIMessage originalMessage, Action<ActionResult> afterSendMessage = null)
		{
			TRCustomsAutoSendProviderFactory.SendMessage<ETradeAutoReceiveResponseMessageProvider>(header, (sender) => new ETradeAutoReceiveResponseMessageGenerator(sender, queryGuid, originalMessage), afterSendMessage);
		}

		public static void SendNCTSAutoReceiveResponseMessage(ZString messageType, Integration.Customs.TR.ICusInBondHeader nctsHeader, ZString queryGuid, EDIMessage originalMessage, Action<ActionResult> afterSendMessage = null)
		{
			Func<IMessageSender, ITRCustomsMessageGenerator> generatorCreator = null;
			switch (messageType)
			{
				case TRMessageTypes.Codes.TRN:
					generatorCreator = (sender) => new NCTSGetMessagesListByGuidAutoReceiveResponseMessageGenerator(sender, queryGuid, originalMessage);
					break;
				case TRMessageTypes.Codes.T1N:
					generatorCreator = (sender) => new NCTSDownloadMessageByIndexAutoReceiveResponseMessageGenerator(sender, queryGuid, originalMessage);
					break;
			}

			if (generatorCreator != null)
			{
				TRCustomsAutoSendProviderFactory.SendMessage<NCTSAutoReceiveResponseMessageProvider>(nctsHeader, generatorCreator, afterSendMessage);
			}
		}

		public static void SendSPTSAutoReceiveResponseMessage(Integration.Customs.TR.ICusInBondSPTSHeader sptsHeader, ZString queryGuid, EDIMessage originalMessage)
		{
			TRCustomsAutoSendProviderFactory.SendMessage<TRSPTSAutoReceiveResponseMessageProvider>(sptsHeader, (sender) => new TRSPTSAutoReceiveResponseMessageGenerator(sender, queryGuid, originalMessage));
		}

		public static void SendImportExportAutoReceiveResponseMessage(CusEntryHeader cusEntryHeader, ZString queryGuid, EDIMessage originalMessage, ZString messageType, Action<ActionResult> afterSendMessage = null)
		{
			TRCustomsAutoSendProviderFactory.SendMessage<DeclarationAutoReceiveResponseMessageProvider>(cusEntryHeader, (sender) => new DeclarationAutoReceiveResponseMessageGenerator(sender, queryGuid, originalMessage, messageType), afterSendMessage);
		}

		public static void SendImportExportAutoReceiveResponseMessageDT2(CusEntryHeader cusEntryHeader, EDIMessage originalMessage, ZString messageType)
		{
			EDIMessage message = null;
			if (messageType == TRMessageTypes.Codes.DT1)
			{
				message = TRInterchangeHelper.GetMainMessageByType(originalMessage, messageType);
			}
			else
			{
				message = TRInterchangeHelper.GetLastMessageWithApplicationReferenceByType(originalMessage, EDIMessage.Direction.Receive, messageType);
			}
			var processStartDate = message?.EM_SystemCreateTimeUtc ?? ZDateTime.Invalid;

			TRCustomsAutoSendProviderFactory.SendMessage<DeclarationAutoReceiveResponseMessageProvider>(cusEntryHeader, (sender) => new DeclarationAutoReceiveResponseMessageGeneratorDT2(sender, originalMessage, processStartDate));
		}

		public static void SendImportExportAutoReceiveResponseMessageDT3(CusEntryHeader cusEntryHeader, ZString queryGuid, EDIMessage originalMessage, Action<ActionResult> afterSendMessage = null)
		{
			TRCustomsAutoSendProviderFactory.SendMessage<DeclarationAutoReceiveResponseMessageProvider>(cusEntryHeader, (sender) => new DeclarationAutoReceiveResponseMessageGeneratorDT3(sender, queryGuid, originalMessage), afterSendMessage);
		}

		public static ZString ProcessCusPollingTransaction(TRBaseMessage message, ZString messageText, ZString queryMessageType) => ProcessCusPollingTransaction(message, queryMessageType, !messageText.IsEmpty);

		public static ZString ProcessCusPollingTransaction(TRBaseMessage message, ZString queryMessageType, bool close)
		{
			var result = ZString.Empty;
			var originalMessage = TRInterchangeHelper.GetOriginalMessageByTrackingId(message);
			if (originalMessage != null)
			{
				var query = QueryTransactionIDMessageSendHelper.GetPollingTransactionFilter(Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND, queryMessageType);
				query.AddToFilter(CusPollingTransactionSchema.CPT_TransactionID, originalMessage.EM_ApplicationReference);
				var pollingTransaction = message.Factory.LoadTop1<CusPollingTransaction>(query);

				if (pollingTransaction != null)
				{
					if (close)
					{
						pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.CLS;
						pollingTransaction.CPT_StatusReason = (NoResString)"Close";
						pollingTransaction.CPT_StatusTimeUtc = message.EM_SystemCreateTimeUtc;
					}
					else
					{
						if (pollingTransaction.CPT_NumberOfAttempts > 1)
						{
							pollingTransaction.CPT_NumberOfAttempts--;
							ZInt[] sendTimeDuration = { 20, 5, 2, 2 }; // Time durations between each sending attempt which makes the sending time of 2nd to 5th attempts are 3mins, 5mins, 10mins, 30mins after the creation time of the original TRO response message.
							pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN;
							pollingTransaction.CPT_StatusReason = (NoResString)"Open";
							pollingTransaction.CPT_StatusTimeUtc = message.EM_SystemCreateTimeUtc;
							pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc = pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc.AddMinutes(sendTimeDuration[pollingTransaction.CPT_NumberOfAttempts - 1]);
						}
						else
						{
							pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.ERR;
							pollingTransaction.CPT_StatusReason = (NoResString)"Error";
							pollingTransaction.CPT_StatusTimeUtc = message.EM_SystemCreateTimeUtc;
						}
					}

					result = pollingTransaction.CPT_Status;
				}
				else
				{
					ErrorReporter.ReportOnce("Cannot load relevant polling transactions in TR manifest message processor", ZString.Format("Query Message Type: {0}, Origial Message Application Reference: {1}", queryMessageType, originalMessage.EM_ApplicationReference));
				}
			}

			return result;
		}

		public static CusPollingTransaction CreateCusPollingTransaction(TRBaseMessage message, ZString queryMessageType, ZString temporaryQueryGUID, int earliestTimeOfNextAttemptUtc = 1)
		{
			var pollingTransaction = message.Factory.New<CusPollingTransaction>();
			pollingTransaction.CPT_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			pollingTransaction.CPT_Type = queryMessageType;
			pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN;
			pollingTransaction.CPT_NumberOfAttempts = 5;
			pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc = message.EM_SystemCreateTimeUtc.AddMinutes(earliestTimeOfNextAttemptUtc);
			pollingTransaction.CPT_TransactionID = temporaryQueryGUID;
			pollingTransaction.CPT_ParentID = message.PK;
			return pollingTransaction;
		}
	}
}
