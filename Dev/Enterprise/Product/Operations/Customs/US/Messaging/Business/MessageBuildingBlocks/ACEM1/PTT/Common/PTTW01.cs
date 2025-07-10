// use C:\Dev\Enterprise\Product\Operations\Customs\US\Messaging\Business\MessageBuildingBlocks\AMS\TAR\Common\TARW01.cs
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
//{
//    using CargoWise.Types;

//    [InputBlock("W01")]
//    [OutputBlock("W01")]
//    public partial class PTTW01 : MessageBlock
//    {
//        public PTTW01()
//            : base("W01")
//        {
//        }

//        /// <summary>
//        /// A code representing the bill of lading sequence number on which the error occurred.
//        /// </summary>
//        [MessageBlockString(14, 4, "M")]
//        public ZString EntityNumber;

//        /// <summary>
//        /// A code representing the CBP port of arrival. Use Census Schedule D in CAMIR Appendix E for valid port codes.
//        /// </summary>
//        [MessageBlockString(4, 30, "M")]
//        public ZString CBPPort;

//        /// <summary>
//        /// A code representing the manifest sequence number. This number is an optional, carrier assigned sequence number. The default is one (000001).
//        /// </summary>
//        [MessageBlockString(6, 34, "M")]
//        public ZString ManifestSequenceNumber;

//        /// <summary>
//        /// A narrative explaining the error.
//        /// </summary>
//        [MessageBlockString(40, 40, "M")]
//        public ZString ErrorMessage;
//    }
//}
