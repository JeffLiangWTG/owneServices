namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("H1")]
	public abstract partial class CRLH1 : MessageBlock // Need to add interface for BIRD System
	{
		public CRLH1()
			: base("H1")
		{
		}

		/// <summary>
		/// A code representing the update action.
		/// </summary>
		[MessageBlockString(1, 3, "M")]
		public ZString UpdateActionCode;

		/// <summary>
		/// A code representing the district/port of entry. Generally, the district code is the same as the District Code (except in the case of authorized cross district processing) contained in the block control header record (Record Identifier B); however, the port code may be any valid port within the given district. Record the district/port code where the merchandise was entered under an entry or immediate delivery permit. Valid district/port codes can be queried through the Extract Reference File chapter of this publication.
		/// </summary>
		[MessageBlockString(4, 4, "M")]
		public ZString DistrictPortOfEntry;

		/// <summary>
		/// A unique code assigned by CBP to all active entry document preparers. The Entry Filer Code occupies the first three positions of an entry number regardless of where the entry is filed. This code must be the same as the Entry Filer Code in the block control header record (Record Identifier B).
		/// </summary>
		[MessageBlockString(3, 8, "M")]
		public ZString EntryFilerCode;

		/// <summary>
		/// The number assigned to the entry. If the entry number contains less than nine positions, it is right justified. For additional information on valid entry number formats, refer to Appendix E of this publication.
		/// </summary>
		[MessageBlockString(9, 11, "M", Justification = Justification.Right)]
		public ZString EntryNumber;

		/// <summary>
		/// A code representing the importer of record.
		/// </summary>
		[MessageBlockString(12, 20, "M")]
		public ZString ImporterNumber;

		/// <summary>
		/// A code representing the mode of transportation. Valid mode of transportation codes are listed in Appendix B of this publication. For entry type 06 (Foreign Trade Zone), do not input a Mode of Transportation Code.
		/// </summary>
		[MessageBlockString(2, 32, "C")]
		public ZString ModeOfTransportationMOTCode;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the estimated date of arrival. Be sure to keep the estimated date of arrival current in the Automated Commercial System (ACS). If the date needs to be changed, transmit a replace transaction and then a new certification transaction for the entry.
		/// </summary>
		[MessageBlockDate(34, "M", "MMddyy")]
		public ZDate EstimatedDateOfArrival;

		/// <summary>
		/// A code representing the bond type. Valid Bond Type Codes are:
		/// 
		/// 0 = No bond required
		/// 8 = Continuous bond
		/// 9 = Single entry bond
		/// </summary>
		[MessageBlockString(1, 40, "M")]
		public ZString BondTypeCode;

		/// <summary>
		/// A code of 1 if the entry is being certified for cargo release processing; otherwise, space fill.
		/// </summary>
		[MessageBlockInt(1, 41, "C")]
		public ZInt ReleaseCertificationCode;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the presentation date. If P is entered in the Entry Date Election Code (position 23) in Record Identifier H2, this date is required; otherwise, space fill.
		/// </summary>
		[MessageBlockDate(50, "C", "MMddyy")]
		public ZDate PresentationDate;

		/// <summary>
		/// The Bureau of the Census vessel code. This code is located on the bill of lading and is provided by the carrier. If this code is not available, the Vessel Name must be provided in Record Identifier H2 (positions 60-79).
		/// </summary>
		[MessageBlockString(5, 56, "C")]
		public ZString ImportingVesselCode;

		/// <summary>
		/// A code identifying the carrier. This code is required for air and sea shipments only at the current time. This code may be required in the future for other modes of transportation. If the mode of transportation code is 10 or 11 (vessel shipments), enter the appropriate 4-position Standard Carrier Alpha Code (SCAC) as issued by the National Motor Traffic Association Inc. This code is usually located on the bill of lading. If it is not, the carrier should be able to provide it. If the mode of transportation code is 40 (air shipments), enter the appropriate 2-position carrier abbreviation, left justified. For a description of the mode of transportation codes, refer to Appendix B of this publication. Air carrier codes may be obtained by querying the Extract Reference File. For additional information, refer to the Extract Reference File chapter of this publication. For entry type 06 (Foreign Trade Zone), do not input a Carrier code.
		/// </summary>
		[MessageBlockString(4, 61, "C")]
		public ZString CarrierCode;

		/// <summary>
		/// A code representing the district/port where merchandise was unladen from the carrier. Valid district/port codes can be queried through the Extract Reference File chapter of this publication.
		/// </summary>
		[MessageBlockString(4, 65, "C")]
		public ZString DistrictPortOfUnlading;

		/// <summary>
		/// A code representing the entry type. Valid entry type codes are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(2, 69, "M")]
		public ZString EntryType;

		/// <summary>
		/// The surety code related to the bond. This code is required for single entry bonds.
		/// </summary>
		[MessageBlockString(3, 71, "C")]
		public ZString SuretyCode;

		/// <summary>
		/// Space fill. This code is for future use.
		/// </summary>
		[MessageBlockString(1, 74, "C")]
		public ZString OtherGovernmentAgencyOGACodes;

		/// <summary>
		/// A code of 1 indicates that this entry transaction record will use a consignee name and address instead of a consignee number.
		/// </summary>
		[MessageBlockString(1, 75, "O")]
		public ZString ConsigneeNameAndAddress;
	}
}
