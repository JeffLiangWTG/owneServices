namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[OutputBlock("SE50")]
	public partial class ASESSE50 : MessageBlock
	{
		public ASESSE50()
			: base("SE50")
		{
		}

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the disposition action date.
		/// </summary>
		[MessageBlockDate(5, "M", "MMddyy")]
		public ZDate DispositionDate;

		/// <summary>
		/// The military time in HHMM (hour, minute) format representing the time of the disposition action.
		/// </summary>
		[MessageBlockString(4, 11, "M")]
		public ZString DispositionTime;

		/// <summary>
		/// A code representing the disposition action.
		/// </summary>
		[MessageBlockString(2, 15, "M")]
		public ZString DispositionCode;

		/// <summary>
		/// The narrative message associated with the disposition code.
		/// </summary>
		[MessageBlockString(40, 17, "M")]
		public ZString NarrativeMessage;

		/// <summary>
		/// Indicates if shipment is split (Y/N)
		/// </summary>
		[MessageBlockString(1, 57, "M")]
		public ZString SplitIndicator;

		/// <summary>
		/// A code identifying the carrier.
		/// </summary>
		[MessageBlockString(4, 58, "C")]
		public ZString CarrierCode;

		/// <summary>
		/// The voyage/flight/trip number of the importing carrier.
		/// </summary>
		[MessageBlockString(5, 62, "C")]
		public ZString VoyageFlightTripManifestNumber;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the date of arrival.
		/// </summary>
		[MessageBlockDate(67, "C", "MMddyy")]
		public ZDate DateOfArrival;

		/// <summary>
		/// District/port of arrival.
		/// </summary>
		[MessageBlockString(4, 73, "C")]
		public ZString DistrictPortOfArrival;
	}
}
