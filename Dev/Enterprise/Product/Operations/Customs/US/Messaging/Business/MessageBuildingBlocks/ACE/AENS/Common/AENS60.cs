namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("60")]
	[OutputBlock("60")]
	public abstract partial class AENS60 : MessageBlock // Need to add interface for BIRD System
	{
		protected AENS60()
			: base("60")
		{
		}

		/// <summary>
		/// CBP accounting classification code representing a specific Internal Revenue tax type. 
		/// 
		/// See 'AE Table 13 - Internal Revenue Accounting Class Codes' for a list those codes supported at the Entry Summary line level.
		/// </summary>
		[MessageBlockString(3, 3, "M")]
		public ZString AccountingClassCode;

		/// <summary>
		/// The Internal Revenue tax amount estimated for the article in U.S. dollars and cents. 
		/// 
		/// Two decimal places are implied.
		/// </summary>
		[MessageBlockDecimal(10, 6, "M", 2)]
		public ZDecimal IRTaxAmount;
	}
}
