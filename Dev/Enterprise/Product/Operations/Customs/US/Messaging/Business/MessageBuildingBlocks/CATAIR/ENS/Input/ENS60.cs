namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("60")]
	public abstract partial class ENS60 : MessageBlock // Need to add interface for BIRD System
	{
		public ENS60()
			: base("60")
		{
		}

		/// <summary>
		/// The CVD duty for the line item. If there is a CVD Case Number reported in positions 13-22 of this record, the duty is mandatory. If the duty is less than one cent, enter 0000000000. Do not space fill. If there is no case number, space fill. Two decimal places are implied. If the number is a whole number, the two low- order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(10, 3, "C", 2)]
		public ZDecimal CountervailingDuty;

		/// <summary>
		/// A code identifying the specific countervailing duty case. Do not enter hyphens. If the CVD duty is 0000000000 or greater, the CVD Case Number is mandatory.
		/// </summary>
		[MessageBlockString(10, 13, "C")]
		public ZString CountervailingCaseNumber;

		/// <summary>
		/// A code identifying the specific antidumping case number. Do not enter hyphens. If the ADD duty is 0000000000 or greater, the ADD Case Number is mandatory.
		/// </summary>
		[MessageBlockString(10, 23, "C")]
		public ZString AntidumpingCaseNumber;

		/// <summary>
		/// The ADD duty for the line item. If there is an ADD case number in positions 23-32 of this record, the duty is mandatory. If the duty is less than one cent, enter 0000000000. Do not space fill. If there is no case number, space fill. Two decimal places are implied. If the number is a whole number, the two low-order (cents) positions will contain zeros.
		/// </summary>
		[MessageBlockDecimal(10, 33, "C", 2)]
		public ZDecimal AntidumpingDuty;

		/// <summary>
		/// A code identifying the manufacturer/supplier. The first two positions of the code for Canadian manufacturers/shippers is a CBP assigned code for the Canadian province/territory instead of the International Organization for Standardization (ISO) country code. If the code contains less than 15 positions, it is left justified. Refer to CBP Directive 3500-13, dated November 24, 1986, for complete instructions on determining the manufacturer/supplier code.
		/// </summary>
		[MessageBlockString(15, 43, "M")]
		public ZString ManufacturerSupplierCode;

		/// <summary>
		/// The tax due to the Internal Revenue Service (IRS). If there is no tax, enter zeros or space fill. Two decimal places are implied. If the tax is a whole number, the two low-order (cents) positions contain zeros. IRS excise tax rates are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockDecimal(10, 59, "C", 2)]
		public ZDecimal InternalRevenueServiceIRSTax;

		/// <summary>
		/// The decimal representation of the percentage of the CVD deposit rate. Four decimal places are implied. If the rate is a whole number, the four low-order positions contain zeros. If no CVD deposit rate is applicable, space fill.
		/// </summary>
		[MessageBlockDecimal(5, 69, "C", 4)]
		public ZDecimal CVDDepositRate;

		/// <summary>
		/// The decimal representation of the percentage of the ADD deposit rate. Four decimal places are implied. If the rate is a whole number, the four low-order positions contain zeros. If no ADD deposit rate is applicable, space fill.
		/// </summary>
		[MessageBlockDecimal(5, 74, "C", 4)]
		public ZDecimal ADDDepositRate;

		/// <summary>
		/// A code indicating if the CVD is bonded. If the CVD is bonded, enter 1; otherwise, enter 0 (zero). If there is a Countervailing Case Number, the Bonded CVD Indicator is mandatory. If there is no case number, space fill.
		/// </summary>
		[MessageBlockString(1, 79, "C")]
		public ZString BondedCVDIndicator;

		/// <summary>
		/// A code indicating if the ADD is bonded. If the ADD is bonded, enter 1; otherwise, enter 0 (zero). If there is an Antidumping Case Number, the Bonded ADD Indicator is mandatory. If there is no case number, space fill.
		/// </summary>
		[MessageBlockString(1, 80, "C")]
		public ZString BondedADDIndicator;
	}
}
