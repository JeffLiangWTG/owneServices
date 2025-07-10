namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[InputBlock("R21")]
	public partial class RECR21 : MessageBlock
	{
		public RECR21()
			: base("R21")
		{
		}

		/// <summary>
		/// The trailer number is the sequence counter of R21 records. R21 records are numbered sequentially, beginning with 01 and ending with a maximum of 10.
		/// </summary>
		[MessageBlockInt(2, 4, "M")]
		public ZInt TrailerNumber;

		/// <summary>
		/// The first fee class code will be input and must be a valid collection class code on the original import entry. Valid fee codes are list in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(3, 6, "M")]
		public ZString FirstFeeClass;

		/// <summary>
		/// The first original fee input is the total fee amount (either the previous reconciliation amount, or the paid and/or liquidated amount) as reflected on the above entry record for the above class code. Input is in implied decimal format, right justified. Leading zeros will be required. If zero, zero fill.
		/// </summary>
		[MessageBlockDecimal(11, 9, "M", 2, "FirstFeeFillType")]//Not specified in the spec
		public ZDecimal FirstOriginalFee;

		/// <summary>
		/// The first estimated reconciliation fee is the total estimated reconciliation fee amount due against the above entry for the above class code. Input is in implied decimal format, right justified. Leading zeros will be required. If reconciliation fee is zero, zeroes will be inserted in this field.
		/// </summary>
		[MessageBlockDecimal(11, 20, "M", 2, "FirstFeeFillType")]//Not specified in the spec
		public ZDecimal FirstEstimateReconciliationFee;

		/// <summary>
		/// The second fee class code, if input, must be a valid collection class code on the original import entry. If not second class needed, space fill.
		/// </summary>
		[MessageBlockString(3, 31, "C")]
		public ZString SecondFeeClass;

		/// <summary>
		/// The second original fee input is the total fee amount (either the previous reconciliation amount, or the paid and/or liquidated amount) as reflected on the above entry record for the above second class code. Input is in implied decimal format, right justified. Leading zeros will be required. If the second fee class code > spaces, this field must be > spaces. If second class equals spaces, zero fill this field.
		/// </summary>
		[MessageBlockDecimal(11, 34, "C", 2, "SecondFeeFillType")]//Not specified in the spec
		public ZDecimal SecondOriginalFee;

		/// <summary>
		/// The second reconciliation fee is the total estimated reconciliation fee amount due against the above entry for the above second class code. Input is in implied decimal format, right justified. Leading zeros will be required. If reconciliation fee is zero, zeros will be inserted in this field. If the second fee class code > spaces, this field must be > spaces. If second class equal spaces, zero fill this field.
		/// </summary>
		[MessageBlockDecimal(11, 45, "C", 2, "SecondFeeFillType")]//Not specified in the spec
		public ZDecimal SecondEstimateReconciliationFee;

		/// <summary>
		/// The third fee class code, if input, must be a valid collection class code on the original import entry. If no third class needed, space fill.
		/// </summary>
		[MessageBlockString(3, 56, "C")]
		public ZString ThirdFeeClass;

		/// <summary>
		/// The third original fee input is the total fee amount (either the previous reconciliation amount, or the paid and/or liquidated amount) as reflected on the above entry record for the above third class code. Input is in implied decimal format, right justified. Leading zeros will be required. If the third fee class code > spaces, this field must be > spaces. If third class equal spaces, zero fill the field.
		/// </summary>
		[MessageBlockDecimal(11, 59, "C", 2, "ThirdFeeFillType")]//Not specified in the spec
		public ZDecimal ThirdOriginalFee;

		/// <summary>
		/// The third estimate reconciliation fee is the total estimated reconciliation fee amount due against the above entry for the above third class code. Input is in implied decimal format, right justified. Leading zeros will be required. If the third fee class code > spaces, this field must be > spaces. If third class equals spaces, zero fill the field.
		/// </summary>
		[MessageBlockDecimal(11, 70, "C", 2, "ThirdFeeFillType")]//Not specified in the spec
		public ZDecimal ThirdEstimatedReconciliationFee;
	}
}
