using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US.DIS;
using Enterprise.MasterFiles.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DIS.Business
{
	public class MessageSendingAction : AutoMessageSendingAction, IDISSubmissionAdditionalData, IMessageSendingActionBase
	{
		public MessageSendingAction(DISDocument disDocument)
			: base(disDocument.Factory)
		{
			this.disDocument = disDocument;
		}

		readonly DISDocument disDocument;

		IDISDocumentBase IMessageSendingActionBase.DisDocument => disDocument;

		ZBool IMessageSendingActionBase.HasAnyDocumentsToSend => Send || SendWithdrawal;

		public ZString DocumentDescription
		{
			get { return disDocument.DocumentLabelUSDISDocCode + (disDocument.EDoc is IeDoc eDoc ? "/" + eDoc.FileName : ""); }
		}

		public ZString Status
		{
			get { return disDocument.Status; }
		}

		public ZString StatusDescription
		{
			get { return disDocument.StatusDescription; }
		}

		public override ZBool Send
		{
			get { return base.Send; }
			set
			{
				base.Send = value;

				if (Send)
				{
					SendWithdrawal = false;
				}
			}
		}

		[ReadOnlyMember(nameof(CannotSendWithdrawal))]
		public override ZBool SendWithdrawal
		{
			get { return base.SendWithdrawal; }
			set
			{
				base.SendWithdrawal = value;

				if (SendWithdrawal)
				{
					Send = false;
				}
				else
				{
					WithdrawalReason = ZString.Empty;
					WithdrawalComment = ZString.Empty;
				}
			}
		}

		bool CannotSendWithdrawal
		{
			get { return !HasBeenLodgedAtCustoms; }
		}

		bool HasBeenLodgedAtCustoms
		{
			get { return StatusList.HasBeenLodgedAtCustoms(disDocument.Status); }
		}

		[ReadOnlyMember(nameof(IsNonWithdrawal))]
		[List(nameof(ReasonCodes))]
		public override ZString WithdrawalReason
		{
			get { return base.WithdrawalReason; }
			set { base.WithdrawalReason = value; }
		}

		public CodeDescriptionPairList ReasonCodes
		{
			get { return Factory.GetCachedValue<DocumentWithdrawalReasonList>(); }
		}

		[ReadOnlyMember(nameof(IsNonWithdrawal))]
		public override ZString WithdrawalComment
		{
			get { return base.WithdrawalComment; }
			set { base.WithdrawalComment = value; }
		}

		bool IsNonWithdrawal
		{
			get { return !SendWithdrawal; }
		}

		public ZString MessageType
		{
			get
			{
				if (SendWithdrawal)
				{
					return "Withdrawal";
				}
				else if (HasBeenLodgedAtCustoms)
				{
					return "Replace";
				}
				else
				{
					return "Add";
				}
			}
		}

		internal bool IsWaitingForResponse
		{
			get { return StatusList.IsWaitingForResponse(disDocument.Status); }
		}

		ZString IDISSubmissionAdditionalData.WithdrawalReasonCode
		{
			get { return WithdrawalReason; }
		}

		ZString IDISSubmissionAdditionalData.Comment
		{
			get { return WithdrawalComment; }
		}

		public ZString MessageSendingWarning
		{
			get { return disDocument.MessageSendingWarning; }
		}

		public ZString MessageSendingError
		{
			get { return disDocument.MessageSendingError; }
		}
	}
}
