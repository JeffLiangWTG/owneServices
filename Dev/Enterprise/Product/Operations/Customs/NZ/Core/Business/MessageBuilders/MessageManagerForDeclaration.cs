using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.MessageBuilders
{
	public abstract class MessageManagerForDeclaration : MessageManagerForClearance
	{
		public MessageManagerForDeclaration(JobDeclaration declaration, OperationType operationType)
			: base(operationType)
		{
			if (declaration == null)
			{
				throw new ArgumentNullException(nameof(declaration), "Declaration");
			}
			EntryHeader = declaration.CusEntryHeader;
			Declaration = declaration;
		}

		public MessageManagerForDeclaration(JobDeclaration declaration, OperationType operationType, IAdditionalInformation additionalInformation)
			: base(operationType)
		{
			if (declaration == null)
			{
				throw new ArgumentNullException(nameof(declaration), "Declaration");
			}
			EntryHeader = declaration.CusEntryHeader;
			Declaration = declaration;
			AdditionalInformation = additionalInformation;
		}

		public readonly CusEntryHeader EntryHeader;
		protected readonly JobDeclaration Declaration;
		protected readonly IAdditionalInformation AdditionalInformation;

		public override bool IsOKToSendWithMessagingErrors()
		{
			if (ConsolidatedDeclaration.GetConsolidatedDeclaration(Declaration) is ConsolidatedDeclaration consolidatedDeclaration)
			{
				return Customs.Business.MessageSendingValidation.New(consolidatedDeclaration, null).CheckBusinessObjectLevelValidation(Declaration.MessageInitiator);
			}
			else
			{
				return Customs.Business.MessageSendingValidation.New(Declaration, null).CheckBusinessObjectLevelValidation(Declaration.MessageInitiator);
			}
		}

		protected override bool ValidateMessagingPreconditions()
		{
			bool result = base.ValidateMessagingPreconditions();
			if (result)
			{
				if (Declaration.IsMiscellaneous)
				{
					result = false;
					fLastHumanReadableStatus = PreconditionCannotSendMiscDeclaration;
				}
				else if (EntryHeader.CH_IsEntryCancelled)
				{
					result = false;
					fLastHumanReadableStatus = PreconditionCannotSendWhenEntryHasBeenCancelled;
				}
			}
			return result;
		}
		public const string PreconditionCannotSendMiscDeclaration = "You cannot send a Miscellaneous Declaration to Customs. Miscellaneous Declarations are used for tracking and billing purposes only.";
		public const string PreconditionCannotSendWhenEntryHasBeenCancelled = "You cannot send a message to Customs for an entry that has been cancelled.";

		public bool MessageIsCurrentlyQueued
		{
			get { return GetMessageIsCurrentlyQueued(); }
		}

		protected virtual bool GetMessageIsCurrentlyQueued()
		{
			return false;
		}

		public string MessageIsCurrentlyQueuedShouldWeCancelQuestion
		{
			get { return GetMessageIsCurrentlyQueuedShouldWeCancelQuestion(); }
		}

		public string ExportUpdateEDITransmitDate
		{
			get
			{
				return @"You cannot Submit to Customs if your EDI Transmit Date is in the past. 

Do you accept your EDI Transmit Date being updated to today?";
			}
		}

		protected virtual string GetMessageIsCurrentlyQueuedShouldWeCancelQuestion()
		{
			return "";
		}

		public void CancelCurrentlyQueuedMessage()
		{
			CancelCurrentlyQueuedMessageInternal();
		}

		protected virtual void CancelCurrentlyQueuedMessageInternal()
		{
		}

		protected override ZString GetEnteredRemarks()
		{
			return EntryHeader.CH_CustomsMessageRemarks;
		}

		protected override void SetEnteredRemarks(ZString value)
		{
			EntryHeader.CH_CustomsMessageRemarks = value;
		}

		protected bool SendMessage(EDIMessage message)
		{
			var factory = Declaration.Factory;
			bool result = false;
			try
			{
				factory.Save();
				result = true;
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				if (message != null)
				{
					if (message.IsInDatabase)
					{
						ErrorReporter.ReportOnce("Exception during Save, but transaction committed", e); // Transaction is saved but exception happened. Leave this for further investigation.
					}
					else
					{
						message.Delete();
					}                  
				}
				ZExceptionReporting.HandleSaveException(e);
			}
			return result;
		}
	}
}
