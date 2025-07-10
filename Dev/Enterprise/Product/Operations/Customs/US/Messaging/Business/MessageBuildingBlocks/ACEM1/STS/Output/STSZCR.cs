// use C:\Dev\Enterprise\Product\Operations\Customs\US\Messaging\Business\MessageBuildingBlocks\AMS\APL\Common\APLZCR.cs
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
//{
//    using CargoWise.Types;

//    [OutputBlock("ZCR")]
//    public partial class STSZCR : MessageBlock
//    {
//        public STSZCR()
//            : base("ZCR")
//        {
//        }

//        /// <summary>
//        /// A code representing the carrier, CBP-assigned port authority, or service bureau.
//        /// </summary>
//        [MessageBlockString(4, 4, "M")]
//        public ZString ACEM1UserCode;

//        /// <summary>
//        /// Output application identifier. Always equal to RC.
//        /// </summary>
//        [MessageBlockString(2, 14, "M")]
//        public ZString ApplicationIdentifier;

//        /// <summary>
//        /// The total number of records transmitted (excludes the ACR and ZCR).
//        /// </summary>
//        [MessageBlockInt(5, 35, "M")]
//        public ZInt NumberOfTransactionDetailRecords;
//    }
//}
