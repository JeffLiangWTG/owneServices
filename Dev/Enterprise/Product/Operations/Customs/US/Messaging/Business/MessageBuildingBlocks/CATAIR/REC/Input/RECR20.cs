namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[InputBlock("R20")]
	public partial class RECR20 : MessageBlock
	{
		public RECR20()
			: base("R20")
		{
		}

		/// <summary>
		/// The trailer number is the sequence counter of R20 records. The R20 Records are numbered sequentially, beginning with 0001 and ending with a maximum of 9999. Minimum input consists of at least one (1) R20 record.
		/// </summary>
		[MessageBlockInt(4, 4, "M")]
		public ZInt TrailerNumber;

		/// <summary>
		/// The import entry number must be in FFFNNNNNNNN format and must be a valid entry summary in ACS as a type 01, 02 or 06 (without ADCVD) entry. The entry must have been previously flagged for reconciliation for the issue code indicated in the R10 record.
		/// </summary>
		[MessageBlockString(11, 8, "M")]
		public ZString ImportEntry;

		/// <summary>
		/// The entry port is the port code associated with the above import entry. Must be an exact match.
		/// </summary>
		[MessageBlockString(4, 19, "M")]
		public ZString EntryPort;

		/// <summary>
		/// The original duty input is the total duty amount (either the previous reconciliation amount, or the paid and/or liquidated amount) as reflected on the above entry record. Input is in implied decimal format, right justified. Leading zeros will be required. If original duty is zero, zeroes will be inserted in this field. If the R10 record aggregate indicator is Y, must be zero filled.
		/// </summary>
		[MessageBlockDecimal(11, 23, "M", 2)]//Not specified in the spec
		public ZDecimal OriginalDuty;

		/// <summary>
		/// The estimated reconciliation duty is the total estimated reconciliation duty amount due against the above entry. Input is in implied decimal format, right justified. Leading zeros will be required. If reconciliation duty is zero, zeroes will be inserted in this field. If the R10 record aggregate indicator is Y, must be zero filled.
		/// </summary>
		[MessageBlockDecimal(11, 34, "M", 2)]//Not specified in the spec
		public ZDecimal EstimatedReconciliationDuty;

		/// <summary>
		/// The original tax input is the total tax amount (either the previous reconciliation amount, or the paid and/or liquidated amount) as reflected on the above entry record. Input is in implied decimal format, right justified. Leading zeros will be required. If original tax is zero, zeroes will be inserted in this field. If the R10 record aggregate indicator is Y, must be zero filled.
		/// </summary>
		[MessageBlockDecimal(11, 45, "M", 2)]//Not specified in the spec
		public ZDecimal OriginalTax;

		/// <summary>
		/// The estimated reconciliation tax is the total estimated reconciliation tax amount due against the above entry. Input is in implied decimal format, right justified. Leading zeros will be required. If reconciliation tax is zero, zeroes will be inserted in this field. If the R10 record aggregate indicator is Y, must be zero filled.
		/// </summary>
		[MessageBlockDecimal(11, 56, "M", 2)]//Not specified in the spec
		public ZDecimal EstimatedReconciliationTax;

		/// <summary>
		/// The estimated reconciliation interest is the total estimated reconciliation interest amount due against the above entry. Input is in implied decimal format, right justified. Leading zeros will be required. If reconciliation interest is zero, zeroes will be inserted in this field. If the R10 record aggregate indicator is Y, must be zero filled.
		/// </summary>
		[MessageBlockDecimal(11, 67, "M", 2)]//Not specified in the spec
		public ZDecimal EstimatedReconciliationInterest;

		/// <summary>
		/// The fee trailer counter is a count of all R21 fee trailer records transmitted as associated with this R20 import entry record. It should equal the highest R21 record trailer number (maximum = 10). This field will be edited to ensure we (or the filers) haven’t missed records. If no R21 records are used, the R20 fee trailer counter must equal 00.
		/// </summary>
		[MessageBlockInt(2, 78, "M")]
		public ZInt FeeTrailerCounter;
	}
}
