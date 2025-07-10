// Use C:\Dev\Enterprise\Product\Operations\Customs\US\Messaging\Business\MessageBuildingBlocks\CATAIR\CRL\Input\CRLH2.cs
//namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
//{
//    using CargoWise.Types;

//    [OutputBlock("H2")]
//    public partial class ACRLH2 : MessageBlock
//    {
//        public ACRLH2()
//            : base("H2")
//        {
//        }

//        /// <summary>
//        /// ACE AE data element: Location of Goods Code.
//        /// The Facilities Information and Resources Management System (FIRMS) code that identifies the known location of the merchandise at the time of filing.
//        /// </summary>
//        [MessageBlockString(4, 3, "M")]
//        public ZString LocationOfGoods;

//        /// <summary>
//        /// ACE AE data element: Ultimate Consignee Number.
//        /// A code identifying the ultimate consignee.
//        /// </summary>
//        [MessageBlockString(12, 11, "C")]
//        public ZString UltimateConsigneeNumber;

//        /// <summary>
//        /// Always equals 'A'.
//        /// </summary>
//        [MessageBlockString(1, 23, "M")]
//        public ZString EntryDateElectionCode;

//        /// <summary>
//        /// ACE AE data element: Trip Identifier.
//        /// The voyage/flight/trip number of the importing carrier.
//        /// </summary>
//        [MessageBlockString(5, 24, "C")]
//        public ZString VoyageFlightTripManifestNumber;

//        /// <summary>
//        /// ACE AE data element: a summation of the Value of Goods Amount for all lines of the entry summary.
//        /// The total entered value of the entry in whole dollars (sum of the Value of Goods Amount).
//        /// </summary>
//        [MessageBlockDecimal(10, 29, "M", 0)]
//        public ZDecimal TotalEntryValue;

//        /// <summary>
//        /// ACE AE data element: Broker Reference Number.
//        /// Filer/Preparer's internal Entry Summary identifier.
//        /// </summary>
//        [MessageBlockString(9, 39, "C")]
//        public ZString BrokerReferenceNumber;

//        /// <summary>
//        /// ACE AE data element: Conveyance Name.
//        /// For vessel shipments: the name of the vessel. May not appear for a non-vessel shipment; a value is allowed, however (e.g., the name of the carrier).
//        /// </summary>
//        [MessageBlockString(20, 60, "C")]
//        public ZString VesselNameForeignTradeZoneNumber;
//    }
//}
