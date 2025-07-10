namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Abstract
{
	using CargoWise.Types;

	[OutputBlock("R1")]
	public abstract partial class CRLR1 : MessageBlock // Need to add interface for BIRD System
	{
		public CRLR1()
			: base("R1")
		{
		}

		/// <summary>
		/// For entries not flagged for Remote Location Filing (RLF), this code represents the district/port of entry. For RLF entries, this code represents the entry’s preparation port. Valid District/Port Codes can be queried through the Extract Reference File chapter of this publication.
		/// </summary>
		[MessageBlockString(4, 3, "M")]
		public ZString DistrictPortOfEntry;

		/// <summary>
		/// A unique code assigned by CBP to all active entry document preparers. The Entry Filer Code occupies the first three positions of an entry number regardless of where the entry is filed. This code must be the same as the Entry Filer Code in the block control header record (Record Identifier B).
		/// </summary>
		[MessageBlockString(3, 7, "M")]
		public ZString EntryFilerCode;

		/// <summary>
		/// The number assigned to the entry. For additional information on valid entry number formats, refer to Appendix E of this publication.
		/// </summary>
		[MessageBlockString(9, 10, "M", Justification = Justification.Right)]
		public ZString EntryNumber;

		/// <summary>
		/// A code representing the entry type. Valid Entry Type Codes are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(2, 19, "M")]
		public ZString EntryTypeCode;

		/// <summary>
		/// A code identifying the importer of record.
		/// </summary>
		[MessageBlockString(12, 21, "M")]
		public ZString ImporterOfRecordNumber;

		/// <summary>
		/// An optional code provided by the participant. This field is not edited during ACS processing. It is for internal user system control in cargo release processing.
		/// </summary>
		[MessageBlockString(9, 33, "C")]
		public ZString BrokerReferenceNumber;

		/// <summary>
		/// A code identifying the carrier. This code is required for air and sea shipments only. This code may be required in the future for other modes of transportation. If the mode of transportation code is 10 or 11 (vessel shipments), enter the appropriate 4-position Standard Carrier Alpha Code (SCAC) as issued by the National Motor Traffic Association Inc. This code is usually located on the bill of lading. If it is not, the carrier should be able to provide it. If the mode of transportation code is 40 (air shipments), enter the appropriate 2-position carrier abbreviation, left justified. For a description of the mode of transportation codes, refer to Appendix B of this publication. Air carrier codes may be obtained by querying the Extract Reference File. For additional information, refer to the Extract Reference File chapter of this publication.
		/// </summary>
		[MessageBlockString(4, 42, "M")]
		public ZString CarrierCode;

		/// <summary>
		/// If the mode of transportation code is 10 or 11 (vessel) and the importing vessel code is not provided in this record, the importing vessel name is required. The vessel name is also required if the entry is a paired entry. For additional information on a paired entry, refer to CBP Directive 3200-18 dated August 21, 1987. If the name exceeds 20 positions, enter the first 20 and truncate the excess.
		/// </summary>
		[MessageBlockString(20, 46, "C")]
		public ZString VesselName;

		/// <summary>
		/// The voyage/flight/trip number of the importing carrier. The voyage/flight/trip number is mandatory for mode of transportation codes 10, 11, 40 and 41. If the mode of transportation code is 40 (air), enter the flight number. If the mode of transportation code is 10 or 11 (vessel), enter the complete voyage number. For example, if the voyage number is V311W, enter all five characters. For a description of the mode of transportation codes, refer to Appendix B of this publication. In the future, this field may be required for others if the automated manifest interfaces are established between CBP and rail/truck (road) carriers.
		/// </summary>
		[MessageBlockString(5, 66, "M")]
		public ZString VoyageFlightTripManifestNumber;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the date of arrival.
		/// </summary>
		[MessageBlockDate(71, "M", "MMddyy")]
		public ZDate DateOfArrival;
	}
}
