namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[InputBlock("R90")]
	public partial class RECR90 : MessageBlock
	{
		public RECR90()
			: base("R90")
		{
		}

		/// <summary>
		/// The import trailer number is the total count of all R20 records for the reconciliation.
		/// </summary>
		[MessageBlockInt(4, 4, "M")]
		public ZInt ImportTrailerCounter;

		/// <summary>
		/// The total original duty input is the total duty amount (paid and/or liquidated) as reflected on the above entries. It is the sum of the original duty amounts from all R20 records if the aggregate indicator = ‘N’ (or if the aggregate indicator = ‘Y’, edited for format only). Input is in implied decimal format, right justified. Leading zeroes will be required. If original duty is zero, zeroes will be inserted in this field.
		/// </summary>
		[MessageBlockDecimal(12, 8, "M", 2)]//Not specified in the spec
		public ZDecimal TotalOriginalDuty;

		/// <summary>
		/// The total estimate reconciliation duty is the total estimated reconciliation duty amount due as reflected on the above entries. It is the sum of the reconciliation duty amounts from all R20 records if the aggregate indicator = ‘N’ (or if the aggregate indicator = ‘Y’, edited for format only). Input is in implied decimal format, right justified. Leading zeroes will be required. If reconciliation duty is zero, zeroes will be inserted in this field. If R10 aggregate indicator equals Y, total estimate reconciliation duty must be equal or greater than total original duty.
		/// </summary>
		[MessageBlockDecimal(12, 20, "M", 2)]//Not specified in the spec
		public ZDecimal TotalEstimateReconciliationDuty;

		/// <summary>
		/// The total original tax input is the total tax amount (paid and/or liquidated) as reflected on the above entries. It is the sum of the original tax amounts from all R20 records if the aggregate indicator = ‘N’ (or if the aggregate indicator = ‘Y’, edited for format only). Input is in implied decimal format, right justified. Leading zeroes will be required. If original tax is zero, zeroes will be inserted in this field.
		/// </summary>
		[MessageBlockDecimal(12, 32, "M", 2)]//Not specified in the spec
		public ZDecimal TotalOriginalTax;

		/// <summary>
		/// The total estimate reconciliation tax is the total estimated reconciliation tax amount due as reflected on the above entries. It is the sum of the reconciliation tax amounts from all R20 records if the aggregate indicator = ‘N’ (or if the aggregate indicator = ‘Y’, edited for format only). Input is in implied decimal format, right justified. Leading zeroes will be required. If reconciliation tax is zero, zeroes will be inserted in this field. If R10 aggregate indicator equals Y, total estimate reconciliation tax must be equal or greater than total original tax.
		/// </summary>
		[MessageBlockDecimal(12, 44, "M", 2)]//Not specified in the spec
		public ZDecimal TotalEstimateReconciliationTax;

		/// <summary>
		/// The total original fee is the total original fee amount and is the sum of all the original fee amounts for all class codes on the R89 records. Input is in implied decimal format, right justified. Leading zeroes will be required.
		/// </summary>
		[MessageBlockDecimal(12, 56, "M", 2)]//Not specified in the spec
		public ZDecimal TotalOriginalFees;

		/// <summary>
		/// The total estimate reconciliation fee is the total estimated reconciliation fee amount due on the above entries and is the sum of all fee amounts for all class codes on the R89 records. Input is in implied decimal format, right justified. Leading zeroes will be required. If reconciliation fee is zero, zeroes will be inserted in this field. If R10 aggregate indicator equals Y, total estimated reconciliation fee must be equal or greater than total original fee.
		/// </summary>
		[MessageBlockDecimal(12, 68, "M", 2)]//Not specified in the spec
		public ZDecimal TotalEstimateReconciliationFees;
	}
}
