namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	using CargoWise.Types;

	[InputBlock("90")]
	[OutputBlock("90")]
	public partial class AENS90 : MessageBlock
	{
		public AENS90()
			: base("90")
		{
		}

		/// <summary>
		/// Grand total estimated duty amount. 
		/// 
		/// Two decimal places are implied. Report zeroes if no duty applies.
		/// </summary>
		[MessageBlockDecimal(11, 3, "C", 2, FillType.AlwaysZeroFill)] // Zero Fill
		public ZDecimal GrandTotalDutyAmount;

		/// <summary>
		/// Grand total estimated user fee amount. 
		/// 
		/// Two decimal places are implied. Report zeroes if no user fee applies.
		/// </summary>
		[MessageBlockDecimal(11, 15, "C", 2, FillType.AlwaysZeroFill)] // Zero Fill
		public ZDecimal GrandTotalUserFeeAmount;

		/// <summary>
		/// Grand total estimated IR tax amount. 
		/// 
		/// Two decimal places are implied. Report zeroes if no IR tax applies.
		/// </summary>
		[MessageBlockDecimal(11, 27, "C", 2, FillType.AlwaysZeroFill)] // Zero Fill
		public ZDecimal GrandTotalIRTaxAmount;

		/// <summary>
		/// Grand total estimated AD duty amount. 
		/// 
		/// Two decimal places are implied. Report zeroes if no AD duty applies.
		/// </summary>
		[MessageBlockDecimal(11, 39, "C", 2, FillType.AlwaysZeroFill)] // Zero Fill
		public ZDecimal GrandTotalADDutyAmount;

		/// <summary>
		/// Grand total estimated CV duty amount. 
		/// 
		/// Two decimal places are implied. Report zeroes if no CV duty applies.
		/// </summary>
		[MessageBlockDecimal(11, 51, "C", 2, FillType.AlwaysZeroFill)] // Zero Fill
		public ZDecimal GrandTotalCVDutyAmount;

		/// <summary>
		/// Grand total other revenue amount. 
		/// 
		/// Two decimal places are implied.  Report zeroes (or spaces) if no other revenue applies.
		/// </summary>
		[MessageBlockDecimal(11, 63, "C", 2, FillType.AlwaysZeroFill)] // Zero Fill
		public ZDecimal GrandTotalOtherRevenueAmount;
	}
}
