// use C:\Dev\Enterprise\Product\Operations\Customs\US\Messaging\Business\MessageBuildingBlocks\CATAIR\INB\Output\INBNS60.cs
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
//{
//    using CargoWise.Types;

//    [OutputBlock("60")]
//    public partial class INBNS60 : MessageBlock
//    {
//        public INBNS60()
//            : base("60")
//        {
//        }

//        /// <summary>
//        /// A code of 1 (one) indicates that the disposition action indicated in the NS30 record is a container level action taken specifically against the container. A blank indicates that the disposition action was not a container level action taken against the container.
//        /// </summary>
//        [MessageBlockInt(1, 3, "C")]
//        public ZInt ActionIndicator;

//        /// <summary>
//        /// A container/equipment number associated with the bill of lading.
//        /// </summary>
//        [MessageBlockString(14, 4, "C")]
//        public ZString ContainerNumber;

//        /// <summary>
//        /// A carrier seal number associated with the container.
//        /// </summary>
//        [MessageBlockString(15, 18, "C")]
//        public ZString SealNumber1;

//        /// <summary>
//        /// A carrier seal number associated with the container.
//        /// </summary>
//        [MessageBlockString(15, 33, "C")]
//        public ZString SealNumber2;
//    }
//}
