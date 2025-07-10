namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	using CargoWise.Types;

	[InputBlock("A01")]
	[OutputBlock("A01")]
	public partial class INPA01 : MessageBlock
	{
		public INPA01()
			: base("A01")
		{
		}

		/// <summary>
		/// A SCAC representing the automated importing carrier/MVOCC/NVOCC.
		/// </summary>
		[MessageBlockString(4, 4, "M")]
		public ZString CarrierCode;

		/// <summary>
		/// A code representing the CBP port of lading/unlading. Use Census Schedule D in CAMIR Appendix E for valid port codes.
		/// </summary>
		[MessageBlockString(4, 8, "M")]
		public ZString CBPPort;

		/// <summary>
		/// A code representing the CBP type of manifest amendment. Valid codes are:
		/// 
		/// A = Add a bill of lading
		/// D = Delete a bill of lading
		/// R = Replace the existing manifest quantity with new manifest quantity
		/// </summary>
		[MessageBlockString(1, 12, "M")]
		public ZString ActionCode;

		/// <summary>
		/// The bill of lading sequence number. Do not include the issuer code as it is contained in the associated J01 record.
		/// </summary>
		[MessageBlockString(12, 13, "M")]
		public ZString BillOfLadingSequenceNumber;

		/// <summary>
		/// Required if the action code is R on this record. Provide the new bill of lading quantity.
		/// </summary>
		[MessageBlockDecimal(10, 25, "C", 0)]
		public ZDecimal Quantity;

		/// <summary>
		/// A code representing the reason(s) for the amendment of the manifest record. See CAMIR Appendix C for valid Amendment codes.
		/// </summary>
		[MessageBlockString(2, 35, "M")] // need to be string
		public ZString AmendmentCode;
	}
}
