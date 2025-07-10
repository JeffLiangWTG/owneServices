using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ShipmentProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		public Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			object shipment = parentID.IsValid ?
				GetShipmentFromFactoryCache(parentTablePrefix, parentID, factory) ?? GetShipmentFromDB(parentTablePrefix, parentID, factory)
				:
				null;
			return MapShipmentToProcessTaskType(shipment);
		}

		public void AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
			Type workflowProviderType = workflowDescriptor.WorkflowProviderType;
			if (typeof(Integration.Agency.IBillOfLading).IsAssignableFrom(workflowProviderType))
			{
				subQuery.AddToFilter(JobShipmentSchema.JS_IsShipping, true);
				subQuery.AddToFilter(JobShipmentSchema.JS_ShipmentStatus, new string[] { "CNF", "WFI" });
			}
			else if (typeof(Integration.Agency.IAgencyBooking).IsAssignableFrom(workflowProviderType))
			{
				subQuery.AddToFilter(JobShipmentSchema.JS_IsShipping, true);
				subQuery.AddToFilter(JobShipmentSchema.JS_ShipmentStatus, SQLComparisonOperator.NotEqual, new string[] { "CNF", "WFI" });
			}
			else if (typeof(Integration.CFS.ICFSShipment).IsAssignableFrom(workflowProviderType))
			{
				subQuery.AddToFilter(JobShipmentSchema.JS_IsCFSRegistered, true);
				subQuery.AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, false);
			}
			else
			{
				subQuery.AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, true);
			}
		}

		object GetShipmentFromFactoryCache(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			var shipments = factory.GetBizOsForPK(parentID.ToGuid());

			foreach (var shipment in shipments)
			{
				if (shipment is Integration.Agency.IBillOfLading ||
					shipment is Integration.Agency.IAgencyBooking ||
					(shipment is Enterprise.Integration.Forwarding.IForwardingShipment
						&& ((ZBool)shipment[JobShipmentSchema.JS_IsForwardRegistered] || shipments.Length == 1)))
				{
					return shipment;
				}

				if (shipment is Integration.CFS.ICFSShipment)
				{
					if (!(ZBool)shipment[JobShipmentSchema.JS_IsForwardRegistered])
					{
						return shipment;
					}
					return ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>();
				}
			}

			return null;
		}

		object GetShipmentFromDB(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			ZQuery baseFilter = new ZQuery(JobShipmentSchema.PK, parentID);
			baseFilter.IgnoreActiveFilter = true;

			ZQuery agencyBillOfLadingFilter = new ZQuery(JobShipmentSchema.JS_IsShipping, true);
			agencyBillOfLadingFilter.AddToFilter(JobShipmentSchema.JS_ShipmentStatus, new string[] { "CNF", "WFI" });
			object shipment = factory.LoadTop1<Integration.Agency.IBillOfLading>(new ZQuery(baseFilter, agencyBillOfLadingFilter));

			if (shipment == null)
			{
				ZQuery agencyBookingFilter = new ZQuery(JobShipmentSchema.JS_IsShipping, true);
				agencyBookingFilter.AddToFilter(JobShipmentSchema.JS_ShipmentStatus, SQLComparisonOperator.NotEqual, new string[] { "CNF", "WFI" });
				shipment = factory.LoadTop1<Integration.Agency.IAgencyBooking>(new ZQuery(baseFilter, agencyBookingFilter));
			}

			if (shipment == null)
			{
				ZQuery cfsShipmentFilter = new ZQuery(JobShipmentSchema.JS_IsCFSRegistered, true);
				cfsShipmentFilter.AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, false);
				shipment = factory.LoadTop1<Integration.CFS.ICFSShipment>(new ZQuery(baseFilter, cfsShipmentFilter));
			}

			return shipment;
		}

		Type MapShipmentToProcessTaskType(object shipment)
		{
			if (shipment == null || shipment is Enterprise.Integration.Forwarding.IForwardingShipment)
			{
				return ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipmentProcessTask>();
			}

			if (shipment is Integration.Agency.IBillOfLading)
			{
				return ObjectFactory.GetType<Integration.Agency.IBillOfLadingProcessTask>();
			}

			if (shipment is Integration.Agency.IAgencyBooking)
			{
				return ObjectFactory.GetType<Integration.Agency.IAgencyBookingProcessTask>();
			}

			if (shipment is Integration.CFS.ICFSShipment)
			{
				return ObjectFactory.GetType<Integration.CFS.ICFSShipmentProcessTask>();
			}

			return ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipmentProcessTask>();
		}
	}
}
