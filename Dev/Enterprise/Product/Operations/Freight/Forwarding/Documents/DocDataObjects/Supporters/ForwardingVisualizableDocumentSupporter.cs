using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Business
{
	public abstract class ForwardingVisualizableDocumentSupporter<T> : IVisualizableDocumentSupporter where T : BusinessObject
	{
		protected ForwardingVisualizableDocumentSupporter(T parent)
		{
			this.parent = Argument.NotNull(parent, nameof(parent));
		}

		protected readonly T parent;

		public virtual object GetBusinessObjectInAnotherFactory(BusinessObjectFactory factory, object bizObj)
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

		public virtual IEnumerable<IDocument> GetAdditionalDocuments(IDocument document, IMessageInstructions messageInstructions)
		{
			yield break;
		}

		#region GetUniversalXmlDataObject

		public Either<string, ITopLevelDataObject> GetUniversalXmlDataObject(IDataObjectWriterStrategy strategy, IDocument document, MessageType messageType)
		{
			var dataObject = ObjectFactory.Get<IForwardingDocDataObjectUXmlWriter>().GetDataObject(strategy, document, messageType);

			return dataObject != null
				? new Either<string, ITopLevelDataObject>(dataObject)
				: Res.GetString("e5ca512d-fadf-45ba-b2a5-35f58341fb2e", "Cannot produce Universal XML for {0}.", document.DataContext);
		}

		#endregion

		#region GetDocDataObject

		public Either<string, object> GetDocDataObject(object parent, string dataContext, IDocDataObjectParameters parameters)
		{
			var docDataObject = ObjectFactory.Get<IForwardingDocDataObjectProvider>().GetDocDataObject(parent, dataContext, parameters);

			return docDataObject
				?? Res.GetString("4a1cf23c-ad9f-4d59-8fd2-1d37db9dc8aa", "DataContext '{0}' is not supported.", dataContext);
		}

		#endregion

		#region GetMessageBroker

		public virtual string GetMessageBroker() => string.Empty;

		#endregion

		#region ShouldUseDraftWatermark

		public virtual bool ShouldUseDraftWatermark(IDocument document) => false;

		#endregion
	}
}
