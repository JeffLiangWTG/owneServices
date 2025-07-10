namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[InputBlock("F111")]
	public partial class ERFF111 : MessageBlock
	{
		public ERFF111()
			: base("F111")
		{
		}

		/// <summary>
		/// A Facilities Information and Resources Management System (FIRMS) code representing the location of goods. Each FIRMS Code query transaction requires the transmission of only one F111 record. Any number of F111 records can be included in a single transmission, batch or block. If this record contains a FIRMS Code, the Name of the Facility (positions 9-43) and the District Code (positions 44-45) must be space filled.
		/// </summary>
		[MessageBlockString(4, 5, "C")]
		public ZString FIRMSCode;

		/// <summary>
		/// The name of the facility where the goods are located. This name is left justified. If there is a FIRMS Code in positions 5-8, space fill. If there is a Facility Name, the District Code (positioned 44-45) is required. No matter how many characters are transmitted in the Name of Facility field, data is returned for all FIRMS codes assigned to the specific district which have the same set of characters at the beginning of the facility name in the FIRMS Code file. For example, if Yellow is transmitted in the Name of Facility field, data for all records associated with the specified district and having the facility name beginning with Yellow are returned.
		/// </summary>
		[MessageBlockString(35, 9, "C")]
		public ZString NameOfFacility;

		/// <summary>
		/// The district code is the first two numbers of the district/port code. Valid District/Port Codes can be queried through Record Identifier F101 of this chapter. If there is a FIRMS Code in positions 5-8, space fill. A query can be on the District Code or on the District Code and Name of Facility.
		/// </summary>
		[MessageBlockString(2, 44, "C")] // string
		public ZString DistrictCode;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the begin date. This date cannot be in the future. If a begin date is used, only those FIRMS codes which were added or updated on or after that date will be returned. A query by begin date can only be requested if the begin date is transmitted by itself or in conjunction with a district code. The FIRMS code or facility name cannot be used with a begin date.
		/// </summary>
		[MessageBlockDate(46, "C", "MMddyy")]
		public ZDate BeginDate;
	}
}
