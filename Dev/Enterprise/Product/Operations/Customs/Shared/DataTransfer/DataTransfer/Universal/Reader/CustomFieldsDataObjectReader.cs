using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CustomFieldsDataObjectReader<T> : DataObjectWithWorkflowCustomFieldsReader<IDataObject>
		where T : IWorkflowProviderCore
	{
		public CustomFieldsDataObjectReader(IDataObject dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		public void PopulateCustomFields(T workflowProvider, (string, DataType?)[] usedCustomFields)
		{
			var customizedFieldsContainer = dataObject as ICustomizedFieldContainer;
			if (customizedFieldsContainer != null)
			{
				PopulateWorkflowCustomFields(workflowProvider, customizedFieldsContainer, usedCustomFields);
			}
		}
	}
}
