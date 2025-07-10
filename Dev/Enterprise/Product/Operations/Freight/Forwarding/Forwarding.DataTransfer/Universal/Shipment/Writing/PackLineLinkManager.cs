using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class PackLineLinkManager
	{
		public void AllocateLink(PackLine packline, PackingLine packlineDataObject)
		{
			var link = GetPacklineLink(packline);
			packlineDataObject.Link = link;
		}

		public int GetPacklineLink(PackLine packline)
		{
			int link;
			if (!packlineToLinkMap.TryGetValue(packline.PK, out link))
			{
				link = LastLink++;
				packlineToLinkMap.Add(packline.PK, link);
			}

			return link;
		}

		readonly Dictionary<ZGuid, int> packlineToLinkMap = new Dictionary<ZGuid, int>();
		public int LastLink { get; private set; } = 1;
	}
}
