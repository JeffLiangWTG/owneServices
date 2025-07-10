namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Abstract
{
	using CargoWise.Types;

	[OutputBlock("H6")]
	public abstract partial class CRLH6 : MessageBlock // Need to add interface for Cargo Release
	{
		public CRLH6()
			: base("H6")
		{
		}

		/// <summary>
		/// A code representing the district/port of entry. Valid district/port codes can be queried through the Extract Reference File chapter of this publication.
		/// </summary>
		[MessageBlockString(4, 3, "M")]
		public ZString DistrictPortOfEntry;

		/// <summary>
		/// A unique code assigned by CBP to all active entry document preparers. The Entry Filer Code occupies the first three positions of an entry number regardless of where the entry is filed. This code must be the same as the Entry Filer Code in the block control header record (Record Identifier B).
		/// </summary>
		[MessageBlockString(3, 7, "M")]
		public ZString EntryFilerCode;

		/// <summary>
		/// The number assigned to the entry. For additional information on valid entry number formats, refer to Appendix E of this publication.
		/// </summary>
		[MessageBlockString(9, 10, "M", Justification = Justification.Right)]
		public ZString EntryNumber;

		/// <summary>
		/// A code representing the broker reference number. This data element is provided for the convenience of the Automated Commercial System (ACS) user. It is not edited or changed in any way during ACS processing. It is passed back to the user in various output records to allow easier referencing of the entry filer's records.
		/// </summary>
		[MessageBlockString(9, 19, "C")]
		public ZString BrokerReferenceNumber;

		/// <summary>
		/// A code identifying the importer of record.
		/// </summary>
		[MessageBlockString(12, 28, "M")]
		public ZString ImporterOfRecordNumber;

		/// <summary>
		/// A narrative message indicating the transaction has been accepted or rejected.
		/// </summary>
		[MessageBlockString(30, 40, "M")]
		public ZString NarrativeMessage;

		/// <summary>
		/// A code identifying the message.
		/// </summary>
		[MessageBlockString(3, 70, "M")]
		public ZString MessageIdentifierCode;
	}
}
