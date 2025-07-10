using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public enum ImportMessageSendingMessageType
	{
		Original,
		Replacement,
		Deletion,
		ExtendTIB,
		ConsigneeNameAddressAdd,
		EntrySummaryQuery,
		VisaQuotaQuery,
		ADDCVD,
		StandAlonePriorNotice,
		ClosureTIB,
		EBondRequest
	}

	public enum ImportMessageSendingMessageTypeAdditionalFilter
	{
		EntrySummary,
		CargoRelease,
		None
	}

	public class ImportMessageSendingActionCollection : NonPersistentBusinessObjectCollection<ImportMessageSendingAction>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ImportMessageSendingActionCollection(JobDeclaration declaration, ImportMessageSendingMessageType messageType, Func<CusEntryHeader, bool> entryFilter = null)
			: base(declaration.Factory)
		{
			this.declaration = declaration;
			this.messageSendingMessageType = messageType;
			PopulateElements(entryFilter);

			if (Count == 1 && ShouldDefaultSendMessageIfThereIsOnlyOneElement)
			{
				ImportMessageSendingAction action = this[0];

				using (action.SuspendSettingHasChanges())
				{
					this[0].US_SendMessage = true;
				}
			}
		}

		public readonly JobDeclaration declaration;
		public readonly ImportMessageSendingMessageType messageSendingMessageType;

		public void CopyPSCReasonsAndExplanation()
		{
			foreach (ImportMessageSendingAction action in this)
			{
				action.CopyPSCReasonsAndExplanation();
			}
		}

		public void SelectAll()
		{
			foreach (ImportMessageSendingAction action in this)
			{
				action.US_SendMessage = true;
			}
		}

		public void UnSelectAll()
		{
			foreach (ImportMessageSendingAction action in this)
			{
				action.US_SendMessage = false;
			}
		}

		protected virtual bool ShouldDefaultSendMessageIfThereIsOnlyOneElement
		{
			get { return true; }
		}

		public bool IsOriginal
		{
			get { return messageSendingMessageType == ImportMessageSendingMessageType.Original; }
		}

		public bool IsAmendment
		{
			get { return messageSendingMessageType == ImportMessageSendingMessageType.Replacement; }
		}

		public bool IsWithdrawal
		{
			get { return messageSendingMessageType == ImportMessageSendingMessageType.Deletion; }
		}

		public bool IsExtendTIB
		{
			get { return messageSendingMessageType == ImportMessageSendingMessageType.ExtendTIB; }
		}

		public bool IsStandAlonePriorNotice
		{
			get { return messageSendingMessageType == ImportMessageSendingMessageType.StandAlonePriorNotice; }
		}

		public bool IsEntrySummaryQuery
		{
			get { return messageSendingMessageType == ImportMessageSendingMessageType.EntrySummaryQuery; }
		}

		public T FindFirstElement<T>(ImportMessageStatusList.MessageType messageType)
			where T : ImportMessageSendingAction
		{
			foreach (ImportMessageSendingAction action in this)
			{
				if (action is T && action.messageType == messageType)
				{
					return (T)action;
				}
			}

			return null;
		}

		public bool HasAtLeastOneToSendMessageFor
		{
			get
			{
				foreach (ImportMessageSendingAction action in this)
				{
					if (action.US_SendMessage)
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool SendMessage
		{
			get
			{
				if (!IsCancelled)
				{
					foreach (ImportMessageSendingAction action in this)
					{
						if (action.US_SendMessage)
						{
							return true;
						}
					}
				}

				return false;
			}
		}

		public void CalculatesAndSetValidationModesOnDeclaration()
		{
			ValidationModes result = ValidationModes.None;

			foreach (ImportMessageSendingAction action in this)
			{
				if (action.US_SendMessage)
				{
					result |= action.ValidateMode;
				}
			}

			declaration.ValidationModes = result;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public IReadOnlyList<EntryHeaderMessageSendingAction> EntryHeaderActions
		{
			get
			{
				if (entryHeaderActions == null)
				{
					List<EntryHeaderMessageSendingAction> result = new List<EntryHeaderMessageSendingAction>();
					foreach (ImportMessageSendingAction action in this)
					{
						EntryHeaderMessageSendingAction entryHeaderAction = action as EntryHeaderMessageSendingAction;
						if (entryHeaderAction != null)
						{
							result.Add(entryHeaderAction);
						}
					}
					entryHeaderActions = result.ToArray();
				}

				return entryHeaderActions;
			}
		}
		EntryHeaderMessageSendingAction[] entryHeaderActions;

		#region Send Messages

		public bool SendMessagesWithoutSaving(Customs.Business.ISendsMessagesToCustoms sender)
		{
			bool result = false;

			switch (messageSendingMessageType)
			{
				case ImportMessageSendingMessageType.ExtendTIB:
				case ImportMessageSendingMessageType.ClosureTIB:
					result = GenerateExtendTIBRequestMessages();
					break;
				case ImportMessageSendingMessageType.EBondRequest:
					result = GenerateEBondRequest(sender);
					break;
				case ImportMessageSendingMessageType.StandAlonePriorNotice:
					result = GenerateStandAlonePriorNotice();
					break;
				default:
					result = GenerateOriginalAmendmentWithdrawalMessages(sender);
					break;
			}

			return result;
		}

		bool GenerateEBondRequest(Customs.Business.ISendsMessagesToCustoms sender)
		{
			var result = false;
			var messageCount = 0;

			foreach (var action in this.Cast<EBondMessageSendingAction>())
			{
				var hasMessageCreated = new EBondRequestMessageBuilder(declaration, (CusEntryHeader)action.bizObj).Generate();
				if (hasMessageCreated)
				{
					messageCount++;
					result = true;
				}
			}

			if (messageCount == 0)
			{
				sender.WarnUserAboutSomething(Res.GetString("af89687e-dfe7-46a4-b85c-7ab8d85a0989", "Cannot send eBond Request message. Please close the job, re-open and try again."), Res.GetString("dfb0f2c1-35c2-4525-a1fb-d81cddd2bfaa", "Cannot send"));
			}

			return result;
		}

		bool GenerateStandAlonePriorNotice()
		{
			bool result = false;

			foreach (ImportMessageSendingAction action in this)
			{
				var standAlonePriorNoticeMessageSendingAction = (StandAlonePriorNoticeMessageSendingAction)action;

				if (standAlonePriorNoticeMessageSendingAction != null && standAlonePriorNoticeMessageSendingAction.US_SendMessage)
				{
					var priotNoticeHeader = standAlonePriorNoticeMessageSendingAction.PriorNoticeHeader;
					if (priotNoticeHeader != null)
					{
						new ACEPriorNoticeMessageBuilder(priotNoticeHeader, action.US_PNActionCode).GenerateMessage();
						result = true;
					}
				}
			}

			return result;
		}

		bool GenerateExtendTIBRequestMessages()
		{
			bool messageGenerated = false;

			foreach (EntryHeaderMessageSendingAction action in EntryHeaderActions)
			{
				if (action.US_SendMessage)
				{
					new RequestToExtendTIBMessageBuilder(action.entry, messageSendingMessageType).GenerateMessages();
					messageGenerated = true;
				}
			}

			return messageGenerated;
		}

		bool GenerateOriginalAmendmentWithdrawalMessages(Customs.Business.ISendsMessagesToCustoms sender)
		{
			JobDeclarationImportMessageManager manager = new JobDeclarationImportMessageManager(declaration, this);

			bool result = false;

			if (messageSendingMessageType == ImportMessageSendingMessageType.Original)
			{
				result = (manager.SendOriginalMessages(sender).Count > 0);
			}
			else if (messageSendingMessageType == ImportMessageSendingMessageType.Replacement)
			{
				result = manager.AmendMessages(sender);
			}
			else if (messageSendingMessageType == ImportMessageSendingMessageType.Deletion)
			{
				result = manager.WithdrawMessages(sender);
			}

			declaration.RecalculateValidationModesOnDeclaration(true);

			if (result)
			{
				UpdateCargoReleaseCertificationStatusToPendingIfNecessary();
				UpdateEntrySubmittedDate();
				UpdateCustomsCommenced();

				if (messageSendingMessageType != ImportMessageSendingMessageType.Deletion)
				{
					declaration.PopulatePaymentDueDateIfNeeded();
				}
			}

			return result;
		}

		#endregion

		#region Implementation

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("The method is not supported.");
		}

		internal Customs.Business.SingleMessageManager[] GetSingleMessageManagersSelected()
		{
			List<Customs.Business.SingleMessageManager> result = new List<Customs.Business.SingleMessageManager>();

			foreach (ImportMessageSendingAction action in this)
			{
				if (action.US_SendMessage)
				{
					result.Add(action.MessageManager);
				}
			}

			return result.ToArray();
		}

		internal Customs.Business.SingleMessageManager[] GetAllSingleMessageManagers()
		{
			List<Customs.Business.SingleMessageManager> result = new List<Customs.Business.SingleMessageManager>();
			foreach (ImportMessageSendingAction action in this)
			{
				result.Add(action.MessageManager);
			}

			return result.ToArray();
		}

		void UpdateCargoReleaseCertificationStatusToPendingIfNecessary()
		{
			foreach (EntryHeaderMessageSendingAction action in EntryHeaderActions)
			{
				if (action.US_SendMessage && messageSendingMessageType != ImportMessageSendingMessageType.Deletion && !action.entry.HasCargoReleaseBeenCertified)
				{
					if (action.US_CertifyCargoRelease || action.IsBorderCargoRelease || action.IsACECargoRelease)
					{
						using (action.entry.SuspendMarkingAsNeedingValidation())
						{
							action.entry.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.CertificationSentAckPending;
						}
					}
				}
			}
		}

		void UpdateEntrySubmittedDate()
		{
			foreach (EntryHeaderMessageSendingAction action in EntryHeaderActions)
			{
				if (action.US_SendMessage && action.actions.IsOriginal)
				{
					action.entry.PopulateEntrySubmittedDateIfRequired();
				}
			}
		}

		void UpdateCustomsCommenced()
		{
			foreach (EntryHeaderMessageSendingAction action in EntryHeaderActions)
			{
				if (action.US_SendMessage && (action.actions.IsOriginal || (action.actions.IsAmendment && !action.entry.HasBeenLodgedAtCustoms)))
				{
					if (action.entry.IsBorderCargoRelease ||
						action.entry.IsCargoRelease ||
						action.entry.IsFormalEntry ||
						action.entry.IsACECargoRelease)
					{
						action.entry.Declaration.LogCustomsCommencedIfNeeded();
						break;
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected virtual void PopulateElements(Func<CusEntryHeader, bool> entryFilter)
		{
			if (messageSendingMessageType == ImportMessageSendingMessageType.ExtendTIB
				|| messageSendingMessageType == ImportMessageSendingMessageType.ClosureTIB
				|| messageSendingMessageType == ImportMessageSendingMessageType.EntrySummaryQuery)
			{
				foreach (var entry in declaration.ActiveEntryHeaders.OfType<CusEntryHeader>().Where(x => entryFilter == null || entryFilter(x)))
				{
					if (messageSendingMessageType == ImportMessageSendingMessageType.ExtendTIB || messageSendingMessageType == ImportMessageSendingMessageType.ClosureTIB)
					{
						if (entry.IsTemporaryImportationBond)
						{
							Add(new EntryHeaderMessageSendingAction(entry, ImportMessageStatusList.MessageType.TemporaryImportationBond, MessageAttacheeRecordTypeDescriptions.TemporaryImportationBond, this));
						}
					}
					else
					{
						if (entry.IsFormalEntry)
						{
							Add(new EntryHeaderMessageSendingAction(entry, ImportMessageStatusList.MessageType.EntrySummary, MessageAttacheeRecordTypeDescriptions.Entry, this));
						}
					}
				}
			}
			else if (messageSendingMessageType == ImportMessageSendingMessageType.StandAlonePriorNotice)
			{
				if (declaration.IsACEENTStandAlonePriorNotice || declaration.IsACEBLNStandAlonePriorNotice || declaration.IsFTZFTZStandAlonePriorNotice || declaration.IsFTZBLNStandAlonePriorNotice)
				{
					var priorNoticeWrapper = new ACEStandAlonePriorNoticeWrapper(declaration);
					foreach (var priorNoticeHeader in priorNoticeWrapper.PriorNoticeHeaders)
					{
						Add(new StandAlonePriorNoticeMessageSendingAction(priorNoticeHeader, declaration, this));
					}
				}
				else
				{
					Add(new StandAlonePriorNoticeMessageSendingAction(declaration, this));
				}
			}
			else if (messageSendingMessageType == ImportMessageSendingMessageType.EBondRequest)
			{
				foreach (var entry in declaration.ActiveEntryHeaders.OfType<CusEntryHeader>().Where(x => entryFilter != null && entryFilter(x)))
				{
					Add(new EBondMessageSendingAction(entry, this));
				}
			}
			else
			{
				foreach (var entry in declaration.ActiveEntryHeaders.OfType<CusEntryHeader>().Where(x => entryFilter == null || entryFilter(x)))
				{
					bool isInTheStatusToGenerateMessageFor = false;

					switch (messageSendingMessageType)
					{
						case ImportMessageSendingMessageType.Original:
							isInTheStatusToGenerateMessageFor = entry.CanSendOriginal;
							break;
						case ImportMessageSendingMessageType.Replacement:
						case ImportMessageSendingMessageType.Deletion:
							isInTheStatusToGenerateMessageFor = entry.CanSendWithdrawal;
							break;
					}

					if (isInTheStatusToGenerateMessageFor)
					{
						if (entry.IsFormalEntry)
						{
							Add(new EntryHeaderMessageSendingAction(entry, ImportMessageStatusList.MessageType.EntrySummary, this));
						}
						else if (entry.IsInBond)
						{
							Add(new EntryHeaderMessageSendingAction(entry, ImportMessageStatusList.MessageType.InBondDeparture, this));
						}
						else if (entry.IsBorderCargoRelease)
						{
							Add(new EntryHeaderMessageSendingAction(entry, ImportMessageStatusList.MessageType.BorderCargoRelease, this));
						}
						else if (entry.IsCargoRelease)
						{
							Add(new EntryHeaderMessageSendingAction(entry, ImportMessageStatusList.MessageType.CargoRelease, this));
						}
						else if (entry.IsACECargoRelease)
						{
							Add(new EntryHeaderMessageSendingAction(entry, ImportMessageStatusList.MessageType.ACECargoRelease, this));
						}
						else if (!entry.IsCargoRelease)//cargo release is linked up inside formal entry
						{
							ErrorReporter.ReportOnce("invalid entry.CH_MessageType" + " " + entry.CH_MessageType);
						}
					}
				}
			}
		}

		#endregion

		public
#if DEBUG
 virtual
#endif
 ZBool IsCancelled
		{
			get { return fIsCancelled; }
			set
			{
				fIsCancelled = value;
			}
		}
		ZBool fIsCancelled;
	}
}
