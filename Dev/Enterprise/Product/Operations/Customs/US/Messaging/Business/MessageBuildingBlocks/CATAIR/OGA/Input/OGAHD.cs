namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[InputBlock("FDHD")]
	public partial class OGAHD : MessageBlock
	{
		public OGAHD()
			: base("FDHD")
		{
		}

		/// <summary>
		/// A code identifying the entry filer.
		/// </summary>
		[MessageBlockString(3, 5, "M")]
		public ZString FilerCode;

		/// <summary>
		/// A code identifying the entry.
		/// </summary>
		[MessageBlockString(8, 8, "M")]
		public ZString EntryNumber;
	}
}
