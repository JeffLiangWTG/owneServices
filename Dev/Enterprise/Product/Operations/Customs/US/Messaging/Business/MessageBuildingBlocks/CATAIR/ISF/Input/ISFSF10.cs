namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[InputBlock("SF10")]
	public partial class ISFSF10 : MessageBlock
	{
		public ISFSF10()
			: base("SF10")
		{
		}

		/// <summary>
		/// Please see note 1 for appropriate ISF Submission Type Code.
		/// </summary>
		[MessageBlockString(1, 5, "M")]
		public ZString ISFSubmissionType;

		/// <summary>
		/// Code identifying the type of shipment being submitted. 01 must be used for ISF-5.
		/// </summary>
		[MessageBlockString(2, 6, "M", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString ShipmentTypeCode;

		/// <summary>
		/// A = Add, D = Delete, R = Replace
		/// </summary>
		[MessageBlockString(1, 8, "M")]
		public ZString ActionCode;

		/// <summary>
		/// Valid codes are:
		/// 
		/// CT = Complete Transaction
		/// FR = Flexible Range
		/// FT = Flexible Timing
		/// FX = Flexible Range and Flexible Timing
		/// </summary>
		[MessageBlockString(2, 9, "C")]
		public ZString ActionReasonCode;

		/// <summary>
		/// Qualifier denoting the type of data provided in the ISF Importer Number field.
		/// </summary>
		[MessageBlockString(3, 11, "M")]
		public ZString ISFImporterNumberQualifier;

		/// <summary>
		/// ISF Importer Number
		/// </summary>
		[MessageBlockString(15, 14, "M", IsPersonalInformation = true)]
		public ZString ISFImporterNumber;

		/// <summary>
		/// Required if ISF Importer Number Qualifier = ‘AEF’ (Passport Number) or ‘34’ (SSN). Valid format for date of birth is MMDDYYYY (month, day, year).
		/// </summary>
		[MessageBlockDate(29, "C", "MMddyyyy")] // typo in spec
		public ZDate DateOfBirth;

		/// <summary>
		/// Ocean vessel non-containerized = ‘10’ (Break Bulk)
		/// Ocean vessel containerized = ‘11’
		/// </summary>
		[MessageBlockString(2, 37, "O")]
		public ZString ModeOfTransportationCode;

		/// <summary>
		/// Unique transaction identifier assigned by CBP
		/// </summary>
		[MessageBlockString(15, 39, "C")]
		public ZString ISFTransactionNumber;

		/// <summary>
		/// Standard Carrier Alpha Code
		/// </summary>
		[MessageBlockString(4, 54, "O")]
		public ZString SCACIdentifier;

		/// <summary>
		/// Identification number of the party whose bond is being obligated for the ISF.
		/// </summary>
		[MessageBlockString(15, 58, "C", IsPersonalInformation = true)]
		public ZString BondHolder;

		/// <summary>
		/// CBP code identifying the activity of the bond being used.
		/// </summary>
		[MessageBlockString(2, 73, "C")]
		public ZString BondActivityCode;

		/// <summary>
		/// CBP code identifying the bond type
		/// </summary>
		[MessageBlockString(1, 75, "C")] // string
		public ZString BondType;

		/// <summary>
		/// 2-position ISO Country Code (ISO 3166-1) of country where passport was issued; Required if ISF Importer Number Qualifier = ‘AEF’ (Passport Number)
		/// </summary>
		[MessageBlockString(2, 79, "C")]
		public ZString CountryOfIssuance;
	}
}
