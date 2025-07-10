namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[OutputBlock("90")]
	public partial class FTZNF90 : MessageBlock
	{
		public FTZNF90()
			: base("90")
		{
		}

		/// <summary>
		/// A code representing the Admission Type. Refer to Record Identifier FT20, Note 1 of this chapter for valid codes.
		/// </summary>
		[MessageBlockString(1, 3, "C")]
		public ZString AdmissionType;

		/// <summary>
		/// Zone ID is composed of the following:
		/// 
		/// GPZ ID = 3 numeric
		/// Subzone ID = 2 alpha-numeric
		/// Site ID = 2 numeric
		/// </summary>
		[MessageBlockString(7, 4, "C")]
		public ZString ZoneID;

		/// <summary>
		/// Calendar year in YY (year) format.
		/// </summary>
		[MessageBlockInt(2, 11, "M")]
		public ZInt CalendarYear;

		/// <summary>
		/// Free form number governing the admission. The Zone ID + year + control number = the standardized admission number.
		/// </summary>
		[MessageBlockString(8, 13, "C")]
		public ZString ControlNumber;

		/// <summary>
		/// Schedule D Code (port code) in which the FTZ is located.
		/// </summary>
		[MessageBlockString(4, 21, "M")]
		public ZString PortCode;

		/// <summary>
		/// N = Not Direct Delivery
		/// Y = Direct Delivery 
		/// 
		/// A delivery into the FTZ of an admission traveling on an in-bond movement.
		/// </summary>
		[MessageBlockString(1, 25, "C")]
		public ZString DirectDeliveryIndicator;
	}

	[OutputBlock("90", "01")]
	public partial class FTZNF90_01 : MessageBlock
	{
		public FTZNF90_01()
			: base("90")
		{
		}

		/// <summary>
		/// A code representing the Admission Type. Refer to Record Identifier FT20, Note 1 of this chapter for valid codes.
		/// Space filled for responses to FZ messages that do not contain an Admission Number.
		/// </summary>
		[MessageBlockString(1, 3, "C")]
		public ZString AdmissionType;

		/// <summary>
		/// The Zone ID is composed of the following:
		/// Legacy 7-character Zone ID*
		/// FTZ ID = 3 numeric
		/// Sub zone/GP ID = 2 alpha-numeric Site ID = 2 numeric
		/// *Left Justify
		/// Updated ACE 9-character Zone ID
		/// FTZ ID = 3 numeric
		/// Sub zone/GP ID = 3 alpha-numeric Site ID = 3 alpha-numeric
		/// Space filled for responses to FZ messages that do not contain an Admission Number.
		/// </summary>
		[MessageBlockString(9, 4, "C")]
		public ZString ZoneID;

		/// <summary>
		/// Calendar year in YY (year) format.
		/// '00' for responses to FZ messages that do not contain an Admission Number.
		/// </summary>
		[MessageBlockInt(2, 13, "M")]
		public ZInt CalendarYear;

		/// <summary>
		/// Free form number governing the admission. The Zone ID + year + control number = the standardized admission number.
		/// Space filled for responses to FZ messages that do not contain an Admission Number.
		/// </summary>
		[MessageBlockString(8, 15, "C")]
		public ZString ControlNumber;

		/// <summary>
		/// Schedule D Code (port code) in which the FTZ is located.
		/// '0' for responses to FZ messages that do not contain an Admission Number.
		/// </summary>
		[MessageBlockString(4, 23, "M")]
		public ZString PortCode;

		/// <summary>
		/// A code indicating whether direct delivery is involved
		/// 
		/// N	Not Direct Delivery
		/// Y	Direct Delivery
		/// Space filled for responses to FZ messages that do not contain an Admission Number.
		/// </summary>
		[MessageBlockString(1, 27, "C")]
		public ZString DirectDeliveryIndicator;
	}
}
