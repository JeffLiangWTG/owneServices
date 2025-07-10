using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;
using UniversalContainerMode = Enterprise.UniversalDataBuss.DataObjects.Universal.ContainerMode;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	internal class SupplierBookingAllocatedContainerDataObjectWriter : DataObjectWriter<ForwardingContainer, UniversalShipment>
	{
		public SupplierBookingAllocatedContainerDataObjectWriter(IDataWritingManager manager) : base(manager)
		{
		}

		protected override UniversalShipment PopulateDataObject(ForwardingContainer sourceBO)
		{
			var result = new UniversalShipment(writeManager.WriterStrategy);
			if (sourceBO != null)
			{
				var containerData = new UniversalContainer();
				containerData.ContainerNumber = sourceBO.JC_ContainerNum;
				containerData.FCL_LCL_AIR = ListHelper.GetWithDescription<UniversalContainerMode>(sourceBO.JC_ContainerMode, sourceBO.JC_ContainerMode_List);
				containerData.DeliveryMode = sourceBO.JC_DeliveryMode;
				containerData.ContainerType = ContainerType.New(sourceBO.RefContainer);
				containerData.ContainerCount = sourceBO.JC_ContainerCount;

				result.DataContext = DataContextFactory.New();
				result.DataContext.AddDataSource(DataContextType.ForwardingConsol, sourceBO.Consol.JK_UniqueConsignRef);
				result.SetContainerCollection(() => new DataObjectList<UniversalContainer> { containerData });
			}

			return result;
		}
	}
}
