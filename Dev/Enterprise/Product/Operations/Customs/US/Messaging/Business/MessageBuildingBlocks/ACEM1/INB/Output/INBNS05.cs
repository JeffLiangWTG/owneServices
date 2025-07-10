//use C:\Dev\Enterprise\Product\Operations\Customs\US\Messaging\Business\MessageBuildingBlocks\CATAIR\INB\Output\INBNS05.cs
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
//{
//    using CargoWise.Types;

//    [OutputBlock("05")]
//    public partial class INBNS05 : MessageBlock
//    {
//        public INBNS05()
//            : base("05")
//        {
//        }

//        /// <summary>
//        /// The name which identifies the importing conveyance.
//        /// </summary>
//        [MessageBlockString(23, 3, "M")]
//        public ZString ConveyanceName;

//        /// <summary>
//        /// The voyage/trip number.
//        /// </summary>
//        [MessageBlockInt(5, 26, "M")]
//        public ZInt VoyageTripNumber;

//        /// <summary>
//        /// A code representing the USCBP district/port of arrival. See Census Schedule D in CAMIR Appendix E for valid district/port codes.
//        /// </summary>
//        [MessageBlockString(4, 31, "M")]
//        public ZString USCBPDistrictPort;

//        /// <summary>
//        /// A date in YYMMDD (year, month, day) format representing the original scheduled date of arrival.
//        /// </summary>
//        [MessageBlockDate(35, "M", "yyMMdd")]
//        public ZDate EstimatedDateOfArrival;

//        /// <summary>
//        /// A time in HHMMSS (hour, minute, second) 24-hour clock format representing the estimated time of conveyance arrival. Eastern Standard/Daylight time will be returned.
//        /// </summary>
//        [MessageBlockString(6, 41, "C")]
//        public ZString EstimatedTimeOfArrival;
//    }
//}
