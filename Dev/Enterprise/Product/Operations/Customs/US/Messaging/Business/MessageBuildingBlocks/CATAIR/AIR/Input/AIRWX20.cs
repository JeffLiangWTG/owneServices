namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[InputBlock("20")]
	public partial class AIRWX20 : MessageBlock
	{
		public AIRWX20()
			: base("20")
		{
		}

		/// <summary>
		/// A date in YYMMDD (year, month, day) format representing the date of actual arrival or export at the destination port.
		/// </summary>
		[MessageBlockDate(3, "M", "yyMMdd")]
		public ZDate Date;

		/// <summary>
		/// A time in HHMMSS (hour, minute, second) 24-hour clock format representing the time of actual arrival or export at the destination port.
		/// </summary>
		[MessageBlockString(6, 9, "M")]
		public ZString Time;

		/// <summary>
		/// The Census Schedule D code representing the CBP port of destination for the in-bond movement.
		/// </summary>
		[MessageBlockString(4, 15, "M")]
		public ZString PortOfArrivalDepartureOrExport;

		/// <summary>
		/// This data element is reserved for future use. Space fill.
		/// </summary>
		[MessageBlockString(12, 23, "O")]
		public ZString BondedCarrierID;

		/// <summary>
		/// This data element is reserved for future use. Space fill.
		/// </summary>
		[MessageBlockString(19, 35, "O")]
		public ZString CityName;

		/// <summary>
		/// This data element is reserved for future use. Space fill
		/// </summary>
		[MessageBlockString(2, 54, "O")]
		public ZString StateCode;

		/// <summary>
		/// The mode of transportation (MOT) code of the exporting conveyance. Valid codes are:
		/// 
		/// 10 = Ocean
		/// 20 = Rail
		/// 30 = Truck
		/// 40 = Air
		/// </summary>
		[MessageBlockString(2, 56, "O")]
		public ZString ExportMOT;

		/// <summary>
		/// This data element is reserved for future use. Space fill.
		/// </summary>
		[MessageBlockString(3, 58, "O")]
		public ZString OnwardCarrierCode;

		/// <summary>
		/// A code representing the International Air Transport Association Code (IATA) of the importing carrier. Required when action code 2 or 6 is used in the WX 10 record and if split shipment.
		/// </summary>
		[MessageBlockString(3, 61, "C")]
		public ZString ImportingCarrierCode;

		/// <summary>
		/// Flight number assigned by the importing carrier. Format must be NNN, NNNA, NNNN or NNNNA. Left justified. Required when action code 2 or 6 is used in the WX 10 record and if split shipment.
		/// </summary>
		[MessageBlockString(5, 64, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString FlightNumber;

		/// <summary>
		/// Importing carrier's scheduled arrival date in MMDDYY (month, day, year) format. Required when action code 2 or 6 is used in the WX 10 record and if split shipment.
		/// </summary>
		[MessageBlockDate(69, "C", "MMddyy")]
		public ZDate ScheduledArrivalDate;

		/// <summary>
		/// A code indicating the type of transport used by the importing carrier. Valid code is:
		/// 
		/// 40 = Air.
		/// </summary>
		[MessageBlockString(2, 75, "M")]
		public ZString ModeOfTransportMOTCode;
	}
}
