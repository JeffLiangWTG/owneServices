namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[InputBlock("R89")]
	public partial class RECR89 : MessageBlock
	{
		public RECR89()
			: base("R89")
		{
		}

		/// <summary>
		/// The fee summary trailing number is the sequence counter of the R89 records. R89 records are numbered sequentially, beginning with 01 and ending with a maximum of 10.
		/// </summary>
		[MessageBlockInt(2, 4, "M")]
		public ZInt FeeSummaryTrailerNumber;

		/// <summary>
		/// The first fee class code will be input and must be a valid collection class code on at least one R21 record. The class code given may appear on more than one R21 record, but must be present on at least one R21 Record. If R21 record is ignored or not given because R10 aggregated indicator equals Y, code may equal any valid fee class code. IF the recon is AGGREGATE or if there were NO fees on the original entries, the BLANK R89 is given (an R89 is required). IF there are no fees and no R21’s given, and you DO report a fee class code in R89, we ACCEPT the entry but the warning lets them know they gave us a fee that was NOT on the original entry (ies).
		/// </summary>
		[MessageBlockString(3, 6, "C")]
		public ZString FeeClass;

		/// <summary>
		/// The original fee input is the total fee amount (paid and/or liquidated) as reflected for a reconciliation for the above class code. The amount will be the sum of the original fee amount for all R21 records on the reconciliation for the above class code. For example, if class code 499 appears on five different R21 records, the amount input here will be the sum of all R21 original fee amount fields for class code 499. If no R21 records, insert total original fee for class code. Input is in implied decimal format, right justified. Leading zeros will be required. If original fee is zero, zeroes will be inserted in this field.
		/// </summary>
		[MessageBlockDecimal(11, 9, "C", 2, "FeeClassFillType")]//Not specified in the spec
		public ZDecimal TotalOriginalFee;

		/// <summary>
		/// The total estimate reconciliation fee is the total estimated reconciliation fee amount due against the above entry for the above class code. The amount will be the sum of the reconciliation fee amount for all R21 records on the reconciliation for the above class code. For example, if class code 499 appears on five different R21records, the amount input here would be the sum of all R21 reconciliation fee amount fields for class code 499. Input is in implied decimal format, right justified. Leading zeroes will be required. If reconciliation fee is zero, zeroes will be inserted in this field. If R10 aggregated indicator equals Y, total estimate reconciliation fee for class code must be equal to or greater than corresponding original fee.
		/// </summary>
		[MessageBlockDecimal(11, 20, "C", 2, "FeeClassFillType")]//Not specified in the spec
		public ZDecimal TotalEstimateReconciliationFee;

		/// <summary>
		/// The second fee class code, if input, must be a valid collection class code on at least one R21 Record. The class code given may appear on more than one R21 record, but must be present on at least one R21 record. If no second class needed, space fill. If R21 records were ignored or not given because R10 aggregate indicator equals Y, code may equal any valid fee class code.
		/// </summary>
		[MessageBlockString(3, 31, "C")]
		public ZString FeeClass1;

		/// <summary>
		/// The original fee input is the total fee amount (paid and/or liquidated) as reflected for a reconciliation for the above class code. The amount will be the sum of the original fee amount for all R21 records on the reconciliation for the above class code. For example, if class code 499 appears on five different R21 records, the amount input here will be the sum of all R21 original fee amount fields for class code 499. If no R21 records, insert total original fee for class code. Input is in implied decimal format, right justified. Leading zeros will be required. If original fee is zero, zeroes will be inserted in this field. If the second fee class code > spaces, this field must be > spaces. If second class equals spaces, zero fill this field.
		/// </summary>
		[MessageBlockDecimal(11, 34, "C", 2, "FeeClass1FillType")]//Not specified in the spec
		public ZDecimal TotalOriginalFee1;

		/// <summary>
		/// The total estimate reconciliation fee is the total estimated reconciliation fee amount due against the above entry for the above second-class code. The amount will be the sum of the reconciliation fee amount for all R21 records on the reconciliation for the above second-class code. For example, if class code 499 appears on five different R21 records, the amount input here will be the sum of all R21 reconciliation fee amount fields for class code 499. Input is in implied decimal format, right justified. Leading zeroes will be required. If reconciliation fee is zero, zeroes will be inserted in this field. If the second fee class code > spaces, this field must be > spaces. If second class equals spaces, zero fill this field. If R10 aggregated indicator equals Y, total estimate reconciliation fee for class code must be equal to or greater than corresponding original fee.
		/// </summary>
		[MessageBlockDecimal(11, 45, "C", 2, "FeeClass1FillType")]//Not specified in the spec
		public ZDecimal TotalEstimateReconciliationFee1;

		/// <summary>
		/// The third fee class code, if input, must be a valid collection class code on at least one R21 Record. The class code given may appear on more than one R21 record, but must be present on at least one R21 record. If no third class needed, space fill. If R21 records were ignored or not given because R10 aggregate indicator equals Y, code may equal any valid fee class code.
		/// </summary>
		[MessageBlockString(3, 56, "C")]
		public ZString FeeClass2;

		/// <summary>
		/// The original fee input is the total fee amount (paid and/or liquidated) as reflected for a reconciliation for the above third class code. The amount will be the sum of the original fee amount for all R21 records on the reconciliation for the above third class code. For example, if class code 499 appears on five different R21 records, the amount input here will be the sum of all R21 original fee amount fields for class code 499. If no R21 records, insert total original fee for class code. Input is in implied decimal format, right justified. Leading zeros will be required. If original fee is zero, zeroes will be inserted in this field. If the third fee class code > spaces, this field must be > spaces. If third class equals spaces, zero fill this field.
		/// </summary>
		[MessageBlockDecimal(11, 59, "C", 2, "FeeClass2FillType")]//Not specified in the spec
		public ZDecimal TotalOriginalFee2;

		/// <summary>
		/// The total estimate reconciliation fee is the total estimated reconciliation fee amount due against the above entry for the above third class code. The amount will be the sum of the reconciliation fee amount for all R21 records on the reconciliation for the above third class code. For example, if class code 499 appears on five different R21 records, the amount input here would be the sum of all R21 reconciliation fee amount fields for class code 499. Input is in implied decimal format, right justified. Leading zeroes will be required. If reconciliation fee is zero, zeroes will be inserted in this field. If the third fee class code > spaces, this field must be > spaces. If third class equals spaces, zero fill this field. If R10 aggregated indicator equals Y, total estimate reconciliation fee for class code must be equal to or greater than corresponding original fee.
		/// </summary>
		[MessageBlockDecimal(11, 70, "C", 2, "FeeClass2FillType")]//Not specified in the spec
		public ZDecimal TotalReconciliationFee2;
	}
}
