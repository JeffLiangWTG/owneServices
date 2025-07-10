namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[OutputBlock("GE90")]
	public partial class AGE90 : MessageBlock
	{
		public AGE90()
			: base("GE90")
		{
		}

		/// <summary>
		/// 01 = Message Rejected
		/// 02 = Message Accepted
		/// 11 = Record Rejected
		/// </summary>
		[MessageBlockString(2, 5, "M")]
		public ZString MessageTypeCode;

		/// <summary>
		/// A code that identifies the message.
		/// </summary>
		[MessageBlockString(3, 7, "C")]
		public ZString MessageIdentifierCode;

		/// <summary>
		/// Narrative message text.
		/// </summary>
		[MessageBlockString(70, 10, "M")]
		public ZString NarrativeMessageText;
	}
}
