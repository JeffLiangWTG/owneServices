namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Abstract
{
	using CargoWise.Types;

	[OutputBlock("J0")]
	public abstract partial class ENQJ0 : MessageBlock // Need to add interface for BIRD System
	{
		public ENQJ0()
			: base("J0")
		{
		}

		/// <summary>
		/// A code representing the district/port of entry summary. This code is required for entry numbers issued prior to October 1, 1986, and it is optional for entry numbers issued after that date. If no district/port of entry summary is provided, space fill. Valid district/port codes can be queried through the Extract Reference File chapter of this publication.
		/// </summary>
		[MessageBlockString(4, 3, "C")]
		public ZString DistrictPortOfEntrySummary;

		/// <summary>
		/// A unique code assigned by CBP to all active entry document preparers. The Entry Filer Code occupies the first three positions of a CBP entry number regardless of where the entry is filed. The Entry Filer Code must be the same as the Entry Filer Code in the block control header record (Record Identifier B). If the filer is a NILS filer (National Importer Liquidation System), the filer code in positions 7-9 of the J0 record can be different from the filer code in the B record.
		/// </summary>
		[MessageBlockString(3, 7, "M")]
		public ZString BrokerNumberOrEntryFilerCode;

		/// <summary>
		/// The number assigned to the entry number. For additional information on valid entry number formats, refer to Appendix E of this publication.
		/// </summary>
		[MessageBlockString(9, 10, "M", Justification = Justification.Right)]
		public ZString EntryNumber;

		/// <summary>
		/// A narrative message that no data is on file or access is not authorized for the given entry number.
		/// </summary>
		[MessageBlockString(30, 19, "M")]
		public ZString NarrativeMessage;
	}
}
