namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[OutputBlock("BN02")]
	public partial class INBBN02 : MessageBlock
	{
		public INBBN02()
			: base("BN02")
		{
		}

		/// <summary>
		/// The CBP line number when the Header Identifier is “E” (Entry Summary). This will only apply to entries sent via “EI”, “HI” or “HN”. Otherwise, space fill.
		/// </summary>
		[MessageBlockInt(3, 5, "C")]
		public ZInt CBPLine;

		/// <summary>
		/// The line number applicable to the OGA form requirement.
		/// </summary>
		[MessageBlockInt(4, 8, "M")]
		public ZInt FDALine;

		/// <summary>
		/// The confirmation number received from FDA when prior notice is received.
		/// </summary>
		[MessageBlockString(12, 12, "C")]
		public ZString PriorNoticeConfirmationNumber;

		/// <summary>
		/// Prior Notice line level message from FDA.
		/// </summary>
		[MessageBlockString(26, 24, "M")]
		public ZString PriorNoticeLineMessage;

		/// <summary>
		/// Date received from FDA when prior notice was received in MMDDYY (month, day, year) format.
		/// </summary>
		[MessageBlockDate(50, "C", "MMddyyyy")] // different format
		public ZDate PriorNoticeClockStartDate;

		/// <summary>
		/// Time received from FDA when prior notice was received in HHMMSS (hour, minute, second) format.
		/// </summary>
		[MessageBlockString(6, 58, "C")]
		public ZString PriorNoticeClockStartTime;

		/// <summary>
		/// Code identifying reject reasons when prior notice is rejected.
		/// </summary>
		[MessageBlockString(16, 64, "C", ShouldTrimBegining = false)] // error code + position is unique therefore should not trim the leading spaces
		public ZString PriorNoticeLineRejectCode;
	}
}
