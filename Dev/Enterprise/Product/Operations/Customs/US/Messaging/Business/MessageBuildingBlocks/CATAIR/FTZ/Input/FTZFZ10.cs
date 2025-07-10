namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[InputBlock("10")]
	public partial class FTZFZ10 : MessageBlock
	{
		public FTZFZ10()
			: base("10")
		{
		}

		/// <summary>
		/// Qualifier defining Action Being Taken:
		/// 1	Action taken at FTZ Admission Number level. Admission Number follows beginning in position 4.
		/// 2	Bill of Lading number action taken. BOL begins in position 4.
		/// 3	In-Bond number action taken. In- bond number begins in position 4.
		/// 4	Container number action taken. BOL number begins in position 4.
		/// </summary>
		[MessageBlockInt(1, 3, "M")]
		public ZInt ActionQualifier;

		/// <summary>
		/// Min 9, Max 35. Identification Number corresponds to code in position 3 of this record: FTZ number is 19 positions starting in position 4. If ocean or rail mode of transport, the B/L is a min of 5 positions, max of 16 positions with the first 4 positions being only alpha. If air cargo, the bill is a min of 11 AN and a max of 23 if a house air waybill is concatenated (11 master + up to 12 characters for the house). Since motor cargo is mostly not automated at this time, the formatting is open, left justified.
		/// </summary>
		[MessageBlockString(35, 4, "M")]
		public ZString IdentificationNumber;

		/// <summary>
		/// An alpha code describing the event this record is reporting: PTT, Operator Acceptance/Refusal, [FTZ] Arrival or Operator Concurrence/Post Admission Correction.
		/// </summary>
		[MessageBlockString(2, 39, "M")]
		public ZString ActionCode;

		/// <summary>
		/// For concurrences (FZ Action Code = A,B,C, or D), enter the quantity received in whole numbers.
		/// For PTT requests (FZ Action Code = F), enter the quantity being moved in whole numbers.
		/// 
		/// Leading zeros, right justify.
		/// Mandatory if FZ10 Action Code = A, B, C, D, or F (positions 39-40).
		/// 
		/// Space fill if not used or if FZ10 Action Code = K
		/// </summary>
		[MessageBlockDecimal(10, 41, "C", 0)]
		public ZDecimal ReceivedQuantity;

		/// <summary>
		/// Valid Delivery Codes for reporting are:
		/// PA	Partial manifest reported
		/// FI	Final manifest portion reported
		/// Mandatory if FZ10 Action Code = A Space fill if not used.
		/// </summary>
		[MessageBlockString(2, 51, "C")]
		public ZString DeliveryCode;

		/// <summary>
		/// Importer of Record Number identifier of bonded carrier responsible for custodial Permit To Transfer within port movement.
		/// The IOR number identifier is used by CBP to validate the specified bonded carrier has an active type 2 bond and is authorized for bonded cargo movements.
		/// Exception: If the FTZ Operator (IOR number identifier) reported in the FT10 Record is the specified bonded carrier, then he can be authorized for the PTT movement to the FTZ Site under his active type 4 FTZ bond.
		/// Mandatory if FZ10 Action code = F (positions 39-40).
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(12, 53, "C")]
		public ZString IRSIdentifierBondedCarrier;

		/// <summary>
		/// FIRMS Code of the destination location for PTT move.
		/// Note: The FIRMS Code must be for an active FTZ Site facility, with an active Activity type 4 FTZ bond, and be located in the same Port, or port cluster, as the originating bonded location of the cargo.
		/// Mandatory if FZ10 Action code = F (positions 39-40).
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(4, 65, "C")]
		public ZString FIRMS;

		/// <summary>
		/// IATA 3-Letter Airport Code (Example: Reagan National = DCA).
		/// Mandatory if Bill of Lading Mode of Transportation = Air
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(3, 69, "C")]
		public ZString AirportCode;
	}
}
