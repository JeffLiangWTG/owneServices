namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("SE60", "01")]
	public partial class ASESE60_01 : MessageBlock
	{
		public ASESE60_01()
			: base("SE60")
		{
		}

		/// <summary>
		/// An indicator for whether the filer is claiming this HTS is not subject to sanctions.
		/// </summary>
		[MessageBlockString(1, 5, "C")]
		public ZString SanctionDisclaimIndicator;
	}
}
