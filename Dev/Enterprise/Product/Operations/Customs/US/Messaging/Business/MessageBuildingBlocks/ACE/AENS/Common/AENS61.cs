namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("61")]
	[OutputBlock("61")]
	public abstract partial class AENS61 : MessageBlock // Need to add interface for BIRD System
	{
		protected AENS61()
			: base("61")
		{
		}

		/// <summary>
		/// CBP accounting classification code representing an other revenue type. 
		/// 
		/// See 'AE Table 17 – Other Revenue Accounting Class Codes' for a list those codes supported at the Entry Summary line level. 
		/// </summary>
		[MessageBlockString(3, 3, "M")]
		public ZString AccountingClassCode;

		/// <summary>
		/// The other revenue amount estimated for the article in U.S. dollars and cents. 
		/// 
		/// Two decimal places are implied.
		/// </summary>
		[MessageBlockDecimal(8, 6, "M", 2)]
		public ZDecimal UserFeeAmount;
	}
}
