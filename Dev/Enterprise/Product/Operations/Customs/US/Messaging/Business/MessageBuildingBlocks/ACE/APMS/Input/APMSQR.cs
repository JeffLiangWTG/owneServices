// use C:\Dev\Enterprise\Product\Operations\Customs\US\Messaging\Business\MessageBuildingBlocks\CATAIR\PMS\Input\PMSQR.cs

//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
//{
//	using CargoWise.Types;


//	[InputBlock("QR")]
//	public partial class APMSQR : MessageBlock
//	{
//		public APMSQR()
//			: base("QR")
//		{
//		}

//		/// <summary>
//		/// The transmission date in month, day, year (MMDDYY) format. The date may not be more than 60 days in the past. 
//		/// A date is required unless a statement number is provided.
//		/// </summary>
//		[MessageBlockDate(3, "C", "MMddyy")]
//		public ZDate TransmissionDateOfStatement; //field name changed

//		/// <summary>
//		/// Use this field if requesting a Statement for a particular importer, otherwise, leave blank. ACE will retrieve the matching statement if it is on file.
//		/// </summary>
//		[MessageBlockString(12, 9, "O")]
//		public ZString ImporterOfRecordNumber;

//		/// <summary>
//		/// Use this field if requesting a Statement for a particular branch of the office. This will be compared with the client branch designation on the B record. If not requesting a statement for a particular branch, leave blank.
//		/// </summary>
//		[MessageBlockString(2, 21, "O")]
//		public ZString ClientBranchIdentifier;

//		/// <summary>
//		/// Complete this field if the Statement Number is known. ACE will retrieve the statement indicated in this field. If the statement number is not known, leave blank.
//		/// </summary>
//		[MessageBlockString(10, 23, "O")]
//		public ZString StatementNumber;

//		/// <summary>
//		/// Insert an "A" to receive all documents for all ports in the receiver's dp site filer code/district/ port of the A-Record. If blank, ACE will reroute only those Statements that match the processing district/port, filer code, and office code of the input B-Record.
//		/// </summary>
//		[MessageBlockString(1, 33, "O")]
//		public ZString ScopeIndicator;

//		/// <summary>
//		/// Must be "Y" to request a preliminary Daily Statement. Otherwise, submit "N"
//		/// </summary>
//		[MessageBlockString(1, 34, "M")]
//		public ZString PreliminaryDailyStatementRequest;

//		/// <summary>
//		/// Must be "Y" to request a final Daily Statement. Otherwise, submit "N"
//		/// </summary>
//		[MessageBlockString(1, 35, "M")]
//		public ZString FinalDailyStatementRequest;

//		/// <summary>
//		/// Must be "Y" to request a Preliminary Periodic Monthly Statement. Otherwise, submit "N"
//		/// </summary>
//		[MessageBlockString(1, 36, "M")]
//		public ZString PreliminaryPeriodicMonthlyStatementRequest;

//		/// <summary>
//		/// Must be "Y" to request a final Periodic Monthly Statement. Otherwise, submit "N".
//		/// </summary>
//		[MessageBlockString(1, 37, "M")]
//		public ZString FinalPeriodicMonthlyStatementRequest;

//		/// <summary>
//		/// Must be "Y" to request an ACH Statement Payment Authorization. Otherwise, submit "N"
//		/// </summary>
//		[MessageBlockString(1, 38, "M")]
//		public ZString ACHStatementPaymentAuthorizationRequest;
//	}
//}
