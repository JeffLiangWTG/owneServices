using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class CargoIMPPhase2MessageManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		public const string RouteMapInformationType = "RMI";
		public const string RouteMapCancellationType = "RMX";
		public const string MilestoneStatusUpdateType = "MSU";

		protected CargoIMPPhase2MessageManager(ForwardingShipment shipment)
			: base(shipment.Factory)
		{
			this.shipment = shipment;
		}
		protected readonly ForwardingShipment shipment;

		public static CargoIMPPhase2MessageManager New(ForwardingShipment shipment)
		{
			Argument.NotNull(shipment, "shipment");

			CargoIMPPhase2MessageManager result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(shipment);
			}
			else
			{
				result = new CargoIMPPhase2MessageManager(shipment);
			}

			return result;
		}

		protected delegate CargoIMPPhase2MessageManager NewDelegate(ForwardingShipment shipment);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#region Properties

		public ForwardingShipment Shipment
		{
			get { return shipment; }
		}

		public CargoIMPPhase2EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new CargoIMPPhase2EDIMessageCollection(shipment);
					fMessages.CountChanged += Messages_CountChanged;
				}

				return fMessages;
			}
		}
		CargoIMPPhase2EDIMessageCollection fMessages;

		public ZString CurrentStatus
		{
			get
			{
				foreach (EDIMessage message in OrderedByDateMessages)
				{
					switch (message.EM_Status)
					{
						case EDIMessage.Status.Queued:
							return Res.GetString("ee59ba4d-fe66-43a6-b298-233fa194087c", "Message Queued for Sending");

						case EDIMessage.Status.Sent:
							if (message.EM_MessageType == RouteMapCancellationType)
							{
								return Res.GetString("dd203e7a-26dd-45d9-bb2f-a2ae48b17f68", "Message Canceled");
							}
							else if (message.EM_MessageType == RouteMapInformationType)
							{
								return Res.GetString("6a2f0ec0-8d65-4f87-9f73-3278c38f2070", "Route Map Information Sent");
							}
							else if (message.EM_MessageType == MilestoneStatusUpdateType)
							{
								if (message.EM_MessageSubType.IsEmpty)
								{
									return Res.GetString("7bc0042e-0e19-499d-b9c6-a148688659b8", "Milestone Status Update Sent");
								}
								else
								{
									return Res.GetString("eff19916-55b0-4630-9384-af94645a0cc7", "Milestone Status Update ({0}) Sent", message.EM_MessageSubType);
								}
							}

							return Res.GetString("897eda23-dd20-4683-bb6a-985eb4894e68", "Message Sent");

						case EDIMessage.Status.Failed:
							return Res.GetString("0d878366-3de9-4bb3-a61b-6863d88f54d1", "Message Failed");
					}
				}

				return Res.GetString("d42a671a-f405-4e1f-8019-15d346ee0d81", "No messages have been sent");
			}
		}

		public ZPropertyInfo CurrentStatusInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentStatus)); }
		}

		public void ValidateCurrentStatus()
		{
			CurrentStatusInfo.ClearAllNotifications();
		}

		public ZString LastMessageCreationLog
		{
			get
			{
				if (!this.lastMessageCreationLogInitialized)
				{
					this.lastMessageCreationLogInitialized = true;
					CargoIMPPhase2MessageNote note = new CargoIMPPhase2MessageNote(Shipment);
					this.lastMessageCreationLog = note.LoadFromNote();
				}

				return this.lastMessageCreationLog;
			}
		}

		bool lastMessageCreationLogInitialized;
		ZString lastMessageCreationLog;

		public ZPropertyInfo LastMessageCreationLogInfo
		{
			get { return GetZPropertyInfo(nameof(LastMessageCreationLog)); }
		}

		#endregion

		#region Public Methods

		public ZBool IsRouteMapInformationMessageCanBeCreated(INotifications notifications)
		{
			bool result = CheckShipmentIsAir(notifications);
			if (result)
			{
				result = CheckForwarderIdIsSet(notifications);
			}

			if (result)
			{
				result = CheckPODIsNotSent(notifications);
			}

			if (result)
			{
				ForwardingConsol targetConsol = FindTargetCosol();
				if (targetConsol == null)
				{
					if (notifications != null)
					{
						notifications.AddError(Res.GetString("7f3f6229-90c6-b187-4be2-a26bd8297ed9", "The consol must be gateway-enabled and the Sending and Receiving agent must be in the same Management grouping as the Current Company/Branch Organization proxy."));
					}

					result = false;
				}
				else if (!DoesConsolCorrespondToRouteMap(targetConsol))
				{
					if (notifications != null)
					{
						notifications.AddError(Res.GetString("b5396834-d34d-41a6-ab2e-cca1a384819d", "No records found in '{0}' registry item corresponding to any Flights in consolidation", ForwardingConfigurationRegistry.Instance.CargoIMPPhase2RouteMap.Caption));
					}

					result = false;
				}

				if (result)
				{
					result = IsMessageValidAfterAdditionalCheck(notifications);
				}
			}

			return result;
		}

		public ZBool CreateRouteMapInformationMessage(INotifications notifications)
		{
			MessageBuilder builder = new RMIMessageBuilder();
			return CreateAndSaveMessage(builder, RouteMapInformationType, notifications);
		}

		public ZBool IsRouteMapCancellationMessageCanBeCreated(INotifications notifications)
		{
			bool result = true;
			if (OrderedByDateMessages.Length == 0 || OrderedByDateMessages[0].EM_MessageType == RouteMapCancellationType)
			{
				result = false;
				if (notifications != null)
				{
					notifications.AddError(Res.GetString("3faf5bcc-d0d3-4e5e-ba77-d3229deb0122", "CargoIMP Phase 2 Cancellation message can be sent only after previously sent Route Map or Status Update message."));
				}
			}

			return result;
		}

		public ZBool CreateRouteMapCancellationMessage(INotifications notifications)
		{
			MessageBuilder builder = new RMXMessageBuilder();
			return CreateAndSaveMessage(builder, RouteMapCancellationType, notifications);
		}

		internal ZBool MustRouteMapInformationBeSentBeforeMilestoneStatusUpdate()
		{
			return OrderedByDateMessages.Length == 0 || OrderedByDateMessages[0].EM_MessageType == RouteMapCancellationType;
		}

		public ZBool IsMilestoneStatusUpdateCanBeCreated(INotifications notifications)
		{
			bool result = true;
			if (MustRouteMapInformationBeSentBeforeMilestoneStatusUpdate())
			{
				result = false;
				if (notifications != null)
				{
					notifications.AddError(Res.GetString("c0c11924-6d00-4f3b-a99a-eedbcb3c7a7d", "CargoIMP Phase 2 Status Update message can be sent only after previously sent Route Map or Status Update message."));
				}
			}
			else
			{
				result = IsRouteMapInformationMessageCanBeCreated(notifications);
			}

			return result;
		}

		public ZBool CreateMilestoneStatusUpdateMessage(ZString milestoneStatusCode, ZDateTime milestoneStatusDate, INotifications notifications)
		{
			MSUMessageBuilder builder = new MSUMessageBuilder();
			builder.MilestoneStatusCode = milestoneStatusCode;
			builder.MilestoneStatusDate = milestoneStatusDate;
			return CreateAndSaveMessage(builder, MilestoneStatusUpdateType, milestoneStatusCode, notifications);
		}

		#endregion

		#region Implementation

		protected virtual bool IsMessageValidAfterAdditionalCheck(INotifications notificationBuffer)
		{
			return true;
		}

		void Messages_CountChanged(object sender, EventArgs e)
		{
			CurrentStatusInfo.RefreshBinding();
		}

		EDIMessage[] OrderedByDateMessages
		{
			get
			{
				return EDIMessageComparer.GetSortedMessages<EDIMessage>(Messages, System.ComponentModel.ListSortDirection.Descending);
			}
		}

		bool IsMessageStillCanBeCreatedAfterErrors(NotificationBuffer notificationBuffer)
		{
			bool createMessage = true;
			bool containsCriticalErrors = false;
			if (ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ServiceProvider.Value == CargoIMPPhase2ServiceProviderList.Codes.Traxon &&
					string.IsNullOrEmpty(ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.Value))
			{
				notificationBuffer.Add(ErrorType.Error, Res.GetString("58c41dde-a0d4-4ce8-98d7-a2467cb29a9e", "Registry item '{0}' must be set", ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.Caption));
				containsCriticalErrors = true;
			}

			if (notificationBuffer.HasErrors || notificationBuffer.HasWarnings)
			{
				if (containsCriticalErrors || notificationBuffer.HasErrors && !ForwardingConfigurationRegistry.Instance.CargoIMPPhase2AllowToSendMessagesWithErrors.Value)
				{
					notificationBuffer.Inner.Add(ErrorType.Error, Res.GetString("d68bf4f0-5688-46ec-9850-297d1e623d4f", "Message was created with errors. Sending this message with errors is not allowed as it will be rejected."));
					createMessage = false;
				}
				else
				{
					string question = Res.GetString("c5bc42a7-e80c-4561-92ad-a51bb139de26", "Message was created with {0}.\r\nDo you still wish to send it?",
						notificationBuffer.HasErrors
							? (NoResString)"errors.\r\nSending this message with errors is not advised\r\nas it will most likely be rejected and cause delays in your process"
							: Res.GetString("0e9f1c00-d50f-4a46-88a6-84aba43621e3", "warnings.\r\nSending this message with warnings can cause delays in your process"));
					QueryUserYesNoEventArgs queryUserargs = new QueryUserYesNoEventArgs(Res.GetString("829b1275-2742-40f5-8d29-d2d6937896bf", "CargoIMP Phase 2 Message Creation"), question, false);
					notificationBuffer.Inner.QueryUser(queryUserargs);

					if (!queryUserargs.Response)
					{
						createMessage = false;
					}
				}
			}

			return createMessage;
		}

		ZBool CreateAndSaveMessage(MessageBuilder builder, ZString messageType, ZString messageSubType, INotifications notifications)
		{
			NotificationBuffer notificationBuffer = new NonDuplicateNotificationBuffer(notifications);
			builder.Shipment = Shipment;
			builder.Notifications = notificationBuffer;
			ZString messageText = builder.MessageText;
			bool createMessage = IsMessageStillCanBeCreatedAfterErrors(notificationBuffer);
			if (createMessage)
			{
				EDIMessage message = Messages.AddNew();
				message.EM_MessageText = messageText;
				message.EM_MessageType = messageType;
				message.EM_MessageSubType = messageSubType;
				shipment.Factory.Save();
			}

			return createMessage;
		}

		ZBool CreateAndSaveMessage(MessageBuilder builder, ZString messageType, INotifications notifications)
		{
			return CreateAndSaveMessage(builder, messageType, ZString.Empty, notifications);
		}

		bool CheckShipmentIsAir(INotifications notifications)
		{
			bool result = true;
			if (!shipment.IsAir)
			{
				if (notifications != null)
				{
					notifications.AddError(Res.GetString("2aa268ce-e94c-4972-9af7-9f6ac85a91f4", "CargoIMP Phase 2 integration is only available for Air Shipments."));
				}

				result = false;
			}

			return result;
		}

		bool CheckForwarderIdIsSet(INotifications notifications)
		{
			bool result = true;
			if (string.IsNullOrEmpty(ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.Value))
			{
				if (notifications != null)
				{
					notifications.AddError(Res.GetString("88ea6e8b-6c1c-4d4f-8b5a-1fe07cd03ae5", "'{0}' registry item must be set", ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.Caption));
				}

				result = false;
			}

			return result;
		}

		bool CheckPODIsNotSent(INotifications notifications)
		{
			bool result = true;
			foreach (EDIMessage message in OrderedByDateMessages)
			{
				if (message.EM_MessageType != MilestoneStatusUpdateType)
				{
					break;
				}
				else if (message.EM_MessageSubType == CargoIMPPhase2MSUEventCodeList.Codes.POD)
				{
					if (!message.EM_MessageDateTime.IsEmpty && ZDateTime.Now - message.EM_MessageDateTime > DelayAfterPODMessageToSendAnotherMessage)
					{
						result = false;
						if (notifications != null)
						{
							notifications.AddError(Res.GetString("24f726b2-53f3-400e-8cdf-ef6e45068154", "No CargoIMP Phase 2 message can be sent after previously sent POD Status Update message."));
						}
					}

					break;
				}
			}

			return result;
		}

		TimeSpan DelayAfterPODMessageToSendAnotherMessage
		{
			get { return new TimeSpan(0, 15, 0); }
		}

		ForwardingConsol FindTargetCosol()
		{
			ForwardingConsol targetConsol = null;
			foreach (ForwardingConsol consol in shipment.Consols)
			{
				if (consol.JK_TransportMode == Constants.TransportModes.Air && (consol.IsAgent || consol.IsGatewayConsol))
				{
					if (OrgHasSameManagementGroupAsOrgProxies(consol.SendingForwarder) &&
						OrgHasSameManagementGroupAsOrgProxies(consol.ReceivingForwarder))
					{
						targetConsol = consol;
						break;
					}
				}
			}

			return targetConsol;
		}

		public bool IsShipmentAttachedToCorrectConsol()
		{
			return FindTargetCosol() != null;
		}

		bool OrgHasSameManagementGroupAsOrgProxies(OrgHeader org)
		{
			return org != null
				&& GlbBranch.CurrentBranch.OrgProxy != null
				&& org.ManagementGrouping.PK == GlbBranch.CurrentBranch.OrgProxy.ManagementGrouping.PK;
		}

		bool DoesConsolCorrespondToRouteMap(ForwardingConsol consol)
		{
			bool routeMapFound = false;
			foreach (Transport transport in consol.Transports)
			{
				if (transport.JW_TransportMode == Constants.TransportModes.Air)
				{
					var routeMap = ForwardingConfigurationRegistry.Instance.CargoIMPPhase2RouteMap.Value;
					for (int i = 0; i < routeMap.Count; i++)
					{
						var route = routeMap[i];
						if (route.AirlineTwoCharacterCode == consol.TwoLetterAirlineCode
								&& IsOrContains(route.Origin, transport.JW_RL_NKLoadPort)
								&& IsOrContains(route.Destination, transport.JW_RL_NKDiscPort))
						{
							routeMapFound = true;
							break;
						}
					}
				}
			}

			return routeMapFound;
		}

		bool IsOrContains(ZString locationCode, ZString unlocoCode)
		{
			ILocation location = LocationHelper.GetLocationFromString(locationCode, Factory);
			ILocation unloco = LocationHelper.GetLocationFromString(unlocoCode, Factory);
			return location != null && unloco != null && location.CompletelyCovers(unloco);
		}

		#endregion
	}
}

#region Creation
#endregion
#region Saving/Message Number
#endregion
