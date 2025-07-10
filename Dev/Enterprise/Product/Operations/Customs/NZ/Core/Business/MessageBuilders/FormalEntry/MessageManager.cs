using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using CusEntryHeader = Enterprise.Customs.NZ.Business.Declaration.FormalEntry.CusEntryHeader;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry
{
	public class MessageManager : MessageManagerForDeclaration
	{
		public MessageManager(JobDeclaration declaration, OperationType operationType)
			: base(declaration, operationType)
		{
		}

		public new CusEntryHeader EntryHeader
		{
			get
			{
				if (Declaration != null && Declaration.IsCompletion)
				{
					foreach (CusEntryHeader header in Declaration.ActiveEntryHeaders)
					{
						if (header.GetType() == typeof(Declaration.FormalEntry.CompletionCusEntryHeader))
						{
							return header;
						}
					}
				}

				return (CusEntryHeader)base.EntryHeader;
			}
		}

		public MessageManager(JobDeclaration declaration, OperationType operationType, IAdditionalInformation additionalInformation)
			: base(declaration, operationType, additionalInformation)
		{
		}

		public override bool Execute()
		{
			bool result = IsOkToExecute;
			if (result)
			{
				if (EntryHeader.Declaration.JE_EDITransmitDate.IsEmpty)
				{
					EntryHeader.Declaration.JE_EDITransmitDate = EntryHeader.Declaration.CachedTodaysDate;
					// this can be an aggregate declaration, which is not to be persisted or merged. See Enterprise.Customs.Business.ConsolidateDeclaration
					if (!ConsolidatedDeclaration.IsConsolidated(EntryHeader.Declaration))
					{
						result = EntryHeader.Declaration.DoMerge();
					}
				}

				if (result)
				{
					var tuple = ExecuteTSWMessages();
					if (tuple.success)
					{
						using (new Customs.Business.MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(EntryHeader.Declaration))
						{
							Declaration.InvoiceLines.ResetHadErrorInLastResponse();
						}

						result = SendMessage(tuple.message);
					}
				}
			}

			return result;
		}

		(bool success, EDIMessage message) ExecuteTSWMessages()
		{
			MessageType typeToExecute = MessageTypeToBeSent;
			if (Declaration.IsImport)
			{
				return ExecuteIM1Message(typeToExecute);
			}
			else if (Declaration.IsExport || Declaration.IsDrawback)
			{
				return ExecuteEX1Message(typeToExecute);
			}
			else
			{
				return default;
			}
		}

		(bool success, EDIMessage message) ExecuteIM1Message(MessageType typeToExecute)
		{
			TSWIM1MessageBuilder msgBuilder;
			switch (typeToExecute)
			{
				case MessageType.Original:
					if (EntryHeader is Declaration.FormalEntry.CompletionCusEntryHeader)
					{
						msgBuilder = new TSWIM1MessageBuilder(EntryHeader, AdditionalInformation, MessageBuilder.MessageTypes.CompletionEntry);
						fLastHumanReadableStatus = "'Original Completion Entry' Message " + GetGenerationMessage(EntryHeader.CH_EDITransmitDate);
						return (true, msgBuilder.GenerateMessage());
					}
					else
					{
						msgBuilder = new TSWIM1MessageBuilder(EntryHeader, AdditionalInformation, MessageBuilder.MessageTypes.Original);
						fLastHumanReadableStatus = "Original Entry Message " + GetGenerationMessage(EntryHeader.CH_EDITransmitDate);
						return (true, msgBuilder.GenerateMessage());
					}

				case MessageType.CancelEntry:
					msgBuilder = new TSWIM1MessageBuilder(EntryHeader, AdditionalInformation, MessageBuilder.MessageTypes.CancelEntry);
					fLastHumanReadableStatus = "Cancel Entry Message " + GetGenerationMessage(EntryHeader.CH_EDITransmitDate);
					return (true, msgBuilder.GenerateMessage());

				case MessageType.Replacement:
					msgBuilder = new TSWIM1MessageBuilder(EntryHeader, AdditionalInformation, MessageBuilder.MessageTypes.Replacement);
					fLastHumanReadableStatus = "Replacement Entry Message " + GetGenerationMessage(EntryHeader.CH_EDITransmitDate);
					return (true, msgBuilder.GenerateMessage());

				case MessageType.ResetToOriginal:
					Declaration.ResetToOriginal();
					fLastHumanReadableStatus = "Job has been 'Reset to Original'";
					return (true, null);

				default:
					fLastHumanReadableStatus = "Internal Error: IM1 Message Type (" + typeToExecute + ") is not valid";
					return default;
			}
		}

		(bool success, EDIMessage message) ExecuteEX1Message(MessageType typeToExecute)
		{
			TSWEX1MessageBuilder msgBuilder;
			switch (typeToExecute)
			{
				case MessageType.Original:
					if (EntryHeader is Declaration.FormalEntry.CompletionCusEntryHeader)
					{
						msgBuilder = new TSWEX1MessageBuilder(EntryHeader, AdditionalInformation, MessageBuilder.MessageTypes.CompletionEntry);
						fLastHumanReadableStatus = "'Original Completion Entry' Message " + GetGenerationMessage(EntryHeader.CH_EDITransmitDate);
						return (true, msgBuilder.GenerateMessage());
					}
					else
					{
						msgBuilder = new TSWEX1MessageBuilder(EntryHeader, AdditionalInformation, MessageBuilder.MessageTypes.Original);
						fLastHumanReadableStatus = "Original Entry Message " + GetGenerationMessage(EntryHeader.CH_EDITransmitDate);
						return (true, msgBuilder.GenerateMessage());
					}

				case MessageType.CancelEntry:
					msgBuilder = new TSWEX1MessageBuilder(EntryHeader, AdditionalInformation, MessageBuilder.MessageTypes.CancelEntry);
					fLastHumanReadableStatus = "Cancel Entry Message " + GetGenerationMessage(EntryHeader.CH_EDITransmitDate);
					return (true, msgBuilder.GenerateMessage());

				case MessageType.ResetToOriginal:
					Declaration.ResetToOriginal();
					fLastHumanReadableStatus = "Job has been 'Reset to Original'";
					return (true, null);

				case MessageType.Replacement:
					msgBuilder = new TSWEX1MessageBuilder(EntryHeader, AdditionalInformation, MessageBuilder.MessageTypes.Replacement);
					fLastHumanReadableStatus = "Replacement Entry Message " + GetGenerationMessage(EntryHeader.CH_EDITransmitDate);
					return (true, msgBuilder.GenerateMessage());

				default:
					fLastHumanReadableStatus = "Internal Error: EX1 Message Type (" + typeToExecute + ") is not valid";
					return default;
			}
		}

		string GetGenerationMessage(ZDateTime eDITransmitDate)
		{
			return (eDITransmitDate.Date > EntryHeader.Declaration.CachedTodaysDate ? MessageReportingQueuedSend + eDITransmitDate.ToShortDateString() : MessageReportingImmediateSend);
		}
		public const string MessageReportingQueuedSend = "Generated and Queued to be sent by Service Tasks on: ";

		protected override ZString GetErrorsForEnteredValuesCore()
		{
			ZString result = base.GetErrorsForEnteredValuesCore();
			if (EnteredPinNumber != UsersPin.DecryptedPinCode)
			{
				result += "Invalid PIN: Does not match your PIN entered in the Staff Master File.";
			}
			if (EnteredOverrideFlag && EnteredRemarks.IsEmpty)
			{
				result += "Invalid Remarks: Must have Remarks when using the Override Flag.";
			}
			return result;
		}

		protected CurrentUsersPin UsersPin
		{
			get
			{
				if (fUsersPin == null)
				{
					fUsersPin = new CurrentUsersPin(EntryHeader.Factory, Declaration.IsInTestMode);
				}
				return fUsersPin;
			}
		}
		CurrentUsersPin fUsersPin;

		protected override MessageType GetMessageTypeForSubmit()
		{
			if (EntryHeader.EntryNumber == "" || EntryHeader.EntryNumber == "00000000")
			{
				return MessageType.Original;
			}

			return MessageType.Replacement;
		}

		protected override bool ValidateMessagingPreconditions()
		{
			bool result = base.ValidateMessagingPreconditions();
			if (result)
			{
				if (!Declaration.IsPrimaryIndustriesImportDeclaration && IsUserMissingBrokerIdOrPin)
				{
					result = false;
					fLastHumanReadableStatus = ErrorMustEnterBrokerIDAndPINAgainstStaffRecord;
				}
				else if (EntryHeader.CH_EntryStatus == FormalEntryStatusList.Codes.NotSentToCustoms && EntryHeader.CH_EDITransmitDate < ZDateTime.Today)
				{
					result = false;
					fLastHumanReadableStatus = Declaration.IsExport ? ExportEDITransmitDateInThePast : ErrorEDITransmitDateCannotBeInThePast;
				}
				else if (AreMergedLinesTooMany(out var tooManyEntryLinesMessage))
				{
					result = false;
					fLastHumanReadableStatus = tooManyEntryLinesMessage;
				}
			}

			return result;
		}

		bool AreMergedLinesTooMany(out string tooManyEntryLinesMessage)
		{
			var tooMany = false;
			tooManyEntryLinesMessage = string.Empty;
			if (EntryHeader.MergedLines.Count > MaximumEntryLinesSentToCustomsInOneEntry)
			{
				tooMany = true;
				tooManyEntryLinesMessage = ErrorCantSendWithMoreLinesThanCustomsWillAccept + MaximumEntryLinesSentToCustomsInOneEntry + ".";
			}

			return tooMany;
		}

		bool IsUserMissingBrokerIdOrPin
		{
			get
			{
				CurrentUsersPin usersPin = new CurrentUsersPin(EntryHeader.Factory, Declaration.IsInTestMode);
				return usersPin.BrokerID.IsEmpty || usersPin.DecryptedPinCode.IsEmpty;
			}
		}

		public const string ErrorMustEnterBrokerIDAndPINAgainstStaffRecord = "You must enter a Broker ID and PIN against your Staff Record (Under Broker Tab) before sending to Customs.";

		public static string ErrorEDITransmitDateCannotBeInThePast
		{
			get
			{
				return @"You cannot Submit to Customs if your EDI Transmit Date is in the past. 

Please update your EDI Transmit Date to either today or a date in the future 
and check the exchange rates on your Invoice Headers. 

Then you can Submit to Customs.
";
			}
		}

		public static string ExportEDITransmitDateInThePast
		{
			get { return @"You cannot Submit to Customs if your EDI Transmit Date is in the past."; }
		}

		// NZ Customs wont accept more than X merged lines
		public const int MaximumEntryLinesSentToCustomsInOneEntry = 2000;
		public const string ErrorCantSendWithMoreLinesThanCustomsWillAccept = @"You have generated more Entry Lines than Customs' CUSMOD system can accept. 

Please either select a different Merge type, or reduce the number of commercial invoices on this Declaration. 

NB: The maximum number of Entry Lines you can send to Customs is ";

		protected override ZString GetSendMessageImpediment()
		{
			ZString result = new ZString();
			var status = Declaration.JE_EntryStatus;
			switch (status)
			{
				case FormalEntryStatusList.Codes.ManualEntryCannotBeSentToCustoms:
					result = "Job is a 'Manual Entry'";
					break;
				case FormalEntryStatusList.Codes.QueuedForSending:
					result = "Message is Already Queued to be sent on: " + EntryHeader.DateMessageQueuedToBeSentOn.ToShortDateString();
					break;
				case FormalEntryStatusList.Codes.SentToCustoms:
					result = "Job is Waiting for a Response";
					break;
				case FormalEntryStatusList.Codes.AgencyResponsePending:
					result = "Job is not in a state that is valid for sending:\r\nThis entry is currently waiting for a response from the TSW/Customs system.";
					break;
				default:
					if (fOperationType == OperationType.CancelMessage && EntryHeader.EntryNumber == "")
					{
						result = "Cannot Cancel when a Clearance Number has never been received From Customs.";
					}
					break;
			}

			if (result == "")
			{
				string message;
				if (!Declaration.Invoices.AreChargesBalancedForInvoices(out message))
				{
					result = string.Format("Current apportionment is not balanced, {0}. Please click Brokerage > Perform Apportionment to correct calculation or remove the specified cause of imbalance.", message);
				}
			}

			return result;
		}

		protected override ZString GetResetToOriginalImpediment()
		{
			ZString result = new ZString();
			switch (EntryHeader.CH_EntryStatus)
			{
				case FormalEntryStatusList.Codes.ManualEntryCannotBeSentToCustoms:
					result = "Job is a 'Manual Entry'";
					break;
			}
			return result;
		}

		protected GlbStaff CurrentUser
		{
			get { return fCurrentUser ?? (fCurrentUser = EntryHeader.Factory.Load<GlbStaff>(Env.CurrentUser.PK)); }
		}
		GlbStaff fCurrentUser;

		public ZString BrokersDeclaration
		{
			get { return fBrokersDeclaration ?? (fBrokersDeclaration = GetBrokersDeclaration()); }
		}
		string fBrokersDeclaration;

		public ZPropertyInfo BrokersDeclarationInfo
		{
			get { return GetZPropertyInfo(nameof(BrokersDeclaration)); }
		}

		string GetBrokersDeclaration()
		{
			return "I " + Env.CurrentUser.FullName + " (" + UsersPin.BrokerID + ") HEREBY DECLARE THAT THE PARTICULARS CONTAINED IN THIS ELECTRONIC ENTRY "
				+ "MESSAGE ARE TRUE AND CORRECT AND ARE IN ACCORDANCE WITH THE CUSTOMS AND EXCISE ACT 1996. "
				+ "VALIDATED BY THE ENDORSEMENT OF MY ELECTRONIC SIGNATURE NUMBER.";
		}

		[MaxLength(6)]
		[Password]
		public ZString EnteredPinNumber
		{
			get { return fEnteredPinNumber; }
			set
			{
				if (fEnteredPinNumber != value)
				{
					CheckMaximumLength(EnteredPinNumberInfo, value);
					SetNonPersistentPropertyValue(EnteredPinNumberInfo, ref fEnteredPinNumber, value);
					SetPDOFlagIfCurrentSendIsAmendment();
				}
			}
		}
		ZString fEnteredPinNumber;
		public ZPropertyInfo EnteredPinNumberInfo
		{
			get { return GetZPropertyInfo(nameof(EnteredPinNumber)); }
		}

		public ZBool EnteredOverrideFlag
		{
			get { return EntryHeader.CH_OverrideIndicator; }
			set { EntryHeader.CH_OverrideIndicator = value; }
		}
		public ZPropertyInfo EnteredOverrideFlagInfo
		{
			get { return EntryHeader == null ? null : GetWrappedZPropertyInfo(nameof(EnteredOverrideFlag), x => EntryHeader.CH_OverrideIndicatorInfo); }
		}

		public ZBool EnteredPDOFlag
		{
			get { return EntryHeader.Declaration.JE_PDOOtherInfoValue; }
			set { EntryHeader.Declaration.JE_PDOOtherInfoValue = value; }
		}
		public ZPropertyInfo EnteredPDOFlagInfo
		{
			get { return EntryHeader == null ? null : GetWrappedZPropertyInfo(nameof(EnteredPDOFlag), x => EntryHeader.Declaration.JE_PDOOtherInfoValueInfo); }
		}

		protected override bool GetMessageIsCurrentlyQueued()
		{
			return EntryHeader.Declaration.IsQueuedForSending;
		}

		protected override string GetMessageIsCurrentlyQueuedShouldWeCancelQuestion()
		{
			return LastHumanReadableStatus + CurrentlyQueuedMessageSuffixToTurnLastHumanReadableStatusIntoAnOverrideQuestion;
		}

		public static string CurrentlyQueuedMessageSuffixToTurnLastHumanReadableStatusIntoAnOverrideQuestion { get { return "\r\n\r\nDo you want to Cancel the Queued Message and reset the EDI Transmit Date so you can re-submit to Customs?"; } }

		protected override void CancelCurrentlyQueuedMessageInternal()
		{
			EntryHeader.Declaration.CancelQueuedMessagesAndResetEDITransmitDate();
		}

		void SetPDOFlagIfCurrentSendIsAmendment()
		{
			if (!EnteredPDOFlag
				&& MessageTypeToBeSent != MessageType.Original
				&& !EnteredPinNumber.IsEmpty
				&& EnteredPinNumber == UsersPin.DecryptedPinCode)
			{
				EnteredPDOFlag = true;
				EnteredPDOFlagInfo.RefreshBinding();
			}
		}
	}
}
