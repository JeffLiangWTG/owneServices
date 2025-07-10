using System;
using System.Collections.Generic;

namespace Enterprise.Freight.Agency.Business
{
	public interface IPortAuthorityMessagingData
	{
		DateTime MessagePrepared { get; }

		PortAuthorityMessageFunction MessageFunction { get; }

		string VesselName { get; }
		string VesselLloyds { get; }
		string Voyage { get; }

		/// <summary>
		/// Exports Only:
		/// LOC 9 -- Port Of Loading
		/// </summary>
		string Load { get; }

		/// <summary>
		/// Imports Only:
		/// LOC 11 -- Port Of Discharge
		/// </summary>
		string Discharge { get; }

		IEnumerable<IPortAuthorityEquipmentData> Equipment { get; }
		IEnumerable<IPortAuthorityConsignmentData> Consignments { get; }
	}
}
