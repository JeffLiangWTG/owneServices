namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("10")]
	public abstract partial class ENS10 : MessageBlock // Need to add interface for BIRD System
	{
		public ENS10()
			: base("10")
		{
		}

		/// <summary>
		/// A code representing the action to be taken.
		/// </summary>
		[MessageBlockString(1, 3, "M")]
		public ZString UpdateActionCode;

		/// <summary>
		/// A code representing the district/port where the goods were entered under either an entry or immediate delivery permit. Generally, the district code is the same as the district code (except in the case of authorized cross district processing) contained in the block control header record (Record Identifier B); however, the port code can be different. Valid district/port codes can be queried through the Extract Reference File chapter of this publication.
		/// </summary>
		[MessageBlockString(4, 4, "M", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString DistrictPortOfEntry;

		/// <summary>
		/// A code representing the importer of record. This code is optional when using the 10 record to delete a record.
		/// </summary>
		[MessageBlockString(12, 8, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString ImporterOfRecordNumber;

		/// <summary>
		/// A code representing the ultimate consignee. If the ultimate consignee is the same as the importer of record, enter the importer of record number. If this is a consolidated entry summary and there are multiple consignees, enter zeros in one of the three acceptable formats. This field is mandatory if the entry/entry summary data is being certified for cargo release processing.
		/// </summary>
		[MessageBlockString(12, 20, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString UltimateConsigneeNumber;

		/// <summary>
		/// A reference number contained on CBP Form (CBPF) 4811, Special Address Notification that is filed with the CBP. If there is no CBPF-4811 reference number, space fill.
		/// </summary>
		[MessageBlockString(12, 32, "O", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString CBPF4811ReferenceNumber;

		/// <summary>
		/// A code indicating whether an entry/entry summary (live) is to be filed. If an entry/entry summary is to be filed, enter 1; otherwise, space fill.
		/// </summary>
		[MessageBlockInt(1, 44, "C")]
		public ZInt LiveEntryIndicator;

		/// <summary>
		/// Up to two 2-position alphanumeric codes may be entered to indicate that documents are missing. Valid missing document codes are listed in Appendix B of this publication. If the missing document is not listed in Appendix B, enter 98. If more than two documents are missing, enter a 2-position code in the first two positions and 99 in the last two positions.
		/// </summary>
		[MessageBlockString(4, 45, "C")]
		public ZString MissingDocumentCodes;

		/// <summary>
		/// A code representing the valid bond type. Valid Bond Type Codes are:
		/// 
		/// 0 = No bond required
		/// 8 = Continuous bond
		/// 9 = Single transaction bond
		/// 
		/// Appendix B presents a table of bond types valid for each entry type.
		/// </summary>
		[MessageBlockString(1, 49, "C")]
		public ZString BondType;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the estimated entry date. This date is used to calculate duty on non-quota goods when there is no immediate transportation (IT) date or entry date. If there is no entry date, the estimated entry date is used to calculate duties on quota entries, user fees, and perform bond validations. This date is mandatory for entry types 31, 32, 34, and 38.
		/// </summary>
		[MessageBlockDate(50, "C", "MMddyy")]
		public ZDate EstimatedEntryDate;

		/// <summary>
		/// A code indicating whether the entry summary is part of the Automated Invoice Interface (AII) program. If it is, enter E; otherwise, space fill.
		/// </summary>
		[MessageBlockString(1, 56, "C")]
		public ZString ElectronicInvoiceIndicator;

		/// <summary>
		/// A unique code assigned by CBP to all active entry document preparers. The Entry Filer Code occupies the first three positions of a CBP entry number regardless of where the entry is filed. The Entry Filer Code must be the same as the Entry Filer Code in the block control header record (Record Identifier B).
		/// </summary>
		[MessageBlockString(3, 59, "M", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString EntryFilerCode;

		/// <summary>
		/// The number assigned to the entry. For additional information on valid entry number formats, refer to Appendix E of this publication.
		/// </summary>
		[MessageBlockString(9, 62, "M", Justification = Justification.Right)]
		public ZString EntryNumber;

		/// <summary>
		/// A code identifying the type of entry. Valid entry type codes are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(2, 71, "M", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString EntryType;

		/// <summary>
		/// A code identifying the surety company providing bond coverage for the importation. If the Surety Code is 990-997 or 999, the error message SURETY NOT VALID FOR ENTRY TYPE is system generated.
		/// </summary>
		[MessageBlockString(3, 73, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString SuretyCode;

		/// <summary>
		/// For future use. Space fill.
		/// </summary>
		[MessageBlockString(1, 76, "C")]
		public ZString OtherGovernmentAgencyOGACode;

		/// <summary>
		/// The state code assigned by the United States Postal Service representing the state of destination. This code is not required if the entry type is 11 (informal).
		/// </summary>
		[MessageBlockString(2, 78, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString StateOfDestination;

		/// <summary>
		/// Insert “1” to indicate that Other Government Agency (OGA) merchandise on this entry is processed through Line Release at a border port. Insert “2” to indicate that OGA reporting is being processed for an entry released via NCAP/FAST.’ Otherwise, leave blank.
		/// </summary>
		[MessageBlockInt(1, 80, "O")]
		public ZInt OGALineReleaseIndicator;
	}
}
