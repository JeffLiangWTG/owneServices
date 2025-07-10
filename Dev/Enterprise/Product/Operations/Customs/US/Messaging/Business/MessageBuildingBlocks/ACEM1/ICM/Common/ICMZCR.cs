// use C:\Dev\Enterprise\Product\Operations\Customs\US\Messaging\Business\MessageBuildingBlocks\AMS\APL\Common\APLZCR.cs
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
//{
//    using CargoWise.Types;

//    [InputBlock("ZCR")]
//    [OutputBlock("ZCR")]
//    public partial class ICMZCR : MessageBlock
//    {
//        public ICMZCR()
//            : base("ZCR")
//        {
//        }

//        /// <summary>
//        /// A code representing the carrier, CBP-assigned port authority, or service bureau. Not validated upon input. AMS User Code from the ACR record will be returned in response.
//        /// </summary>
//        [MessageBlockString(4, 4, "M")]
//        public ZString AMSUserCode;

//        /// <summary>
//        /// Output application identifier. This data element is not included in input transactions (HI) but is returned to the AMS participant in output transaction trailer records. Application Identifier is HR for response to HI.
//        /// </summary>
//        [MessageBlockString(2, 14, "C")]
//        public ZString ApplicationIdentifier;

//        /// <summary>
//        /// The total number of records transmitted (excludes the ACR and ZCR). Not validated upon input. Valid number will be returned in response.
//        /// </summary>
//        [MessageBlockString(5, 35, "C")]
//        public ZString NumberOfTransactionDetailRecords;
//    }
//}
