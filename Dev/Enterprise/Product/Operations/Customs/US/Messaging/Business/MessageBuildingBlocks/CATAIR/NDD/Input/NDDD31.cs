namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[InputBlock("D31")]
	public partial class NDDD31 : MessageBlock
	{
		public NDDD31()
			: base("D31")
		{
		}

		/// <summary>
		/// A value representing the total bonded antidumping duty.
		/// </summary>
		[MessageBlockDecimal(11, 4, "C", 2)] // 2 decimals
		public ZDecimal TotalBondedAntidumpingDuty;

		/// <summary>
		/// A value representing the total payable antidumping duty.
		/// </summary>
		[MessageBlockDecimal(11, 15, "C", 2)] // 2 decimals
		public ZDecimal TotalPayableAntidumpingDuty;

		/// <summary>
		/// A value representing the total bonded and payable antidumping duty.
		/// </summary>
		[MessageBlockDecimal(11, 26, "C", 2)] // 2 decimals
		public ZDecimal GrandTotalAntidumpingDuty;

		/// <summary>
		/// A value representing the total bonded countervailing duty.
		/// </summary>
		[MessageBlockDecimal(11, 37, "C", 2)] // 2 decimals
		public ZDecimal TotalBondedCountervailingDuty;

		/// <summary>
		/// A value representing the total payable countervailing duty.
		/// </summary>
		[MessageBlockDecimal(11, 48, "C", 2)] // 2 decimals
		public ZDecimal TotalPayableCountervailingDuty;

		/// <summary>
		/// A value representing the total bonded and payable countervailing duty.
		/// </summary>
		[MessageBlockDecimal(11, 59, "C", 2)] // 2 decimals
		public ZDecimal GrandTotalCountervailingDuty;
	}
}
