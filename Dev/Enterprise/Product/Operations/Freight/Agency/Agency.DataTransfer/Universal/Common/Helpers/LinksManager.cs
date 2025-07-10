using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class LinksManager
	{
		public enum LinkType
		{
			Container = 1
		}

		readonly Dictionary<LinkType, Dictionary<ZInt, ZGuid>> links = new Dictionary<LinkType, Dictionary<ZInt, ZGuid>>();

		public void AddLink(LinkType linkType, ZInt? link, BusinessObject businessObject)
		{
			if (businessObject != null)
			{
				AddLink(linkType, link, businessObject.PK);
			}
		}

		public void AddLink(LinkType linkType, ZInt? link, ZGuid pk)
		{
			if (pk.IsValid && link.HasValue)
			{
				Dictionary<ZInt, ZGuid> typeLinks;
				if (!links.TryGetValue(linkType, out typeLinks))
				{
					typeLinks = new Dictionary<ZInt, ZGuid>();
					this.links[linkType] = typeLinks;
				}

				typeLinks[link.Value] = pk;
			}
		}

		public bool ContainsLink(LinkType linkType, ZInt? link)
		{
			bool result = false;
			if (link.HasValue)
			{
				Dictionary<ZInt, ZGuid> typeLinks;
				if (links.TryGetValue(linkType, out typeLinks))
				{
					result = typeLinks.ContainsKey(link.Value);
				}
			}

			return result;
		}

		public ZGuid this[LinkType linkType, ZInt? link]
		{
			get
			{
				if (!link.HasValue)
				{
					throw new KeyNotFoundException("Link is empty");
				}

				return links[linkType][link.Value];
			}
		}
	}
}
