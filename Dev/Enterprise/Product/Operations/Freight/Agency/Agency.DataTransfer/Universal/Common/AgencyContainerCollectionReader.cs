using System.Linq;
using CargoWise.Common;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class AgencyContainerCollectionReader<T> : ContainerCollectionReader<T> where T : AgencyShipmentContainer
	{
		public AgencyContainerCollectionReader(DataObjectList<Container> containers, IXmlImportLogger logger, UniversalObjectFactory factory, LinksManager links, AgencyContainersInfo containersInfo)
			: base(containers, logger, factory, containersInfo.CollectionToImportTo)
		{
			this.links = links;
			this.containersInfo = Argument.NotNull(containersInfo, nameof(containersInfo));
		}

		readonly LinksManager links;
		readonly AgencyContainersInfo containersInfo;

		#region Implementation

		protected override T ReadIntoBusinessObject(Container dataObject, T container)
		{
			var reader = new AgencyContainerReader(dataObject, container, containersInfo, logger, factory);
			var result = reader.ReadIntoBusinessObject() as T;

			if (links != null)
			{
				links.AddLink(LinksManager.LinkType.Container, dataObject.Link, result);
			}

			return result;
		}

		protected override T FindMatchingBusinessObject(Container dataObject)
		{
			if (containersInfo.IsVGM)
			{
				var containerFinder = new ContainerBusinessObjectFinder<T>(dataObject);
				var shipment = containersInfo.Shipment;
				return containerFinder.Find(shipment.RealContainers.Cast<T>()) ?? containerFinder.Find(shipment.BookedContainers.Cast<T>());
			}

			return base.FindMatchingBusinessObject(dataObject);
		}

		#endregion
	}
}


