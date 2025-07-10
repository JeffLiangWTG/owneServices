namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	[OutputBlock("WO72")]
	public class ACEQWO72 : ASESSO72Base
	{
		public ACEQWO72()
			: base("WO72")
		{
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification)]
	[OutputBlock("SO72")]
	public class ASESSO72 : ASESSO72Base
	{
		public ASESSO72()
			: base("SO72")
		{
		}
	}

	public abstract partial class ASESSO72Base : MessageBlock
	{
		protected ASESSO72Base(string mandatoryCharacters)
			: base(mandatoryCharacters)
		{
		}

		/// <summary>
		/// A narrative message from trade to the PGAs regarding a particular PGA line.
		/// </summary>
		[MessageBlockString(76, 5, "M")]
		public ZString CommentsToTradeFromPGA;
	}
}
