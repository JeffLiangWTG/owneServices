namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[OutputBlock("1M")]
	public partial class AMS1M : MessageBlock
	{
		public AMS1M()
			: base("1M")
		{
		}

		/// <summary>
		/// This is the Standard Carrier Alpha Code (SCAC) of the importing carrier.
		/// </summary>
		[MessageBlockString(4, 3, "M")]
		public ZString CarrierCode;

		/// <summary>
		/// A code indicating the method of transportation.
		/// 
		/// 10 = Vessel, non-container, or unable to determine if container (Including Lightered, Land Bridge and LASH)
		/// 11 = Vessel Containerized (Container)
		/// 20 = Rail non-container or unable to determine if container
		/// 21 = Rail, containerized
		/// 30 = Land, non-container or unable to determine if container
		/// 40 = Air, non-container, or unable to determine container (future use)
		/// 41 = Air, containerized (future use)
		/// </summary>
		[MessageBlockString(2, 7, "M")]
		public ZString TransportationIndicator;

		/// <summary>
		/// ISO country code for the importing conveyance. In Rail and Ocean AMS this is mandatory. In Truck AMS this is not used.
		/// </summary>
		[MessageBlockString(2, 9, "C")]
		public ZString CountryCodeOfImportingConveyance;

		/// <summary>
		/// Name of the importing conveyance, or trip number in motor environment. In truck manifest, this is the trip number. In truck manifest preliminary bills, if the trip number is unknown, “system” will appear here. In Sea AMS either this or the Lloyds Vessel Code (positions 51-57) will be returned.
		/// </summary>
		[MessageBlockString(23, 11, "C")]
		public ZString ImportingConveyanceName;

		/// <summary>
		/// Rail AMS - Julian date in YYDDD (year, date) format. 
		/// Ocean AMS - The voyage number entered.
		/// </summary>
		[MessageBlockString(5, 34, "C")]//The comments above indicate why this needed to change.
		public ZString TripData;

		/// <summary>
		/// A code representing the manifest sequence number. This number is an optional carrier-assigned number. The system- generated default is one (000001). It may be a date. Once transmitted it cannot be changed. All subsequent transmissions must use the original manifest sequence number. This is used only in Rail and Ocean AMS.
		/// </summary>
		[MessageBlockString(6, 44, "O")]
		public ZString ManifestSequenceNumber;

		/// <summary>
		/// The International Maritime Organization (IMO) code representing the importing vessel. Either this or the Importing Conveyance Name (positions 11-33) will be returned.
		/// </summary>
		[MessageBlockString(7, 51, "C")]
		public ZString VesselCode;

		/// <summary>
		/// P = Preliminary (Rail and Truck AMS)
		/// Y= Amendment (Rail, Ocean and Truck AMS)
		/// T = Intransit (Rail and Truck AMS) Future Use
		/// W = Complete (Ocean and Truck AMS)
		/// </summary>
		[MessageBlockString(1, 58, "M")]
		public ZString ManifestTypeCode;
	}
}
