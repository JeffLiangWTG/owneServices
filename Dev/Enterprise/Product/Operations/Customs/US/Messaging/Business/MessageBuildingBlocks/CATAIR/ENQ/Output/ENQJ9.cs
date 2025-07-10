namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Abstract
{
	using CargoWise.Types;

	[OutputBlock("J9")]
	public abstract partial class ENQJ9 : MessageBlock // Need to add interface for BIRD System
	{
		public ENQJ9()
			: base("J9")
		{
		}

		/// <summary>
		/// A code representing the district/port where the merchandise was entered under an entry or immediate delivery permit. Generally, the district code is the same as the district code contained in the block control header record (Record Identifier B); however, the port code can be different. Valid district/port codes can be queried through the Extract Reference File chapter of this document.
		/// </summary>
		[MessageBlockString(4, 3, "M")]
		public ZString DistrictPortOfEntrySummary;

		/// <summary>
		/// A unique code assigned by CBP to all active entry document preparers. The Entry Filer Code occupies the first three positions of a CBP entry number regardless of where the entry is filed. The Entry Filer Code must be the same as the Entry Filer Code in the block control header record (Record Identifier B). If the filer is a NILS filer (National Importer Liquidation System), the filer code positions 7-9 of the J9 record can be different from the filer code in the B record.
		/// </summary>
		[MessageBlockString(3, 7, "M")]
		public ZString BrokerNumberOrEntryFilerCode;

		/// <summary>
		/// The number assigned to the entry. For additional information on valid entry number formats, refer to Appendix E of this publication.
		/// </summary>
		[MessageBlockString(9, 10, "M", Justification = Justification.Right)]
		public ZString EntryNumber;

		/// <summary>
		/// A narrative message stating the transmission was received with errors.
		/// </summary>
		[MessageBlockString(30, 19, "M")]
		public ZString NarrativeMessage;
	}
}
