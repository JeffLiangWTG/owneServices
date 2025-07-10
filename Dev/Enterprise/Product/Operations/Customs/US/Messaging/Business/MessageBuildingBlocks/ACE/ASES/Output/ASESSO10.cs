namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	[OutputBlock("WO10")]
	public class ACEQWO10 : ASESSO10Base
	{
		public ACEQWO10()
			: base("WO10")
		{
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)]
	[OutputBlock("SO10")]
	public class ASESSO10 : ASESSO10Base
	{
		public ASESSO10()
			: base("SO10")
		{
		}
	}

	public abstract class ASESSO10Base : MessageBlock
	{
		public ASESSO10Base(string mandatoryCharacters)
			: base(mandatoryCharacters)
		{
		}

		/// <summary>
		/// District/port of entry.
		/// </summary>
		[MessageBlockString(4, 5, "C")]
		public ZString DistrictPortOfEntry;

		/// <summary>
		/// A unique code assigned by CBP to all active entry document preparers.
		/// </summary>
		[MessageBlockString(3, 9, "M")]
		public ZString EntryFilerCode;

		/// <summary>
		/// The number assigned to the entry.
		/// </summary>
		[MessageBlockString(8, 14, "M")]
		public ZString EntryNumber;

		/// <summary>
		/// A code representing the entry type.
		/// </summary>
		[MessageBlockString(2, 23, "M")]
		public ZString EntryTypeCode;

		/// <summary>
		/// A code identifying the importer of record.
		/// </summary>
		[MessageBlockString(12, 25, "M")]
		public ZString ImporterOfRecordNumber;

		/// <summary>
		/// A code identifying the carrier.
		/// </summary>
		[MessageBlockString(4, 37, "C")]
		public ZString CarrierCode;

		/// <summary>
		/// Importing vessel name.
		/// </summary>
		[MessageBlockString(20, 41, "C")]
		public ZString VesselName;

		/// <summary>
		/// The voyage/flight/trip number of the importing carrier.
		/// </summary>
		[MessageBlockString(5, 61, "C")]
		public ZString VoyageFlightTripManifestNumber;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the date of arrival.
		/// </summary>
		[MessageBlockDate(66, "C", "MMddyy")]
		public ZDate EstimatedDateOfArrival;

		/// <summary>
		/// Split Shipment release code selected by the filer.
		/// </summary>
		[MessageBlockString(1, 72, "O")]
		public ZString SplitShipmentReleaseCode;

		/// <summary>
		/// Indicates that the SO response is due to a PGA CA (correction) request.
		/// </summary>
		[MessageBlockString(1, 73, "C")]
		public ZString CorrectionResponseIndicator;
	}
}
