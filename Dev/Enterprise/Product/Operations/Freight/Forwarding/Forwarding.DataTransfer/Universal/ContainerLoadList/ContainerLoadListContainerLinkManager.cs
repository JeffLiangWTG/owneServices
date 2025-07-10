using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ContainerLoadListContainerLinkManager
	{
		public void CollectContainerLink(CommonContainer container, Container containerDataObject)
		{
			if (containerDataObject.Link.HasValue && !linkToContainerMap.ContainsKey(containerDataObject.Link.Value))
			{
				linkToContainerMap.Add(containerDataObject.Link.Value, container);
			}
		}

		public CommonContainer GetContainer(int link)
		{
			if (linkToContainerMap.TryGetValue(link, out var container))
			{
				return container;
			}

			return null;
		}

		public int GetContainerLink(CommonContainer container)
		{
			int link;
			if (!containerToLinkMap.TryGetValue(container.PK, out link))
			{
				link = lastLink++;
				containerToLinkMap.Add(container.PK, link);
			}

			return link;
		}

		public bool HasCollected(CommonContainer container) => linkToContainerMap.Values.Any(c => c == container);

		readonly Dictionary<int, CommonContainer> linkToContainerMap = new Dictionary<int, CommonContainer>();
		readonly Dictionary<ZGuid, int> containerToLinkMap = new Dictionary<ZGuid, int>();
		int lastLink = 1;
	}
}
