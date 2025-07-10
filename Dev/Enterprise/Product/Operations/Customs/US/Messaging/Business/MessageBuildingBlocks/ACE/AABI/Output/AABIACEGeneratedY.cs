// Not Use
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
//{
//    using CargoWise.Types;

//    [OutputBlock(Constant.MandatoryCharacters)]
//    public partial class AABIACEGeneratedY : MessageBlock
//    {
//        public static class Constant
//        {
//            public const string MandatoryCharacters = "Y        ";
//        }

//        public AABIACEGeneratedY()
//            : base(Constant.MandatoryCharacters)
//        {
//        }

//        /// <summary>
//        /// Number of output images (i.e., records) returned in the block. The count does not include the B-Record or the Y-Record.
//        /// </summary>
//        [MessageBlockInt(5, 13, "M")]
//        public ZInt OutputTransactionImageCount;

//        /// <summary>
//        /// Always Y.
//        /// </summary>
//        [MessageBlockString(1, 80, "M")]
//        public ZString ACEGeneratedYRecordIndicator;
//    }
//}
