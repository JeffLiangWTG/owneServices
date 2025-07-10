namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	// This message block has been removed from the latest spec but we still keep it here for old messages
	[OutputBlock("30")]
	public partial class FTZZD30 : MessageBlock
	{
		public FTZZD30()
			: base("30")
		{
		}

		/// <summary>
		/// SCAC of Transportation company moving goods.
		/// </summary>
		[MessageBlockString(4, 3, "M")]
		public ZString InBondCarrierCode;

		/// <summary>
		/// Minimum 9, Max 23 (Air Master + House). In-Bond control numbers vary in length from a minimum of nine-digits (9) for conventional CBPF-7512 in-bond movements (including QP and CAFES), eleven-positions (11) for Sea/Rail AMS and up to twenty-three (23) for Air AMS when master airway bill and house airway bill used for primary Air AMS in-bond movement. Subsequent In-Bond movements or ‘legs’ in Air AMS use the conventional nine-digit (9) in-bond identifier.
		/// </summary>
		[MessageBlockString(35, 7, "M")]
		public ZString ITNumber;

		/// <summary>
		/// Date In-Bond Movement Authorized. The date an in-bond request was approved by CBP.
		/// </summary>
		[MessageBlockInt(8, 42, "M")]
		public ZInt InBondDate;

		/// <summary>
		/// U.S. Census Schedule D Code of the U.S. CBP port where the in-bond movement started.
		/// </summary>
		[MessageBlockString(4, 50, "M")]
		public ZString OriginPortCode;

		/// <summary>
		/// In-Bond quantity – shipment quantity covered by this in-bond movement.
		/// </summary>
		[MessageBlockDecimal(10, 54, "O", 0)]
		public ZDecimal Quantity;
	}
}
