namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[OutputBlock("JN")]
	public partial class AENQJN : MessageBlock
	{
		public AENQJN()
			: base("JN")
		{
		}

		/// <summary>
		/// A code representing the surety.
		/// </summary>
		[MessageBlockString(3, 3, "C")]
		public ZString SuretyCode;

		/// <summary>
		/// A code indicating whether a surety is designated as the primary surety.Valid codes are:
		/// 
		/// Y = Primary Surety
		/// N = Non-Primary Surety
		/// </summary>
		[MessageBlockString(1, 6, "C")]
		public ZString PrimarySuretyIndicator;

		/// <summary>
		/// The date the bill first appeared on an the CBP Report 612 Formal Demand on Surety.
		/// </summary>
		[MessageBlockDate(7, "C", "MMddyy")]
		public ZDate ReportDate612;

		/// <summary>
		/// A code assigned to the bill.
		/// </summary>
		[MessageBlockString(11, 13, "M")]
		public ZString BillNumber;

		/// <summary>
		/// A numeric date in MMDDYY format representing the date of the bill.
		/// </summary>
		[MessageBlockDate(24, "M", "MMddyy")]
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
		[MessageBlockString(1, 30, "M")]
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
		[MessageBlockString(2, 31, "M")]
		public ZString BillCollectionStatus;

		/// <summary>
		/// A value representing the total bill amount. Two decimal places are implied.
		/// If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 33, "M", 2)]
		public ZDecimal TotalBillAmount;

		/// <summary>
		/// A value representing the amount paid. Two decimal places are implied.
		/// If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 44, "C", 2)]
		public ZDecimal PaidAmount;

		/// <summary>
		/// A value representing the principal amount.Two decimal places are implied.
		/// If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 55, "C", 2)]
		public ZDecimal PrincipalAmount;

		/// <summary>
		/// A value representing the interest amount. Two decimal places are implied.
		/// If the number is a whole number, the two low-order (cents) positions contain zeros
		/// </summary>
		[MessageBlockDecimal(11, 66, "C", 2)]
		public ZDecimal InterestAmount;
	}

	[OutputBlock("JN", "01")]
	public partial class AENQJN_01 : MessageBlock
	{
		public AENQJN_01()
			: base("JN")
		{
		}

		/// <summary>
		/// A code representing the surety.
		/// </summary>
		[MessageBlockString(3, 4, "C")]
		public ZString SuretyCode;

		/// <summary>
		/// A code indicating whether a surety is designated as the primary surety.Valid codes are:
		/// 
		/// Y = Primary Surety
		/// N = Non-Primary Surety
		/// </summary>
		[MessageBlockString(1, 8, "C")]
		public ZString PrimarySuretyIndicator;

		[MessageBlockString(24, 10, "M")]
		public ZString NarrativeText;
	}
}
