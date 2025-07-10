namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common
{
	using CargoWise.Types;

	public partial class APLB : MessageBlock
	{
		public APLB()
			: base("B")
		{
		}

		/// <summary>
		/// A number from 01 to 99. If the user is transmitting a single block of detail data, the block number is 01. The block number is not retained in ACS as part of the record. If more than one block is being transmitted, the block number will start with 01 and be incremented by one for each batch transmitted.
		/// </summary>
		[MessageBlockInt(2, 2, "M")]
		public ZInt BlockNumber;

		/// <summary>
		/// A code representing the processing district/ port. Valid district/port codes can be queried through the Extract Reference File chapter of this publication. This code also designates where (CBPs district/port location) the statement is printed and processed.
		/// </summary>
		[MessageBlockString(4, 4, "M", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString ProcessingDistrictPortCode;

		/// <summary>
		/// A unique code assigned by the CBP to all active entry document preparers. The Entry Filer Code occupies the first three positions of an entry number regardless of where the entry is filed. This code must be the same as the Entry Filer Code in the block control trailer record (Record Identifier Y). This code represents the ACS user who is responsible for the data contained within the block. A computer service bureau should insert the Entry Filer Code of the user responsible for the entry/entry summary.
		/// </summary>
		[MessageBlockString(3, 8, "M")]
		public ZString EntryFilerCode;

		/// <summary>
		/// The application identifier defines the type of application detail data contained within the block.
		/// </summary>
		[MessageBlockString(2, 11, "M")]
		public ZString ApplicationIdentifier;

		/// <summary>
		/// A code representing the statement status. This data element is located only on Daily Statement output records. Valid Statement Status codes are:
		/// 
		/// P = Preliminary statement
		/// F = Final statement
		/// </summary>
		[MessageBlockString(1, 13, "C")]
		public ZString StatementStatus;

		/// <summary>
		/// This data element is located only on Daily Statement output records so it can be available for printing user report headings. The last 3 positions may be alpha characters due to the increase in the numbers of statements being generated in some of the larger ports.
		/// </summary>
		[MessageBlockString(10, 14, "C")]
		public ZString StatementNumber;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the Preliminary Statement Print Date. This data element is located only on Preliminary and Final Daily Statement output records so it can be available for printing user report headings.
		/// </summary>
		[MessageBlockDate(24, "C", "MMddyy")]
		public ZDate PreliminaryStatementPrintDate;

		/// <summary>
		/// This data element is located only on Daily Statement output records so it can be available for printing user report headings.
		/// </summary>
		[MessageBlockString(1, 30, "C")]
		public ZString PaymentTypeIndicator;

		/// <summary>
		/// This data element is present only on Daily Statement output records so it can be available for printing user report headings.
		/// </summary>
		[MessageBlockString(12, 31, "C")]
		public ZString ImporterOfRecordNumber;

		/// <summary>
		/// A code representing the client branch designation. This code is present only if a filer uses the client branch designation and Daily Statement processing. This code can be used for printing user report headings.
		/// </summary>
		[MessageBlockString(2, 43, "C")]
		public ZString ClientBranchDesignation;

		/// <summary>
		/// A unique code assigned by the broker. This code must be coordinated with CBP and associated with the filers automated profile.
		/// </summary>
		[MessageBlockString(2, 45, "O", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString ProcessingOfficeCode;

		/// <summary>
		/// To be used when the Preparer Indicator in position 56 is NOT blank, otherwise, leave blank.
		/// </summary>
		[MessageBlockString(4, 47, "C")]
		public ZString PreparerDistrictPort;

		/// <summary>
		/// This code must equal entry filer code if the Preparer Indicator in position 56 is NOT blank, otherwise leave blank.
		/// </summary>
		[MessageBlockString(3, 51, "C")]
		public ZString PreparerFilerCode;

		/// <summary>
		/// To be used when the Preparer Indicator in position 56 is NOT blank and the preparing filer in positions 51-53 uses an Office Code.
		/// </summary>
		[MessageBlockString(2, 54, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString PreparerOfficeCode;

		/// <summary>
		/// Blank if not remote entry. One (1), if remote entry or two (2) for reconciliation entries.
		/// </summary>
		[MessageBlockString(1, 56, "C")]
		public ZString PreparerIndicator; // string

		/// <summary>
		/// Positions 60 through 80 are for the participant's internal use. These positions will be returned unmodified to the user when the B record is transmitted as output from ACS.
		/// </summary>
		[MessageBlockString(21, 60, "C")]
		public ZString UserData;
	}
}
