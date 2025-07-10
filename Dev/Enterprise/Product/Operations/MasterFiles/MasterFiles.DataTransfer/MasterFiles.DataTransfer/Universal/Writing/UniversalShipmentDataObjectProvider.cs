using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	sealed class UniversalShipmentDataObjectProvider : IUniversalShipmentDataObjectProvider
	{
		public IDataObject GetDataObject(BusinessObject businessObject, IDataWritingInformationCollector informationCollector)
		{
			var workflowProvider = businessObject as IWorkflowProvider;

			if (workflowProvider == null)
			{
				return null;
			}

			var workflowDescriptior = WorkflowDescriptors.Instance.TryGetValueSafe(workflowProvider.WorkflowType);

			if (workflowDescriptior == null)
			{
				return null;
			}

			var actionInfo = new ActionInfo(RecipientRoleType.ORP, businessObject);
			var dataWritingManager = new DataWritingManager(actionInfo, informationCollector, UniversalXmlSchema.Version_2012_11_DO_NOT_USE);
			var dataObjectWriter = workflowDescriptior.GetUniversalShipmentDataObjectWriter(dataWritingManager);

			IExternalFetchHintSupporter externalFetchHintSupporter = businessObject.Factory;
			using (externalFetchHintSupporter.SetupCreator())
			{
				return dataObjectWriter != null
				? dataObjectWriter.GetDataObject(businessObject)
				: null;
			}
		}
	}
}
