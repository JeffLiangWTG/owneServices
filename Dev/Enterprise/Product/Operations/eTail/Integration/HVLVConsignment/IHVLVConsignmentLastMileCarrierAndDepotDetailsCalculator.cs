using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVConsignmentLastMileCarrierAndDepotDetailsCalculator
	{
		void UpdateConsignmentsDestinationDetails(IEnumerable<ZGuid> consignmentPKs);

		void UpdateConsignmentsDestinationDetailsByClusterKeys(IEnumerable<int> bookingHeaderClusterKeys);
	}
}
