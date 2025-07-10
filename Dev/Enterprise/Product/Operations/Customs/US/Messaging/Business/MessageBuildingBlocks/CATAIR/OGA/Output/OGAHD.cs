namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[OutputBlock("FDHD")]
	public partial class OGAHD : MessageBlock
	{
		public OGAHD()
			: base("FDHD")
		{
		}

		/// <summary>
		/// A code identifying the entry filer.
		/// </summary>
		[MessageBlockString(3, 5, "M")]
		public ZString FilerCode;

		/// <summary>
		/// A code identifying the entry.
		/// </summary>
		[MessageBlockString(8, 8, "M")]
		public ZString EntryNumber;

		/// <summary>
		/// Must always equal DATA REPLACED AS REQUESTED.
		/// </summary>
		[MessageBlockString(26, 16, "M")]
		public ZString DataReplacedMessage;
	}
}
