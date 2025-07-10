using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business
{
	public class EntryHeaderSingleMessageManager : FormalEntrySingleMessageManager
	{
		public EntryHeaderSingleMessageManager(CusEntryHeader entry, ImportMessageStatusList.MessageType messageType)
			: base(entry)
		{
			this.entry = entry;
			this.messageType = messageType;
		}

		public EntryHeaderSingleMessageManager(CusEntryHeader entry, EntryHeaderMessageSendingAction action)
			: this(entry, action.messageType)
		{
			this.action = action;
		}

		readonly EntryHeaderMessageSendingAction action;

		public readonly CusEntryHeader entry;
		public readonly ImportMessageStatusList.MessageType messageType;

		#region Overriden

		protected override void OnAmendmentSentCore()
		{
			base.OnAmendmentSentCore();

			CreateDocPrintingDataForEntrySummary();
		}

		protected override void OnOriginalSentCore()
		{
			base.OnOriginalSentCore();

			CreateDocPrintingDataForEntrySummary();
		}

		protected override bool IncludeBusinessLayerNotificationsInMessageSendingNotifications
		{
			get { return entry.IsExport; }
		}

		protected override bool ShouldWaitUntilResponded
		{
			get { return false; }
		}

		public override bool HasActiveMessages
		{
			get { return entry.HasActiveTransactionsWithCustoms; }
		}

		public override bool CanSendOriginal
		{
			get { return entry.CanSendOriginal; }
		}

		public override bool CanSendWithdrawal
		{
			get { return entry.CanSendWithdrawal; }
		}

		public override string MessageFriendlyName
		{
			get { return GetStringRepresentationFor() + " - " + entry.EntryNumber; }
		}

		public override bool IsWaitingForResponse
		{
			get { return entry.IsWaitingForResponse; }
		}

		/// <summary>
		/// Base compares two messages using factory & db objects and stops users from saving the current change where the subsequent amendment message is not selected to be sent
		/// US system allows users to save any changes at any time even after an entry is lodged. 
		/// </summary>
		/// <returns></returns>
		protected override bool RequiresAmendmentCore()
		{
			return entry.IsExport && base.RequiresAmendmentCore();
		}

		#endregion

		#region Sending Messages

		protected override Enterprise.Messaging.Business.EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo)
		{
			return GenerateOriginalMessages((CusEntryHeader)bizo, false);
		}

		protected override Enterprise.Messaging.Business.EDIMessage[] GenerateMessagesForAmendmentDetection(BusinessObject bizo)
		{
			return GenerateOriginalMessages((CusEntryHeader)bizo, true);
		}

		MQEDIMessage[] GenerateOriginalMessages(CusEntryHeader entry, bool isForAmendmentDetection)
		{
			var result = new List<MQEDIMessage>();
			MQEDIMessage message = null;
			MessageStatusCalculator statusCalculator = null;
			switch (messageType)
			{
				case ImportMessageStatusList.MessageType.EntrySummary:
					{
						EnsureActionIsNotNull(entry.IsExport);

						message = GenerateEntrySummaryMessage(UpdateActionCode.Add);
						statusCalculator = new EntrySummaryMessageStatusCalculator(entry);
						eBondLogger.CancelAutoSendLog(entry.Logs);

						break;
					}
				case ImportMessageStatusList.MessageType.BorderCargoRelease:
					{
						message = new BorderCargoReleaseMessageBuilder(entry, UpdateActionCode.Add).PopulateMessage();
						statusCalculator = new BorderCargoReleaseMessageStatusCalculator(entry);
						break;
					}
				case ImportMessageStatusList.MessageType.CargoRelease:
					{
						EnsureActionIsNotNull(entry.IsExport);

						message = new CargoReleaseMessageBuilder(entry, UpdateActionCode.Add, action.US_CertifyCargoRelease).PopulateMessage();
						statusCalculator = new CargoReleaseMessageStatusCalculator(entry);
						break;
					}
				case ImportMessageStatusList.MessageType.ACECargoRelease:
					{
						EnsureActionIsNotNull(entry.IsExport);

						message = new SimplifiedEntryMessageBuilder(entry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action)).PopulateMessage();
						statusCalculator = new SimplifiedEntryMessageStatusCalculator(entry);
						eBondLogger.CancelAutoSendLog(entry.Logs);

						break;
					}
				default:
					throw new NotSupportedException(string.Format("{0}.GenerateOriginalMessages doesn't support ImportMessageStatusList.MessageType : {1}", GetType().FullName, messageType));
			}

			if (message != null)
			{
				result.Add(message);
				if (!isForAmendmentDetection)
				{
					entry.Messages.Add(message);
					if (statusCalculator != null)
					{
						statusCalculator.CalculateStatus(message, ABIResponseStatus.Undefined);
						message.UpdateFDAMsgStatusIfRelevant(entry.Declaration);
					}
				}
			}

			return result.ToArray();
		}

		void EnsureActionIsNotNull(bool isExport)
		{
			if (!isExport && action == null)
			{
				throw new InvalidOperationException("For import, you should use a constructor with ImportMessageSendingAction");
			}
		}

		protected override Enterprise.Messaging.Business.EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo)
		{
			var result = new List<MQEDIMessage>();
			var entry = (CusEntryHeader)bizo;
			EnsureActionIsNotNull(entry.IsExport);

			MQEDIMessage message = null;
			MQEDIMessage statusCalculationMessage = null;
			MessageStatusCalculator statusCalculator = null;
			if (action == null || action.US_SendMessage)
			{
				switch (messageType)
				{
					case ImportMessageStatusList.MessageType.BorderCargoRelease:
						{
							message = new BorderCargoReleaseMessageBuilder(entry, UpdateActionCode.Replace).PopulateMessage();
							statusCalculationMessage = message;
							statusCalculator = new BorderCargoReleaseMessageStatusCalculator(entry);
							break;
						}
					case ImportMessageStatusList.MessageType.CargoRelease:
						{
							message = new CargoReleaseMessageBuilder(entry, UpdateActionCode.Replace, action.US_CertifyCargoRelease).PopulateMessage();
							statusCalculationMessage = message;
							statusCalculator = new CargoReleaseMessageStatusCalculator(entry);
							break;
						}
					case ImportMessageStatusList.MessageType.ACECargoRelease:
						{
							var updateActionType = action.US_SE_ActionType == ACECargoReleaseActionType.Codes.Update ? UpdateActionCode.Update : UpdateActionCode.Replace;
							message = new SimplifiedEntryMessageBuilder(entry, updateActionType, ACEEntrySummaryMessageSendingOption.New(action)).PopulateMessage();
							statusCalculationMessage = message;
							statusCalculator = new SimplifiedEntryMessageStatusCalculator(entry);
							eBondLogger.CancelAutoSendLog(entry.Logs);
							break;
						}
					case ImportMessageStatusList.MessageType.EntrySummary:
						{
							message = GenerateEntrySummaryMessage(UpdateActionCode.Replace);
							statusCalculationMessage = message;
							statusCalculator = new EntrySummaryMessageStatusCalculator(entry);
							eBondLogger.CancelAutoSendLog(entry.Logs);
							break;
						}
					default:
						{
							throw new NotSupportedException(string.Format("{0}.GenerateAmendmentMessages doesn't support ImportMessageStatusList.MessageType : {1}", GetType().FullName, messageType));
						}
				}
			}

			if (message != null)
			{
				entry.Messages.Add(message);
				result.Add(message);
			}

			if (statusCalculator != null)
			{
				statusCalculator.CalculateStatus(statusCalculationMessage, ABIResponseStatus.Undefined);
			}

			return result.ToArray();
		}

		MQEDIMessage GenerateEntrySummaryMessage(UpdateActionCode actionCode)
		{
			MQEDIMessage message = null;
			var declaration = entry.Declaration;
			if (declaration.IsACE)
			{
				if (declaration.US_PSC)
				{
					message = new ACEEntrySummaryMessageBuilder(entry, ACEEntrySummaryMessageSendingOption.New(action), action.US_PSCExplanation, action.PSCReasonCodes, actionCode).PopulateMessage();
				}
				else
				{
					message = new ACEEntrySummaryMessageBuilder(entry, ACEEntrySummaryMessageSendingOption.New(action), actionCode).PopulateMessage();
				}
			}
			else
			{
				message = new EntrySummaryMessageBuilder(entry, actionCode, action.US_CertifyCargoRelease).PopulateMessage();
			}
			return message;
		}

		protected override Enterprise.Messaging.Business.EDIMessage[] GenerateWithdrawalMessagesCore(BusinessObject bizo)
		{
			var entry = (CusEntryHeader)bizo;
			var result = new List<MQEDIMessage>();

			MQEDIMessage message = null;
			MessageStatusCalculator statusCalculator = null;
			switch (messageType)
			{
				case ImportMessageStatusList.MessageType.BorderCargoRelease:
					{
						message = new BorderCargoReleaseMessageBuilder(entry, UpdateActionCode.Delete).PopulateMessage();
						statusCalculator = new BorderCargoReleaseMessageStatusCalculator(entry);
						break;
					}
				case ImportMessageStatusList.MessageType.CargoRelease:
					{
						message = new CargoReleaseMessageBuilder(entry, UpdateActionCode.Delete, false).PopulateMessage();
						statusCalculator = new CargoReleaseMessageStatusCalculator(entry);
						break;
					}
				case ImportMessageStatusList.MessageType.ACECargoRelease:
					{
						message = new SimplifiedEntryMessageBuilder(entry, UpdateActionCode.Delete, ACEEntrySummaryMessageSendingOption.New(action)).PopulateMessage();
						statusCalculator = new SimplifiedEntryMessageStatusCalculator(entry);
						eBondLogger.CancelAutoSendLog(entry.Logs);
						break;
					}
				case ImportMessageStatusList.MessageType.EntrySummary:
					{
						var declaration = entry.Declaration;
						if (declaration.IsACE)
						{
							message = new ACEEntrySummaryMessageBuilder(entry, ACEEntrySummaryMessageSendingOption.New(action), UpdateActionCode.Delete).PopulateMessage();
						}
						else
						{
							message = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Delete, false).PopulateMessage();
						}

						statusCalculator = new EntrySummaryMessageStatusCalculator(entry);
						eBondLogger.CancelAutoSendLog(entry.Logs);
						break;
					}
				default:
					throw new NotSupportedException(string.Format("{0}.GenerateWithdrawalMessages doesn't support ImportMessageStatusList.MessageType : {1}", GetType().FullName, messageType));
			}

			if (message != null)
			{
				entry.Messages.Add(message);
				result.Add(message);
				if (statusCalculator != null)
				{
					statusCalculator.CalculateStatus(message, ABIResponseStatus.Undefined);
				}
			}
			return result.ToArray();
		}

		protected override IEnumerable<INotification> GetMessageErrors()
		{
			return new USCustomsNotificationCollector((BusinessObjectForNotification as IMessageNotificationsProvider), true, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors();
		}

		#endregion

		#region Implementation

		void CreateDocPrintingDataForEntrySummary()
		{
			if (messageType == ImportMessageStatusList.MessageType.EntrySummary)
			{
				entry.Declaration.US_JobReadyForPost = action.US_JobReadyForPosting;

				var applicationIdentifier = entry.IsACE ? ACEApplicationIdentifierCodeList.Codes.EntrySummary : ApplicationIdentifierCodeList.Codes.EntrySummary;

				MQEDIMessage message = (MQEDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, applicationIdentifier, EDIMessage.Direction.Transmit);
				if (message != null)
				{
					entry.CreateDocPrintingDetails(message.PK);
				}
			}
		}

		internal string GetStringRepresentationFor()
		{
			string result = "message";

			if (messageType == ImportMessageStatusList.MessageType.EntrySummary)
			{
				result = MessageAttacheeRecordTypeDescriptions.Entry;
			}
			else if (messageType == ImportMessageStatusList.MessageType.ACECargoRelease)
			{
				result = MessageAttacheeRecordTypeDescriptions.SimplifiedEntry;
			}
			return result;
		}

		#endregion
	}
}
