using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TW.Business.MessageManagers
{
	public class LicensingMultiMessageManager : MultiMessageManager
	{
		#region Constructor

		public LicensingMultiMessageManager(LicensingMessageSendingObjectParent licensingWrapper, ZString messageType) : base()
		{
			this.licensingWrapper = Argument.NotNull(licensingWrapper, nameof(licensingWrapper));
			this.messageType = messageType;
		}

		readonly ZString messageType;

		readonly LicensingMessageSendingObjectParent licensingWrapper;

		protected override bool SendWheneverPossibleOnceMessagingActive => true;

		public override IMessageManageableBizObj TopLevelBizObjToManage => licensingWrapper.Declaration;

		#endregion

		protected override SingleMessageManager[] GetAllMessageManagers()
		{
			SingleMessageManager[] result = null;
			switch (messageType)
			{
				case ControllingMessageTypeList.Codes.NX101:
				case ControllingMessageTypeList.Codes.NX201_01:
				case ControllingMessageTypeList.Codes.NX201_07:
				case ControllingMessageTypeList.Codes.NX301:
				case ControllingMessageTypeList.Codes.NX301_AX:
				case ControllingMessageTypeList.Codes.NX301_DN:
				case ControllingMessageTypeList.Codes.NX401:
				case ControllingMessageTypeList.Codes.NX601:
				case ControllingMessageTypeList.Codes.NX603:
					result = licensingWrapper.SendingObjectsCollection.Where(x => x.ShouldSend).OfType<LicensingMessageSendingObject>().Select(x => new LicensingMessageManager(x)).ToArray();
					break;
				default:
					result = Array.Empty<SingleMessageManager>();
					break;
			}
			return result;
		}

		public bool SendMessages(ISendsMessagesToCustoms sender)
		{
			var messages = SendMessagesWithoutSaving(sender);
			if (messages.Any())
			{
				try
				{
					LogDeclarationEvent();
					try
					{
						licensingWrapper.Factory.Save();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						messages.ForEach(f => f.Delete());
						throw;
					}
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
			return true;
		}

		internal IList<EDIMessage> SendMessagesWithoutSaving(ISendsMessagesToCustoms sender)
		{
			Initialise();
			var singleMessageManagers = AllMessageManagers;
			return singleMessageManagers.Any() ? SendOriginal(sender, singleMessageManagers) : Array.Empty<EDIMessage>();
		}

		protected void LogDeclarationEvent()
		{
			var menuCaption = licensingWrapper.MenuCaption;
			if (!menuCaption.IsEmpty)
			{
				var logs = licensingWrapper.Declaration.Logs;
				foreach (LicensingMessageSendingObject sendingObject in licensingWrapper.SelectedSendingObjects)
				{
					var action = sendingObject.Action;
					var eventRef = $"{menuCaption}, SendingObjectsCollectionAction='{action}'";
					logs.AddNew(Events.MessageSent, eventRef);
					switch (action)
					{
						case ActionCodeList.Codes.Update:
						case NX101ActionCodeList.Codes._17:
						case NX101ActionCodeList.Codes._18:
							logs.AddNew(Events.DeclarationAmendmentSent, eventRef);
							break;
					}
				}
			}
		}
	}
}
