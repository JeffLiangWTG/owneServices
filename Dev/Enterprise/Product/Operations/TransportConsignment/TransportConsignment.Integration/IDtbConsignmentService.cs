using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.TransportConsignment.Integration
{
	public interface IDtbConsignmentService
	{
		Dictionary<string, string> GetDefaultValuesForConsignment(ZGuid pickupAddressPK, ZGuid deliveryAddressPK, string jobType);
		Dictionary<string, string> GetDefaultValuesForConsignment(ZGuid pickupAddressPK, ZGuid deliveryAddressPK, string jobType, BusinessObjectFactory factory);

		string IncoTermKey { get; }
		string ServiceLevelKey { get; }
	}
}
