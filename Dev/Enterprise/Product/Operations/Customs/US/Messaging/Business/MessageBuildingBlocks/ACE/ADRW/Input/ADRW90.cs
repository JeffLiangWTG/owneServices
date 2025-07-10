namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("90")]
	public partial class ADRW90 : MessageBlock
	{
		public ADRW90()
			: base("90")
		{
		}

		/// <summary>
		/// Grand total estimated duty amount.
		/// 
		/// Two decimal places are implied. Report zeroes if no duty applies.
		/// </summary>
		[MessageBlockDecimal(11, 3, "C", 2, FillType.AlwaysZeroFill)]
		public ZDecimal GrandTotalDutyAmount;

		/// <summary>
		/// Grand total estimated user fee amount.
		/// 
		/// Two decimal places are implied. Report zeroes if no user fee applies.
		/// </summary>
		[MessageBlockDecimal(11, 15, "C", 2, FillType.AlwaysZeroFill)]
		public ZDecimal GrandTotalUserFeeAmount;

		/// <summary>
		/// Grand total estimated IR tax amount.
		/// 
		/// Two decimal places are implied. Report zeroes if no IR tax applies.
		/// </summary>
		[MessageBlockDecimal(11, 27, "C", 2, FillType.AlwaysZeroFill)]
		public ZDecimal GrandTotalIRTaxAmount;
	}
}
