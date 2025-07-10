namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("34")]
	[OutputBlock("34")]
	public abstract partial class AENS34 : MessageBlock // Need to add interface for BIRD System
	{
		protected AENS34()
			: base("34")
		{
		}

		/// <summary>
		/// CBP accounting classification code representing a specific fee type applicable for the summary as a whole.
		/// </summary>
		[MessageBlockString(3, 3, "M")]
		public ZString AccountingClassCode1;

		/// <summary>
		/// The applicable fee amount in U.S. dollars and cents that corresponds to Accounting Class Code (1). 
		/// 
		/// Two decimal places are implied.
		/// </summary>
		[MessageBlockDecimal(8, 6, "M", 2)]
		public ZDecimal HeaderFeeAmount1;

		/// <summary>
		/// An additional header fee accounting classification code and applicable fee amount in U.S. dollars and cents.
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(3, 14, "C")]
		public ZString AccountingClassCode2;

		/// <summary>
		/// An additional header fee accounting classification code and applicable fee amount in U.S. dollars and cents.
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockDecimal(8, 17, "C", 2)]
		public ZDecimal HeaderFeeAmount2;
	}
}
