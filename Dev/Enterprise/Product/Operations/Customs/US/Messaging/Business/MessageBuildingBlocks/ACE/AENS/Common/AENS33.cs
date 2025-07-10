namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("33")]
	[OutputBlock("33")]
	public abstract partial class AENS33 : MessageBlock // Need to add interface for BIRD System
	{
		protected AENS33()
			: base("33")
		{
		}

		/// <summary>
		/// First missing document code or '98' (Other document not codified).
		/// </summary>
		[MessageBlockString(2, 3, "M")]
		public ZString MissingDocumentCode1;

		/// <summary>
		/// Second missing document code (if any), '98' (Other document not codified), or '99' (More than two documents missing). 
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(2, 5, "C")]
		public ZString MissingDocumentCode2;
	}
}
