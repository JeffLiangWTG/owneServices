namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("H2")]
	public abstract partial class CRLH2 : MessageBlock // Need to add interface for BIRD System
	{
		public CRLH2()
			: base("H2")
		{
		}

		/// <summary>
		/// A Facilities Information and Resources Management System (FIRMS) code. This code is a 4-position serial number. For additional information on FIRMS codes, refer to the Extract Reference File chapter of this publication.
		/// </summary>
		[MessageBlockString(4, 3, "M")]
		public ZString LocationOfGoods;

		/// <summary>
		/// A code identifying the ultimate consignee. If there are multiple ultimate consignee numbers to be reported leave this field blank and report them in the H5 record. If a 1 is reported in the consignee name and address field, leave blank.
		/// </summary>
		[MessageBlockString(12, 11, "C")]
		public ZString UltimateConsigneeNumber;

		/// <summary>
		/// A code representing the Entry Date Election Code. Valid Entry Date Election Codes are:
		/// 
		/// P = Date of Presentation
		/// A = Date of Arrival
		/// </summary>
		[MessageBlockString(1, 23, "C")]
		public ZString EntryDateElectionCode;

		/// <summary>
		/// The voyage/flight/trip number of the importing carrier. The voyage/ flight/trip number is mandatory for mode of transportation codes 10, 11, 40 and 41. If the mode of transportation code is 40 (air), enter the flight number. If the mode of transportation code is 10 or 11 (vessel), enter the complete voyage number. For example, if the voyage number is V311W, enter all five characters. For a description of the mode of transportation codes, refer to Appendix B of this publication. In the future, this field may be required for others if the automated manifest interfaces are established between CBP and rail/truck (road) carriers. For entry type 06 (Foreign Trade Zone), do not input a Voyage/ Flight/Trip Manifest Number.
		/// </summary>
		[MessageBlockString(5, 24, "C")]
		public ZString VoyageFlightTripManifestNumber;

		/// <summary>
		/// The total entered value of the entry in whole dollars.
		/// </summary>
		[MessageBlockDecimal(10, 29, "M", 0)]
		public ZDecimal TotalEntryValue;

		/// <summary>
		/// An optional code provided by the participant. This field is not edited during ACS processing. It is for internal user system control in cargo release processing.
		/// </summary>
		[MessageBlockString(9, 39, "O")]
		public ZString BrokerReferenceNumber;

		/// <summary>
		/// If the mode of transportation code is 10 or 11 (vessel) and the importing vessel code is not provided in the H1 record, the importing vessel name is required. If the name exceeds 20 positions, enter the first 20 and truncate the excess. Type 06 entries require an FTZ number. The first three positions of a FTZ number are always FTZ followed by a three-position character codes within the range of “001” through “300” inclusive. No number outside of this range will be accepted. The last character can be any alpha (A-Z) or numeric (0-9) character. For a description of the mode of transport codes. Refer to Appendix B of this publication
		/// </summary>
		[MessageBlockString(20, 60, "C")]
		public ZString VesselNameForeignTradeZoneNumber;
	}
}
