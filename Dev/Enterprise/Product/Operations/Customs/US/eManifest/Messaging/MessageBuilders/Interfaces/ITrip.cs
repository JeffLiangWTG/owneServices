using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public interface ITrip : Customs.Business.MessageBuilders.eManifest.ITrip
	{
		/// <summary>
		/// Indicates whether the conveyance is entering or leaving the United States. (C/1)
		/// In-Transit as defined in 19 CFR, 123.121. It is a domestic point to point move that's routed through a foreign country.
		/// Condition: To specify if 'In Transit'.
		/// </summary>
		ZString TransitDirectionCode { get; }

		IEnumerable<ICrew> CrewMembers { get; }
		IConveyance Conveyance { get; }
		IEnumerable<IEquipment> Equipment { get; }
		ZString DepartmentOfTransportationNumber { get; }
	}
}
