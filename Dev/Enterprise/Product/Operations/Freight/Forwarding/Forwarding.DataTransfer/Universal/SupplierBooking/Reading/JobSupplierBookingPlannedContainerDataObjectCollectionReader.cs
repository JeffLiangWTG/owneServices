using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class JobSupplierBookingPlannedContainerDataObjectCollectionReader : DataObjectCollectionReader<UniversalContainer, JobSupplierBookingPlannedContainer>
	{
		internal JobSupplierBookingPlannedContainerDataObjectCollectionReader(JobSupplierBooking supplierBooking, DataObjectList<UniversalContainer> containerDataObjects, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(containerDataObjects)
		{
			this.logger = logger;
			this.factory = factory;
			this.supplierBooking = supplierBooking;
			plannedContainers = supplierBooking.PlannedContainers.OfType<JobSupplierBookingPlannedContainer>().ToList();
			plannedContainersCache = this.plannedContainers.ToList();
		}

		readonly JobSupplierBooking supplierBooking;
		readonly IList<JobSupplierBookingPlannedContainer> plannedContainers;
		readonly IList<JobSupplierBookingPlannedContainer> plannedContainersCache;
		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;

		protected override void AddToCollection(JobSupplierBookingPlannedContainer container)
		{
			supplierBooking.PlannedContainers.Add(container);
		}

		protected override JobSupplierBookingPlannedContainer[] BusinessObjects => plannedContainers.OfType<JobSupplierBookingPlannedContainer>().ToArray();

		protected override JobSupplierBookingPlannedContainer FindMatchingBusinessObject(UniversalContainer containerDataObject)
		{
			var found = plannedContainersCache.FirstOrDefault(container => container.Container?.RC_Code == containerDataObject.ContainerType?.Code && container.J1_ContainerCount == containerDataObject.ContainerCount);
			if (found != null)
			{
				plannedContainersCache.Remove(found);
				return found;
			}

			return null;
		}

		protected override JobSupplierBookingPlannedContainer ReadIntoBusinessObject(UniversalContainer containerDataObject, JobSupplierBookingPlannedContainer orderContainer)
		{
			if (orderContainer != null)
			{
				return orderContainer;
			}
			else
			{
				var target = new OrderContainerDataObjectReader<JobSupplierBookingPlannedContainer>(containerDataObject, logger, factory).ReadIntoBusinessObject();
				return target;
			}
		}

		protected override void RemoveFromCollection(JobSupplierBookingPlannedContainer orderContainer)
		{
			supplierBooking.PlannedContainers.RemoveAndDelete(orderContainer);
		}
	}
}
