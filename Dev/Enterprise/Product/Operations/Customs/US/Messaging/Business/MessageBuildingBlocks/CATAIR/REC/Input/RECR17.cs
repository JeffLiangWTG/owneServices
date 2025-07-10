namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[InputBlock("R17")]
	public partial class RECR17 : MessageBlock
	{
		public RECR17()
			: base("R17")
		{
		}

		/// <summary>
		/// A code indicating the payment type. Valid codes are:
		/// 
		/// 1 = payment on individual basis
		/// 2 = payments batched by preliminary
		/// statement print date and filer code
		/// 3 = payments batched by preliminary
		/// statement print date and importer
		/// 5 = payment batched by preliminary
		/// statement print date and importer for
		/// importers with several subdivisions
		/// </summary>
		[MessageBlockString(1, 4, "M")]
		public ZString PaymentTypeIndicator;

		/// <summary>
		/// This field applies to ACS participants authorized for Daily Statement capabilities. It is the date selected for this entry summary to appear on the preliminary Daily Statement. It must be greater than the current date and it cannot be a Saturday, Sunday, or holiday. It may be prior to the date on which the statement will be paid. Transmitting a Preliminary Statement Print Date that allows timely filing and payment of the statement is the filer's responsibility. A Preliminary Statement Print Date of 90 days greater than the system date will be rejected. Enter the Preliminary Statement Print Date in MMDDYY (month, day, year) format when the Payment Type Indicator = 2, 3, or 5. Space fill this data field when the payment type indicator = 1 or if not authorized for batch payment.
		/// </summary>
		[MessageBlockDate(5, "C", "MMddyy")]
		public ZDate PreliminaryStatementPrintDate;

		/// <summary>
		/// A code that allows a filer to designate separate statements for individual branches within the same port code. This code is mandatory for statement filers using a Client Branch Designation. Entry summaries with the same processing district/port code, preliminary statement print date, payment type indicator and client branch designation code appear on the same statement. Use of this field at a port requires prior approval through the CBP Client Representative.
		/// </summary>
		[MessageBlockString(2, 11, "C")]
		public ZString ClientBranchDesignation;

		/// <summary>
		/// The duty payment amount field is the amount of duty that should appear as payable on the statement. It must be equal to the difference between the total original duty and the total reconciliation duty on the R90 record. If the duty payment amount is a negative amount or zero, zero-fill the field. Input is in the implied decimal format, right justified. Leading zeros will be required. For “R” (replacement) transactions, no new statement will be generated. While this is the new total duty due amount filer will actually pay only the difference between the original paid amount and the new amount, if any, and will submit single payment.
		/// </summary>
		[MessageBlockDecimal(12, 13, "M", 2)]//Not specified in the spec
		public ZDecimal DutyPaymentAmount;

		/// <summary>
		/// The tax payment amount field is the amount of the tax that should appear as payable on the statement. It must be equal to the difference between the Total Original Taxes and the Total Reconciliation Taxes on the R90 record. If the tax payment amount is negative, or zero, zero-fill this field. Input is in implied decimal format, right justified. Leading zeros will be required. For “R” (replacement) transactions, no new statement will be generated. While this is the new total tax due amount, filer will actually pay only the difference between the original paid amount and the new amount, if any, and will submit single payment.
		/// </summary>
		[MessageBlockDecimal(12, 25, "M", 2)]//Not specified in the spec
		public ZDecimal TaxPaymentAmount;

		/// <summary>
		/// The fee payment amount field is the amount of the fees that should appear as payable on the statement. It must be equal to the difference between the Total Original Fees and the Total Reconciliation Fees on the R90 record. If the fee payment amount is negative, or zero, zero-fill this field. Input is in implied decimal format, right justified. Leading zeros will be required. The total will be broken down by class code from the R89 record(s) on the statement. For “R” (replacement) transactions, no new statement will be generated. While this is the new total fee due amount, filer will actually pay only the difference between the original paid amount and the new amount, if any. Payment will have to be broken down by class code(s) as shown in the R89 Record, and will be single payment.
		/// </summary>
		[MessageBlockDecimal(12, 37, "M", 2)]//Not specified in the spec
		public ZDecimal FeePaymentAmount;

		/// <summary>
		/// The interest payment amount field is the amount of the interest that should appear as payable on the statement. It must be equal to the Total Reconciliation Interest on the R91 record. If the interest payment amount is zero, zero-fill this field. Input is in implied decimal format (last two digits of transmitted amount will be considered as cents), right justified. Leading zeros will be required. For “R” (replacement) transactions, no new statement will be generated.
		/// </summary>
		[MessageBlockDecimal(12, 49, "M", 2)]//Not specified in the spec
		public ZDecimal InterestPaymentAmount;
	}
}
