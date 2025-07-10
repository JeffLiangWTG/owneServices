using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer
{
	public sealed class DataWritingManager : IDataWritingManager
	{
		public DataWritingManager(IDataObjectWriterStrategy writerStrategy)
		{
			WriterStrategy = writerStrategy;
		}
		public bool PKAlreadyExported(ZGuid pk) => false;

		public void AddPK(ZGuid pk)
		{
		}

		public IDisposable UseNewListForDuplicatePKCheck() => null;

		public void NotifyExported(IDataObject dataObject, BusinessObject businessObject)
		{
		}

		public IUniversalActionInfo Action { get; }
		public IUniversalXmlSchema Schema => SchemaVersionManager.Current;
		public bool OverrideSendCostingData { get; set; }
		public bool ShouldPopulateInternalMilestones { get; set; }
		public DataContextType? FilteredDataContextType { get; set; }
		public bool IsPublishingInternally { get; }

		public IDataObjectWriterStrategy WriterStrategy { get; }

		public IEDIMessageContentFilterManager ContentFilterManager { get; }

		public IDisposable SetIsPublishingInternally() => null;
	}
}
