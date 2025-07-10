namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[InputBlock("10")]
	public partial class FTZFT10 : MessageBlock
	{
		public FTZFT10()
			: base("10")
		{
		}

		/// <summary>
		/// A code representing the action to be taken. Valid action codes are:
		/// 
		/// A = Add
		/// D = Delete
		/// R = Replace
		/// 
		/// CBP will send electronic record of any changed items to Census.
		/// </summary>
		[MessageBlockString(1, 3, "M")]
		public ZString ActionCode;

		/// <summary>
		/// The Zone ID is composed of the following:
		/// 
		/// FTZ ID = 3 numeric
		/// Sub zone ID = 2 Alpha-numeric 
		/// Site ID = 2 numeric
		/// </summary>
		[MessageBlockString(7, 4, "M")]
		public ZString ZoneID;

		/// <summary>
		/// Calendar year in YY (year) format.
		/// </summary>
		[MessageBlockInt(2, 11, "M")]
		public ZInt CalendarYear;

		/// <summary>
		/// Open format, controlled by Applicant. The zone ID + calendar year + control number = the standardized admission number. Must be a minimum of 2 characters.
		/// </summary>
		[MessageBlockString(8, 13, "M")]
		public ZString ControlNumber;

		/// <summary>
		/// U.S Census Schedule D Code (port code) in which the FTZ is located.
		/// </summary>
		[MessageBlockString(4, 21, "M")]
		public ZString PortCode;

		/// <summary>
		/// N = Not Direct Delivery
		/// Y = Direct Delivery
		/// </summary>
		[MessageBlockString(1, 25, "M")]
		public ZString DirectDeliveryIndicator;

		/// <summary>
		/// Filer code of submitter/filer transmitting the data.
		/// </summary>
		[MessageBlockString(3, 26, "M")]
		public ZString ABIFilerCode;

		/// <summary>
		/// ABI Routing code and optional office extension if filer of e214 wants copy sent to another ABI participant via this routing information (broker download). CBP will send electronic copy of FT data set to designated ABI participant following error free acceptance of FT input. The format will be DDPPFLROF (district port code, filer code, office code).
		/// </summary>
		[MessageBlockString(9, 29, "O")]
		public ZString ABIRoutingCode;

		/// <summary>
		/// Importer of Record number identifier of Zone Operator Note: This entity must have an active type 4 FTZ Bond. If this entity is also the Applicant for Admission, then the Applicant does not need to be declared in the FT60 record.
		/// </summary>
		[MessageBlockString(12, 38, "M")]
		public ZString ZoneOperatorIdentifierformerlyIRSIdentifier;
	}

	[InputBlock("10", "01")]
	public partial class FTZFT10_01 : MessageBlock
	{
		public FTZFT10_01()
			: base("10")
		{
		}

		/// <summary>
		/// A code representing the action to be taken. Valid action codes are:
		/// A	Add
		/// D	Delete
		/// R	Replace
		/// S	Merchandise Zone Status Change
		/// 
		/// CBP will send electronic record of any changed items to Census.
		/// </summary>
		[MessageBlockString(1, 3, "M")]
		public ZString ActionCode;

		/// <summary>
		/// The Zone ID is composed of the following:
		/// Legacy 7-character Zone ID*
		/// FTZ ID = 3 numeric
		/// Sub zone/GP ID = 2 alpha-numeric
		/// Site ID = 2 numeric
		/// *Left Justify
		/// Updated ACE 9-character Zone ID
		/// FTZ ID = 3 numeric
		/// Sub zone/GP ID = 3 alpha-numeric
		/// Site ID = 3 alpha-numeric
		/// </summary>
		[MessageBlockString(9, 4, "M")]
		public ZString ZoneID;

		/// <summary>
		/// Calendar year in YY (year) format.
		/// </summary>
		[MessageBlockInt(2, 13, "M")]
		public ZInt CalendarYear;

		/// <summary>
		/// Open format, controlled by Applicant.
		/// The zone ID + calendar year + control number = the standardized admission number.
		/// Must be a unique and sequential number, minimum of 2 characters.
		/// </summary>
		[MessageBlockString(8, 15, "M")]
		public ZString ControlNumber;

		/// <summary>
		/// A code indicating whether an expanded Zone ID is being provided in position 4-12.
		/// 
		/// Y	9 character Zone ID included
		/// N	7 character Zone ID included
		/// </summary>
		[MessageBlockString(1, 23, "M")]
		public ZString ExpandedZoneIDIndicator;

		/// <summary>
		/// U.S Census Schedule D Code (port code) in which the FTZ is located.
		/// </summary>
		[MessageBlockString(4, 24, "M")]
		public ZString PortCode;

		/// <summary>
		/// A code indicating a delivery into the FTZ of an admission traveling on an in-bond movement.
		/// 
		/// N	Not Direct Delivery
		/// Y	Direct Delivery
		/// </summary>
		[MessageBlockString(1, 28, "M")]
		public ZString DirectDeliveryIndicator;

		/// <summary>
		/// Filer code of submitter/filer transmitting the data.
		/// </summary>
		[MessageBlockString(3, 29, "M")]
		public ZString ABIFilerCode;

		/// <summary>
		/// ABI Routing code and optional office extension if filer of e214 wants copy sent to another ABI participant via this routing information (broker download). CBP will send electronic copy of FT data set to designated ABI participant following error free acceptance of FT input. The format will be DDPPFLROF (district port code, filer code, office code).
		/// </summary>
		[MessageBlockString(9, 32, "O")]
		public ZString ABIRoutingCode;

		/// <summary>
		/// Importer of Record number identifier of Zone Operator. Format is NN- NNNNNNNNN
		/// Note: This entity must have an active type 4 FTZ Bond.
		/// If this entity is also the Applicant for Admission, then the Applicant does not need to be declared in position 57-68 of this record.
		/// </summary>
		[MessageBlockString(12, 41, "M", IsPersonalInformation = true)]
		public ZString ZoneOperatorIdentifierformerlyIRSIdentifier;

		/// <summary>
		/// FIRMS Code representing the Admission FTZ Site location.
		/// </summary>
		[MessageBlockString(4, 53, "M")]
		public ZString FIRMSIdentifier;

		/// <summary>
		/// Importer of Record ID of the applicant for admission.
		/// This data is mandatory if the Applicant for Admission is different from the FTZ Zone Operator (i.e. the two entities have different Importer of Record IDs)
		/// Space fill if Zone Operator ID and Applicant for Admission are the same.
		/// </summary>
		[MessageBlockString(12, 57, "C", IsPersonalInformation = true)]
		public ZString ApplicantForAdmission;
	}
}
