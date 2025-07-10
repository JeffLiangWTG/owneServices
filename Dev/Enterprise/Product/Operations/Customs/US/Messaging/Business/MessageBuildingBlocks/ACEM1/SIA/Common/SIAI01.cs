//use C:\Dev\Enterprise\Product\Operations\Customs\US\Messaging\Business\MessageBuildingBlocks\ACEM1\INP\Common\INPI01.cs
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
//{
//    using CargoWise.Types;

//    [InputBlock("I01")]
//    [OutputBlock("I01")]
//    public partial class SIAI01 : MessageBlock
//    {
//        public SIAI01()
//            : base("I01")
//        {
//        }

//        /// <summary>
//        /// The ACS code for the various types of in-bond movements. Valid codes are:
//        /// 
//        /// 61 = Immediate Transportation (IT)
//        /// 62 = Transportation and Exportation (T&E)
//        /// 63 = Immediate Exportation (IE)
//        /// </summary>
//        [MessageBlockString(2, 4, "M")]
//        public ZString InbondEntryType;

//        /// <summary>
//        /// Must be either “Y” of “N” to indicate whether or not any of the cargo on this in-bond is subject to the Bioterrorism Act of 2002 reporting requirements. This field is required only for In-bond Type T&E (62) on Supplemental In-bond movements. Not required on Subsequent In-bond movements.
//        /// </summary>
//        [MessageBlockString(1, 6, "C")]
//        public ZString BTAFDAIndicator;

//        /// <summary>
//        /// A CBP assigned in-bond control number used with conventional MIB movements. Either this data element or the Paperless In-bond Number data element (positions 50-60) must be completed.
//        /// </summary>
//        [MessageBlockString(9, 8, "C")]
//        public ZString ConventionalInbondNumber;

//        /// <summary>
//        /// A SCAC representing the original in-bond carrier, if different from the importing/exporting carrier.
//        /// </summary>
//        [MessageBlockString(4, 17, "C")]
//        public ZString InbondCarrierCode;

//        /// <summary>
//        /// A code representing the CBP port of termination for an IT (61) entry, or the port of exportation for a T&E (62) entry, or the port of arrival for an IE (63) entry. This code is the port of destination for the Supplemental/ Subsequent In-bond movement. See CAMIR Appendix E for valid port codes.
//        /// </summary>
//        [MessageBlockString(4, 21, "M")]
//        public ZString USPortOfDestination;

//        /// <summary>
//        /// A code representing the foreign port of destination for T&E (62) or IE (63) entries. This data field is left blank for IT (61) entries. See Census Schedule K in CAMIR Appendix F for valid foreign port codes.
//        /// </summary>
//        [MessageBlockString(5, 25, "C")]
//        public ZString ForeignDestination;

//        /// <summary>
//        /// A value in whole dollars of the shipment moving in-bond. Twenty dollars per kilo may be used if the value is unknown. This data element must be greater than zero. No decimals.
//        /// </summary>
//        [MessageBlockDecimal(8, 30, "M", 2)]
//        public ZDecimal Value;

//        /// <summary>
//        /// The identification (ID) number of the bonded carrier. This must include any embedded hyphens. Valid formats for importer numbers are:
//        /// 
//        /// NN-NNNNNNNXX = Internal Revenue Service Number
//        /// YYDDPP-NNNNN = CBP Assigned Number
//        /// NNN-NN-NNNN = Social Security Number
//        /// 
//        /// The system will validate the ID is on file and has a valid Bond type.
//        /// </summary>
//        [MessageBlockString(12, 38, "M")]
//        public ZString BondedCarrierID;

//        /// <summary>
//        /// The carrier assigned V in-bond number used with paperless MIB movements. Either the Conventional In-bond Number data element or the Paperless In-bond Number data element must be completed.
//        /// </summary>
//        [MessageBlockString(11, 50, "C")]
//        public ZString PaperlessInbondNumber;
//    }
//}
