namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Abstract
{
	using CargoWise.Types;

	[OutputBlock("J6")]
	public abstract partial class ENQJ6 : MessageBlock // Need to add interface for BIRD System
	{
		public ENQJ6()
			: base("J6")
		{
		}

		/// <summary>
		/// A unique code assigned by CBP to all active entry document preparers. The Entry Filer Code occupies the first three positions of a CBP entry number regardless of where the entry is filed. The Entry Filer Code must be the same as the Entry Filer Code in the block control header record (Record Identifier B). If the filer is a NILS filer (National Importer Liquidation System), the filer code in positions 3-5 of the J6 record can be different from the filer code in the B record.
		/// </summary>
		[MessageBlockString(3, 3, "M")]
		public ZString BrokerNumberOrEntryFilerCode;

		/// <summary>
		/// The number assigned to the entry. For additional information on valid entry number formats, refer to Appendix E of this publication.
		/// </summary>
		[MessageBlockString(9, 6, "M", Justification = Justification.Right)]
		public ZString EntryNumber;

		/// <summary>
		/// A code assigned to the bill.
		/// </summary>
		[MessageBlockString(11, 15, "M")]
		public ZString BillNumber;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the date of the bill.
		/// </summary>
		[MessageBlockDate(26, "M", "MMddyy")]
		public ZDate BillDate;

		/// <summary>
		/// A code representing the bill type. Valid Bill Type Codes are:
		/// 
		/// 1 = Supplemental Duties
		/// 2 = Deferred Tax
		/// 3 = Fines, Penalties and Forfeiture Billing
		/// </summary>
		[MessageBlockInt(1, 32, "M")]
		public ZInt BillType;

		/// <summary>
		/// A code representing the protest status. Valid Protest Status Codes are:
		/// 
		/// 0 = Not Protested
		/// 1 = Protested
		/// </summary>
		[MessageBlockString(1, 33, "M")]
		public ZString ProtestStatus;

		/// <summary>
		/// A code representing the bill status. Valid Bill Status Codes are:
		/// 
		/// 1 = Open
		/// 2 = Paid
		/// 3 = Cancelled
		/// 4 = Administrative Pay
		/// 5 = Voided
		/// </summary>
		[MessageBlockInt(1, 34, "M")]
		public ZInt BillStatus;

		/// <summary>
		/// A value representing the total bill amount. Two decimal places are implied. If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 35, "M", 2)]
		public ZDecimal TotalBillAmount;

		/// <summary>
		/// A value representing the amount paid. Two decimal places are implied. If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 46, "C", 2)]
		public ZDecimal PaidAmount;

		/// <summary>
		/// A value representing the principal amount. Two decimal places are implied. If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 57, "C", 2)]
		public ZDecimal PrincipalAmount;

		/// <summary>
		/// A value representing the interest amount. Two decimal places are implied. If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(11, 68, "C", 2)]
		public ZDecimal PrincipalInterestAmount;
	}
}
