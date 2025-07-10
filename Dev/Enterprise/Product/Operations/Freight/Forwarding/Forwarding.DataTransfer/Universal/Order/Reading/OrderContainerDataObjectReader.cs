using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class OrderContainerDataObjectReader<T> : BaseContainerDataObjectReader<T>
		where T : AutoJobOrderContainer
	{
		public OrderContainerDataObjectReader(Container containerDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(containerDataObject, logger, factory)
		{
		}

		#region Match Existing

		protected override T GetExistingBusinessObject()
		{
			return null;
		}

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(T plannedContainer)
		{
			SetValue(plannedContainer, JobOrderContainerSchema.J1_Additional2SealNum, dataObject.ThirdSeal);
			SetValue(plannedContainer, JobOrderContainerSchema.J1_AdditionalSealNum, dataObject.SecondSeal);
			SetValue(plannedContainer, JobOrderContainerSchema.J1_ContainerNumber, dataObject.ContainerNumber);
			SetValue(plannedContainer, JobOrderContainerSchema.J1_ContainerCount, dataObject.ContainerCount);
			SetValue(plannedContainer, JobOrderContainerSchema.J1_SealNum, dataObject.Seal);
			SetValue(plannedContainer, JobOrderContainerSchema.J1_RC, dataObject.ContainerType);
		}

		#endregion
	}
}

