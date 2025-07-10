namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("SE11")]
	public abstract partial class ASESE11 : MessageBlock // Need to add interface for BIRD System
	{
		protected ASESE11()
			: base("SE11")
		{
		}

		/// <summary>
		/// A code representing the source of the Elected Entry Date. Valid codes are: 
		/// 
		/// P = Date of Presentation
		/// A = Date of Arrival
		/// W = Weekly Entry (Entry type 06, only)
		/// 
		/// Codes "P" and "A" are not valid for entry type 06.
		/// </summary>
		[MessageBlockString(1, 5, "C")]
		public ZString EntryDateElectionCode;

		/// <summary>
		/// Enter a numeric date in MMDDYY (month, day, year) format.
		/// 
		/// The date submitted in this field will be used as the Date of Entry for Entry Summary and duty calculation purposes.
		/// 
		/// For entry type 06, and an Entry Date Election Code = "W" (Weekly Entry), this represents the date of the first day of the Zone Week.
		/// </summary>
		[MessageBlockDate(6, "C", "MMddyy")]
		public ZDate ElectedEntryDate;

		/// <summary>
		/// FIRMS code of the location where the cargo is currently stored.
		/// </summary>
		[MessageBlockString(4, 12, "C")]
		public ZString LocationOfGoodsFIRMS;

		/// <summary>
		/// Filer's preferred CES location if cargo needs to be examined.
		/// </summary>
		[MessageBlockString(4, 16, "O")]
		public ZString ElectedExamSiteFIRMS;

		/// <summary>
		/// Name of the Conveyance.
		/// 
		/// For entry type 06, list the Foreign Trade Zone 'Zone ID'. The data must be submitted in the format specified in Note 6*.
		/// 
		/// *NOTE: Please refer to Note 6 below for the proper format of the FTZ Zone ID.
		/// </summary>
		[MessageBlockString(20, 20, "C", OnLengthViolation = LengthViolationAction.Substring)]
		public ZString ConveyanceNameOrFTZZoneID;

		/// <summary>
		/// The voyage/flight/trip number of the importing carrier.
		/// </summary>
		[MessageBlockString(5, 40, "C")]
		public ZString VoyageFlightTripManifestNumber;

		/// <summary>
		/// The G.O. Number if the cargo has been placed in General Order.
		/// </summary>
		[MessageBlockString(20, 45, "O")]
		public ZString GeneralOrderGONumber;

		/// <summary>
		/// FIRMS code of the CBP Bonded Warehouse where cargo is to be entered. (Entry Types 21 and 22 only)
		/// </summary>
		[MessageBlockString(4, 65, "C")]
		public ZString CBPBondedWarehouseFIRMS;

		/// <summary>
		/// Filer Code of the originating warehouse entry that the merchandise is coming from. The Originating entry listed must be entry type 21 or 22 only.
		/// 
		/// The Entry Filer Code occupies the first three positions of an entry number.
		/// </summary>
		[MessageBlockString(3, 69, "C")]
		public ZString OriginatingWarehouseEntryFilerCode;

		/// <summary>
		/// Originating warehouse entry number that the merchandise is coming from. The Originating entry listed must be entry type 21 or 22 only.
		/// 
		/// Unique identifying number assigned to the Entry by the Filer. For additional information on valid entry number formats, refer to Appendix B of the CATAIR publication.
		/// </summary>
		[MessageBlockString(8, 72, "C")]
		public ZString OriginatingWarehouseEntryNumber;

		/// <summary>
		/// Enter 'Y' if requesting to use Immediate Delivery Procedures.
		/// 
		/// This field is not related to special procedures for Split Shipments. Do not use this field if the sole purpose is to take advantage of the special split shipment procedures.
		/// 
		/// If this field is not applicable, Space fill.
		/// </summary>
		[MessageBlockString(1, 80, "C")]
		public ZString ImmediateDeliveryIndicator;
	}
}
