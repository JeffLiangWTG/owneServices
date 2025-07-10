using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Agency.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class AgencyShipmentReadStrategy<TShipment, TContainer, TPackLine> : IAgencyShipmentReadStrategy<TShipment>
		where TShipment : AgencyShipment
		where TContainer : AgencyShipmentContainer
		where TPackLine : AgencyShipmentPackLine
	{
		public AgencyShipmentReadStrategy(IXmlImportLogger logger, UniversalObjectFactory factory, Shipment dataObject)
		{
			this.logger = logger;
			this.factory = factory;
			this.dataObject = dataObject;
		}

		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;
		readonly LinksManager links = new LinksManager();
		readonly Shipment dataObject;

		void IAgencyShipmentReadStrategy<TShipment>.ReadContainers(TShipment agencyShipment, DataObjectList<Container> containers)
		{
			var isVerifiedGrossContainerWeight = dataObject.ContainsServiceCode(ServiceCodeType.VGM);
			var containerInfo = new AgencyContainersInfo(agencyShipment, isVerifiedGrossContainerWeight);

			var reader = new AgencyContainerCollectionReader<AgencyShipmentContainer>(containers, logger, factory, links, containerInfo);
			reader.ReadIntoCollection();
		}

		void IAgencyShipmentReadStrategy<TShipment>.ReadPackingLines(TShipment agencyShipment, IEnumerable<PackingLine> packLines)
		{
			var containersCollection = agencyShipment.ShippingContainers;
			if (agencyShipment.IsTopLevelPacksMode)
			{
				var reader = new TopLevelPackPackingLineCollectionReader<TContainer>(packLines.ToArray(), logger, factory, containersCollection, links);
				reader.ReadIntoCollection();
			}
			else
			{
				var reader = new AgencyPackingLineCollectionReader<TPackLine, TShipment>(packLines.ToArray(), logger, factory, agencyShipment, agencyShipment.OuterPackLines, containersCollection.Cast<TContainer>(), links);
				reader.ReadIntoCollection();
			}
		}
	}
}
