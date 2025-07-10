//use C:\Dev\Enterprise\Product\Operations\Customs\US\Messaging\Business\MessageBuildingBlocks\CATAIR\INB\Output\INBNS10.cs
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
//{
//    using CargoWise.Types;

//    [OutputBlock("10")]
//    public partial class INBNS10 : MessageBlock
//    {
//        public INBNS10()
//            : base("10")
//        {
//        }

//        /// <summary>
//        /// The code representing the type of in-bond movement. Valid codes are:
//        /// 
//        /// 61 = Immediate Transportation (IT)
//        /// 62 = Transportation and Exportation (T&E)
//        /// 63 = Immediate Exportation (IE)
//        /// </summary>
//        [MessageBlockString(2, 3, "M")]
//        public ZString InBondEntryType;

//        /// <summary>
//        /// The in-bond entry number. The only format currently used is the conventional 9 numeric in-bond number as listed on the CBPF-7512. This number is left justified and contains no embedded spaces, hyphens, slashes, or special characters.
//        /// </summary>
//        [MessageBlockString(12, 5, "M")]
//        public ZString InbondNumber;

//        /// <summary>
//        /// The Census Schedule D code representing the USCBP port of termination for an IT '61' entry, or the port of exportation for a T&E '62' entry, or the port of arrival for an IE '63' entry. Refer to the CAMIR Appendix E for valid port codes.
//        /// </summary>
//        [MessageBlockString(4, 17, "M")]
//        public ZString USPortOfDestination;

//        /// <summary>
//        /// The Census Schedule K code representing the foreign port of destination for T&E '62' or IE '63' entries. Space fill for IT '61' entries. Refer to the CAMIR Appendix F for valid foreign port codes.
//        /// </summary>
//        [MessageBlockString(5, 21, "C")]
//        public ZString ForeignDestination;
//    }
//}
