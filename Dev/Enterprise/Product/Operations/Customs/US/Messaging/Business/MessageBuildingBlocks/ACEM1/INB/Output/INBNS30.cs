// use C:\Dev\Enterprise\Product\Operations\Customs\US\Messaging\Business\MessageBuildingBlocks\CATAIR\INB\Output\INBNS30.cs
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
//{
//    using CargoWise.Types;

//    [OutputBlock("30")]
//    public partial class INBNS30 : MessageBlock
//    {
//        public INBNS30()
//            : base("30")
//        {
//        }

//        /// <summary>
//        /// A code advising the recipient of the posting action taken on a bill of lading. Refer to the CAMIR Appendix D for valid Disposition Codes.
//        /// </summary>
//        [MessageBlockString(2, 3, "M")]
//        public ZString DispositionCode;

//        /// <summary>
//        /// A code representing the Standard Carrier Alpha Code (SCAC) of the party who actually issued the bill of lading. Do not confuse the issuer of the bill with the operator of the vessel. In an in-bond movement from a withdrawal from an FTZ or bonded warehouse, the FIRMS code of the FTZ or bonded warehouse may be returned in lieu of the SCAC of the carrier, if the carrier has no SCAC or if it is unknown.
//        /// </summary>
//        [MessageBlockString(4, 5, "M")]
//        public ZString IssuerCodeOfMasterBillNumber;

//        /// <summary>
//        /// The master bill number as listed on the manifest. If the number is less than 12 positions, it is left justified. Will not include embedded spaces, hyphens, slashes or special characters.
//        /// </summary>
//        [MessageBlockString(12, 9, "M")]
//        public ZString MasterBillNumber;

//        /// <summary>
//        /// This field is reserved for future use. Space fill.
//        /// </summary>
//        [MessageBlockString(4, 21, "C")]
//        public ZString IssuerCodeOfHouseBillNumber;

//        /// <summary>
//        /// This field is reserved for future use. Space fill.
//        /// </summary>
//        [MessageBlockString(12, 25, "C")]
//        public ZString HouseBillNumber;

//        /// <summary>
//        /// This field is reserved for future use. Space fill.
//        /// </summary>
//        [MessageBlockString(4, 37, "C")]
//        public ZString IssuerCodeOfSubhouseBillNumber;

//        /// <summary>
//        /// This field is reserved for future use. Space fill.
//        /// </summary>
//        [MessageBlockString(12, 41, "C")]
//        public ZString SubhouseBillNumber;

//        /// <summary>
//        /// A value representing the total number of pieces on the bill of lading affected by the action indicated by the disposition code.
//        /// </summary>
//        [MessageBlockDecimal(10, 53, "M", 0)]
//        public ZDecimal Quantity;

//        /// <summary>
//        /// A code of N when a negative number is associated with a disposition code of 1A, 1B or 1C; otherwise, space fill.
//        /// </summary>
//        [MessageBlockString(1, 63, "C")]
//        public ZString NegativeIndicator;

//        /// <summary>
//        /// A date in YYMMDD (year, month, day) format representing the date on which the action was authorized by USCBP or another federal agency.
//        /// </summary>
//        [MessageBlockDate(64, "M", "yyMMdd")]
//        public ZDate ActionDate;

//        /// <summary>
//        /// A time in HHMM (hour, minute) 24-hour clock format representing the time that the release (or other posting action) was authorized. Eastern Standard/Daylight time will be returned.
//        /// </summary>
//        [MessageBlockString(4, 70, "M")]
//        public ZString ActionTime;

//        /// <summary>
//        /// A code representing the Standard Carrier Alpha Code (SCAC) of the in-bond carrier. In an in-bond movement from a withdrawal from an FTZ or bonded warehouse, the FIRMS code of the FTZ or bonded warehouse may be used in lieu of the SCAC of the carrier, if the carrier has no SCAC or if it is unknown.
//        /// </summary>
//        [MessageBlockString(4, 74, "M")]
//        public ZString InbondCarrierCode;
//    }
//}
