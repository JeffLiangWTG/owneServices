using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration.Freight;

namespace Enterprise.Freight.Integration
{
	public static partial class Forwarding
	{
		public interface ISupplyChainSecurityImportExportSupporter
		{
			bool IsAir { get; }

			ZString LoadCountryForSupplyChainSecurity { get; }

			ZString DischargeCountryForSupplyChainSecurity { get; }

			IEnumerable<ITransport> SupplyChainSecurityRelatedTransports { get; }
		}
	}
}
