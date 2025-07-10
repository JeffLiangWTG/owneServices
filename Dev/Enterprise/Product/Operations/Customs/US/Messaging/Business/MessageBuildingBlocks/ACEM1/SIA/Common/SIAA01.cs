//use C:\Dev\Enterprise\Product\Operations\Customs\US\Messaging\Business\MessageBuildingBlocks\ACEM1\INP\Common\INPA01.cs
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
//{
//    using CargoWise.Types;

//    [InputBlock("A01")]
//    [OutputBlock("A01")]
//    public partial class SIAA01 : MessageBlock
//    {
//        public SIAA01()
//            : base("A01")
//        {
//        }

//        /// <summary>
//        /// A SCAC representing the importing carrier.
//        /// </summary>
//        [MessageBlockString(4, 4, "M")]
//        public ZString CarrierCode;

//        /// <summary>
//        /// A code representing the CBP port of arrival. Use Census Schedule D in CAMIR Appendix E for valid port codes.
//        /// </summary>
//        [MessageBlockString(4, 8, "M")]
//        public ZString CBPPort;

//        /// <summary>
//        /// A code representing the CBP type of manifest amendment. Valid codes are:
//        /// 
//        /// B = Add Supplemental/Subsequent In-bond Movement 
//        /// E = Delete Supplemental/Subsequent In-bond Movement.
//        /// </summary>
//        [MessageBlockString(1, 12, "M")]
//        public ZString ActionCode;

//        /// <summary>
//        /// The bill of lading sequence number. Do not include the issuer code as it is contained in the associated J01 record.
//        /// </summary>
//        [MessageBlockString(12, 13, "M")]
//        public ZString BillOfLadingSequenceNumber;

//        /// <summary>
//        /// A code representing the reason(s) for the amendment of the manifest record. Refer to CAMIR Appendix C for valid amendment codes.
//        /// </summary>
//        [MessageBlockInt(2, 35, "M")]
//        public ZInt AmendmentCode;
//    }
//}
