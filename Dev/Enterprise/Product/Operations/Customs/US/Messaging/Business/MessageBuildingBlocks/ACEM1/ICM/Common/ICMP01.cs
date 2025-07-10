// C:\Dev\Enterprise\Product\Operations\Customs\US\Messaging\Business\MessageBuildingBlocks\ACEM1\INP\Common\ICMP01.cs
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
//{
//    using CargoWise.Types;

//    [InputBlock("P01")]
//    [OutputBlock("P01")]
//    public partial class ICMP01 : MessageBlock
//    {
//        public ICMP01()
//            : base("P01")
//        {
//        }

//        /// <summary>
//        /// A code representing the CBP port. See Census Schedule D in CAMIR Appendix E for valid port codes.
//        /// </summary>
//        [MessageBlockString(4, 4, "C")]
//        public ZString PortOfUnladingCode;

//        /// <summary>
//        /// A date in MMDDYY (month, day, year) format representing the original scheduled date of arrival (for imports) or departure (for exports) at this port.
//        /// </summary>
//        [MessageBlockDate(8, "C", "MMddyy")]
//        public ZDate OriginalEstimatedDate;
//    }
//}
