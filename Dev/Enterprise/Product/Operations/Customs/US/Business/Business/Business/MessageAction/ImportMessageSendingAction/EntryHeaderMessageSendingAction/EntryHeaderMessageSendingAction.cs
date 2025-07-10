using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public sealed class EntryHeaderMessageSendingAction : ImportMessageSendingAction
	{
		public EntryHeaderMessageSendingAction(CusEntryHeader entry, ImportMessageStatusList.MessageType messageType, ImportMessageSendingActionCollection actions)
			: this(entry, messageType, messageType.ToString(), actions)
		{
		}

		/// <param name="entry">An entry to generate a message against</param>
		/// <param name="messageTypeDesc">this is the entity type + entry number. For other types of messages like Bill of lading update, use 'entry' instead of 'entry summary' to make it clear</param>
		public EntryHeaderMessageSendingAction(CusEntryHeader entry, ImportMessageStatusList.MessageType messageType, string messageTypeDesc, ImportMessageSendingActionCollection actions)
			: base(entry, messageType, actions)
		{
			if (!entry.IsRelevantFor(messageType))
			{
				throw new InvalidOperationException("The passed message type is not relevant for the entry");
			}
			this.messageTypeDesc = messageTypeDesc;

			if (entry.IsFormalEntry)
			{
				if (IsOriginalOrReplacement)
				{
					DefaultPSCExplanationTextFromLastFailedTransmission();
					var declaration = entry.Declaration;

					if (IsPaidRelevant)
					{
						US_Paid = declaration.US_Paid;

						if (US_Paid != YesNoDefaultList.Codes.Yes)
						{
							if (declaration.RelatedStatement != null && declaration.RelatedStatement.IsFinal || declaration.US_PSC)
							{
								US_Paid = YesNoDefaultList.Codes.Yes;
							}
							else if (declaration.US_PaymentType != PaymentTypeList.Codes.IndividualBasis || declaration.US_PaymentDueDate.IsInTheFutureDatePartOnly)
							{
								US_Paid = YesNoDefaultList.Codes.No;
							}
						}
					}
				}
			}

			if (entry.IsACECargoRelease)
			{
				if (actions.messageSendingMessageType == ImportMessageSendingMessageType.Deletion)
				{
					US_SE_MultipleDispositionsIndic = entry.Declaration.US_SEMultiCargoDispInd;
				}
			}
		}

		bool IsOriginalOrReplacement
		{
			get
			{
				return actions.messageSendingMessageType == ImportMessageSendingMessageType.Original ||
					   actions.messageSendingMessageType == ImportMessageSendingMessageType.Replacement;
			}
		}

		protected override bool IsPaidRelevantCore
		{
			get
			{
				var declaration = entry.Declaration;
				return declaration.IsACE && !declaration.US_PSC &&
					  IsEntrySummary && (actions.IsOriginal || actions.IsAmendment) && entry.HasBeenLodgedAtCustoms;
			}
		}

		public override ZString US_MessageContents
		{
			get { return IsSerialiseMessageContentsSupported() ? GetSerialiseMessageContents() : base.US_MessageContents; }
		}

		bool IsSerialiseMessageContentsSupported()
		{
			return IsEntrySummary || IsBorderCargoRelease || IsCargoRelease || IsACECargoRelease || IsTemporaryImportationBond;
		}

		ZString GetSerialiseMessageContents()
		{
			if (!messageContentsCached.HasValue)
			{
				string applicationIdentifier = null;
				IEnumerable<MessageBlock> messageBlocks = null;
				switch (messageType)
				{
					case ImportMessageStatusList.MessageType.EntrySummary:
						if (entry.Declaration.IsACE)
						{
							var sendingOption = ACEEntrySummaryMessageSendingOption.New(this);
							messageBlocks = new ACEEntrySummaryBlockBuilder(GetAction(), entry, US_PSCExplanation, PSCReasonCodes, sendingOption).Build();
							applicationIdentifier = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
						}
						else
						{
							messageBlocks = new EntrySummaryBlockBuilder(entry, ApplicationIdentifierCodeList.Codes.EntrySummary).Build(GetAction(), US_CertifyCargoRelease);
							applicationIdentifier = ApplicationIdentifierCodeList.Codes.EntrySummary;
						}
						break;
					case ImportMessageStatusList.MessageType.BorderCargoRelease:
						messageBlocks = new BorderCargoReleaseBlockBuilder(entry, ApplicationIdentifierCodeList.Codes.BorderCargoRelease).Build(GetAction());
						applicationIdentifier = ApplicationIdentifierCodeList.Codes.BorderCargoRelease;
						break;
					case ImportMessageStatusList.MessageType.CargoRelease:
						messageBlocks = new CargoReleaseBlockBuilder(entry, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions).Build(GetAction(), US_CertifyCargoRelease);
						applicationIdentifier = ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions;
						break;
					case ImportMessageStatusList.MessageType.ACECargoRelease:
						messageBlocks = new SimplifiedEntryBlockBuilder(entry, ACEEntrySummaryMessageSendingOption.New(this)).Build(GetAction());
						applicationIdentifier = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
						break;
					case ImportMessageStatusList.MessageType.TemporaryImportationBond:
						messageBlocks = new RequestToExtendTIBMessageBuilder(entry, actions.messageSendingMessageType).Build();
						applicationIdentifier = ACEApplicationIdentifierCodeList.Codes.TIBExtensionOrClosure;
						break;
				}

				BlockControlGenerator generator = GetBlockControlGenerator(applicationIdentifier);
				generator.B.ApplicationIdentifier = applicationIdentifier;
				generator.AddMessageBlocks(messageBlocks);
				messageContentsCached = generator.Serialise(true);
			}
			return messageContentsCached.Value;
		}
		ZString? messageContentsCached;

		BlockControlGenerator GetBlockControlGenerator(string applicationIdentifier)
		{
			BlockControlGenerator generator = null;

			if (IsInACS(applicationIdentifier))
			{
				var aBIgenerator = new ABIInputBlockControlGenerator(entry);
				aBIgenerator.B.SetupBlockBDetails(entry);
				generator = aBIgenerator;
			}
			else
			{
				var aCEgenerator = new ACEInputBlockControlGenerator(entry);
				aCEgenerator.B.SetupBlockBDetails(entry);
				generator = aCEgenerator;
			}

			return generator;
		}

		static bool IsInACS(string applicationIdentifier)
		{
			return applicationIdentifier == ApplicationIdentifierCodeList.Codes.EntrySummary
				|| applicationIdentifier == ApplicationIdentifierCodeList.Codes.BorderCargoRelease
				|| applicationIdentifier == ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions;
		}

		UpdateActionCode GetAction()
		{
			var result = UpdateActionCode.Add;

			if (actions.IsAmendment)
			{
				result = IsACECargoRelease && US_SE_ActionType == ACECargoReleaseActionType.Codes.Update ? UpdateActionCode.Update : UpdateActionCode.Replace;
			}
			else if (actions.IsWithdrawal)
			{
				result = UpdateActionCode.Delete;
			}
			return result;
		}

		void ReGenerateMessageContents()
		{
			var oldMessageContents = ZString.Empty;
			if (messageContentsCached.HasValue)
			{
				oldMessageContents = messageContentsCached.Value;
				messageContentsCached = null;
			}
			US_MessageContentsInfo.RefreshBinding(oldMessageContents);
		}

		public override ZBool US_CertifyCargoRelease
		{
			get { return base.US_CertifyCargoRelease; }
			set
			{
				var oldValue = US_CertifyCargoRelease;
				base.US_CertifyCargoRelease = value;
				if (oldValue != US_CertifyCargoRelease)
				{
					if (IsSerialiseMessageContentsSupported())
					{
						ReGenerateMessageContents();
					}
					ValidationModesCalculator.UpdateValidationModes(ValidationModes.CargoRelease, value);
				}
			}
		}

		ValidationModesCalculator ValidationModesCalculator
		{
			get { return new ValidationModesCalculator(entry.Declaration); }
		}

		public override ZString US_SE_ActionType
		{
			get { return base.US_SE_ActionType; }
			set
			{
				var oldValue = US_SE_ActionType;
				base.US_SE_ActionType = value;

				if (oldValue != US_SE_ActionType)
				{
					if (IsSerialiseMessageContentsSupported())
					{
						ReGenerateMessageContents();
					}
				}
			}
		}

		public override ZString US_MessageDescription
		{
			get { return string.Format("{0} ({1})", messageTypeDesc, entry.EntryNumber).Trim(); }
		}

		public override ZBool US_SendMessage
		{
			get { return base.US_SendMessage; }
			set
			{
				base.US_SendMessage = value;

				if (value)
				{
					SetDefaults();
				}
				else
				{
					US_CertifyCargoRelease = false;
				}
			}
		}

		public bool US_JobReadyForPosting_ReadOnly
		{
			get { return !US_SendMessage || actions.messageSendingMessageType == ImportMessageSendingMessageType.Deletion; }
		}

		public new EntryHeaderMessageSendingActionValidation Validation
		{
			get { return (EntryHeaderMessageSendingActionValidation)base.Validation; }
		}

		public override ZBool US_AcknowledgeAndSign
		{
			get { return base.US_AcknowledgeAndSign; }
			set
			{
				var oldValue = US_AcknowledgeAndSign;
				base.US_AcknowledgeAndSign = value;
				if (US_SendMessage && US_AcknowledgeAndSign && US_DateOfDeclaration.IsEmpty)
				{
					US_DateOfDeclaration = ZDateTime.Today;
				}
				if (oldValue != US_AcknowledgeAndSign && IsSerialiseMessageContentsSupported())
				{
					ReGenerateMessageContents();
				}
			}
		}

		public override ZString US_PSCExplanation
		{
			get { return base.US_PSCExplanation; }
			set
			{
				var oldValue = US_PSCExplanation;
				base.US_PSCExplanation = value;
				if (oldValue != US_PSCExplanation && IsSerialiseMessageContentsSupported())
				{
					ReGenerateMessageContents();
				}
			}
		}

		public override void CopyPSCReasonsAndExplanation()
		{
			if (ShouldPSCReasonAndExplanationBeSaved)
			{
				PSCReasonCodes.SaveLastPSCReasonCodes();
				SavePSCExplanation();
			}
		}

		protected override USImportMessageSendingActionValidation GetNewValidation()
		{
			return new EntryHeaderMessageSendingActionValidation(this);
		}

		protected override bool GetUS_DateOfDeclarationInfoReadOnly()
		{
			return !US_AcknowledgeAndSign;
		}

		#region ACE Cargo Release Cancellation Details

		public override ZString US_SE_ContactName
		{
			get { return base.US_SE_ContactName; }
			set
			{
				var oldValue = US_SE_ContactName;
				base.US_SE_ContactName = value;

				if (oldValue != US_SE_ContactName && IsSerialiseMessageContentsSupported())
				{
					ReGenerateMessageContents();
				}
			}
		}

		public override ZString US_SE_ContactPhone
		{
			get { return base.US_SE_ContactPhone; }
			set
			{
				var oldValue = US_SE_ContactPhone;
				base.US_SE_ContactPhone = value;
				if (oldValue != US_SE_ContactPhone && IsSerialiseMessageContentsSupported())
				{
					ReGenerateMessageContents();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(USImportMessageSendingActionLookups.ReasonCodeList))]
		public override ZString US_SE_ReasonCode
		{
			get { return base.US_SE_ReasonCode; }
			set
			{
				var oldValue = US_SE_ReasonCode;
				base.US_SE_ReasonCode = value;
				if (oldValue != value && IsSerialiseMessageContentsSupported())
				{
					ReGenerateMessageContents();
				}
				US_SE_ReasonCodeInfo.RefreshBinding(oldValue);
				if (!ReasonCodeList.IsReferenceNoRequired(US_SE_ReasonCode))
				{
					US_SE_ReferenceNo = ZString.Empty;
				}
				Validation.ValidateUS_SE_ReasonCode();
			}
		}

		public override ZString US_SE_ReferenceNo
		{
			get { return base.US_SE_ReferenceNo; }
			set
			{
				var oldValue = US_SE_ReferenceNo;
				base.US_SE_ReferenceNo = value;
				if (oldValue != value && IsSerialiseMessageContentsSupported())
				{
					ReGenerateMessageContents();
				}
				US_SE_ReferenceNoInfo.RefreshBinding();
				Validation.ValidateUS_SE_ReferenceNo();
			}
		}

		public override bool US_SE_ReferenceNo_ReadOnly
		{
			get { return !ReasonCodeList.IsReferenceNoRequired(US_SE_ReasonCode); }
		}

		public override ZBool US_SE_MultipleDispositionsIndic
		{
			get { return base.US_SE_MultipleDispositionsIndic; }
			set
			{
				var oldValue = US_SE_MultipleDispositionsIndic;
				base.US_SE_MultipleDispositionsIndic = value;
				if (oldValue != value && IsSerialiseMessageContentsSupported())
				{
					ReGenerateMessageContents();
				}
			}
		}

		public override ZBool US_SE_DISIndicator
		{
			get { return base.US_SE_DISIndicator; }
			set
			{
				var oldValue = US_SE_DISIndicator;
				base.US_SE_DISIndicator = value;
				if (!US_SE_DISIndicator)
				{
					US_SE_DISIDRefNo = string.Empty;
				}

				if (oldValue != value && IsSerialiseMessageContentsSupported())
				{
					ReGenerateMessageContents();
				}
			}
		}

		[List(nameof(entry) + "." + nameof(CusEntryHeader.Declaration) + "." + nameof(JobDeclaration.DISDocumentIDList))]
		public override ZString US_SE_DISIDRefNo
		{
			get { return base.US_SE_DISIDRefNo; }
			set
			{
				var oldValue = US_SE_DISIDRefNo;
				base.US_SE_DISIDRefNo = value;
				if (oldValue != value && IsSerialiseMessageContentsSupported())
				{
					ReGenerateMessageContents();
				}
			}
		}

		internal ZString ReferenceIdentifierQualifier
		{
			get
			{
				switch (US_SE_ReasonCode)
				{
					case ReasonCodeList.Codes.EntryReplacedBy7512:
						return ReferenceIdentifierCodeList.Codes.ReplacementInBondNumber;
					case ReasonCodeList.Codes.MerchandiseClearedByAnother:
						return ReferenceIdentifierCodeList.Codes.ReplacementEntryNumber;
					case ReasonCodeList.Codes.EntryReplacedByFTZ:
						return ReferenceIdentifierCodeList.Codes.ReplacementFTZAdmissionNumber;
					default:
						return ReferenceIdentifierCodeList.Codes.UserDefinedReferenceNumber;
				}
			}
		}

		#endregion

		protected override void SetDefaults()
		{
			base.SetDefaults();
			var declaration = entry.Declaration;

			if (declaration.DispositionCodes.Count == 0)
			{
				if (IsCargoRelease)
				{
					US_CertifyCargoRelease = true;
				}
				else if (IsEntrySummary && !entry.HasCargoReleaseBeenCertified && !entry.IsCargoReleaseBeingCertified)
				{
					if (!(declaration.US_EnableCRL && entry.IsFormalEntry))//if standalone cargo release is activated, then only certify on the CRL message not the entry summary
					{
						US_CertifyCargoRelease = declaration.US_CertifyCargoRelease;
					}
				}
			}

			if (actions.messageSendingMessageType != ImportMessageSendingMessageType.Deletion && IsEntrySummary)
			{
				US_JobReadyForPosting = declaration.US_JobReadyForPost;
			}
		}

		protected override bool IsElectronicInvoicing
		{
			get { return iEntry.IsElectronicInvoicing; }
		}

		internal CusEntryHeader entry
		{
			get { return (CusEntryHeader)bizObj; }
		}

		internal ICusEntryHeader iEntry
		{
			get { return entry; }
		}

		protected override Customs.Business.SingleMessageManager GetMessageManager()
		{
			return new EntryHeaderSingleMessageManager(entry, this);
		}

		protected override bool GetUS_CertifyCargoReleaseInfoReadOnly()
		{
			return !IsCargoRelease && !IsEntrySummary;
		}

		void DefaultPSCExplanationTextFromLastFailedTransmission()
		{
			if (entry.Declaration.US_PSC && entry.HasBeenLodgedAtCustoms)
			{
				if (entry.CH_Status == ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal || entry.CH_Status == ImportMessageStatusList.Codes.ErrorEntrySummaryReplace)
				{
					var message = (MQEDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
					if (message != null)
					{
						var ens36s = message.MessageBlock.MessageBlocks.OfType<AENS36>();

						var result = new ZStringBuilder();

						foreach (var ens36 in ens36s)
						{
							result.Append(ens36.PSCFilingExplanationText);
						}

						if (!result.IsEmpty)
						{
							US_PSCExplanation = result.ToString();
						}
					}
				}
			}
		}

		void SavePSCExplanation()
		{
			if (entry != null)
			{
				var pscExplanation = entry.PSCExplanation;
				if (pscExplanation == null)
				{
					pscExplanation = Factory.New<PSCExplanationCusAddInfo>();
					pscExplanation.B7_ParentID = entry.PK;
					pscExplanation.B7_ParentTableCode = CusEntryHeaderSchema.Constants.Prefix;
				}
				pscExplanation.B7_AddInfoData = US_PSCExplanation.Left(PSCExplanationCusAddInfo.Schema.B7_AddInfoDataMaxLength);
			}
		}

		readonly string messageTypeDesc;

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (entry != null && entry.IsACE && IsOriginalOrReplacement && eBondLogger.GetAutoSendEvent(entry.Logs) != null)
			{
				var sendMessageAction = new AutoSendMessageCusAddInfo.Loader(Factory).Load(entry);
				if (sendMessageAction == null)
				{
					sendMessageAction = Factory.New<AutoSendMessageCusAddInfo>();
					sendMessageAction.B7_ParentID = entry.PK;
					sendMessageAction.B7_ParentTableCode = CusEntryHeaderSchema.Constants.Prefix;
				}

				this.US_PSCExplanation = ZString.Empty;
				sendMessageAction.B7_AddInfoData = this.Serialize().Replace("\r\n", "").Replace(" ", "");
			}
		}
	}
}
