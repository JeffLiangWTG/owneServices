namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[InputBlock("R91")]
	public partial class RECR91 : MessageBlock
	{
		public RECR91()
			: base("R91")
		{
		}

		/// <summary>
		/// The total estimate reconciliation interest is the total estimated reconciliation interest amount due as reflected on the above entries. It is the sum of the reconciliation interest amounts from all R20 records if the aggregate indicator = ‘N’ (if the R10 aggregate indicator equals Y, we edit format only). Input is in implied decimal format, right justified. Leading zeroes will be required. This field represents interest due and payable. The amount in this record will be reflected, as needed, in the R17 payment record. If NO interest is payable, zero fill this field.
		/// </summary>
		[MessageBlockDecimal(12, 4, "M", 2)]//Not specified in the spec
		public ZDecimal TotalEstimateReconciliationInterest;
	}
}
