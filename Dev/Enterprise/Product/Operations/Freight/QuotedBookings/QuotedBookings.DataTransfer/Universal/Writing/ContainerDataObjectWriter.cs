using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	class ContainerDataObjectWriter : DataObjectWriter<ForwardingContainer, Container>
	{
		internal ContainerDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override Container PopulateDataObject(ForwardingContainer containerBO)
		{
			var containerData = new Container(writeManager.WriterStrategy);

			containerData.AirVentFlow = containerBO.JC_AirVentFlow;
			containerData.Commodity = ListHelper.GetWithDescription<Commodity>(containerBO.JC_RH_NKContainerCommodityCode, containerBO.ContainerCommodityCode_List);
			containerData.ContainerCount = containerBO.JC_ContainerCount;
			containerData.ContainerNumber = containerBO.JC_ContainerNum;
			containerData.ContainerType = ContainerType.New(containerBO.RefContainer);
			containerData.DeliveryMode = containerBO.JC_DeliveryMode;
			containerData.DepartureEstimatedPickup = containerBO.JC_DepartureEstimatedPickup;
			containerData.DepartureSlotDateTime = containerBO.JC_DepartureSlotDateTime;
			containerData.DepartureSlotReference = containerBO.JC_DepartureSlotReference;
			containerData.EmptyRequired = containerBO.JC_EmptyRequired;
			containerData.FCL_LCL_AIR = ListHelper.GetWithDescription<ContainerMode>(containerBO.JC_ContainerMode, containerBO.JC_ContainerMode_List);
			containerData.HumidityPercent = containerBO.JC_HumidityPercent;
			containerData.IsShipperOwned = containerBO.JC_IsShipperOwned;
			containerData.ReleaseNum = containerBO.JC_ReleaseNum;
			containerData.SetPointTemp = containerBO.JC_SetPointTemp;
			containerData.TempRecorderSerialNo = containerBO.JC_TempRecorderSerialNo;

			if (containerBO.ArrivalContainerYardAddress != null)
			{
				containerData.AddOrgAddress(writeManager, containerBO.ArrivalContainerYardAddress, nameof(DocAddressType.ContainerYardEmptyReturnAddress));
			}

			if (containerBO.DepartureContainerYardAddress != null)
			{
				containerData.AddOrgAddress(writeManager, containerBO.DepartureContainerYardAddress, nameof(DocAddressType.ContainerYardEmptyPickupAddress));
			}

			return containerData;
		}
	}
}
