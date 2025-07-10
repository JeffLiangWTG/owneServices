// C:\Dev\Enterprise\Product\Operations\Customs\US\Messaging\Business\MessageBuildingBlocks\ACEM1\INP\Common\INPM01.cs
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
//{
//    using CargoWise.Types;

//    [InputBlock("M01")]
//    [OutputBlock("M01")]
//    public partial class ICMM01 : MessageBlock
//    {
//        public ICMM01()
//            : base("M01")
//        {
//        }

//        /// <summary>
//        /// A Standard Carrier Alpha Code (SCAC) representing the automated carrier/NVOCC reporting the In-bond and Vessel Departure/Arrival Message.
//        /// </summary>
//        [MessageBlockString(4, 4, "M")]
//        public ZString CarrierCode;

//        /// <summary>
//        /// A code indicating the Mode of Transport of the Carrier in the Carrier Code data element. Valid codes are:
//        /// 
//        /// 10 = Vessel, non-container, or unable to determine if container (Including Lightered, Land Bridge and LASH)
//        /// 11 = Vessel Containerized (Container)
//        /// </summary>
//        [MessageBlockString(2, 8, "M")]
//        public ZString ModeOfTransportationCode;

//        /// <summary>
//        /// An International Organization for Standardization (ISO) country code representing the flag country of the vessel. Refer to CAMIR Appendix G for valid codes.
//        /// </summary>
//        [MessageBlockString(2, 10, "M")]
//        public ZString VesselCountryCode;

//        /// <summary>
//        /// A valid vessel name entered using no slashes. The vessel name will always be populated in the response transmission from CBP when either the Vessel Name or Vessel Code is provided in the input transaction.
//        /// </summary>
//        [MessageBlockString(23, 12, "C")]
//        public ZString VesselName;

//        /// <summary>
//        /// The voyage number entered using no slashes.
//        /// </summary>
//        [MessageBlockString(5, 35, "M")]
//        public ZString VoyageNumber;

//        /// <summary>
//        /// The manifest sequence number. This number is a carrier-assigned sequence number. The system-generated default is one (000001). It may be a date. Once transmitted, it cannot be changed. All subsequent transmissions for the manifest must use the original manifest sequence number.
//        /// </summary>
//        [MessageBlockString(6, 45, "C")]
//        public ZString ManifestSequenceNumber;

//        /// <summary>
//        /// International Maritime Organization (IMO) Code issued by Lloyds representing the importing vessel.
//        /// </summary>
//        [MessageBlockString(7, 52, "C")]
//        public ZString VesselCode;
//    }
//}
