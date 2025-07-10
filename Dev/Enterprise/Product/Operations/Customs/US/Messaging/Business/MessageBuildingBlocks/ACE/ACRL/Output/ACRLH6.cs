// Use C:\Dev\Enterprise\Product\Operations\Customs\US\Messaging\Business\MessageBuildingBlocks\CATAIR\CRL\Output\CRLH6.cs
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
//{
//    using CargoWise.Types;

//    [OutputBlock("H6")]
//    public partial class ACRLH6 : MessageBlock
//    {
//        public ACRLH6()
//            : base("H6")
//        {
//        }

//        /// <summary>
//        /// ACE AE data element: District/Port of Entry.
//        /// A code representing the district/port of entry summary.
//        /// </summary>
//        [MessageBlockString(4, 3, "M")]
//        public ZString DistrictPortOfEntry;

//        /// <summary>
//        /// ACE AE data element: Entry Filer Code.
//        /// Entry Filer's identification code (as assigned by CBP).
//        /// </summary>
//        [MessageBlockString(3, 7, "M")]
//        public ZString EntryFilerCode;

//        /// <summary>
//        /// ACE AE data element: Entry Number.
//        /// Unique identifying number assigned to the Entry by the Filer
//        /// </summary>
//        [MessageBlockString(9, 10, "M", Justification.Right)]
//        public ZString EntryNumber;

//        /// <summary>
//        /// ACE AE data element: Broker Reference Number.
//        /// Filer/Preparer's internal Entry Summary identifier.
//        /// </summary>
//        [MessageBlockString(9, 19, "C")]
//        public ZString BrokerReferenceNumber;

//        /// <summary>
//        /// ACE AE data element: Importer of Record Number.
//        /// A code identifying the importer of record.
//        /// </summary>
//        [MessageBlockString(12, 28, "M")]
//        public ZString ImporterOfRecordNumber;

//        /// <summary>
//        /// A narrative text description indicating that the transaction has been accepted or rejected and corresponds with the Condition Code.
//        /// </summary>
//        [MessageBlockString(30, 40, "M")]
//        public ZString NarrativeText;

//        /// <summary>
//        /// A code identifying the condition or disposition regarding the request for cargo release.
//        /// </summary>
//        [MessageBlockString(3, 70, "M")]
//        public ZString MessageIdentifierCode;
//    }
//}
