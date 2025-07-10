namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("20")]
	public abstract partial class ENS20 : MessageBlock // Need to add interface for BIRD System
	{
		public ENS20()
			: base("20")
		{
		}

		/// <summary>
		/// A code representing the Bureau of the Census Importing Vessel Code as provided by the carrier. This code appears on the bill of lading. If there is no Importing Vessel Code, space fill.
		/// </summary>
		[MessageBlockString(5, 3, "C")]
		public ZString ImportingVesselCode;

		/// <summary>
		/// If the mode of transportation is 10 (vessel, non-container) or 11 (vessel, container), the vessel name is mandatory. If both vessel name and vessel code is entered, both are accepted. If the entry type code is 06, the first three characters must be “FTZ”, followed by any number in the range of “001” through “300”, inclusive. No number outside this range will be accepted. The last character can be any alpha (A-Z) or numeric (0-9) character. For a description of the mode of transportation codes, refer to Appendix B of this publication.
		/// </summary>
		[MessageBlockString(20, 8, "C")]
		public ZString ImportingVesselName;

		/// <summary>
		/// A code representing the mode of transportation. This code is mandatory if the entry/entry summary data is a formal entry or is being certified for cargo release processing. Refer to Appendix B of this publication for valid mode of transportation codes. For merchandise entering the U.S. from a foreign trade zone, do not transmit the mode of transportation.
		/// </summary>
		[MessageBlockString(2, 28, "C")]
		public ZString ModeOfTransportationMOTCode;

		/// <summary>
		/// A code representing the CBP district/port of unlading. For merchandise arriving in the U.S. by means of transportation other than vessel or air, space fill. For merchandise arriving in the U.S. from a Foreign Trade Zone (FTZ), space fill. Valid district/port codes can be queried through the Extract Reference File chapter of this publication.
		/// </summary>
		[MessageBlockString(4, 30, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString DistrictPortOfUnlading;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the date of importation at the first U.S. port of unlading. This date is mandatory for warehouse entry types 21, 22, and TIB entry type 23. It is also mandatory for entry types 01, 02 and 06 if entry is being flagged for FTA reconciliation.
		/// </summary>
		[MessageBlockDate(34, "C", "MMddyy")]
		public ZDate DateOfImportation;

		/// <summary>
		/// This field is provided for the convenience of the user. An optional code provided by the participant, it is not edited or changed during ACS processing. It is for internal user system control in entry summary processing. The Broker Reference Number is mandatory when the Update Action Code A is used to replace previously entered entry summary data and must match exactly the Broker Reference Number as originally transmitted. ACS replaces data when the entry numbers match the entry number previously transmitted to ACS. Refer to Record Identifier 10 for additional information on update action codes.
		/// </summary>
		[MessageBlockString(9, 40, "O")]
		public ZString BrokerReferenceNumber;

		/// <summary>
		/// A code that allows a filer to designate separate statements for individual branches within the same port code. This field is optional for filers using statement processing. This code is mandatory for statement filers using a Client Branch Designation. Entry summaries with the same processing district/port code, preliminary statement print date, payment type indicator, and client branch designation code appear on the same statement. This field is only edited for format. Use of this field at a port requires prior coordination with the CBP Client Representative.
		/// </summary>
		[MessageBlockString(2, 49, "C")]
		public ZString ClientBranchDesignation;

		/// <summary>
		/// The voyage/trip/manifest number of the importing carrier is mandatory if the mode of transportation code is 10 or 11 (vessel), 40, or 41 (air). If the mode of transportation code is 40 or 41 (air), enter the flight number. If the mode of transportation code is 10 or 11 (vessel), enter the complete voyage number (e.g., V311W). In the future, this field may be required if the automated manifest interfaces are established between CBP and rail/truck (road) carriers. For a description of the mode of transportation codes, refer to Appendix B of this publication.
		/// </summary>
		[MessageBlockString(5, 61, "C")]
		public ZString VoyageFlightTripManifestNumber;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the estimated date of arrival of the goods at the intended port of entry. The date is mandatory if certifying from entry summary.
		/// </summary>
		[MessageBlockDate(66, "C", "MMddyy")]
		public ZDate EstimatedDateOfArrival;

		/// <summary>
		/// This data element is mandatory for entry types 06 (Consumption, Foreign Trade Zone), 21 through 38 (warehouse category), 01 through 07 (entry summary (live)), or an entry certified for cargo release processing. The Location of Goods Code is a Facilities Information and Resources Management System (FIRMS) code and it is provided by the CBP. This code is a 4-position identification number.
		/// </summary>
		[MessageBlockString(4, 72, "C")]
		public ZString LocationOfGoods;

		/// <summary>
		/// Space - No trade agreement reconciliation
		/// 
		/// 1 = 19 USC 1520(d) reconciliations for certain eligible trade agreements
		/// </summary>
		[MessageBlockString(1, 76, "O")]
		public ZString TradeAgreementReconciliationIndicator;

		/// <summary>
		/// 001 = Value Issue
		/// 002 = Class Issue
		/// 003 = 9802 Issue
		/// 004 = Value - Class Issue
		/// 005 = Value - 9802 Issue
		/// 006 = Class - 9802 Issue
		/// 007 = Value - Class - 9802 Issue
		/// Space = No Reconciliation
		/// </summary>
		[MessageBlockString(3, 77, "O")]
		public ZString OtherReconciliationIndicator;
	}
}
