using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class AgencyContainerBusinessObjectFinderFromPackingLine<T> : MatchingBusinessObjectFinder<PackingLine, T> where T : AgencyShipmentContainer
	{
		public AgencyContainerBusinessObjectFinderFromPackingLine(PackingLine dataObject, LinksManager links)
			: base(dataObject)
		{
			this.links = links;
		}

		readonly LinksManager links;

		protected override T FindCore(IEnumerable<T> businessObjects)
		{
			T result = null;
			if (links != null && links.ContainsLink(LinksManager.LinkType.Container, dataObject.ContainerLink))
			{
				ZGuid containerPK = links[LinksManager.LinkType.Container, dataObject.ContainerLink];
				result = businessObjects.FirstOrDefault(container => container.PK == containerPK);
			}

			if (result == null)
			{
				var containerNumber = dataObject.ReferenceNumber.GetValueOrDefault();
				if (!containerNumber.IsEmpty)
				{
					result = businessObjects.FirstOrDefault(container => container.JC_ContainerNum.ToUpper() == containerNumber.ToUpper());
				}
			}

			return result;
		}
	}
}
