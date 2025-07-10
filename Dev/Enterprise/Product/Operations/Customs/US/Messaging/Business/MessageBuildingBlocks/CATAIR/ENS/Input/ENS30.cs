namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("30")]
	public abstract partial class ENS30 : MessageBlock // Need to add interface for BIRD System
	{
		public ENS30()
			: base("30")
		{
		}

		/// <summary>
		/// A code identifying the entry filer code of the associated warehouse entry. If the entry type code is 31, 32, 34, or 38, this field is mandatory.
		/// </summary>
		[MessageBlockString(3, 21, "C")]
		public ZString EntryFilerCodeOfWarehouseEntry;

		/// <summary>
		/// The number assigned to the associated warehouse entry. If the entry type code is 31, 32, 34, or 38, this field is mandatory.
		/// </summary>
		[MessageBlockString(8, 24, "C")]
		public ZString WarehouseEntryNumber;

		/// <summary>
		/// The district/port code for the associated warehouse entry. If the entry type code is 31, 32, 34, or 38, this field is mandatory.
		/// </summary>
		[MessageBlockString(4, 32, "C")]
		public ZString DistrictPortCodeOfWarehouseEntry;

		/// <summary>
		/// A code identifying if a final withdrawal condition exists. If the entry type code is 31, 32, 34, or 38, this field is mandatory. Valid codes are:
		/// 
		/// 0	not final
		/// 1	final
		/// </summary>
		[MessageBlockString(1, 36, "C")]
		public ZString FinalWarehouseIndicator;

		/// <summary>
		/// A 1 in this position serves as the equivalent of an electronic signature for paperless entry summary and indicates the filer's willingness to accept the paperless status of the summary; otherwise, enter 0 (zero). Certification is required for AII and RLF. Valid codes are:
		/// 
		/// 1	Summary Data is to be certified for paperless summary processing.
		/// 0	Summary data is not to be certified for paperless summary processing.
		/// 
		/// Although both certification codes are still available for use, ABI no longer requires either code in performing summary selectivity. Thus, the filer may still receive “paperless-filer retain records” if “0”is transmitted.
		/// </summary>
		[MessageBlockString(1, 37, "C")]
		public ZString SummaryCertificationCode;

		/// <summary>
		/// A code representing the release certification. Certification is required for AII and RLF.
		/// </summary>
		[MessageBlockInt(1, 38, "C")]
		public ZInt ReleaseCertificationCode;

		/// <summary>
		/// A code representing the consolidated/informal indicator. If Record Identifier 32 is present, this code must be C.
		/// </summary>
		[MessageBlockString(1, 39, "C")]
		public ZString ConsolidatedInformalIndicator;

		/// <summary>
		/// The designated exam port code to be used with remote entry filing only.
		/// </summary>
		[MessageBlockString(4, 40, "C")]
		public ZString DesignatedExamPort;

		/// <summary>
		/// The numeric month (MM) when the entry summary will appear on the preliminary periodic monthly statement. Ex. The merchandise was entered or released in the month of June, then this field will be entered as 07 (July)
		/// </summary>
		[MessageBlockString(2, 51, "C")]
		public ZString PeriodicStatementMonth;

		/// <summary>
		/// This field is mandatory for participants who are authorized for Daily Statement capabilities. ACS participants who are not authorized for Daily Statement capabilities should space fill this data field.
		/// </summary>
		[MessageBlockString(1, 53, "C")]
		public ZString PaymentTypeIndicator;

		/// <summary>
		/// This field applies to ACS participants authorized for Daily Statement capabilities. It is the date selected for this entry summary to appear on the Preliminary Daily Statement. It must greater than the current date and it cannot be a Saturday, Sunday, or holiday. It may be prior to the date on which the statement will be paid. Transmitting a Preliminary Statement Print Date that allows timely filing and payment of the statement is the filer's responsibility. A Preliminary Statement Print Date of 90 days greater than the system date will be rejected. Enter the Preliminary Statement Print Date in MMDDYY (month, day, year) format when the Payment Type Indicator = 2, 3, 5, 6, 7, or 8. Space fill this data field when the Payment Type Indicator = 1 or if not authorized for batch payment. This date is required for paperless processing.
		/// </summary>
		[MessageBlockDate(54, "C", "MMddyy")]
		public ZDate PreliminaryStatementPrintDate;

		/// <summary>
		/// A code identifying the carrier. This field is mandatory only if mode of transportation 10, 11, 20, 21, 40, or 41 in the 20 record is mandatory. For a description of the mode of transportation codes, refer to Appendix B of this publication.
		/// </summary>
		[MessageBlockString(4, 73, "C")]
		public ZString CarrierCode;

		/// <summary>
		/// A code indicating the CBP import specialist team assigned the entry summary. This code is edited against the ACS assigned team number. If the codes do not match and the user has elected the no acknowledgment option, the filer receives a warning message and the team number is returned in the E0 or E90 output records.
		/// </summary>
		[MessageBlockString(3, 77, "O")]
		public ZString TeamNumber;
	}
}
