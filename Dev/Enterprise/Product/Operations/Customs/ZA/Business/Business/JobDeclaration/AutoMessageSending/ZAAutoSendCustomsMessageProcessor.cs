using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class ZAAutoSendCustomsMessageProcessor : AutoSendCustomsMessageProcessor
	{
		public ZAAutoSendCustomsMessageProcessor(JobDeclaration declaration) : base(declaration)
		{
			wrapper = new JobDeclarationMessageSendingObjectParent(Declaration);
			entryHeadersToSend = new List<Customs.Business.CusEntryHeader>();
		}

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;
		readonly JobDeclarationMessageSendingObjectParent wrapper;
		List<Customs.Business.CusEntryHeader> entryHeadersToSend;

		protected override IEnumerable<Customs.Business.CusEntryHeader> GetEntryHeadersToSendCore()
		{
			var notifier = new SendsMessagesToCustomsShutterUpperer(false);
			if (Declaration.DoMerge(notifier))
			{
				entryHeadersToSend = Declaration.ActiveEntryHeaders.Cast<Customs.Business.CusEntryHeader>().ToList();
			}
			return entryHeadersToSend;
		}

		protected override ZBool CanSendEntryHeader(Customs.Business.CusEntryHeader entryHeader)
		{
			bool canSend = false;

			if (base.CanSendEntryHeader(entryHeader))
			{
				var entryToSend = entryHeadersToSend.FirstOrDefault(x => x == entryHeader);
				if ((entryToSend != null) && (entryToSend.Messages.Count == 0))
				{
					canSend = true;
				}
			}
			return canSend;
		}

		protected override ZBool SendCustomsMessageCore(INotifications notifications, Customs.Business.CusEntryHeader entryHeader)
		{
			try
			{
				Action<MessageSendingObject> prepareAction = x =>
				{
					x.ShouldSend = x.Header == entryHeader;
					if (x.ShouldSend)
					{
						x.MessageType = MessageSubTypeCodes.Codes.Original;
						x.SubmissionDate = ZDateTime.UtcNow.AddMinutes(10);
					}
				};

				wrapper.SendingObjectsCollection.Cast<MessageSendingObject>().ToList().ForEach(prepareAction);

				var notifier = new NotificationAccumulatorForAutomatedMessageSending();
				var messageManager = new MessageManagers.MessageManager(wrapper, notifier)
				{
					IsAutoSendCustomsMessageProcessor = true
				};
				messageManager.SendMessages();

				return true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				LogSystemError(notifications, ex.Message);
				return false;
			}
		}

		protected override ZString MessageDescription => "South African Customs Declaration";
	}
}
