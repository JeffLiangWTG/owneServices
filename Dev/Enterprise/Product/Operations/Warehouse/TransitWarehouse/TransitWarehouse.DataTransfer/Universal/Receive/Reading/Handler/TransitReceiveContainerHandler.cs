using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.WhsTransitPackageStateBusinessObjectFinderForASN;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class TransitReceiveContainerHandler : ITransitDataObjectReaderHandler
	{
		public TransitReceiveContainerHandler(UniversalShipment dataObject)
		{
			this.dataObject = dataObject;
		}

		readonly UniversalShipment dataObject;

		TransitDataObjectReaderHandlerManager HandlerManager
		{
			get
			{
				handlerManager ??= ObjectFactory.Get<TransitDataObjectReaderHandlerManager>();
				return handlerManager;
			}
		}
		TransitDataObjectReaderHandlerManager handlerManager;

		public void Execute(UniversalObjectFactory factory, UniversalShipment dataObject, IXmlImportLogger logger)
		{
			HandleContainer(factory, logger);
		}

		void HandleContainer(UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			var consolDO = TransitUniversalExtensions.GetConsolDataObject(logger, factory);
			var isFromSeaCargo = consolDO?.GetMatchingDataSource(DataContextType.SeaCargoOutturn) != null;
			var bookingParty = TransitUniversalHelper.GetBookingParty(factory, logger, isRequired: !isFromSeaCargo);

			var isFromForwardingToATW = consolDO?.GetMatchingDataSource(DataContextType.ForwardingConsol) != null && dataObject.IsArrivalTransitWarehouse();
			var isFromForwardingToATWAndSameBranch = WarehouseDataRegistry.Instance.CreateSingleASNForAllContainers.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty) && isFromForwardingToATW;

			var packagesByContainerLink = HandlerManager.GetPackagesByContainerLink();
			if (isFromForwardingToATWAndSameBranch)
			{
				var packagePKs = packagesByContainerLink.SelectMany(p => p.Value).Select(p => p.PK);
				var finder = new WhsTransitPackageStateBusinessObjectFinderForASN(FinderType.Packages, factory, packagePKs: packagePKs);

				new WhsTransitReceiveASNDataObjectReader(dataObject, consolDO?.ContainerCollection, null, bookingParty, HandlerManager.GetWarehouse(), logger, factory, finder: finder).ReadIntoBusinessObject();
			}
			else
			{
				var asnPKsForEmptyContainer = new HashSet<ZGuid>();
				if (isFromForwardingToATW && consolDO?.ContainerCollection != null)
				{
					var containerLinksWithPackline = packagesByContainerLink.Select(p => p.Key).ToList();
					foreach (var container in consolDO?.ContainerCollection)
					{
						if (container?.Link != null && !containerLinksWithPackline.Contains(container.Link.Value))
						{
							var reader = new WhsTransitReceiveASNDataObjectReader(dataObject, new Container[] { container }, null, bookingParty, HandlerManager.GetWarehouse(), logger, factory);
							var asn = reader.ReadIntoBusinessObject();
							asnPKsForEmptyContainer.Add(asn.PK);
						}
					}
				}

				foreach (var packagesByContainerPair in packagesByContainerLink.OrderBy(p => p.Key))
				{
					var containerLink = packagesByContainerPair.Key;
					var packages = packagesByContainerPair.Value;
					var packagePKs = packages.Select(p => p.PK);
					var finder = new WhsTransitPackageStateBusinessObjectFinderForASN(FinderType.Packages, factory, packagePKs: packagePKs);
					var packageStates = finder.Find().ToArray();

					if (packageStates.Length == 0)
					{
						continue;
					}

					var container = GetContainerFromLink(consolDO, containerLink);

					foreach (var packageState in packageStates)
					{
						var previousASNPK = packageState.WPS_WRP_ReceiveExpectedPacking;
						if (!previousASNPK.IsEmpty && !asnPKsForEmptyContainer.Contains(previousASNPK))
						{
							HandlerManager.AddAffectedASNPK(previousASNPK);
						}
					}

					new WhsTransitReceiveASNDataObjectReader(dataObject, new Container[] { container }, null, bookingParty, HandlerManager.GetWarehouse(), logger, factory, finder: finder).ReadIntoBusinessObject();
				}
			}
		}

		Container GetContainerFromLink(UniversalShipment consolDO, ZInt containerLink)
		{
			Container container = null;
			if (containerLink >= 0)
			{
				if (consolDO != null && consolDO.ContainerCollection != null)
				{
					container = consolDO.ContainerCollection.SingleOrDefault(c => c.Link == containerLink);
				}
			}

			return container;
		}
	}
}
