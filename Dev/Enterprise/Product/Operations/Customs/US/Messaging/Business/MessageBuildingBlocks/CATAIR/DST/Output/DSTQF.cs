namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[OutputBlock("QF")]
	public partial class DSTQF : MessageBlock
	{
		public DSTQF()
			: base("QF")
		{
		}

		/// <summary>
		/// A code representing the district/port which processed the entries.
		/// </summary>
		[MessageBlockString(4, 3, "M")]
		public ZString DistrictPortWhichProcessesEntries;

		/// <summary>
		/// A unique code assigned by CBP to all active entry document preparers.
		/// </summary>
		[MessageBlockString(3, 7, "M")]
		public ZString EntryFilerCode;

		/// <summary>
		/// A value representing the total Raspberry fee amount. Two decimal places are implied. If the fee is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(9, 10, "C", 2)]
		public ZDecimal TotalRaspberryFee;

		/// <summary>
		/// A value representing the total potato fee amount. Two decimal places are implied. If the fee is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(9, 19, "C", 2)]
		public ZDecimal TotalPotatoFee;

		/// <summary>
		/// A value representing the total lime fee amount. Two decimal places are implied. If the fee is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(9, 28, "C", 2)]
		public ZDecimal TotalLimeFee;

		/// <summary>
		/// A value representing the total mushroom fee amount. Two decimal places are implied. If the fee is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(9, 37, "C", 2)]
		public ZDecimal TotalMushroomFee;

		/// <summary>
		/// A value representing the total watermelon fee. Two decimal places are implied. If the fee is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(9, 46, "C", 2)]
		public ZDecimal TotalWatermelonFee;

		/// <summary>
		/// A value representing the total sheep fee. Two decimal places are implied. If the fee is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(9, 55, "C", 2)]
		public ZDecimal TotalSheepFee;

		/// <summary>
		/// A value representing the total blueberry fee amount. Two decimal places are implied. If the fee is a whole number, the two low order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(9, 64, "C", 2)]
		public ZDecimal TotalBlueberryFee;
	}
}
