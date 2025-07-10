using System;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	public class ContainerEventsDataImporter : DataImporter
	{
		protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
		{
			NotificationBuffer buffer = new NotificationBuffer(notifications);
			ValueObjectImportContext context = new ValueObjectImportContext(FactoryProvider.Current, buffer);
			additionalTransactionActions = Array.Empty<ITransactionParticipant>();

			try
			{
				ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.ContainerEvents));

				object serializedObject = serializer.Deserialize(dataReader);
				Xsd.ContainerEvents xsdContainerEvents = serializedObject as Xsd.ContainerEvents;
				if (xsdContainerEvents != null)
				{
					foreach (Xsd.ContainerEvent containerEvent in xsdContainerEvents.EventDates)
					{
						Importer.Import(context, containerEvent);
					}
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				HandleException(e, notifications);
			}
			return !buffer.ContainsNotificationType(ErrorType.XmlSchemaValidation);
		}

		ContainerEventImporter Importer
		{
			get
			{
				if (importer == null)
				{
					importer = new ContainerEventImporter();
				}
				return importer;
			}
		}
		ContainerEventImporter importer;
	}
}
