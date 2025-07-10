// use C:\Dev\Enterprise\Product\Operations\Customs\US\Messaging\Business\MessageBuildingBlocks\ACEM1\INP\Common\INPN00.cs
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
//{
//    using CargoWise.Types;

//    [InputBlock("N00")]
//    [OutputBlock("N00")]
//    public partial class SIAN00 : MessageBlock
//    {
//        public SIAN00()
//            : base("N00")
//        {
//        }

//        /// <summary>
//        /// A code identifying the type of entity. Valid codes are:
//        /// 
//        /// SNP = Secondary Notify Party
//        /// 
//        /// If this data element is used, the Name, Code Qualifier and ID Code must be provided.
//        /// </summary>
//        [MessageBlockString(3, 4, "M")]
//        public ZString EntityCode;

//        /// <summary>
//        /// A free form text for name of entity. Required if Entity Code is submitted.
//        /// </summary>
//        [MessageBlockString(35, 7, "C")]
//        public ZString EntityName;

//        /// <summary>
//        /// If positions 4-6, Entity Code = SNP, then Code '2 ' or Code '17' is required to be used in positions 42-43. (NOTE: The leading numeric 2 must be followed by 1 column of space fill). 
//        /// 
//        /// 2 	=	SCAC or FIRMS code
//        /// 17	=	ABI Routing code
//        /// </summary>
//        [MessageBlockString(2, 42, "C")]
//        public ZString CodeQualifier;

//        /// <summary>
//        /// A code related to preceding Code Qualifier data element. This code is required if the Code Qualifier = '2 ' or '17'. The ID Code will be ABI Routing code if the qualifier is equal to '17' or the SCAC or FIRMS code if the qualifier is equal to '2 '.
//        /// </summary>
//        [MessageBlockString(17, 44, "C")]
//        public ZString IDCode;

//        /// <summary>
//        /// For future use. Leave this data element blank.
//        /// </summary>
//        [MessageBlockString(2, 61, "C")]
//        public ZString EntityRelationshipCode;

//        /// <summary>
//        /// For future use. Leave this data element blank.
//        /// </summary>
//        [MessageBlockString(2, 63, "C")]
//        public ZString EntityIDCode;
//    }
//}
