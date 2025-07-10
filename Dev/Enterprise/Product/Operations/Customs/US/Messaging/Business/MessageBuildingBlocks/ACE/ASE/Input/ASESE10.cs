namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("SE10")]
	public abstract partial class ASESE10 : MessageBlock // Need to add interface for BIRD System
	{
		protected ASESE10()
			: base("SE10")
		{
		}

		/// <summary>
		/// A code representing the update action.
		/// </summary>
		[MessageBlockString(1, 5, "M")]
		public ZString UpdateActionCode;

		/// <summary>
		/// A unique code assigned by CBP to all active entry document preparers. The Entry Filer Code occupies the first three positions of an entry number regardless of where the entry is filed. This code must be the same as the Entry Filer Code in the block control header record (Record Identifier B).
		/// </summary>
		[MessageBlockString(3, 6, "M")]
		public ZString EntryFilerCode;

		/// <summary>
		/// Unique identifying number assigned to the Entry by the Filer. For additional information on valid entry number formats, refer to Appendix E of this publication.
		/// </summary>
		[MessageBlockString(8, 11, "M")]
		public ZString EntryNumber;

		/// <summary>
		/// A code representing the entry type. Valid entry type codes are listed in Appendix B of the CATAIR publication.
		/// </summary>
		[MessageBlockString(2, 20, "M")]
		public ZString EntryType;

		/// <summary>
		/// The type of number being used to identify the importer of record.
		/// </summary>
		[MessageBlockString(3, 22, "M")]
		public ZString ImporterOfRecordType;

		/// <summary>
		/// Identification of the importer of record.
		/// </summary>
		[MessageBlockString(12, 25, "M", IsPersonalInformation = true)]
		public ZString ImporterOfRecord;

		/// <summary>
		/// A code representing the mode of transportation. Valid mode of transportation codes are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(2, 37, "C")]
		public ZString ModeOfTransportationMOTCode;

		/// <summary>
		/// A code representing the bond type.
		/// </summary>
		[MessageBlockString(1, 39, "M")]
		public ZString BondTypeCode;

		/// <summary>
		/// The total entered value of the entry in whole US dollars.
		/// </summary>
		[MessageBlockDecimal(10, 40, "M", 0)]
		public ZDecimal EstimatedEntryValue;

		/// <summary>
		/// Planned / scheduled port of entry in Schedule D code. Required if PGA data is reported in the filing. 
		///  
		/// **See Note 7 for explanation regarding 
		/// input for this data element.**
		/// </summary>
		[MessageBlockString(5, 50, "C")]
		public ZString PlannedPortOfEntry;

		/// <summary>
		/// A code representing an option for releasing split shipments.
		/// </summary>
		[MessageBlockString(1, 55, "O")]
		public ZString SplitShipmentReleaseCode;

		/// <summary>
		/// Planned / scheduled port of unlading in Schedule D code. Required if PGA data is reported in the filing.
		/// </summary>
		[MessageBlockString(5, 56, "C")]
		public ZString PortOfUnlading;
	}
}
