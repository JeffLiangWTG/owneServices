// use C:\Dev\Enterprise\Product\Operations\Customs\US\Messaging\Business\MessageBuildingBlocks\AMS\APL\Common\APLACR.cs
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
//{
//    using CargoWise.Types;

//    [OutputBlock("ACR")]
//    public partial class STSACR : MessageBlock
//    {
//        public STSACR()
//            : base("ACR")
//        {
//        }

//        /// <summary>
//        /// A code representing the carrier, CBP-assigned port authority, or service bureau.
//        /// </summary>
//        [MessageBlockString(4, 4, "M")]
//        public ZString ACEM1UserCode;

//        /// <summary>
//        /// A code representing the type of application detail data contained within the block. Always equal to RC.
//        /// </summary>
//        [MessageBlockString(2, 14, "M")]
//        public ZString ApplicationIdentifier;

//        /// <summary>
//        /// A date in YYMMDD (year, month, day) format representing the date of processing.
//        /// </summary>
//        [MessageBlockDate(16, "M", "yyMMdd")]
//        public ZDate Date;

//        /// <summary>
//        /// A time in HHMMSS (hours, minutes, seconds) format representing the time of processing. Eastern Standard/Daylight Time should be reported.
//        /// </summary>
//        [MessageBlockString(6, 22, "M")]
//        public ZString Time;

//        /// <summary>
//        /// A CBP-generated 5-position numeric code from 00001 to 99999. The batch number is used in conjunction with the date of transmission to uniquely identify a user transmission.
//        /// </summary>
//        [MessageBlockInt(5, 28, "M")]
//        public ZInt BatchNumber;
//    }
//}
