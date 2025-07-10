using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class PackLineTypeDecider : TypeDecider
	{
		public override sealed Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;

			BusinessObject[] packlinesInFactoryCache = factory.GetBizOsForPK((Guid)row[JobPackLinesSchema.PK.Name]);
			if (packlinesInFactoryCache != null && packlinesInFactoryCache.Length == 1)
			{
				result = packlinesInFactoryCache[0].GetType();
			}
			else
			{
				var shipment = factory.Load(JobShipmentSchema.Constants.Prefix, new ZGuid(row[JobPackLinesSchema.JL_JS.Name]));
				if (shipment != null)
				{
					if (shipment is Enterprise.Integration.Forwarding.IForwardingShipment)
					{
						result = ObjectFactory.GetType<Integration.Forwarding.IForwardingPackLine>();
					}
					else if (shipment is Integration.CFS.ICFSShipment)
					{
						result = ObjectFactory.GetType<Integration.CFS.ICFSPackLine>();
					}
					else if (shipment is Integration.Agency.IAgencyShipment)
					{
						result = ObjectFactory.GetType<Integration.Agency.IAgencyPackLine>();
					}
				}
			}

			return result ?? ObjectFactory.GetType<Integration.IPackLine>();
		}

		public override Type GetTypeForNew()
		{
			return ObjectFactory.GetType<Integration.IPackLine>();
		}

		public override Type GetTypeForBinding()
		{
			return ObjectFactory.GetType<Integration.IPackLine>();
		}
	}
}
