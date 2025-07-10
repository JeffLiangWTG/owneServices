#nullable enable
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	class DtbConsignmentChargeHelper
	{
		public static DtbConsignment? GetConsignment(JobCharge charge)
		{
			if (charge?.Job?.Parent is DtbConsignment consignment)
			{
				return consignment;
			}

			return null;
		}
	}
}
