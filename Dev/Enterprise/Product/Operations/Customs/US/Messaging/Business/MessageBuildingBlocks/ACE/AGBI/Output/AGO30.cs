namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[OutputBlock("GO30")]
	public partial class AGO30 : MessageBlock
	{
		public AGO30()
			: base("GO30")
		{
		}

		/// <summary>
		/// A numeric date inMMDDYY(month,day,year) format representingthe disposition
		/// </summary>
		[MessageBlockDate(5, "M", "MMddyy")]
		public ZDate StatusActionDate;

		/// <summary>
		/// The military time in HHMM (hour, minute) format representing the time of the disposition action.
		/// </summary>
		[MessageBlockString(4, 11, "M")]
		public ZString StatusActionTime;

		/// <summary>
		/// A code representing the disposition action.
		/// </summary>
		[MessageBlockString(3, 15, "M")]
		public ZString DispositionActionCode;

		/// <summary>
		/// The narrative message associated with the disposition code.
		/// </summary>
		[MessageBlockString(63, 18, "M")]
		public ZString NarrativeMessage;
	}
}
