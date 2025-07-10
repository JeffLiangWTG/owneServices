namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[OutputBlock("A")]
	public partial class AABIOutputA : MessageBlock
	{
		public AABIOutputA()
			: base("A")
		{
		}

		/// <summary>
		/// The CBP assigned code for the 'data processing' site/location of the recipient (i.e., both sender of the batch and recipient of the response).
		/// </summary>
		[MessageBlockString(4, 2, "M")]
		public ZString SenderReceiverSiteCode;

		/// <summary>
		/// Recipient's identification code (as assigned by CBP).
		/// </summary>
		[MessageBlockString(3, 6, "M")]
		public ZString SenderReceiverIDCode;

		/// <summary>
		/// Space fill.
		/// 
		/// 
		/// 
		/// A pre-established password used to authorize the transmitter of the data.
		/// </summary>
		[MessageBlockString(6, 9, "O")]
		public ZString FillerESARORCommunicationPasswordeMAN; // ACE ABI eMAN transactions (QT and WT) return the Communication Password in the output A-Record.

		/// <summary>
		/// For an A-Record returned in response to an input: transmitter's date of input batch transmission.
		/// 
		/// For ACE generated notification: the date that ACE prepared the notification batch for transmission.
		/// </summary>
		[MessageBlockDate(15, "C", "MMddyy")]
		public ZDate TransmissionDate;

		/// <summary>
		/// A code that identifies the type of transaction data within the batch.
		/// 
		/// Space will be returned if the batch is rejected.
		/// </summary>
		[MessageBlockString(2, 26, "C")]
		public ZString ApplicationIdentifierCode;

		/// <summary>
		/// A code agreed upon by the receiver and CBP representing a specific recipient 'office' (or sub-location).
		/// </summary>
		[MessageBlockString(2, 38, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)] // Length violation changed, no need to throw exception
		public ZString SenderReceiverOfficeCode;

		/// <summary>
		/// For an A-Record returned in response to an input: the exact value submitted in the input A-Record.
		/// 
		/// For ACE generated notification: always space fill.
		/// </summary>
		[MessageBlockString(21, 60, "C")]
		public ZString TransmittersUserDataText;
	}
}
