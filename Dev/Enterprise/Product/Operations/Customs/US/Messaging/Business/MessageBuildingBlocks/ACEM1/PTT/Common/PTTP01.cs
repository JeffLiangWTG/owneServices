// C:\Dev\Enterprise\Product\Operations\Customs\US\Messaging\Business\MessageBuildingBlocks\ACEM1\INP\Common\ICMP01.cs
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
//{
//    using CargoWise.Types;

//    [InputBlock("P01")]
//    [OutputBlock("P01")]
//    public partial class PTTP01 : MessageBlock
//    {
//        public PTTP01()
//            : base("P01")
//        {
//        }

//        /// <summary>
//        /// A code representing the CBP port. See Census Schedule D in CAMIR Appendix E for valid port codes. This element will not be validated by CBP during electronic data exchange. Data will be returned as transmitted.
//        /// </summary>
//        [MessageBlockString(4, 4, "M")]
//        public ZString PortOfUnladingCode;

//        /// <summary>
//        /// A date in MMDDYY (month, day, year) format representing the original scheduled date of arrival (for imports) at this port. This element will not be validated by CBP during electronic data exchange. Data will be returned as transmitted.
//        /// </summary>
//        [MessageBlockDate(8, "M", "MMddyy")]
//        public ZDate OriginalEstimatedDate;
//    }
//}
