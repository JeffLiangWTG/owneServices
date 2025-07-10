namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Abstract
{
	using CargoWise.Types;

	[OutputBlock("R5")]
	public abstract partial class CMQR5 : MessageBlock // Need to add interface for Entry Summary
	{
		public CMQR5()
			: base("R5")
		{
		}

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the disposition action date.
		/// </summary>
		[MessageBlockDate(3, "M", "MMddyy")]
		public ZDate DispositionActionDate;

		/// <summary>
		/// The military time in HHMM (hour, minute) format representing the time of the disposition action.
		/// </summary>
		[MessageBlockString(4, 9, "M")]
		public ZString DispositionActionTime;

		/// <summary>
		/// A code representing the disposition action.
		/// </summary>
		[MessageBlockString(2, 13, "M")]
		public ZString DispositionActionCode;

		/// <summary>
		/// The narrative message associated with the disposition code.
		/// </summary>
		[MessageBlockString(33, 15, "M")]
		public ZString NarrativeMessage;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the release date. This data element is only returned if the Disposition Action Code is 22.
		/// </summary>
		[MessageBlockDate(55, "C", "MMddyy")]
		public ZDate ReleaseDate;

		/// <summary>
		/// A code representing the action or date ACS has used to determine the current release date. This data element is only returned if the Disposition Action Code is 22.
		/// </summary>
		[MessageBlockString(2, 61, "C")]
		public ZString ReleaseOrigin;

		/// <summary>
		/// Quantity of the specific transaction.
		/// </summary>
		[MessageBlockInt(8, 63, "C")]
		public ZInt Quantity;

		/// <summary>
		/// Serial number related to sequence of the transaction.
		/// </summary>
		[MessageBlockInt(3, 72, "C")]
		public ZInt Sequence;
	}
}
