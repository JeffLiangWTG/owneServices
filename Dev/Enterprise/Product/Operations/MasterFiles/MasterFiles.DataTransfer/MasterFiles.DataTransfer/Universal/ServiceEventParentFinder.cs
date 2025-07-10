using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class ServiceEventParentFinder : EventParentFinder
	{
		internal ServiceEventParentFinder(BusinessObjectFactory factory, ServiceDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent eventDataObject)
		{
			var eventValueObject = (IXmlEventValueObject)eventDataObject;
			if (eventValueObject?.Context == null)
			{
				return null;
			}

			var serviceId = eventValueObject.Context.ServiceId.GetValueOrDefault();
			if (serviceId.IsEmpty)
			{
				return null;
			}

			var filter = new ZQuery(JobServiceSchema.ES_ServiceId, SQLComparisonOperator.NotEqual, ZString.Empty);
			filter.AddToFilter(JobServiceSchema.ES_ExternalServiceId, serviceId);
			var service = factory.LoadTop1<JobService>(filter);
			if (service == null)
			{
				var shipmentNumber = eventValueObject.Context.ShipmentNumber.GetValueOrDefault();
				if (!shipmentNumber.IsEmpty)
				{
					var shipment = factory.LoadFromUniqueKey<Forwarding.IForwardingShipment>(JobShipmentSchema.JS_UniqueConsignRef, shipmentNumber);
					service = (shipment?.DocsAndCartage as IHaveServices)?.Services?.AddNew();
				}
			}

			return service == null ? null : new BusinessObject[] { service };
		}
	}
}
