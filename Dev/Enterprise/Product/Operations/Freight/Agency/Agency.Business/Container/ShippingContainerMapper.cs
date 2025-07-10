using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public static class ShippingContainerMapper
	{
		public static IDictionary<ZGuid, ZGuid[]> GetBookedRealContainerMap(AgencyShipment shipment)
		{
			List<ZGuid> mapped = new List<ZGuid>();
			Dictionary<ZGuid, ZGuid[]> realBookedContainerMap = new Dictionary<ZGuid, ZGuid[]>();

			foreach (AgencyShipmentContainer container in shipment.BookedContainers.Find(new ZQuery(JobContainerSchema.JC_ContainerNum, SQLComparisonOperator.NotEqual, "")))
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(JobContainerSchema.JC_ContainerNum, container.JC_ContainerNum);
				filter.AddToFilter(JobContainerSchema.PK, SQLComparisonOperator.NotEqual, mapped.ToArray());

				AgencyShipmentContainer[] matchingContainers = (AgencyShipmentContainer[])shipment.RealContainers.Find(filter);

				if (matchingContainers.Length > 0)
				{
					realBookedContainerMap.Add(container.PK, new ZGuid[] { matchingContainers[0].PK });
					mapped.Add(matchingContainers[0].PK);
				}
				else
				{
					realBookedContainerMap.Add(container.PK, Array.Empty<ZGuid>());
				}
			}

			foreach (AgencyShipmentContainer container in shipment.BookedContainers.Find(new ZQuery(JobContainerSchema.JC_ContainerNum, "")))
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(JobContainerSchema.JC_RC, container.JC_RC);
				filter.AddToFilter(JobContainerSchema.JC_IsShipperOwned, container.JC_IsShipperOwned);
				filter.AddToFilter(JobContainerSchema.PK, SQLComparisonOperator.NotEqual, mapped.ToArray());
				//filter.MaximumRows = container.JC_ContainerCount;

				AgencyShipmentContainer[] matchingContainers = (AgencyShipmentContainer[])shipment.RealContainers.Find(filter);

				if (matchingContainers.Length > container.JC_ContainerCount)
				{
					ZGuid[] pks = new ZGuid[container.JC_ContainerCount];
					for (int i = 0; i < pks.Length; i++)
					{
						pks[i] = matchingContainers[i].PK;
					}
					realBookedContainerMap.Add(container.PK, pks);
					mapped.AddRange(pks);
				}
				else if (matchingContainers.Length > 0)
				{
					ZGuid[] pks = Array.ConvertAll(matchingContainers, (c) => c.PK);
					realBookedContainerMap.Add(container.PK, pks);
					mapped.AddRange(pks);
				}
				else
				{
					realBookedContainerMap.Add(container.PK, Array.Empty<ZGuid>());
				}
			}

			AgencyShipmentContainer[] remaining = (AgencyShipmentContainer[])shipment.RealContainers.Find(new ZQuery(JobContainerSchema.PK, SQLComparisonOperator.NotEqual, mapped.ToArray()));
			if (remaining.Length > 0)
			{
				realBookedContainerMap.Add(ZGuid.Empty, Array.ConvertAll(remaining, (c) => c.PK));
			}

			return realBookedContainerMap;
		}
	}
}


