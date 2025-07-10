using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.OceanCarrier.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.OceanCarrier.DataTransfer.Universal
{
	public sealed class CarrierShipmentContainerLinkManager
	{
		public int GetContainerLink(CarrierShipmentCargo container)
		{
			int link;
			if (!containerToLinkMap.TryGetValue(container.PK, out link))
			{
				link = lastLink++;
				containerToLinkMap.Add(container.PK, link);
			}

			return link;
		}

		public void CollectContainerLink(CarrierShipmentCargo container, Container containerDataObject)
		{
			if (containerDataObject.Link.HasValue && !linkToContainerMap.ContainsKey(containerDataObject.Link.Value))
			{
				linkToContainerMap.Add(containerDataObject.Link.Value, container);
			}
		}

		public CarrierShipmentCargo GetContainer(int link)
		{
			if (linkToContainerMap.TryGetValue(link, out var container))
			{
				return container;
			}

			return null;
		}

		readonly Dictionary<ZGuid, int> containerToLinkMap = new Dictionary<ZGuid, int>();
		int lastLink = 1;
		readonly Dictionary<int, CarrierShipmentCargo> linkToContainerMap = new();
	}
}
