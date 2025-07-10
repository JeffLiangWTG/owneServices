namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[OutputBlock("R02")]
	public partial class STSR02Combine : MessageBlock
	{
		public STSR02Combine()
			: base("R02")
		{
		}

		/// <summary>
		/// The data for either Bill of Lading Status Notification or Bill of Lading Status Notification Continuation
		/// </summary>
		[MessageBlockString(73, 4, "M", ShouldTrimBegining = false)]
		public ZString Data;
	}
}
