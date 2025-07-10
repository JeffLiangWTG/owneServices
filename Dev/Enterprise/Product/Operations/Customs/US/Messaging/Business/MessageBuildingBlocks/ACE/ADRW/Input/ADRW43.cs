namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("43")]
	public partial class ADRW43 : MessageBlock
	{
		public ADRW43()
			: base("43")
		{
		}

		/// <summary>
		/// CBP accounting classification code representing a specific fee type.
		/// 
		/// The following Acct Class Code are allowed for Drawback and eligible for Accelerated payment (AP) with proper privileges and Bond requirement on file. AP request will be removed from Claims filed with Accounting Class Code not listed here.
		///
		/// Acct Class Code		Description
		/// 364				Drawback Duty
		/// 365				Drawback Tax(es)
		/// 369				Drawback Duty
		/// 398				Drawback HMF
		/// 399				Drawback MPF
		/// 674				Oil Spill tax
		/// 675				Domestic Tax
		/// 
		/// For Drawback Provision 51-75. CBP will accept digital Drawback claim with additional accounting class codes beyond Drawback Accounting Class Code listed above. Drawback Claim beyond Drawback Accounting Class Code listed above is subject to additional reviews and Claim is not eligible for Accelerated Payment.
		/// For additional list of all allowable Accounting Class code, please see Appendix F.
		/// </summary>
		[MessageBlockString(3, 3, "M")]
		public ZString AccountingClassCode;

		/// <summary>
		/// The claim amount is the actual amount of the refund claimed in U.S. dollars and cents (calculated to two decimal places).
		///
		/// For Core Drawback and TFTEA direct identification claims: Record on an itemized basis 99% of the duties, taxes, and fees paid on the imported merchandise.
		/// For claims with valuable waste, deduct from the amounts above the amount attributable to the valuable waste, if applicable.
		///
		/// For TFTEA substitution claims: If subject to a lesser of rule under 19 U.S.C. 1313(l) with a substituted value that is less than the entered goods value of the imported merchandise, record on an itemized basis 99% of the duties, taxes, and fees allocated to the imported merchandise.
		/// Otherwise, record on an itemized basis 99% of the duties, taxes, and fees allocated to the entered goods value of the imported merchandise, with those amounts equally apportioned over all units covered by a single line item on an entry summary.
		/// For claims with valuable waste, deduct the amount attributable to the valuable waste.
		///
		/// For claims prepared under drawback provisions with no 1% deduction, record 100% of the appropriate refund amounts.
		///
		/// For 1313(d) claims, for which there is no imported merchandise, record 100% of the domestic taxes paid on the imported merchandise.
		///
		/// For NAFTA claims, record 99% of the lesser of the duties paid (US duty paid vs CA/MX duty paid).
		///
		/// For import entries that were flagged for Reconciliation and the change in value is less than the original import, record the lower value.
		/// </summary>
		[MessageBlockDecimal(8, 6, "M", 2)]
		public ZDecimal ClaimAmount;

		/// <summary>
		/// The calculated amount is the maximum amount of the refund allowable in U.S. dollars and cents (calculated to two decimal places).
		///
		/// For Core Drawback and TFTEA direct identification claims: Record on an itemized basis 99% of the duties, taxes, and fees paid on the imported merchandise.
		///
		/// For TFTEA substitution claims: Record on an itemized basis 99% of the duties, taxes, and fees allocated to the entered goods value of the imported merchandise, with those amounts equally apportioned over all units covered by a single line item on an entry summary.
		///
		/// For 1313(d) claims, for which there is no imported merchandise, record the same amount as the claim amount.
		///
		/// For NAFTA claims, record 99% of the total amount of US duties paid on the corresponding underlying Entry Summary line used for the NAFTA export.
		///
		/// For import entries that were flagged for Reconciliation and the change in value is less than the original import, record the lower value.
		/// </summary>
		[MessageBlockDecimal(8, 14, "M", 2)]
		public ZDecimal CalculatedAmount;

		/// <summary>
		/// The adjusted claimed amount is the positive change in value on the original underlying import entry(s) due to a reconciliation or a prior disclosure, in U.S. dollars and cents (calculated to two decimal points).
		///
		/// Space fill if the underlying import entry was not flagged for reconciliation.
		/// </summary>
		[MessageBlockDecimal(8, 22, "O", 2)]
		public ZDecimal AdjustedClaimedAmount;

		/// <summary>
		/// 01= Quarterly HMT (Only used when Accounting class code is 398 and HMT is paid quarterly)
		///
		/// Space fill if not claiming Quarterly HMT.
		/// </summary>
		[MessageBlockString(2, 30, "C")]
		public ZString QualifierIndicator;
	}
}
