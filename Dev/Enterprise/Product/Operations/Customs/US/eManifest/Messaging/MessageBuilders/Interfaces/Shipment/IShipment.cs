using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public interface IShipment
	{
		/// <summary>
		/// Add/Remove/Change
		/// </summary>
		ZString ShipmentActionCode { get; }

		/// <summary>
		/// Shipment type. (M/2)
		/// </summary>
		ZString ShipmentType { get; }

		/// <summary>
		/// SCAC + sequence number. Truck - PRO Number. (M/16)
		/// A unique BOL number used by the reporting trade participant(s) to identify the shipment or consolidation.
		/// Ideally, a master number, identifying the transaction throughout its lifecycle for both commercial and transportation purposes. (Master bill).
		/// The same issuer and Shipment Control Number may not be repeated for 1 year.
		/// </summary>
		ZString ShipmentControlNumber { get; }

		/// <summary>
		/// Trader Reference Number. (O/50)
		/// Number provided by the shipper to be passed through to the broker on the Customs download.
		/// </summary>
		ZString ShipmentIdentifier { get; }

		/// <summary>
		/// Free Form Text. (M/40)
		/// Port/Point at which merchandize is loaded on the conveyance that will cross the border.
		/// Can be Schedule K, IATA Code, Inland Schedule K or location name (as per qualifier)
		/// </summary>
		ZString PortOrPointOfLoading { get; }

		/// <summary>
		/// Schedule K, IATA Code, Inland Schedule K or location name (as per qualifier)
		/// </summary>
		ZString PortOrPointOfLoadingCodeType { get; }

		/// <summary>
		/// City where carrier took receipt of goods. (Actual City Free Form). (C/17)
		/// Condition: If different from Port/Point of loading.
		/// </summary>
		ZString PlaceOfReceipt { get; }

		/// <summary>
		/// Type of shipping contract. (C/2)
		/// Condition: If the shipment is being delivered under one of the listed service types,
		/// then the appropriate service type must be reported; otherwise, do not report the service type.
		/// </summary>
		ZString ServiceType { get; }

		/// <summary>
		/// Firms - XRJ This would be used to request a local transfer from the carrier to a bonded facility such as a CFS or for In-Bond. (C/4)
		/// Condition: if Transfer requested
		/// </summary>
		ZString TransferDestinationFIRMSCode { get; }

		/// <summary>
		/// This quantity is what is actually boarded by the carrier when loading the shipment. (C/10)
		/// Condition: specify for Split Shipments.
		/// </summary>
		ZInt BoardedQuantity { get; }

		/// <summary>
		/// Used to specify amendment reason  for changes after complete submission of Manifest to Customs. (C/2)
		/// Condition: Used to amend a completed Manifest
		/// </summary>
		ZString ShipmentAmendmentReasonCode { get; }

		/// <summary>
		/// US Customs & Border Protection is required by FDA (Food and Drug Administration) (C/1)
		/// in compliance with the Bioterrorism Act to capture the FDA indication from carriers transporting animal or human food into the U.S.
		/// Condition: for transporting animal or human food into the U.S.
		/// </summary>
		ZBool FDAFreightIndicator { get; }

		/// <summary>
		/// To indicate whether or not the shipment re-entering the U.S. has been out of the country under foreign Customs custody (C/1)
		/// or under custody of the carrier for 45 days or less since the date of exportation.
		/// Condition: specify for Goods Astray.
		/// </summary>
		ZBool ShipmentWasOutOfUSFor45DaysOrLessIndicator { get; }

		/// <summary>
		/// To report the exit date from the U.S.in the case of shipments re-entering the country
		/// and claiming release as free astray or under Headnote 1 of the Tariff Act.
		/// Condition: specify for Goods Astray.
		/// </summary>
		ZDate ExportDate { get; }

		/// <summary>
		/// Shipment-Commodity - multiple occurances if applicable (M/1)
		/// </summary>
		IEnumerable<ICommodity> Commodities { get; }

		/// <summary>
		/// Shipment-Parties - multiple occurances. Shipper/Consignee - required. (M/2)
		/// </summary>
		IEnumerable<IParty> Parties { get; }

		/// <summary>
		/// Shipment-In-Bond - provide for In-Bond (C)
		/// </summary>
		IInBond InBond { get; }

		/// <summary>
		/// Determines if a shipment is in Customs file.
		/// </summary>
		bool IsLodged { get; }

		/// <summary>
		/// Determines if a shipment is split between trips.
		/// </summary>
		bool IsSplit { get; }
	}
}
