using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ContainerLinkManager<T> : IContainerLinkManager<T> where T : CommonConsol
	{
		public ContainerLinkManager(T consol)
		{
			Consol = consol;
		}

		public T Consol { get; private set; }

		public void CollectContainerLink(CommonContainer container, Container containerDataObject)
		{
			if (containerDataObject.Link.HasValue && !linkToContainerMap.ContainsKey(containerDataObject.Link.Value))
			{
				linkToContainerMap.Add(containerDataObject.Link.Value, container);
			}
		}

		public void AllocateContainerLink(CommonContainer container, Container containerDataObject)
		{
			var link = GetContainerLink(container);
			containerDataObject.Link = link;
		}

		public void PackIntoContainer(PackLine packLine, PackingLine packingLine)
		{
			if (Consol != null && packingLine.ContainerLink.HasValue && linkToContainerMap.ContainsKey(packingLine.ContainerLink.Value))
			{
				var container = linkToContainerMap[packingLine.ContainerLink.Value];

				using (container.SuspendContainerPenalties())
				{
					packLine.SetContainer(Consol, container);
				}
			}
		}

		public void SetContainerLink(CommonContainer container, PackingLine packingLine)
		{
			var link = GetContainerLink(container);
			packingLine.ContainerLink = link;
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

		readonly Dictionary<int, CommonContainer> linkToContainerMap = new Dictionary<int, CommonContainer>();
		readonly Dictionary<ZGuid, int> containerToLinkMap = new Dictionary<ZGuid, int>();
		int lastLink = 1;
	}
}
