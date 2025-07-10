// use C:\Dev\Enterprise\Product\Operations\Customs\US\Messaging\Business\MessageBuildingBlocks\AMS\TAR\Common\TARW02.cs
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
//{
//    using CargoWise.Types;

//    [OutputBlock("W02")]
//    public partial class INPW02Only : MessageBlock
//    {
//        public INPW02Only()
//            : base("W02")
//        {
//        }

//        /// <summary>
//        /// A SCAC representing the automated importing carrier/MVOCC/NVOCC.
//        /// </summary>
//        [MessageBlockString(4, 4, "M")]
//        public ZString CarrierCode;

//        /// <summary>
//        /// A date in YYMMDD (year, month, day) format representing the date that the inbound transmission to CBP was processed. In most cases the date of processing and the date of transmission will be the same.
//        /// </summary>
//        [MessageBlockDate(8, "M", "yyMMdd")]
//        public ZDate DateOfTransmission;

//        /// <summary>
//        /// A time in HHMMSS (hour, minute, second) 24-hour clock format representing the time that the inbound transmission to CBP was processed. Eastern Standard/Daylight Time should be reported.
//        /// </summary>
//        [MessageBlockString(6, 14, "M")]
//        public ZString TimeOfTransmission;

//        /// <summary>
//        /// The total number of manifests (M01 records) read on a given transmission.
//        /// </summary>
//        [MessageBlockInt(2, 20, "M")]
//        public ZInt TotalManifestsRead;

//        /// <summary>
//        /// The total number of ports (P01 records) read on a single transmission.
//        /// </summary>
//        [MessageBlockInt(3, 22, "M")]
//        public ZInt TotalPortsRead;

//        /// <summary>
//        /// The total number of bills (B01 records) read on a single transmission.
//        /// </summary>
//        [MessageBlockInt(5, 25, "M")]
//        public ZInt TotalBillsRead;

//        /// <summary>
//        /// Zero filled. This field is not used by CBP.
//        /// </summary>
//        [MessageBlockInt(5, 30, "M")]
//        public ZInt TotalHouseBillsRead;

//        /// <summary>
//        /// The total number of amendments (A01 records) read on a single transmission.
//        /// Zero filled for response to Original Manifest submission (MR).
//        /// </summary>
//        [MessageBlockInt(5, 35, "M")]
//        public ZInt TotalAmendmentsRead;

//        /// <summary>
//        /// Zero filled for response to Original Manifest submission (MR) and Manifest Amendment response (AR).
//        /// </summary>
//        [MessageBlockInt(5, 40, "M")]
//        public ZInt TotalH01RecordsInput;

//        /// <summary>
//        /// The total number of Bills of Lading (B01 record groupings) that were rejected for a single transmission.
//        /// </summary>
//        [MessageBlockInt(5, 45, "M")]
//        public ZInt TotalBillsRejected;

//        /// <summary>
//        /// The total number of Bills of Lading (B01 record groupings) that were accepted into the database.
//        /// </summary>
//        [MessageBlockInt(5, 50, "M")]
//        public ZInt TotalBillsAccepted;

//        /// <summary>
//        /// The total number of records read on a single inbound transmission (excludes the ACR and ZCR).
//        /// </summary>
//        [MessageBlockInt(5, 55, "M")]
//        public ZInt TotalRecordsRead;
//    }
//}
