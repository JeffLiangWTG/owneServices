namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[OutputBlock("SC")]
	public partial class CMQSC : MessageBlock
	{
		public CMQSC()
			: base("SC")
		{
		}

		/// <summary>
		/// The two or three character identifier of the importing air carrier.
		/// </summary>
		[MessageBlockString(3, 3, "M")]
		public ZString ImportingCarrierCode;

		/// <summary>
		/// If less than five characters are transmitted, the data is “normalized” to five positions to facilitate matches with entries.
		/// </summary>
		[MessageBlockString(5, 6, "M")]
		public ZString FlightNumber;

		/// <summary>
		/// Six-character date in MMDDYY (month, day, year) format representing the date the importing carrier expects the flight to arrive in the U.S.
		/// </summary>
		[MessageBlockDate(11, "M", "MMddyy")]
		public ZDate ScheduledArrivalDate;

		/// <summary>
		/// The first three positions identify the air carrier, the next seven are a sequential number and the last position is a check digit based on MOD 7.
		/// </summary>
		[MessageBlockString(11, 17, "M")]
		public ZString AirWaybillNumber;

		/// <summary>
		/// An alpha character indicating the “split” identifier associated to a split master air waybill.
		/// </summary>
		[MessageBlockString(1, 28, "C")]
		public ZString PartIndicator;

		/// <summary>
		/// The quantity associated with the master air waybill.
		/// </summary>
		[MessageBlockInt(5, 29, "M")]
		public ZInt ManifestQuantity;

		/// <summary>
		/// The quantity associated with a specific split. If there is a part indicator this record is mandatory.
		/// </summary>
		[MessageBlockInt(5, 34, "C")]
		public ZInt BoardedQuantity;

		/// <summary>
		/// The house air waybill is associated to an Air Waybill number and may be used to define the query. It may be sent only in conjunction with an Air Waybill Number.
		/// </summary>
		[MessageBlockString(12, 39, "O")]
		public ZString HouseAirWaybillNumber;

		/// <summary>
		/// An alpha character indicating the “split” identifier associated to a split house air waybill.
		/// </summary>
		[MessageBlockString(1, 51, "C")]
		public ZString PartIndicator1;

		/// <summary>
		/// The quantity associated with the house air waybill.
		/// </summary>
		[MessageBlockInt(5, 52, "C")]
		public ZInt ManifestQuantity1;

		/// <summary>
		/// The quantity associated with a specific split. If there is a part indicator this record is mandatory.
		/// </summary>
		[MessageBlockInt(5, 57, "C")]
		public ZInt BoardedQuantity1;

		/// <summary>
		/// An 11 or 9-digit in-bond number
		/// </summary>
		[MessageBlockString(11, 62, "C")]
		public ZString InbondNumber;

		/// <summary>
		/// A code indicating the status of the in-bond. For future use. Until further notice, blanks will be returned in this field.
		/// </summary>
		[MessageBlockInt(2, 73, "C")]
		public ZInt InbondStatus;
	}
}
