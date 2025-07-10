using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.Document
{
	public abstract class TransportBookingsVisualizableDocumentSupporter<T> : IVisualizableDocumentSupporter where T : BusinessObject
	{
		protected TransportBookingsVisualizableDocumentSupporter(T parent)
		{
			this.parent = Argument.NotNull(parent, nameof(parent));
		}

		protected readonly T parent;

		public object GetBusinessObjectInAnotherFactory(BusinessObjectFactory factory, object bizObj)
		{
			if (bizObj is T t)
			{
				return factory.ImportFromAnotherFactory(t);
			}

			return null;
		}

		public object GetEventParent(IXmlEventValueObject universalEvent)
		{
			var documentName = universalEvent?.DataContext?.DocumentaryOverride?.DocumentName;

			if (!documentName.HasValue)
			{
				return null;
			}

			var eventParameters = universalEvent is Event eventValue
				? eventValue.EventParameters
				: null;

			var dataStoreName = GetDataStoreNameFromDocumentName(documentName, eventParameters);

			if (!string.IsNullOrWhiteSpace(dataStoreName))
			{
				var loader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
				return (BusinessObject)loader.Load(parent, dataStoreName);
			}

			return null;
		}

		protected abstract string GetDataStoreNameFromDocumentName(string documentName, EventParameters eventParameters);

		public abstract IMessageEventsProcessor GetMessageEventsProcessor(IDocument document);
		public abstract IMessageLogCreator GetMessageLogCreator(IDocument document);
		public abstract IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions);

		public abstract ISecurityCheckpoint CustomizeFormCheckpoint { get; }
		public abstract Either<string, object> GetAdditionalData(object parent, IStmMenuItem menuItem);
		public abstract IEnumerable<IMacroLibrary> GetLibraries(string dataContext);

		public virtual IEnumerable<ICommand> GetCustomCommands(string dataContext)
		{
			yield break;
		}

		public Either<string, ITopLevelDataObject> GetUniversalXmlDataObject(IDataObjectWriterStrategy strategy, IDocument document, MessageType messageType)
		{
			var dataObject = ObjectFactory.Get<ITransportBookingsDocDataObjectUXmlWriter>().GetDataObject(strategy, document, messageType);

			return dataObject != null
				? new Either<string, ITopLevelDataObject>(dataObject)
				: Res.GetString("cec04501-4dd9-4913-bfdd-5fe2373ed9f5", "Cannot produce Universal XML for {0}.", document.DataContext);
		}

		public Either<string, object> GetDocDataObject(object parent, string dataContext, IDocDataObjectParameters parameters)
		{
			var docDataObject = ObjectFactory.Get<IDtbBookingDocDataObjectProvider>().GetDocDataObject(parent, dataContext, parameters);

			return docDataObject
				?? ResString.GetMultilingualString("a28be331-6044-4591-8146-6eca7c1f2be2", "DataContext '{0}' is not supported.", dataContext);
		}

		public abstract string GetMessageBroker();

		public IEnumerable<IDocument> GetAdditionalDocuments(IDocument document, IMessageInstructions messageInstructions)
		{
			yield break;
		}

		public abstract bool ShouldUseDraftWatermark(IDocument document);
	}
}
