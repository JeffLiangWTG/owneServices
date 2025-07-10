namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("OI")]
	[OutputBlock("OI")]
	public abstract partial class AENSOI : MessageBlock // Need to add interface for BIRD System
	{
		public AENSOI()
			: base("OI")
		{
		}

		/// <summary>
		/// The commercial description as provided on the invoice line; reported per the other agency's instruction. 
		/// 
		/// Broad, generalized language is unacceptable, as are tariff descriptions. Complete commercial terminology, in the English language, is mandatory. 
		/// 
		/// Left justify.
		/// </summary>
		[MessageBlockString(70, 11, "M")]
		public ZString CommercialDescriptionText;
	}
}
