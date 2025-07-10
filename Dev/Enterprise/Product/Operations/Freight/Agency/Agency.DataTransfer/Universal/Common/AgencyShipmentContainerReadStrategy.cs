using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	public class AgencyShipmentContainerReadStrategy : IAgencyShipmentReadStrategy
	{
		public AgencyShipmentContainerReadStrategy(AgencyShipmentContainer container, IXmlImportLogger logger, UniversalObjectFactory factory, bool isVerifiedGrossContainerWeight = false)
		{
			this.container = container;
			this.logger = logger;
			this.factory = factory;
			this.isVerifiedGrossContainerWeight = isVerifiedGrossContainerWeight;
		}

		readonly AgencyShipmentContainer container;
		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;
		readonly bool isVerifiedGrossContainerWeight;

		void IAgencyShipmentReadStrategy<AgencyShipment>.ReadContainers(AgencyShipment agencyShipment, DataObjectList<Container> containers)
		{
			var containerDataObject = containers.FirstOrDefault();

			if (containerDataObject != null)
			{
				var containerInfo = new AgencyContainersInfo(agencyShipment, isVerifiedGrossContainerWeight);
				var containerReader = new AgencyContainerReader(containerDataObject, container, containerInfo, logger, factory);
				containerReader.ReadIntoBusinessObject();
			}
		}

		void IAgencyShipmentReadStrategy<AgencyShipment>.ReadPackingLines(AgencyShipment agencyShipment, IEnumerable<PackingLine> packLines)
		{
			if (agencyShipment.IsTopLevelPacksMode)
			{
				ReadTopLevelContainerPacklingLines(packLines);
			}
			else
			{
				ReadOtherContainerPacklingLines(agencyShipment, packLines);
			}
		}

		void ReadTopLevelContainerPacklingLines(IEnumerable<PackingLine> packingLines)
		{
			var packingLine = packingLines.FirstOrDefault();

			if (packingLine != null)
			{
				var reader = new TopLevelPackPackingLineDataObjectReader<AgencyShipmentContainer>(packingLine, logger, factory, (packingLineDataObject) => container);
				reader.ReadIntoBusinessObject();
			}
		}

		void ReadOtherContainerPacklingLines(AgencyShipment agencyShipment, IEnumerable<PackingLine> packLines)
		{
			foreach (var existingContainerPackLine in container.PackLines.ToArray())
			{
				agencyShipment.OuterPackLines.RemoveAndDelete(existingContainerPackLine);
			}

			foreach (var packingLine in packLines)
			{
				var reader = new PackingLineDataObjectReader<AgencyShipmentPackLine, AgencyShipment>(
					packingLine, logger, factory, agencyShipment, line => null, agencyShipment.OuterPackLines.AddNew);

				var packLine = reader.ReadIntoBusinessObject();
				packLine.SetContainer(container.PK);
			}
		}
	}
}
