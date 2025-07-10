using System.Collections.Generic;
using Enterprise.Freight.Agency.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	interface IAgencyShipmentReadStrategy<in T>
		where T : AgencyShipment
	{
		void ReadContainers(T agencyShipment, DataObjectList<Container> containers);
		void ReadPackingLines(T agencyShipment, IEnumerable<PackingLine> packLines);
	}

	interface IAgencyShipmentReadStrategy : IAgencyShipmentReadStrategy<AgencyShipment> { }
}
