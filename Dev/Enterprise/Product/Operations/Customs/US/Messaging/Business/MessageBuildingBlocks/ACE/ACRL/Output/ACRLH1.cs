namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[OutputBlock("H1")]
	public partial class ACRLH1 : MessageBlock
	{
		public ACRLH1()
			: base("H1")
		{
		}

		/// <summary>
		/// ACE AE data element: Update Action Code.
		/// A code representing the update action.
		/// </summary>
		[MessageBlockString(1, 3, "M")]
		public ZString UpdateActionCode;

		/// <summary>
		/// ACE AE data element: District/Port of Entry.
		/// The code for the U.S. port that the merchandise is entered.
		/// </summary>
		[MessageBlockString(4, 4, "M")]
		public ZString DistrictPortOfEntry;

		/// <summary>
		/// ACE AE data element: Entry Filer Code.
		/// Entry Filer's identification code (as assigned by CBP).
		/// </summary>
		[MessageBlockString(3, 8, "M")]
		public ZString EntryFilerCode;

		/// <summary>
		/// ACE AE data element: Entry Number.
		/// Unique identifying number assigned to the Entry by the Filer.
		/// </summary>
		[MessageBlockString(9, 11, "M")] // Justification removed by Svit in WI00054313
		public ZString EntryNumber;

		/// <summary>
		/// ACE AE data element: Importer of Record Number.
		/// A code representing the importer of record.
		/// </summary>
		[MessageBlockString(12, 20, "M")]
		public ZString ImporterNumber;

		/// <summary>
		/// ACE AE data element: Mode of Transportation (MOT) Code.
		/// A code identifying the mode of transportation by which the merchandise entered the U.S. port of arrival from the last foreign country.
		/// </summary>
		[MessageBlockString(2, 32, "C")]
		public ZString ModeOfTransportationMOTCode;

		/// <summary>
		/// ACE AE data element: Estimated Date of Arrival.
		/// A numeric date in MMDDYY (month, day, and year) format representing the estimated date of arrival.
		/// </summary>
		[MessageBlockDate(34, "M", "MMddyy")]
		public ZDate EstimatedDateOfArrival;

		/// <summary>
		/// ACE AE data element: Bond Type Code.
		/// A code representing the bond type.
		/// </summary>
		[MessageBlockString(1, 40, "M")]
		public ZString BondTypeCode;

		/// <summary>
		/// ACE AE data element: Cargo Release Certification Request Indicator.
		/// A code of Y - the entry summary is being certified for cargo release processing.
		/// </summary>
		[MessageBlockString(1, 41, "M")]
		public ZString ReleaseCertificationCode;

		/// <summary>
		/// This field is not being used by ACE AE.
		/// Space fill.
		/// SVA: now this field is used, example from client: H1A3901OHL 0312722603-0593691004012271381        131225     KL  390101001  1    
		/// </summary>
		[MessageBlockDate(50, "M", "yyMMdd")] // different format
		public ZDate PresentationDate;

		/// <summary>
		/// ACE AE data element: Vessel Code.
		/// The first five positions of the Lloyd's code identifier of the importing vessel.
		/// </summary>
		[MessageBlockString(5, 56, "C")]
		public ZString ImportingVesselCode;

		/// <summary>
		/// ACE AE data element: Carrier Code.
		/// The identification of the entity responsible for transporting the merchandise from the foreign port of lading to the first U.S. port of unlading. For vessel, rail or truck shipments: the SCAC. For air shipments: the IATA code.
		/// </summary>
		[MessageBlockString(4, 61, "C")]
		public ZString CarrierCode;

		/// <summary>
		/// ACE AE data element: District/Port of Unlading.
		/// The code for the U.S. port where the merchandise is unladed from the importing conveyance.
		/// </summary>
		[MessageBlockString(4, 65, "C")]
		public ZString DistrictPortOfUnlading;

		/// <summary>
		/// ACE AE data element: Entry Type Code.
		/// A code representing the entry type.
		/// </summary>
		[MessageBlockString(2, 69, "M")]
		public ZString EntryType;

		/// <summary>
		/// ACE AE data element: Surety Code.
		/// Identification of the Surety company that has underwritten the bond liability for the entry or a specific bond waiver reason.
		/// </summary>
		[MessageBlockString(3, 71, "C")]
		public ZString SuretyCode;

		/// <summary>
		/// This field is not being used by ACE AE.
		/// Space fill.
		/// </summary>
		[MessageBlockString(1, 74, "M")]
		public ZString OtherGovernmentAgencyOGACodes;

		/// <summary>
		/// This field is not being used by ACE AE.
		/// Space fill.
		/// </summary>
		[MessageBlockString(1, 75, "M")]
		public ZString ConsigneeNameAndAddress;
	}
}
