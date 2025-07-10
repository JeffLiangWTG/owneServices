using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class JobSupplierBookingAllocatedContainerDataObjectCollectionReader : DataObjectCollectionReader<UniversalShipment, ForwardingContainer>
	{
		readonly JobSupplierBooking supplierBooking;
		readonly IXmlImportLogger logger;
		readonly ForwardingContainer[] attachedContainers;
		readonly List<ForwardingContainer> attachedContainersCache;

		internal JobSupplierBookingAllocatedContainerDataObjectCollectionReader(JobSupplierBooking supplierBooking, DataObjectList<UniversalShipment> dataObjects, IXmlImportLogger logger)
			: base(dataObjects)
		{
			this.supplierBooking = supplierBooking;
			this.logger = logger;
			attachedContainers = supplierBooking.Containers.OfType<ForwardingContainer>().ToArray();
			attachedContainersCache = attachedContainers.ToList();
		}

		protected override ForwardingContainer[] BusinessObjects => attachedContainers;

		protected override void AddToCollection(ForwardingContainer businessObject)
		{
			this.logger.Log(Enterprise.Integration.LogType.Information, Res.GetString("837216e8-571a-4273-84e5-17ed4edf46d0", "Successfully allocated  {0} container {1} to this supplier booking.", businessObject.Consol?.JK_UniqueConsignRef, GetContainerDesc(businessObject)));
			supplierBooking.Containers.Add(businessObject);
		}

		protected override ForwardingContainer FindMatchingBusinessObject(UniversalShipment dataObject)
		{
			if (CheckAndGetContainerObject(dataObject, true, out var consolKey, out var containerObject))
			{
				var matchedContainer = attachedContainersCache
					.Where(container => (container.Consol?.JK_UniqueConsignRef ?? ZString.Empty) == consolKey && container.JC_ContainerCount == containerObject.ContainerCount && ConsolContainerMatcherForContainerLoadListLine.MatchContainer(container, containerObject))
					.OrderByDescending(container => container.JC_ContainerNum)
					.FirstOrDefault();

				if (matchedContainer != null)
				{
					attachedContainersCache.Remove(matchedContainer);
				}

				return matchedContainer;
			}

			return null;
		}

		bool CheckAndGetContainerObject(UniversalShipment dataObject, bool enabledLog, out string consolKey, out UniversalContainer containerObject)
		{
			consolKey = dataObject.GetMatchingDataSource(DataContextType.ForwardingConsol)?.Key.GetValueOrDefault() ?? ZString.Empty;
			containerObject = null;
			if (string.IsNullOrWhiteSpace(consolKey))
			{
				if (enabledLog)
				{
					this.logger.Log(Enterprise.Integration.LogType.Warning, Res.GetString("2f08fbe2-654a-47f0-ba68-4637b26e66b0", "Consol Key is required."));
				}

				return false;
			}

			if (dataObject.ContainerCollection?.Count != 1)
			{
				if (enabledLog)
				{
					this.logger.Log(Enterprise.Integration.LogType.Warning, Res.GetString("fa5e44e5-f4a8-4af1-bb0e-5a70e9a1edf8", "Only one container should be provided {0}.", consolKey));
				}

				return false;
			}

			containerObject = dataObject.ContainerCollection.Single();
			return containerObject != null;
		}

		protected override ForwardingContainer ReadIntoBusinessObject(UniversalShipment dataObject, ForwardingContainer businessObject)
		{
			if (businessObject == null)
			{
				if (CheckAndGetContainerObject(dataObject, false, out var consolKey,  out var containerObject))
				{
					var consol = supplierBooking.Factory.LoadFromNaturalKey<ForwardingConsol>(JobConsolSchema.JK_UniqueConsignRef, consolKey);
					if (consol == null)
					{
						this.logger.Log(Enterprise.Integration.LogType.Warning, Res.GetString("27c673cc-1406-41f0-978c-d72b826d72bf", "Could not match the Consol {0}.", consolKey));
						return null;
					}

					var matchedContainer = (from container in consol.Containers.OfType<ForwardingContainer>()
											where container.JC_ContainerCount == containerObject.ContainerCount && ConsolContainerMatcherForContainerLoadListLine.MatchContainer(container, containerObject)
											orderby container.JC_ContainerNum descending, container.JC_JSB_SupplierBooking, container.JC_CLH_LoadListPlan
											select container).FirstOrDefault();

					if (matchedContainer == null)
					{
						this.logger.Log(Enterprise.Integration.LogType.Warning, Res.GetString("597f3012-8f15-49ed-b970-57ca5183438a", "Could not match the Container {0} {1}.", consolKey, GetContainerDesc(containerObject)));
					}
					else if ((matchedContainer.SupplierBooking?.JSB_Status ?? Core.Constants.SupplierBookingStatus.Cancelled) != Core.Constants.SupplierBookingStatus.Cancelled
						|| (matchedContainer.ContainerLoadPlan?.CLH_Status ?? Core.Constants.ContainerLoadListHeaderStatus.Cancelled) != Core.Constants.ContainerLoadListHeaderStatus.Cancelled)
					{
						this.logger.Log(Enterprise.Integration.LogType.Warning, Res.GetString("9e4c6328-2c67-4873-8ad3-a2b75cb07347", "{0} container {1} is already allocated to another supplier booking or a container load plan. Element is skipped.", consolKey, GetContainerDesc(containerObject)));
						return null;
					}

					return matchedContainer;
				}
			}

			return businessObject;
		}

		protected override void RemoveFromCollection(ForwardingContainer businessObject)
		{
			supplierBooking.Containers.Remove(businessObject);
		}

		string GetContainerDesc(UniversalContainer container) => string.IsNullOrEmpty(container.ContainerNumber) ? (container.ContainerType?.Code + " " + container.ContainerCount) : container.ContainerNumber;
		string GetContainerDesc(ForwardingContainer container) => string.IsNullOrEmpty(container.JC_ContainerNum) ? (container.Container?.RC_Code + " " + container.JC_ContainerCount) : container.JC_ContainerNum;
	}
}
