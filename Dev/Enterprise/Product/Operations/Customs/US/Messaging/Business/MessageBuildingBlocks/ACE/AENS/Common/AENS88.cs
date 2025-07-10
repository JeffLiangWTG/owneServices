namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	using CargoWise.Types;

	[InputBlock("88")]
	[OutputBlock("88")]
	public partial class AENS88 : MessageBlock
	{
		public AENS88()
			: base("88")
		{
		}

		/// <summary>
		/// Total bonded AD duty amount for Entry Summary. 
		/// 
		/// Two decimal places are implied. Zero fill if not applicable.
		/// </summary>
		[MessageBlockDecimal(11, 3, "C", 2, FillType.AlwaysZeroFill)]// otherswise gets rejected
		public ZDecimal TotalBondedADDutyAmount;

		/// <summary>
		/// Total cash deposit AD duty amount for Entry Summary. 
		/// 
		/// Two decimal places are implied. Zero fill if not applicable.
		/// </summary>
		[MessageBlockDecimal(11, 15, "C", 2, FillType.AlwaysZeroFill)]// otherswise gets rejected
		public ZDecimal TotalCashDepositADDutyAmount;

		/// <summary>
		/// Total bonded CV duty amount for Entry Summary. 
		/// 
		/// Two decimal places are implied. Zero fill if not applicable.
		/// </summary>
		[MessageBlockDecimal(11, 27, "C", 2, FillType.AlwaysZeroFill)]// otherswise gets rejected
		public ZDecimal TotalBondedCVDutyAmount;

		/// <summary>
		/// Total cash deposit CV duty amount for Entry Summary. 
		/// 
		/// Two decimal places are implied. Zero fill if not applicable.
		/// </summary>
		[MessageBlockDecimal(11, 39, "C", 2, FillType.AlwaysZeroFill)]// otherswise gets rejected
		public ZDecimal TotalCashDepositCVDutyAmount;
	}
}
