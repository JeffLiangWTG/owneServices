using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.ProcessManagement.DataTransfer.Test
{
	static class UniversalActivityTestExtensionMethods
	{
		public static OrganizationAddress FindByType_ForTest(this List<OrganizationAddress> addresses, ActivityOrganizationAddressType type)
		{
			return addresses.SingleOrDefault(x => x.AddressType.HasValue && string.Equals(x.AddressType, type.ToString(), StringComparison.OrdinalIgnoreCase));
		}
	}
}
