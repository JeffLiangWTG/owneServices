using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Business.Documents.DocDataObjects
{
	public abstract class CustomsVisualizableDocumentSupporter<T> : IVisualizableDocumentSupporter where T : BusinessObject
	{
		protected CustomsVisualizableDocumentSupporter(T parent)
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

			var dataStoreName = GetDataStoreNameFromDocumentName(documentName);

			if (!string.IsNullOrWhiteSpace(dataStoreName))
			{
				var loader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
				return (BusinessObject)loader.Load(parent, dataStoreName);
			}

			return null;
		}

		protected abstract string GetDataStoreNameFromDocumentName(string documentName);
		public abstract IMessageEventsProcessor GetMessageEventsProcessor(IDocument document);
		public abstract IMessageLogCreator GetMessageLogCreator(IDocument document);
		public abstract IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions);

		public abstract ISecurityCheckpoint CustomizeFormCheckpoint { get; }
		public abstract Either<string, object> GetAdditionalData(object obj, IStmMenuItem menuItem);
		public abstract IEnumerable<IMacroLibrary> GetLibraries(string dataContext);

		public IEnumerable<ICommand> GetCustomCommands(string dataContext)
		{
			return GetCustomCommandsCore(dataContext);
		}

		protected virtual IEnumerable<ICommand> GetCustomCommandsCore(string dataContext)
		{
			yield break;
		}

		protected virtual string GetDocProviderKey()
		{
			return string.Empty;
		}

		public IEnumerable<IDocument> GetAdditionalDocuments(IDocument document, IMessageInstructions messageInstructions)
		{
			yield break;
		}

		#region GetUniversalXmlDataObject

		public Either<string, ITopLevelDataObject> GetUniversalXmlDataObject(IDataObjectWriterStrategy strategy, IDocument document, MessageType messageType = MessageType.Unspecified)
		{
			var key = GetDocProviderKey();
			var dataContext = document.DataContext;
			var errorMessage = Res.GetString("e5ca512d-fadf-45ba-b2a5-35f58344da3", "Cannot produce Universal XML for {0}.", dataContext);
			if (string.IsNullOrWhiteSpace(key))
			{
				return errorMessage;
			}

			var writeDict = ObjectFactory.Get<Hashtable>("ICustomsDocDataObjectUXmlWriter");
			var handle = (ObjectHandle)writeDict[key];

			return handle != null
				? new Either<string, ITopLevelDataObject>(((ICustomsDocDataObjectUXmlWriter)handle.GetObject()).GetDataObject(strategy, document))
				: errorMessage;
		}

		#endregion

		#region GetDocDataObject

		public Either<string, object> GetDocDataObject(object obj, string dataContext, IDocDataObjectParameters parameters)
		{
			var providerKey = GetDocProviderKey();
			var errorMessage = Res.GetString("4a1cf23c-ad9f-4d59-8fd2-1d37db9dc1da", "DataContext '{0}' is not supported.", dataContext);
			var objectName = obj?.GetFormattedName() ?? ZString.Empty;
			var provider = CustomsDocDataObjectProviderHelper.GetProvider(providerKey, dataContext, objectName);
			return provider?.GetDocDataObject(obj, dataContext, parameters) ?? errorMessage;
		}

		#endregion

		public string GetMessageBroker() => string.Empty;
		public bool ShouldUseDraftWatermark(IDocument document) => false;
	}
}
