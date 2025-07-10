using CargoWise.Types;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public interface IInBond
	{
		//TODO: Paper Customs Form 7512 is required.

		/// <summary>
		/// Code creating entry for consignment. Needed by Express or Courier Consignment if doing Express Releases. (C/2)
		/// For In-Bond specify type i.e. 
		///  - IT (Immediate Transport port to port within the US),
		///  - IE (Immediate Export from the same US port),
		///  - TE (Transportation Export), Intransit, Multi-transit. 
		/// Condition: Entry types including In-Bond.
		/// </summary>
		ZString InbondType { get; }

		/// <summary>
		/// In-bond port/point of where Entry is filed or the port/point of exit - where the cargo clears (C/4)
		/// Schedule D - Domestic Port Codes
		/// Condition: if In-Bond move requested
		/// </summary>
		ZString InbondDestination { get; }

		/// <summary>
		/// !!!Future Use!!!
		/// SCAC code of Onward carrier to whom in bond goods are being transferred, if applicable. (C/4)
		/// This is the carrier who takes it out of the country for an Inbond TE or IE.
		/// Condition: if In-Bond move requested
		/// </summary>
		ZString OnwardCarrier { get; }

		/// <summary>
		/// A code representing the identification number of the bonded carrier. (C/12)
		/// Also referred to as the Importer or IRS number. This is the carrier used to transfer domestic within the US
		/// Condition: if In-Bond move requested
		/// </summary>
		ZString BondedCarrier { get; }

		/// <summary>
		/// Number issued for In-bond cargo. (C/16)
		/// Truck Manifest will use the Shipment Control Number as the inbond number whenever possible.
		/// Condition: if In-Bond move requested
		/// </summary>
		ZString Inbond7512Number { get; }

		/// <summary>
		/// !!!Future Use!!!
		/// IRS Number of transfer Carrier. (C/12)
		/// Condition: if Transfer requested
		/// </summary>
		ZString TransferCarrier { get; }

		/// <summary>
		/// Foreign Port of destination of cargo. Required for T&E and IE cargo. (C/5)
		/// Condition: specify for TE and IE.
		/// </summary>
		ZString ForeignPortOfDestination { get; }

		/// <summary>
		/// Schedule K, IATA Code, Inland Schedule K (as per qualifier)
		/// </summary>
		ZString ForeignPortOfDestinationCodeType { get; }

		/// <summary>
		/// Estimated Date of exit from the US-for use with Intransit, Export, and Inbond TE. (C/MMDDYYYY)
		/// Condition: for Export/In transit/In-bond TE & IE
		/// </summary>
		ZDate EstimatedDateOfUSExit { get; }

		/// <summary>
		/// The Mexican entry number: required for all Immediate Export inbonds to Mexico from Mexican border ports. (C/15)
		/// Condition: Required for T&E and IE to Mexico.
		/// </summary>
		ZString MexicanPedimentoNumber { get; }
	}
}
