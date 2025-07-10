using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ShipmentTypeDecider : TypeDecider
	{
		public override sealed Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			if ((bool)row[JobShipmentSchema.JS_IsForwardRegistered.Name] && (bool)row[JobShipmentSchema.JS_IsCFSRegistered.Name])
			{
				BusinessObject[] shipmentsInFactoryCache = factory.GetBizOsForPK((Guid)row[JobShipmentSchema.PK.Name]);
				if (shipmentsInFactoryCache != null && shipmentsInFactoryCache.Length == 1)
				{
					var shipmentInCache = shipmentsInFactoryCache[0];
					if (shipmentInCache is CFS.ICFSShipment ||
						shipmentInCache is Enterprise.Integration.Forwarding.IForwardingShipment)
					{
						return shipmentInCache.GetType();
					}

					//We have to do it because stupid GenericJob uses shipment's Pk
					return null;
				}
				else
				{
					return factory.GetFreightDomainContext() == FreightDomainContext.CFS
						? ObjectFactory.GetType<CFS.ICFSShipment>()
						: ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>();
				}
			}

			if ((bool)row[JobShipmentSchema.JS_IsForwardRegistered.Name] || (bool)row[JobShipmentSchema.JS_IsBooking.Name])
			{
				return ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>();
			}

			if ((bool)row[JobShipmentSchema.JS_IsCFSRegistered.Name])
			{
				return ObjectFactory.GetType<CFS.ICFSShipment>();
			}

			if ((bool)row[JobShipmentSchema.JS_IsShipping.Name])
			{
				var shipmentStatus = new ZString(row[JobShipmentSchema.JS_ShipmentStatus.Name]);
				if (shipmentStatus == ShipmentStatusList.Codes.Confirmed || shipmentStatus == ShipmentStatusList.Codes.WebFwdInstruction || shipmentStatus == ShipmentStatusList.Codes.SIRejected)
				{
					return ObjectFactory.GetType<Agency.IBillOfLading>();
				}
				else
				{
					return ObjectFactory.GetType<Agency.IAgencyBooking>();
				}
			}

			return GetTypeForNew();
		}

		public override Type GetTypeForNew()
		{
			return ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonShipment>();
		}

		public override Type GetTypeForBinding()
		{
			return ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonShipment>();
		}
	}
}
