using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class N5101HMultiMessageManager : MultiMessageManager
	{
		public N5101HMultiMessageManager(MessageSendingObjectParent n5101HWrapper) : base()
		{
			N5101HWrapper = Argument.NotNull(n5101HWrapper, "n5101HWrapper");
		}

		protected readonly MessageSendingObjectParent N5101HWrapper;

		protected AsycudaManifestHeader ManifestHeader => (AsycudaManifestHeader)N5101HWrapper.TopLevelBusinessObject;

		public override IMessageManageableBizObj TopLevelBizObjToManage => ManifestHeader;

		protected override bool SendWheneverPossibleOnceMessagingActive => true;

		protected override SingleMessageManager[] GetAllMessageManagers()
		{
			var result = new List<SingleMessageManager>();
			var groupedByAction = N5101HWrapper.SendingObjectsCollection.Cast<MessageSendingObject>().Where(x => x.ShouldSend).GroupBy(x => x.Action).ToDictionary(x => x.Key, y => y.ToArray());

			foreach (var pair in groupedByAction)
			{
				var collection = new MessageSendingObjectCollection(Factory);
				collection.AddRange(pair.Value);
				var singleManager = new N5101HMessageManager(new N5101HMessageSendingObject(ManifestHeader, collection, N5101HWrapper));
				result.Add(singleManager);
			}
			return result.ToArray();
		}

		public bool SendMessages(ISendsMessagesToCustoms sender)
		{
			var result = SendMessagesWithoutSaving(sender);
			if (result)
			{
				LogManifestEvent();
				ManifestHeader.Bills.Cast<AsycudaBill>().ForEach(c => c.ABL_MessageStatus = TWMessageStatusCodeList.Codes.AwaitingResponse);
				N5101HWrapper.Factory.Save();
			}
			return result;
		}

		bool SendMessagesWithoutSaving(ISendsMessagesToCustoms sender)
		{
			Initialise();
			var result = false;
			var singleMessageManagers = AllMessageManagers;
			if (singleMessageManagers.Length > 0)
			{
				result = SendOriginal(sender, singleMessageManagers).Any();
			}
			return result;
		}

		void LogManifestEvent()
		{
			var menuCaption = N5101HWrapper.MenuCaption;
			if (!menuCaption.IsEmpty)
			{
				var sendingObjects = N5101HWrapper.SendingObjectsCollection.Cast<MessageSendingObject>().Where(x => x.ShouldSend);
				foreach (var sendingObject in sendingObjects)
				{
					var bill = sendingObject.Bill;
					var action = sendingObject.Action;
					var logs = bill.Logs;
					var messageRef = $"{menuCaption}, SendingObjectsCollectionAction='{action}'";
					logs.AddNew(Events.MessageSent, messageRef);
					switch (action)
					{
						case ActionCodeList.Codes.New:
							logs.AddNew(Events.CustomsCommenced, messageRef);
							break;
						case ActionCodeList.Codes.Delete:
							logs.AddNew(Events.DeclarationCancellationSent, messageRef);
							break;
						case ActionCodeList.Codes.Replace:
							logs.AddNew(Events.DeclarationAmendmentSent, messageRef);
							break;
					}
				}
			}
		}
	}
}
