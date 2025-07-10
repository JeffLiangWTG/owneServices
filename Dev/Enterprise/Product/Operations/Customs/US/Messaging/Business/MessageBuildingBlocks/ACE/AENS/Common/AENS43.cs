namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("43")]
	[OutputBlock("43")]
	public abstract partial class AENS43 : MessageBlock // Need to add interface for BIRD System
	{
		protected AENS43()
			: base("43")
		{
		}

		/// <summary>
		/// An indication as to the type of information provided in the record:
		/// 
		/// C = A Pre-Classification number, specifically citing the Importer and merchandise, is provided in the Ruling Number.
		/// P = A Pre-Approval number, specifically citing the Importer and merchandise, is provided in the Ruling Number.
		/// R = A Binding Ruling number is provided in the Ruling Number.
		/// </summary>
		[MessageBlockString(1, 3, "M")]
		public ZString RulingTypeCode;

		/// <summary>
		/// The administrative number assigned by CBP to a binding ruling or a ruling under the pre-importation review program (PIRP). 
		/// 
		/// Left justify, space fill.
		/// </summary>
		[MessageBlockString(6, 9, "C")]
		public ZString RulingNumber;
	}
}
