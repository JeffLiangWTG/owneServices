//use C:\Dev\Enterprise\Product\Operations\Customs\US\Messaging\Business\MessageBuildingBlocks\ACEM1\INP\Common\INPB04.cs
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
//{
//    using CargoWise.Types;

//    [InputBlock("B04")]
//    [OutputBlock("B04")]
//    public partial class SIAB04 : MessageBlock
//    {
//        public SIAB04()
//            : base("B04")
//        {
//        }

//        /// <summary>
//        /// This data element identifies the type of reference identifier being reported for a bill of lading at the BOL grouping level. Left justify code qualifier values that are less than 3 characters in length.
//        /// </summary>
//        [MessageBlockString(3, 4, "M")]
//        public ZString ReferenceIdentifierQualifier;

//        /// <summary>
//        /// The unique reference number.
//        /// </summary>
//        [MessageBlockString(30, 7, "M")]
//        public ZString ReferenceIdentifier;
//    }
//}
