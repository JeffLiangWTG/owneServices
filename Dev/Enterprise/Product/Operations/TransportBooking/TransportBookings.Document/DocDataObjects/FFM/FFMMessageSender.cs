using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.TransportBookings.Document
{
	[CodeAlive("Will called in WI00794773.")]
	public static class FFMMessageSender
	{
		public static bool SendMessage(IFFMDetails ffmDetails, out IReadOnlyCollection<INotification> notifications)
		{
			if (ffmDetails == null)
			{
				notifications = Array.Empty<INotification>();
				return false;
			}

			IContext context = new TransportBookingsCommonContext(ffmDetails.BusinessObject.Factory.GetCachedReadOnlyFactory());

			var builder = new FFMMessageBuilder(ffmDetails, context);
			var ffmMessage = builder.Build();

			if (ffmMessage.HasErrors)
			{
				notifications = ffmMessage.GetErrors().ToArray();
				return false;
			}

			var ffmDetailsForMessageProcessing = GetFFMDetailsForMessageProcessing(ffmDetails);
			using (ffmDetailsForMessageProcessing.Factory.AddDisposableServiceIfRequired())
			{
				var documentDataStorage = LoadOrCreateDocumentDataStorage(ffmDetailsForMessageProcessing);

				var messageInstructions = new FFMMessageMessageInstructions();
				var notificationsHandler = new NotificationsHandler();

				var res = NoUIMessageSender.SendUXml(messageInstructions, documentDataStorage, ffmMessage, notificationsHandler);

				notifications = notificationsHandler.Notifications.ToArray();
				return res;
			}
		}

		static BusinessObject GetFFMDetailsForMessageProcessing(IFFMDetails ffmDetails)
		{
			if (Globals.IsUserInteractive)
			{
				// to prevent setting bizObj as HasChanges
				var messageFactory = new BusinessObjectFactory();
				messageFactory.RefreshEnabled = false;
				var reloadBizObj = messageFactory.ImportFromAnotherFactory(ffmDetails.BusinessObject);

				return reloadBizObj;
			}

			// reloading bizObj is not necessary when sending message from workflow
			return ffmDetails.BusinessObject;
		}

		static IVisualizerDocumentData LoadOrCreateDocumentDataStorage(BusinessObject businessObject)
		{
			var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();

			var documentData = documentDataLoader.Load(businessObject, BookingsDocumentDataStoreNames.FFMMessageRequest);

			if (documentData == null)
			{
				documentData = businessObject.Factory.New<IVisualizerDocumentData>();
				documentData.Parent = businessObject;
				documentData.Name = BookingsDocumentDataStoreNames.FFMMessageRequest;
			}

			return documentData;
		}
	}
}
