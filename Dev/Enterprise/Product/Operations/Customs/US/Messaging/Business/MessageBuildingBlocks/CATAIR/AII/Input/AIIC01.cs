namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[InputBlock("C01")]
	public partial class AIIC01 : MessageBlock
	{
		public AIIC01()
			: base("C01")
		{
		}

		/// <summary>
		/// A code representing the update action. Valid Update Action Codes are:
		/// 
		/// A = Add
		/// D = Delete
		/// R = Replace
		/// </summary>
		[MessageBlockString(1, 4, "M")]
		public ZString UpdateActionCode;

		/// <summary>
		/// A code representing the supplier party. This code is based on the supplier's name and address and is derived the same way as the Manufacturer/Supplier Code (Record Identifier 60) and the Manufacturer/Shipper Code (Record Identifier H5). Refer to CBP Directive 3500-13 dated November 24, 1986, for complete instructions on determining this code.
		/// </summary>
		[MessageBlockString(15, 5, "M")]
		public ZString SupplierPartyIDCode;

		/// <summary>
		/// The invoice number. Valid characters are alpha, numeric and dash (-) only.
		/// </summary>
		[MessageBlockString(17, 20, "M", OnLengthViolation = LengthViolationAction.Substring)]
		public ZString InvoiceNumber;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the invoice date.
		/// </summary>
		[MessageBlockDate(37, "M", "MMddyy")]
		public ZDate InvoiceDate;

		/// <summary>
		/// A code representing the invoice type. Valid Invoice Type Codes are:
		/// 
		/// IN = Commercial Invoice
		/// PI = Proforma Invoice
		/// CO = Corrected
		/// CI = Consolidated Invoice
		/// CN = Consignment
		/// </summary>
		[MessageBlockString(2, 43, "M")]
		public ZString InvoiceType;

		/// <summary>
		/// A code representing the type of currency specified on the invoice. Valid currency codes are listed in Appendix B of this publication. If this code equals USD (U.S. dollars), space fill the remaining data elements on this record.
		/// </summary>
		[MessageBlockString(3, 45, "M")]
		public ZString CurrencyCode;

		/// <summary>
		/// A code indicating if the conversion rate reported in positions 49-55 in this record is fixed. A code of 1 indicates the rate is fixed via the purchase contract, 0 indicates the rate is not fixed.
		/// </summary>
		[MessageBlockString(1, 48, "C")]
		public ZString FixedExchangeRateIndicator;

		/// <summary>
		/// A number representing either the fixed rate of exchange or the appropriate CBP rate of exchange for the date of export as published by CBP Information Exchange (CIE). Six decimal places are implied.
		/// </summary>
		[MessageBlockDecimal(7, 49, "C", 6, FillType.AlwaysZeroFill)]
		public ZDecimal RateOfExchange;

		/// <summary>
		/// A code of R indicates that this invoice transmission is in response to a specific CBP request. For complete invoices that are transmitted independently of a direct CBP request, space fill.
		/// </summary>
		[MessageBlockString(1, 56, "C")]
		public ZString InvoiceRequestResponseIndicator;
	}
}
