namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[OutputBlock("FDER")]
	public partial class OGAER : MessageBlock
	{
		public OGAER()
			: base("FDER")
		{
		}

		/// <summary>
		/// A code identifying the entry filer.
		/// </summary>
		[MessageBlockString(3, 5, "M")]
		public ZString Filer;

		/// <summary>
		/// A code identifying the entry.
		/// </summary>
		[MessageBlockString(8, 8, "M")]
		public ZString EntryNumber;

		/// <summary>
		/// The CBP line item number identifying the other government agency item containing an error.
		/// </summary>
		[MessageBlockInt(3, 16, "M")]
		public ZInt CBPLine;

		/// <summary>
		/// The tariff number associated with the line item number containing an error. This number must match the tariff number that was originally filed and must be sequenced in the exact order within the entry as originally transmitted when certified for cargo release.
		/// </summary>
		[MessageBlockString(10, 19, "M")]
		public ZString TariffNumber;

		/// <summary>
		/// A code identifying the error message.
		/// </summary>
		[MessageBlockString(3, 29, "M")]
		public ZString ErrorMessageIdentifier;

		/// <summary>
		/// A narrative description of the error.
		/// </summary>
		[MessageBlockString(40, 32, "M")]
		public ZString NarrativeMessage;
	}
}
