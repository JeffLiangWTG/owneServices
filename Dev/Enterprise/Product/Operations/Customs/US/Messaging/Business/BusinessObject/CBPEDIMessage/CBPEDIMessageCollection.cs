using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Messaging.Business
{
	public abstract class CBPEDIMessageCollection : Enterprise.Messaging.Business.EDIMessageCollection
	{
		protected CBPEDIMessageCollection(BusinessObject master, ZQuery cBPTypeFilter)
			: base(master)
		{
			this.cBPTypeFilter = Argument.NotNull(cBPTypeFilter, "CBPTypeFilter");
		}

		public new CBPEDIMessage this[int index]
		{
			get { return (CBPEDIMessage)base[index]; }
		}

		public new CBPEDIMessage AddNew()
		{
			return (CBPEDIMessage)base.AddNew();
		}

		public new CBPEDIMessage AddNew(Type bizOType)
		{
			return (CBPEDIMessage)base.AddNew(bizOType);
		}

		#region New Methods

		public List<MessageBlock> GetMatchedErrorBlocks(ZString applicationCode, ZString messageType)
		{
			List<MessageBlock> errorBlocks = new List<MessageBlock>();

			Enterprise.Messaging.Business.EDIMessage[] receivedMessages = GetMatchingMessages(applicationCode, new ZString[] { messageType }, CBPEDIMessage.Direction.Receive);
			foreach (CBPEDIMessage message in receivedMessages)
			{
				errorBlocks.AddRange(message.MessageBlock.MessageBlocks.FindAll((MessageBlock block) => block is IStatusesAndErrors));
			}

			return errorBlocks;
		}

		public T GetLastMessageWithSpecificMessageBlock<T>(ZString applicationCode, ZString messageType, ZString tRXorRCV, Type type)
			where T : CBPEDIMessage
		{
			T lastMessage = null;

			foreach (T message in this)
			{
				if (message.EM_ApplicationCode == applicationCode
					&& message.EM_MessageType == messageType
					&& message.EM_ReceiveTransmit == tRXorRCV)
				{
					MessageBlock block = message.MessageBlock.MessageBlocks.Find(x => x.GetType() == type);

					if (block != null)
					{
						if (lastMessage == null)
						{
							lastMessage = message;
						}
						else
						{
							if (!message.IsInDatabase || (message.EM_SystemCreateTimeUtc > lastMessage.EM_SystemCreateTimeUtc))
							{
								lastMessage = message;
							}
						}
					}
				}
			}

			return lastMessage;
		}

		#endregion

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var message = (CBPEDIMessage)child;
			if (message.EM_SendWithMessageErrors && !message.IsMessageSendWithMessageErrorsCorrect())
			{
				message.EM_SendWithMessageErrors = false;
			}
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(cBPTypeFilter.ShallowClone());
			return result;
		}

		readonly ZQuery cBPTypeFilter;
	}
}
