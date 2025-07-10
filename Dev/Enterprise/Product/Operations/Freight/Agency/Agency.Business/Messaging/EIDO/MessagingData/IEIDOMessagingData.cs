using System;
using System.Collections.Generic;

namespace Enterprise.Freight.Agency.Business
{
	public interface IEIDOMessagingData
	{
		DateTime MessagePrepared { get; }
		DateTime? EstimatedArrivalDate { get; }

		/// <summary>
		/// NAD MR - Message Recipient
		/// (Discharge Terminal)
		/// </summary>
		IEIDOOrganisation MessageRecipient { get; }

		/// <summary>
		/// NAD MS - Message Sender
		/// (Shipping Line / Agent)
		/// </summary>
		IEIDOOrganisation MessageSender { get; }

		/// <summary>
		/// NAD CA - Issuer of Delivery Order
		/// (Shipping Line / Agent / Forwarder)
		/// </summary>
		IEIDOOrganisation Issuer { get; }

		/// <summary>
		/// NAD SF -- Place where Cargo is Available for Collection.
		/// </summary>
		IEIDOOrganisation CargoCollection { get; }

		/// <summary>
		/// NAD AV - Authorising Official
		/// </summary>
		string Password { get; }

		/// <summary>
		/// REF AAJ - Delivery Order
		/// </summary>
		string PIN { get; }

		string ReferenceNumber { get; }
		string BillOfLading { get; }

		string CarrierName { get; }
		string CarrierACOS { get; }

		string VesselName { get; }
		string VesselLloyds { get; }
		string Voyage { get; }

		/// <summary>
		/// LOC 11 - Port of Discharge
		/// </summary>
		string DischargePort { get; }

		EIDOMessageFunction MessageFunction { get; }

		IEnumerable<IEIDOEquiptmentData> Equipment { get; }
	}
}
