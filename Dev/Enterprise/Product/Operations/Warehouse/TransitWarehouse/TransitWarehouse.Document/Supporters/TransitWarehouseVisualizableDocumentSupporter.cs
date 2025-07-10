using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Integration;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Document
{
	public abstract class TransitWarehouseVisualizableDocumentSupporter<T> : IVisualizableDocumentSupporter where T : BusinessObject
	{
		public const string FRPortsGoodsReceivedCRESA = "FRPortsGoodsReceivedCRESA";

		protected TransitWarehouseVisualizableDocumentSupporter(T parent)
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
		public abstract Either<string, object> GetAdditionalData(object obj, IStmMenuItem menuItem);
		public abstract IEnumerable<IMacroLibrary> GetLibraries(string dataContext);

		public virtual IEnumerable<ICommand> GetCustomCommands(string dataContext)
		{
			yield break;
		}

		#region GetUniversalXmlDataObject

		public Either<string, ITopLevelDataObject> GetUniversalXmlDataObject(IDataObjectWriterStrategy writerStrategy, IDocument document, MessageType messageType)
		{
			if (writerStrategy == null || document == null)
			{
				throw new NotSupportedException();
			}

			ITopLevelDataObject dataObject;

			if (document.DataContext == FRPortsGoodsReceivedCRESA)
			{
				dataObject = ObjectFactory.Get<IForwardingDocDataObjectUXmlWriter>().GetDataObject(writerStrategy, document, messageType);
			}
			else
			{
				dataObject = ObjectFactory.Get<ITransitDocDataObjectUXmlWriter>().GetDataObject(writerStrategy, document, messageType);
			}
			return dataObject != null
				? new Either<string, ITopLevelDataObject>(dataObject)
				: Res.GetString("932915a7-45ee-45bf-8ca6-e6c7ca9657a6", "Cannot produce Universal XML for {0}.", document.DataContext);
		}

		#endregion

		#region GetDocDataObject

		public Either<string, object> GetDocDataObject(object obj, string dataContext, IDocDataObjectParameters parameters)
		{
			if (obj == null || string.IsNullOrEmpty(dataContext))
			{
				throw new NotSupportedException();
			}

			var docDataObject = ObjectFactory.Get<ITransitDocDataObjectProvider>().GetDocDataObject(obj, dataContext, parameters);

			return docDataObject
				?? Res.GetString("ba415b2a-715c-407e-9e2e-0dd631b2c166", "DataContext '{0}' for Object Type {1} is not supported.", dataContext, obj.GetType());
		}

		#endregion

		protected ZString[] GetContextTypes(IStmMenuItem menuItem)
		{
			return menuItem
						?.Documents
						.OfType<IStmMenuTemplatePivot>()
						.Select(p => p.Template.SO_DataContext)
						.ToArray();
		}

		public string GetMessageBroker() => string.Empty;

		public IEnumerable<IDocument> GetAdditionalDocuments(IDocument document, IMessageInstructions messageInstructions)
		{
			yield break;
		}

		public bool ShouldUseDraftWatermark(IDocument document) => false;
	}
}
