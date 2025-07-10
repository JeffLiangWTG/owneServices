using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Messaging.Business;
using CusEntryHeader = Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.CusEntryHeader;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff
{
	public class MessageManager : MessageManagerForDeclaration
	{
		public MessageManager(JobDeclaration declaration, OperationType operationType)
			: base(declaration, operationType)
		{
		}

		public MessageManager(JobDeclaration declaration, OperationType operationType, IAdditionalInformation additionalInformation, string submitterCode)
			: base(declaration, operationType, additionalInformation)
		{
			this.submitterCode = submitterCode;
		}

		readonly string submitterCode;

		#region EntryHeader
		public new CusEntryHeader EntryHeader
		{
			get { return (CusEntryHeader)base.EntryHeader; }
		}
		#endregion

		#region Execute
		public override bool Execute()
		{
			(bool success, EDIMessage message) result = default;

			if (Declaration.IsTSWDeclaration && AdditionalInformation != null)
			{
				QueueForManifesting = AdditionalInformation.QueueForManifesting;
			}

			if (IsOkToExecute)
			{
				if (QueueForManifesting)
				{
					Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ReadyForManifesting;
					Declaration.JE_ECI_LastResponseStatus = LowValueConsignmentStatusList.Codes.NoStatusReported;
					fLastHumanReadableStatus = "Job has been Queued for Manifesting";
					result.success = true;
				}
				else
				{
					result = ExecuteTSWMessages();
				}

				if (result.success)
				{
					result.success = SendMessage(result.message);
				}
			}

			return result.success;
		}

		#endregion

		(bool success, EDIMessage message) ExecuteTSWMessages()
		{
			MessageType typeToExecute = MessageTypeToBeSent;
			if (Declaration.IsImport)
			{
				return ExecuteICRMessage(typeToExecute);
			}
			else if (Declaration.IsExport)
			{
				return ExecuteCREMessage(typeToExecute);
			}

			return default;
		}

		(bool success, EDIMessage message) ExecuteCREMessage(MessageType typeToExecute)
		{
			TSWCREMessageBuilder msgBuilder;
			switch (typeToExecute)
			{
				case MessageType.Original:
					msgBuilder = new TSWCREMessageBuilder(EntryHeader, AdditionalInformation, Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.MessageBuilder.MessageTypes.Original, submitterCode);
					fLastHumanReadableStatus = "Original Entry Message " + MessageManager.MessageReportingImmediateSend;
					return (true, msgBuilder.GenerateMessage());

				case MessageType.CancelEntry:
					msgBuilder = new TSWCREMessageBuilder(EntryHeader, AdditionalInformation, Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.MessageBuilder.MessageTypes.CancelEntry, submitterCode);
					fLastHumanReadableStatus = "Cancel Entry Message " + MessageManager.MessageReportingImmediateSend;
					return (true, msgBuilder.GenerateMessage());

				case MessageType.Replacement:
				case MessageType.ReplaceHeaderAndLines:
					msgBuilder = new TSWCREMessageBuilder(EntryHeader, AdditionalInformation, Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.MessageBuilder.MessageTypes.Replacement, submitterCode);
					fLastHumanReadableStatus = "Replacement Entry Message " + MessageManager.MessageReportingImmediateSend;
					return (true, msgBuilder.GenerateMessage());

				case MessageType.ResetToOriginal:
					Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.NotSentToCustoms;
					Declaration.JE_ECI_LastResponseStatus = LowValueConsignmentStatusList.Codes.NoStatusReported;
					EntryHeader.CH_IsActive = false;
					fLastHumanReadableStatus = "Job has been 'Reset to Original'";
					return (true, null);

				default:
					fLastHumanReadableStatus = "Internal Error: CRE Message Type (" + typeToExecute + ") is not valid";
					return default;
			}
		}

		(bool success, EDIMessage message) ExecuteICRMessage(MessageType typeToExecute)
		{
			TSWICRMessageBuilder msgBuilder;
			switch (typeToExecute)
			{
				case MessageType.Original:
					msgBuilder = new TSWICRMessageBuilder(EntryHeader, AdditionalInformation, Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.MessageBuilder.MessageTypes.Original, submitterCode);
					fLastHumanReadableStatus = "ICR Original Entry Message " + MessageManager.MessageReportingImmediateSend;
					return (true, msgBuilder.GenerateMessage());

				case MessageType.CancelEntry:
					msgBuilder = new TSWICRMessageBuilder(EntryHeader, AdditionalInformation, Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.MessageBuilder.MessageTypes.CancelEntry, submitterCode);
					fLastHumanReadableStatus = "ICR Cancel Entry Message " + MessageManager.MessageReportingImmediateSend;
					return (true, msgBuilder.GenerateMessage());

				case MessageType.Replacement:
				case MessageType.ReplaceHeaderAndLines:
					msgBuilder = new TSWICRMessageBuilder(EntryHeader, AdditionalInformation, Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.MessageBuilder.MessageTypes.Replacement, submitterCode);
					fLastHumanReadableStatus = "ICR Replacement Entry Message " + MessageManager.MessageReportingImmediateSend;
					return (true, msgBuilder.GenerateMessage());

				case MessageType.ResetToOriginal:
					Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.NotSentToCustoms;
					Declaration.JE_ECI_LastResponseStatus = LowValueConsignmentStatusList.Codes.NoStatusReported;
					EntryHeader.CH_IsActive = false;
					fLastHumanReadableStatus = "Job has been 'Reset to Original'";
					return (true, null);

				default:
					fLastHumanReadableStatus = "Internal Error: ICR Message Type (" + typeToExecute + ") is not valid";
					return default;
			}
		}

		protected virtual MessageBuilderFromEntryHeader GetNewMessageBuilder(ECIMessageGenerator.MessageTypes messageType)
		{
			return new MessageBuilderFromEntryHeader(EntryHeader, messageType);
		}

		protected override MessageType GetMessageTypeForSubmit()
		{
			MessageType result = MessageType.ReplaceHeaderAndLines;
			switch (EntryHeader.CH_EntryStatus)
			{
				case LowValueManifestStatusList.Codes.NotSentToCustoms:
					result = MessageType.Original;
					break;
				case LowValueManifestStatusList.Codes.ManifestRejected:
				case LowValueManifestStatusList.Codes.InspectionsAuditRequirements:
				case LowValueManifestStatusList.Codes.ManifestAccepted:
				case LowValueManifestStatusList.Codes.ManifestInError:
				case "":
					if (EntryHeader.EntryNumber == "" || EntryHeader.EntryNumber == "00000000")
					{
						result = MessageType.Original;
					}
					else
					{
						result = MessageType.ReplaceHeaderAndLines;
					}
					break;
			}

			return result;
		}

		protected override bool ValidateMessagingPreconditions()
		{
			bool result = base.ValidateMessagingPreconditions();
			if (result)
			{
				if (Declaration.IsECIWriteoffAndGSTIsApplicable && !Declaration.HasTheSameGSTDetailsOnAllInvoices)
				{
					result = false;
					fLastHumanReadableStatus = "Cannot Send when the Supplier GST Number / Prepaid combination is not the same on all Invoices.";
				}
			}

			return result;
		}

		protected override ZString GetSendMessageImpediment()
		{
			ZString result = new ZString();

			if (Declaration.JE_EntryStatus == LowValueConsignmentStatusList.Codes.ReadyForManifesting)
			{
				result = "Job has already been queued for Manifesting";
			}
			else if (EntryHeader.CH_EntryStatus == LowValueManifestStatusList.Codes.SentToCustoms)
			{
				result = "Job is Waiting for a Response";
			}
			else if (fOperationType == OperationType.CancelMessage && EntryHeader.EntryNumber.IsEmpty)
			{
				result = "Cannot Cancel when a Clearance Number has never been received From Customs.";
			}

			return result;
		}

		protected override ZString GetResetToOriginalImpediment()
		{
			ZString result = new ZString();
			if (EntryHeader.IsManifestEntry)
			{
				result = "You Cannot Reset a Manifest Entry to Original Once it has been Manifested";
			}

			return result;
		}

		public virtual bool CanQueueForManifesting
		{
			get
			{
				bool notSentToCustoms = EntryHeader.CH_EntryStatus == LowValueManifestStatusList.Codes.NotSentToCustoms;
				bool isAir = Declaration != null && Declaration.IsAir;
				return notSentToCustoms && isAir;
			}
		}

		[BusinessObjectTestExclude]
		public ZBool QueueForManifesting
		{
			get { return fQueueForManifesting; }
			set
			{
				fQueueForManifesting = CanQueueForManifesting ? value : ZBool.False;
				QueueForManifestingInfo.RefreshBinding();
				EnteredRemarksInfo.RefreshBinding();
			}
		}
		bool fQueueForManifesting;

		public ZPropertyInfo QueueForManifestingInfo
		{
			get { return GetZPropertyInfo(nameof(QueueForManifesting)); }
		}

		public bool EnteredRemarks_ReadOnly
		{
			get { return QueueForManifesting; }
		}
	}
}
