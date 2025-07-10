using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	abstract class CargoImpPhase2MessageDelivery : IProcessor
	{
		public CargoImpPhase2MessageDelivery(ProcessTaskNotification action, IQueuedLog queuedLog)
		{
			this.action = action;
			this.queuedLog = queuedLog;
		}

		readonly ProcessTaskNotification action;
		readonly IQueuedLog queuedLog;

		#region IProcessor Members

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			ZString messageType;
			ZString cargoIMPEvent;
			if (this.action.Parent.TriggerEventCode == Events.Detached.Code)
			{
				cargoIMPEvent = ZString.Empty;
				messageType = CargoIMPPhase2MessageManager.RouteMapCancellationType;
			}
			else
			{
				cargoIMPEvent = GetCargoIMPEvent();
				messageType = cargoIMPEvent.IsEmpty ? CargoIMPPhase2MessageManager.RouteMapInformationType : CargoIMPPhase2MessageManager.MilestoneStatusUpdateType;
			}

			ProcessCore(messageType, cargoIMPEvent, notifications);
		}

		#endregion

		protected abstract void ProcessCore(ZString messageType, ZString cargoIMPEvent, INotifications notifications);

		protected void CreateMessage(ForwardingShipment shipment, ZString messageType, ZString cargoIMPEvent, INotifications notifications)
		{
			CargoIMPPhase2MessageManager manager = CargoIMPPhase2MessageManager.New(shipment);
			NotificationBuffer messageCreationNotificationBuffer = new NotificationBufferWithWarnings(notifications, shipment, messageType);
			NotificationBuffer completeNotificationBuffer = new NotificationBuffer();
			switch (messageType)
			{
				case CargoIMPPhase2MessageManager.MilestoneStatusUpdateType:
					CreateMilestoneStatusUpdateMessage(manager, cargoIMPEvent, messageCreationNotificationBuffer, completeNotificationBuffer);
					break;

				case CargoIMPPhase2MessageManager.RouteMapCancellationType:
					CreateRouteMapCancellationMessage(manager, messageCreationNotificationBuffer);
					break;

				default:
					CreateRouteMapInformationMessage(manager, messageCreationNotificationBuffer, completeNotificationBuffer);
					break;
			}

			AttachCreationLogToTheShipment(manager, messageCreationNotificationBuffer, completeNotificationBuffer);
		}

		static void AttachCreationLogToTheShipment(CargoIMPPhase2MessageManager manager, NotificationBuffer messageCreationNotificationBuffer, NotificationBuffer completeNotificationBuffer)
		{
			if (messageCreationNotificationBuffer.Events.Length > 0)
			{
				completeNotificationBuffer.AddRange(messageCreationNotificationBuffer.Events);
			}

			CargoIMPPhase2MessageNote note = new CargoIMPPhase2MessageNote(manager.Shipment);
			if (completeNotificationBuffer.AsString != note.LoadFromNote())
			{
				note.WriteToNote(completeNotificationBuffer.AsString);
			}
		}

		void CreateMilestoneStatusUpdateMessage(CargoIMPPhase2MessageManager manager, ZString cargoIMPEvent, NotificationBuffer messageCreationNotificationBuffer, NotificationBuffer canCreateNotificationBuffer)
		{
			bool createMessage = true;
			if (manager.MustRouteMapInformationBeSentBeforeMilestoneStatusUpdate())
			{
				messageCreationNotificationBuffer.Notify(new InfoNotification(Res.GetString("71b01f81-6ceb-4155-9aac-a67df16c5d4c", "CargoIMP Phase 2 Route Map Information message must be sent before first Status Update message.")));
				createMessage = CreateRouteMapInformationMessage(manager, messageCreationNotificationBuffer, canCreateNotificationBuffer);
			}

			if (createMessage && manager.IsMilestoneStatusUpdateCanBeCreated(canCreateNotificationBuffer))
			{
				if (!manager.CreateMilestoneStatusUpdateMessage(cargoIMPEvent, ((IWorkflowTrigger)action.Parent).LastFiredTime.ToZDateTime(), messageCreationNotificationBuffer))
				{
					SendErrorEmail(messageCreationNotificationBuffer, CargoIMPPhase2MessageManager.MilestoneStatusUpdateType, manager);
				}
			}
		}

		void CreateRouteMapCancellationMessage(CargoIMPPhase2MessageManager manager, NotificationBuffer messageCreationNotificationBuffer)
		{
			if (manager.IsRouteMapCancellationMessageCanBeCreated(null) &&
					!manager.IsShipmentAttachedToCorrectConsol())
			{
				if (!manager.CreateRouteMapCancellationMessage(messageCreationNotificationBuffer))
				{
					SendErrorEmail(messageCreationNotificationBuffer, CargoIMPPhase2MessageManager.RouteMapCancellationType, manager);
				}
			}
		}

		bool CreateRouteMapInformationMessage(CargoIMPPhase2MessageManager manager, NotificationBuffer messageCreationNotificationBuffer, NotificationBuffer canCreateNotificationBuffer)
		{
			bool result = false;
			if (manager.IsRouteMapInformationMessageCanBeCreated(canCreateNotificationBuffer))
			{
				result = manager.CreateRouteMapInformationMessage(messageCreationNotificationBuffer);
				if (!result)
				{
					SendErrorEmail(messageCreationNotificationBuffer, CargoIMPPhase2MessageManager.RouteMapInformationType, manager);
				}
			}

			return result;
		}

		void SendErrorEmail(NotificationBuffer notifications, string messageType, CargoIMPPhase2MessageManager manager)
		{
			EmailDef notificationEmail = new EmailDef();
			notificationEmail.AddRecipientForUserCommunication(NotificationUtils.GetRecipients(this.action.Factory,
				GetRecipientEmail(),
				ForwardingConfigurationRegistry.Instance.CargoIMPPhase2SendMessageErrors.Value,
				ForwardingConfigurationRegistry.Instance.CargoIMPPhase2GroupToSendMessageErrors.Value).ToArray());

			if (notificationEmail.Recipients.Count > 0)
			{
				string body = notifications.AsString;
				notificationEmail.Subject = Res.GetString("a621d035-10a2-41e7-afd1-e46976073ebb", "CargoIMP Phase 2 {0} Message for {1} Shipment Creation Error", messageType, manager.Shipment.JS_UniqueConsignRef);
				notificationEmail.Body = body;
				Env.OutgoingMailManager.CreateAndSave(notificationEmail);
			}
		}

		string GetRecipientEmail()
		{
			return Env.CurrentUser?.EmailAddress ?? string.Empty;
		}

		ZString GetCargoIMPEvent()
		{
			var cargoImpEvent = ZString.Empty;
			if (queuedLog != null)
			{
				var eventsMapping = ForwardingConfigurationRegistry.Instance.CargoIMPPhase2MSUEventsMapping.Value;
				if (eventsMapping != null)
				{
					cargoImpEvent = eventsMapping.GetCargoIMPPhase2MSUEventFromEnterpriseEvent(queuedLog.SJ_SE_NKEvent, queuedLog.SJ_Reference);
				}
			}

			return cargoImpEvent;
		}

		class NotificationBufferWithWarnings : NotificationBuffer
		{
			public NotificationBufferWithWarnings(INotifications inner, ForwardingShipment shipment, string messageType)
				: base(inner)
			{
				this.shipment = shipment;
				this.messageType = messageType;
			}

			readonly ForwardingShipment shipment;
			readonly string messageType;
			bool headerMessageIsAlreadyAdded;

			protected override void QueryUser(IQueryUserEventArgs e)
			{
				base.QueryUser(e);
				QueryUserYesNoEventArgs yesNoEventArgs = e as QueryUserYesNoEventArgs;
				if (yesNoEventArgs != null && !HasErrors && HasWarnings &&
						ForwardingConfigurationRegistry.Instance.CargoIMPPhase2AllowToSendMilestoneMessagesWithWarnings.Value)
				{
					yesNoEventArgs.Response = true;
				}
			}

			public override void Notify(INotification @event)
			{
				if (!this.headerMessageIsAlreadyAdded)
				{
					this.headerMessageIsAlreadyAdded = true;
					Inner.Add(new InfoNotification(Res.GetString("d064b07e-7406-4f84-90d1-ef5535fbd666", "CargoIMP Phase 2 {0} Message for Shipment {1}", messageType, shipment.JS_UniqueConsignRef)));
				}

				base.Notify(@event);
			}
		}
	}
}
