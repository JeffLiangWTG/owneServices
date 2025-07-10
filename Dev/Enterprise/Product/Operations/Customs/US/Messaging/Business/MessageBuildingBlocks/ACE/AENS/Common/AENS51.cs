namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("51")]
	[OutputBlock("51")]
	public abstract partial class AENS51 : MessageBlock // Need to add interface for BIRD System
	{
		protected AENS51()
			: base("51")
		{
		}

		/// <summary>
		/// The identifying 'number' on the visa issued/furnished by the country of origin. 
		/// 
		/// Position 1-1 = Year of export (decade excluded).
		/// Position 2-3 = Standard Country of Origin ISO Code. 
		/// Position 4-9 = Identifying/sequence number.
		/// </summary>
		[MessageBlockString(9, 3, "M")]
		public ZString StandardVisaNumber;
	}
}
