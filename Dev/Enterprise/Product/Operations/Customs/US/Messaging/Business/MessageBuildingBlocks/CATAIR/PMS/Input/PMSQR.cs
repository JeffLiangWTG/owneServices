namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[InputBlock("QR")]
	public partial class PMSQR : MessageBlock
	{
		public PMSQR()
			: base("QR")
		{
		}

		/// <summary>
		/// The transmission date in month, day, year (MMDDYY) format. The date may not be more than 14 days in the past.
		/// </summary>
		[MessageBlockDate(3, "C", "MMddyy")]
		public ZDate TransmissionDateOfStatement;

		/// <summary>
		/// Use on Preliminary or Final reports only. Use this field if requesting a statement for a particular importer, otherwise, leave blank. The system will retrieve the matching statement if it is on file.
		/// </summary>
		[MessageBlockString(12, 9, "C")]
		public ZString ImporterOfRecordNumber;

		/// <summary>
		/// Use on preliminary or final statement request(s).This will be compared with the client branch designation on the B record. Use this field if requesting a statement for a particular branch of the office. If not requesting a statement for a particular branch, leave blank.
		/// </summary>
		[MessageBlockString(2, 21, "C")]
		public ZString ClientBranch;

		/// <summary>
		/// Fill this field if the statement number is known. The system will retrieve the statement indicated in this field. If the statement number is not known, leave blank. The last 3 positions may be alpha characters due to the increase in the numbers of statements being generated in some of the larger ports.
		/// </summary>
		[MessageBlockString(10, 23, "C")]
		public ZString StatementNumber;

		/// <summary>
		/// Insert an “A” to receive all documents for all ports in the receiver district port of the A record. If blank, system will reroute only those documents that match the processing district, port filer, and office code of the input B record.
		/// </summary>
		[MessageBlockString(1, 33, "C")]
		public ZString ScopeIndicator;

		/// <summary>
		/// Must be “Y” or “N” to request a preliminary statement. Do not leave blank.
		/// </summary>
		[MessageBlockString(1, 34, "M")]
		public ZString PreliminaryStatementRequest;

		/// <summary>
		/// Must be “Y” or “N” to request a final statement. Do not leave blank.
		/// </summary>
		[MessageBlockString(1, 35, "M")]
		public ZString FinalStatementRequest;

		/// <summary>
		/// Must be "Y" to request a Preliminary Periodic Monthly Statement. Otherwise, submit "N"
		/// </summary>
		[MessageBlockString(1, 36, "M")]
		public ZString PreliminaryPeriodicMonthlyStatementRequest;

		/// <summary>
		/// Must be "Y" to request a final Periodic Monthly Statement. Otherwise, submit "N".
		/// </summary>
		[MessageBlockString(1, 37, "M")]
		public ZString FinalPeriodicMonthlyStatementRequest;
	}
}
