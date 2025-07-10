using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.Freight.Agency.Documents.DataObjects.Res;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public abstract class AgencyShipmentVisualizableDocumentSupporter<T> : IVisualizableDocumentSupporter where T : AgencyShipment
	{
		protected AgencyShipmentVisualizableDocumentSupporter(T parent)
		{
			this.parent = Argument.NotNull(parent, nameof(parent));
		}

		protected readonly T parent;

		public abstract IMessageEventsProcessor GetMessageEventsProcessor(IDocument document);

		public abstract IMessageLogCreator GetMessageLogCreator(IDocument document);

		public abstract IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions);

		public abstract ISecurityCheckpoint CustomizeFormCheckpoint { get; }

		public abstract Either<string, object> GetAdditionalData(object parent, IStmMenuItem menuItem);

		public virtual IEnumerable<IMacroLibrary> GetLibraries(string dataContext)
		{
			switch (dataContext)
			{
				case DataContext.UXML:
					yield return new FreightLibrary();
					yield break;

				case DataContext.HouseBill:
					yield return new AgencyHouseBillMacroLibrary();
					break;
			}
		}

		public virtual IEnumerable<ICommand> GetCustomCommands(string dataContext)
		{
			yield break;
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

		public object GetBusinessObjectInAnotherFactory(BusinessObjectFactory factory, object bizObj)
		{
			if (bizObj is T t)
			{
				return factory.ImportFromAnotherFactory(t);
			}

			return null;
		}

		public Either<string, object> GetDocDataObject(object parent, string dataContext, IDocDataObjectParameters parameters)
		{
			var docDataObject = new AgencyDocDataObjectProvider().GetDocDataObject(parent, dataContext, parameters);

			return docDataObject
				?? Res.GetString("c3086126-de5b-43ab-bfeb-5348e7d9204a", "DataContext '{0}' is not supported.", dataContext);
		}

		public Either<string, ITopLevelDataObject> GetUniversalXmlDataObject(IDataObjectWriterStrategy strategy, IDocument document, MessageType messageType = MessageType.Unspecified)
		{
			var dataObject = ObjectFactory.Get<IAgencyDocDataObjectUXmlWriter>().GetDataObject(strategy, document, messageType);

			return dataObject != null
				? new Either<string, ITopLevelDataObject>(dataObject)
				: Res.GetString("3a61ee93-5e97-4f30-8aa2-cdb253f2a53b", "Cannot produce Universal XML for {0}.", document?.DataContext ?? string.Empty);
		}

		public string GetMessageBroker() => string.Empty;

		public IEnumerable<IDocument> GetAdditionalDocuments(IDocument document, IMessageInstructions messageInstructions)
		{
			yield break;
		}

		public bool ShouldUseDraftWatermark(IDocument document) => false;
	}
}
