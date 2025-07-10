using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Freight.Business
{
	public class ContainerTypeDecider : TypeDecider
	{
		public override sealed Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;

			BusinessObject[] containersInFactoryCache = factory.GetBizOsForPK((Guid)row[JobContainerSchema.PK.Name]);
			if (containersInFactoryCache != null && containersInFactoryCache.Length == 1)
			{
				result = containersInFactoryCache[0].GetType();
			}
			else
			{
				var consol = factory.Load(JobConsolSchema.Constants.Prefix, new ZGuid(row[JobContainerSchema.JC_JK.Name]));
				if (consol != null)
				{
					if (consol is Enterprise.Integration.Forwarding.IForwardingConsol)
					{
						result = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingContainer>();
					}
					else if (consol is Integration.CFS.ICFSLoadListConsol)
					{
						result = ObjectFactory.GetType<Integration.CFS.ICFSContainer>();
					}
				}
				else
				{
					var shipment = factory.Load(JobShipmentSchema.Constants.Prefix, new ZGuid(row[JobContainerSchema.JC_JS_FCLBookingOnlyLink.Name]));
					if (shipment != null)
					{
						if (shipment is Integration.Agency.IAgencyBooking)
						{
							result = ObjectFactory.GetType<Integration.Agency.IAgencyBookingContainer>();
						}
						else if (shipment is Integration.Agency.IBillOfLading)
						{
							result = ObjectFactory.GetType<Integration.Agency.IBillOfLadingContainer>();
						}
						else if (shipment is Enterprise.Integration.Forwarding.IForwardingShipment)
						{
							result = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingContainer>();
						}
					}
					else
					{
						var query = new ZQuery(CusContainerSchema.CO_JC, new ZGuid(row[JobContainerSchema.PK.Name]));
						var cusContainers = factory.Load<IBaseCusContainer>(query);
						if (cusContainers.Any())
						{
							var declarationPKs = new List<ZGuid>();
							foreach (var cusContainer in cusContainers)
							{
								declarationPKs.Add(cusContainer.CO_JE);
							}

							query = new ZQuery(JobDeclarationSchema.PK, declarationPKs);
							var declarations = factory.Load<IBaseJobDeclaration>(query);
							if (declarations.Any())
							{
								result = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingContainer>();
							}
						}
					}
				}
			}

			return result ?? ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonContainer>();
		}

		public override Type GetTypeForNew()
		{
			return ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonContainer>();
		}

		public override Type GetTypeForBinding()
		{
			return ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonContainer>();
		}
	}
}
