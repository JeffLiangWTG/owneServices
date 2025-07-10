using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public interface IEquipment : Customs.Business.MessageBuilders.eManifest.IEquipment
	{
		/// <summary>
		/// ACE ID of Equipment if pre-registered in ACE.
		/// (If pre-registered, only Equipment ACE ID is sufficient to associate pre-registered Equipment to Manifest. Otherwise the subsequent Equipment data elements need to be provided)
		/// US: (C/10), Condition: if Equipment pre-registered in ACE.
		/// </summary>
		ZString EquipmentACEId { get; }

		/// <summary>
		/// Indicates party whose bond is obligated for release of Instruments of International Traffic (IIT).
		/// Use this for IITs in the Conveyance. If IIT are in the Equipment, use the Equipment level IITs.
		/// US: (C/2), Condition: If IIT's in equipment.
		/// </summary>
		IEnumerable<ZString> IITEntityIndicators { get; }
	}
}
