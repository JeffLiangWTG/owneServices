using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.QuotedBookings.Business
{
	internal sealed class QuotedBookingVisualizableDocumentSupporter : IVisualizableDocumentSupporter
	{
		public object GetBusinessObjectInAnotherFactory(BusinessObjectFactory factory, object bizObj)
		{
			if (bizObj is QuotedBooking quotedBooking)
			{
				return QuotedBooking.New(quotedBooking.Quote?.PK ?? ZGuid.Empty, quotedBooking.Booking?.PK ?? ZGuid.Empty, factory);
			}

			return null;
		}

		public object GetEventParent(IXmlEventValueObject universalEvent) => default;

		public IMessageEventsProcessor GetMessageEventsProcessor(IDocument document) => null;
		public IMessageLogCreator GetMessageLogCreator(IDocument document) => null;
		public IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions) => null;

		public IEnumerable<IMacroLibrary> GetLibraries(string dataContext)
		{
			switch (dataContext)
			{
				case DataContext.HouseBill:
					yield return (IMacroLibrary)ObjectFactory.Get<IHouseBillMacroLibrary>();
					break;
			}
		}

		public IEnumerable<ICommand> GetCustomCommands(string dataContext)
		{
			yield break;
		}

		public Either<string, object> GetDocDataObject(object parent, string dataContext, IDocDataObjectParameters parameters)
		{
			var docDataObject = ObjectFactory.Get<IForwardingDocDataObjectProvider>().GetDocDataObject(parent, dataContext, parameters);

			return docDataObject
				?? Res.GetString("1a09da06-59d9-4254-820c-7c4427bd8a9a", "DataContext '{0}' is not supported.", dataContext);
		}

		public Either<string, object> GetAdditionalData(object parent, IStmMenuItem menuItem) => (object)null;

		public Either<string, ITopLevelDataObject> GetUniversalXmlDataObject(IDataObjectWriterStrategy writerStrategy, IDocument document, MessageType messageType = MessageType.Unspecified)
		{
			return new Shipment(writerStrategy);
		}

		public IEnumerable<IDocument> GetAdditionalDocuments(IDocument document, IMessageInstructions messageInstructions)
		{
			yield break;
		}

		public ISecurityCheckpoint CustomizeFormCheckpoint => Env.Security.MaintainShipmentCustomiseForms;

		public string GetMessageBroker() => string.Empty;
		public bool ShouldUseDraftWatermark(IDocument document) => false;
	}
}
