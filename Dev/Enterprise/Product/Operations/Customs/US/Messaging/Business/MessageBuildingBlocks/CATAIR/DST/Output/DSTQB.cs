namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[OutputBlock("QB")]
	public partial class DSTQB : MessageBlock
	{
		public DSTQB()
			: base("QB")
		{
		}

		/// <summary>
		/// A code representing the district/port of entry summary.
		/// </summary>
		[MessageBlockString(4, 3, "M")]
		public ZString DistrictPortOfEntrySummary;

		/// <summary>
		/// A unique code assigned by CBP to all active entry document preparers.
		/// </summary>
		[MessageBlockString(3, 7, "M")]
		public ZString EntryFilerCode;

		/// <summary>
		/// The number assigned to the entry.
		/// </summary>
		[MessageBlockString(9, 10, "M", Justification = Justification.Right)]
		public ZString EntryNumber;

		/// <summary>
		/// A value representing the interest amount for reconciliation summary. Two decimal places are implied. If the interest is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(8, 19, "C", 2)]
		public ZDecimal InterestAmountForReconciliationSummary;

		/// <summary>
		/// A value representing the Raspberry fee amount. Two decimal places are implied. If the fee is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(8, 27, "C", 2)]
		public ZDecimal RaspberryFee;

		/// <summary>
		/// A value representing the potato fee. Two decimal places are implied. If the fee is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(8, 35, "C", 2)]
		public ZDecimal PotatoFee;

		/// <summary>
		/// A value representing the lime fee. Two decimal places are implied. If the fee is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(8, 43, "C", 2)]
		public ZDecimal LimeFee;

		/// <summary>
		/// A value representing the mushroom fee. Two decimal places are implied. If the fee is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(8, 51, "C", 2)]
		public ZDecimal MushroomFee;

		/// <summary>
		/// A value representing the watermelon fee. Two decimal places are implied. If the fee is a whole number, the two low-order (cents) positions must contain zeros.
		/// </summary>
		[MessageBlockDecimal(8, 59, "C", 2)]
		public ZDecimal WatermelonFee;

		/// <summary>
		/// A value representing the sheep fee. Two decimal places are implied. If the fee is a whole number, the two low-order (cents) positions must contain zeros.
		/// </summary>
		[MessageBlockDecimal(8, 67, "C", 2)]
		public ZDecimal SheepFee;
	}
}
