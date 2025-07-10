using System.Collections.Generic;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn
{
	public class CusContainerLinkManager
	{
		public Dictionary<string, int> ContainerNumberAndLink
		{
			get
			{
				if (containerNumberAndLink == null)
				{
					containerNumberAndLink = new Dictionary<string, int>();
				}
				return containerNumberAndLink;
			}
		}
		Dictionary<string, int> containerNumberAndLink;
	}
}
