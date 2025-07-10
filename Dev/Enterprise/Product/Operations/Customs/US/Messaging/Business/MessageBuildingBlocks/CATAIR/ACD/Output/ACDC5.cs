namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[OutputBlock("C5")]
	public partial class ACDC5 : MessageBlock
	{
		public ACDC5()
			: base("C5")
		{
		}

		/// <summary>
		/// A code identifying the number assigned to the antidumping/countervailing duty case.
		/// </summary>
		[MessageBlockString(10, 3, "M")]
		public ZString CaseNumber;

		/// <summary>
		/// A code of 0 (zero) indicates no rate exists for the case; a code of 1 indicates a rate applies.
		/// </summary>
		[MessageBlockString(1, 13, "M")]
		public ZString RateIndicator;

		/// <summary>
		/// A value representing the deposit rate. Four decimal places are implied (e.g., if the deposit rate is 25%, 02500 is the rate returned).
		/// </summary>
		[MessageBlockString(5, 14, "C")]
		public ZString DepositRate1;//Zero and spaces have different meaning

		/// <summary>
		/// A value representing the deposit rate. Four decimal places are implied (e.g., if the deposit rate is 25%, 02500 is the rate returned).
		/// </summary>
		[MessageBlockDecimal(5, 19, "C", 4)]
		public ZDecimal DepositRate2;

		/// <summary>
		/// A value representing the deposit rate. Four decimal places are implied (e.g., if the deposit rate is 25%, 02500 is the rate returned).
		/// </summary>
		[MessageBlockDecimal(5, 24, "C", 4)]
		public ZDecimal DepositRate3;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the date the deposit rate is effective for entry into the United States.
		/// </summary>
		[MessageBlockDate(29, "C", "MMddyy")]
		public ZDate EffectiveEntryDate;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the date the deposit rate is effective for exported goods.
		/// </summary>
		[MessageBlockDate(35, "C", "MMddyy")]
		public ZDate EffectiveExportDate;

		/// <summary>
		/// A code representing the first bond/cash indicator.
		/// </summary>
		[MessageBlockString(1, 41, "M")]
		public ZString BondCashIndicator1;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the first (earliest) bond/cash date.
		/// </summary>
		[MessageBlockDate(42, "M", "MMddyy")]
		public ZDate BondCashDate1;

		/// <summary>
		/// A code representing the second bond/cash indicator
		/// </summary>
		[MessageBlockString(1, 48, "C")]
		public ZString BondCashIndicator2;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year format) representing the second bond/cash date.
		/// </summary>
		[MessageBlockDate(49, "C", "MMddyy")]
		public ZDate BondCashDate2;

		/// <summary>
		/// A code representing the third bond/cash indicator.
		/// </summary>
		[MessageBlockString(1, 55, "C")]
		public ZString BondCashIndicator3;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year format) representing the third bond/cash date.
		/// </summary>
		[MessageBlockDate(56, "C", "MMddyy")]
		public ZDate BondCashDate3;

		/// <summary>
		/// A code representing the fourth bond/cash indicator.
		/// </summary>
		[MessageBlockString(1, 62, "C")]
		public ZString BondCashIndicator4;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the fourth bond/cash date.
		/// </summary>
		[MessageBlockDate(63, "C", "MMddyy")]
		public ZDate BondCashDate4;

		/// <summary>
		/// A code representing the fifth bond/cash indicator.
		/// </summary>
		[MessageBlockString(1, 69, "C")]
		public ZString BondCashIndicator5;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the fifth bond/cash date.
		/// </summary>
		[MessageBlockDate(70, "C", "MMddyy")]
		public ZDate BondCashDate5;
	}
}
