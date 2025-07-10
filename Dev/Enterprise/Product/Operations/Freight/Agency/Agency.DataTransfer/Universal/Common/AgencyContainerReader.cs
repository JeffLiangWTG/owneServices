using CargoWise.Common;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class AgencyContainerReader : ContainerWithVGMDataObjectReader<AgencyShipmentContainer>
	{
		public AgencyContainerReader(Container containerDataObject,
				AgencyShipmentContainer containerBO,
				AgencyContainersInfo containerInfo,
				IXmlImportLogger logger,
				UniversalObjectFactory factory)
			: base(containerDataObject, logger, factory, null, null)
		{
			this.containersInfo = Argument.NotNull(containerInfo, nameof(containerInfo));
			this.containerBO = containerBO;
		}

		readonly AgencyShipmentContainer containerBO;
		readonly AgencyContainersInfo containersInfo;

		protected override AgencyShipmentContainer GetExistingBusinessObject()
		{
			return containerBO;
		}

		protected override AgencyShipmentContainer GetNewBusinessObject()
		{
			return containersInfo.CollectionToImportTo.AddNew();
		}
	}
}
