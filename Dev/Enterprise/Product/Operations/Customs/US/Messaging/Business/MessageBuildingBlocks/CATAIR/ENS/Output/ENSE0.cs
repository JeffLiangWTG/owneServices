namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Abstract
{
	using CargoWise.Types;

	[OutputBlock("E0")]
	public abstract partial class ENSE0 : MessageBlock // Need to add interface for Entry Symmary
	{
		public ENSE0()
			: base("E0")
		{
		}

		/// <summary>
		/// A code representing the district/ port of entry summary. Generally, the district code is the same as the district code contained in the block control header record (Record Identifier B); however, the port code can be different. Valid district/port codes can be queried through the Extract Reference File chapter of this publication.
		/// </summary>
		[MessageBlockString(4, 3, "M")]
		public ZString DistrictPortOfEntrySummary;

		/// <summary>
		/// A unique code assigned by the CBP to all active entry document preparers. The Entry Filer Code occupies the first three positions of an entry number regardless of where the entry is filed. This code must be the same as the Entry Filer Code in the block control header record (Record Identifier B).
		/// </summary>
		[MessageBlockString(3, 7, "M")]
		public ZString EntryFilerCode;

		/// <summary>
		/// The number assigned to the entry. For additional information on valid entry number formats, refer to Appendix E of this publication.
		/// </summary>
		[MessageBlockString(9, 10, "M", Justification = Justification.Right)]
		public ZString EntryNumber;

		/// <summary>
		/// An optional code provided by the participant. This field is not edited or changed during ACS processing. It is provided for internal user system control in entry summary processing.
		/// </summary>
		[MessageBlockString(9, 19, "O")]
		public ZString BrokerReferenceNumber;

		/// <summary>
		/// A narrative message that the entry summary transaction has been received error free; entry summary release is certified via summary; and/or cargo release data is certified.
		/// </summary>
		[MessageBlockString(40, 28, "M")]
		public ZString NarrativeMessage;

		/// <summary>
		/// A code representing the entry type. Valid entry type codes are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(2, 68, "M")]
		public ZString EntryTypeCode;

		/// <summary>
		/// A code indicating the CBP import specialist team assigned the entry summary by ACS.
		/// </summary>
		[MessageBlockString(3, 70, "M")]
		public ZString AssignedTeamNumber;

		/// <summary>
		/// A code identifying the narrative message.
		/// </summary>
		[MessageBlockString(3, 73, "M")]
		public ZString ErrorMessageIdentifier;
	}
}
