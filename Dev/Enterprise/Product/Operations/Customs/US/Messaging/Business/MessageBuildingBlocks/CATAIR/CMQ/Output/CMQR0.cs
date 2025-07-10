namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Abstract
{
	using CargoWise.Types;

	[OutputBlock("R0")]
	public abstract partial class CMQR0 : MessageBlock // Need to add interface for Entry Summary
	{
		public CMQR0()
			: base("R0")
		{
		}

		/// <summary>
		/// A unique code assigned by CBP to all active entry document preparers. The filer code occupies the first three positions of an entry number regardless of where the entry is filed.
		/// </summary>
		[MessageBlockString(3, 3, "M")]
		public ZString EntryFilerCode;

		/// <summary>
		/// The number assigned to the entry. Only the new entry number format may be used. For additional information on valid entry number formats, refer to Appendix E of this publication.
		/// </summary>
		[MessageBlockString(9, 6, "M", Justification = Justification.Right)]
		public ZString EntryNumber;

		/// <summary>
		/// A code identifying the error message.
		/// </summary>
		[MessageBlockString(3, 30, "M")]
		public ZString ErrorMessageIdentifier;

		/// <summary>
		/// A narrative message indicating the error condition preventing processing of the query.
		/// </summary>
		[MessageBlockString(40, 33, "M")]
		public ZString NarrativeMessage;
	}
}
