namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[InputBlock("D90")]
	public partial class DRWD90 : MessageBlock
	{
		public DRWD90()
			: base("D90")
		{
		}

		/// <summary>
		/// Total of all previous D30 and D40 duty amounts. If no duty is claimed, zero fill.
		/// </summary>
		[MessageBlockDecimal(12, 4, "M", 2)] // 2 decimals
		public ZDecimal TotalClaimDuty;

		/// <summary>
		/// Total of all previous D30 and D40 tax amounts. If no tax is claimed, zero fill.
		/// </summary>
		[MessageBlockDecimal(12, 16, "M", 2)] // 2 decimals
		public ZDecimal TotalClaimTax;

		/// <summary>
		/// Total count of all D30 records. If no import trailer records, zero fill.
		/// </summary>
		[MessageBlockInt(4, 28, "M")]
		public ZInt ImportTrailerCount;

		/// <summary>
		/// Total count of all D40 records. If no certificate of manufacture records, zero fill.
		/// </summary>
		[MessageBlockInt(4, 32, "M")]
		public ZInt CertificateOfManufactureCount;

		/// <summary>
		/// Total count of all contracts on D12 records. If no D12 records, zero fill.
		/// </summary>
		[MessageBlockInt(2, 36, "M")]
		public ZInt ContractCount;

		/// <summary>
		/// Total count of all D50 records. If no D50 records, zero fill.
		/// </summary>
		[MessageBlockInt(5, 38, "M")]
		public ZInt NAFTACountryImportEntryTariffCount;

		/// <summary>
		/// Total NAFTA Country duty paid on all D50 records in the currency of the NAFT country. Two decimal places are implied. If zero, zero fill.
		/// </summary>
		[MessageBlockDecimal(12, 43, "M", 2)]
		public ZDecimal TotalNAFTACountryImportDuty;

		/// <summary>
		/// Total U.S. dollar equivalent of the above total NAFTA country duty amount. Sum of all U.S. amounts from all D50 records. Two decimal places are implied. If zero, zero fill.
		/// </summary>
		[MessageBlockDecimal(12, 55, "M", 2)]
		public ZDecimal TotalUSDollarEquivalentOfNAFTACountryDuty;

		/// <summary>
		/// Total count of all D20 records. If no D20 Records, enter 0000 (zeros).
		/// </summary>
		[MessageBlockInt(4, 67, "M")]
		public ZInt ImportTariffNumberRecordTrailerCount;

		/// <summary>
		/// Total count of all D25 records. If no D25 records, enter 0000 (zeros).
		/// </summary>
		[MessageBlockInt(4, 71, "M")]
		public ZInt ScheduleBNumberRecordTrailerCount;
	}
}
