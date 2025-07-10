// Use ADDT345 instead
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
//{
//    using CargoWise.Types;

//    [OutputBlock("T")]
//    public partial class AADDT3T4T5 : MessageBlock
//    {
//        public AADDT3T4T5()
//            : base("T")
//        {
//        }

//        /// <summary>
//        /// A code representing the record type. Valid Record Type Codes are:
//        /// 
//        /// 3 = Error condition
//        /// 4 = Record added
//        /// 5 = Data updated on database
//        /// </summary>
//        [MessageBlockString(1, 2, "M")]
//        public ZString RecordType;

//        /// <summary>
//        /// A code identifying the importer. This number is the same as the Importer Number (positions 4-15) in Record Identifier T1 if the Update Action Code was A.
//        /// </summary>
//        [MessageBlockString(12, 3, "M")]
//        public ZString ImporterNumber;

//        /// <summary>
//        /// A narrative message that the transaction received an error or was received error free and was added to the Importer/Consignee File, or a CBP importer number was assigned.
//        /// </summary>
//        [MessageBlockString(33, 15, "M")]
//        public ZString NarrativeMessage;

//        /// <summary>
//        /// The importer name as shown in the Importer/Consignee File.
//        /// </summary>
//        [MessageBlockString(32, 48, "M")]
//        public ZString ImporterName;
//    }
//}
