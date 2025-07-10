using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class WorkflowExceptionCollectionReader : DataObjectCollectionReader<WorkflowException, ProcessTask>
	{
		public WorkflowExceptionCollectionReader(WorkflowException[] dataObjects, IXmlImportLogger logger, UniversalObjectFactory factory, IWorkflowProvider parent) : base(dataObjects)
		{
			Logger = logger;
			Factory = factory;
			Parent = parent;
		}

		protected UniversalObjectFactory Factory { get; }

		protected IXmlImportLogger Logger { get; }

		IWorkflowProvider Parent { get; }

		protected virtual ExceptionCollectionView Exceptions => Parent.WorkflowItems.Exceptions;

		protected override CollectionContent DefaultCollectionContent => CollectionContent.Partial;

		protected override ProcessTask[] BusinessObjects => Exceptions.Cast<ProcessTask>().ToArray();

		protected override void AddToCollection(ProcessTask businessObject) { }

		protected override ProcessTask FindMatchingBusinessObject(WorkflowException dataObject)
		{
			if (!dataObject.ExceptionID.HasValue)
			{
				return null;
			}

			return Parent.WorkflowItems.ExceptionsIncludingRelated.Cast<ProcessTask>().FirstOrDefault(t => t.P9_TaskID == dataObject.ExceptionID.Value);
		}

		protected override ProcessTask ReadIntoBusinessObject(WorkflowException dataObject, ProcessTask businessObject)
		{
			return new WorkflowExceptionReader(dataObject, Logger, Factory, businessObject, Exceptions).ReadIntoBusinessObject();
		}

		protected override void RemoveFromCollection(ProcessTask businessObject) { }
	}
}
