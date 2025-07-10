using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class UniversalExceptionReader : IUniversalExceptionReader
	{
		public void PopulateExceptions(IDataObject dataObject, BusinessObject parent, IXmlImportLogger logger, IUniversalObjectFactory factory)
		{
			if (dataObject is IExceptionCollectionParent dataObjectWithExceptionCollection && parent is IWorkflowProvider parentWithWorkflow)
			{
				if (dataObjectWithExceptionCollection.ExceptionCollection != null && dataObjectWithExceptionCollection.ExceptionCollection.Count > 0)
				{
					var workflowExceptionCollectionReader = new WorkflowExceptionCollectionReader(dataObjectWithExceptionCollection.ExceptionCollection.ToArray(), logger, (UniversalObjectFactory)factory, parentWithWorkflow);
					workflowExceptionCollectionReader.ReadIntoCollection();
				}
			}
		}
	}
}
