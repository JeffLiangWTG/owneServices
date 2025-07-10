namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("21")]
	[OutputBlock("21")]
	public abstract partial class AENS21 : MessageBlock // Need to add interface for BIRD System
	{
		protected AENS21()
			: base("21")
		{
		}

		/// <summary>
		/// Carrier's identification of the trip/manifest.
		/// </summary>
		[MessageBlockString(5, 3, "M")]
		public ZString TripIdentifier;
	}
}
