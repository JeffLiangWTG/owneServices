using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture.Data.Mutex;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public class MultiEManifestMessageManager
	{
		public MultiEManifestMessageManager(eManifestMessageSendingObjectParent wrapper, IMessageNotificationCollector notification)
		{
			this.wrapper = Argument.NotNull(wrapper, "wrapper");
			this.notification = Argument.NotNull(notification, "notification");
		}
		readonly eManifestMessageSendingObjectParent wrapper;
		readonly IMessageNotificationCollector notification;

		public void SendMessages(bool shouldSkipNotification = false)
		{
			sendableObjects = null;
			firstMessage = null;
			var trip = wrapper.Trip;
			var tripMutex = GetTripMutexToSendMessage(trip);
			try
			{
				if (tripMutex.Lock())
				{
					if (CanSendMessages(shouldSkipNotification))
					{
						var sendWithMessageErrors = notification.Notifications?.ContainsWarning() ?? ZBool.False;

						notification.Notifications?.Clear();
						for (var i = 0; i < SendableObjects.Length; i++)
						{
							var sendingObject = SendableObjects[i];
							trip.ResetShipmentsActionsIfMessageTypeChanged(sendingObject.MessageType);
							SendMessage(sendingObject, sendWithMessageErrors, i > 0);
						}

						wrapper.TopLevelBusinessObject.Factory.Save();
						if (!shouldSkipNotification)
						{
							notification.ShowInformation(notification.Notifications.NotificationsAsString(), Res.GetString("D192681C-FF73-4911-878E-25E4C1C73F48", "Sending Result"));
						}
					}
				}
				else
				{
					var mutexLockedByInfo = tripMutex.GetMutexLockByInfo();
					notification.ShowError(Res.GetString("9F7BD61C-064E-4CC7-9212-836713DF4719", "{0} is sending messages via Submit eManifest (No ACE ID) for this Trip, please try again later.", mutexLockedByInfo), Res.GetString("199B2B70-6674-49FD-9A2B-11DD890411B2", "Error"));
				}
			}
			catch (ZSaveException e)
			{
				ZExceptionReporting.HandleSaveException(e);
			}
			finally
			{
				if (tripMutex != null && tripMutex.HasLock)
				{
					tripMutex.Unlock();
				}
			}
		}
		Enterprise.Messaging.Business.EDIMessage firstMessage;

		void SendMessage(eManifestMessageSendingObject sendingObjects, ZBool sendWithMessageErrors, bool lazyGenerateMessageContent)
		{
			var builder = eManifestMessageManagerHelper.GetMessageBuilder(sendingObjects, sendingObjects.MessageType, sendingObjects.ActionCode, lazyGenerateMessageContent);
			var builderResult = builder.PopulateMessages().GetBuilderResults().FirstOrDefault();
			if (builderResult != null)
			{
				var message = builderResult.Message;
				message.EM_SendWithMessageErrors = sendWithMessageErrors;
				var iEDIFACTMessageAttachee = ((IEDIFACTMessageAttachee)sendingObjects);
				var shouldSetToPending = firstMessage != null;
				if (shouldSetToPending)
				{
					message.EM_Status = EDIMessage.Status.Pending;
					message.Saving -= SavingMessage;
					message.Saving += SavingMessage;
					message.Saved -= SavedPendingMessage;
					message.Saved += SavedPendingMessage;
				}
				else
				{
					firstMessage = message;
					if (SendableObjects.Length > 1)
					{
						message.Saving -= SavingMessage;
						message.Saving += SavingMessage;
						message.Saved -= SavedFirstMessage;
						message.Saved += SavedFirstMessage;
					}
					iEDIFACTMessageAttachee.MessageStatus = sendingObjects.StatusCalculator.GetMessageAwaitingStatus(message);
				}
				iEDIFACTMessageAttachee.AddMessage(message);
				notification.Notifications.AddInformation(Res.GetString("AA58B6FC-4991-4D59-9352-DD564D233CB8", "1 {0} message {1}.", sendingObjects.MessageDescription, shouldSetToPending ? "set to pending on acceptance of previous message" : "queued for sending"));
			}
		}

		void SavedPendingMessage(Enterprise.Messaging.Business.EDIMessage message, bool saveSucceeded)
		{
			if (saveSucceeded)
			{
				message.Saving -= SavingMessage;
				message.Saved -= SavedPendingMessage;
			}
		}

		void SavedFirstMessage(Enterprise.Messaging.Business.EDIMessage message, bool saveSucceeded)
		{
			if (saveSucceeded)
			{
				message.Saving -= SavingMessage;
				message.Saved -= SavedFirstMessage;
			}
		}

		void SavingMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
			if (firstMessage != null)
			{
				message.EM_ApplicationReference = firstMessage.EM_MessageNum;
			}
		}

		bool CanSendMessages(bool shouldSkipNotification)
		{
			var result = true;
			if (!shouldSkipNotification)
			{
				var notifications = notification?.Notifications;
				if (notifications != null)
				{
					CollectNotificationsFromSendingObjects(notifications);

					if (notifications.ContainsError())
					{
						notification.ShowError(notifications.ErrorNotificationsAsString(), Res.GetString("74764D0A-9D86-4512-9EAF-26912B71C582", "Cannot send this message"));
						result = false;
					}
					else if (notifications.ContainsWarning())
					{
						result = notification.ShowConfirmation(notifications.WarningNotificationsAsString(), Res.GetString("E9C29B86-D68E-400F-91E7-2784B5014020", "Continue sending with warning?"));
					}
				}
			}
			return result;
		}

		void CollectNotificationsFromSendingObjects(MessageSendingNotificationCollection notifications)
		{
			if (SendableObjects.Length > 0)
			{
				if (!wrapper.AllShipmentsHaveSameReleaseStatus)
				{
					notifications.AddError(Res.GetString("802BCF51-CF3E-4B09-8CE3-770B6B9449FE", "Submit eManifest (No ACE ID) only available for jobs with all shipments have same release status."));
				}
				else
				{
					var trip = wrapper.Trip;
					trip.LoadChildEditableObjects();
					using (((IBusinessObjectInternals)trip).ResumeValidationForAllDescendantsTemporarily())
					{
						trip.RunPreSaveValidation();
					}

					if (trip.HasErrors)
					{
						var errorCollector = new CustomsNotificationCollector(trip, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors();
						notifications.AddError(errorCollector.ToUniqueMessageListString() + System.Environment.NewLine);
					}

					if (trip.HasMessageErrors)
					{
						var messageErrorCollector = new CustomsNotificationCollector(trip, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors();
						notifications.AddWarning(messageErrorCollector.ToUniqueMessageListString());
					}
				}

				foreach (var obj in SendableObjects)
				{
					ZString messageText;
					var canSendResult = eManifestMessageManagerHelper.CanSendThisMessage(obj, obj.MessageType, obj.ActionCode, true);
					if (!canSendResult.result)
					{
						messageText = canSendResult.messageText;
						if (!string.IsNullOrEmpty(messageText))
						{
							var friendlyName = GetMessageFriendlyNameForNotSentPopup(obj);
							messageText = Res.GetString("49724532-28C0-4038-BA35-E2E00C35913D", "System cannot send {0}{1} message as {2}", Grammar.Instance.IndefiniteArticlePrefix(friendlyName), friendlyName, messageText);
						}

						notifications.AddError(messageText);
					}
				}
			}
			else
			{
				notifications.AddError(Res.GetString("{7D446BF9-F342-4056-9275-945BBFE2F985}", "At least one need to be flagged for sending."));
			}
		}

		string GetMessageFriendlyNameForNotSentPopup(eManifestMessageSendingObject obj)
		{
			var result = obj.StatusCalculator.MessageTypeDescription;
			if (obj.ActionCode != MessageSubTypes.Undefined)
			{
				result = string.Format(CultureInfo.CurrentCulture, "{0} {1}", result, eManifestMessageManagerHelper.GetActionCodeDescription(obj.ActionCode));
			}
			return result;
		}

		eManifestMessageSendingObject[] SendableObjects
		{
			get
			{
				return sendableObjects ?? (sendableObjects = wrapper.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().Where(x => x.ShouldSend).OrderBy(x => x.Order).ToArray());
			}
		}
		eManifestMessageSendingObject[] sendableObjects;

		ZGlobalMutex GetTripMutexToSendMessage(Trip trip)
		{
			return tripMutexToSendMessage ?? (tripMutexToSendMessage = new ZGlobalMutex(Enterprise.ZArchitecture.Modules.MutexIDs.SendCustomsMessage, trip.PK.ToString()));
		}
		ZGlobalMutex tripMutexToSendMessage;
	}
}
