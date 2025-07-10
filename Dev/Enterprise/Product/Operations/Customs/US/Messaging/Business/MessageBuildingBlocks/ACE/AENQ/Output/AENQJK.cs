namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[OutputBlock("JK")]
	public partial class AENQJK : MessageBlock
	{
		public AENQJK()
			: base("JK")
		{
		}

		/// <summary>
		/// A code assigned to the bill.
		/// </summary>
		[MessageBlockString(11, 3, "M")]
		public ZString BillNumber;

		/// <summary>
		/// A numeric date in MMDDYY format representing the date of the bill.
		/// </summary>
		[MessageBlockDate(14, "M", "MMddyy")]
		public ZDate BillDate;

		/// <summary>
		/// A code representing the bill type. Valid codes are:
		///
		/// 1 = Deferred Tax
		/// 2 = Supplemental Duty
		/// 3 = Regional Supplemental Duty
		/// 4 = Miscellaneous
		/// 5 = Region Reimbursable
		/// 6 = System Reimbursable
		/// </summary>
		[MessageBlockString(1, 20, "M")]
		public ZString BillType;

		/// <summary>
		/// A code representing the bill collection status. Valid codes are:
		///
		/// 1 = Not Paid
		/// 2 = Authorized
		/// 3 = Transmitted
		/// 4 = Paid
		/// 5 = Partial Payment
		/// 6 = Canceled
		/// 7 = Void
		/// 8 = Admin Pay
		/// 9 = Write Off
		/// 10 = Canceled with Partial Payment
		/// 11 = Write-off with Partial Payment
		/// </summary>
		[MessageBlockString(2, 21, "M")]
		public ZString BillCollectionStatus;

		/// <summary>
		/// A value representing the total bill amount. Two decimal places are implied.
		/// If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 23, "M", 2)]
		public ZDecimal TotalBillAmount;

		/// <summary>
		/// A value representing the amount paid. Two decimal places are implied.
		/// If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 34, "C", 2)]
		public ZDecimal PaidAmount;

		/// <summary>
		/// A value representing the principal amount.Two decimal places are implied.
		/// If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 45, "C", 2)]
		public ZDecimal PrincipalAmount;

		/// <summary>
		/// A value representing the interest amount. Two decimal places are implied.
		/// If the number is a whole number, the two low-order (cents) positions contain zeros
		/// </summary>
		[MessageBlockDecimal(11, 56, "C", 2)]
		public ZDecimal InterestAmount;
	}

	[OutputBlock("JK", "01")]
	public partial class AENQJK_01 : MessageBlock
	{
		public AENQJK_01()
			: base("JK")
		{
		}

		[MessageBlockString(24, 4, "M")]
		public ZString NarrativeText;
	}
}
