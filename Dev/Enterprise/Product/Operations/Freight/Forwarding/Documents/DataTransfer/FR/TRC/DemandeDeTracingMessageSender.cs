using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.ZArchitecture.Environment;
using ConsolDocumentDataStoreNames = Enterprise.Freight.Forwarding.Documents.DocDataObjects.ConsolDocumentDataStoreNames;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.FR
{
	public static class DemandeDeTracingMessageSender
	{
		public static bool SendMessage(ITRCDetails trcDetails, DemandeDeTracingDirection direction, out IReadOnlyCollection<INotification> notifications)
			=> SendMessage(trcDetails, null, direction, out notifications);

		public static bool SendMessage(ITRCDetails trcDetails, CommonContainer preSelectedContainer, DemandeDeTracingDirection direction, out IReadOnlyCollection<INotification> notifications)
		{
			if (trcDetails == null)
			{
				notifications = Array.Empty<INotification>();
				return false;
			}

			var selectedContainers = preSelectedContainer != null
				? new[] { preSelectedContainer }
				: GetContainersToSend(trcDetails);

			if (selectedContainers.IsLeft)
			{
				notifications = Array.Empty<INotification>();
				return false;
			}

			if (selectedContainers.Right.Length == 0)
			{
				notifications = Array.Empty<INotification>();
				return false;
			}

			var containers = selectedContainers
				.Right
				.ToArray();

			var builder = new DemandeDeTracingBuilder(trcDetails, containers, direction);
			var demandeDeTracing = builder.Build();

			if (demandeDeTracing.HasMessageErrors)
			{
				notifications = demandeDeTracing.Notifications.GetNotifications(CargoWise.EntityFramework.NotificationType.MessageError).ToArray();
				return false;
			}

			var trcDetailsForMessageProcessing = GetTRCDetailsForMessageProcessing(trcDetails);
			using (trcDetailsForMessageProcessing.Factory.AddDisposableServiceIfRequired())
			{
				var documentDataStorage = LoadOrCreateDocumentDataStorage(trcDetailsForMessageProcessing, direction);

				var messageInstructions = new DemandeDeTracingMessageMessageInstructions(direction);
				var notificationsHandler = new NotificationsHandler();

				var res = NoUIMessageSender.SendUXml(messageInstructions, documentDataStorage, demandeDeTracing, notificationsHandler);

				notifications = notificationsHandler.Notifications.ToArray();
				return res;
			}
		}

		static Either<string, CommonContainer[]> GetContainersToSend(ITRCDetails trcDetails)
		{
			var availableContainers = trcDetails
				.Containers
				.ToArray();

			if (Globals.IsUserInteractive)
			{
				var selector = ObjectFactory.Get<IContainerSelector>();
				return selector.SelectContainers(availableContainers, ContainerSelectorMode.TRC);
			}

			return availableContainers;
		}

		static BusinessObject GetTRCDetailsForMessageProcessing(ITRCDetails trcDetails)
		{
			if (Globals.IsUserInteractive)
			{
				// to prevent setting bizObj as HasChanges
				var messageFactory = new BusinessObjectFactory();
				messageFactory.RefreshEnabled = false;
				var reloadBizObj = messageFactory.ImportFromAnotherFactory(trcDetails.BusinessObject);

				return reloadBizObj;
			}

			// reloading bizObj is not necessary when sending message from workflow
			return trcDetails.BusinessObject;
		}

		static IVisualizerDocumentData LoadOrCreateDocumentDataStorage(BusinessObject businessObject, DemandeDeTracingDirection direction)
		{
			var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();

			var dataStoreName = direction == DemandeDeTracingDirection.Import
				? ConsolDocumentDataStoreNames.DemandeDeTracingImport
				: ConsolDocumentDataStoreNames.DemandeDeTracingExport;

			var documentData = documentDataLoader.Load(businessObject, dataStoreName);

			if (documentData == null)
			{
				documentData = businessObject.Factory.New<IVisualizerDocumentData>();
				documentData.Parent = businessObject;
				documentData.Name = dataStoreName;
			}

			return documentData;
		}
	}
}
