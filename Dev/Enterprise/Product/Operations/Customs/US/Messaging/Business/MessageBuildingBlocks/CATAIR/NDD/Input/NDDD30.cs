namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[InputBlock("D30")]
	public partial class NDDD30 : MessageBlock
	{
		public NDDD30()
			: base("D30")
		{
		}

		/// <summary>
		/// A value representing the total value of the claim.
		/// </summary>
		[MessageBlockDecimal(11, 4, "M", 2)] // 2 decimals
		public ZDecimal TotalValueOfClaim;

		/// <summary>
		/// A value representing the total estimated U.S. duty of the claim.
		/// </summary>
		[MessageBlockDecimal(11, 15, "C", 2)] // 2 decimals
		public ZDecimal TotalEstimatedDutyUS;

		/// <summary>
		/// A value representing the NAFTA country paid duty, in U.S. dollars for the claim.
		/// </summary>
		[MessageBlockDecimal(11, 26, "C", 2)] // 2 decimals
		public ZDecimal TotalNAFTACountryPaidDutyInUSDollars;

		/// <summary>
		/// A value representing the total duty owed the CBP. The value is equal to the TOTAL ESTIMATED DUTY (U.S.) minus TOTAL NAFTA COUNTRY PAID DUTY (IN U.S. DOLLARS). If no duty is owed, report zeros in this field. If the TOTAL NAFTA COUNTRY PAID DUTY (IN U.S. DOLLARS) is greater than the TOTAL ESTIMATED DUTY (U.S.) then zeros shall appear in this field. If the TOTAL NAFTA COUNTRY PAID DUTY (IN U.S. DOLLARS) is less than the TOTAL ESTIMATED DUTY (U.S.) then the difference between the two amounts shall appear in this field.
		/// </summary>
		[MessageBlockDecimal(11, 37, "M", 2)] // 2 decimals
		public ZDecimal TotalDutyOwed;

		/// <summary>
		/// A value representing the total user fee amount for the claim, two decimal places are implied.
		/// </summary>
		[MessageBlockDecimal(11, 48, "C", 2)]
		public ZDecimal TotalFee;

		/// <summary>
		/// A code representing the method of payment. Valid payment type indicators are:
		/// 
		/// 1 = If payment of the claim is to be made on
		/// an individual basis.
		/// 2 = If payments are to be batched by
		/// preliminary statement print date and filer
		/// code.
		/// 3 = If payments are to be batched by
		/// preliminary statement print date and
		/// importer of record number.
		/// 5 = If payments are to be batched by
		/// preliminary statement print date and
		/// importer of record for an importer with
		/// several suffixes.
		/// 
		/// If the PAYMENT TYPE INDICATOR equals 2, 3, or 5 then at least one valid D20 record must be present.
		/// </summary>
		[MessageBlockString(1, 59, "C")]
		public ZString PaymentTypeIndicator;

		/// <summary>
		/// This field applies to ACS participants authorized for Daily Statement capabilities. It is the date selected for this entry summary to appear on the Preliminary Daily Statement. It must be greater than the current date and it cannot be a Saturday, Sunday, or holiday. It may be prior to the date on which the claim will be paid. Transmitting a Preliminary Statement Print Date that allows timely filing and payment of the statement is the filer's responsibility. A Preliminary Statement Print Date of 90 days greater than the system date will be rejected. Enter the Preliminary Statement Print Date in MMDDYY (month, day, year) format when the Payment Type Indicator = 2, 3, or 5. Space fill this data element when the Payment Type Indicator = 1 or if not authorized for batch payment.
		/// </summary>
		[MessageBlockDate(60, "C", "MMddyy")]
		public ZDate PreliminaryStatementPrintDate;

		/// <summary>
		/// A code that allows a filer to designate separate statements for individual branches within the same district/port code.
		/// </summary>
		[MessageBlockString(2, 66, "O")]
		public ZString ClientBranchDesignation;

		/// <summary>
		/// A code, which indicates that NAFTA country entry data associated with the claim, will or will not be provided. Valid codes are:
		/// 
		/// Space fill = Associated NAFTA country entry data will be provided.
		/// 1 = Associated NAFTA country entry data will not be provided.
		/// 
		/// If NAFTA COUNTRY DISCLAIMER = ‘space fill’ then the transaction must contain at least one valid D20 record. If NAFTA COUNTRY DISCLAIMER = ‘1’ then the transaction shall not contain a valid D20 record.
		/// </summary>
		[MessageBlockString(1, 68, "M")]
		public ZString NAFTACountryEntryListDisclaimer;
	}
}
