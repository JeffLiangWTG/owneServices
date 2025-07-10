using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using CusEntryHeader = Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader;
using CustomsNotificationCollector = Enterprise.Customs.Business.CustomsNotificationCollector;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff.Manifesting
{
	public class MessageManager : ECIWriteOff.MessageManager
	{
		public MessageManager(JobDeclaration declaration, OperationType operationType)
			: base(declaration, operationType)
		{
		}

		public MessageManager(JobDeclaration declaration, OperationType operationType, IAdditionalInformation additionalInformation, string submitterCode)
			: base(declaration, operationType, additionalInformation, submitterCode)
		{
			this.submitterCode = submitterCode;
		}
		readonly string submitterCode;

		#region Execute
		public override bool Execute()
		{
			bool result = false;
			if (AdditionalInformation != null && AdditionalInformation.SendManifest)
			{
				if (IsOkToExecute)
				{
					result = ExecuteTSWManifest();
					if (result)
					{
						result = SaveHandlingSaveExceptions(Declaration.Factory);
					}
				}
			}
			else
			{
				result = base.Execute();
			}

			return result;
		}
		#endregion

		bool ExecuteTSWManifest()
		{
			MessageType typeToExecute = MessageTypeToBeSent;
			if (Declaration.IsExport)
			{
				return ExecuteCREManifest(typeToExecute);
			}
			else if (Declaration.IsImport)
			{
				return ExecuteICRManifest(typeToExecute);
			}

			return false;
		}

		bool ExecuteCREManifest(MessageType typeToExecute)
		{
			bool result = false;
			TSWCREMessageBuilder msgBuilder;

			switch (typeToExecute)
			{
				// this needs to use MessageBuilderFromManifestEntryHeader here
				case MessageType.Original:
					msgBuilder = new TSWCREMessageBuilder(EntryHeader, AdditionalInformation, FormalEntry.MessageBuilder.MessageTypes.Original, submitterCode);
					msgBuilder.GenerateMessage();
					fLastHumanReadableStatus = "CRE Original Manifest Entry Message " + MessageManager.MessageReportingImmediateSend;
					result = true;
					break;

				case MessageType.CancelEntry:
					msgBuilder = new TSWCREMessageBuilder(EntryHeader, AdditionalInformation, FormalEntry.MessageBuilder.MessageTypes.CancelEntry, submitterCode);
					msgBuilder.GenerateMessage();
					fLastHumanReadableStatus = "CRE Cancel Manifest Entry Message " + MessageManager.MessageReportingImmediateSend;
					result = true;
					break;

				case MessageType.Replacement:
				case MessageType.ReplaceHeaderAndLines:
					msgBuilder = new TSWCREMessageBuilder(EntryHeader, AdditionalInformation, FormalEntry.MessageBuilder.MessageTypes.Replacement, submitterCode);
					msgBuilder.GenerateMessage();
					fLastHumanReadableStatus = "CRE Replacement Manifest Entry Message " + MessageManager.MessageReportingImmediateSend;
					result = true;
					break;

				case MessageType.ResetToOriginal:
					Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.NotSentToCustoms;
					Declaration.JE_ECI_LastResponseStatus = LowValueConsignmentStatusList.Codes.NoStatusReported;
					EntryHeader.CH_IsActive = false;
					fLastHumanReadableStatus = "Job has been 'Reset to Original'";
					result = true;
					break;

				default:
					fLastHumanReadableStatus = "Message type determined for Manifest sending is not a valid type of: original, cancellation, replacement or reset to original";
					result = false;
					break;
			}

			return result;
		}

		bool ExecuteICRManifest(MessageType typeToExecute)
		{
			bool result = false;
			TSWICRMessageBuilder msgBuilder;

			switch (typeToExecute)
			{
				case MessageType.Original:
					msgBuilder = new TSWICRMessageBuilder(EntryHeader, AdditionalInformation, FormalEntry.MessageBuilder.MessageTypes.Original, submitterCode);
					msgBuilder.GenerateMessage();
					fLastHumanReadableStatus = "ICR Original Manifest Entry Message " + MessageManager.MessageReportingImmediateSend;
					result = true;
					break;

				case MessageType.CancelEntry:
					msgBuilder = new TSWICRMessageBuilder(EntryHeader, AdditionalInformation, FormalEntry.MessageBuilder.MessageTypes.CancelEntry, submitterCode);
					msgBuilder.GenerateMessage();
					fLastHumanReadableStatus = "ICR Cancel Manifest Entry Message " + MessageManager.MessageReportingImmediateSend;
					result = true;
					break;

				case MessageType.Replacement:
				case MessageType.ReplaceHeaderAndLines:
					msgBuilder = new TSWICRMessageBuilder(EntryHeader, AdditionalInformation, FormalEntry.MessageBuilder.MessageTypes.Replacement, submitterCode);
					msgBuilder.GenerateMessage();
					fLastHumanReadableStatus = "ICR Replacement Manifest Entry Message " + MessageManager.MessageReportingImmediateSend;
					result = true;
					break;

				case MessageType.ResetToOriginal:
					Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.NotSentToCustoms;
					Declaration.JE_ECI_LastResponseStatus = LowValueConsignmentStatusList.Codes.NoStatusReported;
					EntryHeader.CH_IsActive = false;
					fLastHumanReadableStatus = "Job has been 'Reset to Original'";
					result = true;
					break;

				default:
					fLastHumanReadableStatus = "Message type determined for Manifest sending is not a valid type of: original, cancellation, replacement or reset to original";
					result = false;
					break;
			}

			return result;
		}

		#region EntryHeader
		public new CusEntryHeader EntryHeader
		{
			get { return (CusEntryHeader)base.EntryHeader; }
		}
		#endregion

		#region GetNewMessageBuilder
		protected override MessageBuilderFromEntryHeader GetNewMessageBuilder(ECIMessageGenerator.MessageTypes messageType)
		{
			return new MessageBuilderFromManifestEntryHeader(EntryHeader, messageType);
		}
		#endregion

		#region Declarations
		public JobDeclarationCollectionECIWriteOff Declarations
		{
			get
			{
				if (declarations == null)
				{
					declarations = EntryHeader.Declarations;
					declarations.Sort(JobDeclaration.Schema.JE_DeclarationReference, System.ComponentModel.ListSortDirection.Ascending);
				}
				return declarations;
			}
		}
		JobDeclarationCollectionECIWriteOff declarations;
		#endregion

		#region CanQueueForManifesting
		public override bool CanQueueForManifesting
		{
			get { return false; }
		}
		#endregion

		#region IsOKToSendWithMessagingErrors
		public override bool IsOKToSendWithMessagingErrors()
		{
			bool result = true;
			foreach (JobDeclaration declaration in Declarations)
			{
				if (!declaration.IsValidationSuspended)
				{
					declaration.LoadChildEditableObjects();
					declaration.RunPreSaveValidation();
				}
			}

			ZString fatalError = ZString.Empty;
			foreach (JobDeclaration declaration in Declarations)
			{
				if (!declaration.IsValidationSuspended)
				{
					if (declaration.HasErrors)
					{
						fatalError = "Declaration " + declaration.JE_DeclarationReference + ":" + System.Environment.NewLine + new CustomsNotificationCollector(declaration, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).ToUniqueMessageListString();
						break;
					}
				}
			}

			if (!fatalError.IsEmpty)
			{
				result = false;
				Declaration.MessageInitiator.NotifyUserOfAnInvalidOperation("Unable to Submit Message. Please fix this error:" + System.Environment.NewLine + System.Environment.NewLine + fatalError);
			}
			else
			{
				bool hasMessageErrors = false;
				foreach (JobDeclaration declaration in Declarations)
				{
					if (!declaration.IsValidationSuspended)
					{
						if (declaration.HasMessageErrors)
						{
							hasMessageErrors = true;
							break;
						}
					}
				}

				if (hasMessageErrors)
				{
					string messageText =
@"The manifest you are trying to send currently contains message errors.
This means that there is a high likelyhood that messages sent to Customs 
will fail to be processed.  

Are you sure you want to continue?";
					if (!Declaration.MessageInitiator.ContinueWithAction(messageText, "Send to Customs"))
					{
						result = false;
					}
				}
			}

			return result;
		}
		#endregion
	}
}
