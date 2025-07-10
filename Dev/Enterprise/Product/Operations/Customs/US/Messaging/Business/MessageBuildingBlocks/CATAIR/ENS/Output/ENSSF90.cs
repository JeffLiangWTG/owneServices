// Use ISFSF90 instead

//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
//{
//    using CargoWise.Types;

//    [OutputBlock("SF90")]
//    public partial class ENSSF90 : MessageBlock
//    {
//        public ENSSF90()
//            : base("SF90")
//        {
//        }

//        /// <summary>
//        /// 01 = Message Rejected
//        /// 02 = Message Accepted
//        /// 03 = Message Accepted with Warning(s)
//        /// 11 = Record Rejected
//        /// 13 = Record Accepted with a Warning
//        /// </summary>
//        [MessageBlockString(2, 5, "M")]
//        public ZString MessageTypeCode;

//        /// <summary>
//        /// A code that identifies a record-level error.
//        /// </summary>
//        [MessageBlockString(3, 7, "C")]
//        public ZString ErrorCode;

//        /// <summary>
//        /// Narrative message text.
//        /// </summary>
//        [MessageBlockString(40, 10, "M")]
//        public ZString NarrativeMessageText;
//    }
//}
