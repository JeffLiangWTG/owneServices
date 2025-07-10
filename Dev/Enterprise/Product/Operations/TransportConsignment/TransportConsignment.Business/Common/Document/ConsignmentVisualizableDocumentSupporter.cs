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

namespace Enterprise.TransportConsignment.Business
{
	public abstract class ConsignmentVisualizableDocumentSupporter<T> : IVisualizableDocumentSupporter where T : BusinessObject
	{
		protected ConsignmentVisualizableDocumentSupporter(T parent)
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

		public Either<string, ITopLevelDataObject> GetUniversalXmlDataObject(IDataObjectWriterStrategy strategy, IDocument document, MessageType messageType = MessageType.Unspecified)
		{
			return new Either<string, ITopLevelDataObject>((ITopLevelDataObject)null);
		}

		public Either<string, object> GetDocDataObject(object parent, string dataContext, IDocDataObjectParameters parameters)
		{
			var docDataObject = ObjectFactory.Get<IDtbConsignmentDocDataObjectProvider>().GetDocDataObject(parent, dataContext);

			return docDataObject
				?? ResString.GetMultilingualString("78113fa1-14f2-11f0-a17f-5c00e99e3d97", "DataContext '{0}' is not supported.", dataContext);
		}

		public abstract string GetMessageBroker();

		public IEnumerable<IDocument> GetAdditionalDocuments(IDocument document, IMessageInstructions messageInstructions)
		{
			yield break;
		}

		public abstract bool ShouldUseDraftWatermark(IDocument document);
	}
}
