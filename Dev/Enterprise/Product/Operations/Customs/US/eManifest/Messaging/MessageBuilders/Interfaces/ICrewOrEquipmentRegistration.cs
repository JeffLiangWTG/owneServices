using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public interface ICrewOrEquipmentRegistration : IEDIMessageCollectionProvider
	{
		/// <summary>
		/// Carrier SCAC Code. (M)
		/// A unique ID assigned by the CBSA to an approved carrier.
		/// </summary>
		ZString CarrierCode { get; }

		/// <summary>
		/// Name of person to contact in case of errors. (M)
		/// </summary>
		ZString OriginatorFullName { get; }

		/// <summary>
		/// Phone number of person to contact in case of errors. (M)
		/// </summary>
		ZString OriginatorPhone { get; }

		IEnumerable<ICrew> CrewMembers { get; }
		IConveyance Conveyance { get; }
		IEnumerable<IEquipment> Equipment { get; }
	}
}
