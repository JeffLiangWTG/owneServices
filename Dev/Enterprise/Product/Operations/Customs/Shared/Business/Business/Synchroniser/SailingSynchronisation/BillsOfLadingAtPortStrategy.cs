using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class BillsOfLadingAtPortStrategy
	{
		public BillsOfLadingAtPortStrategy(JobVoyage voyage)
		{
			this.voyage = voyage;
		}
		readonly JobVoyage voyage;

		public IEnumerable<BillOfLading> GetBillsOnVesselAt(ZDateTime arrivalTime, bool isContainerisedOnly)
		{
			var portVisitingSailingPKs = new List<ZGuid>();
			foreach (JobSailing sailing in voyage.Sailings)
			{
				if (sailing.JX_JA_E_DEP <= arrivalTime && sailing.JX_JB_E_ARV >= arrivalTime)
				{
					portVisitingSailingPKs.Add(sailing.PK);
				}
			}

			var query = new ZQuery(JobShipmentSchema.JS_JX, portVisitingSailingPKs);
			if (isContainerisedOnly)
			{
				var containerisedPackingTypes = new string[] { Core.Constants.ContainerModes.FCL, Core.Constants.ContainerModes.FCLMixedShipper, Core.Constants.ContainerModes.BuyersConsol, Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModes.Combination };
				var containerisedQuery = new ZQuery(JobShipmentSchema.JS_PackingMode, containerisedPackingTypes);
				query.AddToFilter(containerisedQuery, JoinCondition.And);
			}

			return voyage.Factory.Load<BillOfLading>(query);
		}
	}
}
