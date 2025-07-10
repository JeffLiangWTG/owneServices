using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business
{
	public abstract class MessageBuilder<T, TEDIMessage> : IMessageBuilder<TEDIMessage>
		where T : BlockControlGenerator
		where TEDIMessage : CBPEDIMessage
	{
		protected MessageBuilder(IMessageAttachee messageAttachee)
		{
			if (messageAttachee == null)
			{
				throw new ArgumentNullException("messageAttachee for " + this.GetType().Name);
			}

			this.messageAttachee = messageAttachee;
		}

		protected MessageBuilder(IMessageAttachee messageAttachee, UpdateActionCode action)
			: this(messageAttachee)
		{
			if (!IsActionSupported(action))
			{
				throw new NotSupportedException("The Update Action '" + action.ToString() + "' is not supported by the message builder " + GetType().FullName);
			}
			this.action = action;
		}

		public TEDIMessage PopulateMessage()
		{
			T block = GetNewInputBlockControlGenerator();
			block.B.ApplicationIdentifier = ApplicationIdentifier;
			UpdateMessageBlocks(block);

			TEDIMessage message = block.CreateMessage<TEDIMessage>(messageAttachee.Factory);
			if (ShouldSetMessageBranchToAttacheeBranch)
			{
				var branch = messageAttachee.Branch;
				if (branch != null)
				{
					message.EM_GB = branch.PK;
				}
			}
			messageAttachee.Messages.Add(message);
			SetMessageSubType(message);
			return message;
		}

		protected abstract T GetNewInputBlockControlGenerator();

		protected virtual bool ShouldSetMessageBranchToAttacheeBranch
		{
			get { return false; }
		}

		protected ZString GetActionCode()
		{
			return UpdateActionCodeConverter.ConvertToString(action);
		}

		bool IsActionSupported(UpdateActionCode action)
		{
			return SupportedUpdateActionCodeList.Contains(action);
		}

		List<UpdateActionCode> SupportedUpdateActionCodeList
		{
			get { return supportedUpdateActionCodeList ?? (supportedUpdateActionCodeList = GetSupportedUpdateActionCodeList()); }
		}
		List<UpdateActionCode> supportedUpdateActionCodeList;

		protected virtual List<UpdateActionCode> GetSupportedUpdateActionCodeList()
		{
			return new List<UpdateActionCode>();
		}

		protected abstract string ApplicationIdentifier { get; }
		protected abstract void UpdateMessageBlocks(T block);
		protected abstract void SetMessageSubType(TEDIMessage message);

		protected readonly IMessageAttachee messageAttachee;
		protected readonly UpdateActionCode action;
	}
}
