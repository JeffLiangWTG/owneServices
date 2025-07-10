using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.Business
{
	public class EHBLUniversalEventSenderHelper
	{
		public EHBLUniversalEventSenderHelper()
		{
		}

		public string EHubClientID { get; set; }

		public string DirectXTClientID { get; set; }

		public string MessageBroker { get; set; } = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;

		#region SendUniversalEvent

		public ZString SendUniversalEvent(BusinessObject bizObj, ZString eventCode, KeyValuePair<string, string>[] parameters, string reason)
		{
			Argument.NotNull(bizObj, nameof(bizObj));
			Argument.NotNullOrEmpty(eventCode, nameof(eventCode));
			Argument.NotNull(parameters, nameof(parameters));
			Argument.NotNullOrEmpty(reason, nameof(reason));

			if (string.IsNullOrEmpty(EHubClientID) && string.IsNullOrEmpty(DirectXTClientID))
			{
				throw new ArgumentException("Incorrect EHubClientID and DirectXTClientID parameters.");
			}

			var eventManager = bizObj.GetUniversalDataContextManager() as IEventDataContextManager ?? throw new ArgumentException("bizObj is not an event data context manager.", nameof(bizObj));
			var useNewFactory = bizObj.IsInDatabase && Globals.IsUserInteractive;

			BusinessObjectFactory factory;
			BusinessObject docDataParent;

			if (useNewFactory)
			{
				factory = new BusinessObjectFactory();
				docDataParent = factory.ImportFromAnotherFactory(bizObj);
			}
			else
			{
				factory = bizObj.Factory;
				docDataParent = bizObj;
			}

			using (useNewFactory ? factory.AddDisposableService() : null)
			{
				var actionInfo = new ActionInfo(null, docDataParent);
				var writer = eventManager.GetEventDataObjectWriter(new DataWritingManager(actionInfo));

				var msgSentParams = parameters.Where(p => p.Key != CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department).ToList();
				msgSentParams.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, ElectronicBOLConstants.EHBLEventDepartments.TitleRegistry));

				var eventLog = docDataParent.GetLogs().AddNew(AutoEvents.MessageSent, msgSentParams.ToArray());
				if (writer.GetDataObject(eventLog) is UniversalEvent eventData)
				{
					eventData.EventReference = eventData.EventReference.HasValue ? StmALog.GetFreeTextFromReference(eventData.EventReference.Value) : null;
					if (eventData.EventReference.HasValue && string.IsNullOrEmpty(eventData.EventReference.Value))
					{
						eventData.EventReference = null;
					}
					eventData.EventType = eventCode;

					AddEventParameters(eventData, parameters.ToDictionary(p => p.Key, p => p.Value));
					AddNotificationDetails(eventData, reason);

					var notifications = new NullLogger();
					var context = new DeliveryContext(factory)
					{
						ParentInfo = EntityInfo.New(docDataParent),
						ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging,
						MessageTypeCode = EDIMessageTypeList.Codes.XDC,
						MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalEvent,
						Notifications = notifications
					};
					var mode = SetupCommunicationMode();
					var streamWrapper = new DeliveryStreamWrapperUXML(context.ParentInfo, eventData, ObjectFactory.Get<IXmlWriter>(), null);
					var delivery = new EDIMessageDelivery();
					delivery.Deliver(context, mode, streamWrapper);

					if (Globals.IsUserInteractive || Globals.IsWebService)
					{
						try
						{
							ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);
						}
						catch (ZSaveException ex)
						{
							return Res.GetString("e2eca487-e2dd-4aed-a56d-f6969ccfb162", "The following error was encountered when saving the universal event: {0}", ex.Message);
						}
					}
				}
				return ZString.Empty;
			}
		}

		NonPersistentEDICommunicationMode SetupCommunicationMode()
		{
			var mode = new NonPersistentEDICommunicationMode();
			mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
			mode.EK_CommunicationsTransport = MessageBroker;

			mode.EK_Destination = MessageBroker == EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface
				? DirectXTClientID
				: EHubClientID;

			return mode;
		}

		void AddNotificationDetails(UniversalEvent eventData, string reason)
		{
			eventData.ContextCollection ??= new List<Context>();

			var notificationDetailsContext = new Context
			{
				Type = new ContextType() { Type = nameof(UniversalEvent.ContextTypes.NotificationDetails) },
				Value = reason
			};

			eventData.ContextCollection.Add(notificationDetailsContext);
		}

		void AddEventParameters(UniversalEvent eventData, Dictionary<string, string> parameters)
		{
			eventData.EventParameters ??= new EventParameters();
			eventData.EventParameters.Type = parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type);
			eventData.EventParameters.Department = parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department);
			eventData.EventParameters.ReferenceNumber = parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber);
			eventData.EventParameters.RequestNumber = parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.RequestNumber);
		}

		#endregion

		#region NullLogger

		class NullLogger : INotifications
		{
			public void Add(INotification notification)
			{
			}
		}

		#endregion
	}
}
