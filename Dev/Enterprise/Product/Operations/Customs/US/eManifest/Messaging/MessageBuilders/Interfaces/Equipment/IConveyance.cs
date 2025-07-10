using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public interface IConveyance : Customs.Business.MessageBuilders.eManifest.IConveyance
	{
		/// <summary>
		/// ACE ID of Conveyance if pre-registered in ACE. (C/10)
		/// (If pre-registered, only Conveyance ACE ID is sufficient to associate pre-registered Conveyance to Manifest. Otherwise the subsequent Conveyance data elements need to be provided)
		/// Condition: if Conveyance pre-registered in ACE.
		/// </summary>
		ZString ConveyanceACEId { get; }

		/// <summary>
		/// Unique identification of conveyance specified by Carrier. (C/23)
		/// Maybe pre-registered in ACE. (If pre-registered, only Conveyance ID is sufficient to associate pre-registered Conveyance to Manifest. Otherwise the subsequent Conveyance data elements need to be provided)
		/// Condition: if Conveyance pre-registered in ACE.
		/// </summary>
		ZString ConveyanceId { get; }
		/// <summary>
		/// Vehicle Insurance information-needed for trucks hauling hazardous material
		/// Condition: if Hazmat Shipment.
		/// </summary>
		IInsurance Insurance { get; }

		/// <summary>
		/// Indicates party whose bond is obligated for release of Instruments of International Traffic (IIT). (C/2)
		/// Use this for IITs in the Conveyance. If IIT are in the Equipment, use the Equipment level IITs.
		/// Condition: If IIT's in equipment.
		/// </summary>
		IEnumerable<ZString> IITEntityIndicators { get; }
	}
}
