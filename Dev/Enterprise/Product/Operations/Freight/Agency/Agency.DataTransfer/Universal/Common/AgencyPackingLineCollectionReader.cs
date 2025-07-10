using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class AgencyPackingLineCollectionReader<T, U> : PackingLineCollectionReader<T, U>
		where T : AgencyShipmentPackLine
		where U : AgencyShipment
	{
		public AgencyPackingLineCollectionReader(PackingLine[] packingLines, IXmlImportLogger logger, UniversalObjectFactory factory, U parentShipment, IBusinessObjectCollection packLinesCollection, IEnumerable<AgencyShipmentContainer> containersCollection, LinksManager links)
			: base(packingLines, logger, factory, parentShipment, packLinesCollection, containersCollection)
		{
			this.links = links;
		}

		readonly LinksManager links;

		#region Implementation

		protected override T ReadIntoBusinessObject(PackingLine dataObject, T packLine)
		{
			packLine = base.ReadIntoBusinessObject(dataObject, packLine);
			if (links != null && links.ContainsLink(LinksManager.LinkType.Container, dataObject.ContainerLink))
			{
				packLine.SetContainer(links[LinksManager.LinkType.Container, dataObject.ContainerLink]);
			}

			return packLine;
		}

		#endregion
	}
}


