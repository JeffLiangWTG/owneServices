using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer
{
	[Serializable]
	public class AgencyShipmentEventLogSubscriber : ShipmentEventLogProcessor
	{
		protected override BusinessObject GetLogParent(IQueuedLog log)
		{
			var factory = new ReadOnlyBusinessObjectFactory();
			return factory.Load(JobShipmentSchema.Constants.Prefix, log.SJ_ParentID);
		}

		protected override BusinessObject GetLogParentForBusinessObject(BusinessObject logParent, IQueuedLog log)
		{
			return log.Factory.Load(JobShipmentSchema.Constants.Prefix, logParent.PK);
		}

		protected override bool ShouldProcess(BusinessObject logParent)
		{
			return logParent is AgencyShipment or BillOfLading;
		}

		protected override bool IsLCLDatesOverrideConsol(BusinessObject logParent)
		{
			return false;
		}

		protected override string GetPackingMode(BusinessObject logParent)
		{
			return (logParent as AgencyShipment)?.JS_PackingMode;
		}

		protected override IEnumerable<BusinessObject> GetContainerShipmentParent(CommonContainer container)
		{
			return container.GetParentShipments().ToList();
		}

		protected override IEnumerable<CommonContainer> GetContainers(BusinessObject logParent)
		{
			return (logParent as AgencyShipment)?.Containers;
		}

		protected override EventDataObjectWriter GetEventDataObjectWriter(IDataWritingManager dataWritingManager, BusinessObject logParent, string subscriptionReference, IDictionary<string, string> contextMappings)
		{
			var shipment = logParent as CommonShipment;
			return new ShipmentEventDataObjectWriter(dataWritingManager, shipment, contextMappings)
			{
				SubscriptionReference = subscriptionReference
			};
		}
	}
}
