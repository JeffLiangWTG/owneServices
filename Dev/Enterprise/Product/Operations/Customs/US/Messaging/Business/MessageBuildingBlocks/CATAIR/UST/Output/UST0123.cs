// use USTO123 instead as there is a typo in the document

//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
//{
//    using CargoWise.Types;

//    [OutputBlock("O")]
//    public partial class UST0123 : MessageBlock
//    {
//        public UST0123()
//            : base("O")
//        {
//        }

//        /// <summary>
//        /// A code representing the record type. Record Type Codes are produced in the following order and are:
//        /// 
//        /// 1 = Current month
//        /// 2 = Previous month
//        /// 3 = Fiscal Year to date
//        /// </summary>
//        [MessageBlockInt(1, 2, "M")]
//        public ZInt RecordType;

//        /// <summary>
//        /// A code representing the district/port code where the entry summaries were filed. Valid district/port codes can be queries through the Extract Reference File chapter in this publication.
//        /// </summary>
//        [MessageBlockString(4, 3, "M")]
//        public ZString DistrictPortCode;

//        /// <summary>
//        /// The volume of formal entry summaries filed with CBP.
//        /// </summary>
//        [MessageBlockInt(7, 7, "M")]
//        public ZInt EntrySummaryVolumeFormal;

//        /// <summary>
//        /// The volume of informal entry summaries filed with CBP.
//        /// </summary>
//        [MessageBlockInt(7, 14, "M")]
//        public ZInt EntrySummaryVolumeInformal;

//        /// <summary>
//        /// The total formal and informal entry summaries filed with CBP.
//        /// </summary>
//        [MessageBlockInt(8, 21, "M")]
//        public ZInt TotalEntrySummaryVolume;

//        /// <summary>
//        /// The total formal and informal entry summaries filed via ABI.
//        /// </summary>
//        [MessageBlockInt(8, 29, "M")]
//        public ZInt TotalABIEntrySummaryVolume;

//        /// <summary>
//        /// The percentage of formal entry summaries filed that ABI is capable of accepting as compared to total informal entry summaries filed.
//        /// </summary>
//        [MessageBlockInt(3, 37, "M")]
//        public ZInt PercentFormalEntrySummaryVolumeEligibleForABI;

//        /// <summary>
//        /// The percentage of informal entry summaries filed that ABI is capable of accepting as compared to total informal entry summaries filed.
//        /// </summary>
//        [MessageBlockInt(3, 40, "M")]
//        public ZInt PercentInformalEntrySummaryVolumeEligibleForABI;

//        /// <summary>
//        /// The percentage of formal entry summaries filed via ABI as compared to the total that were eligible for ABI filing.
//        /// </summary>
//        [MessageBlockInt(3, 43, "M")]
//        public ZInt PercentFormalEntrySummariesSentABI;

//        /// <summary>
//        /// An asterisk (*) in this data field indicates that volumes filed through ABI fall below acceptable performance standards (less than 90% ABI filed). Otherwise, it is space filled.
//        /// </summary>
//        [MessageBlockString(1, 46, "C")]
//        public ZString PerformanceWarningFlag;

//        /// <summary>
//        /// The percentage of informal entry summaries filed via ABI compared to the total that were eligible for ABI filing (including adds, replaces and deletes).
//        /// </summary>
//        [MessageBlockInt(3, 47, "M")]
//        public ZInt PercentEligibleInformalEntrySummariesSentABI;

//        /// <summary>
//        /// An asterisk (*) in this data field indicates that volumes filed through ABI fall below acceptable performance standards (less than 90% ABI filed). Otherwise, it is space filled.
//        /// </summary>
//        [MessageBlockString(1, 50, "C")]
//        public ZString PerformanceWarningFlag1;

//        /// <summary>
//        /// Total entry summary transactions sent through ABI (including adds, replaces, and deletes).
//        /// </summary>
//        [MessageBlockInt(8, 51, "M")]
//        public ZInt TotalABIEntrySummaryTransactions;

//        /// <summary>
//        /// Percentage of system rejects as compared to the total number of ABI entry summary transactions (including adds, replaces, and deletes). Warning messages are not included.
//        /// </summary>
//        [MessageBlockInt(3, 59, "M")]
//        public ZInt PercentABIEntrySummaryTransactionSystemRejects;

//        /// <summary>
//        /// An asterisk (*) in this data field indicates that the total system rejects exceeds the acceptable level (greater than a 10% reject rate). Otherwise, it is space filled.
//        /// </summary>
//        [MessageBlockString(1, 62, "C")]
//        public ZString PerformanceWarningFlag2;

//        /// <summary>
//        /// If positions 46, 50 and/or 62 contain an asterisk (*), the message PERFORMANCE ALERT is system generated. Contact your client representative if this message is received
//        /// </summary>
//        [MessageBlockString(17, 63, "C")]
//        public ZString PerformanceWarningMessage;
//    }
//}
